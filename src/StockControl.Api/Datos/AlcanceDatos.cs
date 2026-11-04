using System.Data;
using System.Data.Common;
using Microsoft.EntityFrameworkCore;

namespace StockControl.Api.Datos;

/// <summary>
/// Qué filas puede ver esta conexión. Sin alcance ni local, Postgres no devuelve filas de un local.
/// </summary>
public class AlcanceDatos
{
    public const string Todo = "todo";

    public bool VerTodo { get; private set; }
    public int? IdLocalFijado { get; private set; }
    private readonly List<int> _locales = new();

    public void AbrirTodo() => VerTodo = true;

    public void FijarLocal(int idLocal) => IdLocalFijado = idLocal;

    public void PermitirLectura(IEnumerable<int> ids)
    {
        foreach (var id in ids)
        {
            if (id > 0 && !_locales.Contains(id))
                _locales.Add(id);
        }
    }

    public int? LocalDe(SesionActual sesion) => IdLocalFijado ?? sesion.IdLocal;

    public string TextoLocales => _locales.Count == 0 ? "" : string.Join(',', _locales);

    public async Task Aplicar(DbConnection conexion, SesionActual sesion, CancellationToken cancelar = default)
    {
        if (conexion.State != ConnectionState.Open)
            await conexion.OpenAsync(cancelar);
        await Escribir(conexion, sesion, false, cancelar);
    }

    /// <summary>
    /// Dentro de la transacción ve todos los locales. Al confirmar, la conexión vuelve al alcance de la sesión.
    /// </summary>
    public static async Task MientrasTodo(AppDbContext db, Func<Task> accion, CancellationToken cancelar = default)
    {
        await using var tx = await db.Database.BeginTransactionAsync(cancelar);
        await using var cmd = db.Database.GetDbConnection().CreateCommand();
        cmd.CommandText = "SELECT set_config('app.alcance', 'todo', true)";
        await cmd.ExecuteNonQueryAsync(cancelar);
        await accion();
        await tx.CommitAsync(cancelar);
    }

    internal async Task Escribir(DbConnection conexion, SesionActual sesion, bool soloTransaccion, CancellationToken cancelar)
    {
        await using var cmd = conexion.CreateCommand();
        cmd.CommandText = """
            SELECT set_config('app.alcance', @alcance, @local),
                   set_config('app.id_local', @id_local, @local),
                   set_config('app.id_usuario', @id_usuario, @local),
                   set_config('app.locales', @locales, @local)
            """;
        Texto(cmd, "alcance", VerTodo ? Todo : "");
        Texto(cmd, "id_local", LocalDe(sesion)?.ToString() ?? "");
        Texto(cmd, "id_usuario", sesion.IdUsuario?.ToString() ?? "");
        Texto(cmd, "locales", TextoLocales);
        var marca = cmd.CreateParameter();
        marca.ParameterName = "local";
        marca.Value = soloTransaccion;
        cmd.Parameters.Add(marca);
        await cmd.ExecuteNonQueryAsync(cancelar);
    }

    private static void Texto(DbCommand cmd, string nombre, string valor)
    {
        var p = cmd.CreateParameter();
        p.ParameterName = nombre;
        p.Value = valor;
        cmd.Parameters.Add(p);
    }
}
