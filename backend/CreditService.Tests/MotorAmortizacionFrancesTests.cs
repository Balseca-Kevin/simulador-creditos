using CreditService.Aplicacion.Servicios;

namespace CreditService.Tests;

/// <summary>
/// Verifica el método francés: cuota constante, interés decreciente y capital creciente.
/// </summary>
public class MotorAmortizacionFrancesTests
{
    private readonly MotorAmortizacion _motor = new();

    [Fact]
    public void Cuota_coincide_con_el_valor_de_referencia_del_manual_financiero()
    {
        // Caso clásico: 10 000 al 12 % anual (1 % mensual) a 12 meses.
        // cuota = 10000 · 0.01 · 1.01^12 / (1.01^12 − 1) = 888.4879 -> 888.49
        var tabla = _motor.CalcularFrances(monto: 10_000m, tasaAnual: 12m, plazoMeses: 12);

        Assert.Equal(888.49m, tabla.PrimeraCuota);
    }

    [Fact]
    public void La_cuota_es_identica_en_todos_los_periodos_incluido_el_ultimo()
    {
        var tabla = _motor.CalcularFrances(50_000m, 15.50m, 36);

        Assert.Single(tabla.Cuotas.Select(c => c.Cuota).Distinct());
    }

    [Fact]
    public void Regresion_el_redondeo_no_se_capitaliza_en_plazos_largos()
    {
        // Defecto detectado en el Sprint 2: redondear la cuota e iterar sobre el
        // saldo redondeado hacía que la fracción de centavo impagada se
        // capitalizara con (1+i)^n. Con estos parámetros la última "cuota fija"
        // salía en 2 693.27 frente a 1 833.63: 859.64 de desviación.
        var tabla = _motor.CalcularFrances(100_000m, 22.00m, 480);

        Assert.Equal(1_833.63m, tabla.PrimeraCuota);
        Assert.Equal(tabla.PrimeraCuota, tabla.UltimaCuota);
    }

    [Fact]
    public void El_interes_decrece_y_el_capital_crece_periodo_a_periodo()
    {
        var tabla = _motor.CalcularFrances(20_000m, 8.50m, 24);

        for (var k = 1; k < tabla.Cuotas.Count; k++)
        {
            Assert.True(tabla.Cuotas[k].Interes < tabla.Cuotas[k - 1].Interes,
                $"El interés no decreció en el período {k + 1}.");
            Assert.True(tabla.Cuotas[k].Capital > tabla.Cuotas[k - 1].Capital,
                $"El capital no creció en el período {k + 1}.");
        }
    }

    [Fact]
    public void Un_credito_sin_interes_reparte_el_monto_en_partes_iguales()
    {
        // La fórmula general se indetermina con i = 0; debe resolverse por el caso especial.
        var tabla = _motor.CalcularFrances(1_200m, tasaAnual: 0m, plazoMeses: 12);

        Assert.Equal("Frances", tabla.Metodo);
        Assert.Equal(100m, tabla.PrimeraCuota);
        Assert.Equal(0m, tabla.TotalInteres);
        Assert.Equal(1_200m, tabla.TotalPagado);
    }
}
