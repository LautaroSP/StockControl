namespace StockControl
{
    partial class frmCrearGrupo
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
            btnCancelar = new Button();
            btnAceptar = new Button();
            label3 = new Label();
            txtIVA = new TextBox();
            label6 = new Label();
            txtGanancia = new TextBox();
            Ganancia = new Label();
            chkGananciaProd = new CheckBox();
            txtCosto = new TextBox();
            Costo = new Label();
            label1 = new Label();
            label5 = new Label();
            txtPrecio = new TextBox();
            txtNombre = new TextBox();
            SuspendLayout();
            // 
            // btnCancelar
            // 
            btnCancelar.Location = new Point(93, 130);
            btnCancelar.Name = "btnCancelar";
            btnCancelar.Size = new Size(75, 23);
            btnCancelar.TabIndex = 0;
            btnCancelar.Text = "Cancelar";
            btnCancelar.UseVisualStyleBackColor = true;
            btnCancelar.Click += btnCancelar_Click;
            // 
            // btnAceptar
            // 
            btnAceptar.Font = new Font("Segoe UI Semibold", 9F, FontStyle.Bold, GraphicsUnit.Point, 0);
            btnAceptar.ForeColor = Color.OliveDrab;
            btnAceptar.Location = new Point(12, 130);
            btnAceptar.Name = "btnAceptar";
            btnAceptar.Size = new Size(75, 23);
            btnAceptar.TabIndex = 1;
            btnAceptar.Text = "Aceptar";
            btnAceptar.UseVisualStyleBackColor = true;
            btnAceptar.Click += btnAceptar_Click;
            // 
            // label3
            // 
            label3.AutoSize = true;
            label3.Font = new Font("Segoe UI", 12F, FontStyle.Bold | FontStyle.Underline, GraphicsUnit.Point, 0);
            label3.ForeColor = SystemColors.Highlight;
            label3.Location = new Point(12, 9);
            label3.Name = "label3";
            label3.Size = new Size(112, 21);
            label3.TabIndex = 4;
            label3.Text = "Nuevo Grupo";
            // 
            // txtIVA
            // 
            txtIVA.Location = new Point(238, 130);
            txtIVA.Name = "txtIVA";
            txtIVA.PlaceholderText = "00,00";
            txtIVA.Size = new Size(38, 23);
            txtIVA.TabIndex = 37;
            txtIVA.TextChanged += txtIVA_TextChanged;
            // 
            // label6
            // 
            label6.AutoSize = true;
            label6.Location = new Point(206, 136);
            label6.Name = "label6";
            label6.Size = new Size(24, 15);
            label6.TabIndex = 36;
            label6.Text = "IVA";
            // 
            // txtGanancia
            // 
            txtGanancia.Location = new Point(207, 104);
            txtGanancia.Name = "txtGanancia";
            txtGanancia.PlaceholderText = "00,00";
            txtGanancia.Size = new Size(100, 23);
            txtGanancia.TabIndex = 35;
            txtGanancia.TextChanged += txtGanancia_TextChanged;
            // 
            // Ganancia
            // 
            Ganancia.AutoSize = true;
            Ganancia.Location = new Point(143, 110);
            Ganancia.Name = "Ganancia";
            Ganancia.Size = new Size(56, 15);
            Ganancia.TabIndex = 34;
            Ganancia.Text = "Ganancia";
            // 
            // chkGananciaProd
            // 
            chkGananciaProd.AutoSize = true;
            chkGananciaProd.Location = new Point(159, 73);
            chkGananciaProd.Name = "chkGananciaProd";
            chkGananciaProd.Size = new Size(148, 19);
            chkGananciaProd.TabIndex = 33;
            chkGananciaProd.Text = "Ganancia por producto";
            chkGananciaProd.UseVisualStyleBackColor = true;
            chkGananciaProd.CheckedChanged += chkGananciaProd_CheckedChanged;
            // 
            // txtCosto
            // 
            txtCosto.Location = new Point(70, 71);
            txtCosto.Name = "txtCosto";
            txtCosto.PlaceholderText = "0.00";
            txtCosto.Size = new Size(58, 23);
            txtCosto.TabIndex = 32;
            txtCosto.TextChanged += txtCosto_TextChanged;
            // 
            // Costo
            // 
            Costo.AutoSize = true;
            Costo.Location = new Point(12, 75);
            Costo.Name = "Costo";
            Costo.Size = new Size(38, 15);
            Costo.TabIndex = 31;
            Costo.Text = "Costo";
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new Point(12, 104);
            label1.Name = "label1";
            label1.Size = new Size(43, 15);
            label1.TabIndex = 29;
            label1.Text = "Precio ";
            // 
            // label5
            // 
            label5.AutoSize = true;
            label5.Location = new Point(12, 50);
            label5.Name = "label5";
            label5.Size = new Size(51, 15);
            label5.TabIndex = 28;
            label5.Text = "Nombre";
            // 
            // txtPrecio
            // 
            txtPrecio.Location = new Point(70, 101);
            txtPrecio.Name = "txtPrecio";
            txtPrecio.PlaceholderText = "00.00";
            txtPrecio.Size = new Size(58, 23);
            txtPrecio.TabIndex = 27;
            txtPrecio.TextChanged += txtPrecio_TextChanged;
            // 
            // txtNombre
            // 
            txtNombre.Location = new Point(70, 41);
            txtNombre.Name = "txtNombre";
            txtNombre.Size = new Size(171, 23);
            txtNombre.TabIndex = 26;
            // 
            // frmCrearGrupo
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(387, 165);
            Controls.Add(txtIVA);
            Controls.Add(label6);
            Controls.Add(txtGanancia);
            Controls.Add(Ganancia);
            Controls.Add(chkGananciaProd);
            Controls.Add(txtCosto);
            Controls.Add(Costo);
            Controls.Add(label1);
            Controls.Add(label5);
            Controls.Add(txtPrecio);
            Controls.Add(txtNombre);
            Controls.Add(label3);
            Controls.Add(btnAceptar);
            Controls.Add(btnCancelar);
            MaximizeBox = false;
            Name = "frmCrearGrupo";
            Text = "Crear Grupo";
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Button btnCancelar;
        private Button btnAceptar;
        private Label label3;
        private TextBox txtIVA;
        private Label label6;
        private TextBox txtGanancia;
        private Label Ganancia;
        private CheckBox chkGananciaProd;
        private TextBox txtCosto;
        private Label Costo;
        private Label label1;
        private Label label5;
        private TextBox txtPrecio;
        private TextBox txtNombre;
    }
}