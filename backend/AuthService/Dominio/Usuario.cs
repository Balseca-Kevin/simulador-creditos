namespace AuthService.Dominio;

/// <summary>
/// Identidad de una persona registrada en el simulador.
/// La contraseña nunca se almacena en claro: solo su hash BCrypt.
/// </summary>
public class Usuario
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public string NombreCompleto { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public string PasswordHash { get; set; } = string.Empty;

    public DateTime FechaRegistro { get; set; } = DateTime.UtcNow;
}
