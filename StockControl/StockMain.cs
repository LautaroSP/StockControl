using Microsoft.VisualBasic;
using StockControl.Domain;
using StockControl.Repository;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing.Printing;
using System.Globalization;

namespace StockControl
{
    public partial class StockMain : Form
    {
        private ProductoRepository _prodRepository = new ProductoRepository();
        List<Producto> productos = new List<Producto>();
        private BindingList<ItemSeleccionado> _carrito;
        private InformeVentaRepository _informeVentaRepository = new InformeVentaRepository();
        private ConfiguracionRepository _configuracionRepository = new ConfiguracionRepository();
        public static string nombreLocal;
        public static string factorGanancia;
        public static decimal _factorGanancia;
        public static decimal _valorDolar = 1;
        private string MetodoDePago = string.Empty;
        private string cobrarEnPesos = string.Empty;
        public static bool _cobrarEnPesos = false;
        private Dictionary<int, decimal> preciosBase = new Dictionary<int, decimal>();
        private MetodoPagoRepository _metodoPagoRepository = new MetodoPagoRepository();
        public static decimal IVA;
        public static string factorIVA;
        private List<MetodoDePago> metodosDePago = new List<MetodoDePago>();
        private List<MetodoDePago> multiplesMetodos = new List<MetodoDePago>();
        private GrupoRepository _grupoRepository = new GrupoRepository();
        private bool imprimirTicket = true;
        private bool editandoProductoSector = false;
        private List<ItemSeleccionado> prodGenericos = new();

        public StockMain()
        {
            InitializeComponent();
            this.Icon = new Icon("Resources\\stockIcon.ico");
            _carrito = new BindingList<ItemSeleccionado>();
            dataGridView2.AutoGenerateColumns = false;
            CrearDataGrid2();
            dataGridView2.DataSource = _carrito;
            nombreLocal = _configuracionRepository.ObtenerPorClave("NombreLocal");
            factorGanancia = _configuracionRepository.ObtenerPorClave("FactorGanancia");
            cobrarEnPesos = _configuracionRepository.ObtenerPorClave("CobrarEnPesos");
            factorIVA = _configuracionRepository.ObtenerPorClave("IVA");
            
            if (nombreLocal == string.Empty)
            {
                Configuracion frmconfig = new Configuracion();
                MessageBox.Show("Debe indicar el nombre del local antes de continuar", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
                frmconfig.ShowDialog();
                if (frmconfig.DialogResult != DialogResult.OK)
                {
                    MessageBox.Show("Debe indicar el nombre del local antes de continuar", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    frmconfig.ShowDialog();
                }
                nombreLocal = _configuracionRepository.ObtenerPorClave("NombreLocal");
            }
            if (factorGanancia == string.Empty)
            {
                _factorGanancia = 1;
            }
            else
            {
                if (TryParseDecimal(factorGanancia, out decimal result))
                {
                    _factorGanancia = result;
                }
            }
            if (factorIVA == string.Empty)
            {
                IVA = 1;
            }
            else
            {
                if (TryParseDecimal(factorIVA, out decimal result))
                {
                    IVA = result;
                }
            }
            if (cobrarEnPesos == string.Empty)
            {
                _cobrarEnPesos = false;
            }
            else
            {
                if (bool.TryParse(cobrarEnPesos, out bool result))
                {
                    _cobrarEnPesos = result;
                }
            }
            CargarMetodosPago();
            CargarProductos();
            Bitmap bmp = new Bitmap("Resources\\gear.png");

            // Opcional: redimensionar si querés un tamaño fijo para el botón (ej: 32x32)
            Bitmap bmpRedimensionado = new Bitmap(bmp, new Size(20, 20));

            // Asignar la imagen al botón
            btnConfiguracion.Image = bmpRedimensionado;
            if (_cobrarEnPesos)
            {
                lblDolar.Visible = false;
                chkCobroEnPesos.Visible = false;
                txtValorDolar.Visible = false;
            }
            dataGridView2.CellEndEdit += dataGridView2_CellEndEdit;

        }

        private void CargarProductos()
        {
            productos = _prodRepository.Listar();
            productos = productos.OrderBy(x => x.Nombre).ToList();
            foreach (var prod in productos)
            {
                if (prod.ProductoSector == 1)
                {
                    prod.Sector = "Si";
                }
                else
                {
                    prod.Sector = "No";
                }
                if (prod.IdGrupoProducto != 0)
                    prod.NombreGrupo = _grupoRepository.BuscarPorId(prod.IdGrupoProducto).NombreGrupo;
                else
                    prod.NombreGrupo = "";
            }
            dataGridView1.DataSource = productos;
            dataGridView1.Columns["Id"].Visible = false;
            dataGridView1.Columns["Codigo"].AutoSizeMode = DataGridViewAutoSizeColumnMode.DisplayedCells;
            dataGridView1.Columns["Nombre"].AutoSizeMode = DataGridViewAutoSizeColumnMode.DisplayedCells;
            dataGridView1.Columns["fechaModificacion"].HeaderText = "Fecha de Modificación";
            dataGridView1.Columns["fechaModificacion"].AutoSizeMode = DataGridViewAutoSizeColumnMode.DisplayedCells;
            lblTotal.AutoSize = false;
            lblTotal.MaximumSize = new Size(200, 0);
            lblTotal.AutoEllipsis = false;
            lblTotal.TextAlign = ContentAlignment.MiddleLeft;
            dataGridView1.ReadOnly = true;
            dataGridView1.Columns["ProductoSector"].Visible = false;
            dataGridView1.Columns["IdGrupoProducto"].Visible = false;
            dataGridView1.Columns["GananciaIndividual"].Visible = false;
            dataGridView1.Columns["ValorGanancia"].Visible = false;
            dataGridView1.Columns["NombreGrupo"].HeaderText = "Grupo";
            dataGridView1.Columns["Costo"].DefaultCellStyle.Format = "C2";
            dataGridView1.Columns["Costo"].DefaultCellStyle.FormatProvider = new CultureInfo("es-AR");
            dataGridView1.Columns["Precio"].DefaultCellStyle.Format = "C2";
            dataGridView1.Columns["Precio"].DefaultCellStyle.FormatProvider = new CultureInfo("es-AR");
            int cantidad = dataGridView1.Rows.Count;
            lblCantProd.Text = cantidad.ToString();

            var prodFechaModif = productos.Where(p => p.fechaModificacion.Date == DateTime.Now.Date).ToList();
            CargarFiltro();
        }

        private void CargarFiltro()
        {
            string filtro = txtBuscar.Text.ToLower();

            var filtrados = productos.Where(p =>
                p.Codigo.ToLower().Contains(filtro) ||
                p.Nombre.ToLower().Contains(filtro)
            ).ToList();

            dataGridView1.DataSource = filtrados;
        }

        private void CrearDataGrid2()
        {
            dataGridView2.AutoGenerateColumns = false;

            dataGridView2.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = "Codigo",
                HeaderText = "Código",
                ReadOnly = true,
                Visible = false
            });

            dataGridView2.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = "Nombre",
                HeaderText = "Nombre",
                ReadOnly = true
            });

