using CreditService.Aplicacion.Contratos;
using CreditService.Aplicacion.Dtos;
using CreditService.Dominio;

namespace CreditService.Aplicacion.Servicios;

public interface IServicioSimulacion
{
    Task<IReadOnlyList<TipoCreditoResponse>> Tipos();
    Task<Resultado<SimulacionResponse>> Simular(Guid usuarioId, SimulacionRequest solicitud);
    Task<IReadOnlyList<SimulacionHistorialResponse>> Historial(Guid usuarioId);
    Task<Resultado<EstimacionesResponse>> Estimaciones(EstimacionesRequest solicitud);
}

/// <summary>
/// Casos de uso del simulador: consultar el catálogo, ejecutar una simulación y
/// listar el historial del usuario. Aquí vive la regla central del documento —el
/// tipo de crédito determina la tasa— y la orquestación entre el motor de
/// amortización y la persistencia.
/// </summary>
public class ServicioSimulacion(IRepositorioCreditos repositorio, IMotorAmortizacion motor)
    : IServicioSimulacion
{
    private const int TopeHistorial = 50;

    /// <summary>Montos y plazos que el formulario ofrece como sugerencia.</summary>
    private static readonly decimal[] MontosSugeridos = [1_000m, 5_000m, 10_000m, 25_000m, 50_000m, 100_000m];
    private static readonly int[] PlazosSugeridos = [6, 12, 24, 36, 48, 60, 120, 240];

    public async Task<IReadOnlyList<TipoCreditoResponse>> Tipos()
    {
        var tipos = await repositorio.ListarTiposActivos();
        return [.. tipos.Select(ProyectarTipo)];
    }

    public async Task<Resultado<SimulacionResponse>> Simular(Guid usuarioId, SimulacionRequest solicitud)
    {
        var tipo = await repositorio.BuscarTipoActivo(solicitud.TipoCreditoId);

        if (tipo is null)
        {
            return Resultado<SimulacionResponse>.Fallo(
                MotivoFallo.SolicitudInvalida,
                "El tipo de crédito seleccionado no existe o no está disponible.");
        }

        var mesesPorPeriodo = solicitud.FrecuenciaPago.MesesPorPeriodo(solicitud.PlazoMeses);

        if (solicitud.PlazoMeses % mesesPorPeriodo != 0)
        {
            return Resultado<SimulacionResponse>.Fallo(
                MotivoFallo.SolicitudInvalida,
                $"Con frecuencia {solicitud.FrecuenciaPago.Etiqueta().ToLowerInvariant()} " +
                $"el plazo debe ser múltiplo de {mesesPorPeriodo} meses.");
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

        await repositorio.GuardarSimulacion(simulacion);

        return Resultado<SimulacionResponse>.Ok(new SimulacionResponse
        {
            Id = simulacion.Id,
            FechaSimulacion = simulacion.FechaSimulacion,
            TipoCredito = ProyectarTipo(tipo),
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

    public async Task<IReadOnlyList<SimulacionHistorialResponse>> Historial(Guid usuarioId)
    {
        var simulaciones = await repositorio.UltimasSimulaciones(usuarioId, TopeHistorial);

        return [.. simulaciones.Select(s => new SimulacionHistorialResponse
        {
            Id = s.Id,
            FechaSimulacion = s.FechaSimulacion,
            TipoCredito = s.TipoCredito?.Nombre ?? string.Empty,
            Monto = s.Monto,
            PlazoMeses = s.PlazoMeses,
            FrecuenciaPago = s.FrecuenciaPago,
            IncluyeSeguroDesgravamen = s.IncluyeSeguroDesgravamen,
            TasaAnualAplicada = s.TasaAnualAplicada,
            CuotaFija = s.CuotaFija,
            TotalInteresFrances = s.TotalInteresFrances,
            TotalInteresAleman = s.TotalInteresAleman,
            IngresoMinimoRequerido = s.IngresoMinimoRequerido
        })];
    }

    /// <summary>
    /// Calcula, para cada opción de los desplegables, qué cuota resultaría si el
    /// usuario la eligiera dejando fijo el resto del formulario.
    ///
    /// Se resuelve en el servidor a propósito: reutiliza el mismo motor que la
    /// simulación final, de modo que la cifra que se anticipa en la lista no
    /// puede diferir de la que se obtiene al calcular. Replicarlo en el
    /// navegador habría duplicado la matemática del dinero en dos lenguajes.
    /// </summary>
    public async Task<Resultado<EstimacionesResponse>> Estimaciones(EstimacionesRequest solicitud)
    {
        var tipos = await repositorio.ListarTiposActivos();
        var tipoActual = tipos.FirstOrDefault(t => t.Id == solicitud.TipoCreditoId) ?? tipos.FirstOrDefault();

        if (tipoActual is null)
        {
            return Resultado<EstimacionesResponse>.Fallo(
                MotivoFallo.SolicitudInvalida,
                "No hay tipos de crédito disponibles.");
        }

        var frecuencia = solicitud.FrecuenciaPago;

        // Se muestra la cuota del método francés: es la fija, y por tanto la
        // única que se puede resumir en un solo número.
        decimal? CuotaDe(TipoCredito tipo, decimal monto, int plazo)
        {
            if (plazo <= 0 || monto <= 0) return null;
            if (plazo % frecuencia.MesesPorPeriodo(plazo) != 0) return null;

            var tabla = motor.CalcularFrances(new ParametrosCredito(
                monto, tipo.TasaAnual, plazo, frecuencia,
                solicitud.IncluirSeguroDesgravamen ? tipo.TasaSeguroDesgravamenMensual : 0m));

            return tabla.PrimeraCuotaTotal;
        }

        var porTipo = tipos.Select(t =>
        {
            var tabla = solicitud.PlazoMeses % frecuencia.MesesPorPeriodo(solicitud.PlazoMeses) == 0
                ? motor.CalcularFrances(new ParametrosCredito(
                    solicitud.Monto, t.TasaAnual, solicitud.PlazoMeses, frecuencia,
                    solicitud.IncluirSeguroDesgravamen ? t.TasaSeguroDesgravamenMensual : 0m))
                : null;

            return new EstimacionTipo
            {
                TipoCreditoId = t.Id,
                Nombre = t.Nombre,
                Categoria = t.Categoria,
                Descripcion = t.Descripcion,
                TasaAnual = t.TasaAnual,
                CuotaEstimada = tabla?.PrimeraCuotaTotal,
                IngresoMinimoRequerido = tabla?.IngresoMinimoRequerido
            };
        }).ToList();

        // Al monto y al plazo escritos por el usuario se les suman los sugeridos,
        // para que su valor propio también aparezca en la lista con su estimación.
        var montos = MontosSugeridos.Append(solicitud.Monto).Distinct().OrderBy(m => m);
        var porMonto = montos
            .Select(m => new EstimacionMonto { Monto = m, CuotaEstimada = CuotaDe(tipoActual, m, solicitud.PlazoMeses) })
            .ToList();

        var plazos = PlazosSugeridos.Append(solicitud.PlazoMeses).Distinct().OrderBy(p => p);
        var porPlazo = plazos
            .Select(p => new EstimacionPlazo
            {
                PlazoMeses = p,
                CompatibleConFrecuencia = p % frecuencia.MesesPorPeriodo(p) == 0,
                CuotaEstimada = CuotaDe(tipoActual, solicitud.Monto, p)
            })
            .ToList();

        return Resultado<EstimacionesResponse>.Ok(new EstimacionesResponse
        {
            PorTipo = porTipo,
            PorMonto = porMonto,
            PorPlazo = porPlazo,
            Metodo = "Frances"
        });
    }

    private static TipoCreditoResponse ProyectarTipo(TipoCredito tipo) => new()
    {
        Id = tipo.Id,
        Codigo = tipo.Codigo,
        Nombre = tipo.Nombre,
        Categoria = tipo.Categoria,
        TasaAnual = tipo.TasaAnual,
        TasaSeguroDesgravamenMensual = tipo.TasaSeguroDesgravamenMensual,
        Descripcion = tipo.Descripcion
    };
}
