using Microsoft.VisualBasic;
using StockControl.Domain;
using StockControl.Repository;
using System.ComponentModel;
using System.Drawing.Printing;
using System.Globalization;

namespace StockControl
{
    public partial class StockMain : Form
    {
        private ProductoRepository _prodRepository = new ProductoRepository();
        private List<Producto> _productos = new List<Producto>();
        private BindingList<ItemSeleccionado> _carrito;
        private InformeVentaRepository _informeVentaRepository = new InformeVentaRepository();
        private ConfiguracionRepository _configuracionRepository = new ConfiguracionRepository();
        private MetodoPagoRepository _metodoPagoRepository = new MetodoPagoRepository();
        private GrupoRepository _grupoRepository = new GrupoRepository();
        
        private string _metodoDePago = string.Empty;
        private List<MetodoDePago> _metodosDePago = new List<MetodoDePago>();
        private List<MetodoDePago> _multiplesMetodos = new List<MetodoDePago>();
        private List<ItemSeleccionado> _preciosGenericos = new List<ItemSeleccionado>();
        
        private bool _imprimirTicket = true;

        // Campos estáticos para compatibilidad con otros forms (deprecated - usar ConfiguracionRepository)
        public static string nombreLocal = string.Empty;
        public static string factorGanancia = string.Empty;
        public static decimal _factorGanancia = 1;
        public static decimal IVA = 1;
        public static string factorIVA = string.Empty;
        public static decimal _valorDolar = 1;
        public static bool _cobrarEnPesos = false;

        public StockMain()
        {
            InitializeComponent();
            this.Icon = new Icon("Resources\\stockIcon.ico");
            _carrito = new BindingList<ItemSeleccionado>();
            dataGridView2.AutoGenerateColumns = false;
            CrearDataGrid2();
            dataGridView2.DataSource = _carrito;
            
            CargarMetodosPago();
            ActualizarGridProductos();
            
            Bitmap bmp = new Bitmap("Resources\\gear.png");
            Bitmap bmpRedimensionado = new Bitmap(bmp, new Size(20, 20));
            btnConfiguracion.Image = bmpRedimensionado;
            
            dataGridView2.CellEndEdit += dataGridView2_CellEndEdit;
        }

        private void ActualizarGridProductos()
        {
            _productos = _prodRepository.Listar();
            _productos = _productos.OrderBy(x => x.Nombre).ToList();
            
            foreach (var prod in _productos)
            {
                prod.Sector = prod.ProductoSector == 1 ? "Si" : "No";
                
                if (prod.IdGrupoProducto != 0)
                    prod.NombreGrupo = _grupoRepository.BuscarPorId(prod.IdGrupoProducto).NombreGrupo;
                else
                    prod.NombreGrupo = "";
            }
            
            dataGridView1.DataSource = _productos;
            dataGridView1.Columns["Id"].Visible = false;
            dataGridView1.Columns["Codigo"].AutoSizeMode = DataGridViewAutoSizeColumnMode.DisplayedCells;
            dataGridView1.Columns["Nombre"].AutoSizeMode = DataGridViewAutoSizeColumnMode.DisplayedCells;
            dataGridView1.Columns["fechaModificacion"].HeaderText = "Fecha de Modificación";
            dataGridView1.Columns["fechaModificacion"].AutoSizeMode = DataGridViewAutoSizeColumnMode.DisplayedCells;
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
            
            lblCantProd.Text = _productos.Count.ToString();
            AplicarFiltro();
        }

        private void AplicarFiltro()
        {
            string filtro = txtBuscar.Text.ToLower();
            var filtrados = _productos.Where(p =>
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
            frmProducto frmProducto = new frmProducto(null, _prodRepository);
            frmProducto.FormClosed += (s, args) =>
            {
                if (frmProducto.DialogResult == DialogResult.OK)
                    ActualizarGridProductos();
            };
            frmProducto.Show();
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            AplicarFiltro();
        }

        private void dataGridViewProductos_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0) return;
            var producto = (Producto)dataGridView1.Rows[e.RowIndex].DataBoundItem;
            AgregarAlCarrito(producto);
        }

