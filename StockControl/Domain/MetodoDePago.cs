using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StockControl.Domain
{
    public class MetodoDePago
    {
        public string Descripcion { get; set; }
        public decimal Monto { get; set; } = 0;
    }
}
