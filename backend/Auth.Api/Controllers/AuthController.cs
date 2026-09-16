using System.Security.Claims;
using Auth.Api.Data;
using Auth.Api.Dtos;
using Auth.Api.Models;
using Auth.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Auth.Api.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController(AuthDbContext contexto, ITokenService tokenService) : ControllerBase
{
    /// <summary>HU-01: registra una cuenta nueva y devuelve la sesión ya iniciada.</summary>
    [HttpPost("register")]
    [ProducesResponseType(typeof(AuthResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<AuthResponse>> Registrar(RegistroRequest solicitud)
    {
        var email = solicitud.Email.Trim().ToLowerInvariant();

        if (await contexto.Usuarios.AnyAsync(u => u.Email == email))
        {
            return Conflict(new { mensaje = "Ya existe una cuenta registrada con ese correo." });
        }

        var usuario = new Usuario
        {
            NombreCompleto = solicitud.NombreCompleto.Trim(),
            Email = email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(solicitud.Password)
        };

        contexto.Usuarios.Add(usuario);
        await contexto.SaveChangesAsync();

        return CreatedAtAction(nameof(Perfil), null, ConstruirRespuesta(usuario));
    }

    /// <summary>HU-02: valida credenciales y emite el JWT de acceso.</summary>
    [HttpPost("login")]
    [ProducesResponseType(typeof(AuthResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<AuthResponse>> Login(LoginRequest solicitud)
    {
        var email = solicitud.Email.Trim().ToLowerInvariant();
        var usuario = await contexto.Usuarios.SingleOrDefaultAsync(u => u.Email == email);

        // Se responde igual si el correo no existe o si la contraseña es incorrecta,
        // para no revelar qué correos están registrados.
        if (usuario is null || !BCrypt.Net.BCrypt.Verify(solicitud.Password, usuario.PasswordHash))
        {
            return Unauthorized(new { mensaje = "Correo o contraseña incorrectos." });
        }

        return Ok(ConstruirRespuesta(usuario));
    }

    /// <summary>Devuelve el perfil del portador del token; sirve para revalidar la sesión al recargar la SPA.</summary>
    [HttpGet("me")]
    [Authorize]
    [ProducesResponseType(typeof(UsuarioResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<UsuarioResponse>> Perfil()
    {
        var idTexto = User.FindFirstValue(ClaimTypes.NameIdentifier)
                      ?? User.FindFirstValue("sub");

        if (!Guid.TryParse(idTexto, out var id))
        {
            return Unauthorized();
        }

        var usuario = await contexto.Usuarios.FindAsync(id);

        return usuario is null
            ? Unauthorized()
            : Ok(ProyectarUsuario(usuario));
    }

    private AuthResponse ConstruirRespuesta(Usuario usuario)
    {
        var (token, expiraEn) = tokenService.Generar(usuario);

        return new AuthResponse
        {
            Token = token,
            ExpiraEn = expiraEn,
            Usuario = ProyectarUsuario(usuario)
        };
    }

    private static UsuarioResponse ProyectarUsuario(Usuario usuario) => new()
    {
        Id = usuario.Id,
        NombreCompleto = usuario.NombreCompleto,
        Email = usuario.Email,
        FechaRegistro = usuario.FechaRegistro
    };
}
