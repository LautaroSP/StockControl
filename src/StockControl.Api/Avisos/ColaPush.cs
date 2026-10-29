using System.Threading.Channels;

namespace StockControl.Api.Avisos;

public record MensajePush(int IdLocal, IReadOnlyList<int> IdsUsuario, string Titulo, string Texto, string Url);

/// <summary>El pedido responde sin esperar a los servicios de push: el envío sale en segundo plano.</summary>
public class ColaPush
{
    private readonly Channel<MensajePush> _canal = Channel.CreateBounded<MensajePush>(
        new BoundedChannelOptions(500) { FullMode = BoundedChannelFullMode.DropOldest, SingleReader = true });

    public void Encolar(MensajePush mensaje)
    {
        if (mensaje.IdsUsuario.Count > 0)
            _canal.Writer.TryWrite(mensaje);
    }

    public IAsyncEnumerable<MensajePush> Leer(CancellationToken cancelar) => _canal.Reader.ReadAllAsync(cancelar);
}
