using AssetService.Dominio;

namespace AssetService.Aplicacion.Contratos;

/// <summary>
/// Acceso a los activos y su catálogo. La interfaz vive en Aplicacion y la
/// implementación en Estructura, de modo que los casos de uso no dependen de
/// EF Core ni de PostgreSQL.
///
/// Todas las operaciones sobre un activo reciben el usuario: filtrar por él
/// evita que alguien lea o modifique un bien ajeno conociendo su identificador.
/// </summary>
public interface IRepositorioActivos
{
    Task<IReadOnlyList<CategoriaActivo>> ListarCategorias();

    Task<bool> ExisteCategoria(int categoriaId);

    Task<IReadOnlyList<Activo>> ListarDelUsuario(Guid usuarioId);

    Task<Activo?> Buscar(Guid id, Guid usuarioId);

    Task Agregar(Activo activo);

    Task Actualizar(Activo activo);

    Task Eliminar(Activo activo);
}
