using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StockControl.Domain
{
    public class Producto
    {
        public int Id { get; set; }
        public string Codigo { get; set; }
        public string Nombre { get; set; }
        public decimal Cantidad { get; set; }
        public decimal Costo { get; set; }
        public decimal Precio { get; set; }
        public int ProductoSector { get; set; } 
        public string Sector { get; set; }
        public int GananciaIndividual { get; set; }
        public decimal ValorGanancia { get; set; }
        public int IdGrupoProducto { get; set; }
        public string NombreGrupo {  get; set; }
        public DateTime fechaModificacion { get; set; }
    }
}
