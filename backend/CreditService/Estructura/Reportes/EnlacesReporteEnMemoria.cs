using System.Security.Cryptography;
using CreditService.Aplicacion.Contratos;
using Microsoft.Extensions.Caching.Memory;

namespace CreditService.Estructura.Reportes;

/// <summary>
/// Guarda los enlaces de reporte en memoria, con caducidad automática.
///
/// En memoria basta porque el enlace vive dos minutos y se usa una sola vez,
/// justo entre que el usuario pulsa el botón y el navegador abre la pestaña.
/// Si el servicio se reiniciara en ese intervalo, el peor caso es que el
/// usuario vuelva a pulsar el botón.
///
/// Con varias instancias del servicio detrás de un balanceador esto dejaría de
/// funcionar, porque la pestaña podría caer en otra instancia; entonces habría
/// que moverlo a un almacén compartido. Se deja documentado aquí para que el
/// día que ocurra no haya que deducirlo.
/// </summary>
public class EnlacesReporteEnMemoria(IMemoryCache cache) : IEnlacesReporte
{
    public static readonly TimeSpan Vigencia = TimeSpan.FromMinutes(2);

    public (string Token, DateTime ExpiraEn) Emitir(VigenciaEnlace vigencia)
    {
        // Aleatorio y suficientemente largo para que no se pueda adivinar
        // el enlace de otro usuario durante su corta vida.
        var token = Convert.ToHexString(RandomNumberGenerator.GetBytes(32)).ToLowerInvariant();

        cache.Set(Clave(token), vigencia, Vigencia);

        return (token, DateTime.UtcNow.Add(Vigencia));
    }

    public bool TryCanjear(string token, out VigenciaEnlace vigencia)
    {
        vigencia = default!;

        if (string.IsNullOrWhiteSpace(token)) return false;
        if (!cache.TryGetValue(Clave(token), out VigenciaEnlace? guardada) || guardada is null) return false;

        // De un solo uso: se retira en cuanto se canjea, para que el enlace no
        // siga sirviendo si queda en el historial del navegador.
        cache.Remove(Clave(token));

        vigencia = guardada;
        return true;
    }

    private static string Clave(string token) => $"reporte:{token}";
}
