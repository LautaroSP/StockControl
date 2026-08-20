using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StockControl.Domain
{
    public class Caja
    {
        public int IdCaja { get; set; }
        public DateTime Fecha { get; set; } = DateTime.Now;
        public decimal Total { get; set; }
        public string MetodoPago { get; set; }
        public int CantidadVentas { get; set; }
    }
}
