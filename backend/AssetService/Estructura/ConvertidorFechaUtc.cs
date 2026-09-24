using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace AssetService.Estructura;

/// <summary>
/// Guarda las fechas en UTC y las devuelve marcadas como UTC.
///
/// Hace falta por una diferencia entre motores: PostgreSQL tiene un tipo con
/// zona horaria y devolvía las fechas ya marcadas, pero `datetime2` de SQL
/// Server no guarda zona y las devuelve sin marca. Sin este convertidor, el
/// serializador JSON escribiría la fecha sin la "Z" final, el navegador la
/// interpretaría como hora local y el historial y el reporte mostrarían horas
/// corridas varias horas. El fallo sería silencioso: nada falla, solo miente.
/// </summary>
public class ConvertidorFechaUtc() : ValueConverter<DateTime, DateTime>(
    fecha => fecha.ToUniversalTime(),
    fecha => DateTime.SpecifyKind(fecha, DateTimeKind.Utc));

/// <summary>Misma conversión para las fechas opcionales.</summary>
public class ConvertidorFechaUtcOpcional() : ValueConverter<DateTime?, DateTime?>(
    fecha => fecha.HasValue ? fecha.Value.ToUniversalTime() : fecha,
    fecha => fecha.HasValue ? DateTime.SpecifyKind(fecha.Value, DateTimeKind.Utc) : fecha);
