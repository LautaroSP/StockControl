using StockControl.Dominio;

namespace StockControl.Tests;

/// <summary>ARCA de mentira: numera, aprueba y, si se le pide, rechaza o pierde la respuesta. No toca la red.</summary>
public class ClienteArcaFalso : IClienteArca
{
    public Dictionary<(int Tipo, long Numero), RespuestaArca> Emitidos { get; } = new();
    public List<SolicitudArca> Solicitudes { get; } = new();
    public bool Rechazar { get; set; }
    public bool PerderRespuestaUnaVez { get; set; }
    public bool ConsultaCaida { get; set; }
    public bool Caido { get; set; }

    public Task<long> UltimoAutorizado(string cuit, int puntoVenta, int tipo, CancellationToken ct = default)
    {
        if (Caido) throw new ExcepcionArca("ARCA caída", transitoria: true);
        var ultimo = Emitidos.Keys.Where(k => k.Tipo == tipo).Select(k => k.Numero).DefaultIfEmpty(0).Max();
        return Task.FromResult(ultimo);
    }

    public Task<RespuestaArca> Solicitar(SolicitudArca s, CancellationToken ct = default)
    {
        Solicitudes.Add(s);
        if (Rechazar)
            return Task.FromResult(new RespuestaArca { Aprobado = false, Observacion = "Rechazado por ARCA" });
        var r = new RespuestaArca
        {
            Aprobado = true,
            Numero = s.Numero,
            Cae = "70123456789012",
            VencimientoCae = s.Fecha.AddDays(10)
        };
        Emitidos[(s.Tipo, s.Numero)] = r;
        if (PerderRespuestaUnaVez)
        {
            PerderRespuestaUnaVez = false;
            throw new ExcepcionArca("timeout", transitoria: true);
        }
        return Task.FromResult(r);
    }

    public Task<RespuestaArca?> Consultar(string cuit, int puntoVenta, int tipo, long numero, CancellationToken ct = default)
    {
        if (ConsultaCaida) throw new ExcepcionArca("ARCA caída", transitoria: true);
        return Task.FromResult(Emitidos.TryGetValue((tipo, numero), out var r) ? r : null);
    }
}
