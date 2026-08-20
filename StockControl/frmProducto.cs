using StockControl.Domain;
using StockControl.Repository;
using System.Globalization;

namespace StockControl
{
    public partial class frmProducto : Form
    {
        private Producto _prod;
        private bool esEdicion = false;
        private ProductoRepository _prodRepository;
        private GrupoRepository _grupoRepository = new GrupoRepository();
        private bool _actualizando = false;
        private bool _cargandoInfo = false;

        public frmProducto(Producto? prod, ProductoRepository prodRep)
        {
            InitializeComponent();
            this.Icon = new Icon("Resources\\stockIcon.ico");
            _prodRepository = prodRep;
            _cargandoInfo = true;
            if (prod != null)
            {
                _prod = prod;
                esEdicion = true;
                txtCodigo.Enabled = false;
                chkSector.Enabled = false;
                CargarInformacionDelProducto();
            }
            else
                _prod = new Producto();

            txtGanancia.Enabled = false;
            txtIVA.Enabled = false;
            txtIVA.Text = StockMain.IVA.ToString();
            _cargandoInfo = false;
        }
        public frmProducto(string codigo, ProductoRepository prodRep, bool nuevoCodigo)
        {
            InitializeComponent();
            this.Icon = new Icon("Resources\\stockIcon.ico");
            _prodRepository = prodRep;
            _prod = new Producto();
            txtGanancia.Enabled = false;
            txtIVA.Enabled = false;
            txtIVA.Text = StockMain.IVA.ToString();
            txtCodigo.Text = codigo;
        }
        private void CargarInformacionDelProducto()
        {
            txtCodigo.Text = _prod.Codigo.ToString();
            txtCosto.Text = _prod.Costo.ToString();
            txtNombre.Text = _prod.Nombre.ToString();
            txtPrecio.Text = _prod.Precio.ToString();
            txtCantidad.Text = _prod.Cantidad.ToString();
            chkSector.Checked = _prod.ProductoSector == 1;
            chkGananciaProd.Checked = _prod.GananciaIndividual == 1;
            if (_prod.GananciaIndividual == 1)
                txtGanancia.Text = _prod.ValorGanancia.ToString();
            if (_prod.IdGrupoProducto != 0)
            {
                lblGrupoSel.Text = _prod.NombreGrupo;
                txtPrecio.Enabled = false;
                txtCosto.Enabled = false;
                chkSector.Enabled = false;
                chkGananciaProd.Enabled = false;
                txtGanancia.Enabled = false;
            }

            if (chkSector.Checked)
                btnBuscarGrupo.Enabled = false;

            lblFechaModificacion.Text = $"Fecha de última modificación: {_prod.fechaModificacion.ToString("dd/MM/yyyy HH:mm:ss")}";
        }

