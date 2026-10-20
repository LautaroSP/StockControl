using StockControl.Infrastructure;
using StockControl.Repository;
using System.Globalization;

namespace StockControl
{
    public partial class Configuracion : Form
    {
        private ConfiguracionRepository _configuracionRepository = new();
        private bool _CambioLocal = false;
        private bool _CambioGanancia = false;
        private bool _CambioIVA = false;
        private bool _CambioGananciaSector = false;
        private bool _CambioGananciaGenericos = false;
        private bool _CambioBdd = false;
        private bool _cargando = true;
        private string _rutaBddPendiente = string.Empty;
        private ProductoRepository _rprod = new ProductoRepository();

        public Configuracion()
        {
            InitializeComponent();
            CargarRutaBdd();
            CargarPorcentajes();
            _cargando = false;
        }
        public Configuracion(string nombreLocal, string factorGanancia, string IVA)
        {
            InitializeComponent();
            txtNombreLocal.Text = nombreLocal;
            txtFactorGanancia.Text = factorGanancia;
            txtIVA.Text = IVA;
            CargarRutaBdd();
            CargarPorcentajes();
            _cargando = false;
        }

        private void CargarPorcentajes()
        {
            txtGananciaSector.Text = _configuracionRepository.ObtenerPorClave("GananciaSector");
            txtGananciaGenericos.Text = _configuracionRepository.ObtenerPorClave("GananciaGenericos");
        }

        private void CargarRutaBdd()
        {
            _rutaBddPendiente = DbPath.CurrentPath;
            ActualizarTextoRutaBdd();
            _CambioBdd = false;
        }

        private void ActualizarTextoRutaBdd()
        {
            txtRutaBdd.Text = DbPath.IsDefaultPath(_rutaBddPendiente)
                ? $"Raíz ({DbPath.DefaultPath})"
                : _rutaBddPendiente;
        }

        private void btnExaminarBdd_Click(object? sender, EventArgs e)
        {
            using var dialog = new OpenFileDialog
            {
                Title = "Seleccionar base de datos",
                Filter = "Base de datos SQLite (*.db)|*.db|Todos los archivos (*.*)|*.*",
                CheckFileExists = false,
                CheckPathExists = true
            };

            if (!string.IsNullOrWhiteSpace(_rutaBddPendiente))
            {
                try
                {
                    dialog.InitialDirectory = Path.GetDirectoryName(_rutaBddPendiente);
                    dialog.FileName = Path.GetFileName(_rutaBddPendiente);
                }
                catch
                {
                    // ignore invalid pending path
                }
            }

            if (dialog.ShowDialog(this) != DialogResult.OK)
                return;

            string seleccion = Path.GetFullPath(dialog.FileName);
            if (!File.Exists(seleccion))
            {
                var crear = MessageBox.Show(
                    $"El archivo no existe:\n{seleccion}\n\n¿Desea crear una base de datos nueva en esa ubicación?",
                    "Base de datos",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question);
                if (crear != DialogResult.Yes)
                    return;
            }

            _rutaBddPendiente = seleccion;
            _CambioBdd = !string.Equals(
                Path.GetFullPath(_rutaBddPendiente),
                Path.GetFullPath(DbPath.CurrentPath),
                StringComparison.OrdinalIgnoreCase);
            ActualizarTextoRutaBdd();
        }

        private void btnUsarRaizBdd_Click(object? sender, EventArgs e)
        {
            _rutaBddPendiente = DbPath.DefaultPath;
            _CambioBdd = !DbPath.IsUsingDefault;
            ActualizarTextoRutaBdd();
        }

