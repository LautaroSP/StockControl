using System.ComponentModel.DataAnnotations;
using StockControl.Dominio;

namespace StockControl.Api.Contratos;

public class LoginPedido
{
    [Required, MaxLength(80)]
    public string Usuario { get; set; } = "";

    [Required, MinLength(4)]
    public string Clave { get; set; } = "";
}

public class ProductoPedido
{
    [Required, MaxLength(80)]
    public string Codigo { get; set; } = "";

    [Required, MaxLength(200)]
    public string Nombre { get; set; } = "";

    public decimal Cantidad { get; set; }
    public decimal Costo { get; set; }
    public decimal Precio { get; set; }
    public bool ProductoSector { get; set; }
    public int? IdGrupoProducto { get; set; }

    /// <summary>null no la toca; 0 la saca (hereda la del grupo).</summary>
    [Range(0, int.MaxValue)]
    public int? IdCategoria { get; set; }
}

public class GrupoPedido
{
    [Required, MaxLength(200)]
    public string NombreGrupo { get; set; } = "";

    public decimal Costo { get; set; }
    public decimal PrecioGrupo { get; set; }
    public decimal Ganancia { get; set; }
    public bool GananciaIndividual { get; set; }

    /// <summary>null no la toca; 0 la saca.</summary>
    [Range(0, int.MaxValue)]
    public int? IdCategoria { get; set; }
}

public class CategoriaPedido
{
    [Required, MaxLength(40)]
    public string Nombre { get; set; } = "";
}

public class MiembrosGrupoPedido
{
    public List<int> IdsProducto { get; set; } = new();
}

public class StockPedido
{
    public decimal Cantidad { get; set; }
}

public class VentaPedido
{
    [Required, MinLength(1)]
    public List<ItemVentaPedido> Items { get; set; } = new();

    [MaxLength(80)]
    public string MetodoPago { get; set; } = "";

    public List<PagoPedido> Pagos { get; set; } = new();

    [Range(0, 100)]
    public int DescuentoPorcentaje { get; set; }

    public bool CobrarAlCosto { get; set; }

    public int? IdPedidoEspera { get; set; }

    public bool SinOfertas { get; set; }
}

public class PedidoEsperaPedido
{
    [Required, MinLength(1)]
    public List<ItemVentaPedido> Items { get; set; } = new();

    [MaxLength(80)]
    public string Nota { get; set; } = "";

    [Range(0, 100)]
    public int DescuentoPorcentaje { get; set; }

    public bool AlCosto { get; set; }
}

public class ClientePedido
{
    [Required, MaxLength(80)]
    public string Nombre { get; set; } = "";

    [MaxLength(30)]
    public string? Telefono { get; set; }
}

public class CuentaPedido
{
    [Required, MinLength(1)]
    public List<ItemVentaPedido> Items { get; set; } = new();

    [Range(0, 100)]
    public int DescuentoPorcentaje { get; set; }

    public bool CobrarAlCosto { get; set; }

    public bool SinOfertas { get; set; }

    public int? IdCliente { get; set; }

    public ClientePedido? ClienteNuevo { get; set; }
}

public class PagoCuentaPedido
{
    [Range(0.01, 999999999)]
    public decimal Importe { get; set; }

    public int IdMetodoPago { get; set; }
}

public class OfertaPedido
{
    [Range(1, int.MaxValue)]
    public int IdProducto { get; set; }

    [Required, MaxLength(20)]
    public string Tipo { get; set; } = "";

    public int Porcentaje { get; set; }

    public int Lleva { get; set; }

    public int Paga { get; set; }

    [Range(0, 999999999)]
    public decimal PrecioUnidad { get; set; }

    public DateOnly FechaDesde { get; set; }

    public DateOnly FechaHasta { get; set; }

    public bool SinFin { get; set; }

    public bool AnulaConDescuentoCaja { get; set; }

    public bool AnulaConAlCosto { get; set; } = true;

    public bool PermiteNoAplicarEnCaja { get; set; }
}

public class ComboPedido
{
    [Required, MaxLength(60)]
    public string Nombre { get; set; } = "";

    [Range(0, 999999999)]
    public decimal PrecioTotal { get; set; }

    public DateOnly FechaDesde { get; set; }

    public DateOnly FechaHasta { get; set; }

    public bool SinFin { get; set; }

    [Required, MaxLength(20)]
    public List<ComboLineaPedido> Lineas { get; set; } = [];
}

public class ComboLineaPedido
{
    [Range(1, int.MaxValue)]
    public int IdProducto { get; set; }

    [Range(1, 100)]
    public int Cantidad { get; set; }
}

public class PrecioProductoPedido
{
    [Range(0, 999999999)]
    public decimal Precio { get; set; }
}

public class UsuarioPedido
{
    [Required, MaxLength(120)]
    public string Nombre { get; set; } = "";

    [Required, MaxLength(80)]
    public string NombreUsuario { get; set; } = "";

    [MaxLength(20)]
    public string Rol { get; set; } = Roles.Empleado;

