using StockControl.Domain;
using StockControl.Repository;
using System.Globalization;
using System.IO.Ports;
using System.Text;

namespace StockControl
{
    public class frmBalanza : Form
    {
        private const string ClavePuerto = "PuertoBalanza";
        private static readonly CultureInfo Cultura = new("es-AR");

        private readonly ProductoRepository _productos;
        private readonly ConfiguracionRepository _configuracion;
        private readonly List<Producto> _todos = new();
        private readonly List<ItemSeleccionado> _pesadas = new();
        private readonly StringBuilder _bufferSerie = new();

        private readonly TextBox txtBuscador = new();
        private readonly ListBox lstProductos = new();
        private readonly Label lblPesando = new();
        private readonly Label lblTotal = new();
        private readonly DataGridView gridCarrito = new();
        private readonly Label lblTotalCarrito = new();
        private readonly Button btnAceptar = new();
        private readonly Button btnSalir = new();
        private readonly Label lblAvisoPuerto = new();

        private SerialPort? _puerto;
        private decimal _peso;
        private Producto? _elegido;
        private bool _cerrando;

        public IReadOnlyList<ItemSeleccionado> Pesadas => _pesadas;

        public frmBalanza(ProductoRepository productos, ConfiguracionRepository configuracion)
        {
            _productos = productos;
            _configuracion = configuracion;
            ArmarPantalla();
            CargarProductos();
            KeyPreview = true;
            KeyDown += frmBalanza_KeyDown;
            Shown += (_, _) =>
            {
                ConectarBalanza();
                txtBuscador.Focus();
            };
            FormClosing += frmBalanza_FormClosing;
        }

