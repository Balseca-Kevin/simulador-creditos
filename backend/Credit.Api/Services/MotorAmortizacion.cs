using Credit.Api.Dtos;

namespace Credit.Api.Services;

public interface IMotorAmortizacion
{
    TablaAmortizacion CalcularFrances(decimal monto, decimal tasaAnual, int plazoMeses);
    TablaAmortizacion CalcularAleman(decimal monto, decimal tasaAnual, int plazoMeses);
}

/// <summary>
/// Genera las tablas de amortización francesa y alemana.
///
/// Todo el cálculo usa <see cref="decimal"/>, no <c>double</c>: en dinero un error
/// de redondeo binario es un defecto, no una aproximación aceptable.
///
/// Estrategia de redondeo: primero se calcula el saldo teórico exacto de cada
/// período con precisión completa, y solo después se redondea a centavos para
/// presentarlo. El capital de cada fila es la diferencia entre dos saldos
/// consecutivos ya redondeados y el interés es lo que resta de la cuota.
///
/// Por qué así: si se redondeara la cuota y luego se iterara sobre el saldo
/// redondeado, la fracción de centavo que se deja de pagar cada mes se
/// capitalizaría con (1+i)^n. En un crédito a 480 meses al 22 % eso desplaza
/// la última "cuota fija" en más de 850 dólares. Anclando cada fila al saldo
/// exacto, el error nunca supera un centavo por fila y no se acumula.
/// </summary>
public class MotorAmortizacion : IMotorAmortizacion
{
    /// <summary>Convierte la tasa anual porcentual en tasa nominal mensual. 15.50 % anual -> 0.0129166... mensual.</summary>
    public static decimal TasaMensual(decimal tasaAnual) => tasaAnual / 100m / 12m;

    /// <summary>
    /// Método francés: cuota total constante.
    /// cuota = P · i · (1+i)^n / ((1+i)^n − 1)
    /// El interés baja y el capital sube período a período.
    /// </summary>
    public TablaAmortizacion CalcularFrances(decimal monto, decimal tasaAnual, int plazoMeses)
    {
        Validar(monto, tasaAnual, plazoMeses);

        var i = TasaMensual(tasaAnual);

        // Sin interés la fórmula general se indetermina (denominador cero) y el
        // francés coincide con el alemán: capital igual en cada cuota.
        if (i == 0m) return CalcularAleman(monto, tasaAnual, plazoMeses) with { Metodo = "Frances" };

        var cuotaExacta = CuotaExacta(monto, i, plazoMeses);
        var cuotaFija = Redondear(cuotaExacta);

        var filas = new List<CuotaAmortizacion>(plazoMeses);
        var saldoExacto = monto;
        var saldoAnterior = monto;

        for (var periodo = 1; periodo <= plazoMeses; periodo++)
        {
            saldoExacto = saldoExacto * (1m + i) - cuotaExacta;

            // En el último período el saldo teórico es cero salvo por ruido en el
            // dígito 20 o más; se fija en cero para cerrar la deuda exactamente.
            var saldo = periodo == plazoMeses ? 0m : Redondear(saldoExacto);
            var capital = saldoAnterior - saldo;
            var interes = cuotaFija - capital;

            // Salvaguarda para saldos diminutos, donde el interés real es menor
            // que un centavo: el interés nunca se muestra negativo.
            if (interes < 0m) interes = 0m;

            filas.Add(new CuotaAmortizacion
            {
                Periodo = periodo,
                Cuota = capital + interes,
                Interes = interes,
                Capital = capital,
                SaldoRestante = saldo
            });

            saldoAnterior = saldo;
        }

        return Consolidar("Frances", filas);
    }

    /// <summary>
    /// Método alemán: amortización de capital constante (P / n).
    /// El interés se calcula sobre el saldo deudor, así que la cuota total decrece.
    /// </summary>
    public TablaAmortizacion CalcularAleman(decimal monto, decimal tasaAnual, int plazoMeses)
    {
        Validar(monto, tasaAnual, plazoMeses);

        var i = TasaMensual(tasaAnual);
        var capitalExacto = monto / plazoMeses;

        var filas = new List<CuotaAmortizacion>(plazoMeses);
        var saldoAnterior = monto;

        for (var periodo = 1; periodo <= plazoMeses; periodo++)
        {
            // Saldo teórico tras k amortizaciones iguales: P − k·P/n. Cuando P/n
            // no es un valor exacto en centavos, el capital alterna entre dos
            // centavos consecutivos en lugar de amontonar el resto al final.
            var saldo = periodo == plazoMeses ? 0m : Redondear(monto - capitalExacto * periodo);
            var capital = saldoAnterior - saldo;
            var interes = Redondear(saldoAnterior * i);

            filas.Add(new CuotaAmortizacion
            {
                Periodo = periodo,
                Cuota = capital + interes,
                Interes = interes,
                Capital = capital,
                SaldoRestante = saldo
            });

            saldoAnterior = saldo;
        }

        return Consolidar("Aleman", filas);
    }

    /// <summary>
    /// Cuota del método francés con precisión completa.
    /// (1+i)^n se calcula por multiplicación repetida en decimal para no perder
    /// precisión pasando por Math.Pow, que trabaja en coma flotante binaria.
    /// </summary>
    private static decimal CuotaExacta(decimal monto, decimal i, int plazoMeses)
    {
        var factor = 1m;
        for (var k = 0; k < plazoMeses; k++) factor *= 1m + i;

        return monto * i * factor / (factor - 1m);
    }

    private static TablaAmortizacion Consolidar(string metodo, List<CuotaAmortizacion> filas) => new()
    {
        Metodo = metodo,
        Cuotas = filas,
        TotalCapital = filas.Sum(f => f.Capital),
        TotalInteres = filas.Sum(f => f.Interes),
        TotalPagado = filas.Sum(f => f.Cuota),
        PrimeraCuota = filas[0].Cuota,
        UltimaCuota = filas[^1].Cuota
    };

    private static decimal Redondear(decimal valor) =>
        Math.Round(valor, 2, MidpointRounding.AwayFromZero);

    private static void Validar(decimal monto, decimal tasaAnual, int plazoMeses)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(monto);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(plazoMeses);
        ArgumentOutOfRangeException.ThrowIfNegative(tasaAnual);
    }
}