        private void AgregarAlCarrito(Producto producto)
        {
            var existente = _carrito.FirstOrDefault(i => i.Codigo == producto.Codigo);
            
            if (existente == null)
            {
                var item = new ItemSeleccionado
                {
                    Codigo = producto.Codigo,
                    Nombre = producto.Nombre,
                    Precio = producto.Precio,
                    Cantidad = 1,
                    IdProducto = producto.Id,
                    PrecioEditado = false
                };
                _carrito.Add(item);
            }
            else
            {
                if (producto.ProductoSector != 1)
                    existente.Cantidad += 1;
            }
            
            dataGridView2.Refresh();
            ActualizarPreciosCarrito();
            CalcularTotal();
            
            if (producto.ProductoSector == 1)
            {
                SeleccionarPrecioProductoSector();
            }
        }

        private void ActualizarPreciosCarrito()
        {
            for (int i = 0; i < _carrito.Count; i++)
            {
                var item = _carrito[i];
                if (item.Codigo.Contains("GENERIC-"))
                {
                    var precioGenerico = _preciosGenericos.FirstOrDefault(x => x.IdProducto == item.IdProducto);
                    if (precioGenerico != null && !item.PrecioEditado)
                    {
                        item.Precio = precioGenerico.Precio;
                    }
                }
                else
                {
                    var prod = _productos.FirstOrDefault(x => x.Id == item.IdProducto);
                    if (prod != null && !item.PrecioEditado)
                    {
                        item.Precio = prod.Precio;
                    }
                }
            }
        }

