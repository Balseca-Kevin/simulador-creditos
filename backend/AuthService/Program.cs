using System.Text;
using AuthService.Aplicacion.Contratos;
using AuthService.Aplicacion.Servicios;
using AuthService.Estructura;
using AuthService.Estructura.Repositorios;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

const string PoliticaCors = "SpaSimulador";

// ---------- Persistencia: base "authdb", exclusiva de este microservicio ----------
// El contexto vive en la capa Estructura, pero las migraciones se mantienen en
// este proyecto: hay que decírselo a EF, que por omisión las busca junto al
// contexto.
builder.Services.AddDbContext<AuthDbContext>(opciones =>
    opciones.UseSqlServer(
        builder.Configuration.GetConnectionString("AuthDb"),
        sql => sql.MigrationsAssembly(typeof(Program).Assembly.FullName)));

// ---------- Emisión de tokens ----------
builder.Services.Configure<JwtOptions>(
    builder.Configuration.GetSection(JwtOptions.SeccionConfiguracion));
builder.Services.AddScoped<ITokenService, TokenService>();

// ---------- Casos de uso y acceso a datos ----------
// La interfaz del repositorio vive en Aplicacion y su implementacion en
// Estructura: es aqui, en el arranque, donde se conectan las dos capas.
builder.Services.AddScoped<IRepositorioUsuarios, RepositorioUsuarios>();
builder.Services.AddScoped<IServicioAutenticacion, ServicioAutenticacion>();

// ---------- Validación de tokens ----------
var jwt = builder.Configuration.GetSection(JwtOptions.SeccionConfiguracion).Get<JwtOptions>()
          ?? throw new InvalidOperationException("Falta la sección de configuración 'Jwt'.");

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
            ValidIssuer = jwt.Issuer,
            ValidAudience = jwt.Audience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt.Key)),
            ClockSkew = TimeSpan.FromSeconds(30)
        };
    });

builder.Services.AddAuthorization();

// ---------- Acceso desde la SPA de React ----------
builder.Services.AddCors(opciones =>
    opciones.AddPolicy(PoliticaCors, politica => politica
        .WithOrigins(builder.Configuration.GetSection("Cors:OrigenesPermitidos").Get<string[]>() ?? [])
        .AllowAnyHeader()
        .AllowAnyMethod()));

// Los controladores viven en el proyecto Presentacion, no en este: hay que
// registrar explicitamente ese ensamblado para que ASP.NET los descubra.
builder.Services
    .AddControllers()
    .AddApplicationPart(typeof(AuthService.Presentacion.AuthController).Assembly);
builder.Services.AddOpenApi();

var app = builder.Build();

// En desarrollo la base se pone al día sola para no frenar el ritmo de los sprints.
if (app.Environment.IsDevelopment())
{
    using var alcance = app.Services.CreateScope();
    await alcance.ServiceProvider.GetRequiredService<AuthDbContext>().Database.MigrateAsync();

    app.MapOpenApi();
}

app.UseCors(PoliticaCors);
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.MapGet("/health", () => Results.Ok(new { servicio = "Auth API", estado = "activo" }));

app.Run();
