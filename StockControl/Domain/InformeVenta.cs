using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StockControl.Domain
{
    public class InformeVenta
    {
        public int IdInformeVenta { get; set; }
        public DateTime Fecha { get; set; } = DateTime.Now;
        public decimal Total { get; set; }
        public string MetodoPago { get; set; }
        public int MultipleMetodoDePago { get; set; } = 0; // 0 = No, 1 = Si
        public int DetalleAdjunto { get; set; }
        public string DetalleAdjuntoStr { get; set; } = string.Empty;
        public string MultipleMetodoDePagoStr { get; set; } = string.Empty;
        public decimal subTotal { get; set; } = 0;
        public decimal Descuento { get; set; } = 0;
        public string PrecioCosto { get; set; } = string.Empty;

    }
}
