using Credit.Api.Dtos;
using Credit.Api.Models;

namespace Credit.Api.Services;

/// <summary>Todo lo que define un crédito a efectos del cálculo.</summary>
public record ParametrosCredito(
    decimal Monto,
    decimal TasaAnual,
    int PlazoMeses,
    FrecuenciaPago Frecuencia = FrecuenciaPago.Mensual,
    decimal TasaSeguroMensual = 0m);

public interface IMotorAmortizacion
{
    TablaAmortizacion CalcularFrances(ParametrosCredito parametros);
    TablaAmortizacion CalcularAleman(ParametrosCredito parametros);
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
/// redondeado, la fracción de centavo que se deja de pagar cada período se
/// capitalizaría con (1+i)^n. En un crédito a 480 meses al 22 % eso desplaza
/// la última "cuota fija" en más de 850 dólares. Anclando cada fila al saldo
/// exacto, el error nunca supera un centavo por fila y no se acumula.
/// </summary>
public class MotorAmortizacion : IMotorAmortizacion
{
    /// <summary>
    /// Proporción máxima del ingreso que puede comprometerse en la cuota.
    /// El 40 % es el umbral de capacidad de pago habitual en la banca ecuatoriana.
    /// </summary>
    public const decimal RelacionCuotaIngreso = 0.40m;

    /// <summary>Convierte la tasa anual porcentual en tasa nominal mensual. 15.50 % anual -> 0.0129166... mensual.</summary>
    public static decimal TasaMensual(decimal tasaAnual) => TasaPeriodica(tasaAnual, 1);

    /// <summary>Tasa nominal del período: la anual repartida según los meses que cubre cada cuota.</summary>
    public static decimal TasaPeriodica(decimal tasaAnual, int mesesPorPeriodo) =>
        tasaAnual / 100m * mesesPorPeriodo / 12m;

    /// <summary>Atajo: método francés con pago mensual y sin seguro.</summary>
    public TablaAmortizacion CalcularFrances(decimal monto, decimal tasaAnual, int plazoMeses) =>
        CalcularFrances(new ParametrosCredito(monto, tasaAnual, plazoMeses));

    /// <summary>Atajo: método alemán con pago mensual y sin seguro.</summary>
    public TablaAmortizacion CalcularAleman(decimal monto, decimal tasaAnual, int plazoMeses) =>
        CalcularAleman(new ParametrosCredito(monto, tasaAnual, plazoMeses));

    /// <summary>
    /// Método francés: cuota constante (sin contar el seguro).
    /// cuota = P · i · (1+i)^n / ((1+i)^n − 1)
    /// El interés baja y el capital sube período a período.
    /// </summary>
    public TablaAmortizacion CalcularFrances(ParametrosCredito parametros)
    {
        var (mesesPorPeriodo, numeroCuotas) = Validar(parametros);

        var monto = parametros.Monto;
        var i = TasaPeriodica(parametros.TasaAnual, mesesPorPeriodo);

        // Sin interés la fórmula general se indetermina (denominador cero) y el
        // francés coincide con el alemán: capital igual en cada cuota.
        if (i == 0m) return CalcularAleman(parametros) with { Metodo = "Frances" };

        var cuotaExacta = CuotaExacta(monto, i, numeroCuotas);
        var cuotaFija = Redondear(cuotaExacta);

        var filas = new List<CuotaAmortizacion>(numeroCuotas);
        var saldoExacto = monto;
        var saldoAnterior = monto;

        for (var periodo = 1; periodo <= numeroCuotas; periodo++)
        {
            saldoExacto = saldoExacto * (1m + i) - cuotaExacta;

            // En el último período el saldo teórico es cero salvo por ruido en el
            // dígito 20 o más; se fija en cero para cerrar la deuda exactamente.
            var saldo = periodo == numeroCuotas ? 0m : Redondear(saldoExacto);
            var capital = saldoAnterior - saldo;
            var interes = cuotaFija - capital;

            // Salvaguarda para saldos diminutos, donde el interés real es menor
            // que un centavo: el interés nunca se muestra negativo.
            if (interes < 0m) interes = 0m;

            filas.Add(ConstruirFila(periodo, capital, interes, saldoAnterior, saldo, parametros, mesesPorPeriodo));
            saldoAnterior = saldo;
        }

        return Consolidar("Frances", filas, mesesPorPeriodo);
    }

    /// <summary>
    /// Método alemán: amortización de capital constante (P / n).
    /// El interés se calcula sobre el saldo deudor, así que la cuota total decrece.
    /// </summary>
    public TablaAmortizacion CalcularAleman(ParametrosCredito parametros)
    {
        var (mesesPorPeriodo, numeroCuotas) = Validar(parametros);

        var monto = parametros.Monto;
        var i = TasaPeriodica(parametros.TasaAnual, mesesPorPeriodo);
        var capitalExacto = monto / numeroCuotas;

        var filas = new List<CuotaAmortizacion>(numeroCuotas);
        var saldoAnterior = monto;

        for (var periodo = 1; periodo <= numeroCuotas; periodo++)
        {
            // Saldo teórico tras k amortizaciones iguales: P − k·P/n. Cuando P/n
            // no es un valor exacto en centavos, el capital alterna entre dos
            // centavos consecutivos en lugar de amontonar el resto al final.
            var saldo = periodo == numeroCuotas ? 0m : Redondear(monto - capitalExacto * periodo);
            var capital = saldoAnterior - saldo;
            var interes = Redondear(saldoAnterior * i);

            filas.Add(ConstruirFila(periodo, capital, interes, saldoAnterior, saldo, parametros, mesesPorPeriodo));
            saldoAnterior = saldo;
        }

        return Consolidar("Aleman", filas, mesesPorPeriodo);
    }

