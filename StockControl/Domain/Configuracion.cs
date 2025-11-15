using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StockControl.Domain
{
    public class Configuracion
    {
        public int Id { get; set; }
        public string Clave { get; set; }
        public string Valor { get; set; }

        public Configuracion() { }
    }
}
