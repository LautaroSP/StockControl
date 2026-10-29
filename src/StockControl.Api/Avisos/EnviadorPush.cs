using System.Net;
using System.Text.Json;
using Lib.Net.Http.WebPush;
using Lib.Net.Http.WebPush.Authentication;
using Microsoft.EntityFrameworkCore;
using StockControl.Api.Datos;

namespace StockControl.Api.Avisos;

/// <summary>Manda los Web Push de la cola. Sin claves VAPID en el entorno, descarta los mensajes.</summary>
public class EnviadorPush(
    ColaPush cola,
    IServiceScopeFactory scopes,
    IHttpClientFactory clientes,
    IConfiguration config,
    ILogger<EnviadorPush> log) : BackgroundService
{
    public const string Cliente = "push";

    protected override async Task ExecuteAsync(CancellationToken cancelar)
    {
        var vapid = ClavesVapid.Autenticacion(config);
        if (vapid == null)
            log.LogInformation("Web Push apagado: faltan WebPush:ClavePublica, WebPush:ClavePrivada o WebPush:Asunto.");
        await foreach (var mensaje in cola.Leer(cancelar))
        {
            if (vapid == null) continue;
            try
            {
                await Enviar(mensaje, vapid, cancelar);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                log.LogWarning(ex, "Falló el envío de avisos push del local {IdLocal}", mensaje.IdLocal);
            }
        }
    }

    private async Task Enviar(MensajePush mensaje, VapidAuthentication vapid, CancellationToken cancelar)
    {
        using var scope = scopes.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var suscripciones = await db.SuscripcionesPush.IgnoreQueryFilters()
            .Where(s => s.IdLocal == mensaje.IdLocal && mensaje.IdsUsuario.Contains(s.IdUsuario))
            .ToListAsync(cancelar);
        var cliente = new PushServiceClient(clientes.CreateClient(Cliente)) { DefaultAuthentication = vapid };
        var contenido = JsonSerializer.Serialize(new { titulo = mensaje.Titulo, texto = mensaje.Texto, url = mensaje.Url });
        foreach (var suscripcion in suscripciones)
        {
            var destino = new PushSubscription
            {
                Endpoint = suscripcion.Endpoint,
                Keys = new Dictionary<string, string> { ["p256dh"] = suscripcion.P256dh, ["auth"] = suscripcion.Auth }
            };
            try
            {
                await cliente.RequestPushMessageDeliveryAsync(
                    destino, new PushMessage(contenido) { TimeToLive = 3600, Urgency = PushMessageUrgency.High }, cancelar);
            }
            catch (PushServiceClientException ex) when (ex.StatusCode is HttpStatusCode.NotFound or HttpStatusCode.Gone)
            {
                db.SuscripcionesPush.Remove(suscripcion);
            }
            catch (Exception ex) when (ex is PushServiceClientException or HttpRequestException or TaskCanceledException && !cancelar.IsCancellationRequested)
            {
                log.LogWarning(ex, "El servicio de push rechazó un aviso");
            }
        }
        await db.SaveChangesAsync(cancelar);
    }
}
