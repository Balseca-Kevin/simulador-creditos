namespace Credit.Api.Dtos;

/// <summary>Una fila de la tabla: lo que se paga en un período concreto.</summary>
public record CuotaAmortizacion
{
    public required int Periodo { get; init; }

    /// <summary>Capital más interés. En el método francés es constante.</summary>
    public required decimal Cuota { get; init; }

    public required decimal Interes { get; init; }
    public required decimal Capital { get; init; }

    /// <summary>Prima de desgravamen del período; cero si no se contrató.</summary>
    public required decimal Seguro { get; init; }

    /// <summary>Lo que efectivamente se paga: cuota más seguro.</summary>
    public required decimal CuotaTotal { get; init; }

    public required decimal SaldoRestante { get; init; }
}

/// <summary>Tabla completa de un método de amortización, con sus totales.</summary>
public record TablaAmortizacion
{
    public required string Metodo { get; init; }
    public required IReadOnlyList<CuotaAmortizacion> Cuotas { get; init; }

    public required decimal TotalCapital { get; init; }
    public required decimal TotalInteres { get; init; }
    public required decimal TotalSeguro { get; init; }

    /// <summary>Capital + interés + seguro: el costo total del crédito para el cliente.</summary>
    public required decimal TotalPagado { get; init; }

    /// <summary>Primera cuota sin seguro. En el método alemán es la más alta de todas.</summary>
    public required decimal PrimeraCuota { get; init; }

    /// <summary>Última cuota sin seguro. En el método francés coincide con la primera.</summary>
    public required decimal UltimaCuota { get; init; }

    public required decimal PrimeraCuotaTotal { get; init; }
    public required decimal UltimaCuotaTotal { get; init; }

    /// <summary>La cuota total más alta de la tabla; es la que determina si el cliente califica.</summary>
    public required decimal CuotaTotalMaxima { get; init; }

    /// <summary>
    /// Ingreso mensual mínimo para que la cuota no supere la capacidad de pago.
    /// Se calcula sobre la cuota más alta, llevada a su equivalente mensual.
    /// </summary>
    public required decimal IngresoMinimoRequerido { get; init; }
}
