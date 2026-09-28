using StockControl.Domain;
using StockControl.Repository;
using System.Globalization;

namespace StockControl
{
    public class frmPagoProveedor : Form
    {
        private readonly PagoProveedorRepository _repo = new();
        private readonly MetodoPagoRepository _metodos = new();
        private readonly TextBox _txtProveedor = new();
        private readonly TextBox _txtMonto = new();
        private readonly ComboBox _cbMetodo = new();
        private readonly DataGridView _grid = new();
        private List<PagoProveedor> _pagos = new();

        public frmPagoProveedor()
        {
            Text = "Pago a proveedor";
            StartPosition = FormStartPosition.CenterParent;
            ClientSize = new Size(640, 420);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            Icon = new Icon("Resources\\stockIcon.ico");

            var lblProveedor = new Label { Text = "Proveedor", Location = new Point(12, 16), AutoSize = true };
            _txtProveedor.Location = new Point(80, 12);
            _txtProveedor.Size = new Size(180, 23);

            var lblMonto = new Label { Text = "Monto", Location = new Point(272, 16), AutoSize = true };
            _txtMonto.Location = new Point(320, 12);
            _txtMonto.Size = new Size(90, 23);

            var lblMetodo = new Label { Text = "Medio de pago", Location = new Point(12, 52), AutoSize = true };
            _cbMetodo.Location = new Point(110, 48);
            _cbMetodo.Size = new Size(180, 23);
            _cbMetodo.DropDownStyle = ComboBoxStyle.DropDownList;

            var btnAgregar = new Button { Text = "Cargar pago", Location = new Point(310, 47), Size = new Size(110, 25) };
            btnAgregar.Click += BtnAgregar_Click;
            var btnEliminar = new Button { Text = "Eliminar", Location = new Point(430, 47), Size = new Size(90, 25) };
            btnEliminar.Click += BtnEliminar_Click;

            var aviso = new Label
            {
                Text = "Se resta de la ganancia de ese medio al cerrar la caja de hoy.",
                Location = new Point(12, 80),
                AutoSize = true
            };

            _grid.Location = new Point(12, 104);
            _grid.Size = new Size(616, 260);
            _grid.ReadOnly = true;
            _grid.AllowUserToAddRows = false;
            _grid.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            _grid.MultiSelect = false;
            _grid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;

            var btnCerrar = new Button { Text = "Cerrar", Location = new Point(538, 378), Size = new Size(90, 28) };
            btnCerrar.Click += (_, _) => Close();

            Controls.Add(lblProveedor);
            Controls.Add(_txtProveedor);
            Controls.Add(lblMonto);
            Controls.Add(_txtMonto);
            Controls.Add(lblMetodo);
            Controls.Add(_cbMetodo);
            Controls.Add(btnAgregar);
            Controls.Add(btnEliminar);
            Controls.Add(aviso);
            Controls.Add(_grid);
            Controls.Add(btnCerrar);

            CargarMetodos();
            CargarPagos();
        }

        private void CargarMetodos()
        {
            _cbMetodo.Items.Clear();
            foreach (var metodo in _metodos.ObtenerTodos())
                _cbMetodo.Items.Add(metodo);
            if (_cbMetodo.Items.Count > 0)
                _cbMetodo.SelectedIndex = 0;
        }

        private void CargarPagos()
        {
            _pagos = _repo.ListarDelDia(DateTime.Today);
            _grid.DataSource = null;
            _grid.DataSource = _pagos;
            if (_grid.Columns["IdPagoProveedor"] != null)
                _grid.Columns["IdPagoProveedor"].Visible = false;
            if (_grid.Columns["Fecha"] != null)
                _grid.Columns["Fecha"].DefaultCellStyle.Format = "dd/MM/yyyy HH:mm";
            if (_grid.Columns["Monto"] != null)
            {
                _grid.Columns["Monto"].DefaultCellStyle.FormatProvider = new CultureInfo("es-AR");
                _grid.Columns["Monto"].DefaultCellStyle.Format = "C2";
            }
            if (_grid.Columns["MetodoPago"] != null)
                _grid.Columns["MetodoPago"].HeaderText = "Medio de pago";
        }

        private void BtnAgregar_Click(object? sender, EventArgs e)
        {
            string proveedor = _txtProveedor.Text.Trim();
            if (proveedor.Length == 0)
            {
                MessageBox.Show("Indicá el proveedor.", "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            if (!TryParseDecimal(_txtMonto.Text, out decimal monto) || monto <= 0)
            {
                MessageBox.Show("El monto debe ser mayor a cero.", "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            if (_cbMetodo.SelectedItem == null)
            {
                MessageBox.Show("Elegí el medio de pago.", "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            string metodo = _cbMetodo.SelectedItem.ToString() ?? string.Empty;
            var confirmar = MessageBox.Show(
                $"¿Cargar un pago de {monto.ToString("C2", new CultureInfo("es-AR"))} a {proveedor} con {metodo}?",
                "Confirmar pago",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question);
            if (confirmar != DialogResult.Yes)
                return;

            _repo.Insertar(new PagoProveedor
            {
                Fecha = DateTime.Now,
                Proveedor = proveedor,
                Monto = monto,
                MetodoPago = metodo
            });
            _txtProveedor.Clear();
            _txtMonto.Clear();
            CargarPagos();
        }

        private void BtnEliminar_Click(object? sender, EventArgs e)
        {
            if (_grid.CurrentRow?.DataBoundItem is not PagoProveedor pago)
            {
                MessageBox.Show("Seleccioná un pago.", "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var confirmar = MessageBox.Show(
                $"¿Eliminar el pago a {pago.Proveedor}?",
                "Confirmar",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning);
            if (confirmar != DialogResult.Yes)
                return;

            _repo.Eliminar(pago.IdPagoProveedor);
            CargarPagos();
        }

        private static bool TryParseDecimal(string input, out decimal value)
        {
            input = input.Trim().Replace(',', '.');
            return decimal.TryParse(input, NumberStyles.Any, CultureInfo.InvariantCulture, out value);
        }
    }
}
