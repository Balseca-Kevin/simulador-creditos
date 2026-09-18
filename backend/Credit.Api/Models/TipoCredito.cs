namespace Credit.Api.Models;

/// <summary>
/// Catálogo de productos crediticios. La tasa vive aquí y no en el código:
/// así la regla de negocio "tipo de crédito determina la tasa" es dinámica y
/// puede actualizarse sin recompilar el microservicio.
/// </summary>
public class TipoCredito
{
    public int Id { get; set; }

    /// <summary>Identificador estable usado por el frontend (p. ej. CONSUMO).</summary>
    public string Codigo { get; set; } = string.Empty;

    public string Nombre { get; set; } = string.Empty;

    /// <summary>Tasa de interés referencial anual, expresada en porcentaje (15.50 = 15.50 %).</summary>
    public decimal TasaAnual { get; set; }

    public string Descripcion { get; set; } = string.Empty;

    public bool Activo { get; set; } = true;
}
