using System.Text.RegularExpressions;

namespace StockControl.Dominio;

/// <summary>El comprador transfiere al CBU o alias del local, y el dueño o el socio la confirman a mano.</summary>
public static class ServicioTransferenciaTienda
{
    public const int LargoCbu = 22;
    public const int LargoMaximoAlias = 20;
    public const int LargoMaximoTitular = 80;
    public const int PlazoInicialHoras = 24;
    public const int PlazoMaximoHoras = 168;

    private static readonly Regex FormatoAlias = new(@"^[A-Z0-9.\-]{6,20}$", RegexOptions.Compiled);
    private static readonly int[] PesosBanco = [7, 1, 3, 9, 7, 1, 3];
    private static readonly int[] PesosCuenta = [3, 9, 7, 1, 3, 9, 7, 1, 3, 9, 7, 1, 3];

    /// <summary>Sin CBU o alias, o sin titular, la tienda no ofrece transferencia.</summary>
    public static bool Acepta(Local local) =>
        (local.CbuTienda.Length > 0 || local.AliasCbuTienda.Length > 0) && local.TitularTienda.Length > 0;

    public static void AsignarDatos(Local local, string? cbu, string? alias, string? titular, int? plazoHoras)
    {
        var cuenta = NormalizarCbu(cbu);
        var aliasCbu = NormalizarAlias(alias);
        var nombre = (titular ?? "").Trim();
        if (nombre.Length > LargoMaximoTitular)
            throw new ErrorNegocio($"El titular admite hasta {LargoMaximoTitular} caracteres.");
        if ((cuenta.Length > 0 || aliasCbu.Length > 0) && nombre.Length == 0)
            throw new ErrorNegocio("Cargá el titular de la cuenta para recibir transferencias.");
        if (plazoHoras is < 1 or > PlazoMaximoHoras)
            throw new ErrorNegocio($"El plazo para confirmar va de 1 a {PlazoMaximoHoras} horas, o sin vencimiento.");

        local.CbuTienda = cuenta;
        local.AliasCbuTienda = aliasCbu;
        local.TitularTienda = nombre;
        local.PlazoTransferenciaHoras = plazoHoras;
    }

    /// <summary>CBU o CVU: 22 dígitos con los dos dígitos verificadores del BCRA. Vacío: sin CBU.</summary>
    public static string NormalizarCbu(string? cbu)
    {
        var valor = (cbu ?? "").Replace(" ", "").Replace("-", "");
        if (valor.Length == 0) return "";
        if (valor.Length != LargoCbu || !valor.All(char.IsAsciiDigit)
            || !Verifica(valor[..8], PesosBanco) || !Verifica(valor[8..], PesosCuenta))
            throw new ErrorNegocio("El CBU o CVU no es válido. Son 22 números.");
        return valor;
    }

    /// <summary>De 6 a 20 caracteres: letras, números, punto y guion. Vacío: sin alias.</summary>
    public static string NormalizarAlias(string? alias)
    {
        var valor = (alias ?? "").Trim().ToUpperInvariant();
        if (valor.Length == 0) return "";
        if (!FormatoAlias.IsMatch(valor))
            throw new ErrorNegocio("El alias va de 6 a 20 caracteres: letras sin acento, números, punto y guion.");
        return valor;
    }

    public static DateTimeOffset? VencePago(Local local, DateTimeOffset creado) =>
        local.PlazoTransferenciaHoras is int horas ? creado.AddHours(horas) : null;

    public static bool EsperaConfirmacion(PedidoTienda pedido) =>
        pedido.MedioPago == ServicioPagoPedidoTienda.Transferencia && pedido.EstadoPago == ServicioPagoPedidoTienda.Pendiente;

    /// <summary>El local vio la plata en su cuenta. Desde acá la gestión queda como deuda del local.</summary>
    public static void Confirmar(PedidoTienda pedido, DateTimeOffset ahora)
    {
        if (!EsperaConfirmacion(pedido) || pedido.Estado != ServicioPedidosTienda.Nuevo)
            throw new ErrorNegocio("El pedido no espera una transferencia.");
        if (ServicioPagoPedidoTienda.Vencido(pedido, ahora))
            throw new ErrorNegocio("Venció el plazo para confirmar la transferencia.");
        pedido.EstadoPago = ServicioPagoPedidoTienda.Pagado;
        pedido.Pagado = ahora;
        pedido.Actualizado = ahora;
    }

    /// <summary>El local ya le devolvió la transferencia al comprador. No deja deuda de gestión.</summary>
    public static void CancelarConfirmada(PedidoTienda pedido, DateTimeOffset ahora)
    {
        if (pedido.MedioPago != ServicioPagoPedidoTienda.Transferencia || pedido.EstadoPago != ServicioPagoPedidoTienda.Pagado)
            throw new ErrorNegocio("El pedido no tiene una transferencia confirmada.");
        ServicioPedidosTienda.Anular(pedido, ahora);
        pedido.EstadoPago = ServicioPagoPedidoTienda.Reembolsado;
    }

    private static bool Verifica(string bloque, int[] pesos)
    {
        var suma = pesos.Select((peso, i) => peso * (bloque[i] - '0')).Sum();
        return (10 - suma % 10) % 10 == bloque[^1] - '0';
    }
}