    public List<int> IdsLocal { get; set; } = new();

    [MinLength(4), MaxLength(200)]
    public string? Clave { get; set; }
}

public class ResetearClavePedido
{
    [Required, MinLength(4), MaxLength(200)]
    public string Clave { get; set; } = "";
}

public class ContratarLocalPedido
{
    public string Nombre { get; set; } = "";
}

public class AltaLocalAdminPedido
{
    public string Nombre { get; set; } = "";
    public int IdDueno { get; set; }
    public string NombreDueno { get; set; } = "";
    public string Email { get; set; } = "";
    public string NombreUsuario { get; set; } = "";
    public string ClaveDueno { get; set; } = "";
    public int IdLocalOrigen { get; set; }
    public bool CopiarStock { get; set; }
}

public class PagoAdminPedido
{
    public string Clave { get; set; } = "";
    public string Canal { get; set; } = "";
}

public class VencimientoAdminPedido
{
    public string Vence { get; set; } = "";
    public string Clave { get; set; } = "";
}

public class TelefonoVentasPedido
{
    public string Telefono { get; set; } = "";
    public string Email { get; set; } = "";
    public string Clave { get; set; } = "";
}

public class MensajeContactoPedido
{
    [MaxLength(120)]
    public string Nombre { get; set; } = "";

    [MaxLength(200)]
    public string Correo { get; set; } = "";

    [MaxLength(2000)]
    public string Mensaje { get; set; } = "";

    [MaxLength(200)]
    public string? Sitio { get; set; }

    [MaxLength(400)]
    public string? Apertura { get; set; }
}

public class FormularioContactoPedido
{
    public bool Activo { get; set; }
    public string Clave { get; set; } = "";
}

public class ClaveAdminPedido
{
    public string Clave { get; set; } = "";
}

public class CambioClaveAdminPedido
{
    public string Clave { get; set; } = "";
    public string ClaveSecreta { get; set; } = "";
}

public class ConfiguracionLocalPedido
{
    [Required, MaxLength(200)]
    public string NombreLocal { get; set; } = "";

    [Range(0.0001, 999999)]
    public decimal FactorGanancia { get; set; }

    [Range(0.0001, 999999)]
    public decimal FactorGananciaSector { get; set; } = 1;

    [Range(0.0001, 999999)]
    public decimal FactorGananciaGenerico { get; set; } = 1;

    [Range(0, 999999)]
    public decimal Iva { get; set; }

    public bool StockRigido { get; set; }

    [Range(0, 999999)]
    public decimal UmbralStockBajo { get; set; }

    public bool EmpleadoPuedeModificarPrecios { get; set; }

    public bool EmpleadoPuedeFiar { get; set; }

    [Required, MaxLength(10)]
    public string FormatoTicket { get; set; } = "POS-80";

    public bool ImprimirTicketAlCobrar { get; set; }

    [Range(1, 50)]
    public int CantidadCajas { get; set; } = 1;

    public bool RecalcularPrecios { get; set; }
}

public class PagoPedido
{
    public int IdMetodoPago { get; set; }

    [Range(0.01, 999999999)]
    public decimal Importe { get; set; }
}

public class MetodoPagoPedido
{
    [Required, MaxLength(80)]
    public string Descripcion { get; set; } = "";

    [MaxLength(20)]
    public string Integracion { get; set; } = IntegracionPago.Manual;
}

public class LectorMercadoPagoPedido
{
    [MaxLength(80)]
    public string IdLector { get; set; } = "";
}

public class ItemVentaPedido
{
    public int IdProducto { get; set; }

    [Required]
    public string Codigo { get; set; } = "";

    public string Nombre { get; set; } = "";

    [Range(0.001, 999999)]
    public decimal Cantidad { get; set; }

    public decimal PrecioUnitario { get; set; }
}

public class CerrarCajaPedido
{
    public DateOnly? Fecha { get; set; }

    /// <summary>medio | usuario</summary>
    public string Desglose { get; set; } = "medio";

    public bool Todas { get; set; }
}

public class CantidadCajasPedido
{
    [Range(1, 50)]
    public int Cantidad { get; set; } = 1;
}

public class UnificarCajasPedido
{
    [Required, MinLength(2)]
    public List<int> IdsCierre { get; set; } = new();
}

public class CartelesPedido
{
    [Required, MinLength(1), MaxLength(300)]
    public List<int> IdsProducto { get; set; } = new();

    [Required, MaxLength(10)]
    public string Tamano { get; set; } = "grande";
}

public class PlantillaCartelPedido
{
    /// <summary>Base64 de la imagen (acepta el prefijo data:image/...;base64,).</summary>
    [Required, MaxLength(1_500_000)]
    public string Imagen { get; set; } = "";
}

public class ProveedorPedido
{
    [Required, MaxLength(200)]
    public string Nombre { get; set; } = "";

    [MaxLength(200)]
    public string TipoProductos { get; set; } = "";

    [MaxLength(30)]
    public string Telefono { get; set; } = "";
}