            dataGridView2.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = "Cantidad",
                HeaderText = "Cant.",
                Name = "Cantidad",
                ReadOnly = false,
                Width = 50
            });

            dataGridView2.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = "Precio",
                HeaderText = "Precio",
                Name = "Precio",
                ReadOnly = false,
                Width = 50,
                DefaultCellStyle = new DataGridViewCellStyle
                {
                    FormatProvider = new CultureInfo("es-AR")
                }
            });

            dataGridView2.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = "Subtotal",
                HeaderText = "Subtotal",
                ReadOnly = true,
                Width = 70,
                DefaultCellStyle = new DataGridViewCellStyle
                {
                    FormatProvider = new CultureInfo("es-AR")
                }
            });
        }

        private void btnAgregar_Click(object sender, EventArgs e)
        {
            frmProducto frmProducto = new frmProducto(null, _prodRepository, _cobrarEnPesos);
            frmProducto.FormClosed += (s, e) =>
            {
                if (frmProducto.DialogResult == DialogResult.OK)
                    Load();
            };

            frmProducto.Show();
        }
        private void Load()
        {
            productos = _prodRepository.Listar();
            productos = productos.OrderBy(x => x.Nombre).ToList();
            foreach (var prod in productos)
            {
                if (prod.ProductoSector == 1)
                {
                    prod.Sector = "Si";
                }
                else
                {
                    prod.Sector = "No";
                }
            }
            dataGridView1.DataSource = productos;
            dataGridView1.Columns["Codigo"].AutoSizeMode = DataGridViewAutoSizeColumnMode.DisplayedCells;
            dataGridView1.Columns["Nombre"].AutoSizeMode = DataGridViewAutoSizeColumnMode.DisplayedCells;

            if (txtBuscar.Text != string.Empty)
            {
                string filtro = txtBuscar.Text.ToLower();

                var filtrados = productos.Where(p =>
                    p.Codigo.ToLower().Contains(filtro) ||
                    p.Nombre.ToLower().Contains(filtro)
                ).ToList();

                dataGridView1.DataSource = filtrados;
            }
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            CargarFiltro();
        }
        private void dataGridViewProductos_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            preciosBase.Clear();
            if (e.RowIndex >= 0)
            {
                var producto = (Producto)dataGridView1.Rows[e.RowIndex].DataBoundItem;
                var existente = _carrito.FirstOrDefault(i => i.Codigo == producto.Codigo);

                if (existente == null)
                {
                    var item = new ItemSeleccionado
                    {
                        Codigo = producto.Codigo,
                        Nombre = producto.Nombre,
                        Precio = producto.Precio,
                        Cantidad = 1,
                        IdProducto = producto.Id
                    };

                    _carrito.Add(item);

                }
                else
                {
                    if (producto.ProductoSector != 1)
                        existente.Cantidad += 1;
                }
                int i = 0;
                foreach (var p in _carrito)
                {
                    if (p.Codigo.Contains("GENERIC-"))
                    {
                        preciosBase[i] = p.Precio;
                    }
                    else
                    {
                        var prod = _prodRepository.BuscarPorCodigo(p.Codigo);
                        if (prod.ProductoSector != 1)
                        {
                            preciosBase[i] = prod.Precio;
                        }
                    }
                    i++;
                }
                dataGridView2.Refresh();
                CalcularPrecioEnDolar();
                CalcularTotal();
                if (producto.ProductoSector == 1)
                {
                    SeleccionarPrecioProductoSector(producto);
                }
            }
        }

        private void SeleccionarPrecioProductoSector(Producto producto)
        {
            int lastRow = dataGridView2.Rows.Count - 1;
            int colPrecio = 3;

            dataGridView2.ClearSelection();
            dataGridView2.CurrentCell = dataGridView2.Rows[lastRow].Cells[colPrecio];

            dataGridView2.FirstDisplayedScrollingRowIndex = lastRow;

            this.BeginInvoke(new Action(() =>
            {
                dataGridView2.BeginEdit(true);
            }));
        }
        private void dataGridView2_CellEndEdit(object sender, DataGridViewCellEventArgs e)
        {
            dataGridView2.CommitEdit(DataGridViewDataErrorContexts.Commit);
            dataGridView2.EndEdit();
            int row = e.RowIndex;

            if (e.ColumnIndex == 1)
            {
                dataGridView2.Rows[row].Cells[1].ReadOnly = true;
                IrACelda(row, 2);
            }
            else if (e.ColumnIndex == 2)
            {
                IrACelda(row, 3);
            }
            else if (e.ColumnIndex == 3)
            {
                CalcularTotal();
                var itemSeleccionado = dataGridView2.Rows[row].DataBoundItem as ItemSeleccionado;
                if (itemSeleccionado != null && itemSeleccionado.Codigo.Contains("GENERIC-"))
                {
                    var prodGenerico = new ItemSeleccionado
                    {
                        IdProducto = itemSeleccionado.IdProducto,
                        Precio = itemSeleccionado.Precio
                    };
                    prodGenericos.Add(prodGenerico);
                }
                VolverAScanner();
            }
        }

        private void IrACelda(int row, int col)
        {
            try
            {
                if (_carrito.Count == 0) return;
                BeginInvoke(new Action(() =>
                {
                    try
                    {
                        dataGridView2.CurrentCell = dataGridView2.Rows[row].Cells[col];
                        dataGridView2.BeginEdit(true);
                    }
                    catch
                    {
                        return;
                    }
                }));
            }
            catch
            {
                return;
            }

        }

        private void dataGridViewCarrito_CellValueChanged(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0)
                return;

            var columnName = dataGridView2.Columns[e.ColumnIndex].DataPropertyName;

            if (columnName == "Cantidad" || columnName == "Precio")
            {
                if (columnName == "Cantidad")
                {
                    var cellValue = dataGridView2.Rows[e.RowIndex].Cells[e.ColumnIndex].Value;

                    if (cellValue == null || string.IsNullOrWhiteSpace(cellValue.ToString()))
                        return;
                }

                if (columnName == "Precio")
                {
                    var row = dataGridView2.Rows[e.RowIndex];
                    var item = (ItemSeleccionado)row.DataBoundItem;
                    var itemCarrito = _carrito.First(x => x.IdProducto == item.IdProducto);
                    itemCarrito.Precio = item.Precio;
                }
                CalcularTotal();
                dataGridView2.Refresh();
            }
        }

        private void dataGridViewCarrito_CurrentCellDirtyStateChanged(object sender, EventArgs e)
        {
            var cell = dataGridView2.CurrentCell;
            if (cell == null)
                return;

            if (cell.OwningColumn is DataGridViewCheckBoxColumn)
            {
                if (dataGridView2.IsCurrentCellDirty)
                {
                    dataGridView2.CommitEdit(DataGridViewDataErrorContexts.Commit);
                }
            }
        }

        private void brnCancelar_Click(object sender, EventArgs e)
        {
            _carrito.Clear();
            dataGridView2.Refresh();
            CalcularTotal();
        }

        private void btnEditar_Click(object sender, EventArgs e)
        {
            if (dataGridView1.CurrentRow == null)
            {
                MessageBox.Show($"Se debe seleccionar un producto a editar,", "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var producto = (Producto)dataGridView1.CurrentRow.DataBoundItem;
            using (var frmProducto = new frmProducto(producto, _prodRepository, _cobrarEnPesos))
            {
                frmProducto.ShowDialog();
                if (frmProducto.DialogResult == DialogResult.OK)
                {
                    CargarProductos();
                }
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            if (dataGridView1.CurrentRow == null)
            {
                MessageBox.Show($"Se debe seleccionar un producto a eliminar,", "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            var result = MessageBox.Show("¿Estás seguro desea eliminar este Producto?",
                                "Confirmar eliminacion",
                                MessageBoxButtons.YesNo,
                                MessageBoxIcon.Warning
                            );

            if (result == DialogResult.Yes)
            {
                var producto = (Producto)dataGridView1.CurrentRow.DataBoundItem;
                _prodRepository.Eliminar(producto.Id);
                Load();
            }
        }
        private void dataGridView1_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            // Verificamos que sea la columna "Cantidad"
            if (dataGridView1.Columns[e.ColumnIndex].Name == "Cantidad")
            {
                if (e.Value != null && int.TryParse(e.Value.ToString(), out int cantidad))
                {
                    if (cantidad < 5)
                    {
                        // Cambiamos el color de toda la fila
                        dataGridView1.Rows[e.RowIndex].DefaultCellStyle.BackColor = Color.LightSalmon;
                        dataGridView1.Rows[e.RowIndex].DefaultCellStyle.ForeColor = Color.Black;
                    }
                    else
                    {
                        // Restauramos color por defecto si no cumple la condición
                        dataGridView1.Rows[e.RowIndex].DefaultCellStyle.BackColor = Color.White;
                        dataGridView1.Rows[e.RowIndex].DefaultCellStyle.ForeColor = Color.Black;
                    }
                }
            }
        }

        private void btnCobrar_Click(object sender, EventArgs e)
        {
            try
            {
                if (string.IsNullOrEmpty(MetodoDePago) &&
                (!chkMultiPago.Checked || (chkMultiPago.Checked && multiplesMetodos.Count == 0)))
                {
                    MessageBox.Show($"Debe seleccionar un Metodo de pago", "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
                if (_carrito.Count > 0)
                {
                    //var stockValido = VerificarStockDisponible();
                    //if (stockValido != null)
                    //{
                    //    MessageBox.Show($"El producto {stockValido.Nombre} no tiene stock suficiente, por favor verificar", "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    //    return;
                    //}
                    if (chkCosto.Checked)
                    {
                        var result = MessageBox.Show("Está a punto de cobrar el ticket utilizando el costo del producto, en lugar del precio. ¿Desea continuar?",
                            "Confirmar",
                            MessageBoxButtons.YesNo,
                            MessageBoxIcon.Warning
                        );
                        if (result == DialogResult.No)
                        {
                            return;
                        }
                    }

                    foreach (var item in _carrito)
                    {
                        
                        if (!item.Codigo.Contains("GENERIC-")) 
                        {
                            var producto = productos.FirstOrDefault(x => x.Id == item.IdProducto);
                            if (producto!.ProductoSector != 1)
                            {
                                producto.Cantidad = producto.Cantidad - item.Cantidad;

                                if (producto.Cantidad <= 0)
                                    producto.Cantidad = 0;

                                _prodRepository.Actualizar(producto);
                            }
                        }
                    }

                    Load();
                    GenerarInformeDeVenta();



                    if (imprimirTicket)
                    {
                        ImprimirTicket();
                    }
                    _carrito.Clear();
                    dataGridView2.Refresh();
                    frmMetodoPago._metodosDePago.Clear();
                    chkMultiPago.Checked = false;
                    lblItems.Text = "0";
                    txtTotal.Text = "0";
                    chkCosto.Checked = false;
                    chkDescuento.Checked = false;
                }
                VolverAScanner();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

        }

        private void dataGridViewProductos_CellBeginEdit(object sender, DataGridViewCellCancelEventArgs e)
        {
            var prod = (ItemSeleccionado)dataGridView2.Rows[e.RowIndex].DataBoundItem;
            if (prod.Codigo.Contains("GENERIC-") || prod == null)
            {
                return;
            }
            if (dataGridView2.Columns[e.ColumnIndex].Name == "Precio")
            {
                var itemCarrito = (ItemSeleccionado)dataGridView2.Rows[e.RowIndex].DataBoundItem;

                var producto = productos.FirstOrDefault(x => x.Id == itemCarrito.IdProducto);

                if (producto.ProductoSector != 1)
                {
                    e.Cancel = true;
                }
            }

            if (dataGridView2.Columns[e.ColumnIndex].Name == "Cantidad")
            {
                var itemCarrito = (ItemSeleccionado)dataGridView2.Rows[e.RowIndex].DataBoundItem;

                var producto = productos.FirstOrDefault(x => x.Id == itemCarrito.IdProducto);

                if (producto.ProductoSector == 1)
                {
                    e.Cancel = true;
                }
            }
        }


        private void ImprimirTicket()
        {
            try
            {
                TicketPrinter ticketPrinter = new TicketPrinter(_carrito.ToList());
                string impresoraPorDefecto = new PrinterSettings().PrinterName;
                ticketPrinter.PrintTicketFinal(impresoraPorDefecto);
            }
            catch
            {
                throw;
            }
        }

        private void GenerarInformeDeVenta()
        {
            decimal total = _carrito
                .Sum(i => i.Subtotal);
            int idInformeVenta;
            string descuentoStr = txtDescuento.Text.Trim().Replace("%", "");
            decimal descuento = 0;
            string costo = string.Empty;
            if(!chkDescuento.Checked)
            {
                descuentoStr = "0";
            }
            if (!TryParseDecimal(descuentoStr, out descuento))
            {
                MessageBox.Show("El descuento ingresado no es válido.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (chkCosto.Checked)
            {
                costo = "Si";
            }
            else
                costo = "No";
            
            List<InformeVentaDetalle> informeDetalles = new();

            if (chkMultiPago.Checked && multiplesMetodos.Count > 0)
            {
                bool detalleAsignado = false;
                foreach (var metodo in multiplesMetodos)
                {
                    idInformeVenta = _informeVentaRepository.InsertarInformeVenta(metodo.Monto, metodo.Descripcion, true, !detalleAsignado, descuento, costo);
                    if (!detalleAsignado)
                    {
                        foreach (var item in _carrito)
                        {
                            informeDetalles.Add(new InformeVentaDetalle(item, idInformeVenta));
                        }
                        detalleAsignado = true;
                    }

                }
            }
            else
            {
                idInformeVenta = _informeVentaRepository.InsertarInformeVenta(total, MetodoDePago, false, true, descuento, costo);
                foreach (var item in _carrito)
                {
                    informeDetalles.Add(new InformeVentaDetalle(item, idInformeVenta));
                }
            }

            foreach (var informe in informeDetalles)
                _informeVentaRepository.InsertarInformeVentaDetalle(informe);

        }

        private Producto? VerificarStockDisponible()
        {
            foreach (var item in _carrito)
            {
                if (item.IdProducto == 0) continue;
                var producto = productos.FirstOrDefault(x => x.Id == item.IdProducto);
                if (producto.ProductoSector == 1)
                {
                    continue;
                }
                if (producto != null)
                {
                    if (producto.Cantidad - item.Cantidad < 0)
                        return producto;
                }
                else
                {
                    throw new Exception($"El producto {item.Nombre} no existe, por favor verificar");
                }
            }
            return null;
        }

        private void btnVerInforme_Click(object sender, EventArgs e)
        {
            frmInformeVentas frmInformeVentas = new frmInformeVentas();
            frmInformeVentas.ShowDialog();
            if (frmInformeVentas.DialogResult == DialogResult.OK)
            {
                Load();
            }
            if (frmInformeVentas.DialogResult == DialogResult.Yes)
            {
                _carrito.Clear();
                _carrito = new BindingList<ItemSeleccionado>(frmInformeVentas.ticket);
                dataGridView2.DataSource = null;
                dataGridView2.DataSource = _carrito;
                ActualizarTotal();
                CalcularTotal();
                MessageBox.Show("Los items del ticket se cargaron en el carrito", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }
        private void dataGridViewCarrito_EditingControlShowing(object sender, DataGridViewEditingControlShowingEventArgs e)
        {
            var columnName = dataGridView2.CurrentCell.OwningColumn.DataPropertyName;

            if (columnName == "Cantidad" || columnName == "Precio")
            {
                if (e.Control is TextBox tb)
                {
                    tb.TextChanged -= Tb_TextChanged;
                    tb.TextChanged += Tb_TextChanged;
                }
            }
        }

        private void Tb_TextChanged(object? sender, EventArgs e)
        {
            if (sender is TextBox tb)
            {
                int selStart = tb.SelectionStart;
                string text = tb.Text;
                if (!(text.EndsWith(",") || text.EndsWith(".")))
                {
                    if (TryParseDecimal(text, out decimal value))
                    {
                        tb.TextChanged -= Tb_TextChanged;
                        tb.Text = value.ToString(CultureInfo.CurrentCulture);
                        tb.SelectionStart = selStart;
                        tb.TextChanged += Tb_TextChanged;
                    }
                }
            }
        }
        private bool TryParseDecimal(string input, out decimal value)
        {
            input = input.Trim();

            input = input.Replace(',', '.');

            return decimal.TryParse(input, NumberStyles.Any, CultureInfo.InvariantCulture, out value);
        }
        private void txtScanner_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                string codigo = txtScanner.Text.Trim();
                txtScanner.Clear();
                preciosBase.Clear();
                if (!string.IsNullOrEmpty(codigo))
                {
                    var producto = productos.FirstOrDefault(p => p.Codigo == codigo);

                    if (producto != null)
                    {
                        var itemExistente = _carrito.FirstOrDefault(c => c.Codigo == producto.Codigo);

                        if (itemExistente != null && producto.ProductoSector != 1)
                        {
                            itemExistente.Cantidad++;
                        }
                        else if (producto.ProductoSector == 1 && itemExistente != null)
                        {
                            SeleccionarPrecioProductoSector(producto);
                            return;
                        }
                        else
                        {
                            _carrito.Add(new ItemSeleccionado
                            {
                                Codigo = producto.Codigo,
                                Nombre = producto.Nombre,
                                Precio = producto.Precio,
                                Cantidad = 1,
                                IdProducto = producto.Id
                            });
                        }
                        int i = 0;
                        foreach (var p in _carrito)
                        {
                            var prod = _prodRepository.BuscarPorCodigo(p.Codigo);
                            if (p.Codigo.Contains("GENERIC"))
                                preciosBase[i] = p.Precio;
                            else
                                preciosBase[i] = prod.Precio;
                            i++;
                        }
                        dataGridView2.DataSource = null;
                        dataGridView2.DataSource = _carrito;
                        if (producto.ProductoSector == 1)
                        {
                            SeleccionarPrecioProductoSector(producto);
                        }
                    }
                    else
                    {
                        var result = MessageBox.Show("El producto no existe. ¿Desea agregar uno nuevo con este código?",
                            "Producto no encontrado",
                            MessageBoxButtons.YesNo,
                            MessageBoxIcon.Question
                        );
                        if (result == DialogResult.Yes)
                        {
                            frmProducto frmProducto = new frmProducto(codigo, _prodRepository, _cobrarEnPesos, true);
                            frmProducto.ShowDialog();

                            if (frmProducto.DialogResult == DialogResult.OK)
                            {
                                Load();
                                producto = productos.FirstOrDefault(p => p.Codigo == codigo);

                                if (producto != null)
                                {
                                    var itemExistente = _carrito.FirstOrDefault(c => c.Codigo == producto.Codigo);

                                    if (itemExistente != null && producto.ProductoSector != 1)
                                    {
                                        itemExistente.Cantidad++;
                                    }
                                    else if (producto.ProductoSector == 1 && itemExistente != null)
                                    {
                                        SeleccionarPrecioProductoSector(producto);
                                        return;
                                    }
                                    else
                                    {
                                        _carrito.Add(new ItemSeleccionado
                                        {
                                            Codigo = producto.Codigo,
                                            Nombre = producto.Nombre,
                                            Precio = producto.Precio,
                                            Cantidad = 1,
                                            IdProducto = producto.Id
                                        });
                                    }
                                    int i = 0;
                                    foreach (var p in _carrito)
                                    {
                                        var prod = _prodRepository.BuscarPorCodigo(p.Codigo);
                                        preciosBase[i] = prod.Precio;
                                        i++;
                                    }
                                    dataGridView2.DataSource = null;
                                    dataGridView2.DataSource = _carrito;
                                    if (producto.ProductoSector == 1)
                                    {
                                        SeleccionarPrecioProductoSector(producto);
                                    }
                                }
                            }
                        }

                    }

                }
                CalcularTotal();
                if (!editandoProductoSector)
                {
                    VolverAScanner();
                }

            }
            else if (e.KeyCode == Keys.Add)
            {
                IrAMedioDePago();
                e.SuppressKeyPress = true;
            }
        }

        private void IrAMedioDePago()
        {
            cbMetodosPago.Focus();
            BeginInvoke(new Action(() =>
           {
               cbMetodosPago.DroppedDown = true;
           }));
        }

        private void VolverAScanner()
        {
            txtScanner.Focus();
        }

        private void btnConfiguracion_Click(object sender, EventArgs e)
        {
            Configuracion frmcofig = new Configuracion(nombreLocal, factorGanancia, _cobrarEnPesos, factorIVA);
            frmcofig.ShowDialog();
            if (frmcofig.DialogResult == DialogResult.Yes)
            {
                lblDolar.Visible = !_cobrarEnPesos;
                chkCobroEnPesos.Visible = !_cobrarEnPesos;
                txtValorDolar.Visible = !_cobrarEnPesos;
            }
            Load();
        }

        private void btnCodeBar_Click(object sender, EventArgs e)
        {
            if (dataGridView1.CurrentRow == null)
            {
                MessageBox.Show($"Se debe seleccionar un producto para generar el codigo de Barras", "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            var result = MessageBox.Show("Se va a generar e ímprimir el codigo de barras. ¿Desea continuar?",
                                "Confirmar",
                                MessageBoxButtons.YesNo,
                                MessageBoxIcon.Information
                            );

            if (result == DialogResult.Yes)
            {
                int cantidad = 0;
                var result2 = MessageBox.Show("¿Desea imprimir más de un codigo de barra?",
                    "Confirmar",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Information
                );
                if (result2 == DialogResult.Yes)
                {
                    string input = Interaction.InputBox("Ingrese la cantidad de códigos a imprimir:", "Cantidad", "1");
                    if (!int.TryParse(input, out cantidad) || cantidad <= 0)
                    {
                        MessageBox.Show("Cantidad inválida, se imprimirá 1 código.", "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        cantidad = 1;
                    }
                }
                else
                    cantidad = 1;
                var producto = (Producto)dataGridView1.CurrentRow.DataBoundItem;
                string impresoraPorDefecto = new PrinterSettings().PrinterName;
                TicketPrinter ticketPrinter = new TicketPrinter();
                ticketPrinter.PrintBarcode(impresoraPorDefecto, producto.Codigo, cantidad);
            }
        }

        private void txtValorDolar_Leave(object sender, EventArgs e)
        {
            if (txtValorDolar.Text != string.Empty)
            {
                if (TryParseDecimal(txtValorDolar.Text, out decimal result))
                {
                    _valorDolar = result;
                    CalcularPrecioEnDolar();
                }
                else
                    MessageBox.Show("El valor del dolar debe ser un numero decimal", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

        }

        private void chkCobroEnPesos_CheckedChanged(object sender, EventArgs e)
        {
            try
            {
                CalcularPrecioEnDolar();
            }
            catch (Exception ex)
            {
                throw ex;
            }

        }

        private void CalcularPrecioEnDolar()
        {
            foreach (DataGridViewRow row in dataGridView2.Rows)
            {
                if (row.Index >= 0 && preciosBase.ContainsKey(row.Index))
                {
                    decimal precioBase = preciosBase[row.Index];
                    if (chkCobroEnPesos.Checked)
                        row.Cells["Precio"].Value = (precioBase * _valorDolar);

                    else
                        row.Cells["Precio"].Value = precioBase;
                }
            }
            dataGridView2.Refresh();
            ActualizarTotal();
            CalcularTotal();
        }

        private void ActualizarTotal()
        {
            decimal total = 0;
            foreach (DataGridViewRow row in dataGridView2.Rows)
            {
                if (row.Cells["Precio"].Value != null && decimal.TryParse(row.Cells["Precio"].Value.ToString(), out decimal precio))
                    total += precio;
            }
            lblTotal.Text = total.ToString("N2");
        }

        private void dataGridView2_CellMouseDoubleClick(object sender, DataGridViewCellMouseEventArgs e)
        {
            if (e.RowIndex < 0 || e.RowIndex >= dataGridView2.Rows.Count)
                return;

            var row = dataGridView2.Rows[e.RowIndex];
            var itemAEliminar = row.DataBoundItem as ItemSeleccionado;

            if (itemAEliminar != null)
            {
                _carrito.Remove(itemAEliminar);

                ActualizarTotal();
                CalcularTotal();
            }

            VolverAScanner();
        }


        private void cbMetodosPago_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cbMetodosPago.SelectedItem?.ToString() == "Agregar método de pago...")
            {
                string nuevo = Interaction.InputBox("Ingrese el nuevo método de pago:", "Nuevo método", "");

                if (!string.IsNullOrWhiteSpace(nuevo))
                {
                    _metodoPagoRepository.Insertar(nuevo);
                    CargarMetodosPago();
                    cbMetodosPago.SelectedItem = nuevo;
                    metodosDePago.Add(new MetodoDePago { Descripcion = nuevo, Monto = 0 });
                }
                else
                {
                    cbMetodosPago.SelectedIndex = -1;
                }
            }
            else
            {
                MetodoDePago = cbMetodosPago.SelectedItem?.ToString();
            }
        }

        private void CargarMetodosPago()
        {
            cbMetodosPago.Items.Clear();

            var metodos = _metodoPagoRepository.ObtenerTodos();

            foreach (var metodo in metodos)
            {
                cbMetodosPago.Items.Add(metodo);
                metodosDePago.Add(new Domain.MetodoDePago { Descripcion = metodo });
            }

            cbMetodosPago.Items.Add("Agregar método de pago...");
            cbMetodosPago.SelectedIndex = 0;
        }

        private void chkMultiPago_CheckedChanged(object sender, EventArgs e)
        {
            if (chkMultiPago.Checked)
            {
                if (_carrito.Count == 0)
                {
                    MessageBox.Show("El carrito está vacío, no se puede seleccionar pago múltiple", "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    chkMultiPago.Checked = false;
                    return;
                }
                frmMetodoPago frmMetodoPago = new frmMetodoPago(metodosDePago, _carrito.Sum(i => i.Subtotal));
                frmMetodoPago.ShowDialog();
                if (frmMetodoPago.DialogResult == DialogResult.OK)
                {
                    multiplesMetodos = frmMetodoPago._metodosDePago;
                }
                else
                    chkMultiPago.Checked = false;
            }
        }

        private void btnGrupos_Click(object sender, EventArgs e)
        {
            Grupo frmGrupo = new Grupo(_cobrarEnPesos);
            frmGrupo.FormClosed += (s, args) => CargarProductos();
            frmGrupo.Show();
        }

        private void chkImprimirTicket_CheckedChanged(object sender, EventArgs e)
        {
            imprimirTicket = chkImprimirTicket.Checked;
        }

        private void btnGeneric_Click(object sender, EventArgs e)
        {
            AgregarProductoGenerico();
        }

        private void AgregarProductoGenerico()
        {
            var itemSeleccionado = new ItemSeleccionado
            {
                Codigo = $"GENERIC-{Guid.NewGuid().ToString().Substring(0, 8)}",
                Nombre = "Producto",
                Precio = 0,
                Cantidad = 1,
                IdProducto = Guid.NewGuid().GetHashCode()
            };
            _carrito.Add(itemSeleccionado);

            dataGridView2.DataSource = null;
            dataGridView2.DataSource = _carrito;
            int lastRow = dataGridView2.Rows.Count - 1;
            BeginInvoke(new Action(() =>
            {
                var cell = dataGridView2.Rows[lastRow].Cells[1]; // Nombre
                cell.ReadOnly = false;

                dataGridView2.CurrentCell = cell;
                dataGridView2.BeginEdit(true);
            }));
        }

        private void cbMetodosPago_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Add)
            {
                IrACobrar();
                e.SuppressKeyPress = true;
            }
        }

        private void IrACobrar()
        {
            btnCobrar.Focus();
        }

        private void chkDescuento_CheckedChanged(object sender, EventArgs e)
        {
            if (chkDescuento.Checked)
            {
                txtDescuento.Enabled = true;
                txtDescuento.Focus();
                
            }
            else
            {
                txtDescuento.Enabled = false;
                txtDescuento.Text = string.Empty;
                CalcularTotal();
            }
        }

        private void AplicarDescuento()
        {
            bool usarCosto = chkCosto.Checked;

            foreach (var item in _carrito)
            {
                // Primero establecemos el precio base correcto
                if (item.Codigo.Contains("GENERIC-"))
                {
                    var prod = prodGenericos.FirstOrDefault(x => x.IdProducto == item.IdProducto);
                    if (prod == null) continue;
                    item.Precio = prod.Precio; // los genéricos no tienen costo, siempre precio
                }
                else
                {
                    var producto = productos.FirstOrDefault(x => x.Id == item.IdProducto);
                    if (producto == null) continue;
                    item.Precio = usarCosto ? producto.Costo : producto.Precio;
                }

                // Después aplicamos el descuento si corresponde
                if (chkDescuento.Checked &&
                    int.TryParse(txtDescuento.Text.Replace("%", "").Trim(), out int descuento) &&
                    descuento > 0)
                {
                    item.Precio = item.Precio - (item.Precio * descuento / 100m);
                }
            }
        }

        private void CalcularTotal()
        {
            AplicarDescuento(); // Esto setea todos los precios correctamente

            decimal total = _carrito.Sum(i => i.Cantidad * i.Precio);

            dataGridView2.Refresh();
            txtTotal.Text = total.ToString("C2", new CultureInfo("es-AR"));

            decimal items = _carrito.Sum(i => i.Cantidad);
            lblItems.Text = items.ToString("0", new CultureInfo("es-AR"));
        }

        // Al entrar al campo: sacamos el % para editar
        private void txtDescuento_Enter(object sender, EventArgs e)
        {
            string valor = txtDescuento.Text.Replace("%", "").Trim();
            txtDescuento.Text = valor;
            txtDescuento.SelectAll();
        }

        // Solo permite dígitos y teclas de control
        private void txtDescuento_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (!char.IsDigit(e.KeyChar) && !char.IsControl(e.KeyChar))
            {
                e.Handled = true;
                return;
            }

            // Evita que supere 100 al tipear
            if (char.IsDigit(e.KeyChar))
            {
                string futuro = txtDescuento.Text + e.KeyChar;
                if (int.TryParse(futuro, out int valor) && valor > 100)
                {
                    e.Handled = true;
                    MessageBox.Show("El descuento no puede superar 100%.",
                                    "Valor inválido",
                                    MessageBoxButtons.OK,
                                    MessageBoxIcon.Warning);
                }
            }
        }

        // Al salir del campo: validamos y concatenamos el %
        private void txtDescuento_Leave(object sender, EventArgs e)
        {
            if (!int.TryParse(txtDescuento.Text.Replace("%", ""), out int valor))
                valor = 0;

            valor = Math.Max(0, Math.Min(100, valor));
            txtDescuento.Text = valor + "%";
            CalcularTotal();
            IrAMedioDePago();
        }

        private void chkCosto_CheckedChanged(object sender, EventArgs e)
        {
            CalcularTotal();
        }
    }
}

