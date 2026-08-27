using Microsoft.Data.Sqlite;
using StockControl.Api.Importacion;
using StockControl.Dominio;

namespace StockControl.Tests;

public class LectorSqliteTests
{
    [Fact]
    public void Import_productos_quedan_con_IdLocal()
    {
        var ruta = Path.Combine(Path.GetTempPath(), $"sc-{Guid.NewGuid():N}.db");
        using (var con = new SqliteConnection($"Data Source={ruta}"))
        {
            con.Open();
            using var cmd = con.CreateCommand();
            cmd.CommandText = @"
                CREATE TABLE Productos (
                    Id INTEGER PRIMARY KEY,
                    Codigo TEXT NOT NULL,
                    Nombre TEXT NOT NULL,
                    Cantidad REAL NOT NULL,
                    Costo REAL,
                    Precio REAL NOT NULL,
                    ProductoSector INTEGER NOT NULL DEFAULT 0,
                    IdGrupoProducto INTEGER NOT NULL DEFAULT 0,
                    GananciaIndividual INTEGER NOT NULL DEFAULT 0,
                    ValorGanancia REAL NOT NULL DEFAULT 0,
                    FechaModificacion TEXT
                );
                INSERT INTO Productos (Id, Codigo, Nombre, Cantidad, Precio, ProductoSector) VALUES
                    (1, 'AAA', 'Yerba', 4, 100, 0),
                    (2, '1', 'Fiambreria', 1, 0, 1);";
            cmd.ExecuteNonQuery();
        }

        try
        {
            var lectura = LectorSqliteEscritorio.Leer(ruta);
            lectura.AsignarLocal(9);
            Assert.Equal(2, lectura.Productos.Count);
            Assert.Equal(9, lectura.Productos[0].IdLocal);
            Assert.False(lectura.Productos[0].ProductoSector);
            Assert.True(lectura.Productos[1].ProductoSector);
            Assert.Equal("Fiambreria", lectura.Productos[1].Nombre);
        }
        finally
        {
            SqliteConnection.ClearAllPools();
            if (File.Exists(ruta))
                File.Delete(ruta);
        }
    }

    [Fact]
    public void Cajas_del_mismo_segundo_comparten_id_cierre()
    {
        var t = new DateTimeOffset(2026, 8, 24, 22, 0, 0, TimeSpan.Zero);
        var cajas = new List<Caja>
        {
            new() { Fecha = t, MetodoPago = "Efectivo", Total = 10 },
            new() { Fecha = t, MetodoPago = "Total", Total = 10 },
            new() { Fecha = t.AddHours(1), MetodoPago = "Total", Total = 5 }
        };
        ImportadorSqlite.AsignarNrosCajaImportadas(cajas);
        Assert.Equal(1, cajas[0].IdCierre);
        Assert.Equal(1, cajas[1].IdCierre);
        Assert.Equal(2, cajas[2].IdCierre);
        Assert.All(cajas, c => Assert.Equal(1, c.NroCaja));
    }
}
