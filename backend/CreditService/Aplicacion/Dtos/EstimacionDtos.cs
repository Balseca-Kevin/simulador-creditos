using System.ComponentModel.DataAnnotations;
using CreditService.Dominio;

namespace CreditService.Aplicacion.Dtos;

/// <summary>
/// Estado actual del formulario. A partir de él se calcula, para cada opción
/// disponible, qué cuota resultaría si el usuario la eligiera.
/// </summary>
public record EstimacionesRequest
{
    [Range(1, int.MaxValue, ErrorMessage = "El tipo de crédito seleccionado no es válido.")]
    public int TipoCreditoId { get; init; }

    [Range(100, 1_000_000, ErrorMessage = "El monto debe estar entre 100 y 1 000 000.")]
    public decimal Monto { get; init; }

    [Range(1, 480, ErrorMessage = "El plazo debe estar entre 1 y 480 meses.")]
    public int PlazoMeses { get; init; }

    [EnumDataType(typeof(FrecuenciaPago), ErrorMessage = "La frecuencia de pago no es válida.")]
    public FrecuenciaPago FrecuenciaPago { get; init; } = FrecuenciaPago.Mensual;

    public bool IncluirSeguroDesgravamen { get; init; } = true;
}

/// <summary>
/// Cuota que resultaría al elegir una opción concreta, dejando fijo el resto
/// del formulario. Es nula cuando esa combinación no se puede calcular.
/// </summary>
public record EstimacionTipo
{
    public required int TipoCreditoId { get; init; }
    public required string Nombre { get; init; }
    public required string Categoria { get; init; }
    public required string Descripcion { get; init; }
    public required decimal TasaAnual { get; init; }
    public decimal? CuotaEstimada { get; init; }
    public decimal? IngresoMinimoRequerido { get; init; }
}

public record EstimacionMonto
{
    public required decimal Monto { get; init; }
    public decimal? CuotaEstimada { get; init; }
}

public record EstimacionPlazo
{
    public required int PlazoMeses { get; init; }

    /// <summary>Falso cuando el plazo no se divide en cuotas de la frecuencia elegida.</summary>
    public required bool CompatibleConFrecuencia { get; init; }

    public decimal? CuotaEstimada { get; init; }
}

/// <summary>
/// Estimaciones para las tres listas desplegables del formulario.
/// Se calculan con el mismo motor de amortización que la simulación final,
/// así que no pueden desviarse del resultado que verá el usuario al calcular.
/// </summary>
public record EstimacionesResponse
{
    public required IReadOnlyList<EstimacionTipo> PorTipo { get; init; }
    public required IReadOnlyList<EstimacionMonto> PorMonto { get; init; }
    public required IReadOnlyList<EstimacionPlazo> PorPlazo { get; init; }

    /// <summary>Método al que corresponden las cuotas mostradas.</summary>
    public required string Metodo { get; init; }
}

/// <summary>Enlace temporal para abrir el reporte en una pestaña nueva.</summary>
public record EnlaceReporteResponse
{
    public required string Url { get; init; }
    public required DateTime ExpiraEn { get; init; }
}

/// <summary>Documento ya maquetado, listo para enviarse al navegador.</summary>
public record ReporteGenerado
{
    public required byte[] Contenido { get; init; }
    public required string NombreArchivo { get; init; }
}
