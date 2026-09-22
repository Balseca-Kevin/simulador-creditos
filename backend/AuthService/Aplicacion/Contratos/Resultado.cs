namespace AuthService.Aplicacion.Contratos;

/// <summary>
/// Motivo por el que una operación no pudo completarse. La capa de aplicación
/// razona en estos términos y es la de presentación quien los traduce a
/// códigos HTTP: así el caso de uso no depende del protocolo por el que se expone.
/// </summary>
public enum MotivoFallo
{
    Ninguno,
    Conflicto,
    NoAutorizado
}

/// <summary>Resultado de un caso de uso: el valor obtenido o el motivo del fallo.</summary>
public record Resultado<T>
{
    public required bool Exito { get; init; }
    public T? Valor { get; init; }
    public MotivoFallo Motivo { get; init; } = MotivoFallo.Ninguno;
    public string? Mensaje { get; init; }

    public static Resultado<T> Ok(T valor) => new() { Exito = true, Valor = valor };

    public static Resultado<T> Fallo(MotivoFallo motivo, string mensaje) =>
        new() { Exito = false, Motivo = motivo, Mensaje = mensaje };
}
