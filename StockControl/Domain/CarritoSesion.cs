using System.ComponentModel;

namespace StockControl.Domain
{
    public class CarritoSesion
    {
        public BindingList<ItemSeleccionado> Items { get; set; } = new();
        public bool CobrarAlCosto { get; set; }
        public bool DescuentoActivo { get; set; }
        public string DescuentoTexto { get; set; } = string.Empty;
        public string MetodoPago { get; set; } = string.Empty;
        public bool PagoMultiple { get; set; }
        public List<MetodoDePago> MultiplesMetodos { get; set; } = new();
        public bool ImprimirTicket { get; set; } = true;
    }
}
