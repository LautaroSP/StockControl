using System.Data;
using System.Data.Common;
using Microsoft.EntityFrameworkCore;
using StockControl.Api.Datos;
using StockControl.Dominio;

namespace StockControl.Api.Tienda;

public static class LocalesTienda
{
    /// <summary>
    /// El alias se resuelve con una función que no está sujeta a las políticas.
    /// Después la conexión queda fijada a ese local.
    /// </summary>
    public static async Task<Local?> PorAlias(AppDbContext db, AlcanceDatos alcance, SesionActual sesion, string alias)
    {
        var valor = alias.Trim().ToLowerInvariant();
        if (valor.Length < ServicioTienda.LargoMinimoAlias || valor.Length > ServicioTienda.LargoMaximoAlias)
            return null;
        var id = await Escalar(db, "SELECT tienda_por_alias(@alias)", ("alias", valor));
        if (id is not int idLocal)
            return null;
        alcance.FijarLocal(idLocal);
        await alcance.Aplicar(db.Database.GetDbConnection(), sesion);
        return await db.Locales.AsNoTracking().FirstOrDefaultAsync(l => l.IdLocal == idLocal);
    }

    public static async Task<bool> AliasTomado(AppDbContext db, string alias, int idActual)
    {
        var valor = await Escalar(db, "SELECT alias_tomado(@alias, @id)", ("alias", alias), ("id", idActual));
        return valor is true;
    }

    public static async Task<string?> AjustePublico(AppDbContext db, string clave)
    {
        var valor = await Escalar(db, "SELECT ajuste_publico(@clave)", ("clave", clave));
        return valor as string;
    }

    private static async Task<object?> Escalar(AppDbContext db, string sql, params (string Nombre, object Valor)[] parametros)
    {
        var conexion = db.Database.GetDbConnection();
        if (conexion.State != ConnectionState.Open)
            await conexion.OpenAsync();
        await using var cmd = conexion.CreateCommand();
        cmd.CommandText = sql;
        foreach (var (nombre, valor) in parametros)
        {
            var p = cmd.CreateParameter();
            p.ParameterName = nombre;
            p.Value = valor;
            cmd.Parameters.Add(p);
        }
        var escalar = await cmd.ExecuteScalarAsync();
        return escalar is DBNull ? null : escalar;
    }
}
