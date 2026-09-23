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

    /// <summary>
    /// Cuota que resultaría con cada opción de los desplegables, para anticipar
    /// el efecto de cada elección antes de calcular la simulación completa.
    /// </summary>
    [HttpPost("estimaciones")]
    [ProducesResponseType(typeof(EstimacionesResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<EstimacionesResponse>> Estimaciones(EstimacionesRequest solicitud)
    {
        var resultado = await simulador.Estimaciones(solicitud);

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

    /// <summary>
    /// Emite un enlace temporal para abrir el reporte de una simulación.
    /// Se necesita porque el navegador, al abrir una pestaña, no puede enviar
    /// la cabecera de autorización.
    /// </summary>
    [HttpPost("simulaciones/{id:guid}/enlace-reporte")]
    [ProducesResponseType(typeof(EnlaceReporteResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<EnlaceReporteResponse>> EnlaceReporte(Guid id)
    {
        if (!TryObtenerUsuario(out var usuarioId)) return Unauthorized();

        var resultado = await simulador.EmitirEnlaceReporte(usuarioId, id, User.Identity?.Name);

        return resultado.Exito ? Ok(resultado.Valor) : Traducir(resultado);
    }

    /// <summary>
    /// Devuelve el PDF del reporte. No lleva [Authorize] porque lo abre el
    /// navegador al navegar, sin cabeceras: la autorización la aporta el enlace
    /// de un solo uso, que ya validó la sesión y la propiedad de la simulación.
    /// Se envía "inline" para que el navegador lo muestre en su visor en lugar
    /// de descargarlo directamente.
    /// </summary>
    [HttpGet("reportes/{token}")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(FileResult), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status410Gone)]
    public async Task<IActionResult> Reporte(string token)
    {
        var resultado = await simulador.GenerarReporte(token);

        if (!resultado.Exito)
        {
            // 410 y no 401: el enlace existió y ya no sirve. Un 401 haría que el
            // navegador pidiera credenciales, que no es lo que corresponde aquí.
            return StatusCode(StatusCodes.Status410Gone, new { mensaje = resultado.Mensaje });
        }

        var reporte = resultado.Valor!;

        Response.Headers.ContentDisposition = $"inline; filename=\"{reporte.NombreArchivo}\"";

        return File(reporte.Contenido, "application/pdf");
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