    /// <summary>
    /// El seguro de desgravamen se cobra sobre el saldo que se adeuda al inicio
    /// del período: cubre la deuda pendiente en caso de fallecimiento del titular.
    /// Por eso baja a medida que se amortiza el crédito.
    /// </summary>
    private static CuotaAmortizacion ConstruirFila(
        int periodo,
        decimal capital,
        decimal interes,
        decimal saldoAnterior,
        decimal saldo,
        ParametrosCredito parametros,
        int mesesPorPeriodo)
    {
        var seguro = Redondear(saldoAnterior * parametros.TasaSeguroMensual / 100m * mesesPorPeriodo);
        var cuota = capital + interes;

        return new CuotaAmortizacion
        {
            Periodo = periodo,
            Cuota = cuota,
            Interes = interes,
            Capital = capital,
            Seguro = seguro,
            CuotaTotal = cuota + seguro,
            SaldoRestante = saldo
        };
    }

    /// <summary>
    /// Cuota del método francés con precisión completa.
    /// (1+i)^n se calcula por multiplicación repetida en decimal para no perder
    /// precisión pasando por Math.Pow, que trabaja en coma flotante binaria.
    /// </summary>
    private static decimal CuotaExacta(decimal monto, decimal i, int numeroCuotas)
    {
        var factor = 1m;
        for (var k = 0; k < numeroCuotas; k++) factor *= 1m + i;

        return monto * i * factor / (factor - 1m);
    }

    private static TablaAmortizacion Consolidar(string metodo, List<CuotaAmortizacion> filas, int mesesPorPeriodo)
    {
        var cuotaTotalMaxima = filas.Max(f => f.CuotaTotal);
        var totalCapital = filas.Sum(f => f.Capital);
        var totalInteres = filas.Sum(f => f.Interes);
        var totalSeguro = filas.Sum(f => f.Seguro);

        return new TablaAmortizacion
        {
            Metodo = metodo,
            Cuotas = filas,
            TotalCapital = totalCapital,
            TotalInteres = totalInteres,
            TotalSeguro = totalSeguro,
            TotalPagado = totalCapital + totalInteres + totalSeguro,
            PrimeraCuota = filas[0].Cuota,
            UltimaCuota = filas[^1].Cuota,
            PrimeraCuotaTotal = filas[0].CuotaTotal,
            UltimaCuotaTotal = filas[^1].CuotaTotal,
            CuotaTotalMaxima = cuotaTotalMaxima,
            IngresoMinimoRequerido = IngresoMinimo(cuotaTotalMaxima, mesesPorPeriodo)
        };
    }

    /// <summary>
    /// Una cuota trimestral de 3 000 exige apartar 1 000 cada mes: por eso la cuota
    /// se lleva primero a su equivalente mensual. Se redondea hacia arriba al
    /// centavo para que el ingreso sugerido nunca quede por debajo del umbral.
    /// </summary>
    private static decimal IngresoMinimo(decimal cuotaMaxima, int mesesPorPeriodo)
    {
        var requerido = cuotaMaxima / mesesPorPeriodo / RelacionCuotaIngreso;
        return Math.Ceiling(requerido * 100m) / 100m;
    }

    private static decimal Redondear(decimal valor) =>
        Math.Round(valor, 2, MidpointRounding.AwayFromZero);

    private static (int MesesPorPeriodo, int NumeroCuotas) Validar(ParametrosCredito p)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(p.Monto);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(p.PlazoMeses);
        ArgumentOutOfRangeException.ThrowIfNegative(p.TasaAnual);
        ArgumentOutOfRangeException.ThrowIfNegative(p.TasaSeguroMensual);

        if (!Enum.IsDefined(p.Frecuencia))
        {
            throw new ArgumentOutOfRangeException(nameof(p.Frecuencia), "La frecuencia de pago no es válida.");
        }

        var mesesPorPeriodo = p.Frecuencia.MesesPorPeriodo(p.PlazoMeses);

        if (p.PlazoMeses % mesesPorPeriodo != 0)
        {
            throw new ArgumentException(
                $"Con frecuencia {p.Frecuencia.Etiqueta().ToLowerInvariant()} el plazo debe ser múltiplo de {mesesPorPeriodo} meses; {p.PlazoMeses} no lo es.",
                nameof(p.PlazoMeses));
        }

        return (mesesPorPeriodo, p.PlazoMeses / mesesPorPeriodo);
    }
}
