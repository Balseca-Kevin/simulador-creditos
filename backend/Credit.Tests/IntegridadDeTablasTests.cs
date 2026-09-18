using Credit.Api.Dtos;
using Credit.Api.Services;

namespace Credit.Tests;

/// <summary>
/// Invariantes que deben cumplirse en ambos métodos y con las tres tasas
/// referenciales del documento oficial. Aquí se detectarían los centavos
/// perdidos por redondeo, que es el defecto más probable de un simulador.
/// </summary>
public class IntegridadDeTablasTests
{
    private readonly MotorAmortizacion _motor = new();

    /// <summary>Tasas de la sección 3: consumo, inmobiliario y microcrédito.</summary>
    public static TheoryData<decimal, decimal, int> Escenarios() => new()
    {
        { 15.50m,    5_000m,  12 },
        { 15.50m,   37_777m,  17 },   // cifras no redondas: fuerzan arrastre de redondeo
        {  8.50m,  120_000m, 240 },   // hipoteca a 20 años
        {  8.50m,      999m,   7 },
        { 22.00m,    1_500m,  18 },
        { 22.00m,  100_000m, 480 },   // plazo máximo: donde el arrastre se capitalizaba
    };

    [Theory]
    [MemberData(nameof(Escenarios))]
    public void El_frances_amortiza_exactamente_el_capital_prestado(decimal tasa, decimal monto, int plazo)
        => VerificarInvariantes(_motor.CalcularFrances(monto, tasa, plazo), monto, plazo);

    [Theory]
    [MemberData(nameof(Escenarios))]
    public void El_aleman_amortiza_exactamente_el_capital_prestado(decimal tasa, decimal monto, int plazo)
        => VerificarInvariantes(_motor.CalcularAleman(monto, tasa, plazo), monto, plazo);

    /// <summary>
    /// La propiedad que define al método francés. Las sumas pueden cuadrar y aun
    /// así la última cuota estar distorsionada: por eso se verifica aparte.
    /// </summary>
    [Theory]
    [MemberData(nameof(Escenarios))]
    public void El_frances_mantiene_la_cuota_fija_en_todas_las_filas(decimal tasa, decimal monto, int plazo)
    {
        var tabla = _motor.CalcularFrances(monto, tasa, plazo);

        foreach (var fila in tabla.Cuotas)
        {
            Assert.True(Math.Abs(fila.Cuota - tabla.PrimeraCuota) <= 0.01m,
                $"La cuota del período {fila.Periodo} ({fila.Cuota}) se alejó de la cuota fija ({tabla.PrimeraCuota}).");
        }
    }

    /// <summary>La propiedad que define al método alemán: capital P/n en cada fila, al centavo.</summary>
    [Theory]
    [MemberData(nameof(Escenarios))]
    public void El_aleman_amortiza_el_mismo_capital_en_todas_las_filas(decimal tasa, decimal monto, int plazo)
    {
        var tabla = _motor.CalcularAleman(monto, tasa, plazo);
        var capitalTeorico = monto / plazo;

        foreach (var fila in tabla.Cuotas)
        {
            Assert.True(Math.Abs(fila.Capital - capitalTeorico) <= 0.01m,
                $"El capital del período {fila.Periodo} ({fila.Capital}) se alejó de P/n ({capitalTeorico:F4}).");
        }
    }

    private static void VerificarInvariantes(TablaAmortizacion tabla, decimal monto, int plazo)
    {
        Assert.Equal(plazo, tabla.Cuotas.Count);

        // Ni un centavo de más ni de menos: la suma del capital es el monto prestado.
        Assert.Equal(monto, tabla.TotalCapital);

        // La deuda queda saldada exactamente en cero.
        Assert.Equal(0m, tabla.Cuotas[^1].SaldoRestante);

        // Cada cuota es la suma de sus partes, y el total pagado la de todas.
        Assert.Equal(tabla.TotalCapital + tabla.TotalInteres, tabla.TotalPagado);

        foreach (var fila in tabla.Cuotas)
        {
            Assert.Equal(fila.Capital + fila.Interes, fila.Cuota);
            Assert.True(fila.SaldoRestante >= 0m, $"El saldo se volvió negativo en el período {fila.Periodo}.");
            Assert.True(fila.Interes >= 0m, $"El interés fue negativo en el período {fila.Periodo}.");
            Assert.True(fila.Capital > 0m, $"El capital no fue positivo en el período {fila.Periodo}.");
        }

        // El saldo debe encadenarse: el de cada período es el anterior menos su capital.
        var saldoEsperado = monto;
        foreach (var fila in tabla.Cuotas)
        {
            saldoEsperado -= fila.Capital;
            Assert.Equal(saldoEsperado, fila.SaldoRestante);
        }
    }
}
