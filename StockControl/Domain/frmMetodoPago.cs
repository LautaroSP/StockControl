using System;
using System.Collections.Generic;
using System.Globalization;
using System.Windows.Forms;

namespace StockControl.Domain
{
    public partial class frmMetodoPago : Form
    {
        private decimal _totalVenta;
        private List<MetodoDePago> _metodoDePagos;
        public static List<MetodoDePago> _metodosDePago = new List<MetodoDePago>();

        public frmMetodoPago(List<MetodoDePago> metodoDePagos, decimal total)
        {
            InitializeComponent();
            InitializeDataGrid();
            _metodoDePagos = metodoDePagos;
            _totalVenta = total;
            CargarMetodosPago();
            lblRestante.Text = $"{_totalVenta:C}";
        }

        private void InitializeDataGrid()
        {
            dgMetodoDePago.AllowUserToAddRows = false;
            dgMetodoDePago.RowHeadersVisible = false;
            dgMetodoDePago.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgMetodoDePago.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;

            // Columna de selección
            var colCheck = new DataGridViewCheckBoxColumn();
            colCheck.HeaderText = "";
            colCheck.Width = 30;
            dgMetodoDePago.Columns.Add(colCheck);

            // Columna de nombre del método
            var colMetodo = new DataGridViewTextBoxColumn();
            colMetodo.HeaderText = "Método de pago";
            colMetodo.ReadOnly = true;
            dgMetodoDePago.Columns.Add(colMetodo);

            // Columna de monto
            var colMonto = new DataGridViewTextBoxColumn();
            colMonto.HeaderText = "Monto";
            dgMetodoDePago.Columns.Add(colMonto);

            // Columna de cobrar restante
            var colRestante = new DataGridViewCheckBoxColumn();
            colRestante.HeaderText = "Cobrar restante";
            colRestante.Width = 80;
            dgMetodoDePago.Columns.Add(colRestante);

            dgMetodoDePago.CellEndEdit += dgMetodoDePago_CellEndEdit;
            dgMetodoDePago.CellContentClick += dgMetodoDePago_CellContentClick;
            dgMetodoDePago.CellValueChanged += dgMetodoDePago_CellValueChanged;
        }

        private void dgMetodoDePago_CellEndEdit(object sender, DataGridViewCellEventArgs e)
        {
            if (e.ColumnIndex == 2) // monto
            {
                decimal totalIngresado = 0;

                foreach (DataGridViewRow row in dgMetodoDePago.Rows)
                {
                    bool seleccionado = Convert.ToBoolean(row.Cells[0].Value);
                    if (seleccionado)
                    {
                        decimal monto = 0;
                        TryParseDecimal(row.Cells[2].Value?.ToString(), out monto);
                        totalIngresado += monto;
                    }
                }

                decimal restante = _totalVenta - totalIngresado;
                lblRestante.Text = $"{restante:C}";
                CalcularRestante();
            }
        }

        private void dgMetodoDePago_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            // 👇 Esto hace que CellValueChanged se dispare al hacer click
            dgMetodoDePago.CommitEdit(DataGridViewDataErrorContexts.Commit);

            if (e.RowIndex < 0) return;

            // Columna de selección
            if (e.ColumnIndex == 0)
            {
                bool isChecked = Convert.ToBoolean(dgMetodoDePago.Rows[e.RowIndex].Cells[0].EditedFormattedValue);
                var montoCell = dgMetodoDePago.Rows[e.RowIndex].Cells[2];
                var restanteCell = dgMetodoDePago.Rows[e.RowIndex].Cells[3];

                montoCell.ReadOnly = !isChecked;
                restanteCell.ReadOnly = !isChecked;

                if (!isChecked)
                {
                    montoCell.Value = "0";
                    restanteCell.Value = false;
                    CalcularRestante();
                }
            }

