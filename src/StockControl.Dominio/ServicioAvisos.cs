using System.Globalization;

namespace StockControl.Dominio;

/// <summary>Avisos de la campanita para cada usuario del local, y a quiénes además les llega Web Push.</summary>
public record AvisosPedido(IReadOnlyList<Aviso> Avisos, IReadOnlyList<int> IdsUsuarioPush, string Titulo, string Texto);

public static class ServicioAvisos
{
    public const string PedidoNuevo = "pedido_nuevo";
    public const string PedidoCancelado = "pedido_cancelado";
    public const int TopeLista = 30;

    private static readonly CultureInfo Ar = new("es-AR");

    /// <summary>La campanita le llega a todo usuario activo del local que puede ver pedidos. Push solo si lo eligió.</summary>
    public static AvisosPedido DePedido(
        string tipo,
        PedidoTienda pedido,
        IEnumerable<UsuarioLocal> vinculos,
        IEnumerable<Usuario> usuarios,
        IEnumerable<PreferenciaAviso> preferencias,
        bool empleadoVePedidos,
        DateTimeOffset ahora)
    {
        var delLocal = vinculos.Where(v => v.IdLocal == pedido.IdLocal).Select(v => v.IdUsuario).ToHashSet();
        var destinatarios = usuarios
            .Where(u => u.Activo && delLocal.Contains(u.Id) && Permisos.PuedeGestionarPedidosTienda(u.Rol, empleadoVePedidos))
            .Select(u => u.Id)
            .Distinct()
            .ToList();
        var texto = Texto(tipo, pedido);
        var avisos = destinatarios.Select(id => new Aviso
        {
            IdLocal = pedido.IdLocal,
            IdUsuario = id,
            Tipo = tipo,
            IdPedidoTienda = pedido.Id,
            Texto = texto,
            Creado = ahora
        }).ToList();
        var push = destinatarios
            .Where(id => QuierePush(tipo, preferencias.FirstOrDefault(p => p.IdUsuario == id && p.IdLocal == pedido.IdLocal)))
            .ToList();
        return new AvisosPedido(avisos, push, Titulo(tipo), texto);
    }

    public static bool QuierePush(string tipo, PreferenciaAviso? preferencia)
    {
        var p = preferencia ?? new PreferenciaAviso();
        return tipo == PedidoNuevo ? p.PushNuevos : p.PushCancelados;
    }

    public static void MarcarLeido(Aviso aviso, DateTimeOffset ahora) => aviso.Leido ??= ahora;

    public static string Titulo(string tipo) => tipo switch
    {
        PedidoNuevo => "Pedido nuevo en la tienda",
        PedidoCancelado => "Pedido cancelado",
        _ => throw new ArgumentOutOfRangeException(nameof(tipo))
    };

    public static string Texto(string tipo, PedidoTienda pedido) => tipo switch
    {
        PedidoNuevo => $"N° {pedido.Numero} de {pedido.Nombre} por $ {pedido.Total.ToString("N2", Ar)}",
        PedidoCancelado => $"{pedido.Nombre} canceló el pedido N° {pedido.Numero}",
        _ => throw new ArgumentOutOfRangeException(nameof(tipo))
    };
}
