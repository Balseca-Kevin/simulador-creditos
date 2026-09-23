using CreditService.Aplicacion.Dtos;

namespace CreditService.Aplicacion.Contratos;

/// <summary>
/// Convierte una simulación en un documento PDF. La interfaz vive aquí y la
/// implementación en Estructura: la capa de aplicación no necesita conocer la
/// librería de maquetación, igual que no conoce EF Core.
/// </summary>
public interface IGeneradorReportePdf
{
    byte[] Generar(SimulacionResponse simulacion, string? solicitante);
}
