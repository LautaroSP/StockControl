using System.ComponentModel;

namespace StockControl.Domain
{
    public class ProductoDTO : INotifyPropertyChanged
    {
        private int _idGrupo;
        private string _nombreGrupo;
        private bool _seleccionado;
        public bool Seleccionado
        {
            get => _seleccionado;
            set
            {
                if (_seleccionado != value)
                {
                    _seleccionado = value;
                    OnPropertyChanged(nameof(Seleccionado));
                }
            }
        }
        public int IdGrupo
        {
            get => _idGrupo;
            set
            {
                if (_idGrupo != value)
                {
                    _idGrupo = value;
                    OnPropertyChanged(nameof(IdGrupo));
                }
            }
        }

        public string NombreGrupo
        {
            get => _nombreGrupo;
            set
            {
                if (_nombreGrupo != value)
                {
                    _nombreGrupo = value;
                    OnPropertyChanged(nameof(NombreGrupo));
                }
            }
        }
        public int IdProducto { get; set; }
        public string NombreProducto { get; set; } = string.Empty;
        public string CodigoProducto {  get; set; } = string.Empty;

        public ProductoDTO(bool seleccionado, int idProducto, string nombreProd, int idGrupo, string nombreGrupo, string CodigoProd)
        {
            _seleccionado = seleccionado;
            IdProducto = idProducto;
            NombreProducto = nombreProd;
            IdGrupo = idGrupo;
            NombreGrupo = nombreGrupo;
            CodigoProducto = CodigoProd;

        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}

