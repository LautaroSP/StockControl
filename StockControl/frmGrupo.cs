using StockControl.Domain;
using StockControl.Extensions;
using StockControl.Repository;
using System.ComponentModel;
using System.Globalization;

namespace StockControl
{
    public partial class Grupo : Form
    {
        private BindingList<ProductoDTO> _productos = new();
        private BindingList<GrupoProductos> _grupos = new();
        private BindingList<ProductoDTO> _productosFiltrados = new();
        private int idGrupoSeleccioando = 0;

        private decimal _dolar = 1;
        private readonly ProductoRepository _rprod = new();
        private readonly GrupoRepository _rgrupo = new();
        private bool _actualizando = false;
        private bool _cobrarEnPesos = false;

        public Grupo(bool cobrarEnPesos)
        {
            InitializeComponent();
            InicializarFormulario();
            _cobrarEnPesos = cobrarEnPesos;
        }

        private void InicializarFormulario()
        {
            dgProductos.Enabled = false;
            cbMostrarSeleccionados.Checked = false;
            CargarDatos();
            dgProductos.AllowUserToAddRows = false;
            dgGrupos.AllowUserToAddRows = false;
            dgProductos.CellValueChanged += dgProductos_CellValueChanged;
            dgProductos.CurrentCellDirtyStateChanged += dgProductos_CurrentCellDirtyStateChanged;
            dgProductos.AllowUserToAddRows = false;
            dgProductos.ReadOnly = false;
            dgProductos.Columns["Seleccionado"].ReadOnly = false;
            dgProductos.Columns["NombreProducto"].ReadOnly = true;
            dgProductos.Columns["NombreGrupo"].ReadOnly = true;
            dgProductos.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dgGrupos.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.ColumnHeader;
            txtGanancia.Enabled = false;
            txtIVA.Enabled = false;
            txtIVA.Text = StockMain.IVA.ToString();

        }

        private void CargarDatos()
        {
            Cursor.Current = Cursors.WaitCursor;

            CargarGrupos();
            CargarProductos();
            OcultarColumnas();

            Cursor.Current = Cursors.Default;
        }

        private void OcultarColumnas()
        {
            if (dgGrupos.Columns["IdGrupoProducto"] != null)
                dgGrupos.Columns["IdGrupoProducto"].Visible = false;
            if (dgProductos.Columns["IdGrupo"] != null)
                dgProductos.Columns["IdGrupo"].Visible = false;
            if (dgProductos.Columns["IdProducto"] != null)
                dgProductos.Columns["IdProducto"].Visible = false;
        }

        private void CargarGrupos()
        {
            var lista = _rgrupo.Listar();
            _grupos = new BindingList<GrupoProductos>(lista);
            dgGrupos.DataSource = _grupos;
        }

        private void CargarProductos()
        {
            var prods = _rprod.Listar();
            prods = prods.Where(x => x.ProductoSector != 1).ToList();
            var lista = prods.Select(prod =>
                new ProductoDTO(
                    false,
                    prod.Id,
                    prod.Nombre,
                    prod.IdGrupoProducto,
                    prod.IdGrupoProducto == 0 ? string.Empty :
                    _grupos.FirstOrDefault(g => g.IdGrupoProducto == prod.IdGrupoProducto)?.NombreGrupo ?? string.Empty,
                    prod.Codigo
                )
            ).ToList();

            _productos = new BindingList<ProductoDTO>(lista);
            dgProductos.DataSource = _productos;
        }

        private void dgGrupos_RowEnter(object sender, DataGridViewCellEventArgs e)
        {
            int idGrupo = 0;
            var fila = dgGrupos.Rows[e.RowIndex];
            if (fila != null)
            {
                var grupo = (GrupoProductos)fila.DataBoundItem;
                idGrupo = grupo.IdGrupoProducto;
                txtNombreGrupo.Text = grupo.NombreGrupo;
                txtPrecio.Text = grupo.PrecioGrupo.ToString();
                txtCosto.Text = grupo.Costo.ToString();
                if (grupo.GananciaIndividual == 1)
                {
                    chkGananciaProd.Checked = true;
                    txtGanancia.Text = grupo.Ganancia.ToString();
                }
            }

            if (idGrupo == 0)
                return;

            idGrupoSeleccioando = idGrupo;
            // Filtramos los productos del grupo
            var prodList = _productos.Where(x => x.IdGrupo == idGrupo).ToList();

            // Marcamos los productos del grupo
            foreach (var p in _productos)
                p.Seleccionado = prodList.Contains(p);

            // Actualizamos lista filtrada
            _productosFiltrados = new BindingList<ProductoDTO>(prodList);

            // Mostramos según checkbox
            dgProductos.DataSource = cbMostrarSeleccionados.Checked
                ? _productosFiltrados
                : _productos;

            dgProductos.Enabled = true;
            dgGrupos.AllowUserToAddRows = false;
            lblCantProductos.Text = prodList.Count.ToString();
            txtBuscador.Text = string.Empty;
        }
        private bool TryParseDecimal(string input, out decimal value)
        {
            input = input.Trim();

            input = input.Replace(',', '.');

            return decimal.TryParse(input, NumberStyles.Any, CultureInfo.InvariantCulture, out value);
        }
        private void cbMostrarSeleccionados_CheckedChanged(object sender, EventArgs e)
        {
            dgProductos.DataSource = cbMostrarSeleccionados.Checked
                ? _productosFiltrados
                : _productos;
            txtBuscador.Text = string.Empty;
        }
        private void dgProductos_CurrentCellDirtyStateChanged(object sender, EventArgs e)
        {
            if (dgProductos.IsCurrentCellDirty)
            {
                dgProductos.CommitEdit(DataGridViewDataErrorContexts.Commit);
            }
        }

