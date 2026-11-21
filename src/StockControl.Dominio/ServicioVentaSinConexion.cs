namespace StockControl.Dominio;

/// <summary>Reglas de las ventas cobradas sin conexión y mandadas después.</summary>
public static class ServicioVentaSinConexion
{
    public const int DiasMaximosAtras = 7;
    public const int LargoMaximoNota = 200;

    /// <summary>La venta que ya recibió el local con ese id, o null. Sirve para no duplicar.</summary>
    public static InformeVenta? Existente(IEnumerable<InformeVenta> ventas, int idLocal, Guid? idVentaCliente)
    {
        if (idVentaCliente == null || idVentaCliente == Guid.Empty) return null;
        return ventas.FirstOrDefault(v => v.IdLocal == idLocal && v.IdVentaCliente == idVentaCliente);
    }

    /// <summary>Fecha del cobro en la caja si no es futura ni de más de 7 días atrás; si no, la del servidor.</summary>
    public static DateTimeOffset FechaVenta(DateTimeOffset? fechaCobro, DateTimeOffset ahora)
    {
        if (fechaCobro == null) return ahora;
        if (fechaCobro > ahora) return ahora;
        if (fechaCobro < ahora.AddDays(-DiasMaximosAtras)) return ahora;
        return fechaCobro.Value;
    }

    /// <summary>Texto para el dueño cuando la venta entró con algo para revisar. Null si no hay nada.</summary>
    public static string? NotaRevision(bool stockEnConflicto, decimal? totalVisto, decimal totalServidor)
    {
        var partes = new List<string>();
        if (stockEnConflicto)
            partes.Add("Stock insuficiente al sincronizar.");
        if (totalVisto != null && totalVisto != totalServidor)
            partes.Add($"El cajero vio {totalVisto:0.##} y el servidor calculó {totalServidor:0.##}.");
        if (partes.Count == 0) return null;
        var nota = string.Join(" ", partes);
        return nota.Length > LargoMaximoNota ? nota[..LargoMaximoNota] : nota;
    }
}