public class ArchivoExcelPedido
{
    /// <summary>Base64 del .xlsx (acepta el prefijo data:...;base64,).</summary>
    [Required, MaxLength(3_000_000)]
    public string Archivo { get; set; } = "";
}

public class FilaCatalogoPedido
{
    public int Fila { get; set; }

    [MaxLength(80)]
    public string Codigo { get; set; } = "";

    [MaxLength(200)]
    public string Nombre { get; set; } = "";

    public decimal? Costo { get; set; }
    public decimal? Cantidad { get; set; }
}

public class ConfirmarCatalogoPedido
{
    [Required, MaxLength(2000)]
    public List<FilaCatalogoPedido> Filas { get; set; } = new();
}

public class LineaOrdenCompraPedido
{
    public int IdProducto { get; set; }

    [Range(0.001, 999999)]
    public decimal Cantidad { get; set; }

    [MaxLength(30)]
    public string NombrePack { get; set; } = "";

    [Range(1, 10000)]
    public int UnidadesPack { get; set; } = 1;
}

public class PackProductoPedido
{
    [MaxLength(30)]
    public string NombrePack { get; set; } = "";

    [Range(1, 10000)]
    public int UnidadesPack { get; set; } = 1;
}

public class LineaRecibidaPedido
{
    public int IdLinea { get; set; }

    [Range(0, 999999)]
    public decimal Cantidad { get; set; }
}

public class RecibirOrdenPedido
{
    [MaxLength(500)]
    public List<LineaRecibidaPedido> Lineas { get; set; } = new();
}

public class OrdenCompraPedido
{
    [Required, MinLength(1), MaxLength(500)]
    public List<LineaOrdenCompraPedido> Lineas { get; set; } = new();
}

public class ConfirmarCorreoPedido
{
    [MaxLength(128)]
    public string Token { get; set; } = "";
}

public class RegistroCuentaPedido : RegistroPedido
{
    [Required, MaxLength(200)]
    public string Clave { get; set; } = "";

    /// <summary>Campo oculto contra bots: una persona lo deja vacío.</summary>
    public string? Sitio { get; set; }

    [MaxLength(2048)]
    public string? Captcha { get; set; }
}

public class DebitoPedido
{
    [Required, MaxLength(200)]
    public string MailPagador { get; set; } = "";
}

public class TiendaPedido
{
    [MaxLength(ServicioTienda.LargoMaximoAlias)]
    public string? Alias { get; set; }

    [Required, MaxLength(20)]
    public string Estado { get; set; } = ServicioTienda.Inactiva;

    [MaxLength(ServicioTienda.LargoMaximoDato)]
    public string? Direccion { get; set; }

    [MaxLength(ServicioTienda.LargoMaximoDato)]
    public string? Horario { get; set; }

    [MaxLength(30)]
    public string? WhatsApp { get; set; }

    public bool HaceEnvios { get; set; }

    [Range(-90, 90)]
    public decimal? Latitud { get; set; }

    [Range(-180, 180)]
    public decimal? Longitud { get; set; }
}

public class PublicarProductosPedido
{
    [Required, MinLength(1), MaxLength(ServicioCatalogoTienda.MaximoPorVez)]
    public List<int> Ids { get; set; } = new();

    public bool Publicado { get; set; }
}

public class ImagenProductoPedido
{
    /// <summary>Base64 de la imagen (acepta el prefijo data:image/...;base64,).</summary>
    [Required]
    public string Imagen { get; set; } = "";

    /// <summary>Dirección de la propuesta que el dueño descartó al subir la propia.</summary>
    [MaxLength(500)]
    public string? Rechazada { get; set; }
}

public class AceptarImagenPedido
{
    [Range(1, ServicioImagenesTienda.MaximoPorProducto)]
    public int Orden { get; set; } = 1;
}

public class CarritoTiendaPedido
{
    [Required, MinLength(1), MaxLength(ServicioPedidosTienda.MaximoLineas)]
    public List<ItemPedidoTiendaPedido> Items { get; set; } = new();
}

/// <summary>Pedido de la tienda pública. Sin importes: los pone el servidor.</summary>
public class PedidoTiendaPedido
{
    [Required, MaxLength(ServicioPedidosTienda.LargoMaximoNombre)]
    public string Nombre { get; set; } = "";

    [Required, MaxLength(30)]
    public string Telefono { get; set; } = "";

    [Required, MaxLength(ServicioPedidosTienda.LargoMaximoEmail)]
    public string Email { get; set; } = "";

    [Required, MinLength(1), MaxLength(ServicioPedidosTienda.MaximoLineas)]
    public List<ItemPedidoTiendaPedido> Items { get; set; } = new();

    /// <summary>Campo oculto contra bots: una persona lo deja vacío.</summary>
    [MaxLength(200)]
    public string? Sitio { get; set; }
}

public class ItemPedidoTiendaPedido
{
    public int IdProducto { get; set; }

    [Range(1, ServicioPedidosTienda.MaximoCantidad)]
    public int Cantidad { get; set; }
}
