using StockControl.Domain;
using StockControl.Repository;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Printing;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace StockControl
{
    public partial class frmInformeVentas : Form
    {
        private List<InformeVenta> informeVentas = new();
        private InformeVentaRepository _informeVentaRepository = new InformeVentaRepository();
        private PagoProveedorRepository _pagoProveedorRepository = new PagoProveedorRepository();
        private MovimientoStockRepository _movimientoStockRepository = new MovimientoStockRepository();
        private ProductoRepository _productoRepository = new ProductoRepository();
        private List<Caja> Cajas = new();
        private List<Caja> cajasFiltradas = new();
        private List<CajaProducto> productosCierre = new();
        public static List<ItemSeleccionado> ticket = new();
        private string lastSortColumn = null;
        private bool lastSortAsc = false;
        private string lastSortColumnCajas;
        private bool lastSortAscCajas = true;
        private string lastSortColumnProductos;
        private bool lastSortAscProductos = true;
        public frmInformeVentas()
        {
            InitializeComponent();
            this.Icon = new Icon("Resources\\stockIcon.ico");
            dataGridView1.ReadOnly = true;
            dtCajas.ReadOnly = true;
            dtCajas.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dtCajas.MultiSelect = false;
            dtProductos.ReadOnly = true;
            dtProductos.AllowUserToAddRows = false;
            dtProductos.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dtProductos.MultiSelect = false;
            dataGridView1.ColumnHeaderMouseClick += dataGridView1_ColumnHeaderMouseClick;
            dtCajas.CellClick += dtCajas_CellClick;
            dtProductos.ColumnHeaderMouseClick += dtProductos_ColumnHeaderMouseClick;

            LoadData();
        }

        private void LoadData()
        {
            _informeVentaRepository.RecalcularSubtotales();
            informeVentas = _informeVentaRepository.ListarInformeVenta();
            informeVentas.ForEach(x =>
            {
                x.DetalleAdjuntoStr = x.DetalleAdjunto == 1 ? "Sí" : "No";
                x.MultipleMetodoDePagoStr = x.MultipleMetodoDePago == 1 ? "Sí" : "No";
            });
            informeVentas = informeVentas.OrderByDescending(i => i.Fecha).ToList();
            BindGrid();
            CargarDtCajas();
        }

        private void BindGrid()
        {
            dataGridView1.DataSource = null;
            dataGridView1.DataSource = informeVentas;

            dataGridView1.Columns["IdInformeVenta"].Visible = false;
            dataGridView1.Columns["MultipleMetodoDePago"].Visible = false;
            dataGridView1.Columns["DetalleAdjunto"].Visible = false;
            if (dataGridView1.Columns["IdGrupo"] != null)
                dataGridView1.Columns["IdGrupo"].Visible = false;
            dataGridView1.Columns["MultipleMetodoDePagoStr"].HeaderText = "Método de Pago Múltiple";
            dataGridView1.Columns["DetalleAdjuntoStr"].HeaderText = "Detalle Adjunto";
            dataGridView1.Columns["Fecha"].DefaultCellStyle.Format = "dd/MM/yyyy HH:mm:ss";
            dataGridView1.Columns["Total"].DefaultCellStyle.FormatProvider = new CultureInfo("es-AR");
            dataGridView1.Columns["Total"].DefaultCellStyle.Format = "C2";
            dataGridView1.Columns["subTotal"].HeaderText = "Subtotal";
            dataGridView1.Columns["subTotal"].DefaultCellStyle.FormatProvider = new CultureInfo("es-AR");
            dataGridView1.Columns["subTotal"].DefaultCellStyle.Format = "C2";
            if (dataGridView1.Columns["Descuento"] != null)
            {
                dataGridView1.Columns["Descuento"].HeaderText = "Descuento %";
                dataGridView1.Columns["Descuento"].DefaultCellStyle.Format = "0'%'";
            }
            if (dataGridView1.Columns["PrecioCosto"] != null)
                dataGridView1.Columns["PrecioCosto"].HeaderText = "Precio al costo";

            string[] orden =
            {
                "Fecha",
                "MetodoPago",
                "subTotal",
                "Descuento",
                "Total",
                "PrecioCosto",
                "MultipleMetodoDePagoStr",
                "DetalleAdjuntoStr"
            };
            int indice = 0;
            foreach (var nombre in orden)
            {
                if (dataGridView1.Columns[nombre] != null)
                    dataGridView1.Columns[nombre].DisplayIndex = indice++;
            }

            foreach (DataGridViewColumn column in dataGridView1.Columns)
                column.SortMode = DataGridViewColumnSortMode.Programmatic;

            dataGridView1.Refresh();
        }
        private void dataGridView1_ColumnHeaderMouseClick(object sender, DataGridViewCellMouseEventArgs e)
        {
            var column = dataGridView1.Columns[e.ColumnIndex];
            var propName = column.DataPropertyName;
            if (string.IsNullOrEmpty(propName)) return;

            bool asc = true;
            if (lastSortColumn == propName)
                asc = !lastSortAsc; // toggle
            else
                asc = true;

            // ordenar usando reflection
            if (asc)
                informeVentas = informeVentas.OrderBy(x => GetPropValue(x, propName)).ToList();
            else
                informeVentas = informeVentas.OrderByDescending(x => GetPropValue(x, propName)).ToList();

            // volver a bindear
            BindGrid();

            // setear glyph
            foreach (DataGridViewColumn col in dataGridView1.Columns)
                col.HeaderCell.SortGlyphDirection = SortOrder.None;

            var currentCol = dataGridView1.Columns
                               .Cast<DataGridViewColumn>()
                               .FirstOrDefault(c => c.DataPropertyName == propName);

            if (currentCol != null)
                currentCol.HeaderCell.SortGlyphDirection = asc ? SortOrder.Ascending : SortOrder.Descending;

            lastSortColumn = propName;
            lastSortAsc = asc;
        }

        private object GetPropValue(object obj, string propName)
        {
            var prop = obj.GetType().GetProperty(propName);
            return prop?.GetValue(obj, null);
        }
        private void btnEliminar_Click(object sender, EventArgs e)
        {
            if (dataGridView1.CurrentRow == null)
            {
                MessageBox.Show($"Se debe seleccionar un informe a eliminar,", "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            var result = MessageBox.Show("¿Estás seguro desea eliminar este informe? Se reestablecerá el stock de los productos asociados a este informe.",
                                "Confirmar eliminacion",
                                MessageBoxButtons.YesNo,
                                MessageBoxIcon.Warning
                            );

            if (result == DialogResult.Yes)
            {
                var informe = (InformeVenta)dataGridView1.CurrentRow.DataBoundItem;
                var informeDetalle = _informeVentaRepository.ListarInformeVentaDetalle(informe.IdInformeVenta);
                if (informeDetalle.Count > 0)
                {
                    foreach (var item in informeDetalle)
                    {
                        ReestablecerStock(item);
                    }
                }
                _informeVentaRepository.EliminarInformeVenta(informe.IdInformeVenta);
                LoadData();
                MessageBox.Show("Informe eliminado correctamente.", "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Information);
                this.DialogResult = DialogResult.OK;
            }
        }

        private void ReestablecerStock(InformeVentaDetalle item)
        {
            ProductoRepository _productoRepository = new ProductoRepository();
            var producto = _productoRepository.BuscarPorCodigo(item.Codigo);
            if (producto != null && producto.ProductoSector != 1)
            {
                producto.Cantidad += item.Cantidad;
                _productoRepository.Actualizar(producto);
            }
        }

        private void btnSalir_Click(object sender, EventArgs e)
        {
            this.Close();
        }
        private void dataGridView1_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0)
            {
                var informe = (InformeVenta)dataGridView1.Rows[e.RowIndex].DataBoundItem;
                var informes = _informeVentaRepository.ListarInformeVentaDetalle(informe.IdInformeVenta);
                frmInformeVentaDetalle frmInformeVentaDetalle = new frmInformeVentaDetalle(informes);
                frmInformeVentaDetalle.ShowDialog();
            }
        }

        private void btnCerrarCaja_Click(object sender, EventArgs e)
        {
            var result = MessageBox.Show("¿Estás seguro que deseas cerrar la caja del día de hoy?",
                                "Confirmar cierre de caja",
                                MessageBoxButtons.YesNo,
                                MessageBoxIcon.Warning
                            );

            if (result == DialogResult.No) return;

            CerrarCaja(DateTime.Now, restarPagosProveedor: true);
        }

        private void btnPagoProveedor_Click(object sender, EventArgs e)
        {
            using var form = new frmPagoProveedor();
            form.ShowDialog();
        }

        private void CargarDtCajas()
        {
            Cajas = _informeVentaRepository.ListarCajas();
            cajasFiltradas = Cajas
                .Where(c => c.Fecha.Date == DateTime.Now.Date)
                .OrderByDescending(x => x.Fecha)
                .ToList();

            dtCajas.DataSource = cajasFiltradas;

            foreach (DataGridViewColumn column in dtCajas.Columns)
                column.SortMode = DataGridViewColumnSortMode.Programmatic;

            ConfigurarColumnasCajas(dtCajas);

            // Asociar el evento (por si no lo hiciste en el diseñador)
            dtCajas.ColumnHeaderMouseClick -= dtCajas_ColumnHeaderMouseClick;
            dtCajas.ColumnHeaderMouseClick += dtCajas_ColumnHeaderMouseClick;
        }

        private void dtCajas_ColumnHeaderMouseClick(object sender, DataGridViewCellMouseEventArgs e)
        {
            var column = dtCajas.Columns[e.ColumnIndex];
            var propName = column.DataPropertyName;
            if (string.IsNullOrEmpty(propName)) return;

            bool asc = true;
            if (lastSortColumnCajas == propName)
                asc = !lastSortAscCajas; // alternar
            else
                asc = true;

            // Detectar qué lista está activa
            bool mostrandoTodos = chkTodos.Checked;
            var listaActual = mostrandoTodos ? Cajas : cajasFiltradas;

            // ordenar usando reflection
            if (asc)
                listaActual = listaActual.OrderBy(x => GetPropValue(x, propName)).ToList();
            else
                listaActual = listaActual.OrderByDescending(x => GetPropValue(x, propName)).ToList();

            // reasignar al origen correcto
            if (mostrandoTodos)
                Cajas = listaActual;
            else
                cajasFiltradas = listaActual;

            // volver a bindear
            dtCajas.DataSource = null;
            dtCajas.DataSource = listaActual;

            foreach (DataGridViewColumn col in dtCajas.Columns)
                col.SortMode = DataGridViewColumnSortMode.Programmatic;

            // setear glyphs
            foreach (DataGridViewColumn col in dtCajas.Columns)
                col.HeaderCell.SortGlyphDirection = SortOrder.None;

            var currentCol = dtCajas.Columns
                               .Cast<DataGridViewColumn>()
                               .FirstOrDefault(c => c.DataPropertyName == propName);

            if (currentCol != null)
                currentCol.HeaderCell.SortGlyphDirection = asc ? SortOrder.Ascending : SortOrder.Descending;

            lastSortColumnCajas = propName;
            lastSortAscCajas = asc;
            ConfigurarColumnasCajas(dtCajas);
        }

        private void ConfigurarColumnasCajas(DataGridView grid)
        {
            if (grid.Columns["IdCaja"] != null)
                grid.Columns["IdCaja"].Visible = false;
            if (grid.Columns["IdCierre"] != null)
                grid.Columns["IdCierre"].Visible = false;
            if (grid.Columns["CantidadVentas"] != null)
                grid.Columns["CantidadVentas"].HeaderText = "Cantidad de ventas";
            if (grid.Columns["MetodoPago"] != null)
                grid.Columns["MetodoPago"].HeaderText = "Método de pago";
            FormatearDinero(grid, "Total");
            FormatearDinero(grid, "Ganancia");
            FormatearDinero(grid, "GenericosOSector");
            FormatearDinero(grid, "PagoProveedor");
            if (grid.Columns["GenericosOSector"] != null)
                grid.Columns["GenericosOSector"].HeaderText = "Genéricos o sector";
            if (grid.Columns["PagoProveedor"] != null)
                grid.Columns["PagoProveedor"].HeaderText = "Pago a proveedores";
            if (grid.Columns["Ganancia"] != null)
                grid.Columns["Ganancia"].HeaderText = "Ganancia";

            OrdenarColumnas(grid,
                "Fecha",
                "MetodoPago",
                "CantidadVentas",
                "GenericosOSector",
                "PagoProveedor",
                "Total",
                "Ganancia");
        }

        private static void OrdenarColumnas(DataGridView grid, params string[] nombres)
        {
            int indice = 0;
            foreach (var nombre in nombres)
            {
                if (grid.Columns[nombre] != null)
                    grid.Columns[nombre].DisplayIndex = indice++;
            }
        }

        private static void FormatearDinero(DataGridView grid, string columna)
        {
            if (grid.Columns[columna] == null)
                return;
            grid.Columns[columna].DefaultCellStyle.FormatProvider = new CultureInfo("es-AR");
            grid.Columns[columna].DefaultCellStyle.Format = "C2";
        }

        private void chkTodos_CheckedChanged(object sender, EventArgs e)
        {
            if (chkTodos.Checked)
            {
                dtCajas.DataSource = Cajas;
            }
            else
                dtCajas.DataSource = cajasFiltradas;

            ConfigurarColumnasCajas(dtCajas);
            dtCajas.Refresh();
        }

        private void btnCopiarTicket_Click(object sender, EventArgs e)
        {
            var row = dataGridView1.CurrentRow;
            if (row == null)
            {
                MessageBox.Show("No hay ninguna fila seleccionada.", "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            var result = MessageBox.Show("Se copiara la venta al carrito actual, ¿Desea continuar?",
                                "Confirmar copia de ticket",
                                MessageBoxButtons.YesNo,
                                MessageBoxIcon.Warning
                            );
            if(result == DialogResult.No) return;

            var informe = (InformeVenta)dataGridView1.CurrentRow.DataBoundItem;
            var informeDetalle = _informeVentaRepository.ListarInformeVentaDetalle(informe.IdInformeVenta);

            if(informeDetalle.Count == 0)
            {
                MessageBox.Show("El informe seleccionado no tiene detalles para copiar.", "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            foreach (var item in informeDetalle)
            {
                Producto producto = new Producto();
                var productoRepo = new ProductoRepository();
                if(item.Codigo.Contains("GENERIC"))
                {
                    producto.Cantidad = item.Cantidad;
                    producto.Nombre = item.Nombre;
                    producto.Precio = item.Precio;
                    producto.Codigo = item.Codigo;
                    producto.Id = Guid.NewGuid().GetHashCode();
                }
                else
                 producto = productoRepo.BuscarPorCodigo(item.Codigo);

                if(producto == null)
                {
                    MessageBox.Show($"El producto con código {item.Codigo} no existe en el sistema. No se podrá agregar al carrito.", "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    continue;
                }
                ticket.Add(new ItemSeleccionado
                {
                    Codigo = item.Codigo,
                    Nombre = item.Nombre,
                    Cantidad = item.Cantidad,
                    Precio = item.Precio,
                    IdProducto = producto.Id
                });
            }
            DialogResult = DialogResult.Yes;
            this.Close();
        }
        private void btnImprimirTicket_Click(object sender, EventArgs e)
        {
            var row = dataGridView1.CurrentRow;
            if (row == null)
            {
                MessageBox.Show("No hay ninguna fila seleccionada.", "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var informe = (InformeVenta)dataGridView1.CurrentRow.DataBoundItem;
            var informeDetalle = _informeVentaRepository.ListarInformeVentaDetalle(informe.IdInformeVenta);

            if (informeDetalle.Count == 0)
            {
                MessageBox.Show("El informe seleccionado no tiene detalles para imprimir.", "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            List<ItemSeleccionado> items = new();
            foreach (var item in informeDetalle)
            {
                items.Add(new ItemSeleccionado
                {
                    Codigo = item.Codigo,
                    Nombre = item.Nombre,
                    Cantidad = item.Cantidad,
                    Precio = item.Precio,
                    IdProducto = 0
                });
            }

            try
            {
                TicketPrinter ticketPrinter = new TicketPrinter(items);
                string impresoraPorDefecto = new PrinterSettings().PrinterName;
                ticketPrinter.PrintTicketFinal(impresoraPorDefecto);
                MessageBox.Show("Ticket impreso correctamente.", "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al imprimir el ticket: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        private void btnCerrarCajaAnterior_Click(object sender, EventArgs e)
        {
            using (var form = new FrmCerrarCajaAnterior())
            {
                if (form.ShowDialog() == DialogResult.OK)
                {
                    DateTime fechaSeleccionada = form.FechaSeleccionada;

                    var result = MessageBox.Show($"¿Cerrar la caja del día {fechaSeleccionada:dd/MM/yyyy}?",
                                                 "Confirmar cierre de caja",
                                                 MessageBoxButtons.YesNo,
                                                 MessageBoxIcon.Warning);

                    if (result == DialogResult.No)
                        return;

                    CerrarCaja(fechaSeleccionada, restarPagosProveedor: false);
                }
            }
        }

        private void CerrarCaja(DateTime fecha, bool restarPagosProveedor)
        {
            Cursor = Cursors.WaitCursor;
            var informes = restarPagosProveedor
                ? _informeVentaRepository.ListarParaCerrarCaja()
                : _informeVentaRepository.ListarParaCerrarCaja(fecha.Date);
            var pagos = restarPagosProveedor
                ? _pagoProveedorRepository.ListarDelDia(fecha.Date)
                : new List<PagoProveedor>();

            if (informes.Count == 0 && pagos.Count == 0)
            {
                MessageBox.Show(
                    restarPagosProveedor
                        ? "No hay ventas para cerrar la caja del día de hoy."
                        : "No hay ventas para esa fecha.",
                    "Aviso",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
                Cursor = Cursors.Default;
                return;
            }

            var detalles = _informeVentaRepository.ListarDetallesDe(informes.Select(i => i.IdInformeVenta));
            var detallesPorInforme = detalles
                .GroupBy(d => d.IdInformeVenta)
                .ToDictionary(g => g.Key, g => g.ToList());
            var productos = _productoRepository.Listar()
                .Where(p => !string.IsNullOrEmpty(p.Codigo))
                .GroupBy(p => p.Codigo, StringComparer.OrdinalIgnoreCase)
                .ToDictionary(g => g.Key, g => g.First(), StringComparer.OrdinalIgnoreCase);

            DateTime desdeElDiaSiguiente = fecha.Date.AddDays(1);
            string idCierre = Guid.NewGuid().ToString("N");
            var resultado = CierreCaja.Calcular(
                informes,
                detallesPorInforme,
                pagos,
                productos,
                _informeVentaRepository.SumarCantidadVendidaDesde(desdeElDiaSiguiente),
                _movimientoStockRepository.SumarDesde(desdeElDiaSiguiente),
                fecha,
                idCierre,
                restarPagosProveedor);

            _informeVentaRepository.InsertarCajaCerradaPorLista(resultado.Cajas);
            _informeVentaRepository.InsertarProductosCierre(resultado.Productos);
            CargarDtCajas();
            MostrarProductos(idCierre, fecha);
            tabInformes.SelectedTab = tabPage4;
            Cursor = Cursors.Default;
            MessageBox.Show("Caja cerrada correctamente.", "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void dtCajas_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0)
                return;
            if (dtCajas.Rows[e.RowIndex].DataBoundItem is not Caja caja)
                return;
            if (string.IsNullOrEmpty(caja.IdCierre))
            {
                productosCierre = new List<CajaProducto>();
                dtProductos.DataSource = null;
                lblProductos.Text = "Esta caja no tiene el listado de productos.";
                return;
            }
            MostrarProductos(caja.IdCierre, caja.Fecha);
        }

        private void MostrarProductos(string idCierre, DateTime fecha)
        {
            productosCierre = _informeVentaRepository.ListarProductosCierre(idCierre);
            lblProductos.Text = $"Productos vendidos del {fecha:dd/MM/yyyy}";
            EnlazarProductos();
        }

        private void EnlazarProductos()
        {
            dtProductos.DataSource = null;
            dtProductos.DataSource = productosCierre;
            ConfigurarColumnasProductos(dtProductos);
        }

        private void dtProductos_ColumnHeaderMouseClick(object sender, DataGridViewCellMouseEventArgs e)
        {
            var column = dtProductos.Columns[e.ColumnIndex];
            var propName = column.DataPropertyName;
            if (string.IsNullOrEmpty(propName))
                return;

            bool asc = lastSortColumnProductos != propName || !lastSortAscProductos;
            productosCierre = asc
                ? productosCierre.OrderBy(x => GetPropValue(x, propName)).ToList()
                : productosCierre.OrderByDescending(x => GetPropValue(x, propName)).ToList();

            EnlazarProductos();
            foreach (DataGridViewColumn col in dtProductos.Columns)
                col.HeaderCell.SortGlyphDirection = SortOrder.None;

            var actual = dtProductos.Columns
                .Cast<DataGridViewColumn>()
                .FirstOrDefault(c => c.DataPropertyName == propName);
            if (actual != null)
                actual.HeaderCell.SortGlyphDirection = asc ? SortOrder.Ascending : SortOrder.Descending;

            lastSortColumnProductos = propName;
            lastSortAscProductos = asc;
        }

        private void ConfigurarColumnasProductos(DataGridView grid)
        {
            if (grid.Columns["IdCajaProducto"] != null)
                grid.Columns["IdCajaProducto"].Visible = false;
            if (grid.Columns["IdCierre"] != null)
                grid.Columns["IdCierre"].Visible = false;
            if (grid.Columns["Codigo"] != null)
                grid.Columns["Codigo"].HeaderText = "Código";
            if (grid.Columns["CantidadVendida"] != null)
                grid.Columns["CantidadVendida"].HeaderText = "Cantidad de ventas";
            if (grid.Columns["StockInicial"] != null)
            {
                grid.Columns["StockInicial"].HeaderText = "Stock inicial";
                grid.Columns["StockInicial"].DefaultCellStyle.Format = "0.##";
                grid.Columns["StockInicial"].DefaultCellStyle.NullValue = "";
            }
            if (grid.Columns["StockFinal"] != null)
            {
                grid.Columns["StockFinal"].HeaderText = "Stock final";
                grid.Columns["StockFinal"].DefaultCellStyle.Format = "0.##";
                grid.Columns["StockFinal"].DefaultCellStyle.NullValue = "";
            }
            if (grid.Columns["Ganancia"] != null)
            {
                grid.Columns["Ganancia"].DefaultCellStyle.FormatProvider = new CultureInfo("es-AR");
                grid.Columns["Ganancia"].DefaultCellStyle.Format = "C2";
                grid.Columns["Ganancia"].DefaultCellStyle.NullValue = "";
            }
            ReemplazarCheck(grid, "EsSector", "Sector");
            ReemplazarCheck(grid, "EsGenerico", "Genérico");
            foreach (DataGridViewColumn column in grid.Columns)
                column.SortMode = DataGridViewColumnSortMode.Programmatic;
        }

        private static void ReemplazarCheck(DataGridView grid, string nombre, string titulo)
        {
            if (grid.Columns[nombre] == null)
                return;
            if (grid.Columns[nombre] is DataGridViewCheckBoxColumn)
            {
                grid.Columns[nombre].HeaderText = titulo;
                return;
            }

            int index = grid.Columns[nombre].Index;
            grid.Columns.Remove(nombre);
            grid.Columns.Insert(index, new DataGridViewCheckBoxColumn
            {
                Name = nombre,
                DataPropertyName = nombre,
                HeaderText = titulo,
                TrueValue = 1,
                FalseValue = 0,
                SortMode = DataGridViewColumnSortMode.Programmatic
            });
        }
        private void btnGenerarResumen_Click(object sender, EventArgs e)
        {
            DateTime fechaSeleccionada = dtpMes.Value;
            DateTime desde = new DateTime(fechaSeleccionada.Year, fechaSeleccionada.Month, 1);
            DateTime hasta = desde.AddMonths(1);

            var cajasDelMes = _informeVentaRepository.ListarCajasPorRango(desde, hasta);

            var resumen = cajasDelMes
                .Where(c => c.MetodoPago != "Total")
                .GroupBy(c => c.MetodoPago)
                .Select(g => new Caja
                {
                    MetodoPago = g.Key,
                    Total = g.Sum(c => c.Total),
                    CantidadVentas = g.Sum(c => c.CantidadVentas),
                    Ganancia = g.Sum(c => c.Ganancia),
                    GenericosOSector = g.Sum(c => c.GenericosOSector),
                    PagoProveedor = g.Sum(c => c.PagoProveedor)
                })
                .OrderBy(c => c.MetodoPago)
                .ToList();

            decimal totalMes = resumen.Sum(c => c.Total);
            int cantidadVentasMes = resumen.Sum(c => c.CantidadVentas);
            decimal gananciaMes = resumen.Sum(c => c.Ganancia);
            decimal pagosMes = resumen.Sum(c => c.PagoProveedor);
            resumen.Add(new Caja
            {
                MetodoPago = "Total",
                Total = totalMes,
                CantidadVentas = cantidadVentasMes,
                Ganancia = gananciaMes,
                GenericosOSector = resumen.Sum(c => c.GenericosOSector),
                PagoProveedor = pagosMes
            });

            dtResumen.DataSource = null;
            dtResumen.DataSource = resumen;

            dtResumen.Columns["IdCaja"].Visible = false;
            dtResumen.Columns["Fecha"].Visible = false;
            if (dtResumen.Columns["IdCierre"] != null)
                dtResumen.Columns["IdCierre"].Visible = false;
            ConfigurarColumnasCajas(dtResumen);

            var cultura = new CultureInfo("es-AR");
            lblTotalMes.Text = $"Total del mes: {totalMes.ToString("C2", cultura)}    Ganancia: {gananciaMes.ToString("C2", cultura)}    Pago a proveedores: {pagosMes.ToString("C2", cultura)}";

            if (resumen.Count == 1)
            {
                MessageBox.Show("No hay cajas cerradas para el mes seleccionado.", "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

    }
}
