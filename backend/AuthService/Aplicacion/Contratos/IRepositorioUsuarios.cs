using AuthService.Dominio;

namespace AuthService.Aplicacion.Contratos;

/// <summary>
/// Acceso a los usuarios persistidos. La interfaz vive en Aplicacion y la
/// implementación en Estructura: la dependencia queda invertida y la capa de
/// aplicación no necesita conocer EF Core ni el motor de base de datos.
/// </summary>
public interface IRepositorioUsuarios
{
    Task<bool> ExisteConEmail(string email);

    Task<Usuario?> BuscarPorEmail(string email);

    Task<Usuario?> BuscarPorId(Guid id);

    Task Agregar(Usuario usuario);
}
