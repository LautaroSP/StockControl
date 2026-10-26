using StockControl.Domain;
using StockControl.Repository;
using System.Globalization;

namespace StockControl
{
    public class frmVistaCliente : Form
    {
        private const string ClaveMonitor = "VistaClienteMonitor";
        private static readonly CultureInfo Cultura = new("es-AR");

        private readonly string _nombreLocal;
        private readonly ConfiguracionRepository _configuracion;
        private readonly Label lblLocal = new();
        private readonly Label lblCentro = new();
        private readonly Panel panelCarrito = new();
        private readonly DataGridView gridLineas = new();
        private readonly Label lblDescuento = new();
        private readonly Label lblTotal = new();
        private readonly System.Windows.Forms.Timer timerGracias = new() { Interval = 5000 };
        private readonly System.Windows.Forms.Timer timerResaltado = new() { Interval = 3000 };

        private CarritoSesion? _carritoMostrado;
        private List<decimal> _cantidades = new();
        private int _lineaResaltada = -1;
        private bool _enGracias;
        private int _cambiosEnGracias;
        private Action? _pendiente;

        public frmVistaCliente(string nombreLocal, ConfiguracionRepository configuracion)
        {
            _nombreLocal = nombreLocal;
            _configuracion = configuracion;
            ArmarPantalla();
            timerGracias.Tick += (_, _) => TerminarGracias();
            timerResaltado.Tick += (_, _) => QuitarResaltado();
            MostrarEspera();
        }

        public void Mostrar(CarritoSesion carrito, bool alCosto, int descuento, decimal total)
        {
            if (_enGracias)
            {
                _pendiente = () => Mostrar(carrito, alCosto, descuento, total);
                if (++_cambiosEnGracias < 2 || carrito.Items.Count == 0)
                    return;
                TerminarGracias();
                return;
            }

            MarcarUltimaLinea(carrito);
            if (carrito.Items.Count == 0)
            {
                MostrarEspera();
                return;
            }

            gridLineas.Rows.Clear();
            foreach (var item in carrito.Items.Reverse())
                gridLineas.Rows.Add(item.Nombre, item.Cantidad.ToString("0.##", Cultura), Dinero(item.Precio), Dinero(item.Subtotal));
            PintarResaltado();

            lblDescuento.Text = descuento > 0 && !alCosto ? $"Descuento {descuento} %" : string.Empty;
            gridLineas.ClearSelection();
            lblTotal.Text = $"Total  {Dinero(total)}";
            panelCarrito.Visible = true;
            lblCentro.Visible = false;
        }

        public void MostrarGracias(decimal total)
        {
            _enGracias = true;
            _cambiosEnGracias = 0;
            _pendiente = null;
            panelCarrito.Visible = false;
            lblCentro.Text = $"¡Gracias!\n{Dinero(total)}";
            lblCentro.Visible = true;
            timerGracias.Stop();
            timerGracias.Start();
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            var guardado = _configuracion.ObtenerPorClave(ClaveMonitor);
            var pantallas = Screen.AllScreens;
            if (pantallas.Length < 2)
                return;
            var pantalla = pantallas.FirstOrDefault(s => s.DeviceName == guardado)
                ?? pantallas.First(s => !s.Primary);
            CentrarEn(pantalla);
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            _configuracion.Insertar(ClaveMonitor, Screen.FromControl(this).DeviceName);
            timerGracias.Stop();
            timerResaltado.Stop();
            base.OnFormClosing(e);
        }

        private void ArmarPantalla()
        {
            Text = "Vista de cliente";
            BackColor = Color.White;
            ForeColor = Color.FromArgb(27, 36, 31);
            StartPosition = FormStartPosition.CenterScreen;
            Size = new Size(1000, 650);
            Icon = new Icon("Resources\\stockIcon.ico");

            lblLocal.Text = _nombreLocal;
            lblLocal.Dock = DockStyle.Top;
            lblLocal.Height = 70;
            lblLocal.Font = new Font("Segoe UI", 28F, FontStyle.Bold);
            lblLocal.Padding = new Padding(24, 12, 0, 0);

            lblCentro.Dock = DockStyle.Fill;
            lblCentro.TextAlign = ContentAlignment.MiddleCenter;
            lblCentro.Font = new Font("Segoe UI", 56F, FontStyle.Bold);

            ArmarGrilla();

            lblDescuento.Dock = DockStyle.Bottom;
            lblDescuento.Height = 50;
            lblDescuento.Font = new Font("Segoe UI", 24F);
            lblDescuento.ForeColor = Color.FromArgb(196, 92, 26);
            lblDescuento.TextAlign = ContentAlignment.MiddleRight;
            lblDescuento.Padding = new Padding(0, 0, 24, 0);

            lblTotal.Dock = DockStyle.Bottom;
            lblTotal.Height = 120;
            lblTotal.Font = new Font("Segoe UI", 60F, FontStyle.Bold);
            lblTotal.TextAlign = ContentAlignment.MiddleRight;
            lblTotal.Padding = new Padding(0, 0, 24, 0);

            panelCarrito.Dock = DockStyle.Fill;
            panelCarrito.Controls.Add(gridLineas);
            panelCarrito.Controls.Add(lblDescuento);
            panelCarrito.Controls.Add(lblTotal);

            Controls.Add(panelCarrito);
            Controls.Add(lblCentro);
            Controls.Add(lblLocal);
        }

