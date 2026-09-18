using System.ComponentModel.DataAnnotations;

namespace Credit.Api.Dtos;

/// <summary>Datos que envía el usuario para simular un crédito.</summary>
public record SimulacionRequest
{
    [Required(ErrorMessage = "Debes seleccionar un tipo de crédito.")]
    [Range(1, int.MaxValue, ErrorMessage = "El tipo de crédito seleccionado no es válido.")]
    public int TipoCreditoId { get; init; }

    [Range(100, 1_000_000, ErrorMessage = "El monto debe estar entre 100 y 1 000 000.")]
    public decimal Monto { get; init; }

    [Range(1, 480, ErrorMessage = "El plazo debe estar entre 1 y 480 meses.")]
    public int PlazoMeses { get; init; }
}

/// <summary>Producto crediticio tal como lo consume el frontend.</summary>
public record TipoCreditoResponse
{
    public required int Id { get; init; }
    public required string Codigo { get; init; }
    public required string Nombre { get; init; }
    public required decimal TasaAnual { get; init; }
    public required string Descripcion { get; init; }
}

/// <summary>Lectura rápida de cuál método conviene y por cuánto.</summary>
public record ComparativoMetodos
{
    public required string MetodoMasEconomico { get; init; }
    public required decimal DiferenciaTotalInteres { get; init; }
}

/// <summary>Resultado completo de una simulación: ambas tablas más su comparativo.</summary>
public record SimulacionResponse
{
    public required Guid Id { get; init; }
    public required DateTime FechaSimulacion { get; init; }
    public required TipoCreditoResponse TipoCredito { get; init; }
    public required decimal Monto { get; init; }
    public required int PlazoMeses { get; init; }
    public required decimal TasaAnualAplicada { get; init; }
    public required decimal TasaMensualAplicada { get; init; }
    public required TablaAmortizacion Francesa { get; init; }
    public required TablaAmortizacion Alemana { get; init; }
    public required ComparativoMetodos Comparativo { get; init; }
}

/// <summary>Entrada del historial; no incluye las tablas para no inflar la respuesta.</summary>
public record SimulacionHistorialResponse
{
    public required Guid Id { get; init; }
    public required DateTime FechaSimulacion { get; init; }
    public required string TipoCredito { get; init; }
    public required decimal Monto { get; init; }
    public required int PlazoMeses { get; init; }
    public required decimal TasaAnualAplicada { get; init; }
    public required decimal CuotaFija { get; init; }
    public required decimal TotalInteresFrances { get; init; }
    public required decimal TotalInteresAleman { get; init; }
}
