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
            db.Productos.Add(p);
        foreach (var m in lectura.Metodos)
            db.MetodosPago.Add(m);
        foreach (var c in lectura.Configuraciones)
            db.Configuracion.Add(c);
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

    private static async Task AjustarSeriales(AppDbContext db)
    {
        await db.Database.ExecuteSqlRawAsync("""
            SELECT setval(pg_get_serial_sequence('"Productos"', 'Id'), COALESCE((SELECT MAX("Id") FROM "Productos"), 1));
            SELECT setval(pg_get_serial_sequence('"Usuarios"', 'Id'), COALESCE((SELECT MAX("Id") FROM "Usuarios"), 1));
            SELECT setval(pg_get_serial_sequence('"GrupoProductos"', 'IdGrupoProducto'), COALESCE((SELECT MAX("IdGrupoProducto") FROM "GrupoProductos"), 1));
            SELECT setval(pg_get_serial_sequence('"InformeVenta"', 'IdInformeVenta'), COALESCE((SELECT MAX("IdInformeVenta") FROM "InformeVenta"), 1));
            SELECT setval(pg_get_serial_sequence('"InformeVentaDetalle"', 'IdInformeVentaDetalle'), COALESCE((SELECT MAX("IdInformeVentaDetalle") FROM "InformeVentaDetalle"), 1));
            SELECT setval(pg_get_serial_sequence('"MetodosPago"', 'Id'), COALESCE((SELECT MAX("Id") FROM "MetodosPago"), 1));
            SELECT setval(pg_get_serial_sequence('"Locales"', 'IdLocal'), COALESCE((SELECT MAX("IdLocal") FROM "Locales"), 1));
            """);
    }
}
