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
        public int InsertarInformeVenta(decimal total, string metodoPago, bool multiple, bool detalle, decimal descuento, string costo)
        {
            using var con = GetConnection();

            decimal subTotal = total / (1 - descuento / 100);
            var id = con.ExecuteScalar<int>(@" INSERT INTO InformeVenta (Total, MetodoPago, MultipleMetodoDePago, DetalleAdjunto, Descuento, SubTotal, PrecioCosto)
                                                VALUES (@Total, @MetodoPago, @MultipleMetodoDePago, @DetalleAdjunto, @Descuento, @SubTotal, @PrecioCosto);
                                                SELECT last_insert_rowid();", new { Total = total, MetodoPago = metodoPago, MultipleMetodoDePago = multiple ? 1 : 0 , DetalleAdjunto = detalle ? 1 : 0, Descuento = descuento, SubTotal = subTotal, PrecioCosto = costo });

            return id;
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
            con.Execute(@"INSERT INTO InformeVentaDetalle (IdInformeVenta, Codigo, Nombre, Cantidad, Precio, Subtotal) 
                      VALUES (@IdInformeVenta,@Codigo, @Nombre, @Cantidad, @Precio, @Subtotal)", i);
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
                con.Execute("INSERT INTO Cajas (Fecha, Total, MetodoPago) VALUES (@Fecha, @Total, @MetodoPago)", caja);
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
    }
}
