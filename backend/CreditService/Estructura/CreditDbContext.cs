using CreditService.Dominio;
using Microsoft.EntityFrameworkCore;

namespace CreditService.Estructura;

/// <summary>
/// Contexto de la base "creditdb", propiedad exclusiva del microservicio de créditos.
/// </summary>
public class CreditDbContext(DbContextOptions<CreditDbContext> options) : DbContext(options)
{
    public DbSet<TipoCredito> TiposCredito => Set<TipoCredito>();
    public DbSet<Simulacion> Simulaciones => Set<Simulacion>();

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
        modelBuilder.Entity<TipoCredito>(entidad =>
        {
            entidad.ToTable("tipos_credito");
            entidad.HasKey(t => t.Id);

            entidad.Property(t => t.Codigo).HasMaxLength(30).IsRequired();
            entidad.Property(t => t.Nombre).HasMaxLength(100).IsRequired();
            entidad.Property(t => t.Categoria).HasMaxLength(40).IsRequired();
            entidad.Property(t => t.Descripcion).HasMaxLength(300).IsRequired();

            // 5 enteros y 2 decimales alcanzan para cualquier tasa porcentual.
            entidad.Property(t => t.TasaAnual).HasPrecision(5, 2).IsRequired();

            // Las primas de desgravamen son fracciones pequeñas: se guardan con 4 decimales.
            entidad.Property(t => t.TasaSeguroDesgravamenMensual).HasPrecision(7, 4).IsRequired();

            entidad.HasIndex(t => t.Codigo).IsUnique();

            // Los tres primeros conservan la tasa fijada en la sección 3 del
            // documento oficial del proyecto, que es la que debe verificarse.
            //
            // Los siguientes amplían el catálogo con los segmentos definidos por
            // el Banco Central del Ecuador, usando sus tasas activas efectivas
            // referenciales de agosto de 2026. Difieren ligeramente de las tres
            // primeras (el BCE publica Consumo 15.78 % e Inmobiliario 8.72 %),
            // y por eso se añaden como segmentos aparte en lugar de sobrescribirlas.
            //
            // Las primas de desgravamen son valores referenciales del mercado:
            // más bajas a mayor plazo y garantía, más altas a mayor riesgo.
            entidad.HasData(
                new TipoCredito
                {
                    Id = 1,
                    Codigo = "CONSUMO",
                    Nombre = "Crédito de Consumo",
                    Categoria = "Consumo",
                    TasaAnual = 15.50m,
                    TasaSeguroDesgravamenMensual = 0.0500m,
                    Descripcion = "Adquisición de bienes de consumo o pago de servicios.",
                    Activo = true
                },
                new TipoCredito
                {
                    Id = 2,
                    Codigo = "INMOBILIARIO",
                    Nombre = "Crédito Inmobiliario",
                    Categoria = "Vivienda",
                    TasaAnual = 8.50m,
                    TasaSeguroDesgravamenMensual = 0.0400m,
                    Descripcion = "Compra, construcción o remodelación de vivienda.",
                    Activo = true
                },
                new TipoCredito
                {
                    Id = 3,
                    Codigo = "MICROCREDITO",
                    Nombre = "Microcrédito",
                    Categoria = "Microcrédito",
                    TasaAnual = 22.00m,
                    TasaSeguroDesgravamenMensual = 0.0700m,
                    Descripcion = "Financiamiento para actividades productivas a pequeña escala.",
                    Activo = true
                },
                new TipoCredito
                {
                    Id = 4,
                    Codigo = "PRODUCTIVO_CORPORATIVO",
                    Nombre = "Productivo Corporativo",
                    Categoria = "Productivo",
                    TasaAnual = 6.79m,
                    TasaSeguroDesgravamenMensual = 0.0300m,
                    Descripcion = "Empresas con ventas anuales superiores a cinco millones de dólares.",
                    Activo = true
                },
                new TipoCredito
                {
                    Id = 5,
                    Codigo = "PRODUCTIVO_EMPRESARIAL",
                    Nombre = "Productivo Empresarial",
                    Categoria = "Productivo",
                    TasaAnual = 8.62m,
                    TasaSeguroDesgravamenMensual = 0.0350m,
                    Descripcion = "Empresas con ventas anuales entre uno y cinco millones de dólares.",
                    Activo = true
                },
                new TipoCredito
                {
                    Id = 6,
                    Codigo = "PRODUCTIVO_PYMES",
                    Nombre = "Productivo PYMES",
                    Categoria = "Productivo",
                    TasaAnual = 9.18m,
                    TasaSeguroDesgravamenMensual = 0.0450m,
                    Descripcion = "Pequeñas y medianas empresas con ventas anuales de hasta un millón de dólares.",
                    Activo = true
                },
                new TipoCredito
                {
                    Id = 7,
                    Codigo = "EDUCATIVO",
                    Nombre = "Crédito Educativo",
                    Categoria = "Educativo",
                    TasaAnual = 8.95m,
                    TasaSeguroDesgravamenMensual = 0.0450m,
                    Descripcion = "Financiamiento de estudios de grado, posgrado y formación profesional.",
                    Activo = true
                },
                new TipoCredito
                {
                    Id = 8,
                    Codigo = "EDUCATIVO_SOCIAL",
                    Nombre = "Crédito Educativo Social",
                    Categoria = "Educativo",
                    TasaAnual = 5.49m,
                    TasaSeguroDesgravamenMensual = 0.0400m,
                    Descripcion = "Estudios para personas en situación de vulnerabilidad, con tasa preferente.",
                    Activo = true
                },
                new TipoCredito
                {
                    Id = 9,
                    Codigo = "VIVIENDA_INTERES_SOCIAL",
                    Nombre = "Vivienda de Interés Social",
                    Categoria = "Vivienda",
                    TasaAnual = 4.99m,
                    TasaSeguroDesgravamenMensual = 0.0400m,
                    Descripcion = "Primera vivienda para familias de bajos ingresos, con tope de precio regulado.",
                    Activo = true
                },
                new TipoCredito
                {
                    Id = 10,
                    Codigo = "VIVIENDA_INTERES_PUBLICO",
                    Nombre = "Vivienda de Interés Público",
                    Categoria = "Vivienda",
                    TasaAnual = 4.99m,
                    TasaSeguroDesgravamenMensual = 0.0400m,
                    Descripcion = "Primera vivienda dentro de proyectos calificados por el Estado.",
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
            entidad.Property(s => s.IngresoMinimoRequerido).HasPrecision(18, 2).IsRequired();

            // Se guarda como texto para que la base sea legible sin conocer el enum.
            entidad.Property(s => s.FrecuenciaPago)
                   .HasConversion<string>()
                   .HasMaxLength(20)
                   .IsRequired();

            entidad.HasOne(s => s.TipoCredito)
                   .WithMany()
                   .HasForeignKey(s => s.TipoCreditoId)
                   .OnDelete(DeleteBehavior.Restrict);

            // El historial siempre se consulta por usuario y de lo más reciente a lo más antiguo.
            entidad.HasIndex(s => new { s.UsuarioId, s.FechaSimulacion });
        });
    }
}
