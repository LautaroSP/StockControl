using System.Data.Common;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace StockControl.Api.Datos;

/// <summary>Cada conexión que sale del pool recibe el local de la sesión. Al devolverse, queda vacía.</summary>
public class InterceptorAlcance(AlcanceDatos alcance, SesionActual sesion) : DbConnectionInterceptor
{
    public override void ConnectionOpened(DbConnection connection, ConnectionEndEventData eventData)
    {
        alcance.Escribir(connection, sesion, false, CancellationToken.None).GetAwaiter().GetResult();
        base.ConnectionOpened(connection, eventData);
    }

    public override async Task ConnectionOpenedAsync(DbConnection connection, ConnectionEndEventData eventData, CancellationToken cancellationToken = default)
    {
        await alcance.Escribir(connection, sesion, false, cancellationToken);
        await base.ConnectionOpenedAsync(connection, eventData, cancellationToken);
    }

    public override void ConnectionClosed(DbConnection connection, ConnectionEndEventData eventData)
    {
        Limpiar(connection);
        base.ConnectionClosed(connection, eventData);
    }

    public override async Task ConnectionClosedAsync(DbConnection connection, ConnectionEndEventData eventData)
    {
        Limpiar(connection);
        await base.ConnectionClosedAsync(connection, eventData);
    }

    private static void Limpiar(DbConnection connection)
    {
        try
        {
            using var cmd = connection.CreateCommand();
            cmd.CommandText = """
                SELECT set_config('app.alcance', '', false),
                       set_config('app.id_local', '', false),
                       set_config('app.id_usuario', '', false),
                       set_config('app.locales', '', false)
                """;
            cmd.ExecuteNonQuery();
        }
        catch (Exception)
        {
            // La conexión ya no acepta comandos. El pool igual la descarta si quedó rota.
        }
    }
}
