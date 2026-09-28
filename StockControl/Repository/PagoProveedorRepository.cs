using Dapper;
using StockControl.Domain;
using StockControl.Infrastructure;

namespace StockControl.Repository
{
    public class PagoProveedorRepository
    {
        private string ConnectionString => DbPath.ConnectionString;

        public void Insertar(PagoProveedor pago)
        {
            using var con = new Microsoft.Data.Sqlite.SqliteConnection(ConnectionString);
            con.Execute(@"INSERT INTO PagosProveedor (Fecha, Proveedor, Monto, MetodoPago)
                          VALUES (@Fecha, @Proveedor, @Monto, @MetodoPago)", pago);
        }

        public void Eliminar(int id)
        {
            using var con = new Microsoft.Data.Sqlite.SqliteConnection(ConnectionString);
            con.Execute("DELETE FROM PagosProveedor WHERE IdPagoProveedor = @id", new { id });
        }

        public List<PagoProveedor> ListarDelDia(DateTime dia)
        {
            var inicio = dia.Date;
            var fin = inicio.AddDays(1);
            using var con = new Microsoft.Data.Sqlite.SqliteConnection(ConnectionString);
            return con.Query<PagoProveedor>(
                @"SELECT * FROM PagosProveedor
                  WHERE Fecha >= @inicio AND Fecha < @fin
                  ORDER BY Fecha",
                new { inicio, fin }).ToList();
        }
    }
}
