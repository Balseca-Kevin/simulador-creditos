namespace Credit.Api.Services;

/// <summary>
/// Parámetros de validación del token. Deben coincidir exactamente con los que
/// usa la Auth API para firmar: si divergen, este servicio rechazará tokens legítimos.
/// </summary>
public class JwtOptions
{
    public const string SeccionConfiguracion = "Jwt";

    public string Issuer { get; set; } = string.Empty;
    public string Audience { get; set; } = string.Empty;
    public string Key { get; set; } = string.Empty;
}
