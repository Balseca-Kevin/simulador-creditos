using System.ComponentModel.DataAnnotations;

namespace Auth.Api.Dtos;

/// <summary>Datos que envía el visitante para crear su cuenta.</summary>
public record RegistroRequest
{
    [Required(ErrorMessage = "El nombre completo es obligatorio.")]
    [StringLength(150, MinimumLength = 3, ErrorMessage = "El nombre debe tener entre 3 y 150 caracteres.")]
    public string NombreCompleto { get; init; } = string.Empty;

    [Required(ErrorMessage = "El correo es obligatorio.")]
    [EmailAddress(ErrorMessage = "El formato del correo no es válido.")]
    [StringLength(150, ErrorMessage = "El correo no puede superar los 150 caracteres.")]
    public string Email { get; init; } = string.Empty;

    [Required(ErrorMessage = "La contraseña es obligatoria.")]
    [StringLength(100, MinimumLength = 8, ErrorMessage = "La contraseña debe tener al menos 8 caracteres.")]
    public string Password { get; init; } = string.Empty;
}

/// <summary>Credenciales para iniciar sesión.</summary>
public record LoginRequest
{
    [Required(ErrorMessage = "El correo es obligatorio.")]
    [EmailAddress(ErrorMessage = "El formato del correo no es válido.")]
    public string Email { get; init; } = string.Empty;

    [Required(ErrorMessage = "La contraseña es obligatoria.")]
    public string Password { get; init; } = string.Empty;
}

/// <summary>Respuesta entregada tras un registro o login exitoso.</summary>
public record AuthResponse
{
    public required string Token { get; init; }
    public required DateTime ExpiraEn { get; init; }
    public required UsuarioResponse Usuario { get; init; }
}

/// <summary>Perfil público del usuario; nunca incluye el hash de la contraseña.</summary>
public record UsuarioResponse
{
    public required Guid Id { get; init; }
    public required string NombreCompleto { get; init; }
    public required string Email { get; init; }
    public required DateTime FechaRegistro { get; init; }
}
