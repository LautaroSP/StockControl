using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http.Metadata;
using Microsoft.Extensions.Options;

namespace StockControl.Api;

/// <summary>
/// En .NET 8 las DataAnnotations de un minimal API no corren solas. Este paso las aplica al cuerpo JSON.
/// </summary>
public static class FiltroValidacion
{
    public static async Task Invocar(HttpContext ctx, Func<Task> siguiente)
    {
        var tipo = ctx.GetEndpoint()?.Metadata.GetMetadata<IAcceptsMetadata>()?.RequestType;
        if (tipo == null || !EsComplejo(tipo) || !EsJson(ctx.Request.ContentType))
        {
            await siguiente();
            return;
        }

        ctx.Request.EnableBuffering();
        if (ctx.Request.ContentLength is 0)
        {
            await siguiente();
            return;
        }

        var opciones = ctx.RequestServices.GetRequiredService<IOptions<Microsoft.AspNetCore.Http.Json.JsonOptions>>().Value.SerializerOptions;
        object? cuerpo;
        try
        {
            cuerpo = await System.Text.Json.JsonSerializer.DeserializeAsync(ctx.Request.Body, tipo, opciones, ctx.RequestAborted);
        }
        catch (System.Text.Json.JsonException)
        {
            ctx.Response.StatusCode = StatusCodes.Status400BadRequest;
            await ctx.Response.WriteAsync("El cuerpo no es válido.");
            return;
        }
        finally
        {
            ctx.Request.Body.Position = 0;
        }

        if (cuerpo != null)
        {
            var errores = new List<ValidationResult>();
            Validar(cuerpo, errores);
            if (errores.Count > 0)
            {
                ctx.Response.StatusCode = StatusCodes.Status400BadRequest;
                ctx.Response.ContentType = "text/plain; charset=utf-8";
                await ctx.Response.WriteAsync(Mensaje(errores[0]));
                return;
            }
        }

        await siguiente();
    }

    private static void Validar(object cuerpo, List<ValidationResult> errores)
    {
        Validator.TryValidateObject(cuerpo, new ValidationContext(cuerpo), errores, validateAllProperties: true);
        foreach (var propiedad in cuerpo.GetType().GetProperties())
        {
            if (propiedad.GetValue(cuerpo) is not System.Collections.IEnumerable lista || propiedad.PropertyType == typeof(string))
                continue;
            foreach (var item in lista)
            {
                if (item != null && EsComplejo(item.GetType()))
                    Validator.TryValidateObject(item, new ValidationContext(item), errores, validateAllProperties: true);
            }
        }
    }

    private static string Mensaje(ValidationResult resultado)
    {
        var texto = resultado.ErrorMessage ?? "";
        if (texto.Length > 0 && !texto.StartsWith("The ") && !texto.StartsWith("El campo"))
            return texto;
        return "Revisá los datos enviados.";
    }

    private static bool EsJson(string? tipo) =>
        tipo != null && tipo.Contains("json", StringComparison.OrdinalIgnoreCase);

    private static bool EsComplejo(Type tipo)
    {
        tipo = Nullable.GetUnderlyingType(tipo) ?? tipo;
        return tipo != typeof(string)
            && !tipo.IsPrimitive
            && tipo != typeof(decimal)
            && tipo != typeof(DateTime)
            && tipo != typeof(DateTimeOffset)
            && tipo != typeof(DateOnly)
            && tipo != typeof(Guid)
            && !tipo.IsEnum;
    }
}
