using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using StockControl.Api.Datos;
using StockControl.Dominio;

namespace StockControl.Api.Importacion;

public static class ImportadorSqlite
{
    public static async Task<int> Importar(AppDbContext db, string ruta, string nombreLocal, string usuario, string clave)
    {
        var lectura = LectorSqliteEscritorio.Leer(ruta);
        var hasher = new PasswordHasher<Usuario>();

        var local = new Local
        {
            Nombre = nombreLocal,
            EstadoAbono = "al_dia",
            Vence = DateTimeOffset.UtcNow.AddDays(30)
        };
        db.Locales.Add(local);
        await db.SaveChangesAsync();

        lectura.AsignarLocal(local.IdLocal);

        if (await db.Usuarios.IgnoreQueryFilters().AnyAsync(u => u.NombreUsuario == usuario))
            throw new ErrorNegocio($"Ya existe el usuario {usuario}.");

        var dueno = new Usuario { NombreUsuario = usuario, Rol = Roles.Dueno };
        dueno.HashClave = hasher.HashPassword(dueno, clave);
        db.Usuarios.Add(dueno);
        await db.SaveChangesAsync();
        db.UsuarioLocales.Add(new UsuarioLocal { IdUsuario = dueno.Id, IdLocal = local.IdLocal });

        foreach (var g in lectura.Grupos)
            db.GrupoProductos.Add(g);
        foreach (var p in lectura.Productos)
        {
            p.UsuarioModificacion = "import";
            db.Productos.Add(p);
        }
        foreach (var m in lectura.Metodos)
            db.MetodosPago.Add(m);
        foreach (var c in lectura.Configuraciones)
            db.Configuracion.Add(c);
        var clavesConfiguracion = lectura.Configuraciones.Select(c => c.Clave).ToHashSet(StringComparer.OrdinalIgnoreCase);
        foreach (var (claveConfig, valor) in ConfiguracionesPorDefecto())
        {
            if (!clavesConfiguracion.Contains(claveConfig))
                db.Configuracion.Add(new Configuracion { IdLocal = local.IdLocal, Clave = claveConfig, Valor = valor });
        }
        // El .db no vincula ventas a un nro de caja: se numeran por Fecha del cierre, sin sellar InformeVenta.
        AsignarNrosCajaImportadas(lectura.Cajas);
        foreach (var caja in lectura.Cajas)
            db.Cajas.Add(caja);
        await db.SaveChangesAsync();
        db.ChangeTracker.Clear();

        var detPorVenta = lectura.Detalles.ToLookup(d => d.IdInformeVenta);
        var lote = 0;
        foreach (var v in lectura.Ventas)
        {
            foreach (var d in detPorVenta[v.IdInformeVenta])
                v.Detalles.Add(d);
            db.InformeVenta.Add(v);
            lote++;
            if (lote % 250 == 0)
            {
                await db.SaveChangesAsync();
                db.ChangeTracker.Clear();
            }
        }

        await db.SaveChangesAsync();
        await AjustarSeriales(db);
        return local.IdLocal;
    }

    // Agrupa por Fecha (segundo): IdCierre correlativo; NroCaja = puesto 1 (historial).
    public static void AsignarNrosCajaImportadas(IEnumerable<Caja> cajas)
    {
        var idCierre = 1;
        foreach (var grupo in cajas
            .GroupBy(c => new DateTimeOffset(c.Fecha.Year, c.Fecha.Month, c.Fecha.Day, c.Fecha.Hour, c.Fecha.Minute, c.Fecha.Second, TimeSpan.Zero))
            .OrderBy(g => g.Key))
        {
            foreach (var c in grupo)
            {
                c.IdCierre = idCierre;
                c.NroCaja = 1;
                c.TipoDesglose = TipoDesgloseCaja.Medio;
                if (string.IsNullOrWhiteSpace(c.NombreCierre))
                    c.NombreCierre = "import";
            }
            idCierre++;
        }
    }

    private static async Task AjustarSeriales(AppDbContext db)
    {
        await db.Database.ExecuteSqlRawAsync("""
            SELECT setval(pg_get_serial_sequence('"Productos"', 'Id'), COALESCE((SELECT MAX("Id") FROM "Productos"), 1));
            SELECT setval(pg_get_serial_sequence('"Usuarios"', 'Id'), COALESCE((SELECT MAX("Id") FROM "Usuarios"), 1));
            SELECT setval(pg_get_serial_sequence('"GrupoProductos"', 'IdGrupoProducto'), COALESCE((SELECT MAX("IdGrupoProducto") FROM "GrupoProductos"), 1));
            SELECT setval(pg_get_serial_sequence('"InformeVenta"', 'IdInformeVenta'), COALESCE((SELECT MAX("IdInformeVenta") FROM "InformeVenta"), 1));
            SELECT setval(pg_get_serial_sequence('"InformeVentaDetalle"', 'IdInformeVentaDetalle'), COALESCE((SELECT MAX("IdInformeVentaDetalle") FROM "InformeVentaDetalle"), 1));
            SELECT setval(pg_get_serial_sequence('"MetodosPago"', 'Id'), COALESCE((SELECT MAX("Id") FROM "MetodosPago"), 1));
            SELECT setval(pg_get_serial_sequence('"Cajas"', 'IdCaja'), COALESCE((SELECT MAX("IdCaja") FROM "Cajas"), 1));
            SELECT setval(pg_get_serial_sequence('"Locales"', 'IdLocal'), COALESCE((SELECT MAX("IdLocal") FROM "Locales"), 1));
            """);
    }

    private static IReadOnlyDictionary<string, string> ConfiguracionesPorDefecto() => new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
    {
        ["CantidadCajas"] = "1",
        ["FactorGanancia"] = "1",
        ["IVA"] = "1.21",
        ["StockRigido"] = "0",
        ["UmbralStockBajo"] = "5",
        ["EmpleadoPuedeModificarPrecios"] = "0",
        ["FormatoTicket"] = "POS",
        ["ImprimirTicketAlCobrar"] = "1"
    };
}
