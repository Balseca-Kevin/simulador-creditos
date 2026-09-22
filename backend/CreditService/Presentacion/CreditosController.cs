using System.Security.Claims;
using CreditService.Aplicacion.Contratos;
using CreditService.Aplicacion.Dtos;
using CreditService.Aplicacion.Servicios;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CreditService.Presentacion;

/// <summary>
/// Traduce entre HTTP y los casos de uso de <see cref="IServicioSimulacion"/>.
/// No conoce la base de datos ni el motor de cálculo.
///
/// Todos los endpoints exigen un JWT válido emitido por AuthService, y la
/// identidad del solicitante se toma del token, nunca del cuerpo de la
/// petición: así un usuario no puede consultar ni escribir el historial de otro.
/// </summary>
[ApiController]
[Route("api/creditos")]
[Authorize]
public class CreditosController(IServicioSimulacion simulador) : ControllerBase
{
    /// <summary>HU-04: catálogo de tipos de crédito con su tasa referencial vigente.</summary>
    [HttpGet("tipos")]
    [ProducesResponseType(typeof(IEnumerable<TipoCreditoResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<TipoCreditoResponse>>> Tipos() =>
        Ok(await simulador.Tipos());

    /// <summary>
    /// HU-05: aplica la regla "tipo de crédito determina la tasa" y devuelve
    /// las dos tablas de amortización comparativas.
    /// </summary>
    [HttpPost("simular")]
    [ProducesResponseType(typeof(SimulacionResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<SimulacionResponse>> Simular(SimulacionRequest solicitud)
    {
        if (!TryObtenerUsuario(out var usuarioId)) return Unauthorized();

        var resultado = await simulador.Simular(usuarioId, solicitud);

        return resultado.Exito ? Ok(resultado.Valor) : Traducir(resultado);
    }

    /// <summary>Historial de simulaciones del usuario autenticado, de la más reciente a la más antigua.</summary>
    [HttpGet("historial")]
    [ProducesResponseType(typeof(IEnumerable<SimulacionHistorialResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<IEnumerable<SimulacionHistorialResponse>>> Historial()
    {
        if (!TryObtenerUsuario(out var usuarioId)) return Unauthorized();

        return Ok(await simulador.Historial(usuarioId));
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
            MotivoFallo.NoAutorizado => Unauthorized(cuerpo),
            _ => BadRequest(cuerpo)
        };
    }
}
