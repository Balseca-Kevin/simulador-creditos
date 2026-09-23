namespace CreditService.Aplicacion.Contratos;

/// <summary>Simulación a la que da acceso un enlace de reporte.</summary>
public record VigenciaEnlace(Guid SimulacionId, Guid UsuarioId, string? Solicitante);

/// <summary>
/// Emite enlaces temporales para abrir un reporte en una pestaña nueva.
///
/// Hace falta porque el navegador, al abrir una pestaña, no puede enviar la
/// cabecera de autorización: navega "en limpio" y el servidor respondería 401.
/// La alternativa sería poner el token de sesión en la URL, pero las URL quedan
/// en el historial y en los registros del servidor, así que en su lugar se emite
/// un identificador de un solo uso, de vida corta y limitado a una simulación.
/// </summary>
public interface IEnlacesReporte
{
    /// <summary>Emite un identificador nuevo y devuelve cuánto tiempo será válido.</summary>
    (string Token, DateTime ExpiraEn) Emitir(VigenciaEnlace vigencia);

    /// <summary>Canjea el identificador. Devuelve falso si no existe, ya se usó o caducó.</summary>
    bool TryCanjear(string token, out VigenciaEnlace vigencia);
}
