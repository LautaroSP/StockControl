using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StockControl.Domain
{
    public class InformeVentaDetalle
    {
        public int IdInformeVentaDetalle { get; set; }
        public int IdInformeVenta { get; set; }
        public string Codigo { get; set; } = string.Empty;
        public string Nombre { get; set; } = string.Empty;
        public decimal Cantidad { get; set; }
        public decimal Precio { get; set; }
        public decimal CostoVenta { get; set; }
        public decimal Subtotal { get; set; }
        public int EsSector { get; set; }
        public int EsGenerico { get; set; }
        public int ClasificacionRegistrada { get; set; }
        public InformeVentaDetalle()
        {

        }
        public InformeVentaDetalle(ItemSeleccionado item, int IdInformeVenta)
        {
            this.IdInformeVenta = IdInformeVenta;
            Codigo = item.Codigo;
            Nombre = item.Nombre;
            Cantidad = item.Cantidad;
            Precio = item.Precio;
            Subtotal = item.Subtotal;
        }
    }


}
