namespace Credit.Api.Dtos;

/// <summary>Una fila de la tabla: lo que se paga en un período concreto.</summary>
public record CuotaAmortizacion
{
    public required int Periodo { get; init; }
    public required decimal Cuota { get; init; }
    public required decimal Interes { get; init; }
    public required decimal Capital { get; init; }
    public required decimal SaldoRestante { get; init; }
}

/// <summary>Tabla completa de un método de amortización, con sus totales.</summary>
public record TablaAmortizacion
{
    public required string Metodo { get; init; }
    public required IReadOnlyList<CuotaAmortizacion> Cuotas { get; init; }
    public required decimal TotalCapital { get; init; }
    public required decimal TotalInteres { get; init; }
    public required decimal TotalPagado { get; init; }

    /// <summary>Primera cuota: en el método alemán es la más alta de todas.</summary>
    public required decimal PrimeraCuota { get; init; }

    /// <summary>Última cuota: en el método francés coincide con la primera.</summary>
    public required decimal UltimaCuota { get; init; }
}
