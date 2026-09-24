using System.Text;
using AssetService.Aplicacion.Contratos;
using AssetService.Aplicacion.Servicios;
using AssetService.Estructura;
using AssetService.Estructura.Repositorios;
using AssetService.Presentacion;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

const string PoliticaCors = "SpaSimulador";

// ---------- Persistencia: base "assetdb", exclusiva de este microservicio ----------
builder.Services.AddDbContext<AssetDbContext>(opciones =>
    opciones.UseSqlServer(builder.Configuration.GetConnectionString("AssetDb")));

// ---------- Casos de uso y acceso a datos ----------
// La interfaz del repositorio vive en Aplicacion y su implementación en
// Estructura: es aquí, en el arranque, donde se conectan las dos capas.
builder.Services.AddScoped<IRepositorioActivos, RepositorioActivos>();
builder.Services.AddScoped<IServicioActivos, ServicioActivos>();

// ---------- Validación de los tokens emitidos por AuthService ----------
var jwt = builder.Configuration.GetSection("Jwt");
var clave = jwt["Key"] ?? throw new InvalidOperationException("Falta la sección de configuración 'Jwt'.");

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(opciones =>
    {
        opciones.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwt["Issuer"],
            ValidAudience = jwt["Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(clave)),
            ClockSkew = TimeSpan.FromSeconds(30)
        };
    });

builder.Services.AddAuthorization();

builder.Services.AddCors(opciones =>
    opciones.AddPolicy(PoliticaCors, politica => politica
        .WithOrigins(builder.Configuration.GetSection("Cors:OrigenesPermitidos").Get<string[]>() ?? [])
        .AllowAnyHeader()
        .AllowAnyMethod()));

builder.Services
    .AddControllers()
    .ConfigureApiBehaviorOptions(o => o.InvalidModelStateResponseFactory = ErroresEnEspanol.Respuesta);

builder.Services.AddOpenApi();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    using var alcance = app.Services.CreateScope();
    await alcance.ServiceProvider.GetRequiredService<AssetDbContext>().Database.MigrateAsync();

    app.MapOpenApi();
}

app.UseCors(PoliticaCors);
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.MapGet("/health", () => Results.Ok(new { servicio = "Asset API", estado = "activo" }));

app.Run();