        private void SeleccionarPrecioProductoSector()
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
                var itemSeleccionado = dataGridView2.Rows[row].DataBoundItem as ItemSeleccionado;
                if (itemSeleccionado != null)
                {
                    itemSeleccionado.PrecioEditado = true;
                    
                    var productoSector = _productos.FirstOrDefault(x => x.Id == itemSeleccionado.IdProducto);
                    if (productoSector != null && productoSector.ProductoSector == 1)
                    {
                        itemSeleccionado.PrecioOriginal = itemSeleccionado.Precio;
                    }
                    
                    if (itemSeleccionado.Codigo.Contains("GENERIC-"))
                    {
                        var prodGenerico = new ItemSeleccionado
                        {
                            IdProducto = itemSeleccionado.IdProducto,
                            Precio = itemSeleccionado.Precio
                        };
                        _preciosGenericos.Add(prodGenerico);
                    }
                }
                CalcularTotal();
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
                    catch { }
                }));
            }
            catch { }
        }

        private void dataGridViewCarrito_CellValueChanged(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0) return;
            var columnName = dataGridView2.Columns[e.ColumnIndex].DataPropertyName;
            if (columnName == "Cantidad" || columnName == "Precio")
            {
                if (columnName == "Cantidad")
                {
                    var cellValue = dataGridView2.Rows[e.RowIndex].Cells[e.ColumnIndex].Value;
                    if (cellValue == null || string.IsNullOrWhiteSpace(cellValue.ToString())) return;
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
            if (cell == null) return;
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
            LimpiarCarrito();
        }

        private void LimpiarCarrito()
        {
            _carrito.Clear();
            _preciosGenericos.Clear();
            dataGridView2.Refresh();
            CalcularTotal();
        }

        private void btnEditar_Click(object sender, EventArgs e)
        {
            if (dataGridView1.CurrentRow == null)
            {
                MessageBox.Show("Se debe seleccionar un producto a editar.", "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            var producto = (Producto)dataGridView1.CurrentRow.DataBoundItem;
            using (var frmProducto = new frmProducto(producto, _prodRepository))
            {
                frmProducto.ShowDialog();
                if (frmProducto.DialogResult == DialogResult.OK)
                {
                    ActualizarGridProductos();
                }
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            if (dataGridView1.CurrentRow == null)
            {
                MessageBox.Show("Se debe seleccionar un producto a eliminar.", "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            var result = MessageBox.Show("¿Está seguro desea eliminar este Producto?",
                                "Confirmar eliminación",
                                MessageBoxButtons.YesNo,
                                MessageBoxIcon.Warning);
            if (result == DialogResult.Yes)
            {
                var producto = (Producto)dataGridView1.CurrentRow.DataBoundItem;
                _prodRepository.Eliminar(producto.Id);
                ActualizarGridProductos();
            }
        }

        private void dataGridView1_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            if (dataGridView1.Columns[e.ColumnIndex].Name == "Cantidad")
            {
                if (e.Value != null && int.TryParse(e.Value.ToString(), out int cantidad))
                {
                    if (cantidad < 5)
                    {
                        dataGridView1.Rows[e.RowIndex].DefaultCellStyle.BackColor = Color.LightSalmon;
                        dataGridView1.Rows[e.RowIndex].DefaultCellStyle.ForeColor = Color.Black;
                    }
                    else
                    {
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
                if (!ValidarMetodoPago()) return;
                
                if (_carrito.Count == 0) return;
                
                if (chkCosto.Checked)
                {
                    var result = MessageBox.Show("Está a punto de cobrar el ticket utilizando el costo del producto, en lugar del precio. ¿Desea continuar?",
                        "Confirmar",
                        MessageBoxButtons.YesNo,
                        MessageBoxIcon.Warning);
                    if (result == DialogResult.No) return;
                }

                GenerarInformeVenta();
                ActualizarStock();
                ActualizarGridProductos();

                if (_imprimirTicket)
                {
                    ImprimirTicket();
                }
                
                LimpiarCarrito();
                LimpiarUIAfterVenta();
                VolverAScanner();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private bool ValidarMetodoPago()
        {
            if (string.IsNullOrEmpty(_metodoDePago) &&
                (!chkMultiPago.Checked || (chkMultiPago.Checked && _multiplesMetodos.Count == 0)))
            {
                MessageBox.Show("Debe seleccionar un Método de pago", "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }
            return true;
        }

        private void ActualizarStock()
        {
            foreach (var item in _carrito)
            {
                if (item.Codigo.Contains("GENERIC-")) continue;
                
                var producto = _productos.FirstOrDefault(x => x.Id == item.IdProducto);
                if (producto?.ProductoSector != 1)
                {
                    producto!.Cantidad = Math.Max(0, producto.Cantidad - item.Cantidad);
                    _prodRepository.Actualizar(producto);
                }
            }
        }

        private void dataGridViewProductos_CellBeginEdit(object sender, DataGridViewCellCancelEventArgs e)
        {
            var prod = (ItemSeleccionado)dataGridView2.Rows[e.RowIndex].DataBoundItem;
            if (prod == null || prod.Codigo.Contains("GENERIC-"))
            {
                return;
            }
            
            if (dataGridView2.Columns[e.ColumnIndex].Name == "Precio")
            {
                var itemCarrito = (ItemSeleccionado)dataGridView2.Rows[e.RowIndex].DataBoundItem;
                var producto = _productos.FirstOrDefault(x => x.Id == itemCarrito.IdProducto);
                if (producto?.ProductoSector != 1)
                {
                    e.Cancel = true;
                }
            }

            if (dataGridView2.Columns[e.ColumnIndex].Name == "Cantidad")
            {
                var itemCarrito = (ItemSeleccionado)dataGridView2.Rows[e.RowIndex].DataBoundItem;
                var producto = _productos.FirstOrDefault(x => x.Id == itemCarrito.IdProducto);
                if (producto?.ProductoSector == 1)
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

        private void GenerarInformeVenta()
        {
            decimal total = _carrito.Sum(i => i.Subtotal);
            string descuentoStr = txtDescuento.Text.Trim().Replace("%", "");
            decimal descuento = 0;
            
            if (!chkDescuento.Checked)
            {
                descuentoStr = "0";
            }
            if (!TryParseDecimal(descuentoStr, out descuento))
            {
                MessageBox.Show("El descuento ingresado no es válido.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            
            string costo = chkCosto.Checked ? "Si" : "No";
            List<InformeVentaDetalle> informeDetalles = new();

            if (chkMultiPago.Checked && _multiplesMetodos.Count > 0)
            {
                bool detalleAsignado = false;
                foreach (var metodo in _multiplesMetodos)
                {
                    int idInformeVenta = _informeVentaRepository.InsertarInformeVenta(metodo.Monto, metodo.Descripcion, true, !detalleAsignado, descuento, costo);
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
                int idInformeVenta = _informeVentaRepository.InsertarInformeVenta(total, _metodoDePago, false, true, descuento, costo);
                foreach (var item in _carrito)
                {
                    informeDetalles.Add(new InformeVentaDetalle(item, idInformeVenta));
                }
            }

            foreach (var informe in informeDetalles)
                _informeVentaRepository.InsertarInformeVentaDetalle(informe);
        }

        private void btnVerInforme_Click(object sender, EventArgs e)
        {
            frmInformeVentas frmInformeVentas = new frmInformeVentas();
            frmInformeVentas.ShowDialog();
            if (frmInformeVentas.DialogResult == DialogResult.OK)
            {
                ActualizarGridProductos();
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
            input = input.Trim().Replace(',', '.');
            return decimal.TryParse(input, NumberStyles.Any, CultureInfo.InvariantCulture, out value);
        }

        private void txtScanner_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                string codigo = txtScanner.Text.Trim();
                txtScanner.Clear();
                if (!string.IsNullOrEmpty(codigo))
                {
                    var producto = _productos.FirstOrDefault(p => p.Codigo == codigo);
                    if (producto != null)
                    {
                        AgregarProductoPorScanner(producto);
                    }
                    else
                    {
                        var result = MessageBox.Show("El producto no existe. ¿Desea agregar uno nuevo con este código?",
                            "Producto no encontrado",
                            MessageBoxButtons.YesNo,
                            MessageBoxIcon.Question);
                        if (result == DialogResult.Yes)
                        {
                            frmProducto frmProducto = new frmProducto(codigo, _prodRepository, true);
                            frmProducto.ShowDialog();
                            if (frmProducto.DialogResult == DialogResult.OK)
                            {
                                ActualizarGridProductos();
                                producto = _productos.FirstOrDefault(p => p.Codigo == codigo);
                                if (producto != null)
                                {
                                    AgregarProductoPorScanner(producto);
                                }
                            }
                        }
                    }
                }
                CalcularTotal();
                VolverAScanner();
            }
            else if (e.KeyCode == Keys.Add)
            {
                IrAMedioDePago();
                e.SuppressKeyPress = true;
            }
        }

        private void AgregarProductoPorScanner(Producto producto)
        {
            var itemExistente = _carrito.FirstOrDefault(c => c.Codigo == producto.Codigo);
            if (itemExistente != null && producto.ProductoSector != 1)
            {
                itemExistente.Cantidad++;
            }
            else if (producto.ProductoSector == 1 && itemExistente != null)
            {
                SeleccionarPrecioProductoSector();
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
                    IdProducto = producto.Id,
                    PrecioEditado = false
                });
            }
            ActualizarPreciosCarrito();
            dataGridView2.DataSource = null;
            dataGridView2.DataSource = _carrito;
            if (producto.ProductoSector == 1)
            {
                SeleccionarPrecioProductoSector();
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
            Configuracion frmcofig = new Configuracion("", "", false, "");
            frmcofig.ShowDialog();
            ActualizarGridProductos();
        }

        private void btnCodeBar_Click(object sender, EventArgs e)
        {
            if (dataGridView1.CurrentRow == null)
            {
                MessageBox.Show("Se debe seleccionar un producto para generar el código de Barras", "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            var result = MessageBox.Show("Se va a generar e imprimir el código de barras. ¿Desea continuar?",
                                "Confirmar",
                                MessageBoxButtons.YesNo,
                                MessageBoxIcon.Information);
            if (result != DialogResult.Yes) return;

            int cantidad = 1;
            var result2 = MessageBox.Show("¿Desea imprimir más de un código de barra?",
                "Confirmar",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Information);
            if (result2 == DialogResult.Yes)
            {
                string input = Interaction.InputBox("Ingrese la cantidad de códigos a imprimir:", "Cantidad", "1");
                if (!int.TryParse(input, out cantidad) || cantidad <= 0)
                {
                    MessageBox.Show("Cantidad inválida, se imprimirá 1 código.", "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    cantidad = 1;
                }
            }
            var producto = (Producto)dataGridView1.CurrentRow.DataBoundItem;
            string impresoraPorDefecto = new PrinterSettings().PrinterName;
            TicketPrinter ticketPrinter = new TicketPrinter();
            ticketPrinter.PrintBarcode(impresoraPorDefecto, producto.Codigo, cantidad);
        }

        private void ActualizarTotal()
        {
            decimal total = 0;
            foreach (DataGridViewRow row in dataGridView2.Rows)
            {
                if (row.Cells["Cantidad"].Value != null && row.Cells["Precio"].Value != null &&
                    decimal.TryParse(row.Cells["Cantidad"].Value.ToString(), out decimal cantidad) &&
                    decimal.TryParse(row.Cells["Precio"].Value.ToString(), out decimal precio))
                {
                    total += cantidad * precio;
                }
            }
            lblTotal.Text = total.ToString("N2");
        }

        private void dataGridView2_CellMouseDoubleClick(object sender, DataGridViewCellMouseEventArgs e)
        {
            if (e.RowIndex < 0 || e.RowIndex >= dataGridView2.Rows.Count) return;
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
                    _metodosDePago.Add(new MetodoDePago { Descripcion = nuevo, Monto = 0 });
                }
                else
                {
                    cbMetodosPago.SelectedIndex = -1;
                }
            }
            else
            {
                _metodoDePago = cbMetodosPago.SelectedItem?.ToString();
            }
        }

        private void CargarMetodosPago()
        {
            cbMetodosPago.Items.Clear();
            _metodosDePago.Clear();
            
            var metodos = _metodoPagoRepository.ObtenerTodos();
            foreach (var metodo in metodos)
            {
                cbMetodosPago.Items.Add(metodo);
                _metodosDePago.Add(new Domain.MetodoDePago { Descripcion = metodo });
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
                frmMetodoPago frmMetodoPago = new frmMetodoPago(_metodosDePago, _carrito.Sum(i => i.Subtotal));
                frmMetodoPago.ShowDialog();
                if (frmMetodoPago.DialogResult == DialogResult.OK)
                {
                    _multiplesMetodos = frmMetodoPago._metodosDePago;
                }
                else
                {
                    chkMultiPago.Checked = false;
                }
            }
        }

        private void btnGrupos_Click(object sender, EventArgs e)
        {
            Grupo frmGrupo = new Grupo();
            frmGrupo.FormClosed += (s, args) => ActualizarGridProductos();
            frmGrupo.Show();
        }

        private void chkImprimirTicket_CheckedChanged(object sender, EventArgs e)
        {
            _imprimirTicket = chkImprimirTicket.Checked;
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
                IdProducto = Math.Abs(Guid.NewGuid().ToString().GetHashCode()),
                PrecioEditado = false
            };
            _carrito.Add(itemSeleccionado);
            dataGridView2.DataSource = null;
            dataGridView2.DataSource = _carrito;
            int lastRow = dataGridView2.Rows.Count - 1;
            BeginInvoke(new Action(() =>
            {
                var cell = dataGridView2.Rows[lastRow].Cells[1];
                cell.ReadOnly = false;
                dataGridView2.CurrentCell = cell;
                dataGridView2.BeginEdit(true);
            }));
        }

        private void cbMetodosPago_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Add)
            {
                btnCobrar.Focus();
                e.SuppressKeyPress = true;
            }
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
            string descuentoStr = txtDescuento.Text.Replace("%", "").Trim();
            int.TryParse(descuentoStr, out int descuento);
            
            foreach (var item in _carrito)
            {
                if (item.PrecioEditado)
                {
                    var productoSector = _productos.FirstOrDefault(x => x.Id == item.IdProducto);
                    if (productoSector != null && productoSector.ProductoSector == 1)
                    {
                        item.Precio = item.PrecioOriginal;
                    }
                    else
                    {
                        continue;
                    }
                }
                
                if (item.Codigo.Contains("GENERIC-"))
                {
                    var prod = _preciosGenericos.FirstOrDefault(x => x.IdProducto == item.IdProducto);
                    if (prod == null) continue;
                    item.Precio = prod.Precio;
                }
                else
                {
                    var producto = _productos.FirstOrDefault(x => x.Id == item.IdProducto);
                    if (producto == null) continue;
                    item.Precio = usarCosto ? producto.Costo : producto.Precio;
                }

                if (chkDescuento.Checked && descuento > 0)
                {
                    item.Precio = item.Precio - (item.Precio * descuento / 100m);
                }
            }
        }

        private void CalcularTotal()
        {
            AplicarDescuento();
            decimal total = _carrito.Sum(i => i.Cantidad * i.Precio);
            dataGridView2.Refresh();
            txtTotal.Text = total.ToString("C2", new CultureInfo("es-AR"));
            decimal items = _carrito.Sum(i => i.Cantidad);
            lblItems.Text = items.ToString("0", new CultureInfo("es-AR"));
        }

        private void LimpiarUIAfterVenta()
        {
            _multiplesMetodos.Clear();
            chkMultiPago.Checked = false;
            lblItems.Text = "0";
            txtTotal.Text = "0";
            chkCosto.Checked = false;
            chkDescuento.Checked = false;
            txtDescuento.Text = string.Empty;
        }

        private void txtDescuento_Enter(object sender, EventArgs e)
        {
            string valor = txtDescuento.Text.Replace("%", "").Trim();
            txtDescuento.Text = valor;
            txtDescuento.SelectAll();
        }

        private void txtDescuento_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (!char.IsDigit(e.KeyChar) && !char.IsControl(e.KeyChar))
            {
                e.Handled = true;
                return;
            }
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