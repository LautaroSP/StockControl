namespace StockControl.Dominio;

public class Local
{
    public int IdLocal { get; set; }
    public string Nombre { get; set; } = "";
    public bool Activo { get; set; } = true;
    public string EstadoAbono { get; set; } = "al_dia";
    public DateTimeOffset? Vence { get; set; }
    public string? IdSuscripcionMp { get; set; }
    public string EstadoSuscripcion { get; set; } = "";
    public string? AliasTienda { get; set; }
    public string EstadoTienda { get; set; } = ServicioTienda.Inactiva;
    public string Direccion { get; set; } = "";
    public string Horario { get; set; } = "";
    public string WhatsApp { get; set; } = "";
    public bool HaceEnvios { get; set; }
    public decimal? Latitud { get; set; }
    public decimal? Longitud { get; set; }
    /// <summary>Pesos por kilómetro. Null si el envío es gratis siempre o no está configurado.</summary>
    public decimal? PrecioEnvioKm { get; set; }
    public decimal? EnvioMinimo { get; set; }
    /// <summary>Kilómetros. Null: sin tope.</summary>
    public decimal? RadioEnvioKm { get; set; }
    /// <summary>Subtotal de productos desde el cual el envío sale gratis.</summary>
    public decimal? EnvioGratisDesde { get; set; }
    public bool EnvioGratisSiempre { get; set; }
    /// <summary>Lo marca el admin: los pedidos de la tienda no pagan gestión.</summary>
    public bool ExentoGestion { get; set; }
    /// <summary>Lo elige el dueño: la gestión se suma al total del comprador o sale de lo que cobra el local.</summary>
    public bool GestionACargoComprador { get; set; }
    /// <summary>CBU o CVU para la transferencia del comprador. Con el alias alcanza uno de los dos.</summary>
    public string CbuTienda { get; set; } = "";
    public string AliasCbuTienda { get; set; } = "";
    public string TitularTienda { get; set; } = "";
    /// <summary>Horas que tiene el local para confirmar la transferencia. Null: sin vencimiento.</summary>
    public int? PlazoTransferenciaHoras { get; set; } = ServicioTransferenciaTienda.PlazoInicialHoras;
}
