using Dapper;
using Microsoft.Data.Sqlite;

namespace StockControl.Infrastructure
{
    public class DbInitializer
    {
        private readonly string _connectionString = "Data Source=stock.db";
        public void Initialize()
        {

            using var con = new SqliteConnection(_connectionString);
            con.Open();

            using var cmd = con.CreateCommand();

            cmd.CommandText = @"
                CREATE TABLE IF NOT EXISTS Productos (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    Codigo TEXT NOT NULL,
                    Nombre TEXT NOT NULL,
                    Cantidad REAL NOT NULL,
                    Costo REAL NULL,
                    Precio REAL NOT NULL,
                    ProductoSector INTEGER NOT NULL DEFAULT 0
                );";
            cmd.ExecuteNonQuery();

            cmd.CommandText = @"
                CREATE TABLE IF NOT EXISTS GrupoProductos (
                    IdGrupoProducto INTEGER PRIMARY KEY AUTOINCREMENT,
                    NombreGrupo TEXT NOT NULL,
                    PrecioGrupo REAL NOT NULL DEFAULT 0
                );";
            cmd.ExecuteNonQuery();

            cmd.CommandText = @"
                CREATE TABLE IF NOT EXISTS MetodosPago (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    Descripcion TEXT NOT NULL
                );";
            cmd.ExecuteNonQuery();

            cmd.CommandText = @"
                CREATE TABLE IF NOT EXISTS InformeVentaDetalle (
                    IdInformeVentaDetalle INTEGER PRIMARY KEY AUTOINCREMENT,
                    IdInformeVenta INTEGER,
                    Codigo TEXT NOT NULL,
                    Nombre TEXT NOT NULL,
                    Cantidad REAL NOT NULL,
                    Costo TEXT NULL,
                    Precio REAL NOT NULL,
                    SubTotal REAL NOT NULL,
                    FOREIGN KEY (IdInformeVenta) REFERENCES InformeVenta(IdInformeVenta)
                );";
            cmd.ExecuteNonQuery();

            cmd.CommandText = @"
                CREATE TABLE IF NOT EXISTS InformeVenta (
                    IdInformeVenta INTEGER PRIMARY KEY AUTOINCREMENT,
                    Fecha DATETIME NOT NULL DEFAULT (datetime('now','localtime')),
                    Total REAL NOT NULL,
                    MetodoPago TEXT NOT NULL
                );";
            cmd.ExecuteNonQuery();

            cmd.CommandText = @"
                CREATE TABLE IF NOT EXISTS Cajas (
                    IdCaja INTEGER PRIMARY KEY AUTOINCREMENT,
                    Fecha DATETIME NOT NULL DEFAULT (datetime('now','localtime')),
                    Total REAL NOT NULL,
                    MetodoPago TEXT NOT NULL
                );";
            cmd.ExecuteNonQuery();

            cmd.CommandText = @"
                CREATE TABLE IF NOT EXISTS Configuracion (
                    Clave TEXT PRIMARY KEY,
                    Valor TEXT NOT NULL
                );";
            cmd.ExecuteNonQuery();

            try
            {
                cmd.CommandText = "ALTER TABLE Productos ADD COLUMN ProductoSector INTEGER NOT NULL DEFAULT 0;";
                cmd.ExecuteNonQuery();

            }
            catch(SqliteException)
            {
                // La columna ya existe, no hacer nada
            }

            try
            {
                cmd.CommandText = "ALTER TABLE InformeVenta ADD COLUMN MetodoPago TEXT NOT NULL DEFAULT '';";
                cmd.ExecuteNonQuery();
            }
            catch (SqliteException ex)
            {
                // La columna ya existe, no hacer nada
            }

            try
            {
                cmd.CommandText = "ALTER TABLE InformeVenta ADD COLUMN MultipleMetodoDePago INTEGER NOT NULL DEFAULT 0;";
                cmd.ExecuteNonQuery();
            }
            catch (SqliteException ex)
            {
                // La columna ya existe, no hacer nada
            }
            try
            {
                cmd.CommandText = "ALTER TABLE InformeVenta ADD COLUMN DetalleAdjunto INTEGER NOT NULL DEFAULT 0;";
                cmd.ExecuteNonQuery();
            }
            catch (SqliteException ex)
            {
                // La columna ya existe, no hacer nada
            }
            try
            {
                cmd.CommandText = "ALTER TABLE Productos ADD COLUMN IdGrupoProducto INTEGER NOT NULL DEFAULT 0;";
                cmd.ExecuteNonQuery();
            }
            catch (SqliteException ex)
            {
                // La columna ya existe, no hacer nada
            }
            try
            {
                cmd.CommandText = "ALTER TABLE Productos ADD COLUMN GananciaIndividual INTEGER NOT NULL DEFAULT 0;";
                cmd.ExecuteNonQuery();
            }
            catch (SqliteException ex)
            {
                // La columna ya existe, no hacer nadaValorGanancia
            }
            try
            {
                cmd.CommandText = "ALTER TABLE Productos ADD COLUMN ValorGanancia REAL NOT NULL DEFAULT 0;";
                cmd.ExecuteNonQuery();
            }
            catch (SqliteException ex)
            {
                // La columna ya existe, no hacer nadaValorGanancia
            }
            try
            {
                cmd.CommandText = "ALTER TABLE GrupoProductos ADD COLUMN Ganancia REAL;";
                cmd.ExecuteNonQuery();
            }
            catch (SqliteException ex)
            {
                // La columna ya existe, no hacer nada
            }
            try
            {
                cmd.CommandText = "ALTER TABLE GrupoProductos ADD COLUMN GananciaIndividual INTEGER NOT NULL DEFAULT 0;";
                cmd.ExecuteNonQuery();
            }
            catch (SqliteException ex)
            {
                // La columna ya existe, no hacer nada
            }
            try
            {
                cmd.CommandText = "ALTER TABLE GrupoProductos ADD COLUMN Costo REAL;";
                cmd.ExecuteNonQuery();
            }
            catch (SqliteException ex)
            {
                // La columna ya existe, no hacer nada
            }

        }
    }
}

