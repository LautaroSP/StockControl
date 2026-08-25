using StockControl.Domain;
using StockControl.Repository;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;

namespace StockControl
{
    public partial class frmImprimirCarteles : Form
    {
        private readonly ProductoRepository _prodRepository = new();
        private readonly List<CartelItem> _todos = new();
        private BindingList<CartelItem> _vista = new();
        private bool _togglingRow;

        public frmImprimirCarteles()
        {
            InitializeComponent();
            CargarProductos();
            ConfigurarGrid();
            ActualizarResumen();

            txtBuscar.TextChanged += (_, _) => Filtrar();
            rdbGrande.CheckedChanged += (_, _) => ActualizarResumen();
            rdbChico.CheckedChanged += (_, _) => ActualizarResumen();
            btnImprimir.Click += btnImprimir_Click;
            btnCancelar.Click += (_, _) => Close();
            dgProductos.CellContentClick += dgProductos_CellContentClick;
            dgProductos.CellClick += dgProductos_CellClick;
            dgProductos.CurrentCellDirtyStateChanged += dgProductos_CurrentCellDirtyStateChanged;
            dgProductos.CellValueChanged += dgProductos_CellValueChanged;
        }

        private void CargarProductos()
        {
            var productos = _prodRepository.Listar().OrderBy(p => p.Nombre).ToList();
            _todos.Clear();
            foreach (var p in productos)
            {
                _todos.Add(new CartelItem
                {
                    Seleccionado = false,
                    Codigo = p.Codigo,
                    Nombre = p.Nombre,
                    Precio = p.Precio
                });
            }
            _vista = new BindingList<CartelItem>(_todos.ToList());
            dgProductos.DataSource = _vista;
        }

        private void ConfigurarGrid()
        {
            dgProductos.AutoGenerateColumns = false;
            dgProductos.Columns.Clear();
            dgProductos.MultiSelect = false;

            dgProductos.Columns.Add(new DataGridViewCheckBoxColumn
            {
                DataPropertyName = nameof(CartelItem.Seleccionado),
                HeaderText = "",
                Name = "Seleccionado",
                Width = 40
            });
            dgProductos.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = nameof(CartelItem.Codigo),
                HeaderText = "Código",
                Name = "Codigo",
                ReadOnly = true,
                Width = 110
            });
            dgProductos.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = nameof(CartelItem.Nombre),
                HeaderText = "Nombre",
                Name = "Nombre",
                AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill
            });
            var colPrecio = new DataGridViewTextBoxColumn
            {
                DataPropertyName = nameof(CartelItem.Precio),
                HeaderText = "Precio",
                Name = "Precio",
                Width = 100
            };
            colPrecio.DefaultCellStyle.Format = "C2";
            colPrecio.DefaultCellStyle.FormatProvider = new CultureInfo("es-AR");
            dgProductos.Columns.Add(colPrecio);
        }

        private void Filtrar()
        {
            string filtro = txtBuscar.Text.Trim().ToLowerInvariant();
            List<CartelItem> filtrados;
            if (string.IsNullOrEmpty(filtro))
            {
                filtrados = _todos.ToList();
            }
            else
            {
                filtrados = _todos
                    .Where(p =>
                        (p.Codigo ?? string.Empty).ToLowerInvariant().Contains(filtro) ||
                        (p.Nombre ?? string.Empty).ToLowerInvariant().Contains(filtro))
                    .ToList();
            }

            _vista = new BindingList<CartelItem>(filtrados);
            dgProductos.DataSource = _vista;
            ActualizarResumen();
        }

        private CartelTamano TamanoSeleccionado =>
            rdbChico.Checked ? CartelTamano.Chico : CartelTamano.Grande;

        private void ActualizarResumen()
        {
            int seleccionados = _todos.Count(x => x.Seleccionado);
            int hojas = CartelPdfGenerator.CalcularHojas(seleccionados, TamanoSeleccionado);
            lblResumen.Text = $"{seleccionados} seleccionados · {hojas} hoja(s)";
        }

        private void dgProductos_CurrentCellDirtyStateChanged(object? sender, EventArgs e)
        {
            if (dgProductos.IsCurrentCellDirty &&
                dgProductos.CurrentCell is DataGridViewCheckBoxCell)
            {
                dgProductos.CommitEdit(DataGridViewDataErrorContexts.Commit);
            }
        }

        private void dgProductos_CellValueChanged(object? sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0) return;
            if (dgProductos.Columns[e.ColumnIndex].Name == "Seleccionado")
                ActualizarResumen();
        }

        private void dgProductos_CellContentClick(object? sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0) return;
            if (dgProductos.Columns[e.ColumnIndex].Name == "Seleccionado")
            {
                dgProductos.CommitEdit(DataGridViewDataErrorContexts.Commit);
                ActualizarResumen();
            }
        }

        private void dgProductos_CellClick(object? sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0 || _togglingRow) return;
            string colName = dgProductos.Columns[e.ColumnIndex].Name;
            // Click en fila (no en Nombre/Precio editables) toggle selección
            if (colName is "Nombre" or "Precio") return;
            if (colName == "Seleccionado") return;

            if (dgProductos.Rows[e.RowIndex].DataBoundItem is not CartelItem item) return;

            _togglingRow = true;
            try
            {
                item.Seleccionado = !item.Seleccionado;
                dgProductos.InvalidateRow(e.RowIndex);
                ActualizarResumen();
            }
            finally
            {
                _togglingRow = false;
            }
        }

        private void btnImprimir_Click(object? sender, EventArgs e)
        {
            // Commit edición en curso
            dgProductos.EndEdit();

            var seleccionados = _todos.Where(x => x.Seleccionado).ToList();
            if (seleccionados.Count == 0)
            {
                MessageBox.Show("Debe seleccionar al menos un producto.", "Imprimir carteles",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            var tamano = TamanoSeleccionado;
            int hojas = CartelPdfGenerator.CalcularHojas(seleccionados.Count, tamano);
            var confirm = MessageBox.Show(
                $"Se van a generar {hojas} hoja(s) A4 con {seleccionados.Count} cartel(es).\n¿Continuar?",
                "Confirmar impresión",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question);

            if (confirm != DialogResult.Yes) return;

            try
            {
                Cursor = Cursors.WaitCursor;
                string path = CartelPdfGenerator.Generar(seleccionados, tamano);
                try
                {
                    Process.Start(new ProcessStartInfo
                    {
                        FileName = path,
                        UseShellExecute = true
                    });
                }
                catch
                {
                    MessageBox.Show(
                        $"El PDF se generó pero no se pudo abrir automáticamente.\nRuta:\n{path}",
                        "PDF generado",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al generar el PDF:\n{ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                Cursor = Cursors.Default;
            }
        }
    }
}
