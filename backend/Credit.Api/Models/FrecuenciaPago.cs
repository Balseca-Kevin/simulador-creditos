namespace Credit.Api.Models;

/// <summary>
/// Cada cuánto se paga una cuota. Determina los meses que cubre cada período y,
/// con ello, la tasa periódica y el número total de cuotas.
/// </summary>
public enum FrecuenciaPago
{
    Mensual = 1,
    Bimensual = 2,
    Trimestral = 3,
    Semestral = 6,

    /// <summary>Un único pago de capital e intereses al final del plazo.</summary>
    AlVencimiento = 99
}

public static class FrecuenciaPagoExtensiones
{
    /// <summary>Meses que abarca cada cuota. Al vencimiento, el período es el plazo completo.</summary>
    public static int MesesPorPeriodo(this FrecuenciaPago frecuencia, int plazoMeses) =>
        frecuencia == FrecuenciaPago.AlVencimiento ? plazoMeses : (int)frecuencia;

    public static string Etiqueta(this FrecuenciaPago frecuencia) => frecuencia switch
    {
        FrecuenciaPago.Mensual => "Mensual",
        FrecuenciaPago.Bimensual => "Bimensual",
        FrecuenciaPago.Trimestral => "Trimestral",
        FrecuenciaPago.Semestral => "Semestral",
        FrecuenciaPago.AlVencimiento => "Al vencimiento",
        _ => frecuencia.ToString()
    };
}