        private void ArmarGrilla()
        {
            gridLineas.Dock = DockStyle.Fill;
            gridLineas.ReadOnly = true;
            gridLineas.AllowUserToAddRows = false;
            gridLineas.AllowUserToDeleteRows = false;
            gridLineas.AllowUserToResizeRows = false;
            gridLineas.RowHeadersVisible = false;
            gridLineas.BorderStyle = BorderStyle.None;
            gridLineas.BackgroundColor = Color.White;
            gridLineas.CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal;
            gridLineas.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            gridLineas.Enabled = false;
            gridLineas.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            gridLineas.Font = new Font("Segoe UI", 20F);
            gridLineas.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 14F, FontStyle.Bold);
            gridLineas.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            gridLineas.RowTemplate.Height = 46;
            gridLineas.DefaultCellStyle.SelectionBackColor = Color.White;
            gridLineas.DefaultCellStyle.SelectionForeColor = ForeColor;
            gridLineas.EnableHeadersVisualStyles = false;

            gridLineas.Columns.Add("Producto", "Producto");
            gridLineas.Columns.Add("Cantidad", "Cant.");
            gridLineas.Columns.Add("Precio", "Precio");
            gridLineas.Columns.Add("Subtotal", "Subtotal");
            gridLineas.Columns["Producto"]!.FillWeight = 50;
            foreach (var nombre in new[] { "Cantidad", "Precio", "Subtotal" })
            {
                gridLineas.Columns[nombre]!.FillWeight = nombre == "Cantidad" ? 12 : 19;
                gridLineas.Columns[nombre]!.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
            }
        }

        private void MarcarUltimaLinea(CarritoSesion carrito)
        {
            var actual = carrito.Items.Select(i => i.Cantidad).ToList();
            if (!ReferenceEquals(carrito, _carritoMostrado))
            {
                _carritoMostrado = carrito;
                _lineaResaltada = -1;
            }
            else if (actual.Count >= _cantidades.Count)
            {
                int cambiada = Enumerable.Range(0, actual.Count)
                    .LastOrDefault(i => i >= _cantidades.Count || actual[i] != _cantidades[i], -1);
                if (cambiada >= 0)
                {
                    _lineaResaltada = cambiada;
                    timerResaltado.Stop();
                    timerResaltado.Start();
                }
            }
            _cantidades = actual;
        }

        private void PintarResaltado()
        {
            int filaResaltada = _lineaResaltada < 0 ? -1 : gridLineas.Rows.Count - 1 - _lineaResaltada;
            for (int i = 0; i < gridLineas.Rows.Count; i++)
            {
                var color = i == filaResaltada ? Color.FromArgb(229, 243, 234) : Color.White;
                gridLineas.Rows[i].DefaultCellStyle.BackColor = color;
                gridLineas.Rows[i].DefaultCellStyle.SelectionBackColor = color;
            }
        }

        private void QuitarResaltado()
        {
            timerResaltado.Stop();
            _lineaResaltada = -1;
            PintarResaltado();
        }

        private void MostrarEspera()
        {
            panelCarrito.Visible = false;
            lblCentro.Text = _nombreLocal;
            lblCentro.Visible = true;
        }

        private void TerminarGracias()
        {
            timerGracias.Stop();
            _enGracias = false;
            var pendiente = _pendiente;
            _pendiente = null;
            if (pendiente != null)
                pendiente();
            else
                MostrarEspera();
        }

        private void CentrarEn(Screen pantalla)
        {
            var area = pantalla.WorkingArea;
            int ancho = Math.Min(Width, area.Width);
            int alto = Math.Min(Height, area.Height);
            Bounds = new Rectangle(area.X + (area.Width - ancho) / 2, area.Y + (area.Height - alto) / 2, ancho, alto);
        }

        private static string Dinero(decimal valor) => valor.ToString("C2", Cultura);
    }
}
