using AuthService.Aplicacion.Contratos;
using AuthService.Dominio;
using Microsoft.EntityFrameworkCore;

namespace AuthService.Estructura.Repositorios;

/// <summary>
/// Implementación del repositorio sobre EF Core. Es la única pieza que conoce
/// el motor de base de datos; cambiarlo no obligaría a tocar Aplicacion.
/// </summary>
public class RepositorioUsuarios(AuthDbContext contexto) : IRepositorioUsuarios
{
    public Task<bool> ExisteConEmail(string email) =>
        contexto.Usuarios.AnyAsync(u => u.Email == email);

    public Task<Usuario?> BuscarPorEmail(string email) =>
        contexto.Usuarios.SingleOrDefaultAsync(u => u.Email == email);

    public async Task<Usuario?> BuscarPorId(Guid id) =>
        await contexto.Usuarios.FindAsync(id);

    public async Task Agregar(Usuario usuario)
    {
        contexto.Usuarios.Add(usuario);
        await contexto.SaveChangesAsync();
    }
}
