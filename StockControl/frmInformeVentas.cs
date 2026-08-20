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
        private List<Caja> Cajas = new();
        private List<Caja> cajasFiltradas = new();
        public static List<ItemSeleccionado> ticket = new();
        private string lastSortColumn = null;
        private bool lastSortAsc = false;
        private string lastSortColumnCajas;
        private bool lastSortAscCajas = true;
        public frmInformeVentas()
        {
            InitializeComponent();
            this.Icon = new Icon("Resources\\stockIcon.ico");
            dataGridView1.ReadOnly = true;
            dtCajas.ReadOnly = true;
            dataGridView1.ColumnHeaderMouseClick += dataGridView1_ColumnHeaderMouseClick;

            LoadData();
        }

        private void LoadData()
        {
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
            dataGridView1.Columns["MultipleMetodoDePagoStr"].HeaderText = "Método de Pago Múltiple";
            dataGridView1.Columns["DetalleAdjuntoStr"].HeaderText = "Detalle Adjunto";
            dataGridView1.Columns["Fecha"].DefaultCellStyle.Format = "dd/MM/yyyy HH:mm:ss";
            dataGridView1.Columns["Total"].DefaultCellStyle.FormatProvider = new CultureInfo("es-AR");
            dataGridView1.Columns["Total"].DefaultCellStyle.Format = "C2";
            dataGridView1.Columns["subTotal"].DefaultCellStyle.FormatProvider = new CultureInfo("es-AR");
            dataGridView1.Columns["subTotal"].DefaultCellStyle.Format = "C2";
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

            Cursor = Cursors.WaitCursor;
            var informesDelDia = _informeVentaRepository.ListarParaCerrarCaja();
            List<Caja> informeCajaCerrada = informesDelDia.GroupBy(i => i.MetodoPago)
                                           .Select(g => new Caja
                                           {
                                               MetodoPago = g.Key,
                                               Total = g.Sum(i => i.Total),
                                               CantidadVentas = g.Count()
                                           }).ToList();

            var informeTotalDia = informeCajaCerrada.Sum(i => i.Total);


            if (informeCajaCerrada.Count == 0)
            {
                MessageBox.Show("No hay ventas para cerrar la caja del día de hoy.", "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Information);
                Cursor = Cursors.Default;
                return;
            }
            informeCajaCerrada.Add(new Caja
            {
                MetodoPago = "Total",
                Total = informeTotalDia,
                CantidadVentas = informesDelDia.Count
            });
            _informeVentaRepository.InsertarCajaCerradaPorLista(informeCajaCerrada);

            CargarDtCajas();
            Cursor = Cursors.Default;
            MessageBox.Show("Caja cerrada correctamente.", "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Information);
            tabInformes.SelectedTab = tabPage2;

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
            if (grid.Columns["CantidadVentas"] != null)
                grid.Columns["CantidadVentas"].HeaderText = "Cantidad de ventas";
            if (grid.Columns["Total"] != null)
            {
                grid.Columns["Total"].DefaultCellStyle.FormatProvider = new CultureInfo("es-AR");
                grid.Columns["Total"].DefaultCellStyle.Format = "C2";
            }
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

                    Cursor = Cursors.WaitCursor;

                    var informesDelDia = _informeVentaRepository.ListarParaCerrarCaja(fechaSeleccionada);

                    if (informesDelDia.Count == 0)
                    {
                        MessageBox.Show("No hay ventas para esa fecha.", "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        Cursor = Cursors.Default;
                        return;
                    }

                    var informeCajaCerrada = informesDelDia
                        .GroupBy(i => i.MetodoPago)
                        .Select(g => new Caja
                        {
                            Fecha = fechaSeleccionada,
                            MetodoPago = g.Key,
                            Total = g.Sum(i => i.Total),
                            CantidadVentas = g.Count()
                        }).ToList();

                    var informeTotalDia = informeCajaCerrada.Sum(i => i.Total);

                    informeCajaCerrada.Add(new Caja
                    {
                        Fecha = fechaSeleccionada,
                        MetodoPago = "Total",
                        Total = informeTotalDia,
                        CantidadVentas = informesDelDia.Count
                    });

                    _informeVentaRepository.InsertarCajaCerradaPorLista(informeCajaCerrada);

                    CargarDtCajas();
                    Cursor = Cursors.Default;

                    MessageBox.Show("Caja cerrada correctamente.", "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    tabInformes.SelectedTab = tabPage2;
                }
            }
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
                    CantidadVentas = g.Sum(c => c.CantidadVentas)
                })
                .OrderBy(c => c.MetodoPago)
                .ToList();

            decimal totalMes = resumen.Sum(c => c.Total);
            int cantidadVentasMes = resumen.Sum(c => c.CantidadVentas);
            resumen.Add(new Caja
            {
                MetodoPago = "Total",
                Total = totalMes,
                CantidadVentas = cantidadVentasMes
            });

            dtResumen.DataSource = null;
            dtResumen.DataSource = resumen;

            dtResumen.Columns["IdCaja"].Visible = false;
            dtResumen.Columns["Fecha"].Visible = false;
            dtResumen.Columns["MetodoPago"].HeaderText = "Método de Pago";
            dtResumen.Columns["CantidadVentas"].HeaderText = "Cantidad de ventas";
            dtResumen.Columns["Total"].DefaultCellStyle.FormatProvider = new CultureInfo("es-AR");
            dtResumen.Columns["Total"].DefaultCellStyle.Format = "C2";

            lblTotalMes.Text = $"Total del mes: {totalMes.ToString("C2", new CultureInfo("es-AR"))}";

            if (resumen.Count == 1)
            {
                MessageBox.Show("No hay cajas cerradas para el mes seleccionado.", "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

    }
}