            // Columna de Cobrar Restante (radio simulado)
            if (e.ColumnIndex == 3)
            {
                bool seleccionado = Convert.ToBoolean(dgMetodoDePago.Rows[e.RowIndex].Cells[0].Value);
                if (!seleccionado) return;

                foreach (DataGridViewRow row in dgMetodoDePago.Rows)
                {
                    if (row.Index != e.RowIndex)
                        row.Cells[3].Value = false;
                }

                // Toggle actual
                var currentValue = Convert.ToBoolean(dgMetodoDePago.Rows[e.RowIndex].Cells[3].Value);
                dgMetodoDePago.Rows[e.RowIndex].Cells[3].Value = !currentValue;
            }
        }

        private void dgMetodoDePago_CellValueChanged(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0) return;

            // Si cambió el check de Cobrar restante, recalculamos
            if (e.ColumnIndex == 3)
            {
                CalcularRestante();
            }

            // Si cambió el check de selección, también recalculamos
            if (e.ColumnIndex == 0)
            {
                CalcularRestante();
            }
        }

        private void CalcularRestante()
        {
            decimal totalIngresado = 0;
            int filaRestante = -1;

            for (int i = 0; i < dgMetodoDePago.Rows.Count; i++)
            {
                bool seleccionado = Convert.ToBoolean(dgMetodoDePago.Rows[i].Cells[0].Value);
                bool esRestante = Convert.ToBoolean(dgMetodoDePago.Rows[i].Cells[3].Value);

                if (seleccionado && !esRestante)
                {
                    decimal monto = 0;
                    decimal.TryParse(dgMetodoDePago.Rows[i].Cells[2].Value?.ToString(), out monto);
                    totalIngresado += monto;
                }

                if (esRestante) filaRestante = i;
            }

            decimal restante = _totalVenta - totalIngresado;
            lblRestante.Text = $"{restante:C}";

            if (filaRestante >= 0)
            {
                dgMetodoDePago.Rows[filaRestante].Cells[2].Value = restante.ToString("0.00");
            }
        }

        private bool TryParseDecimal(string input, out decimal value)
        {
            if (string.IsNullOrWhiteSpace(input))
            {
                value = 0;
                return false;
            }

            input = input.Trim().Replace(',', '.');
            return decimal.TryParse(input, NumberStyles.Any, CultureInfo.InvariantCulture, out value);
        }

        private void CargarMetodosPago()
        {
            foreach (var metodo in _metodoDePagos)
            {
                int index = dgMetodoDePago.Rows.Add(false, metodo.Descripcion, "0", false);
                dgMetodoDePago.Rows[index].Cells[2].ReadOnly = true;
                dgMetodoDePago.Rows[index].Cells[3].ReadOnly = true;
            }
        }

        private void btnCancelar_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }

        private void btnAceptar_Click(object sender, EventArgs e)
        {
            _metodosDePago.Clear();

            foreach (DataGridViewRow row in dgMetodoDePago.Rows)
            {
                bool seleccionado = Convert.ToBoolean(row.Cells[0].Value);
                if (!seleccionado) continue;

                string metodo = row.Cells[1].Value?.ToString();
                string montoStr = row.Cells[2].Value?.ToString();

                if (string.IsNullOrWhiteSpace(metodo)) continue;

                decimal monto;
                TryParseDecimal(montoStr, out monto);


                if(monto <= 0)
                {
                    MessageBox.Show($"El monto para el método '{metodo}' debe ser mayor a cero.", "Monto inválido", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
                
                _metodosDePago.Add(new MetodoDePago
                {
                    Descripcion = metodo,
                    Monto = monto
                });
            }

            decimal totalIngresado = _metodosDePago.Sum(x => x.Monto);
            if (totalIngresado != _totalVenta)
            {
                MessageBox.Show("El total ingresado no coincide con el total de la venta.", "Error de validación", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            this.DialogResult = DialogResult.OK;
            this.Close();
        }
    }
}
