namespace AssetService.Dominio;

/// <summary>
/// Familia a la que pertenece un activo. Vive en la base y no en el código para
/// que el catálogo se pueda ampliar sin recompilar el microservicio, igual que
/// las tasas en el servicio de créditos.
/// </summary>
public class CategoriaActivo
{
    public int Id { get; set; }

    /// <summary>Identificador estable usado por el frontend (p. ej. VEHICULO).</summary>
    public string Codigo { get; set; } = string.Empty;

    public string Nombre { get; set; } = string.Empty;

    public string Descripcion { get; set; } = string.Empty;

    /// <summary>Se llama Habilitada y no Activo para no confundirla con la entidad Activo.</summary>
    public bool Habilitada { get; set; } = true;
}
