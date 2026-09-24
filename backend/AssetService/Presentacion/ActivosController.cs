using System.Security.Claims;
using AssetService.Aplicacion.Contratos;
using AssetService.Aplicacion.Dtos;
using AssetService.Aplicacion.Servicios;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AssetService.Presentacion;

/// <summary>
/// CRUD de los activos declarados por el usuario autenticado.
///
/// Todos los endpoints exigen un JWT válido emitido por AuthService, y la
/// identidad se toma del token, nunca del cuerpo o de la ruta: así nadie puede
/// leer ni modificar los bienes de otra persona conociendo su identificador.
/// </summary>
[ApiController]
[Route("api/activos")]
[Authorize]
public class ActivosController(IServicioActivos servicio) : ControllerBase
{
    /// <summary>Activos del usuario, del más reciente al más antiguo.</summary>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<ActivoResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<IEnumerable<ActivoResponse>>> Listar()
    {
        if (!TryObtenerUsuario(out var usuarioId)) return Unauthorized();

        return Ok(await servicio.Listar(usuarioId));
    }

    /// <summary>Resumen del patrimonio declarado, agrupado por categoría.</summary>
    [HttpGet("resumen")]
    [ProducesResponseType(typeof(ResumenPatrimonioResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<ResumenPatrimonioResponse>> Resumen()
    {
        if (!TryObtenerUsuario(out var usuarioId)) return Unauthorized();

        return Ok(await servicio.Resumen(usuarioId));
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ActivoResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ActivoResponse>> Buscar(Guid id)
    {
        if (!TryObtenerUsuario(out var usuarioId)) return Unauthorized();

        var resultado = await servicio.Buscar(id, usuarioId);

        return resultado.Exito ? Ok(resultado.Valor) : Traducir(resultado);
    }

    [HttpPost]
    [ProducesResponseType(typeof(ActivoResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ActivoResponse>> Crear(ActivoRequest solicitud)
    {
        if (!TryObtenerUsuario(out var usuarioId)) return Unauthorized();

        var resultado = await servicio.Crear(usuarioId, solicitud);

        return resultado.Exito
            ? CreatedAtAction(nameof(Buscar), new { id = resultado.Valor!.Id }, resultado.Valor)
            : Traducir(resultado);
    }

    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(ActivoResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ActivoResponse>> Actualizar(Guid id, ActivoRequest solicitud)
    {
        if (!TryObtenerUsuario(out var usuarioId)) return Unauthorized();

        var resultado = await servicio.Actualizar(id, usuarioId, solicitud);

        return resultado.Exito ? Ok(resultado.Valor) : Traducir(resultado);
    }

    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Eliminar(Guid id)
    {
        if (!TryObtenerUsuario(out var usuarioId)) return Unauthorized();

        var resultado = await servicio.Eliminar(id, usuarioId);

        return resultado.Exito ? NoContent() : Traducir(resultado);
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
            MotivoFallo.NoEncontrado => NotFound(cuerpo),
            _ => BadRequest(cuerpo)
        };
    }
}