        private void ArmarPantalla()
        {
            Text = "Modo balanza";
            WindowState = FormWindowState.Maximized;
            StartPosition = FormStartPosition.CenterParent;
            MinimizeBox = false;
            Font = new Font("Segoe UI", 10F);
            try { Icon = new Icon("Resources\\stockIcon.ico"); } catch { /* sin ícono */ }

            var raiz = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 3,
                RowCount = 1,
                Padding = new Padding(12)
            };
            raiz.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 32));
            raiz.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 43));
            raiz.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25));
            Controls.Add(raiz);

            var izquierda = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                RowCount = 3,
                ColumnCount = 1
            };
            izquierda.RowStyles.Add(new RowStyle(SizeType.Absolute, 24));
            izquierda.RowStyles.Add(new RowStyle(SizeType.Absolute, 36));
            izquierda.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            izquierda.Controls.Add(new Label { Text = "Buscador", Dock = DockStyle.Fill, Font = new Font("Segoe UI Semibold", 11F, FontStyle.Bold) }, 0, 0);
            txtBuscador.Dock = DockStyle.Fill;
            txtBuscador.TextChanged += (_, _) => Filtrar();
            txtBuscador.KeyDown += txtBuscador_KeyDown;
            izquierda.Controls.Add(txtBuscador, 0, 1);
            lstProductos.Dock = DockStyle.Fill;
            lstProductos.FormattingEnabled = true;
            lstProductos.IntegralHeight = false;
            lstProductos.Format += (_, e) =>
            {
                if (e.ListItem is Producto p)
                    e.Value = $"{p.Nombre}  ({p.PrecioPorKilo.ToString("C2", Cultura)}/kg)";
            };
            lstProductos.SelectedIndexChanged += (_, _) =>
            {
                _elegido = lstProductos.SelectedItem as Producto;
                ActualizarCentro();
            };
            lstProductos.Click += (_, _) => IntentarAgregar();
            lstProductos.KeyDown += (s, e) =>
            {
                if (e.KeyCode == Keys.Enter)
                {
                    IntentarAgregar();
                    e.Handled = true;
                    e.SuppressKeyPress = true;
                }
            };
            izquierda.Controls.Add(lstProductos, 0, 2);
            raiz.Controls.Add(izquierda, 0, 0);

            var centro = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                RowCount = 4,
                ColumnCount = 1,
                Padding = new Padding(20)
            };
            centro.RowStyles.Add(new RowStyle(SizeType.Percent, 30));
            centro.RowStyles.Add(new RowStyle(SizeType.Absolute, 80));
            centro.RowStyles.Add(new RowStyle(SizeType.Absolute, 60));
            centro.RowStyles.Add(new RowStyle(SizeType.Percent, 40));
            centro.Controls.Add(new Panel { Dock = DockStyle.Fill }, 0, 0);
            lblPesando.Dock = DockStyle.Fill;
            lblPesando.TextAlign = ContentAlignment.MiddleCenter;
            lblPesando.Font = new Font("Segoe UI Semibold", 36F, FontStyle.Bold);
            lblPesando.Text = "Pesando…";
            centro.Controls.Add(lblPesando, 0, 1);
            lblTotal.Dock = DockStyle.Fill;
            lblTotal.TextAlign = ContentAlignment.MiddleCenter;
            lblTotal.Font = new Font("Segoe UI", 22F, FontStyle.Bold);
            lblTotal.Text = "Total:";
            centro.Controls.Add(lblTotal, 0, 2);
            lblAvisoPuerto.Dock = DockStyle.Top;
            lblAvisoPuerto.TextAlign = ContentAlignment.MiddleCenter;
            lblAvisoPuerto.ForeColor = Color.DarkOrange;
            lblAvisoPuerto.Visible = false;
            centro.Controls.Add(lblAvisoPuerto, 0, 3);
            raiz.Controls.Add(centro, 1, 0);

            var derecha = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                RowCount = 4,
                ColumnCount = 1
            };
            derecha.RowStyles.Add(new RowStyle(SizeType.Absolute, 28));
            derecha.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            derecha.RowStyles.Add(new RowStyle(SizeType.Absolute, 28));
            derecha.RowStyles.Add(new RowStyle(SizeType.Absolute, 100));
            derecha.Controls.Add(new Label { Text = "Carrito", Dock = DockStyle.Fill, Font = new Font("Segoe UI Semibold", 11F, FontStyle.Bold) }, 0, 0);

            gridCarrito.Dock = DockStyle.Fill;
            gridCarrito.AllowUserToAddRows = false;
            gridCarrito.AllowUserToDeleteRows = false;
            gridCarrito.ReadOnly = true;
            gridCarrito.RowHeadersVisible = false;
            gridCarrito.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            gridCarrito.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            gridCarrito.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            gridCarrito.Columns.Add("Nombre", "Producto");
            gridCarrito.Columns.Add("Kilos", "Kg");
            gridCarrito.Columns.Add("Importe", "Importe");
            var colQuitar = new DataGridViewButtonColumn
            {
                Name = "Quitar",
                HeaderText = "",
                Text = "×",
                UseColumnTextForButtonValue = true,
                Width = 36,
                AutoSizeMode = DataGridViewAutoSizeColumnMode.None
            };
            gridCarrito.Columns.Add(colQuitar);
            gridCarrito.Columns["Kilos"]!.DefaultCellStyle.Format = "0.###";
            gridCarrito.Columns["Kilos"]!.DefaultCellStyle.FormatProvider = Cultura;
            gridCarrito.Columns["Importe"]!.DefaultCellStyle.Format = "C2";
            gridCarrito.Columns["Importe"]!.DefaultCellStyle.FormatProvider = Cultura;
            gridCarrito.CellContentClick += gridCarrito_CellContentClick;
            derecha.Controls.Add(gridCarrito, 0, 1);

            lblTotalCarrito.Dock = DockStyle.Fill;
            lblTotalCarrito.TextAlign = ContentAlignment.MiddleRight;
            lblTotalCarrito.Font = new Font("Segoe UI Semibold", 11F, FontStyle.Bold);
            lblTotalCarrito.Text = "Total: $ 0,00";
            derecha.Controls.Add(lblTotalCarrito, 0, 2);

            var botones = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                RowCount = 2,
                ColumnCount = 1,
                Padding = new Padding(0, 8, 0, 0)
            };
            botones.RowStyles.Add(new RowStyle(SizeType.Percent, 50));
            botones.RowStyles.Add(new RowStyle(SizeType.Percent, 50));
            btnAceptar.Dock = DockStyle.Fill;
            btnAceptar.Text = "Aceptar";
            btnAceptar.Font = new Font("Segoe UI Semibold", 12F, FontStyle.Bold);
            btnAceptar.Enabled = false;
            btnAceptar.Click += (_, _) => Aceptar();
            btnSalir.Dock = DockStyle.Fill;
            btnSalir.Text = "Salir";
            btnSalir.Click += (_, _) => Salir();
            botones.Controls.Add(btnAceptar, 0, 0);
            botones.Controls.Add(btnSalir, 0, 1);
            derecha.Controls.Add(botones, 0, 3);
            raiz.Controls.Add(derecha, 2, 0);
        }

        private void CargarProductos()
        {
            _todos.Clear();
            _todos.AddRange(_productos.Listar()
                .Where(p => p.PrecioPorKilo > 0)
                .OrderBy(p => p.Nombre));
            Filtrar();
        }

        private void Filtrar()
        {
            var q = txtBuscador.Text.Trim();
            IEnumerable<Producto> lista = _todos;
            if (!string.IsNullOrEmpty(q))
            {
                lista = _todos.Where(p =>
                    (p.Nombre?.Contains(q, StringComparison.OrdinalIgnoreCase) ?? false) ||
                    (p.Codigo?.Contains(q, StringComparison.OrdinalIgnoreCase) ?? false));
            }

            var idElegido = _elegido?.Id;
            lstProductos.BeginUpdate();
            lstProductos.Items.Clear();
            foreach (var p in lista)
                lstProductos.Items.Add(p);
            lstProductos.EndUpdate();

            if (idElegido is int id)
            {
                for (int i = 0; i < lstProductos.Items.Count; i++)
                {
                    if (lstProductos.Items[i] is Producto prod && prod.Id == id)
                    {
                        lstProductos.SelectedIndex = i;
                        return;
                    }
                }
            }

            _elegido = null;
            ActualizarCentro();
        }

        private void ConectarBalanza()
        {
            var puerto = _configuracion.ObtenerPorClave(ClavePuerto);
            if (string.IsNullOrWhiteSpace(puerto) || !SerialPort.GetPortNames().Contains(puerto))
            {
                puerto = ElegirPuerto(puerto) ?? string.Empty;
                if (!string.IsNullOrEmpty(puerto))
                    _configuracion.Insertar(ClavePuerto, puerto);
            }

            if (string.IsNullOrEmpty(puerto))
            {
                AvisarSinBalanza("Sin balanza por puerto. Usá el lector o escribí el peso.");
                return;
            }

            try
            {
                _puerto = new SerialPort(puerto, 9600)
                {
                    NewLine = "\n",
                    Encoding = Encoding.ASCII,
                    ReadTimeout = 500
                };
                _puerto.DataReceived += Puerto_DataReceived;
                _puerto.Open();
                lblAvisoPuerto.Visible = false;
            }
            catch (Exception)
            {
                CerrarPuerto();
                AvisarSinBalanza($"No se pudo abrir {puerto}. Usá el lector o escribí el peso.");
            }
        }

        private static string? ElegirPuerto(string? actual)
        {
            var puertos = SerialPort.GetPortNames().OrderBy(p => p).ToArray();
            if (puertos.Length == 0)
            {
                MessageBox.Show("No hay puertos serie disponibles.", "Balanza", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return null;
            }

            using var dlg = new Form
            {
                Text = "Puerto de la balanza",
                FormBorderStyle = FormBorderStyle.FixedDialog,
                StartPosition = FormStartPosition.CenterParent,
                ClientSize = new Size(320, 120),
                MaximizeBox = false,
                MinimizeBox = false
            };
            var lbl = new Label { Text = "Elegí el puerto de la balanza:", Location = new Point(12, 12), AutoSize = true };
            var cb = new ComboBox
            {
                DropDownStyle = ComboBoxStyle.DropDownList,
                Location = new Point(12, 40),
                Width = 290
            };
            cb.Items.AddRange(puertos);
            if (!string.IsNullOrEmpty(actual) && cb.Items.Contains(actual))
                cb.SelectedItem = actual;
            else
                cb.SelectedIndex = 0;
            var ok = new Button { Text = "Aceptar", DialogResult = DialogResult.OK, Location = new Point(146, 80), Width = 75 };
            var cancel = new Button { Text = "Sin balanza", DialogResult = DialogResult.Cancel, Location = new Point(227, 80), Width = 75 };
            dlg.Controls.AddRange(new Control[] { lbl, cb, ok, cancel });
            dlg.AcceptButton = ok;
            dlg.CancelButton = cancel;
            return dlg.ShowDialog() == DialogResult.OK ? cb.SelectedItem?.ToString() : null;
        }

        private void AvisarSinBalanza(string texto)
        {
            lblAvisoPuerto.Text = texto;
            lblAvisoPuerto.Visible = true;
        }

        private void Puerto_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            if (_puerto == null || !_puerto.IsOpen)
                return;

            string chunk;
            try { chunk = _puerto.ReadExisting(); }
            catch { return; }

            if (string.IsNullOrEmpty(chunk))
                return;

            lock (_bufferSerie)
            {
                _bufferSerie.Append(chunk);
                var texto = _bufferSerie.ToString();
                int corte;
                while ((corte = IndexOfNuevaLinea(texto)) >= 0)
                {
                    var linea = texto[..corte].Trim();
                    texto = texto[(corte + 1)..];
                    if (Balanza.TryParsePeso(linea, out var kilos) && Balanza.PesoValido(kilos))
                    {
                        BeginInvoke(() =>
                        {
                            _peso = kilos;
                            ActualizarCentro();
                        });
                    }
                }
                _bufferSerie.Clear();
                _bufferSerie.Append(texto);
            }
        }

        private static int IndexOfNuevaLinea(string texto)
        {
            var n = texto.IndexOf('\n');
            var r = texto.IndexOf('\r');
            if (n < 0) return r;
            if (r < 0) return n;
            return Math.Min(n, r);
        }

        private void txtBuscador_KeyDown(object? sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Down)
            {
                if (lstProductos.Items.Count > 0)
                {
                    lstProductos.Focus();
                    if (lstProductos.SelectedIndex < 0)
                        lstProductos.SelectedIndex = 0;
                }
                e.Handled = true;
                e.SuppressKeyPress = true;
                return;
            }

            if (e.KeyCode != Keys.Enter)
                return;

            e.Handled = true;
            e.SuppressKeyPress = true;
            var texto = txtBuscador.Text.Trim();
            txtBuscador.Clear();
            if (string.IsNullOrEmpty(texto))
            {
                IntentarAgregar();
                return;
            }

            var delListado = _todos.FirstOrDefault(p =>
                string.Equals(p.Codigo, texto, StringComparison.OrdinalIgnoreCase));
            if (delListado != null)
            {
                MostrarEnLista(delListado);
                return;
            }

            var prod = _productos.BuscarPorCodigo(texto);
            if (prod != null)
            {
                MessageBox.Show("No es un producto por kilo.", "Balanza", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (Balanza.TryParsePeso(texto, out var kilos) && Balanza.PesoValido(kilos))
            {
                _peso = kilos;
                ActualizarCentro();
            }
        }

        private void MostrarEnLista(Producto producto)
        {
            Filtrar();
            for (int i = 0; i < lstProductos.Items.Count; i++)
            {
                if (lstProductos.Items[i] is Producto p && p.Id == producto.Id)
                {
                    lstProductos.SelectedIndex = i;
                    _elegido = p;
                    ActualizarCentro();
                    return;
                }
            }

            lstProductos.Items.Clear();
            lstProductos.Items.Add(producto);
            lstProductos.SelectedIndex = 0;
            _elegido = producto;
            ActualizarCentro();
        }

        private void ActualizarCentro()
        {
            if (Balanza.PesoValido(_peso))
                lblPesando.Text = $"Pesando… {_peso.ToString("0.###", Cultura)} kg";
            else
                lblPesando.Text = "Pesando…";

            if (_elegido != null && Balanza.PesoValido(_peso))
            {
                var importe = Balanza.Importe(_peso, _elegido.PrecioPorKilo);
                lblTotal.Text = $"Total: {importe.ToString("C2", Cultura)}";
            }
            else
                lblTotal.Text = "Total:";
        }

        private void IntentarAgregar()
        {
            if (_elegido == null || !Balanza.PesoValido(_peso))
                return;

            var kilos = Math.Round(_peso, 3, MidpointRounding.AwayFromZero);
            var precio = _elegido.PrecioPorKilo;
            _pesadas.Insert(0, new ItemSeleccionado
            {
                IdProducto = _elegido.Id,
                Codigo = _elegido.Codigo,
                Nombre = _elegido.Nombre,
                Cantidad = kilos,
                Precio = precio,
                PorKilo = true
            });
            RefrescarCarrito();
            txtBuscador.Focus();
        }

        private void RefrescarCarrito()
        {
            gridCarrito.Rows.Clear();
            foreach (var item in _pesadas)
                gridCarrito.Rows.Add(item.Nombre, item.Cantidad, item.Subtotal, "×");

            var total = Math.Round(_pesadas.Sum(i => i.Subtotal), 2, MidpointRounding.AwayFromZero);
            lblTotalCarrito.Text = $"Total: {total.ToString("C2", Cultura)}";
            btnAceptar.Enabled = _pesadas.Count > 0;
        }

        private void gridCarrito_CellContentClick(object? sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0 || gridCarrito.Columns[e.ColumnIndex].Name != "Quitar")
                return;
            if (e.RowIndex >= _pesadas.Count)
                return;
            _pesadas.RemoveAt(e.RowIndex);
            RefrescarCarrito();
        }

        private void Aceptar()
        {
            if (_pesadas.Count == 0)
                return;
            _cerrando = true;
            DialogResult = DialogResult.OK;
            Close();
        }

        private void Salir()
        {
            if (_pesadas.Count > 0)
            {
                var r = MessageBox.Show(
                    "Hay pesadas sin pasar al carrito. ¿Salir igual?",
                    "Salir",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Warning);
                if (r != DialogResult.Yes)
                    return;
            }
            _cerrando = true;
            DialogResult = DialogResult.Cancel;
            Close();
        }

        private void frmBalanza_KeyDown(object? sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape)
            {
                Salir();
                e.Handled = true;
            }
        }

        private void frmBalanza_FormClosing(object? sender, FormClosingEventArgs e)
        {
            if (_cerrando)
            {
                CerrarPuerto();
                return;
            }

            if (_pesadas.Count > 0 && DialogResult != DialogResult.OK)
            {
                var r = MessageBox.Show(
                    "Hay pesadas sin pasar al carrito. ¿Salir igual?",
                    "Salir",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Warning);
                if (r != DialogResult.Yes)
                {
                    e.Cancel = true;
                    return;
                }
            }

            CerrarPuerto();
        }

        private void CerrarPuerto()
        {
            if (_puerto == null)
                return;
            try
            {
                _puerto.DataReceived -= Puerto_DataReceived;
                if (_puerto.IsOpen)
                    _puerto.Close();
                _puerto.Dispose();
            }
            catch { /* cerrar igual */ }
            _puerto = null;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
                CerrarPuerto();
            base.Dispose(disposing);
        }
    }
}
