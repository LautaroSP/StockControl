namespace StockControl
{
    partial class Grupo
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
            Grupos = new GroupBox();
            txtIVA = new TextBox();
            label6 = new Label();
            txtGanancia = new TextBox();
            Ganancia = new Label();
            chkGananciaProd = new CheckBox();
            txtCosto = new TextBox();
            Costo = new Label();
            lblCantProductos = new Label();
            label5 = new Label();
            label4 = new Label();
            label3 = new Label();
            label2 = new Label();
            btnNuevoGrupo = new Button();
            btnEliminar = new Button();
            btnGuardar = new Button();
            txtPrecio = new TextBox();
            txtNombreGrupo = new TextBox();
            dgGrupos = new DataGridView();
            groupBox2 = new GroupBox();
            cbMostrarSeleccionados = new CheckBox();
            dgProductos = new DataGridView();
            label1 = new Label();
            txtBuscador = new TextBox();
            Grupos.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)dgGrupos).BeginInit();
            groupBox2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)dgProductos).BeginInit();
            SuspendLayout();
            // 
            // Grupos
            // 
            Grupos.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            Grupos.Controls.Add(txtIVA);
            Grupos.Controls.Add(label6);
            Grupos.Controls.Add(txtGanancia);
            Grupos.Controls.Add(Ganancia);
            Grupos.Controls.Add(chkGananciaProd);
            Grupos.Controls.Add(txtCosto);
            Grupos.Controls.Add(Costo);
            Grupos.Controls.Add(lblCantProductos);
            Grupos.Controls.Add(label5);
            Grupos.Controls.Add(label4);
            Grupos.Controls.Add(label3);
            Grupos.Controls.Add(label2);
            Grupos.Controls.Add(btnNuevoGrupo);
            Grupos.Controls.Add(btnEliminar);
            Grupos.Controls.Add(btnGuardar);
            Grupos.Controls.Add(txtPrecio);
            Grupos.Controls.Add(txtNombreGrupo);
            Grupos.Controls.Add(dgGrupos);
            Grupos.Font = new Font("Segoe UI Semibold", 9F, FontStyle.Bold | FontStyle.Italic, GraphicsUnit.Point, 0);
            Grupos.Location = new Point(12, 12);
            Grupos.Name = "Grupos";
            Grupos.Size = new Size(997, 182);
            Grupos.TabIndex = 0;
            Grupos.TabStop = false;
            Grupos.Text = "Grupos";
            // 
            // txtIVA
            // 
            txtIVA.Location = new Point(940, 91);
            txtIVA.Name = "txtIVA";
            txtIVA.PlaceholderText = "00,00";
            txtIVA.Size = new Size(38, 24);
            txtIVA.TabIndex = 6;
            txtIVA.TextChanged += txtIVA_TextChanged;
            // 
            // label6
            // 
            label6.AutoSize = true;
            label6.Location = new Point(908, 97);
            label6.Name = "label6";
            label6.Size = new Size(26, 15);
            label6.TabIndex = 24;
            label6.Text = "IVA";
            // 
            // txtGanancia
            // 
            txtGanancia.Location = new Point(802, 91);
            txtGanancia.Name = "txtGanancia";
            txtGanancia.PlaceholderText = "00,00";
            txtGanancia.Size = new Size(100, 24);
            txtGanancia.TabIndex = 5;
            txtGanancia.TextChanged += txtGanancia_TextChanged;
            // 
            // Ganancia
            // 
            Ganancia.AutoSize = true;
            Ganancia.Location = new Point(738, 97);
            Ganancia.Name = "Ganancia";
            Ganancia.Size = new Size(58, 15);
            Ganancia.TabIndex = 22;
            Ganancia.Text = "Ganancia";
            // 
            // chkGananciaProd
            // 
            chkGananciaProd.AutoSize = true;
            chkGananciaProd.Location = new Point(754, 60);
            chkGananciaProd.Name = "chkGananciaProd";
            chkGananciaProd.Size = new Size(151, 19);
            chkGananciaProd.TabIndex = 4;
            chkGananciaProd.Text = "Ganancia por producto";
            chkGananciaProd.UseVisualStyleBackColor = true;
            chkGananciaProd.CheckedChanged += chkGananciaProd_CheckedChanged;
            // 
            // txtCosto
            // 
            txtCosto.Location = new Point(665, 58);
            txtCosto.Name = "txtCosto";
            txtCosto.PlaceholderText = "0.00";
            txtCosto.Size = new Size(58, 24);
            txtCosto.TabIndex = 2;
            txtCosto.TextChanged += txtCosto_TextChanged;
            // 
            // Costo
            // 
            Costo.AutoSize = true;
            Costo.Location = new Point(607, 62);
            Costo.Name = "Costo";
            Costo.Size = new Size(38, 15);
            Costo.TabIndex = 11;
            Costo.Text = "Costo";
            // 
            // lblCantProductos
            // 
            lblCantProductos.AutoSize = true;
            lblCantProductos.ForeColor = Color.IndianRed;
            lblCantProductos.Location = new Point(972, 158);
            lblCantProductos.Name = "lblCantProductos";
            lblCantProductos.Size = new Size(14, 15);
            lblCantProductos.TabIndex = 10;
            lblCantProductos.Text = "0";
            // 
            // label5
            // 
            label5.AutoSize = true;
            label5.Location = new Point(836, 156);
            label5.Name = "label5";
            label5.Size = new Size(130, 15);
            label5.TabIndex = 9;
            label5.Text = "Cantidad de productos";
            // 
            // label4
            // 
            label4.AutoSize = true;
            label4.Location = new Point(607, 11);
            label4.Name = "label4";
            label4.Size = new Size(41, 15);
            label4.TabIndex = 8;
            label4.Text = "Grupo";
            // 
            // label3
            // 
            label3.AutoSize = true;
            label3.Location = new Point(607, 91);
            label3.Name = "label3";
            label3.Size = new Size(43, 15);
            label3.TabIndex = 7;
            label3.Text = "Precio ";
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Location = new Point(607, 37);
            label2.Name = "label2";
            label2.Size = new Size(52, 15);
            label2.TabIndex = 6;
            label2.Text = "Nombre";
            // 
            // btnNuevoGrupo
            // 
            btnNuevoGrupo.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            btnNuevoGrupo.ForeColor = SystemColors.Highlight;
            btnNuevoGrupo.Location = new Point(486, 23);
            btnNuevoGrupo.Name = "btnNuevoGrupo";
            btnNuevoGrupo.Size = new Size(98, 43);
            btnNuevoGrupo.TabIndex = 7;
            btnNuevoGrupo.Text = "Nuevo Grupo";
            btnNuevoGrupo.UseVisualStyleBackColor = true;
            btnNuevoGrupo.Click += btnNuevoGrupo_Click;
            // 
            // btnEliminar
            // 
            btnEliminar.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            btnEliminar.ForeColor = Color.IndianRed;
            btnEliminar.Location = new Point(486, 74);
            btnEliminar.Name = "btnEliminar";
            btnEliminar.Size = new Size(98, 43);
            btnEliminar.TabIndex = 8;
            btnEliminar.Text = "Eliminar";
            btnEliminar.UseVisualStyleBackColor = true;
            btnEliminar.Click += btnEliminar_Click;
            // 
            // btnGuardar
            // 
            btnGuardar.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            btnGuardar.ForeColor = Color.OliveDrab;
            btnGuardar.Location = new Point(486, 123);
            btnGuardar.Name = "btnGuardar";
            btnGuardar.Size = new Size(98, 43);
            btnGuardar.TabIndex = 9;
            btnGuardar.Text = "Guardar Cambios";
            btnGuardar.UseVisualStyleBackColor = true;
            btnGuardar.Click += btnGuardar_Click;
            // 
            // txtPrecio
            // 
            txtPrecio.Location = new Point(665, 88);
            txtPrecio.Name = "txtPrecio";
            txtPrecio.PlaceholderText = "00.00";
            txtPrecio.Size = new Size(58, 24);
            txtPrecio.TabIndex = 3;
            txtPrecio.TextChanged += txtPrecio_TextChanged;
            txtPrecio.KeyPress += txtPrecio_KeyPress;
            // 
            // txtNombreGrupo
            // 
            txtNombreGrupo.Location = new Point(665, 28);
            txtNombreGrupo.Name = "txtNombreGrupo";
            txtNombreGrupo.Size = new Size(171, 24);
            txtNombreGrupo.TabIndex = 1;
            txtNombreGrupo.TextChanged += txtNombreGrupo_TextChanged;
            // 
            // dgGrupos
            // 
            dgGrupos.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dgGrupos.Location = new Point(6, 23);
            dgGrupos.Name = "dgGrupos";
            dgGrupos.ReadOnly = true;
            dgGrupos.Size = new Size(465, 150);
            dgGrupos.TabIndex = 0;
            dgGrupos.CellClick += dgGrupos_RowEnter;
            dgGrupos.RowEnter += dgGrupos_RowEnter;
            // 
            // groupBox2
            // 
            groupBox2.Anchor = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            groupBox2.Controls.Add(cbMostrarSeleccionados);
            groupBox2.Controls.Add(dgProductos);
            groupBox2.Controls.Add(label1);
            groupBox2.Controls.Add(txtBuscador);
            groupBox2.Font = new Font("Segoe UI Semibold", 9F, FontStyle.Bold | FontStyle.Italic, GraphicsUnit.Point, 0);
            groupBox2.Location = new Point(12, 200);
            groupBox2.Name = "groupBox2";
            groupBox2.Size = new Size(997, 343);
            groupBox2.TabIndex = 1;
            groupBox2.TabStop = false;
            groupBox2.Text = "Productos";
            // 
            // cbMostrarSeleccionados
            // 
            cbMostrarSeleccionados.AutoSize = true;
            cbMostrarSeleccionados.Location = new Point(815, 314);
            cbMostrarSeleccionados.Name = "cbMostrarSeleccionados";
            cbMostrarSeleccionados.Size = new Size(176, 19);
            cbMostrarSeleccionados.TabIndex = 20;
            cbMostrarSeleccionados.Text = "MOSTRAR SELECCIONADOS";
            cbMostrarSeleccionados.UseVisualStyleBackColor = true;
            cbMostrarSeleccionados.CheckedChanged += cbMostrarSeleccionados_CheckedChanged;
            // 
            // dgProductos
            // 
            dgProductos.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            dgProductos.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dgProductos.Location = new Point(6, 18);
            dgProductos.Name = "dgProductos";
            dgProductos.ReadOnly = true;
            dgProductos.Size = new Size(985, 289);
            dgProductos.TabIndex = 10;
            // 
            // label1
            // 
            label1.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            label1.AutoSize = true;
            label1.Location = new Point(6, 315);
            label1.Name = "label1";
            label1.Size = new Size(95, 15);
            label1.TabIndex = 1;
            label1.Text = "Buscar producto";
            // 
            // txtBuscador
            // 
            txtBuscador.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            txtBuscador.Location = new Point(107, 312);
            txtBuscador.Name = "txtBuscador";
            txtBuscador.PlaceholderText = "Codigo o nombre";
            txtBuscador.Size = new Size(156, 24);
            txtBuscador.TabIndex = 0;
            txtBuscador.TextChanged += txtBuscador_TextChanged;
            // 
            // Grupo
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1021, 564);
            Controls.Add(groupBox2);
            Controls.Add(Grupos);
            MaximizeBox = false;
            Name = "Grupo";
            Text = "frmGrupo";
            Grupos.ResumeLayout(false);
            Grupos.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)dgGrupos).EndInit();
            groupBox2.ResumeLayout(false);
            groupBox2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)dgProductos).EndInit();
            ResumeLayout(false);
        }

        #endregion

        private GroupBox Grupos;
        private GroupBox groupBox2;
        private Label label1;
        private TextBox txtBuscador;
        private DataGridView dgProductos;
        private Button btnGuardar;
        private TextBox txtPrecio;
        private TextBox txtNombreGrupo;
        private DataGridView dgGrupos;
        private Label lblCantProductos;
        private Label label5;
        private Label label4;
        private Label label3;
        private Label label2;
        private Button btnNuevoGrupo;
        private Button btnEliminar;
        private CheckBox cbMostrarSeleccionados;
        private TextBox txtCosto;
        private Label Costo;
        private TextBox txtIVA;
        private Label label6;
        private TextBox txtGanancia;
        private Label Ganancia;
        private CheckBox chkGananciaProd;
    }
}