using CreditService.Aplicacion.Contratos;
using CreditService.Dominio;
using Microsoft.EntityFrameworkCore;

namespace CreditService.Estructura.Repositorios;

/// <summary>
/// Implementación del repositorio sobre EF Core. Es la única pieza que conoce
/// el motor de base de datos; cambiarlo no obligaría a tocar Aplicacion.
/// </summary>
public class RepositorioCreditos(CreditDbContext contexto) : IRepositorioCreditos
{
    /// <summary>
    /// Se ordena por categoría y, dentro de ella, por Id. El orden importa:
    /// la lista desplegable agrupa por categoría comparando cada elemento con
    /// el anterior, así que si los productos de una misma familia no quedan
    /// contiguos su encabezado aparecería repetido. El Id como segundo criterio
    /// mantiene primero los tipos del documento oficial dentro de cada familia.
    /// </summary>
    public async Task<IReadOnlyList<TipoCredito>> ListarTiposActivos() =>
        await contexto.TiposCredito
            .AsNoTracking()
            .Where(t => t.Activo)
            .OrderBy(t => t.Categoria)
            .ThenBy(t => t.Id)
            .ToListAsync();

    public Task<TipoCredito?> BuscarTipoActivo(int id) =>
        contexto.TiposCredito
            .AsNoTracking()
            .SingleOrDefaultAsync(t => t.Id == id && t.Activo);

    public async Task GuardarSimulacion(Simulacion simulacion)
    {
        contexto.Simulaciones.Add(simulacion);
        await contexto.SaveChangesAsync();
    }

    public async Task<IReadOnlyList<Simulacion>> UltimasSimulaciones(Guid usuarioId, int tope) =>
        await contexto.Simulaciones
            .AsNoTracking()
            .Include(s => s.TipoCredito)
            .Where(s => s.UsuarioId == usuarioId)
            .OrderByDescending(s => s.FechaSimulacion)
            .Take(tope)
            .ToListAsync();

    public Task<Simulacion?> BuscarSimulacion(Guid id, Guid usuarioId) =>
        contexto.Simulaciones
            .AsNoTracking()
            .Include(s => s.TipoCredito)
            .SingleOrDefaultAsync(s => s.Id == id && s.UsuarioId == usuarioId);
}
