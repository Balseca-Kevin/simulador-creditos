using AssetService.Aplicacion.Dtos;
using AssetService.Aplicacion.Servicios;
using Microsoft.AspNetCore.Authorization;

namespace AssetService.Presentacion;

/// <summary>
/// Catálogo de categorías de activo. Es de solo lectura: las categorías se
/// administran por migración, igual que las tasas del servicio de créditos.
/// </summary>
[ApiController]
[Route("api/categorias")]
[Authorize]
public class CategoriasController(IServicioActivos servicio) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<CategoriaActivoResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<IEnumerable<CategoriaActivoResponse>>> Listar() =>
        Ok(await servicio.Categorias());
}
