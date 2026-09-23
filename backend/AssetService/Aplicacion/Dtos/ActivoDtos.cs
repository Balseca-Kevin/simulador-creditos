using System.ComponentModel.DataAnnotations;

namespace AssetService.Aplicacion.Dtos;

/// <summary>Datos que envía el usuario para registrar o modificar un activo.</summary>
public record ActivoRequest
{
    [Range(1, int.MaxValue, ErrorMessage = "Debes seleccionar una categoría.")]
    public int CategoriaId { get; init; }

    [Required(ErrorMessage = "El nombre del activo es obligatorio.")]
    [StringLength(150, MinimumLength = 3, ErrorMessage = "El nombre debe tener entre 3 y 150 caracteres.")]
    public string Nombre { get; init; } = string.Empty;

    [StringLength(300, ErrorMessage = "La descripción no puede superar los 300 caracteres.")]
    public string Descripcion { get; init; } = string.Empty;

    [Range(1, 100_000_000, ErrorMessage = "El valor estimado debe estar entre 1 y 100 000 000.")]
    public decimal ValorEstimado { get; init; }

    [Required(ErrorMessage = "La fecha de adquisición es obligatoria.")]
    public DateOnly FechaAdquisicion { get; init; }
}

public record CategoriaActivoResponse
{
    public required int Id { get; init; }
    public required string Codigo { get; init; }
    public required string Nombre { get; init; }
    public required string Descripcion { get; init; }
}

public record ActivoResponse
{
    public required Guid Id { get; init; }
    public required CategoriaActivoResponse Categoria { get; init; }
    public required string Nombre { get; init; }
    public required string Descripcion { get; init; }
    public required decimal ValorEstimado { get; init; }
    public required DateOnly FechaAdquisicion { get; init; }
    public required DateTime FechaRegistro { get; init; }
    public DateTime? FechaActualizacion { get; init; }
}

/// <summary>Cuánto suma cada categoría dentro del patrimonio declarado.</summary>
public record PatrimonioPorCategoria
{
    public required string Categoria { get; init; }
    public required int Cantidad { get; init; }
    public required decimal Valor { get; init; }
}

/// <summary>
/// Resumen del patrimonio declarado. El simulador lo muestra junto al ingreso
/// mínimo requerido, para que el usuario vea su respaldo al evaluar una cuota.
/// </summary>
public record ResumenPatrimonioResponse
{
    public required int CantidadActivos { get; init; }
    public required decimal ValorTotal { get; init; }
    public required IReadOnlyList<PatrimonioPorCategoria> PorCategoria { get; init; }
}
