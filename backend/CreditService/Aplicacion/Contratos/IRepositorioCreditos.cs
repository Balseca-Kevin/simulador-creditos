using CreditService.Dominio;

namespace CreditService.Aplicacion.Contratos;

/// <summary>
/// Acceso a los datos del microservicio de créditos. La interfaz vive en
/// Aplicacion y la implementación en Estructura, de modo que los casos de uso
/// no dependen de EF Core ni del motor de base de datos.
/// </summary>
public interface IRepositorioCreditos
{
    Task<IReadOnlyList<TipoCredito>> ListarTiposActivos();

    Task<TipoCredito?> BuscarTipoActivo(int id);

    Task GuardarSimulacion(Simulacion simulacion);

    /// <summary>Simulaciones del usuario, de la más reciente a la más antigua, con su tipo de crédito cargado.</summary>
    Task<IReadOnlyList<Simulacion>> UltimasSimulaciones(Guid usuarioId, int tope);

    /// <summary>
    /// Busca una simulación del usuario indicado. Filtrar también por usuario
    /// evita que alguien abra el reporte de una simulación ajena conociendo su id.
    /// </summary>
    Task<Simulacion?> BuscarSimulacion(Guid id, Guid usuarioId);
}
