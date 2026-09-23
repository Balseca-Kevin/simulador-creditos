namespace AssetService.Dominio;

/// <summary>
/// Bien declarado por una persona: vehículo, inmueble, maquinaria u otro.
/// En el contexto del simulador sirve como respaldo patrimonial del solicitante,
/// junto al ingreso mínimo que exige la cuota.
/// </summary>
public class Activo
{
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>
    /// Identidad tomada del JWT emitido por AuthService. No hay clave foránea:
    /// cada microservicio es dueño de su base y este nunca consulta authdb.
    /// </summary>
    public Guid UsuarioId { get; set; }

    public int CategoriaId { get; set; }
    public CategoriaActivo? Categoria { get; set; }

    public string Nombre { get; set; } = string.Empty;

    public string Descripcion { get; set; } = string.Empty;

    /// <summary>Valor comercial estimado por el propio declarante.</summary>
    public decimal ValorEstimado { get; set; }

    public DateOnly FechaAdquisicion { get; set; }

    public DateTime FechaRegistro { get; set; } = DateTime.UtcNow;

    public DateTime? FechaActualizacion { get; set; }
}
