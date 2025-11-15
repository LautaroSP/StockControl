using Dapper;
using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StockControl.Repository
{
    public class MetodoPagoRepository
    {
        private readonly string _connectionString = "Data Source=stock.db";

        private SqliteConnection GetConnection() => new SqliteConnection(_connectionString);
        public void Insertar(string Descripcion)
        {
            using var con = GetConnection();
            con.Execute(@"INSERT INTO MetodosPago  (Descripcion)
            VALUES (@Descripcion);", new { Descripcion = Descripcion });
        }
        public List<string> ObtenerTodos()
        {
            using var con = GetConnection();
            var result = con.Query<string>(@"SELECT Descripcion FROM MetodosPago");
            if (result != null)
            {
                return result.ToList();
            }
            else
                return new List<string>();
        }
    }
}
