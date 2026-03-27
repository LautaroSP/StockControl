using StockControl.Domain;
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
    public partial class frmInformeVentaDetalle : Form
    {
        public List<InformeVentaDetalle> _informeVentaDetalles = new();
        public frmInformeVentaDetalle(List<InformeVentaDetalle> informeVentaDetalles)
        {
            InitializeComponent();
            this.Icon = new Icon("Resources\\stockIcon.ico");
            _informeVentaDetalles = informeVentaDetalles;
            dataGridView1.DataSource = _informeVentaDetalles;
            dataGridView1.Columns["IdInformeVentaDetalle"].Visible = false;
            dataGridView1.Columns["IdInformeVenta"].Visible = false;
            dataGridView1.Columns["Precio"].DefaultCellStyle.FormatProvider = new CultureInfo("es-AR");
            dataGridView1.Columns["Precio"].DefaultCellStyle.Format = "C2";
            dataGridView1.Columns["Subtotal"].DefaultCellStyle.FormatProvider = new CultureInfo("es-AR");
            dataGridView1.Columns["Subtotal"].DefaultCellStyle.Format = "C2";
            dataGridView1.ReadOnly = true;
            dataGridView1.AutoSize = true;
        }
    }
}
