namespace StockControl.Dominio;

public class LecturaEscritorio
{
    public List<GrupoProductos> Grupos { get; } = new();
    public List<Producto> Productos { get; } = new();
    public List<MetodoPago> Metodos { get; } = new();
    public List<InformeVenta> Ventas { get; } = new();
    public List<InformeVentaDetalle> Detalles { get; } = new();
    public List<Caja> Cajas { get; } = new();
    public List<Configuracion> Configuraciones { get; } = new();

    public void AsignarLocal(int idLocal)
    {
        foreach (var g in Grupos) g.IdLocal = idLocal;
        foreach (var p in Productos) p.IdLocal = idLocal;
        foreach (var m in Metodos) m.IdLocal = idLocal;
        foreach (var v in Ventas) v.IdLocal = idLocal;
        foreach (var d in Detalles) d.IdLocal = idLocal;
        foreach (var caja in Cajas) caja.IdLocal = idLocal;
        foreach (var c in Configuraciones) c.IdLocal = idLocal;
        PasarFechasAUtc();
    }

    public void PasarFechasAUtc()
    {
        foreach (var p in Productos)
        {
            if (p.FechaModificacion.HasValue)
                p.FechaModificacion = p.FechaModificacion.Value.ToUniversalTime();
        }

        foreach (var v in Ventas)
            v.Fecha = v.Fecha.ToUniversalTime();

        foreach (var caja in Cajas)
            caja.Fecha = caja.Fecha.ToUniversalTime();
    }

    public void ValidarCodigosUnicos()
    {
        var dup = Productos.GroupBy(p => p.Codigo, StringComparer.OrdinalIgnoreCase)
            .FirstOrDefault(g => g.Count() > 1);
        if (dup != null)
            throw new ErrorNegocio($"Código duplicado en el .db: {dup.Key}");
    }
}
