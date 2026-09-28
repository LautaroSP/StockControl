using Dapper;
using Microsoft.Data.Sqlite;
using StockControl.Domain;
using StockControl.Infrastructure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StockControl.Repository
{
    public class InformeVentaRepository
    {
        private string ConnectionString => DbPath.ConnectionString;

        private SqliteConnection GetConnection() => new SqliteConnection(ConnectionString);
        public int InsertarInformeVenta(decimal total, string metodoPago, bool multiple, bool detalle, decimal descuento, string costo, int idGrupo, decimal subTotal)
        {
            using var con = GetConnection();

            var id = con.ExecuteScalar<int>(@" INSERT INTO InformeVenta (Total, MetodoPago, MultipleMetodoDePago, DetalleAdjunto, Descuento, SubTotal, PrecioCosto, IdGrupo)
                                                VALUES (@Total, @MetodoPago, @MultipleMetodoDePago, @DetalleAdjunto, @Descuento, @SubTotal, @PrecioCosto, @IdGrupo);
                                                SELECT last_insert_rowid();", new { Total = total, MetodoPago = metodoPago, MultipleMetodoDePago = multiple ? 1 : 0 , DetalleAdjunto = detalle ? 1 : 0, Descuento = descuento, SubTotal = subTotal, PrecioCosto = costo, IdGrupo = idGrupo });

            return id;
        }

        public void AsignarIdGrupo(int idInformeVenta, int idGrupo)
        {
            using var con = GetConnection();
            con.Execute("UPDATE InformeVenta SET IdGrupo = @idGrupo WHERE IdInformeVenta = @idInformeVenta",
                new { idInformeVenta, idGrupo });
        }
        public List<InformeVenta> ListarParaCerrarCaja(DateTime? fechaSeleccionada = null)
        { 
            using var con = GetConnection();
            if(fechaSeleccionada == null)
                return con.Query<InformeVenta>("SELECT * FROM InformeVenta WHERE Fecha >= datetime('now','localtime','start of day')").ToList();
            else
            {
                var inicio = fechaSeleccionada.Value.Date;
                var fin = inicio.AddDays(1);

                return con.Query<InformeVenta>(
                    @"SELECT * 
              FROM InformeVenta 
              WHERE Fecha >= @inicio AND Fecha < @fin",
                    new { inicio, fin }
                ).ToList();
            }
        }
        public List<InformeVenta> ListarInformeVenta()
        {
            using var con = GetConnection();
            return con.Query<InformeVenta>("SELECT * FROM InformeVenta").ToList();
        }

        public void EliminarInformeVenta(int id)
        {
            using var con = GetConnection();
            con.Execute("DELETE FROM InformeVentaDetalle WHERE IdInformeVenta=@id;" +
                "DELETE FROM InformeVenta WHERE IdInformeVenta=@id;", new { id });
        }
        public void InsertarInformeVentaDetalle(InformeVentaDetalle i)
        {
            using var con = GetConnection();
            con.Execute(@"INSERT INTO InformeVentaDetalle (IdInformeVenta, Codigo, Nombre, Cantidad, Precio, CostoVenta, Subtotal, EsSector, EsGenerico, ClasificacionRegistrada) 
                      VALUES (@IdInformeVenta,@Codigo, @Nombre, @Cantidad, @Precio, @CostoVenta, @Subtotal, @EsSector, @EsGenerico, @ClasificacionRegistrada)", i);
        }

        public List<InformeVentaDetalle> ListarDetallesDe(IEnumerable<int> ids)
        {
            var lista = ids.ToList();
            if (lista.Count == 0)
                return new List<InformeVentaDetalle>();

            using var con = GetConnection();
            return con.Query<InformeVentaDetalle>(
                "SELECT * FROM InformeVentaDetalle WHERE IdInformeVenta IN @ids",
                new { ids = lista }).ToList();
        }

        public Dictionary<string, decimal> SumarCantidadVendidaDesde(DateTime desde)
        {
            using var con = GetConnection();
            var filas = con.Query<InformeVentaDetalle>(@"
                SELECT d.Codigo AS Codigo, SUM(d.Cantidad) AS Cantidad
                FROM InformeVentaDetalle d
                INNER JOIN InformeVenta v ON v.IdInformeVenta = d.IdInformeVenta
                WHERE v.Fecha >= @desde
                GROUP BY d.Codigo",
                new { desde }).ToList();

            var mapa = new Dictionary<string, decimal>(StringComparer.OrdinalIgnoreCase);
            foreach (var fila in filas)
            {
                if (string.IsNullOrEmpty(fila.Codigo))
                    continue;
                mapa.TryGetValue(fila.Codigo, out decimal actual);
                mapa[fila.Codigo] = actual + fila.Cantidad;
            }
            return mapa;
        }

        public List<InformeVentaDetalle> ListarInformeVentaDetalle(int id)
        {
            using var con = GetConnection();
            return con.Query<InformeVentaDetalle>("SELECT * FROM InformeVentaDetalle WHERE IdInformeVenta = @id", new {id}).ToList();
        }

        public void InsertarCajaCerradaPorLista(List<Caja> informeCajaCerrada)
        {
            using var con = GetConnection();
            foreach (var caja in informeCajaCerrada)
            {
                con.Execute(@"INSERT INTO Cajas (Fecha, Total, MetodoPago, CantidadVentas, Ganancia, GenericosOSector, PagoProveedor, IdCierre)
                              VALUES (@Fecha, @Total, @MetodoPago, @CantidadVentas, @Ganancia, @GenericosOSector, @PagoProveedor, @IdCierre)", caja);
            }
        }

        public List<Caja> ListarCajas()
        {
            using var con = GetConnection();
            return con.Query<Caja>("SELECT * FROM Cajas").ToList();
        }

        public List<Caja> ListarCajasPorRango(DateTime desde, DateTime hasta)
        {
            using var con = GetConnection();
            return con.Query<Caja>(
                @"SELECT * FROM Cajas 
                  WHERE Fecha >= @desde AND Fecha < @hasta",
                new { desde, hasta }
            ).ToList();
        }

        public void InsertarProductosCierre(List<CajaProducto> productos)
        {
            using var con = GetConnection();
            foreach (var producto in productos)
            {
                con.Execute(@"INSERT INTO CajaProductos
                    (IdCierre, Codigo, Nombre, CantidadVendida, StockInicial, StockFinal, Ganancia, EsSector, EsGenerico)
                    VALUES (@IdCierre, @Codigo, @Nombre, @CantidadVendida, @StockInicial, @StockFinal, @Ganancia, @EsSector, @EsGenerico)",
                    producto);
            }
        }

        public List<CajaProducto> ListarProductosCierre(string idCierre)
        {
            using var con = GetConnection();
            return con.Query<CajaProducto>(
                "SELECT * FROM CajaProductos WHERE IdCierre = @idCierre ORDER BY Nombre",
                new { idCierre }).ToList();
        }

        public void RecalcularSubtotales()
        {
            var informes = ListarInformeVenta();
            if (informes.Count == 0)
                return;

            var detalles = ListarDetallesDe(informes.Select(i => i.IdInformeVenta));
            var porInforme = detalles
                .GroupBy(d => d.IdInformeVenta)
                .ToDictionary(g => g.Key, g => g.ToList());
            var productos = new ProductoRepository().Listar()
                .Where(p => !string.IsNullOrEmpty(p.Codigo))
                .GroupBy(p => p.Codigo, StringComparer.OrdinalIgnoreCase)
                .ToDictionary(g => g.Key, g => g.First(), StringComparer.OrdinalIgnoreCase);

            foreach (var grupo in AgruparInformes(informes))
            {
                var conLineas = grupo.FirstOrDefault(i =>
                    porInforme.TryGetValue(i.IdInformeVenta, out var lineas) && lineas.Count > 0);
                if (conLineas == null)
                    continue;

                decimal subtotal = SubtotalDeLista(conLineas, porInforme[conLineas.IdInformeVenta], productos);
                decimal totalGrupo = grupo.Sum(i => i.Total);
                decimal acumulado = 0;
                for (int i = 0; i < grupo.Count; i++)
                {
                    decimal parte;
                    if (grupo.Count == 1 || totalGrupo == 0)
                        parte = grupo.Count == 1 ? subtotal : 0;
                    else if (i == grupo.Count - 1)
                        parte = subtotal - acumulado;
                    else
                    {
                        parte = Math.Round(subtotal * grupo[i].Total / totalGrupo, 2, MidpointRounding.AwayFromZero);
                        acumulado += parte;
                    }

                    if (grupo[i].subTotal == parte)
                        continue;

                    grupo[i].subTotal = parte;
                    using var con = GetConnection();
                    con.Execute(
                        "UPDATE InformeVenta SET SubTotal = @parte WHERE IdInformeVenta = @id",
                        new { parte, id = grupo[i].IdInformeVenta });
                }
            }
        }

        private static decimal SubtotalDeLista(
            InformeVenta informe,
            List<InformeVentaDetalle> lineas,
            Dictionary<string, Producto> productos)
        {
            bool alCosto = string.Equals(informe.PrecioCosto, "Si", StringComparison.OrdinalIgnoreCase);
            decimal porcentaje = informe.Descuento / 100m;
            decimal suma = 0;

            foreach (var linea in lineas)
            {
                productos.TryGetValue(linea.Codigo ?? string.Empty, out var producto);
                bool generico = linea.EsGenerico == 1
                    || (linea.Codigo?.Contains("GENERIC", StringComparison.OrdinalIgnoreCase) ?? false);
                bool sector = linea.EsSector == 1
                    || (!generico && linea.ClasificacionRegistrada == 0 && producto?.ProductoSector == 1);

                if (alCosto || generico || sector || porcentaje <= 0)
                {
                    suma += linea.Precio * linea.Cantidad;
                    continue;
                }

                if (porcentaje >= 1m)
                {
                    decimal lista = producto?.Precio ?? linea.Precio;
                    suma += lista * linea.Cantidad;
                    continue;
                }

                decimal costo = linea.ClasificacionRegistrada == 1
                    ? linea.CostoVenta
                    : producto?.Costo ?? 0m;
                decimal precioLista = (linea.Precio - costo * porcentaje) / (1 - porcentaje);
                suma += Math.Round(precioLista, 2, MidpointRounding.AwayFromZero) * linea.Cantidad;
            }

            return Math.Round(suma, 2, MidpointRounding.AwayFromZero);
        }

        private static List<List<InformeVenta>> AgruparInformes(List<InformeVenta> informes)
        {
            var grupos = new List<List<InformeVenta>>();
            var ordenados = informes.OrderBy(i => i.IdInformeVenta).ToList();

            foreach (var grupo in ordenados.Where(i => i.IdGrupo > 0).GroupBy(i => i.IdGrupo))
                grupos.Add(grupo.OrderBy(i => i.IdInformeVenta).ToList());

            var resto = ordenados.Where(i => i.IdGrupo == 0).ToList();
            int indice = 0;
            while (indice < resto.Count)
            {
                var actual = resto[indice];
                if (actual.MultipleMetodoDePago == 1 && actual.DetalleAdjunto == 1)
                {
                    var grupo = new List<InformeVenta> { actual };
                    int siguiente = indice + 1;
                    while (siguiente < resto.Count
                        && resto[siguiente].MultipleMetodoDePago == 1
                        && resto[siguiente].DetalleAdjunto == 0)
                    {
                        grupo.Add(resto[siguiente]);
                        siguiente++;
                    }
                    grupos.Add(grupo);
                    indice = siguiente;
                }
                else
                {
                    grupos.Add(new List<InformeVenta> { actual });
                    indice++;
                }
            }

            return grupos;
        }
    }
}
