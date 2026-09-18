using Credit.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace Credit.Api.Data;

/// <summary>
/// Contexto de la base "creditdb", propiedad exclusiva del microservicio de créditos.
/// </summary>
public class CreditDbContext(DbContextOptions<CreditDbContext> options) : DbContext(options)
{
    public DbSet<TipoCredito> TiposCredito => Set<TipoCredito>();
    public DbSet<Simulacion> Simulaciones => Set<Simulacion>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<TipoCredito>(entidad =>
        {
            entidad.ToTable("tipos_credito");
            entidad.HasKey(t => t.Id);

            entidad.Property(t => t.Codigo).HasMaxLength(30).IsRequired();
            entidad.Property(t => t.Nombre).HasMaxLength(100).IsRequired();
            entidad.Property(t => t.Descripcion).HasMaxLength(300).IsRequired();

            // 5 enteros y 2 decimales alcanzan para cualquier tasa porcentual.
            entidad.Property(t => t.TasaAnual).HasPrecision(5, 2).IsRequired();

            entidad.HasIndex(t => t.Codigo).IsUnique();

            // Tasas referenciales definidas en la sección 3 del documento oficial.
            entidad.HasData(
                new TipoCredito
                {
                    Id = 1,
                    Codigo = "CONSUMO",
                    Nombre = "Crédito de Consumo",
                    TasaAnual = 15.50m,
                    Descripcion = "Adquisición de bienes de consumo o pago de servicios.",
                    Activo = true
                },
                new TipoCredito
                {
                    Id = 2,
                    Codigo = "INMOBILIARIO",
                    Nombre = "Crédito Inmobiliario",
                    TasaAnual = 8.50m,
                    Descripcion = "Compra, construcción o remodelación de vivienda.",
                    Activo = true
                },
                new TipoCredito
                {
                    Id = 3,
                    Codigo = "MICROCREDITO",
                    Nombre = "Microcrédito",
                    TasaAnual = 22.00m,
                    Descripcion = "Financiamiento para actividades productivas a pequeña escala.",
                    Activo = true
                });
        });

        modelBuilder.Entity<Simulacion>(entidad =>
        {
            entidad.ToTable("simulaciones");
            entidad.HasKey(s => s.Id);

            entidad.Property(s => s.Monto).HasPrecision(18, 2).IsRequired();
            entidad.Property(s => s.TasaAnualAplicada).HasPrecision(5, 2).IsRequired();
            entidad.Property(s => s.CuotaFija).HasPrecision(18, 2).IsRequired();
            entidad.Property(s => s.TotalInteresFrances).HasPrecision(18, 2).IsRequired();
            entidad.Property(s => s.TotalInteresAleman).HasPrecision(18, 2).IsRequired();

            entidad.HasOne(s => s.TipoCredito)
                   .WithMany()
                   .HasForeignKey(s => s.TipoCreditoId)
                   .OnDelete(DeleteBehavior.Restrict);

            // El historial siempre se consulta por usuario y de lo más reciente a lo más antiguo.
            entidad.HasIndex(s => new { s.UsuarioId, s.FechaSimulacion });
        });
    }
}
