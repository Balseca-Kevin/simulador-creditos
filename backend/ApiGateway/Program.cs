var builder = WebApplication.CreateBuilder(args);

const string PoliticaCors = "SpaSimulador";

// Punto de entrada único: la SPA habla solo con el gateway y este reparte las
// peticiones entre los microservicios. Así el frontend no necesita conocer los
// puertos de cada servicio, y el día que se muevan basta con cambiar
// appsettings.json sin tocar ni recompilar la interfaz.
builder.Services
    .AddReverseProxy()
    .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"));

builder.Services.AddCors(opciones =>
    opciones.AddPolicy(PoliticaCors, politica => politica
        .WithOrigins(builder.Configuration.GetSection("Cors:OrigenesPermitidos").Get<string[]>() ?? [])
        .AllowAnyHeader()
        .AllowAnyMethod()));

var app = builder.Build();

app.UseCors(PoliticaCors);

// El gateway no valida el JWT: lo reenvía tal cual y cada microservicio decide.
// Duplicar aquí la validación obligaría a mantener la misma clave en tres sitios.
app.MapReverseProxy();

app.MapGet("/health", () => Results.Ok(new { servicio = "API Gateway", estado = "activo" }));

app.Run();