        private void btnGuardar_Click(object sender, EventArgs e)
        {
            if (txtNombreLocal.Text.Equals(string.Empty))
            {
                lblObliLocal.Visible = true;
                return;
            }
            if (_CambioLocal)
            {
                _configuracionRepository.Insertar("NombreLocal", txtNombreLocal.Text);
                this.DialogResult = DialogResult.OK;
                StockMain.nombreLocal = txtNombreLocal.Text;
            }
            if (_CambioGanancia)
            {
                if (TryParseDecimal(txtFactorGanancia.Text, out decimal result))
                {
                    _configuracionRepository.Insertar("FactorGanancia", txtFactorGanancia.Text);
                    this.DialogResult = DialogResult.OK;
                    StockMain.factorGanancia = txtFactorGanancia.Text;
                    StockMain._factorGanancia = result;
                }
                else
                    MessageBox.Show("El Factor de Ganancia debe ser un numero decimal", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            if (_CambioIVA)
            {
                if (TryParseDecimal(txtIVA.Text, out decimal result))
                {
                    _configuracionRepository.Insertar("IVA", txtIVA.Text);
                    this.DialogResult = DialogResult.OK;
                    StockMain.factorIVA = txtIVA.Text;
                    StockMain.IVA = result;
                }
                else
                    MessageBox.Show("El IVA debe ser un numero decimal", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            if (_CambioGananciaSector && !GuardarPorcentaje(txtGananciaSector, "GananciaSector", "La ganancia del sector", valor => StockMain.gananciaSector = valor))
                return;
            if (_CambioGananciaGenericos && !GuardarPorcentaje(txtGananciaGenericos, "GananciaGenericos", "La ganancia de genéricos", valor => StockMain.gananciaGenericos = valor))
                return;

            if (_CambioIVA || _CambioGanancia)
            {
                var result = MessageBox.Show($"Se actualizaron valores de ganancia o IVA,¿Desea actualizar los valores de los productos?",
                                            "Actualizacion de precio",
                                            MessageBoxButtons.YesNo,
                                            MessageBoxIcon.Information);

                if (result == DialogResult.Yes)
                {
                    Cursor = Cursors.WaitCursor;
                    var prods = _rprod.Listar();
                    prods = prods.Where(x => x.GananciaIndividual != 1).ToList();
                    foreach (var prod in prods)
                    {
                        prod.Precio = Math.Round(prod.Costo * StockMain._factorGanancia * StockMain.IVA, 2);
                        _rprod.Actualizar(prod);
                    }
                    Cursor = Cursors.Default;
                    MessageBox.Show("Finalizó la actualizacion de productos", "Actualizacion", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }

            if (_CambioBdd)
            {
                string destinoMostrar = DbPath.IsDefaultPath(_rutaBddPendiente)
                    ? $"Raíz ({DbPath.DefaultPath})"
                    : _rutaBddPendiente;

                var confirm = MessageBox.Show(
                    $"Se va a cambiar la base de datos a:\n{destinoMostrar}\n\nLa aplicación se reiniciará. ¿Continuar?",
                    "Cambiar base de datos",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Warning);

                if (confirm != DialogResult.Yes)
                    return;

                try
                {
                    // Vacío = raíz
                    DbPath.Save(DbPath.IsDefaultPath(_rutaBddPendiente) ? null : _rutaBddPendiente);
                    Application.Restart();
                    Environment.Exit(0);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"No se pudo guardar la ruta de la base de datos:\n{ex.Message}",
                        "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }
        private void btnSalir_Click(object sender, EventArgs e)
        {
            this.Close();
        }
        private void txtNombreLocal_Changed(object sender, EventArgs e)
        {
            if (txtNombreLocal.Text.Equals(string.Empty))
            {
                lblObliLocal.Visible = true;
                return;
            }
            _CambioLocal = true;
        }

        private void txtFactorGanancia_TextAlignChanged(object sender, EventArgs e)
        {

        }

        private void txtFactorGanancia_TextChanged(object sender, EventArgs e)
        {

            if (txtFactorGanancia.Text.Equals(string.Empty))
            {
                lblObliLocal.Visible = true;
                return;
            }
            _CambioGanancia = true;
        }
        private bool TryParseDecimal(string input, out decimal value)
        {
            input = input.Trim();

            input = input.Replace(',', '.');

            return decimal.TryParse(input, NumberStyles.Any, CultureInfo.InvariantCulture, out value);
        }

        private void txtIVA_TextChanged(object sender, EventArgs e)
        {
            _CambioIVA = true;
        }

        private void txtGananciaSector_TextChanged(object sender, EventArgs e)
        {
            if (!_cargando)
                _CambioGananciaSector = true;
        }

        private void txtGananciaGenericos_TextChanged(object sender, EventArgs e)
        {
            if (!_cargando)
                _CambioGananciaGenericos = true;
        }

        private bool GuardarPorcentaje(TextBox texto, string clave, string titulo, Action<decimal> aplicar)
        {
            if (!TryParsePorcentaje(texto.Text, out decimal valor))
            {
                MessageBox.Show($"{titulo} debe ser un porcentaje entre 0 y 100", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }

            _configuracionRepository.Insertar(clave, string.IsNullOrWhiteSpace(texto.Text) ? "0" : texto.Text.Trim());
            aplicar(valor);
            this.DialogResult = DialogResult.OK;
            return true;
        }

        private bool TryParsePorcentaje(string input, out decimal value)
        {
            if (string.IsNullOrWhiteSpace(input))
            {
                value = 0;
                return true;
            }

            return TryParseDecimal(input, out value) && value >= 0 && value <= 100;
        }
    }

}
