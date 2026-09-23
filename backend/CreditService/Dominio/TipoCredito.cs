namespace CreditService.Dominio;

/// <summary>
/// Catálogo de productos crediticios. Las tasas viven aquí y no en el código:
/// así la regla de negocio "tipo de crédito determina la tasa" es dinámica y
/// puede actualizarse sin recompilar el microservicio.
/// </summary>
public class TipoCredito
{
    public int Id { get; set; }

    /// <summary>Identificador estable usado por el frontend (p. ej. CONSUMO).</summary>
    public string Codigo { get; set; } = string.Empty;

    public string Nombre { get; set; } = string.Empty;

    /// <summary>
    /// Familia a la que pertenece el producto (Consumo, Vivienda, Productivo...).
    /// Sirve para agrupar el catálogo cuando la lista crece.
    /// </summary>
    public string Categoria { get; set; } = string.Empty;

    /// <summary>Tasa de interés referencial anual, expresada en porcentaje (15.50 = 15.50 %).</summary>
    public decimal TasaAnual { get; set; }

    /// <summary>
    /// Prima mensual del seguro de desgravamen, en porcentaje sobre el saldo
    /// deudor (0.0500 = 0.05 % del saldo cada mes). Vive por producto porque en
    /// la práctica cada línea de crédito tiene su propia póliza.
    /// </summary>
    public decimal TasaSeguroDesgravamenMensual { get; set; }

    public string Descripcion { get; set; } = string.Empty;

    public bool Activo { get; set; } = true;
}
