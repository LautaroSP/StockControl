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
    public partial class Configuracion : Form
    {
        private ConfiguracionRepository _configuracionRepository = new();
        private bool _CambioLocal = false;
        private bool _CambioGanancia = false;
        private bool _ValorMoneda = false;
        private bool _CambioMoneda = false;
        private bool _CambioIVA = false;
        private ProductoRepository _rprod = new ProductoRepository();

        public Configuracion()
        {
            InitializeComponent();
        }
        public Configuracion(string nombreLocal, string factorGanancia, bool cobrarEnDolar, string IVA)
        {
            InitializeComponent();
            txtNombreLocal.Text = nombreLocal;
            txtFactorGanancia.Text = factorGanancia;
            chkValorMoneda.Checked = cobrarEnDolar;
            txtIVA.Text = IVA;
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
            if (_CambioMoneda)
            {
                _configuracionRepository.Insertar("CobrarEnPesos", _ValorMoneda.ToString());
                StockMain._cobrarEnPesos = chkValorMoneda.Checked;
                this.DialogResult = DialogResult.Yes;
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

            if(_CambioIVA || _CambioGanancia)
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
                    MessageBox.Show("Finalizó la actualizacion de productos","Actualizacion", MessageBoxButtons.OK, MessageBoxIcon.Information);
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

        private void chkValorMoneda_CheckedChanged(object sender, EventArgs e)
        {
            _CambioMoneda = true;
            _ValorMoneda = chkValorMoneda.Checked;
        }

        private void txtIVA_TextChanged(object sender, EventArgs e)
        {
            _CambioIVA = true;
        }
    }

}

