namespace Auth.Api.Services;

/// <summary>
/// Parámetros de firma del token. La Credit API deberá usar exactamente
/// el mismo emisor, audiencia y clave para poder validar estos tokens.
/// </summary>
public class JwtOptions
{
    public const string SeccionConfiguracion = "Jwt";

    public string Issuer { get; set; } = string.Empty;
    public string Audience { get; set; } = string.Empty;
    public string Key { get; set; } = string.Empty;
    public int MinutosDeVigencia { get; set; } = 120;
}
