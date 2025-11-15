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
    public partial class ActivationForm : Form
    {
        private TextBox txtMachineId;
        private TextBox txtSerial;
        private Button btnCopy;
        private Button btnActivate;

        public ActivationForm()
        {
            Text = "Activación";
            Width = 520;
            Height = 250;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            StartPosition = FormStartPosition.CenterScreen;

            var lblInfo = new Label
            {
                Text = "Envíe este código de máquina al proveedor para recibir su clave:",
                AutoSize = true,
                Left = 20,
                Top = 20
            };
            Controls.Add(lblInfo);


            txtMachineId = new TextBox
            {
                Left = 20,
                Top = 45,
                Width = 460,
                ReadOnly = true
            };
            Controls.Add(txtMachineId);

            btnCopy = new Button
            {
                Text = "Copiar",
                Left = 400,
                Top = 75,
                Width = 80
            };
            btnCopy.Click += (s, e) =>
            {
                Clipboard.SetText(txtMachineId.Text);
                MessageBox.Show("Código copiado.");
            };
            Controls.Add(btnCopy);


            var lblSerial = new Label
            {
                Text = "Ingrese la clave de activación recibida:",
                AutoSize = true,
                Left = 20,
                Top = 110
            };
            Controls.Add(lblSerial);

            txtSerial = new TextBox
            {
                Left = 20,
                Top = 135,
                Width = 460
            };
            Controls.Add(txtSerial);


            btnActivate = new Button
            {
                Text = "Activar",
                Left = 400,
                Top = 165,
                Width = 80
            };
            btnActivate.Click += BtnActivate_Click;
            Controls.Add(btnActivate);

            txtMachineId.Text = LicenciaHelper.ObtenerMachineIdMostrar();
        }


        private void BtnActivate_Click(object? sender, EventArgs e)
        {
            var serial = (txtSerial.Text ?? string.Empty).Trim();
            if (LicenciaHelper.ValidarYGuardarLicencia(serial))
            {
                MessageBox.Show("Activación correcta.");
                DialogResult = DialogResult.OK;
                Close();
            }
            else
            {
                MessageBox.Show("Clave inválida para esta máquina.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}

