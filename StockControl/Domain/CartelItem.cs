using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace StockControl.Domain
{
    public class CartelItem : INotifyPropertyChanged
    {
        private bool _seleccionado;
        private string _codigo = string.Empty;
        private string _nombre = string.Empty;
        private decimal _precio;

        public bool Seleccionado
        {
            get => _seleccionado;
            set { if (_seleccionado != value) { _seleccionado = value; OnPropertyChanged(); } }
        }

        public string Codigo
        {
            get => _codigo;
            set { if (_codigo != value) { _codigo = value; OnPropertyChanged(); } }
        }

        public string Nombre
        {
            get => _nombre;
            set { if (_nombre != value) { _nombre = value; OnPropertyChanged(); } }
        }

        public decimal Precio
        {
            get => _precio;
            set { if (_precio != value) { _precio = value; OnPropertyChanged(); } }
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        private void OnPropertyChanged([CallerMemberName] string? name = null) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
