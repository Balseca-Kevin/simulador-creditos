using CreditService.Dominio;

namespace CreditService.Aplicacion.Contratos;

/// <summary>
/// Acceso a los datos del microservicio de créditos. La interfaz vive en
/// Aplicacion y la implementación en Estructura, de modo que los casos de uso
/// no dependen de EF Core ni de PostgreSQL.
/// </summary>
public interface IRepositorioCreditos
{
    Task<IReadOnlyList<TipoCredito>> ListarTiposActivos();

    Task<TipoCredito?> BuscarTipoActivo(int id);

    Task GuardarSimulacion(Simulacion simulacion);

    /// <summary>Simulaciones del usuario, de la más reciente a la más antigua, con su tipo de crédito cargado.</summary>
    Task<IReadOnlyList<Simulacion>> UltimasSimulaciones(Guid usuarioId, int tope);
}
