namespace StockControl
{
    partial class FrmCerrarCajaAnterior
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
            label1 = new Label();
            dtpFecha = new DateTimePicker();
            button1 = new Button();
            btnAceptar = new Button();
            SuspendLayout();
            // 
            // label1
            // 
            label1.AutoEllipsis = true;
            label1.AutoSize = true;
            label1.Location = new Point(12, 9);
            label1.Name = "label1";
            label1.Size = new Size(99, 15);
            label1.TabIndex = 0;
            label1.Text = "Seleccionar fecha";
            // 
            // dtpFecha
            // 
            dtpFecha.Location = new Point(76, 27);
            dtpFecha.Name = "dtpFecha";
            dtpFecha.Size = new Size(228, 23);
            dtpFecha.TabIndex = 1;
            // 
            // button1
            // 
            button1.Location = new Point(286, 56);
            button1.Name = "button1";
            button1.Size = new Size(75, 23);
            button1.TabIndex = 2;
            button1.Text = "Cancelar";
            button1.UseVisualStyleBackColor = true;
            button1.Click += btnCancelar_Click;
            // 
            // btnAceptar
            // 
            btnAceptar.Location = new Point(12, 56);
            btnAceptar.Name = "btnAceptar";
            btnAceptar.Size = new Size(75, 23);
            btnAceptar.TabIndex = 3;
            btnAceptar.Text = "Aceptar";
            btnAceptar.UseVisualStyleBackColor = true;
            btnAceptar.Click += btnAceptar_Click;
            // 
            // FrmCerrarCajaAnterior
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(373, 90);
            Controls.Add(btnAceptar);
            Controls.Add(button1);
            Controls.Add(dtpFecha);
            Controls.Add(label1);
            MaximizeBox = false;
            Name = "FrmCerrarCajaAnterior";
            Text = "Cerrar caja anterior";
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Label label1;
        private DateTimePicker dtpFecha;
        private Button button1;
        private Button btnAceptar;
    }
}