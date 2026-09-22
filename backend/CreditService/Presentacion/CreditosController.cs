using System.Security.Claims;
using CreditService.Estructura;
using CreditService.Aplicacion.Dtos;
using CreditService.Dominio;
using CreditService.Aplicacion.Servicios;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CreditService.Presentacion;

/// <summary>
/// Todos los endpoints exigen un JWT válido emitido por la Auth API.
/// La identidad del solicitante se toma del token, nunca del cuerpo de la petición:
/// así un usuario no puede consultar ni escribir el historial de otro.
/// </summary>
[ApiController]
[Route("api/creditos")]
[Authorize]
public class CreditosController(CreditDbContext contexto, IMotorAmortizacion motor) : ControllerBase
{
    /// <summary>HU-04: catálogo de tipos de crédito con su tasa referencial vigente.</summary>
    [HttpGet("tipos")]
    [ProducesResponseType(typeof(IEnumerable<TipoCreditoResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<TipoCreditoResponse>>> Tipos()
    {
        var tipos = await contexto.TiposCredito
            .AsNoTracking()
            .Where(t => t.Activo)
            .OrderBy(t => t.Id)
            .Select(t => new TipoCreditoResponse
            {
                Id = t.Id,
                Codigo = t.Codigo,
                Nombre = t.Nombre,
                TasaAnual = t.TasaAnual,
                TasaSeguroDesgravamenMensual = t.TasaSeguroDesgravamenMensual,
                Descripcion = t.Descripcion
            })
            .ToListAsync();

        return Ok(tipos);
    }

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

        var tipo = await contexto.TiposCredito
            .AsNoTracking()
            .SingleOrDefaultAsync(t => t.Id == solicitud.TipoCreditoId && t.Activo);

        if (tipo is null)
        {
            return BadRequest(new { mensaje = "El tipo de crédito seleccionado no existe o no está disponible." });
        }

        var mesesPorPeriodo = solicitud.FrecuenciaPago.MesesPorPeriodo(solicitud.PlazoMeses);

        if (solicitud.PlazoMeses % mesesPorPeriodo != 0)
        {
            return BadRequest(new
            {
                mensaje = $"Con frecuencia {solicitud.FrecuenciaPago.Etiqueta().ToLowerInvariant()} " +
                          $"el plazo debe ser múltiplo de {mesesPorPeriodo} meses."
            });
        }

        // Regla de negocio central: las tasas provienen del tipo elegido, nunca del cliente.
        var parametros = new ParametrosCredito(
            Monto: solicitud.Monto,
            TasaAnual: tipo.TasaAnual,
            PlazoMeses: solicitud.PlazoMeses,
            Frecuencia: solicitud.FrecuenciaPago,
            TasaSeguroMensual: solicitud.IncluirSeguroDesgravamen ? tipo.TasaSeguroDesgravamenMensual : 0m);

        var francesa = motor.CalcularFrances(parametros);
        var alemana = motor.CalcularAleman(parametros);

        var simulacion = new Simulacion
        {
            UsuarioId = usuarioId,
            TipoCreditoId = tipo.Id,
            Monto = solicitud.Monto,
            PlazoMeses = solicitud.PlazoMeses,
            FrecuenciaPago = solicitud.FrecuenciaPago,
            IncluyeSeguroDesgravamen = solicitud.IncluirSeguroDesgravamen,
            TasaAnualAplicada = tipo.TasaAnual,
            CuotaFija = francesa.PrimeraCuota,
            TotalInteresFrances = francesa.TotalInteres,
            TotalInteresAleman = alemana.TotalInteres,
            IngresoMinimoRequerido = francesa.IngresoMinimoRequerido
        };

        contexto.Simulaciones.Add(simulacion);
        await contexto.SaveChangesAsync();

        return Ok(new SimulacionResponse
        {
            Id = simulacion.Id,
            FechaSimulacion = simulacion.FechaSimulacion,
            TipoCredito = new TipoCreditoResponse
            {
                Id = tipo.Id,
                Codigo = tipo.Codigo,
                Nombre = tipo.Nombre,
                TasaAnual = tipo.TasaAnual,
                TasaSeguroDesgravamenMensual = tipo.TasaSeguroDesgravamenMensual,
                Descripcion = tipo.Descripcion
            },
            Monto = solicitud.Monto,
            PlazoMeses = solicitud.PlazoMeses,
            FrecuenciaPago = solicitud.FrecuenciaPago,
            NumeroCuotas = francesa.Cuotas.Count,
            MesesPorPeriodo = mesesPorPeriodo,
            IncluyeSeguroDesgravamen = solicitud.IncluirSeguroDesgravamen,
            TasaAnualAplicada = tipo.TasaAnual,
            TasaPeriodicaAplicada = Math.Round(MotorAmortizacion.TasaPeriodica(tipo.TasaAnual, mesesPorPeriodo), 8),
            RelacionCuotaIngreso = MotorAmortizacion.RelacionCuotaIngreso,
            Francesa = francesa,
            Alemana = alemana,
            Comparativo = new ComparativoMetodos
            {
                MetodoMasEconomico = alemana.TotalInteres <= francesa.TotalInteres ? "Aleman" : "Frances",
                DiferenciaTotalInteres = Math.Abs(francesa.TotalInteres - alemana.TotalInteres)
            }
        });
    }

    /// <summary>Historial de simulaciones del usuario autenticado, de la más reciente a la más antigua.</summary>
    [HttpGet("historial")]
    [ProducesResponseType(typeof(IEnumerable<SimulacionHistorialResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<IEnumerable<SimulacionHistorialResponse>>> Historial()
    {
        if (!TryObtenerUsuario(out var usuarioId)) return Unauthorized();

        var historial = await contexto.Simulaciones
            .AsNoTracking()
            .Where(s => s.UsuarioId == usuarioId)
            .OrderByDescending(s => s.FechaSimulacion)
            .Take(50)
            .Select(s => new SimulacionHistorialResponse
            {
                Id = s.Id,
                FechaSimulacion = s.FechaSimulacion,
                TipoCredito = s.TipoCredito!.Nombre,
                Monto = s.Monto,
                PlazoMeses = s.PlazoMeses,
                FrecuenciaPago = s.FrecuenciaPago,
                IncluyeSeguroDesgravamen = s.IncluyeSeguroDesgravamen,
                TasaAnualAplicada = s.TasaAnualAplicada,
                CuotaFija = s.CuotaFija,
                TotalInteresFrances = s.TotalInteresFrances,
                TotalInteresAleman = s.TotalInteresAleman,
                IngresoMinimoRequerido = s.IngresoMinimoRequerido
            })
            .ToListAsync();

        return Ok(historial);
    }

    private bool TryObtenerUsuario(out Guid usuarioId)
    {
        var idTexto = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub");
        return Guid.TryParse(idTexto, out usuarioId);
    }
}
