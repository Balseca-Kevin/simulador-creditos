using CreditService.Aplicacion.Contratos;
using CreditService.Aplicacion.Dtos;
using CreditService.Dominio;

namespace CreditService.Aplicacion.Servicios;

public interface IServicioSimulacion
{
    Task<IReadOnlyList<TipoCreditoResponse>> Tipos();
    Task<Resultado<SimulacionResponse>> Simular(Guid usuarioId, SimulacionRequest solicitud);
    Task<IReadOnlyList<SimulacionHistorialResponse>> Historial(Guid usuarioId);
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

    private static TipoCreditoResponse ProyectarTipo(TipoCredito tipo) => new()
    {
        Id = tipo.Id,
        Codigo = tipo.Codigo,
        Nombre = tipo.Nombre,
        TasaAnual = tipo.TasaAnual,
        TasaSeguroDesgravamenMensual = tipo.TasaSeguroDesgravamenMensual,
        Descripcion = tipo.Descripcion
    };
}
