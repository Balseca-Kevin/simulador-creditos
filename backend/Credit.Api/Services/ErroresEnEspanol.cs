using Microsoft.AspNetCore.Mvc;

namespace Credit.Api.Services;

/// <summary>
/// Reescribe las respuestas 400 de validación automática.
///
/// Cuando el JSON no se puede convertir (por ejemplo, una frecuencia inexistente),
/// ASP.NET responde en inglés técnico e incluye el nombre interno del tipo
/// ("Credit.Api.Models.FrecuenciaPago"), además de un error redundante sobre el
/// parámetro completo. Ese texto llegaría tal cual a la pantalla del usuario y
/// expone detalles de implementación. Aquí se sustituye por mensajes en español
/// y se conserva el formato estándar, que el frontend ya sabe interpretar.
/// </summary>
public static class ErroresEnEspanol
{
    private static readonly Dictionary<string, string> NombresDeCampo = new(StringComparer.OrdinalIgnoreCase)
    {
        ["tipoCreditoId"] = "el tipo de crédito",
        ["monto"] = "el monto",
        ["plazoMeses"] = "el plazo",
        ["frecuenciaPago"] = "la frecuencia de pago",
        ["incluirSeguroDesgravamen"] = "el seguro de desgravamen",
    };

    public static IActionResult Respuesta(ActionContext contexto)
    {
        var errores = new Dictionary<string, string[]>();

        foreach (var (clave, entrada) in contexto.ModelState)
        {
            if (entrada.Errors.Count == 0) continue;

            var campo = clave.TrimStart('$', '.');

            // Error del parámetro completo: aparece solo como eco de un fallo de
            // conversión en uno de sus campos, que ya se informa por separado.
            if (campo.Equals("solicitud", StringComparison.OrdinalIgnoreCase)) continue;

            errores[campo] = entrada.Errors
                .Select(error => EsErrorDeConversion(error) ? MensajeDeConversion(campo) : error.ErrorMessage)
                .Distinct()
                .ToArray();
        }

        if (errores.Count == 0)
        {
            errores[""] = ["La solicitud no tiene un formato válido."];
        }

        return new BadRequestObjectResult(new ValidationProblemDetails(errores)
        {
            Title = "Los datos enviados no son válidos.",
            Status = StatusCodes.Status400BadRequest
        });
    }

    private static bool EsErrorDeConversion(Microsoft.AspNetCore.Mvc.ModelBinding.ModelError error) =>
        error.Exception is not null
        || error.ErrorMessage.Contains("could not be converted", StringComparison.OrdinalIgnoreCase)
        || error.ErrorMessage.Contains("JSON", StringComparison.Ordinal);

    private static string MensajeDeConversion(string campo) =>
        NombresDeCampo.TryGetValue(campo, out var nombre)
            ? $"El valor enviado para {nombre} no es válido."
            : "Uno de los valores enviados no tiene el formato esperado.";
}