        private void dgProductos_CellValueChanged(object sender, DataGridViewCellEventArgs e)
        {
            try
            {
                if (dgProductos.Columns[e.ColumnIndex].Name == "Seleccionado")
                {
                    var fila = dgProductos.Rows[e.RowIndex];
                    var producto = (ProductoDTO)fila.DataBoundItem;

                    if (dgGrupos.CurrentRow == null) return;
                    var grupoSeleccionado = (GrupoProductos)dgGrupos.CurrentRow.DataBoundItem;

                    bool marcado = Convert.ToBoolean(fila.Cells["Seleccionado"].Value);
                    if (marcado)
                    {
                        producto.IdGrupo = grupoSeleccionado.IdGrupoProducto;
                        producto.NombreGrupo = grupoSeleccionado.NombreGrupo;
                        _rprod.ActualizarGrupo(producto.IdProducto, grupoSeleccionado.IdGrupoProducto);
                        MarcarProdEnLista(producto, true, grupoSeleccionado);
                        var prod = _rprod.BuscarPorCodigo(producto.CodigoProducto);
                        prod.Precio = grupoSeleccionado.PrecioGrupo;
                        prod.Costo = grupoSeleccionado.Costo;
                        _rprod.Actualizar(prod);
                    }
                    else
                    {
                        producto.IdGrupo = 0;
                        producto.NombreGrupo = string.Empty;
                        _rprod.ActualizarGrupo(producto.IdProducto, 0);
                        MarcarProdEnLista(producto, false, grupoSeleccionado);
                    }

                    ActualizarVistaProductos();
                    FiltrarProductos();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ocurrio un error : {ex.Message}", "Info", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
        }

        private void MarcarProdEnLista(ProductoDTO producto, bool agregar, GrupoProductos grupoSeleccionado)
        {
            ProductoDTO prod = _productos.First(x => x.IdProducto == producto.IdProducto);


            if (agregar)
            {
                prod.IdGrupo = grupoSeleccionado.IdGrupoProducto;
                prod.NombreGrupo = grupoSeleccionado.NombreGrupo;
            }
            else
            {
                prod.IdGrupo = 0;
                prod.NombreGrupo = string.Empty;
            }
            // Filtramos los productos del grupo
            var prodList = _productos.Where(x => x.IdGrupo == idGrupoSeleccioando).ToList();

            // Marcamos los productos del grupo
            foreach (var p in _productos)
                p.Seleccionado = prodList.Contains(p);

            // Actualizamos lista filtrada
            _productosFiltrados = new BindingList<ProductoDTO>(prodList);

        }

        private void ActualizarVistaProductos()
        {
            if (dgGrupos.CurrentRow == null) return;
            var grupoSeleccionado = (GrupoProductos)dgGrupos.CurrentRow.DataBoundItem;

            // Filtra solo los productos del grupo actual
            var productosDelGrupo = _productos.Where(p => p.IdGrupo == grupoSeleccionado.IdGrupoProducto).ToList();

            // Aplica filtro de "mostrar seleccionados"
            if (cbMostrarSeleccionados.Checked)
            {
                var seleccionados = productosDelGrupo.Where(p => p.Seleccionado).ToList();
                dgProductos.DataSource = new BindingList<ProductoDTO>(seleccionados);
            }
            else
            {
                dgProductos.DataSource = new BindingList<ProductoDTO>(productosDelGrupo);
            }
        }

        private void btnNuevoGrupo_Click(object sender, EventArgs e)
        {
            try
            {
                decimal precioGrupo;
                decimal costoGrupo;
                decimal gananciaGrupo;
                int tieneGanancia = chkGananciaProd.Checked ? 1 : 0;
                if (txtNombreGrupo.Text == string.Empty)
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
                grupo.NombreGrupo = txtNombreGrupo.Text;
                grupo.PrecioGrupo = precioGrupo;
                grupo.Costo = costoGrupo;
                grupo.Ganancia = gananciaGrupo;
                grupo.GananciaIndividual = tieneGanancia;
                _rgrupo.Insertar(grupo);
                txtNombreGrupo.Text = string.Empty;
                txtPrecio.Text = string.Empty;
                txtCosto.Text = string.Empty;
                txtGanancia.Text = string.Empty;
                chkGananciaProd.Checked = false;
                CargarDatos();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ocurrio un error : {ex.Message}", "Info", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

        }

        private void btnEliminar_Click(object sender, EventArgs e)
        {
            try
            {

                if (idGrupoSeleccioando == 0)
                {
                    MessageBox.Show($"Debe seleccionar un grupo a eliminar", "Info", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                var result = MessageBox.Show("¿Desea eliminar el grupo?",
                                        "Eliminar",
                                        MessageBoxButtons.YesNo,
                                        MessageBoxIcon.Information
                                    );

                if (result == DialogResult.Yes)
                {
                    foreach (var prod in _productosFiltrados)
                    {
                        _rprod.ActualizarGrupo(prod.IdProducto, 0);
                    }
                    _rgrupo.Eliminar(idGrupoSeleccioando);
                    CargarDatos();
                }
                else
                    return;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ocurrio un error : {ex.Message}", "Info", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
        }

        private void btnGuardar_Click(object sender, EventArgs e)
        {
            try
            {
                if (idGrupoSeleccioando == 0)
                {
                    MessageBox.Show($"Debe seleccionar un grupo a editar", "Info", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
                decimal precioGrupo;
                decimal costoGrupo;
                decimal gananciaGrupo;
                int tieneGanancia = chkGananciaProd.Checked ? 1 : 0;
                if (txtNombreGrupo.Text == string.Empty)
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
                var grupo = _rgrupo.BuscarPorId(idGrupoSeleccioando);
                grupo.NombreGrupo = txtNombreGrupo.Text;
                grupo.PrecioGrupo = precioGrupo;
                grupo.Costo = costoGrupo;
                grupo.Ganancia = gananciaGrupo;
                grupo.GananciaIndividual = tieneGanancia;
                _rgrupo.Actualizar(grupo);
                var productosDelGrupo = _rprod.BuscarPorGrupo(grupo.IdGrupoProducto);
                if (productosDelGrupo.Count > 0)
                {
                    _rprod.ActualizarPrecioPorPrecioGrupo(productosDelGrupo, grupo.PrecioGrupo, grupo.Costo);
                }
                CargarDatos();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ocurrio un error : {ex.Message}", "Info", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

        }

        private void txtBuscador_TextChanged(object sender, EventArgs e)
        {
            FiltrarProductos();
        }

        private void FiltrarProductos()
        {
            var texto = txtBuscador.Text.Trim().ToLower();

            // Si no hay texto, volver a mostrar correctamente la lista original
            if (string.IsNullOrEmpty(texto))
            {
                dgProductos.DataSource = cbMostrarSeleccionados.Checked
                    ? _productosFiltrados
                    : _productos;

                OcultarColumnas();
                return;
            }

            // Filtrar sobre la fuente "real" según el estado del checkbox
            IEnumerable<ProductoDTO> fuente = cbMostrarSeleccionados.Checked
                ? _productosFiltrados
                : _productos;

            var filtrado = fuente
                .Where(x =>
                    (x.CodigoProducto?.ToLower().Contains(texto) ?? false) ||
                    (x.NombreProducto?.ToLower().Contains(texto) ?? false)
                )
                .ToList();

            dgProductos.DataSource = new BindingList<ProductoDTO>(filtrado);

            OcultarColumnas();
        }


        private void txtCosto_TextChanged(object sender, EventArgs e)
        {
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

        private void txtGanancia_TextChanged(object sender, EventArgs e)
        {
            if (_actualizando) return;
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
        private void txtPrecio_TextChanged(object sender, EventArgs e)
        {
            if (_actualizando) return;
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

        private void txtNombreGrupo_TextChanged(object sender, EventArgs e)
        {

        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            FiltrarGrupos();
        }

        private void FiltrarGrupos()
        {
            var texto = textBox1.Text.Trim().ToLower();

            IEnumerable<GrupoProductos> fuente = _grupos;

            if (!string.IsNullOrEmpty(texto))
            {
                fuente = fuente.Where(g =>
                    (g.NombreGrupo?.ToLower().Contains(texto) ?? false)
                );
            }

            dgGrupos.DataSource = new BindingList<GrupoProductos>(fuente.ToList());

            OcultarColumnas();
        }

    }
}
