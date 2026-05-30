namespace StockControl
{
    partial class frmProducto
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
                components.Dispose();
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        private void InitializeComponent()
        {
            brnGrabar = new Button();
            brnCancelar = new Button();
            label1 = new Label();
            label2 = new Label();
            label3 = new Label();
            label4 = new Label();
            label5 = new Label();
            txtCodigo = new TextBox();
            txtNombre = new TextBox();
            txtCosto = new TextBox();
            txtPrecio = new TextBox();
            txtCantidad = new TextBox();
            chkGananciaProd = new CheckBox();
            lblGanancia = new Label();
            txtGanancia = new TextBox();
            chkSector = new CheckBox();
            lblIVA = new Label();
            txtIVA = new TextBox();
            lblGrupo = new Label();
            lblFechaModificacion = new Label();
            lblGrupoSel = new Label();
            btnBuscarGrupo = new Button();
            btnSacarGrupo = new Button();
            SuspendLayout();
            // 
            // brnGrabar
            // 
            brnGrabar.Location = new Point(12, 234);
            brnGrabar.Name = "brnGrabar";
            brnGrabar.Size = new Size(93, 23);
            brnGrabar.TabIndex = 0;
            brnGrabar.Text = "Grabar";
            brnGrabar.UseVisualStyleBackColor = true;
            brnGrabar.Click += brnGrabar_Click;
            // 
            // brnCancelar
            // 
            brnCancelar.Location = new Point(111, 234);
            brnCancelar.Name = "brnCancelar";
            brnCancelar.Size = new Size(83, 23);
            brnCancelar.TabIndex = 1;
            brnCancelar.Text = "Cancelar";
            brnCancelar.UseVisualStyleBackColor = true;
            brnCancelar.Click += brnCancelar_Click;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new Point(25, 27);
            label1.Name = "label1";
            label1.Size = new Size(46, 15);
            label1.TabIndex = 2;
            label1.Text = "Codigo";
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Location = new Point(25, 57);
            label2.Name = "label2";
            label2.Size = new Size(51, 15);
            label2.TabIndex = 3;
            label2.Text = "Nombre";
            // 
            // label3
            // 
            label3.AutoSize = true;
            label3.Location = new Point(25, 86);
            label3.Name = "label3";
            label3.Size = new Size(38, 15);
            label3.TabIndex = 4;
            label3.Text = "Costo";
            // 
            // label4
            // 
            label4.AutoSize = true;
            label4.Location = new Point(25, 115);
            label4.Name = "label4";
            label4.Size = new Size(40, 15);
            label4.TabIndex = 5;
            label4.Text = "Precio";
            // 
            // label5
            // 
            label5.AutoSize = true;
            label5.Location = new Point(25, 144);
            label5.Name = "label5";
            label5.Size = new Size(55, 15);
            label5.TabIndex = 6;
            label5.Text = "Cantidad";
            // 
            // txtCodigo
            // 
            txtCodigo.Location = new Point(93, 24);
            txtCodigo.Name = "txtCodigo";
            txtCodigo.Size = new Size(346, 23);
            txtCodigo.TabIndex = 7;
            // 
            // txtNombre
            // 
            txtNombre.Location = new Point(93, 54);
            txtNombre.Name = "txtNombre";
            txtNombre.Size = new Size(346, 23);
            txtNombre.TabIndex = 8;
            // 
            // txtCosto
            // 
            txtCosto.Location = new Point(93, 83);
            txtCosto.Name = "txtCosto";
            txtCosto.Size = new Size(100, 23);
            txtCosto.TabIndex = 9;
            txtCosto.TextChanged += txtCosto_TextChanged;
            // 
            // txtPrecio
            // 
            txtPrecio.Location = new Point(93, 112);
            txtPrecio.Name = "txtPrecio";
            txtPrecio.Size = new Size(100, 23);
            txtPrecio.TabIndex = 10;
            txtPrecio.TextChanged += txtPrecio_TextChanged;
            txtPrecio.KeyPress += txtPrecio_KeyPress;
            // 
            // txtCantidad
            // 
            txtCantidad.Location = new Point(93, 141);
            txtCantidad.Name = "txtCantidad";
            txtCantidad.Size = new Size(100, 23);
            txtCantidad.TabIndex = 11;
            // 
            // chkGananciaProd
            // 
            chkGananciaProd.AutoSize = true;
            chkGananciaProd.Location = new Point(256, 83);
            chkGananciaProd.Name = "chkGananciaProd";
            chkGananciaProd.Size = new Size(148, 19);
            chkGananciaProd.TabIndex = 15;
            chkGananciaProd.Text = "Ganancia por producto";
            chkGananciaProd.UseVisualStyleBackColor = true;
            chkGananciaProd.CheckedChanged += chkGananciaProd_CheckedChanged;
            // 
            // lblGanancia
            // 
            lblGanancia.AutoSize = true;
            lblGanancia.Location = new Point(256, 141);
            lblGanancia.Name = "lblGanancia";
            lblGanancia.Size = new Size(56, 15);
            lblGanancia.TabIndex = 16;
            lblGanancia.Text = "Ganancia";
            // 
            // txtGanancia
            // 
            txtGanancia.Location = new Point(326, 138);
            txtGanancia.Name = "txtGanancia";
            txtGanancia.PlaceholderText = "00,00";
            txtGanancia.Size = new Size(80, 23);
            txtGanancia.TabIndex = 17;
            txtGanancia.TextChanged += txtGanancia_TextChanged;
            // 
            // chkSector
            // 
            chkSector.AutoSize = true;
            chkSector.Location = new Point(256, 183);
            chkSector.Name = "chkSector";
            chkSector.Size = new Size(59, 19);
            chkSector.TabIndex = 18;
            chkSector.Text = "Sector";
            chkSector.UseVisualStyleBackColor = true;
            chkSector.CheckedChanged += chkSector_CheckedChanged;
            // 
            // lblIVA
            // 
            lblIVA.AutoSize = true;
            lblIVA.Location = new Point(412, 141);
            lblIVA.Name = "lblIVA";
            lblIVA.Size = new Size(24, 15);
            lblIVA.TabIndex = 19;
            lblIVA.Text = "IVA";
            // 
            // txtIVA
            // 
            txtIVA.Location = new Point(442, 138);
            txtIVA.Name = "txtIVA";
            txtIVA.PlaceholderText = "00,00";
            txtIVA.Size = new Size(60, 23);
            txtIVA.TabIndex = 20;
            txtIVA.TextChanged += txtIVA_TextChanged;
            // 
            // lblGrupo
            // 
            lblGrupo.AutoSize = true;
            lblGrupo.Location = new Point(25, 176);
            lblGrupo.Name = "lblGrupo";
            lblGrupo.Size = new Size(40, 15);
            lblGrupo.TabIndex = 22;
            lblGrupo.Text = "Grupo";
            // 
            // lblFechaModificacion
            // 
            lblFechaModificacion.AutoSize = true;
            lblFechaModificacion.Font = new Font("Segoe UI Semibold", 9F, FontStyle.Bold | FontStyle.Italic, GraphicsUnit.Point, 0);
            lblFechaModificacion.Location = new Point(27, 9);
            lblFechaModificacion.Name = "lblFechaModificacion";
            lblFechaModificacion.Size = new Size(0, 15);
            lblFechaModificacion.TabIndex = 23;
            // 
            // lblGrupoSel
            // 
            lblGrupoSel.AutoSize = true;
            lblGrupoSel.Font = new Font("Segoe UI Semibold", 9F, FontStyle.Bold, GraphicsUnit.Point, 0);
            lblGrupoSel.Location = new Point(93, 176);
            lblGrupoSel.Name = "lblGrupoSel";
            lblGrupoSel.Size = new Size(59, 15);
            lblGrupoSel.TabIndex = 24;
            lblGrupoSel.Text = "Sin grupo";
            // 
            // btnBuscarGrupo
            // 
            btnBuscarGrupo.Font = new Font("Segoe UI", 9F, FontStyle.Bold, GraphicsUnit.Point, 0);
            btnBuscarGrupo.ForeColor = Color.DarkGreen;
            btnBuscarGrupo.Location = new Point(12, 205);
            btnBuscarGrupo.Name = "btnBuscarGrupo";
            btnBuscarGrupo.Size = new Size(93, 23);
            btnBuscarGrupo.TabIndex = 25;
            btnBuscarGrupo.Text = "Buscar Grupo";
            btnBuscarGrupo.UseVisualStyleBackColor = true;
            btnBuscarGrupo.Click += btnBuscarGrupo_Click;
            // 
            // btnSacarGrupo
            // 
            btnSacarGrupo.Font = new Font("Segoe UI", 9F, FontStyle.Bold, GraphicsUnit.Point, 0);
            btnSacarGrupo.ForeColor = Color.Brown;
            btnSacarGrupo.Location = new Point(111, 205);
            btnSacarGrupo.Name = "btnSacarGrupo";
            btnSacarGrupo.Size = new Size(83, 23);
            btnSacarGrupo.TabIndex = 26;
            btnSacarGrupo.Text = "Sacar Grupo";
            btnSacarGrupo.UseVisualStyleBackColor = true;
            btnSacarGrupo.Click += btnSacarGrupo_Click;
            // 
            // frmProducto
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(524, 267);
            Controls.Add(btnSacarGrupo);
            Controls.Add(btnBuscarGrupo);
            Controls.Add(lblGrupoSel);
            Controls.Add(lblFechaModificacion);
            Controls.Add(lblGrupo);
            Controls.Add(txtIVA);
            Controls.Add(lblIVA);
            Controls.Add(chkSector);
            Controls.Add(txtGanancia);
            Controls.Add(lblGanancia);
            Controls.Add(chkGananciaProd);
            Controls.Add(txtCantidad);
            Controls.Add(txtPrecio);
            Controls.Add(txtCosto);
            Controls.Add(txtNombre);
            Controls.Add(txtCodigo);
            Controls.Add(label5);
            Controls.Add(label4);
            Controls.Add(label3);
            Controls.Add(label2);
            Controls.Add(label1);
            Controls.Add(brnCancelar);
            Controls.Add(brnGrabar);
            Name = "frmProducto";
            Text = "Producto";
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Button brnGrabar;
        private Button brnCancelar;
        private Label label1;
        private Label label2;
        private Label label3;
        private Label label4;
        private Label label5;
        private TextBox txtCodigo;
        private TextBox txtNombre;
        private TextBox txtCosto;
        private TextBox txtPrecio;
        private TextBox txtCantidad;
        private CheckBox chkGananciaProd;
        private Label lblGanancia;
        private TextBox txtGanancia;
        private CheckBox chkSector;
        private Label lblIVA;
        private TextBox txtIVA;
        private Label lblGrupo;
        private Label lblFechaModificacion;
        private Label lblGrupoSel;
        private Button btnBuscarGrupo;
        private Button btnSacarGrupo;
    }
}