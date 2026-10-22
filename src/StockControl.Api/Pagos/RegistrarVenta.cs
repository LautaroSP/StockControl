using Microsoft.EntityFrameworkCore;
using StockControl.Api.Datos;
using StockControl.Dominio;

namespace StockControl.Api.Pagos;

public static class RegistrarVenta
{
    public static async Task<InformeVenta> Guardar(
        AppDbContext db,
        int idLocal,
        int? idUsuario,
        int nroCaja,
        ResultadoCobro resultado,
        IReadOnlyList<PagoSolicitado> pagos)
    {
        var ids = pagos.Select(p => p.IdMetodoPago).Distinct().ToList();
        var medios = await db.MetodosPago.IgnoreQueryFilters()
            .Where(m => m.IdLocal == idLocal && ids.Contains(m.Id) && m.Activo)
            .ToDictionaryAsync(m => m.Id);
        if (medios.Count != ids.Count)
            throw new ErrorNegocio("El medio de pago no existe o está inactivo.");

        var venta = new InformeVenta
        {
            IdLocal = idLocal,
            IdUsuario = idUsuario,
            Fecha = DateTimeOffset.UtcNow,
            Total = resultado.Total,
            Subtotal = resultado.Subtotal,
            MetodoPago = string.Join(", ", pagos.Select(p => medios[p.IdMetodoPago].Descripcion).Distinct()),
            MultipleMetodoDePago = pagos.Select(p => p.IdMetodoPago).Distinct().Count() > 1,
            Descuento = resultado.Descuento,
            PrecioCosto = resultado.PrecioCosto,
            DetalleAdjunto = true,
            NroCaja = nroCaja
        };
        foreach (var l in resultado.Lineas)
        {
            venta.Detalles.Add(new InformeVentaDetalle
            {
                IdLocal = idLocal,
                IdProducto = l.IdProducto == 0 ? null : l.IdProducto,
                Codigo = l.Codigo,
                Nombre = l.Nombre,
                Cantidad = l.Cantidad,
                Precio = l.Precio,
                Costo = l.Costo,
                SubTotal = l.SubTotal
            });
        }
        venta.Pagos.AddRange(pagos.Select(p => new PagoVenta
        {
            IdLocal = idLocal,
            IdMetodoPago = p.IdMetodoPago,
            DescripcionMetodoPago = medios[p.IdMetodoPago].Descripcion,
            Importe = p.Importe
        }));
        db.InformeVenta.Add(venta);
        return venta;
    }
}
