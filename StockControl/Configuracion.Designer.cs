
namespace StockControl
{
    partial class Configuracion
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
            txtNombreLocal = new TextBox();
            btnGuardar = new Button();
            btnSalir = new Button();
            lblObliLocal = new Label();
            label2 = new Label();
            txtFactorGanancia = new TextBox();
            chkValorMoneda = new CheckBox();
            label3 = new Label();
            txtIVA = new TextBox();
            SuspendLayout();
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Font = new Font("Segoe UI Semibold", 11.25F, FontStyle.Bold | FontStyle.Italic, GraphicsUnit.Point, 0);
            label1.Location = new Point(12, 9);
            label1.Name = "label1";
            label1.Size = new Size(129, 20);
            label1.TabIndex = 0;
            label1.Text = "Nombre del Local";
            // 
            // txtNombreLocal
            // 
            txtNombreLocal.Location = new Point(12, 32);
            txtNombreLocal.Name = "txtNombreLocal";
            txtNombreLocal.Size = new Size(290, 23);
            txtNombreLocal.TabIndex = 1;
            txtNombreLocal.TextChanged += txtNombreLocal_Changed;
            // 
            // btnGuardar
            // 
            btnGuardar.Location = new Point(12, 237);
            btnGuardar.Name = "btnGuardar";
            btnGuardar.Size = new Size(75, 23);
            btnGuardar.TabIndex = 2;
            btnGuardar.Text = "Guardar";
            btnGuardar.UseVisualStyleBackColor = true;
            btnGuardar.Click += btnGuardar_Click;
            // 
            // btnSalir
            // 
            btnSalir.Location = new Point(93, 237);
            btnSalir.Name = "btnSalir";
            btnSalir.Size = new Size(75, 23);
            btnSalir.TabIndex = 3;
            btnSalir.Text = "Salir";
            btnSalir.UseVisualStyleBackColor = true;
            btnSalir.Click += btnSalir_Click;
            // 
            // lblObliLocal
            // 
            lblObliLocal.AutoSize = true;
            lblObliLocal.Font = new Font("Segoe UI", 9F, FontStyle.Italic, GraphicsUnit.Point, 0);
            lblObliLocal.ForeColor = Color.Maroon;
            lblObliLocal.Location = new Point(13, 58);
            lblObliLocal.Name = "lblObliLocal";
            lblObliLocal.Size = new Size(139, 15);
            lblObliLocal.TabIndex = 4;
            lblObliLocal.Text = "Este campo es obligatorio";
            lblObliLocal.Visible = false;
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Font = new Font("Segoe UI Semibold", 11.25F, FontStyle.Bold | FontStyle.Italic);
            label2.Location = new Point(13, 73);
            label2.Name = "label2";
            label2.Size = new Size(142, 20);
            label2.TabIndex = 5;
            label2.Text = "Factor de ganancia";
            // 
            // txtFactorGanancia
            // 
            txtFactorGanancia.Location = new Point(13, 96);
            txtFactorGanancia.Name = "txtFactorGanancia";
            txtFactorGanancia.Size = new Size(289, 23);
            txtFactorGanancia.TabIndex = 6;
            txtFactorGanancia.TextAlignChanged += txtFactorGanancia_TextAlignChanged;
            txtFactorGanancia.TextChanged += txtFactorGanancia_TextChanged;
            // 
            // chkValorMoneda
            // 
            chkValorMoneda.AutoSize = true;
            chkValorMoneda.Font = new Font("Segoe UI Semibold", 9F, FontStyle.Bold, GraphicsUnit.Point, 0);
            chkValorMoneda.Location = new Point(187, 12);
            chkValorMoneda.Name = "chkValorMoneda";
            chkValorMoneda.Size = new Size(113, 19);
            chkValorMoneda.TabIndex = 7;
            chkValorMoneda.Text = "Valores en Pesos";
            chkValorMoneda.UseVisualStyleBackColor = true;
            chkValorMoneda.CheckedChanged += chkValorMoneda_CheckedChanged;
            // 
            // label3
            // 
            label3.AutoSize = true;
            label3.Font = new Font("Segoe UI Semibold", 11.25F, FontStyle.Bold | FontStyle.Italic);
            label3.Location = new Point(13, 122);
            label3.Name = "label3";
            label3.Size = new Size(80, 20);
            label3.TabIndex = 8;
            label3.Text = "Factor IVA";
            // 
            // txtIVA
            // 
            txtIVA.Location = new Point(13, 145);
            txtIVA.Name = "txtIVA";
            txtIVA.Size = new Size(289, 23);
            txtIVA.TabIndex = 9;
            txtIVA.TextChanged += txtIVA_TextChanged;
            // 
            // Configuracion
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(314, 272);
            Controls.Add(txtIVA);
            Controls.Add(label3);
            Controls.Add(chkValorMoneda);
            Controls.Add(txtFactorGanancia);
            Controls.Add(label2);
            Controls.Add(lblObliLocal);
            Controls.Add(btnSalir);
            Controls.Add(btnGuardar);
            Controls.Add(txtNombreLocal);
            Controls.Add(label1);
            Name = "Configuracion";
            Text = "Configuracion";
            ResumeLayout(false);
            PerformLayout();
        }



        #endregion

        private Label label1;
        private TextBox txtNombreLocal;
        private Button btnGuardar;
        private Button btnSalir;
        private Label lblObliLocal;
        private Label label2;
        private TextBox txtFactorGanancia;
        private CheckBox chkValorMoneda;
        private Label label3;
        private TextBox txtIVA;
    }
}