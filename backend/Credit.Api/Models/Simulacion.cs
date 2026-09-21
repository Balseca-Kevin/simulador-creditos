namespace Credit.Api.Models;

/// <summary>
/// Registro histórico de una simulación solicitada por un usuario.
/// Se guarda el resumen y los parámetros de entrada: las tablas completas se
/// recalculan de forma determinista a partir de ellos, así que almacenarlas
/// fila por fila solo duplicaría información.
/// </summary>
public class Simulacion
{
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>
    /// Identidad tomada del JWT emitido por la Auth API. No hay clave foránea:
    /// cada microservicio es dueño de su base y este servicio nunca consulta authdb.
    /// </summary>
    public Guid UsuarioId { get; set; }

    public int TipoCreditoId { get; set; }
    public TipoCredito? TipoCredito { get; set; }

    public decimal Monto { get; set; }

    public int PlazoMeses { get; set; }

    public FrecuenciaPago FrecuenciaPago { get; set; } = FrecuenciaPago.Mensual;

    public bool IncluyeSeguroDesgravamen { get; set; }

    /// <summary>Tasa vigente en el momento de simular; se congela para que el historial sea fiel.</summary>
    public decimal TasaAnualAplicada { get; set; }

    public decimal CuotaFija { get; set; }

    public decimal TotalInteresFrances { get; set; }

    public decimal TotalInteresAleman { get; set; }

    /// <summary>Ingreso mínimo para el método francés, que es el que suele contratarse.</summary>
    public decimal IngresoMinimoRequerido { get; set; }

    public DateTime FechaSimulacion { get; set; } = DateTime.UtcNow;
}
