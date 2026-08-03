namespace StockControl
{
    partial class frmImprimirCarteles
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        private void InitializeComponent()
        {
            lblBuscar = new Label();
            txtBuscar = new TextBox();
            dgProductos = new DataGridView();
            grpTamano = new GroupBox();
            rdbChico = new RadioButton();
            rdbGrande = new RadioButton();
            lblResumen = new Label();
            btnImprimir = new Button();
            btnCancelar = new Button();
            ((System.ComponentModel.ISupportInitialize)dgProductos).BeginInit();
            grpTamano.SuspendLayout();
            SuspendLayout();
            //
            // lblBuscar
            //
            lblBuscar.AutoSize = true;
            lblBuscar.Location = new Point(12, 15);
            lblBuscar.Name = "lblBuscar";
            lblBuscar.Size = new Size(103, 15);
            lblBuscar.TabIndex = 0;
            lblBuscar.Text = "Buscar (código/nombre):";
            //
            // txtBuscar
            //
            txtBuscar.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            txtBuscar.Location = new Point(160, 12);
            txtBuscar.Name = "txtBuscar";
            txtBuscar.Size = new Size(512, 23);
            txtBuscar.TabIndex = 1;
            //
            // dgProductos
            //
            dgProductos.AllowUserToAddRows = false;
            dgProductos.AllowUserToDeleteRows = false;
            dgProductos.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            dgProductos.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dgProductos.Location = new Point(12, 45);
            dgProductos.Name = "dgProductos";
            dgProductos.RowHeadersVisible = false;
            dgProductos.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgProductos.Size = new Size(660, 420);
            dgProductos.TabIndex = 2;
            //
            // grpTamano
            //
            grpTamano.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            grpTamano.Controls.Add(rdbChico);
            grpTamano.Controls.Add(rdbGrande);
            grpTamano.Location = new Point(12, 475);
            grpTamano.Name = "grpTamano";
            grpTamano.Size = new Size(220, 70);
            grpTamano.TabIndex = 3;
            grpTamano.TabStop = false;
            grpTamano.Text = "Tamaño del cartel";
            //
            // rdbChico
            //
            rdbChico.AutoSize = true;
            rdbChico.Location = new Point(110, 30);
            rdbChico.Name = "rdbChico";
            rdbChico.Size = new Size(90, 19);
            rdbChico.TabIndex = 1;
            rdbChico.Text = "Cartel Chico";
            rdbChico.UseVisualStyleBackColor = true;
            //
            // rdbGrande
            //
            rdbGrande.AutoSize = true;
            rdbGrande.Checked = true;
            rdbGrande.Location = new Point(15, 30);
            rdbGrande.Name = "rdbGrande";
            rdbGrande.Size = new Size(99, 19);
            rdbGrande.TabIndex = 0;
            rdbGrande.TabStop = true;
            rdbGrande.Text = "Cartel Grande";
            rdbGrande.UseVisualStyleBackColor = true;
            //
            // lblResumen
            //
            lblResumen.Anchor = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            lblResumen.Location = new Point(250, 490);
            lblResumen.Name = "lblResumen";
            lblResumen.Size = new Size(250, 40);
            lblResumen.TabIndex = 4;
            lblResumen.Text = "0 seleccionados · 0 hojas";
            lblResumen.TextAlign = ContentAlignment.MiddleLeft;
            //
            // btnImprimir
            //
            btnImprimir.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            btnImprimir.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            btnImprimir.Location = new Point(516, 492);
            btnImprimir.Name = "btnImprimir";
            btnImprimir.Size = new Size(75, 35);
            btnImprimir.TabIndex = 5;
            btnImprimir.Text = "Imprimir";
            btnImprimir.UseVisualStyleBackColor = true;
            //
            // btnCancelar
            //
            btnCancelar.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            btnCancelar.Location = new Point(597, 492);
            btnCancelar.Name = "btnCancelar";
            btnCancelar.Size = new Size(75, 35);
            btnCancelar.TabIndex = 6;
            btnCancelar.Text = "Cancelar";
            btnCancelar.UseVisualStyleBackColor = true;
            //
            // frmImprimirCarteles
            //
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(684, 561);
            Controls.Add(btnCancelar);
            Controls.Add(btnImprimir);
            Controls.Add(lblResumen);
            Controls.Add(grpTamano);
            Controls.Add(dgProductos);
            Controls.Add(txtBuscar);
            Controls.Add(lblBuscar);
            MinimizeBox = false;
            MinimumSize = new Size(700, 600);
            Name = "frmImprimirCarteles";
            StartPosition = FormStartPosition.CenterParent;
            Text = "Imprimir carteles";
            ((System.ComponentModel.ISupportInitialize)dgProductos).EndInit();
            grpTamano.ResumeLayout(false);
            grpTamano.PerformLayout();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Label lblBuscar;
        private TextBox txtBuscar;
        private DataGridView dgProductos;
        private GroupBox grpTamano;
        private RadioButton rdbChico;
        private RadioButton rdbGrande;
        private Label lblResumen;
        private Button btnImprimir;
        private Button btnCancelar;
    }
}
