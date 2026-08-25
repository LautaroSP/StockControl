using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using StockControl.Api.Datos;
using StockControl.Dominio;

namespace StockControl.Api.Datos;

public static class SemillaDesarrollo
{
    public static async Task Ejecutar(AppDbContext db, string clave)
    {
        if (await db.Usuarios.IgnoreQueryFilters().AnyAsync())
            return;

        var hasher = new PasswordHasher<Usuario>();
        var jorge = new Usuario { NombreUsuario = "jorge", Rol = Roles.Dueno };
        jorge.HashClave = hasher.HashPassword(jorge, clave);
        var ana = new Usuario { NombreUsuario = "ana", Rol = Roles.Empleado };
        ana.HashClave = hasher.HashPassword(ana, clave);

        var local = new Local
        {
            Nombre = "Lo de Pepe — Almagro",
            EstadoAbono = "al_dia",
            Vence = DateTimeOffset.UtcNow.AddDays(30)
        };

        db.Locales.Add(local);
        db.Usuarios.AddRange(jorge, ana);
        await db.SaveChangesAsync();

        db.UsuarioLocales.AddRange(
            new UsuarioLocal { IdUsuario = jorge.Id, IdLocal = local.IdLocal },
            new UsuarioLocal { IdUsuario = ana.Id, IdLocal = local.IdLocal });

        db.MetodosPago.AddRange(
            new MetodoPago { IdLocal = local.IdLocal, Descripcion = "Efectivo" },
            new MetodoPago { IdLocal = local.IdLocal, Descripcion = "Mercado Pago" });

        db.Productos.AddRange(
            new Producto
            {
                IdLocal = local.IdLocal,
                Codigo = "7790895001234",
                Nombre = "Coca Cola 500ml",
                Cantidad = 12,
                Costo = 1400,
                Precio = 2500
            },
            new Producto
            {
                IdLocal = local.IdLocal,
                Codigo = "7790895005678",
                Nombre = "Pan lacteal",
                Cantidad = 8,
                Costo = 1100,
                Precio = 1800
            },
            new Producto
            {
                IdLocal = local.IdLocal,
                Codigo = "SEC-FIAMBRE",
                Nombre = "Fiambre (sector)",
                Cantidad = 1,
                ProductoSector = true
            });

        db.Configuracion.Add(new Configuracion
        {
            IdLocal = local.IdLocal,
            Clave = "StockRigido",
            Valor = "0"
        });

        await db.SaveChangesAsync();
    }
}
