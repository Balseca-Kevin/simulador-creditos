using Credit.Api.Models;
using Credit.Api.Services;

namespace Credit.Tests;

/// <summary>
/// Seguro de desgravamen (prima sobre el saldo deudor) e ingreso mínimo
/// requerido (cuota más alta sobre la capacidad de pago del 40 %).
/// </summary>
public class SeguroEIngresoTests
{
    private readonly MotorAmortizacion _motor = new();

    private const decimal PrimaMensual = 0.05m; // 0.05 % del saldo cada mes

    [Fact]
    public void El_seguro_se_calcula_sobre_el_saldo_al_inicio_de_cada_periodo()
    {
        // 12 000 al 12 % a 12 meses, alemán, prima 0.05 % mensual.
        // Período 1: 12000 · 0.0005 = 6.00 ; período 12: 1000 · 0.0005 = 0.50
        // Total = 0.0005 · (12000 + 11000 + … + 1000) = 0.0005 · 78000 = 39.00
        var tabla = _motor.CalcularAleman(new ParametrosCredito(12_000m, 12m, 12, TasaSeguroMensual: PrimaMensual));

        Assert.Equal(6.00m, tabla.Cuotas[0].Seguro);
        Assert.Equal(0.50m, tabla.Cuotas[^1].Seguro);
        Assert.Equal(39.00m, tabla.TotalSeguro);
    }

    [Fact]
    public void Sin_seguro_la_prima_es_cero_en_todas_las_filas()
    {
        var tabla = _motor.CalcularFrances(new ParametrosCredito(10_000m, 15.50m, 24));

        Assert.All(tabla.Cuotas, fila => Assert.Equal(0m, fila.Seguro));
        Assert.Equal(0m, tabla.TotalSeguro);
    }

    [Fact]
    public void Con_seguro_el_frances_mantiene_fija_la_cuota_y_la_cuota_total_decrece()
    {
        // La prima baja con el saldo: la cuota de capital + interés sigue fija,
        // pero lo que efectivamente paga el cliente disminuye.
        var tabla = _motor.CalcularFrances(new ParametrosCredito(50_000m, 15.50m, 36, TasaSeguroMensual: PrimaMensual));

        Assert.Single(tabla.Cuotas.Select(c => c.Cuota).Distinct());

        for (var k = 1; k < tabla.Cuotas.Count; k++)
        {
            Assert.True(tabla.Cuotas[k].CuotaTotal < tabla.Cuotas[k - 1].CuotaTotal,
                $"La cuota total no decreció en el período {k + 1}.");
        }
    }

    [Fact]
    public void El_total_pagado_incluye_capital_interes_y_seguro()
    {
        var tabla = _motor.CalcularAleman(new ParametrosCredito(25_000m, 22m, 48, FrecuenciaPago.Trimestral, PrimaMensual));

        Assert.Equal(tabla.TotalCapital + tabla.TotalInteres + tabla.TotalSeguro, tabla.TotalPagado);
        Assert.Equal(tabla.Cuotas.Sum(f => f.CuotaTotal), tabla.TotalPagado);
        Assert.All(tabla.Cuotas, fila => Assert.Equal(fila.Cuota + fila.Seguro, fila.CuotaTotal));
    }

    [Fact]
    public void El_seguro_trimestral_cubre_tres_meses_de_prima()
    {
        // 12 000 trimestral: la primera prima cubre 3 meses sobre 12 000.
        // 12000 · 0.0005 · 3 = 18.00
        var tabla = _motor.CalcularAleman(new ParametrosCredito(12_000m, 12m, 12, FrecuenciaPago.Trimestral, PrimaMensual));

        Assert.Equal(18.00m, tabla.Cuotas[0].Seguro);
    }

    [Fact]
    public void El_ingreso_minimo_es_la_cuota_mas_alta_sobre_el_cuarenta_por_ciento()
    {
        // Alemán 12 000 al 12 % a 12 meses: la cuota más alta es la primera, 1 120.
        // 1120 / 0.40 = 2 800
        var tabla = _motor.CalcularAleman(12_000m, 12m, 12);

        Assert.Equal(1_120m, tabla.CuotaTotalMaxima);
        Assert.Equal(2_800m, tabla.IngresoMinimoRequerido);
    }

    [Fact]
    public void El_ingreso_minimo_se_redondea_hacia_arriba_al_centavo()
    {
        // Francés 10 000 al 12 % a 12 meses: cuota 888.49.
        // 888.49 / 0.40 = 2 221.225 -> 2 221.23. Redondear hacia abajo daría
        // un ingreso con el que la cuota superaría el 40 %.
        var tabla = _motor.CalcularFrances(10_000m, 12m, 12);

        Assert.Equal(2_221.23m, tabla.IngresoMinimoRequerido);
    }

    [Fact]
    public void El_ingreso_minimo_considera_el_seguro()
    {
        // Misma tabla alemana con prima: cuota más alta = 1120 + 6 = 1126 -> 1126 / 0.40 = 2815
        var tabla = _motor.CalcularAleman(new ParametrosCredito(12_000m, 12m, 12, TasaSeguroMensual: PrimaMensual));

        Assert.Equal(1_126m, tabla.CuotaTotalMaxima);
        Assert.Equal(2_815m, tabla.IngresoMinimoRequerido);
    }

    [Fact]
    public void El_ingreso_minimo_lleva_la_cuota_a_su_equivalente_mensual()
    {
        // Trimestral: cuota más alta 3 360, que equivale a 1 120 por mes.
        // 3360 / 3 / 0.40 = 2 800, igual que el caso mensual equivalente.
        var tabla = _motor.CalcularAleman(new ParametrosCredito(12_000m, 12m, 12, FrecuenciaPago.Trimestral));

        Assert.Equal(3_360m, tabla.CuotaTotalMaxima);
        Assert.Equal(2_800m, tabla.IngresoMinimoRequerido);
    }
}
