using CreditService.Aplicacion.Servicios;

namespace CreditService.Tests;

/// <summary>
/// Regla central del documento: el tipo de crédito determina la tasa aplicada,
/// y la tasa anual porcentual se convierte a tasa nominal mensual.
/// </summary>
public class ReglaDeNegocioTests
{
    private readonly MotorAmortizacion _motor = new();

    [Theory]
    [InlineData(15.50, 0.01291667)]   // Crédito de Consumo
    [InlineData(8.50, 0.00708333)]    // Crédito Inmobiliario
    [InlineData(22.00, 0.01833333)]   // Microcrédito
    public void La_tasa_anual_se_convierte_a_tasa_nominal_mensual(decimal tasaAnual, decimal mensualEsperada)
    {
        var mensual = MotorAmortizacion.TasaMensual(tasaAnual);

        Assert.Equal(mensualEsperada, Math.Round(mensual, 8));
    }

    [Fact]
    public void Una_tasa_mayor_encarece_el_credito()
    {
        // Mismo monto y plazo: el microcrédito (22 %) debe costar más que el
        // inmobiliario (8.50 %), y el consumo (15.50 %) quedar en medio.
        var inmobiliario = _motor.CalcularFrances(10_000m, 8.50m, 24).TotalInteres;
        var consumo = _motor.CalcularFrances(10_000m, 15.50m, 24).TotalInteres;
        var micro = _motor.CalcularFrances(10_000m, 22.00m, 24).TotalInteres;

        Assert.True(inmobiliario < consumo);
        Assert.True(consumo < micro);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1000)]
    public void Se_rechaza_un_monto_no_positivo(decimal monto)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => _motor.CalcularFrances(monto, 15.50m, 12));
        Assert.Throws<ArgumentOutOfRangeException>(() => _motor.CalcularAleman(monto, 15.50m, 12));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-6)]
    public void Se_rechaza_un_plazo_no_positivo(int plazo)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => _motor.CalcularFrances(10_000m, 15.50m, plazo));
        Assert.Throws<ArgumentOutOfRangeException>(() => _motor.CalcularAleman(10_000m, 15.50m, plazo));
    }

    [Fact]
    public void Se_rechaza_una_tasa_negativa()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => _motor.CalcularFrances(10_000m, -5m, 12));
    }
}
