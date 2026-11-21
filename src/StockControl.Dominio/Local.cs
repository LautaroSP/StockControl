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
    /// <summary>Botones, chips, enlaces y precio de la tienda. Null: verde de StockControl.</summary>
    public string? ColorMarca { get; set; }
    /// <summary>Fondo de la página de la tienda. Null: crema de StockControl.</summary>
    public string? ColorFondo { get; set; }
    /// <summary>Fondo de la ficha de producto. Null: crema clara. El texto de la ficha contrasta con este color.</summary>
    public string? ColorFicha { get; set; }
    /// <summary>Archivo del logo, ya achicado. Null: la ficha de datos no muestra logo.</summary>
    public string? LogoTienda { get; set; }
    public string EstadoTienda { get; set; } = ServicioTienda.Inactiva;
    public string Direccion { get; set; } = "";
    public string Horario { get; set; } = "";
    public string WhatsApp { get; set; } = "";
    /// <summary>Mail de contacto de la tienda. Vacío: no se muestra.</summary>
    public string Correo { get; set; } = "";
    /// <summary>Usuario o link de Instagram. Vacío: no se muestra.</summary>
    public string Instagram { get; set; } = "";
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
    /// <summary>Si es falso, el débito no suma otro mes. Un pago nuevo lo vuelve a prender.</summary>
    public bool Renueva { get; set; } = true;
    /// <summary>Lo marca el admin: los pedidos de la tienda no pagan gestión.</summary>
    public bool ExentoGestion { get; set; }
    /// <summary>Pesos de gestión por pedido. Lo fija el admin; por defecto ServicioGestionTienda.ImportePorDefecto.</summary>
    public decimal ImporteGestion { get; set; } = ServicioGestionTienda.ImportePorDefecto;
    /// <summary>Lo elige el dueño: la gestión se suma al total del comprador o sale de lo que cobra el local.</summary>
    public bool GestionACargoComprador { get; set; }
    /// <summary>chips = pocas categorías; menu = lateral con subcategorías (estilo Mercado Libre).</summary>
    public string FiltroCategoriasTienda { get; set; } = ServicioTienda.FiltroChips;
    /// <summary>Versión de los términos de la tienda online aceptados por el dueño.</summary>
    public string VersionTerminosTienda { get; set; } = "";
    public DateTimeOffset? AceptoTerminosTiendaEn { get; set; }
    /// <summary>CBU o CVU para la transferencia del comprador. Con el alias alcanza uno de los dos.</summary>
    public string CbuTienda { get; set; } = "";
    public string AliasCbuTienda { get; set; } = "";
    public string TitularTienda { get; set; } = "";
    /// <summary>Horas que tiene el local para confirmar la transferencia. Null: sin vencimiento.</summary>
    public int? PlazoTransferenciaHoras { get; set; } = ServicioTransferenciaTienda.PlazoInicialHoras;
}
