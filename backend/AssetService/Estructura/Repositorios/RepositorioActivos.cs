using AssetService.Aplicacion.Contratos;
using AssetService.Dominio;
using Microsoft.EntityFrameworkCore;

namespace AssetService.Estructura.Repositorios;

/// <summary>
/// Implementación del repositorio sobre EF Core. Es la única pieza que conoce
/// el motor de base de datos; cambiarlo no obligaría a tocar Aplicacion.
/// </summary>
public class RepositorioActivos(AssetDbContext contexto) : IRepositorioActivos
{
    public async Task<IReadOnlyList<CategoriaActivo>> ListarCategorias() =>
        await contexto.Categorias
            .AsNoTracking()
            .Where(c => c.Habilitada)
            .OrderBy(c => c.Id)
            .ToListAsync();

    public Task<bool> ExisteCategoria(int categoriaId) =>
        contexto.Categorias.AnyAsync(c => c.Id == categoriaId && c.Habilitada);

    public async Task<IReadOnlyList<Activo>> ListarDelUsuario(Guid usuarioId) =>
        await contexto.Activos
            .AsNoTracking()
            .Include(a => a.Categoria)
            .Where(a => a.UsuarioId == usuarioId)
            .OrderByDescending(a => a.FechaRegistro)
            .ToListAsync();

    /// <summary>
    /// Filtra también por usuario: así un identificador ajeno no devuelve el
    /// bien de otra persona, sino nada.
    /// </summary>
    public Task<Activo?> Buscar(Guid id, Guid usuarioId) =>
        contexto.Activos
            .Include(a => a.Categoria)
            .SingleOrDefaultAsync(a => a.Id == id && a.UsuarioId == usuarioId);

    public async Task Agregar(Activo activo)
    {
        contexto.Activos.Add(activo);
        await contexto.SaveChangesAsync();
    }

    public async Task Actualizar(Activo activo)
    {
        contexto.Activos.Update(activo);
        await contexto.SaveChangesAsync();
    }

    public async Task Eliminar(Activo activo)
    {
        contexto.Activos.Remove(activo);
        await contexto.SaveChangesAsync();
    }
}