        private void brnCancelar_Click(object sender, EventArgs e)
        {
            var result = MessageBox.Show("¿Estás seguro que querés salir?",
                                            "Salir",
                                            MessageBoxButtons.YesNo,
                                            MessageBoxIcon.Warning
                                        );

            if (result == DialogResult.Yes)
            {
                this.Close();
            }
        }
        private void brnGrabar_Click(object sender, EventArgs e)
        {
            string mensaje = esEdicion ? "editar" : "crear";
            var result = MessageBox.Show($"Vas a {mensaje} este producto, ¿Desea Continuar?",
                                            "Creacion o edicion de producto",
                                            MessageBoxButtons.YesNo,
                                            MessageBoxIcon.Information);

            if (result == DialogResult.Yes)
            {
                if (esEdicion)
                {
                    if (AsignarValoresAProd())
                    {
                        _prodRepository.Actualizar(_prod);
                        this.DialogResult = DialogResult.OK;
                        Close();
                    }
                }
                else
                {
                    var productoExistente = _prodRepository.BuscarPorCodigo(txtCodigo.Text);
                    if (productoExistente == null)
                    {
                        if (AsignarValoresAProd())
                        {
                            _prodRepository.Insertar(_prod);
                            this.DialogResult = DialogResult.OK;
                            Close();
                        }
                    }
                    else
                    {
                        MessageBox.Show($"Existe un Producto con ese codigo, cambie el codigo o elimine el existente", "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    }
                }
            }
            if (this.DialogResult == DialogResult.OK)
                Close();
        }

        private bool AsignarValoresAProd()
        {
            _prod.Codigo = txtCodigo.Text;

            _prod.Nombre = txtNombre.Text;
            decimal precio;
            decimal cantidad;
            decimal costo;
            decimal ganancia;

            // Si el producto tiene grupo asignado, forzar precio/costo desde el grupo
            // para evitar que el formato de los TextBox introduzca decimales espurios.
            if (_prod.IdGrupoProducto > 0)
            {
                var grupo = _grupoRepository.BuscarPorId(_prod.IdGrupoProducto);
                if (grupo != null)
                {
                    _prod.Costo = Math.Round(grupo.Costo, 2);
                    _prod.Precio = Math.Round(grupo.PrecioGrupo, 2);
                }
            }
            else
            {
                if (TryParseDecimal(txtCosto.Text, out costo))
                {
                    _prod.Costo = Math.Round(costo, 2);
                }
                else
                {
                    MessageBox.Show("El costo debe ser un número decimal", "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return false;
                }

                if (TryParseDecimal(txtPrecio.Text, out precio))
                {
                    _prod.Precio = Math.Round(precio, 2);
                }
                else
                {
                    MessageBox.Show("El precio debe ser un número decimal", "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return false;
                }
            }

            if (TryParseDecimal(txtCantidad.Text, out cantidad))
                _prod.Cantidad = cantidad;
            else
            {
                MessageBox.Show("La cantidad debe ser un número", "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }
            if (chkGananciaProd.Checked)
            {
                if (TryParseDecimal(txtGanancia.Text, out ganancia))
                {
                    _prod.GananciaIndividual = 1;
                    _prod.ValorGanancia = Math.Round(ganancia, 2);
                }
                else
                {
                    MessageBox.Show("La ganancia debe ser un número decimal", "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return false;
                }
            }
            else
            {
                _prod.GananciaIndividual = 0;
                _prod.ValorGanancia = 0;
            }

            _prod.ProductoSector = chkSector.Checked ? 1 : 0;
            _prod.fechaModificacion = DateTime.Now;
            return true;

        }
        private bool TryParseDecimal(string input, out decimal value)
        {
            input = input.Trim();

            input = input.Replace(',', '.');

            return decimal.TryParse(input, NumberStyles.Any, CultureInfo.InvariantCulture, out value);
        }

        private void txtCosto_TextChanged(object sender, EventArgs e)
        {
            if (_cargandoInfo) return;
            CalcularCosto();
        }

        private void chkGananciaProd_CheckedChanged(object sender, EventArgs e)
        {

            if (chkGananciaProd.Checked)
            {
                txtGanancia.Enabled = true;
                txtIVA.Enabled = true;

                if (_cargandoInfo) return;
                CalcularCosto();
            }
            else
            {
                txtGanancia.Enabled = false;
                txtIVA.Enabled = false;

                if (_cargandoInfo) return;
                CalcularCosto();
            }
        }

        private void txtGanancia_TextChanged(object sender, EventArgs e)
        {
            if (_actualizando || _cargandoInfo) return;
            CalcularCosto();
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

        private void chkSector_CheckedChanged(object sender, EventArgs e)
        {
            if (chkSector.Checked)
            {
                txtCantidad.Text = "1";
                txtCantidad.Enabled = false;
                txtPrecio.Text = "1";
                txtPrecio.Enabled = false;
                txtCosto.Text = "1";
                txtCosto.Enabled = false;
                txtGanancia.Enabled = false;
                chkGananciaProd.Enabled = false;
                btnBuscarGrupo.Enabled = false;
            }
            else
            {
                txtCantidad.Text = "0";
                txtCantidad.Enabled = true;
                txtPrecio.Text = "0";
                txtPrecio.Enabled = true;
                txtCosto.Text = "0";
                txtCosto.Enabled = true;
                chkGananciaProd.Enabled = true;

                btnBuscarGrupo.Enabled = true;
            }
        }

        private void txtIVA_TextChanged(object sender, EventArgs e)
        {
            if (_actualizando || _cargandoInfo) return;
            CalcularCosto();
        }

       

        private void txtPrecio_TextChanged(object sender, EventArgs e)
        {
            if (_actualizando || _cargandoInfo) return;
            CalcularGanancia();
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

        private void txtPrecio_KeyPress(object sender, KeyPressEventArgs e)
        {
            var separator = System.Globalization.CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator;
            var textBox = (TextBox)sender;

            // Permitir control de teclas
            if (char.IsControl(e.KeyChar))
                return;

            // Permitir dígitos
            if (char.IsDigit(e.KeyChar))
                return;

            // Permitir separador decimal solo si no existe ya
            if (e.KeyChar.ToString() == separator && !textBox.Text.Contains(separator))
                return;

            // Bloquear todo lo demás
            e.Handled = true;
        }

        private void btnBuscarGrupo_Click(object sender, EventArgs e)
        {
            using (var frm = new frmBuscarGrupo())
            {
                if (frm.ShowDialog() == DialogResult.OK)
                {
                    var grupoSeleccionado = frm.GrupoSeleccionado;
                    lblGrupoSel.Text = grupoSeleccionado.NombreGrupo;
                    _prod.IdGrupoProducto = grupoSeleccionado.IdGrupoProducto;
                    txtPrecio.Text = grupoSeleccionado.PrecioGrupo.ToString("#0.00");
                    txtCosto.Text = grupoSeleccionado.Costo.ToString("#0.00");
                    txtPrecio.Enabled = false;
                    txtCosto.Enabled = false;
                    chkSector.Enabled = false;
                    chkGananciaProd.Enabled = false;
                    txtGanancia.Enabled = false;
                }
            }
        }

        private void btnSacarGrupo_Click(object sender, EventArgs e)
        {
            _prod.IdGrupoProducto = 0;
            txtPrecio.Text = string.Empty;
            txtPrecio.Enabled = true;
            txtCosto.Enabled = true;
            chkSector.Enabled = true;
            chkGananciaProd.Enabled = true;
            lblGrupoSel.Text = "Sin Grupo";
            CalcularCosto();
            return;
        }
    }
}
