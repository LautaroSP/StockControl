using Dapper;
using Microsoft.Data.Sqlite;
using StockControl.Domain;
using StockControl.Infrastructure;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StockControl.Repository
{
    public class ConfiguracionRepository
    {
        private string ConnectionString => DbPath.ConnectionString;

        private SqliteConnection GetConnection() => new SqliteConnection(ConnectionString);

        public void Insertar(string clave, string valor)
        {
            using var con = GetConnection();
            con.Execute(@"INSERT INTO Configuracion (Clave, Valor)
            VALUES (@Clave, @Valor)
            ON CONFLICT(Clave) DO UPDATE SET Valor = @Valor;", new {Clave = clave,Valor = valor});
        }
        public string ObtenerPorClave(string clave)
        {
            using var con = GetConnection();
            var result = con.ExecuteScalar<string>(@"SELECT Valor FROM Configuracion
                                               WHERE Clave = @Clave;", new { Clave = clave });
            if (result != null)
            {
                return result;
            }
            else
                return string.Empty;
        }

    }
}
