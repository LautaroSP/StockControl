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
    public class GrupoRepository
    {
        private string ConnectionString => DbPath.ConnectionString;

        private SqliteConnection GetConnection() => new SqliteConnection(ConnectionString);

        public void Insertar(GrupoProductos p)
        {
            using var con = GetConnection();
            con.Execute(@"INSERT INTO GrupoProductos (NombreGrupo, PrecioGrupo,GananciaIndividual,Ganancia,Costo) 
              VALUES (@NombreGrupo, @PrecioGrupo, @GananciaIndividual, @Ganancia, @Costo)",
                          new { p.NombreGrupo, p.PrecioGrupo, p.GananciaIndividual, p.Ganancia, p.Costo });

        }

        public void Eliminar(int IdGrupoProducto)
        {
            using var con = GetConnection();
            con.Execute(@"DELETE FROM GrupoProductos 
                      WHERE IdGrupoProducto = @IdGrupoProducto", new { IdGrupoProducto });
        }

        public void Actualizar(GrupoProductos p)
        {
            using var con = GetConnection();
            con.Execute(@"UPDATE GrupoProductos 
                      SET NombreGrupo=@NombreGrupo, PrecioGrupo=@PrecioGrupo, GananciaIndividual=@GananciaIndividual,
                      Ganancia = @Ganancia, Costo = @Costo
                      WHERE IdGrupoProducto=@IdGrupoProducto", p);
        }
        public List<GrupoProductos> Listar()
        {
            using var con = GetConnection();
            return con.Query<GrupoProductos>(@"SELECT * FROM GrupoProductos").ToList();
        }
        public GrupoProductos BuscarPorId(int idGrupo)
        {
            using var con = GetConnection();
            return con.QueryFirst<GrupoProductos>(@"SELECT * FROM GrupoProductos WHERE IdGrupoProducto = @IdGrupoProducto",
                                                    new { IdGrupoProducto = idGrupo });
        }
    }
}
