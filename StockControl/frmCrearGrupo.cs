using StockControl.Domain;
using StockControl.Repository;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace StockControl
{
    public partial class frmCrearGrupo : Form
    {

        GrupoRepository grupoRepository = new GrupoRepository();
        private bool _actualizando = false;
        public GrupoProductos grupoNuevo = new GrupoProductos();
        public frmCrearGrupo()
        {
            InitializeComponent();
            txtIVA.Text = StockMain.IVA.ToString();
        }

        private void btnAceptar_Click(object sender, EventArgs e)
        {
            decimal precioGrupo;
            decimal costoGrupo;
            decimal gananciaGrupo;
            int tieneGanancia = chkGananciaProd.Checked ? 1 : 0;
            if (txtNombre.Text == string.Empty)
            {
                MessageBox.Show("Debe indicar el nombre del nuevo grupo", "Info", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            if (txtPrecio.Text == string.Empty)
            {
                MessageBox.Show("Debe indicar el precio del grupo", "Info", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            if (txtCosto.Text == string.Empty)
            {
                MessageBox.Show("Debe indicar el costo del grupo", "Info", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            if (TryParseDecimal(txtPrecio.Text, out var precio))
            {
                precioGrupo = precio;
            }
            else
            {
                MessageBox.Show("El precio del grupo debe ser un numero decimal", "Info", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            if (TryParseDecimal(txtCosto.Text, out var costo))
            {
                costoGrupo = costo;
            }
            else
            {
                MessageBox.Show("El costo del grupo debe ser un numero decimal", "Info", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (chkGananciaProd.Checked)
            {
                if (TryParseDecimal(txtGanancia.Text, out var gan))
                {
                    gananciaGrupo = gan;
                }
                else
                {
                    MessageBox.Show("La ganancia del grupo debe ser un numero decimal", "Info", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
            }
            else
            {
                gananciaGrupo = 1;
            }

            var grupo = new GrupoProductos();
            grupo.NombreGrupo = txtNombre.Text;
            grupo.PrecioGrupo = precioGrupo;
            grupo.Costo = costoGrupo;
            grupo.Ganancia = gananciaGrupo;
            grupo.GananciaIndividual = tieneGanancia;
            grupoRepository.Insertar(grupo);
            this.DialogResult = DialogResult.OK;
            grupoNuevo = grupo;
            return;
        }

        private bool TryParseDecimal(string input, out decimal value)
        {
            input = input.Trim();

            input = input.Replace(',', '.');

            return decimal.TryParse(input, NumberStyles.Any, CultureInfo.InvariantCulture, out value);
        }

        private void btnCancelar_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void txtCosto_TextChanged(object sender, EventArgs e)
        {
            CalcularCosto();
        }

        private void txtPrecio_TextChanged(object sender, EventArgs e)
        {
            if (_actualizando) return;
            CalcularGanancia();
        }

        private void txtGanancia_TextChanged(object sender, EventArgs e)
        {
            if (_actualizando) return;
            CalcularCosto();
        }

        private void txtIVA_TextChanged(object sender, EventArgs e)
        {
            if (_actualizando) return;
            CalcularCosto();
        }

        private void chkGananciaProd_CheckedChanged(object sender, EventArgs e)
        {
            if (chkGananciaProd.Checked)
            {
                txtGanancia.Enabled = true;
                txtIVA.Enabled = true;
                CalcularCosto();
            }
            else
            {
                txtGanancia.Enabled = false;
                txtIVA.Enabled = false;
                CalcularCosto();
            }
        }

        private void txtCosto_KeyPress(object sender, KeyPressEventArgs e)
        {
            var separator = System.Globalization.CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator;
            var textBox = (TextBox)sender;

            if (char.IsControl(e.KeyChar)) return;
            if (char.IsDigit(e.KeyChar)) return;
            if (e.KeyChar.ToString() == separator && !textBox.Text.Contains(separator)) return;

            e.Handled = true;
        }

        private void txtGanancia_KeyPress(object sender, KeyPressEventArgs e)
        {
            var separator = System.Globalization.CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator;
            var textBox = (TextBox)sender;

            if (char.IsControl(e.KeyChar)) return;
            if (char.IsDigit(e.KeyChar)) return;
            if (e.KeyChar.ToString() == separator && !textBox.Text.Contains(separator)) return;

            e.Handled = true;
        }

        private void txtPrecio_KeyPress(object sender, KeyPressEventArgs e)
        {
            var separator = System.Globalization.CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator;
            var textBox = (TextBox)sender;

            if (char.IsControl(e.KeyChar)) return;
            if (char.IsDigit(e.KeyChar)) return;
            if (e.KeyChar.ToString() == separator && !textBox.Text.Contains(separator)) return;

            e.Handled = true;
        }
        private void CalcularCosto()
        {
            try
            {
                if (txtCosto.Text != string.Empty)
                {
                    _actualizando = true;
                    if (TryParseDecimal(txtCosto.Text, out decimal value))
                    {
                        if (chkGananciaProd.Checked && !string.IsNullOrEmpty(txtGanancia.Text))
                        {
                            if (TryParseDecimal(txtGanancia.Text, out decimal gananciaProd))
                            {
                                if (TryParseDecimal(txtIVA.Text, out decimal iva))
                                    txtPrecio.Text = (value * (gananciaProd * iva)).ToString("0.##");
                            }
                        }
                        else
                            txtPrecio.Text = (value * (StockMain._factorGanancia * StockMain.IVA)).ToString("0.##");
                    }
                }
            }
            catch (Exception ex)
            {

            }
            finally
            {
                _actualizando = false;
            }

        }
        private void CalcularGanancia()
        {
            if (!TryParseDecimal(txtPrecio.Text, out decimal precio))
                return;
            if (!TryParseDecimal(txtCosto.Text, out decimal costo))
                return;
            if (!TryParseDecimal(txtIVA.Text, out decimal iva))
                return;
            if (costo == 0)
                return;

            decimal ganancia = precio / costo / iva; // o el cálculo que uses
            _actualizando = true;
            try
            {
                chkGananciaProd.Checked = true;
                txtGanancia.Text = ganancia.ToString("0.##");
            }
            finally
            {
                _actualizando = false;
            }
        }
    }
}
