using AuthService.Dominio;
using Microsoft.EntityFrameworkCore;

namespace AuthService.Estructura;

/// <summary>
/// Contexto de la base "authdb", propiedad exclusiva del microservicio de autenticación.
/// </summary>
public class AuthDbContext(DbContextOptions<AuthDbContext> options) : DbContext(options)
{
    public DbSet<Usuario> Usuarios => Set<Usuario>();

    /// <summary>
    /// Todas las fechas se guardan y se leen en UTC. Ver <see cref="ConvertidorFechaUtc"/>:
    /// sin esto, SQL Server las devolvería sin marca de zona y el navegador las
    /// interpretaría como hora local.
    /// </summary>
    protected override void ConfigureConventions(ModelConfigurationBuilder configuracion)
    {
        configuracion.Properties<DateTime>().HaveConversion<ConvertidorFechaUtc>();
        configuracion.Properties<DateTime?>().HaveConversion<ConvertidorFechaUtcOpcional>();
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Usuario>(entidad =>
        {
            entidad.ToTable("usuarios");
            entidad.HasKey(u => u.Id);

            entidad.Property(u => u.NombreCompleto)
                   .HasMaxLength(150)
                   .IsRequired();

            entidad.Property(u => u.Email)
                   .HasMaxLength(150)
                   .IsRequired();

            entidad.Property(u => u.PasswordHash)
                   .HasMaxLength(255)
                   .IsRequired();

            entidad.Property(u => u.FechaRegistro)
                   .IsRequired();

            // El correo identifica de forma única a cada usuario.
            entidad.HasIndex(u => u.Email).IsUnique();
        });
    }
}
