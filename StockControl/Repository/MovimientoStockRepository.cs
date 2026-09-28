using Dapper;
using StockControl.Domain;
using StockControl.Infrastructure;

namespace StockControl.Repository
{
    public class MovimientoStockRepository
    {
        private string ConnectionString => DbPath.ConnectionString;

        public void Insertar(string codigo, decimal cantidad)
        {
            using var con = new Microsoft.Data.Sqlite.SqliteConnection(ConnectionString);
            con.Execute(@"INSERT INTO MovimientosStock (Fecha, Codigo, Cantidad)
                          VALUES (@Fecha, @Codigo, @Cantidad)",
                new { Fecha = DateTime.Now, Codigo = codigo, Cantidad = cantidad });
        }

        public Dictionary<string, decimal> SumarDesde(DateTime desde)
        {
            using var con = new Microsoft.Data.Sqlite.SqliteConnection(ConnectionString);
            var filas = con.Query<MovimientoStock>(
                @"SELECT Codigo, SUM(Cantidad) AS Cantidad
                  FROM MovimientosStock
                  WHERE Fecha >= @desde
                  GROUP BY Codigo",
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
    }
}
