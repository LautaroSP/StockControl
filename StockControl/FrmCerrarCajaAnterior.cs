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
    public partial class FrmCerrarCajaAnterior : Form
    {
        public DateTime FechaSeleccionada { get; private set; }
        public FrmCerrarCajaAnterior()
        {
            InitializeComponent();
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.StartPosition = FormStartPosition.CenterParent;
            dtpFecha.MaxDate = DateTime.Today;
            dtpFecha.MinDate = DateTime.Today.AddDays(-7);
            dtpFecha.Value = DateTime.Today.AddDays(-1); // valor por defecto: ayer
        }
        private void btnAceptar_Click(object sender, EventArgs e)
        {
            FechaSeleccionada = dtpFecha.Value.Date;
            DialogResult = DialogResult.OK;
            Close();
        }

        private void btnCancelar_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }
    }
}
