using StockControl.Domain;
using Microsoft.Data.Sqlite;
using Dapper;
using System.Collections.Generic;

namespace StockControl.Repository
{
    public class ProductoRepository
    {
        private readonly string _connectionString = "Data Source=stock.db";

        private SqliteConnection GetConnection() => new SqliteConnection(_connectionString);

        public void Insertar(Producto p)
        {
            using var con = GetConnection();
            con.Execute(@"INSERT INTO Productos (Codigo, Nombre, Cantidad, Costo, Precio, ProductoSector, GananciaIndividual, ValorGanancia, IdGrupoProducto) 
                      VALUES (@Codigo, @Nombre, @Cantidad, @Costo, @Precio, @ProductoSector, @GananciaIndividual, @ValorGanancia, @IdGrupoProducto)",  p );
        }

        public List<Producto> Listar()
        {
            using var con = GetConnection();
            return con.Query<Producto>("SELECT * FROM Productos").ToList();
        }
        public Producto BuscarPorCodigo(string Codigo)
        {
            using var con = GetConnection();
            return con.QueryFirstOrDefault<Producto>(
            "SELECT * FROM Productos WHERE Codigo = @Codigo",
            new { Codigo });
        }
        public void Actualizar(Producto p)
        {
            using var con = GetConnection();
            con.Execute(@"UPDATE Productos 
                      SET Codigo=@Codigo, Nombre=@Nombre, Cantidad=@Cantidad, 
                          Costo=@Costo, Precio=@Precio, GananciaIndividual = @GananciaIndividual, ValorGanancia = @ValorGanancia, IdGrupoProducto = @IdGrupoProducto
                      WHERE Id=@Id", p);
        }
        public void ActualizarGrupo(int id, int idGrupoProducto)
        {
            using var con = GetConnection();
            con.Execute(@"UPDATE Productos 
                      SET IdGrupoProducto = @IdGrupoProducto
                      WHERE Id=@Id", new {Id = id, IdGrupoProducto = idGrupoProducto });
        }
        public void Eliminar(int id)
        {
            using var con = GetConnection();
            con.Execute("DELETE FROM Productos WHERE Id=@id", new { id });
        }

        public List<Producto> BuscarPorGrupo(int IdGrupoProducto)
        {
            using var con = GetConnection();
            return con.Query<Producto>(
            "SELECT * FROM Productos WHERE IdGrupoProducto = @IdGrupoProducto",
            new { IdGrupoProducto }).ToList();
        }

        public void ActualizarPrecioPorPrecioGrupo(List<Producto> productosDelGrupo, decimal precioGrupo, decimal costo)
        {
            using var con = GetConnection();

            var ids = productosDelGrupo.Select(p => p.Id).ToList();

            var sql = $"UPDATE Productos SET Precio = @precio, Costo = @costo WHERE Id IN ({string.Join(",", ids)})";

            con.Execute(sql, new { precio = precioGrupo, costo = costo });
        }
    }

}
