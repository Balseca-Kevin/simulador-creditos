using System.Security.Claims;
using AuthService.Aplicacion.Contratos;
using AuthService.Aplicacion.Dtos;
using AuthService.Aplicacion.Servicios;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AuthService.Presentacion;

/// <summary>
/// Traduce entre HTTP y los casos de uso de <see cref="IServicioAutenticacion"/>.
/// No conoce la base de datos: recibe una solicitud, delega y convierte el
/// resultado en el código de estado que corresponde.
/// </summary>
[ApiController]
[Route("api/auth")]
public class AuthController(IServicioAutenticacion autenticacion) : ControllerBase
{
    /// <summary>HU-01: registra una cuenta nueva y devuelve la sesión ya iniciada.</summary>
    [HttpPost("register")]
    [ProducesResponseType(typeof(AuthResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<AuthResponse>> Registrar(RegistroRequest solicitud)
    {
        var resultado = await autenticacion.Registrar(solicitud);

        return resultado.Exito
            ? CreatedAtAction(nameof(Perfil), null, resultado.Valor)
            : Traducir(resultado);
    }

    /// <summary>HU-02: valida credenciales y emite el JWT de acceso.</summary>
    [HttpPost("login")]
    [ProducesResponseType(typeof(AuthResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<AuthResponse>> Login(LoginRequest solicitud)
    {
        var resultado = await autenticacion.Login(solicitud);

        return resultado.Exito ? Ok(resultado.Valor) : Traducir(resultado);
    }

    /// <summary>Devuelve el perfil del portador del token; sirve para revalidar la sesión al recargar la SPA.</summary>
    [HttpGet("me")]
    [Authorize]
    [ProducesResponseType(typeof(UsuarioResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<UsuarioResponse>> Perfil()
    {
        if (!TryObtenerUsuario(out var id)) return Unauthorized();

        var resultado = await autenticacion.Perfil(id);

        return resultado.Exito ? Ok(resultado.Valor) : Traducir(resultado);
    }

    private bool TryObtenerUsuario(out Guid usuarioId)
    {
        var idTexto = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub");
        return Guid.TryParse(idTexto, out usuarioId);
    }

    /// <summary>Convierte el motivo del fallo en el código HTTP equivalente.</summary>
    private ObjectResult Traducir<T>(Resultado<T> resultado)
    {
        var cuerpo = new { mensaje = resultado.Mensaje };

        return resultado.Motivo switch
        {
            MotivoFallo.Conflicto => Conflict(cuerpo),
            MotivoFallo.NoAutorizado => Unauthorized(cuerpo),
            _ => BadRequest(cuerpo)
        };
    }
}
