using Credit.Api.Services;

namespace Credit.Tests;

/// <summary>
/// Verifica el método alemán: amortización de capital constante y cuota total decreciente.
/// </summary>
public class MotorAmortizacionAlemanTests
{
    private readonly MotorAmortizacion _motor = new();

    [Fact]
    public void Reproduce_un_caso_calculado_a_mano()
    {
        // 12 000 al 12 % anual (1 % mensual) a 12 meses: capital fijo de 1 000.
        // Cuota 1  = 1000 + 12000·0.01 = 1120
        // Cuota 12 = 1000 +  1000·0.01 = 1010
        // Interés total = 0.01 · (12000 + 11000 + … + 1000) = 0.01 · 78000 = 780
        var tabla = _motor.CalcularAleman(12_000m, 12m, 12);

        Assert.Equal(1_120m, tabla.PrimeraCuota);
        Assert.Equal(1_010m, tabla.UltimaCuota);
        Assert.Equal(780m, tabla.TotalInteres);
    }

    [Fact]
    public void La_amortizacion_de_capital_es_constante()
    {
        var tabla = _motor.CalcularAleman(36_000m, 22m, 36);

        var capitalesRegulares = tabla.Cuotas.Take(tabla.Cuotas.Count - 1).Select(c => c.Capital).Distinct();

        Assert.Single(capitalesRegulares);
        Assert.Equal(1_000m, capitalesRegulares.Single());
    }

    [Fact]
    public void La_cuota_total_decrece_en_cada_periodo()
    {
        var tabla = _motor.CalcularAleman(30_000m, 15.50m, 24);

        for (var k = 1; k < tabla.Cuotas.Count; k++)
        {
            Assert.True(tabla.Cuotas[k].Cuota < tabla.Cuotas[k - 1].Cuota,
                $"La cuota no decreció en el período {k + 1}.");
        }
    }

    [Fact]
    public void Genera_menos_interes_total_que_el_metodo_frances()
    {
        // El alemán amortiza capital más rápido al inicio, así que el saldo sobre
        // el que corre el interés baja antes.
        var aleman = _motor.CalcularAleman(25_000m, 15.50m, 48);
        var frances = _motor.CalcularFrances(25_000m, 15.50m, 48);

        Assert.True(aleman.TotalInteres < frances.TotalInteres);
    }
}
