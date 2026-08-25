using System.Data;
using Microsoft.Data.Sqlite;
using StockControl.Dominio;

namespace StockControl.Api.Importacion;

public static class LectorSqliteEscritorio
{
    public static LecturaEscritorio Leer(string ruta)
    {
        if (!File.Exists(ruta))
            throw new ErrorNegocio($"No existe el archivo: {ruta}");

        var lectura = new LecturaEscritorio();
        using var con = new SqliteConnection($"Data Source={ruta};Mode=ReadOnly;Pooling=False");
        con.Open();

        lectura.Grupos.AddRange(LeerGrupos(con));
        lectura.Productos.AddRange(LeerProductos(con));
        lectura.Metodos.AddRange(LeerMetodos(con));
        lectura.Ventas.AddRange(LeerVentas(con));
        lectura.Detalles.AddRange(LeerDetalles(con));
        lectura.Configuraciones.AddRange(LeerConfig(con));
        lectura.ValidarCodigosUnicos();
        return lectura;
    }

    private static List<GrupoProductos> LeerGrupos(SqliteConnection con)
    {
        using var cmd = con.CreateCommand();
        cmd.CommandText = "SELECT IdGrupoProducto, NombreGrupo, PrecioGrupo, IFNULL(Costo,0), IFNULL(Ganancia,0), IFNULL(GananciaIndividual,0) FROM GrupoProductos";
        try
        {
            using var r = cmd.ExecuteReader();
            var list = new List<GrupoProductos>();
            while (r.Read())
            {
                list.Add(new GrupoProductos
                {
                    IdGrupoProducto = r.GetInt32(0),
                    NombreGrupo = r.GetString(1),
                    PrecioGrupo = Convert.ToDecimal(r.GetValue(2)),
                    Costo = Convert.ToDecimal(r.GetValue(3)),
                    Ganancia = Convert.ToDecimal(r.GetValue(4)),
                    GananciaIndividual = Convert.ToInt32(r.GetValue(5)) == 1
                });
            }
            return list;
        }
        catch (SqliteException)
        {
            return [];
        }
    }

    private static List<Producto> LeerProductos(SqliteConnection con)
    {
        using var cmd = con.CreateCommand();
        cmd.CommandText = @"SELECT Id, Codigo, Nombre, Cantidad, IFNULL(Costo,0), Precio,
            IFNULL(ProductoSector,0), IFNULL(IdGrupoProducto,0), IFNULL(GananciaIndividual,0),
            IFNULL(ValorGanancia,0), FechaModificacion FROM Productos";
        using var r = cmd.ExecuteReader();
        var list = new List<Producto>();
        while (r.Read())
        {
            list.Add(new Producto
            {
                Id = r.GetInt32(0),
                Codigo = r.GetString(1),
                Nombre = r.GetString(2),
                Cantidad = Convert.ToDecimal(r.GetValue(3)),
                Costo = Convert.ToDecimal(r.GetValue(4)),
                Precio = Convert.ToDecimal(r.GetValue(5)),
                ProductoSector = Convert.ToInt32(r.GetValue(6)) == 1,
                IdGrupoProducto = Convert.ToInt32(r.GetValue(7)),
                GananciaIndividual = Convert.ToInt32(r.GetValue(8)) == 1,
                ValorGanancia = Convert.ToDecimal(r.GetValue(9)),
                FechaModificacion = ParseFecha(r.IsDBNull(10) ? null : r.GetValue(10)?.ToString())
            });
        }
        return list;
    }

    private static List<MetodoPago> LeerMetodos(SqliteConnection con)
    {
        try
        {
            using var cmd = con.CreateCommand();
            cmd.CommandText = "SELECT Id, Descripcion FROM MetodosPago";
            using var r = cmd.ExecuteReader();
            var list = new List<MetodoPago>();
            while (r.Read())
                list.Add(new MetodoPago { Id = r.GetInt32(0), Descripcion = r.GetString(1), Activo = true });
            return list;
        }
        catch (SqliteException)
        {
            return [];
        }
    }

    private static List<InformeVenta> LeerVentas(SqliteConnection con)
    {
        try
        {
            using var cmd = con.CreateCommand();
            cmd.CommandText = @"SELECT IdInformeVenta, Fecha, Total, IFNULL(MetodoPago,''),
                IFNULL(MultipleMetodoDePago,0), IFNULL(DetalleAdjunto,0), IFNULL(Descuento,0),
                IFNULL(Subtotal,0), IFNULL(PrecioCosto,'NO') FROM InformeVenta";
            using var r = cmd.ExecuteReader();
            var list = new List<InformeVenta>();
            while (r.Read())
            {
                list.Add(new InformeVenta
                {
                    IdInformeVenta = r.GetInt32(0),
                    Fecha = ParseFecha(r.GetValue(1)?.ToString()) ?? DateTimeOffset.Now,
                    Total = Convert.ToDecimal(r.GetValue(2)),
                    MetodoPago = r.GetString(3),
                    MultipleMetodoDePago = Convert.ToInt32(r.GetValue(4)) == 1,
                    DetalleAdjunto = Convert.ToInt32(r.GetValue(5)) == 1,
                    Descuento = Convert.ToDecimal(r.GetValue(6)),
                    Subtotal = Convert.ToDecimal(r.GetValue(7)),
                    PrecioCosto = r.GetString(8)
                });
            }
            return list;
        }
        catch (SqliteException)
        {
            return [];
        }
    }

    private static List<InformeVentaDetalle> LeerDetalles(SqliteConnection con)
    {
        try
        {
            using var cmd = con.CreateCommand();
            cmd.CommandText = @"SELECT IdInformeVentaDetalle, IdInformeVenta, Codigo, Nombre, Cantidad, Costo, Precio, SubTotal
                FROM InformeVentaDetalle";
            using var r = cmd.ExecuteReader();
            var list = new List<InformeVentaDetalle>();
            while (r.Read())
            {
                decimal? costo = null;
                if (!r.IsDBNull(5) && decimal.TryParse(r.GetValue(5)?.ToString()?.Replace(',', '.'),
                        System.Globalization.NumberStyles.Any,
                        System.Globalization.CultureInfo.InvariantCulture, out var c))
                    costo = c;

                list.Add(new InformeVentaDetalle
                {
                    IdInformeVentaDetalle = r.GetInt32(0),
                    IdInformeVenta = r.IsDBNull(1) ? 0 : r.GetInt32(1),
                    Codigo = r.GetString(2),
                    Nombre = r.GetString(3),
                    Cantidad = Convert.ToDecimal(r.GetValue(4)),
                    Costo = costo,
                    Precio = Convert.ToDecimal(r.GetValue(6)),
                    SubTotal = Convert.ToDecimal(r.GetValue(7))
                });
            }
            return list;
        }
        catch (SqliteException)
        {
            return [];
        }
    }

    private static List<Configuracion> LeerConfig(SqliteConnection con)
    {
        try
        {
            using var cmd = con.CreateCommand();
            cmd.CommandText = "SELECT Clave, Valor FROM Configuracion";
            using var r = cmd.ExecuteReader();
            var list = new List<Configuracion>();
            while (r.Read())
                list.Add(new Configuracion { Clave = r.GetString(0), Valor = r.GetString(1) });
            return list;
        }
        catch (SqliteException)
        {
            return [];
        }
    }

    private static DateTimeOffset? ParseFecha(string? raw)
    {
        if (string.IsNullOrWhiteSpace(raw)) return null;
        if (DateTimeOffset.TryParse(raw, out var d))
            return d.ToUniversalTime();
        return null;
    }
}
