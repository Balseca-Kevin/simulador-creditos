using Credit.Api.Models;
using Credit.Api.Services;

namespace Credit.Tests;

/// <summary>
/// La frecuencia de pago cambia los meses que cubre cada cuota y, con ello,
/// la tasa del período y el número de cuotas.
/// </summary>
public class FrecuenciaPagoTests
{
    private readonly MotorAmortizacion _motor = new();

    [Theory]
    [InlineData(FrecuenciaPago.Mensual, 24)]
    [InlineData(FrecuenciaPago.Bimensual, 12)]
    [InlineData(FrecuenciaPago.Trimestral, 8)]
    [InlineData(FrecuenciaPago.Semestral, 4)]
    [InlineData(FrecuenciaPago.AlVencimiento, 1)]
    public void El_numero_de_cuotas_depende_de_la_frecuencia(FrecuenciaPago frecuencia, int cuotasEsperadas)
    {
        var tabla = _motor.CalcularFrances(new ParametrosCredito(12_000m, 15.50m, 24, frecuencia));

        Assert.Equal(cuotasEsperadas, tabla.Cuotas.Count);
    }

    [Fact]
    public void La_tasa_periodica_reparte_la_anual_segun_los_meses_de_cada_cuota()
    {
        // 12 % anual: 1 % al mes, 3 % al trimestre, 6 % al semestre.
        Assert.Equal(0.01m, MotorAmortizacion.TasaPeriodica(12m, 1));
        Assert.Equal(0.03m, MotorAmortizacion.TasaPeriodica(12m, 3));
        Assert.Equal(0.06m, MotorAmortizacion.TasaPeriodica(12m, 6));
    }

    [Fact]
    public void Trimestral_reproduce_un_caso_calculado_a_mano()
    {
        // 12 000 al 12 % anual, trimestral a 12 meses: 4 cuotas al 3 % por período,
        // capital fijo de 3 000 en el alemán.
        // Cuota 1 = 3000 + 12000·0.03 = 3360 ; cuota 4 = 3000 + 3000·0.03 = 3090
        // Interés total = 0.03 · (12000 + 9000 + 6000 + 3000) = 900
        var tabla = _motor.CalcularAleman(new ParametrosCredito(12_000m, 12m, 12, FrecuenciaPago.Trimestral));

        Assert.Equal(3_360m, tabla.PrimeraCuota);
        Assert.Equal(3_090m, tabla.UltimaCuota);
        Assert.Equal(900m, tabla.TotalInteres);
    }

    [Fact]
    public void Al_vencimiento_se_paga_todo_en_una_cuota_con_interes_simple()
    {
        // 10 000 al 12 % a 6 meses: un solo pago de capital más 6 meses de interés.
        // Interés = 10000 · 0.12 · 6/12 = 600
        var tabla = _motor.CalcularFrances(new ParametrosCredito(10_000m, 12m, 6, FrecuenciaPago.AlVencimiento));

        var unica = Assert.Single(tabla.Cuotas);
        Assert.Equal(10_000m, unica.Capital);
        Assert.Equal(600m, unica.Interes);
        Assert.Equal(0m, unica.SaldoRestante);
    }

    [Theory]
    [InlineData(FrecuenciaPago.Trimestral, 10)]
    [InlineData(FrecuenciaPago.Semestral, 9)]
    [InlineData(FrecuenciaPago.Bimensual, 7)]
    public void Se_rechaza_un_plazo_que_no_es_multiplo_de_la_frecuencia(FrecuenciaPago frecuencia, int plazo)
    {
        Assert.Throws<ArgumentException>(() =>
            _motor.CalcularFrances(new ParametrosCredito(10_000m, 15.50m, plazo, frecuencia)));
    }

    /// <summary>Las invariantes de integridad deben sostenerse con cualquier frecuencia.</summary>
    [Theory]
    [InlineData(FrecuenciaPago.Bimensual)]
    [InlineData(FrecuenciaPago.Trimestral)]
    [InlineData(FrecuenciaPago.Semestral)]
    [InlineData(FrecuenciaPago.AlVencimiento)]
    public void Ambos_metodos_amortizan_el_capital_exacto_con_cualquier_frecuencia(FrecuenciaPago frecuencia)
    {
        var parametros = new ParametrosCredito(37_777m, 22m, 60, frecuencia);

        foreach (var tabla in new[] { _motor.CalcularFrances(parametros), _motor.CalcularAleman(parametros) })
        {
            Assert.Equal(37_777m, tabla.TotalCapital);
            Assert.Equal(0m, tabla.Cuotas[^1].SaldoRestante);
            Assert.All(tabla.Cuotas, fila => Assert.Equal(fila.Capital + fila.Interes, fila.Cuota));
        }
    }
}
