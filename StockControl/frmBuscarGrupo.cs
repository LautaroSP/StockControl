using StockControl.Domain;
using StockControl.Repository;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace StockControl
{
    public partial class frmBuscarGrupo : Form
    {
        private GrupoRepository _grupoRepository = new GrupoRepository();
        private List<GrupoProductos> _grupos = new List<GrupoProductos>();

        public GrupoProductos GrupoSeleccionado { get; private set; }

        public frmBuscarGrupo()
        {
            InitializeComponent();
            CargarGrupos();
            ConfigurarGrid();
            txtBuscar.TextChanged += txtBuscar_TextChanged;
            dataGridView1.CellDoubleClick += dataGridView1_CellDoubleClick;

        }

        private void CargarGrupos()
        {
            _grupos = _grupoRepository.Listar(); // ajustá si tu método se llama distinto
            dataGridView1.DataSource = _grupos;
        }

        private void ConfigurarGrid()
        {
            dataGridView1.AutoGenerateColumns = false;
            dataGridView1.Columns.Clear();

            dataGridView1.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = "NombreGrupo",
                HeaderText = "Grupo",
                AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill
            });

            dataGridView1.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = "PrecioGrupo", // si existe, sino sacalo
                HeaderText = "PrecioGrupo",
                Width = 100
            });

            dataGridView1.ReadOnly = true;
            dataGridView1.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dataGridView1.MultiSelect = false;
        }

        private void txtBuscar_TextChanged(object sender, EventArgs e)
        {
            string filtro = txtBuscar.Text.ToLower();

            var filtrados = _grupos
                .Where(g => g.NombreGrupo.ToLower().Contains(filtro))
                .ToList();

            dataGridView1.DataSource = filtrados;
        }

        private void dataGridView1_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0) return;

            GrupoSeleccionado = (GrupoProductos)dataGridView1.Rows[e.RowIndex].DataBoundItem;
            DialogResult = DialogResult.OK;
            Close();
        }

        private void btnSalir_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }

        private void btnNuevoGrupo_Click(object sender, EventArgs e)
        {
            using (var formNuevoGrupo = new frmCrearGrupo())
            {
                if (formNuevoGrupo.ShowDialog() == DialogResult.OK)
                {
                    CargarGrupos();
                }
            }
        }
    }
}
