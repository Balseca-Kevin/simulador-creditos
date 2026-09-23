using AssetService.Dominio;
using Microsoft.EntityFrameworkCore;

namespace AssetService.Estructura;

/// <summary>
/// Contexto de la base "assetdb", propiedad exclusiva del microservicio de activos.
/// </summary>
public class AssetDbContext(DbContextOptions<AssetDbContext> options) : DbContext(options)
{
    public DbSet<CategoriaActivo> Categorias => Set<CategoriaActivo>();
    public DbSet<Activo> Activos => Set<Activo>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<CategoriaActivo>(entidad =>
        {
            entidad.ToTable("categorias_activo");
            entidad.HasKey(c => c.Id);

            entidad.Property(c => c.Codigo).HasMaxLength(30).IsRequired();
            entidad.Property(c => c.Nombre).HasMaxLength(100).IsRequired();
            entidad.Property(c => c.Descripcion).HasMaxLength(300).IsRequired();

            entidad.HasIndex(c => c.Codigo).IsUnique();

            entidad.HasData(
                new CategoriaActivo
                {
                    Id = 1,
                    Codigo = "VEHICULO",
                    Nombre = "Vehículo",
                    Descripcion = "Automóviles, motocicletas y vehículos de carga.",
                    Habilitada = true
                },
                new CategoriaActivo
                {
                    Id = 2,
                    Codigo = "INMUEBLE",
                    Nombre = "Inmueble",
                    Descripcion = "Casas, departamentos, terrenos y locales comerciales.",
                    Habilitada = true
                },
                new CategoriaActivo
                {
                    Id = 3,
                    Codigo = "MAQUINARIA",
                    Nombre = "Maquinaria y equipo",
                    Descripcion = "Equipos productivos, herramientas y maquinaria industrial.",
                    Habilitada = true
                },
                new CategoriaActivo
                {
                    Id = 4,
                    Codigo = "INVERSION",
                    Nombre = "Inversión financiera",
                    Descripcion = "Depósitos a plazo, acciones y participaciones.",
                    Habilitada = true
                },
                new CategoriaActivo
                {
                    Id = 5,
                    Codigo = "OTRO",
                    Nombre = "Otros bienes",
                    Descripcion = "Bienes que no encajan en las categorías anteriores.",
                    Habilitada = true
                });
        });

        modelBuilder.Entity<Activo>(entidad =>
        {
            entidad.ToTable("activos");
            entidad.HasKey(a => a.Id);

            entidad.Property(a => a.Nombre).HasMaxLength(150).IsRequired();
            entidad.Property(a => a.Descripcion).HasMaxLength(300).IsRequired();
            entidad.Property(a => a.ValorEstimado).HasPrecision(18, 2).IsRequired();
            entidad.Property(a => a.FechaAdquisicion).IsRequired();

            entidad.HasOne(a => a.Categoria)
                   .WithMany()
                   .HasForeignKey(a => a.CategoriaId)
                   .OnDelete(DeleteBehavior.Restrict);

            // Los activos siempre se consultan por usuario y de lo más reciente
            // a lo más antiguo.
            entidad.HasIndex(a => new { a.UsuarioId, a.FechaRegistro });
        });
    }
}
