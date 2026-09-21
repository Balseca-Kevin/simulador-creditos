using System.Text;
using System.Text.Json.Serialization;
using Credit.Api.Data;
using Credit.Api.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

const string PoliticaCors = "SpaSimulador";

// ---------- Persistencia: base "creditdb", exclusiva de este microservicio ----------
builder.Services.AddDbContext<CreditDbContext>(opciones =>
    opciones.UseNpgsql(builder.Configuration.GetConnectionString("CreditDb")));

builder.Services.AddScoped<IMotorAmortizacion, MotorAmortizacion>();

// ---------- Validación de los tokens emitidos por la Auth API ----------
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

builder.Services.AddCors(opciones =>
    opciones.AddPolicy(PoliticaCors, politica => politica
        .WithOrigins(builder.Configuration.GetSection("Cors:OrigenesPermitidos").Get<string[]>() ?? [])
        .AllowAnyHeader()
        .AllowAnyMethod()));

builder.Services
    .AddControllers()
    // Las frecuencias viajan como texto ("Trimestral") y no como número: el JSON
    // se entiende sin conocer el enum y un valor inválido se rechaza con 400.
    .AddJsonOptions(o => o.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()))
    .ConfigureApiBehaviorOptions(o => o.InvalidModelStateResponseFactory = ErroresEnEspanol.Respuesta);
builder.Services.AddOpenApi();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    using var alcance = app.Services.CreateScope();
    await alcance.ServiceProvider.GetRequiredService<CreditDbContext>().Database.MigrateAsync();

    app.MapOpenApi();
}

app.UseCors(PoliticaCors);
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.MapGet("/health", () => Results.Ok(new { servicio = "Credit API", estado = "activo" }));

app.Run();
