namespace StockControl.Domain
{
    partial class frmMetodoPago
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            dgMetodoDePago = new DataGridView();
            label1 = new Label();
            btnAceptar = new Button();
            btnCancelar = new Button();
            label2 = new Label();
            lblRestante = new Label();
            ((System.ComponentModel.ISupportInitialize)dgMetodoDePago).BeginInit();
            SuspendLayout();
            // 
            // dgMetodoDePago
            // 
            dgMetodoDePago.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dgMetodoDePago.Location = new Point(12, 29);
            dgMetodoDePago.Name = "dgMetodoDePago";
            dgMetodoDePago.Size = new Size(395, 301);
            dgMetodoDePago.TabIndex = 0;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Font = new Font("Segoe UI Semibold", 9F, FontStyle.Bold | FontStyle.Italic, GraphicsUnit.Point, 0);
            label1.Location = new Point(12, 11);
            label1.Name = "label1";
            label1.Size = new Size(209, 15);
            label1.TabIndex = 1;
            label1.Text = "Seleccionar Multiples Metodo de pago";
            // 
            // btnAceptar
            // 
            btnAceptar.Location = new Point(12, 336);
            btnAceptar.Name = "btnAceptar";
            btnAceptar.Size = new Size(75, 23);
            btnAceptar.TabIndex = 2;
            btnAceptar.Text = "Aceptar";
            btnAceptar.UseVisualStyleBackColor = true;
            btnAceptar.Click += btnAceptar_Click;
            // 
            // btnCancelar
            // 
            btnCancelar.Location = new Point(93, 336);
            btnCancelar.Name = "btnCancelar";
            btnCancelar.Size = new Size(75, 23);
            btnCancelar.TabIndex = 3;
            btnCancelar.Text = "Cancelar";
            btnCancelar.UseVisualStyleBackColor = true;
            btnCancelar.Click += btnCancelar_Click;
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Location = new Point(273, 11);
            label2.Name = "label2";
            label2.Size = new Size(39, 15);
            label2.TabIndex = 4;
            label2.Text = "Resto:";
            // 
            // lblRestante
            // 
            lblRestante.AutoSize = true;
            lblRestante.Location = new Point(318, 11);
            lblRestante.Name = "lblRestante";
            lblRestante.Size = new Size(38, 15);
            lblRestante.TabIndex = 5;
            lblRestante.Text = "label3";
            // 
            // frmMetodoPago
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(417, 375);
            Controls.Add(lblRestante);
            Controls.Add(label2);
            Controls.Add(btnCancelar);
            Controls.Add(btnAceptar);
            Controls.Add(label1);
            Controls.Add(dgMetodoDePago);
            Name = "frmMetodoPago";
            Text = "frmMetodoPago";
            ((System.ComponentModel.ISupportInitialize)dgMetodoDePago).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private DataGridView dgMetodoDePago;
        private Label label1;
        private Button btnAceptar;
        private Button btnCancelar;
        private Label label2;
        private Label lblRestante;
    }
}