using System.Windows.Forms;

namespace StockControl
{
    partial class StockMain
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
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
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            dataGridView1 = new DataGridView();
            groupBox1 = new GroupBox();
            chkMultiPago = new CheckBox();
            cbMetodosPago = new ComboBox();
            chkCobroEnPesos = new CheckBox();
            txtTotal = new Label();
            lblTotal = new Label();
            dataGridView2 = new DataGridView();
            brnCancelar = new Button();
            btnCobrar = new Button();
            btnCodeBar = new Button();
            btnEliminar = new Button();
            btnEditar = new Button();
            btnAgregar = new Button();
            label2 = new Label();
            label3 = new Label();
            txtBuscar = new TextBox();
            btnVerInforme = new Button();
            label1 = new Label();
            txtScanner = new TextBox();
            btnConfiguracion = new Button();
            lblDolar = new Label();
            txtValorDolar = new TextBox();
            btnGrupos = new Button();
            label4 = new Label();
            lblCantProd = new Label();
            ((System.ComponentModel.ISupportInitialize)dataGridView1).BeginInit();
            groupBox1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)dataGridView2).BeginInit();
            SuspendLayout();
            // 
            // dataGridView1
            // 
            dataGridView1.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            dataGridView1.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dataGridView1.Location = new Point(12, 39);
            dataGridView1.Name = "dataGridView1";
            dataGridView1.Size = new Size(710, 538);
            dataGridView1.TabIndex = 0;
            dataGridView1.CellDoubleClick += dataGridViewProductos_CellDoubleClick;
            dataGridView1.CellFormatting += dataGridView1_CellFormatting;
            // 
            // groupBox1
            // 
            groupBox1.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Right;
            groupBox1.Controls.Add(chkMultiPago);
            groupBox1.Controls.Add(cbMetodosPago);
            groupBox1.Controls.Add(chkCobroEnPesos);
            groupBox1.Controls.Add(txtTotal);
            groupBox1.Controls.Add(lblTotal);
            groupBox1.Controls.Add(dataGridView2);
            groupBox1.Controls.Add(brnCancelar);
            groupBox1.Controls.Add(btnCobrar);
            groupBox1.Location = new Point(728, 39);
            groupBox1.Name = "groupBox1";
            groupBox1.Size = new Size(367, 578);
            groupBox1.TabIndex = 1;
            groupBox1.TabStop = false;
            groupBox1.Text = "Opciones";
            // 
            // chkMultiPago
            // 
            chkMultiPago.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            chkMultiPago.AutoSize = true;
            chkMultiPago.Font = new Font("Segoe UI Semibold", 9F, FontStyle.Bold | FontStyle.Italic, GraphicsUnit.Point, 0);
            chkMultiPago.Location = new Point(183, 546);
            chkMultiPago.Name = "chkMultiPago";
            chkMultiPago.Size = new Size(170, 19);
            chkMultiPago.TabIndex = 11;
            chkMultiPago.Text = "Multiples Metodos de pago";
            chkMultiPago.UseVisualStyleBackColor = true;
            chkMultiPago.CheckedChanged += chkMultiPago_CheckedChanged;
            // 
            // cbMetodosPago
            // 
            cbMetodosPago.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            cbMetodosPago.FormattingEnabled = true;
            cbMetodosPago.Location = new Point(20, 544);
            cbMetodosPago.Name = "cbMetodosPago";
            cbMetodosPago.Size = new Size(156, 23);
            cbMetodosPago.TabIndex = 10;
            cbMetodosPago.SelectedIndexChanged += cbMetodosPago_SelectedIndexChanged;
            // 
            // chkCobroEnPesos
            // 
            chkCobroEnPesos.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            chkCobroEnPesos.AutoSize = true;
            chkCobroEnPesos.Location = new Point(250, 495);
            chkCobroEnPesos.Name = "chkCobroEnPesos";
            chkCobroEnPesos.Size = new Size(111, 19);
            chkCobroEnPesos.TabIndex = 9;
            chkCobroEnPesos.Text = "Cobrar en pesos";
            chkCobroEnPesos.UseVisualStyleBackColor = true;
            chkCobroEnPesos.CheckedChanged += chkCobroEnPesos_CheckedChanged;
            // 
            // txtTotal
            // 
            txtTotal.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            txtTotal.AutoSize = true;
            txtTotal.Font = new Font("Segoe UI", 15F);
            txtTotal.Location = new Point(182, 520);
            txtTotal.Name = "txtTotal";
            txtTotal.Size = new Size(67, 28);
            txtTotal.TabIndex = 7;
            txtTotal.Text = "$$$$$";
            // 
            // lblTotal
            // 
            lblTotal.Anchor = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            lblTotal.AutoSize = true;
            lblTotal.Location = new Point(182, 496);
            lblTotal.Name = "lblTotal";
            lblTotal.Size = new Size(41, 15);
            lblTotal.TabIndex = 6;
            lblTotal.Text = "TOTAL";
            // 
            // dataGridView2
            // 
            dataGridView2.AllowUserToAddRows = false;
            dataGridView2.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            dataGridView2.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dataGridView2.Location = new Point(20, 22);
            dataGridView2.Name = "dataGridView2";
            dataGridView2.Size = new Size(341, 468);
            dataGridView2.TabIndex = 5;
            dataGridView2.CellBeginEdit += dataGridViewProductos_CellBeginEdit;
            dataGridView2.CellMouseDoubleClick += dataGridView2_CellMouseDoubleClick;
            dataGridView2.CellValueChanged += dataGridViewCarrito_CellValueChanged;
            dataGridView2.CurrentCellDirtyStateChanged += dataGridViewCarrito_CurrentCellDirtyStateChanged;
            dataGridView2.EditingControlShowing += dataGridViewCarrito_EditingControlShowing;
            // 
            // brnCancelar
            // 
            brnCancelar.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            brnCancelar.ForeColor = Color.FromArgb(192, 0, 0);
            brnCancelar.Location = new Point(101, 496);
            brnCancelar.Name = "brnCancelar";
            brnCancelar.Size = new Size(75, 42);
            brnCancelar.TabIndex = 4;
            brnCancelar.Text = "CANCELAR";
            brnCancelar.UseVisualStyleBackColor = true;
            brnCancelar.Click += brnCancelar_Click;
            // 
            // btnCobrar
            // 
            btnCobrar.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            btnCobrar.Font = new Font("Segoe UI", 9F, FontStyle.Bold, GraphicsUnit.Point, 0);
            btnCobrar.ForeColor = Color.ForestGreen;
            btnCobrar.Location = new Point(20, 496);
            btnCobrar.Name = "btnCobrar";
            btnCobrar.Size = new Size(75, 42);
            btnCobrar.TabIndex = 3;
            btnCobrar.Text = "COBRAR";
            btnCobrar.UseVisualStyleBackColor = true;
            btnCobrar.Click += btnCobrar_Click;
            // 
            // btnCodeBar
            // 
            btnCodeBar.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            btnCodeBar.Font = new Font("Segoe UI", 9F, FontStyle.Bold, GraphicsUnit.Point, 0);
            btnCodeBar.Location = new Point(387, 583);
            btnCodeBar.Name = "btnCodeBar";
            btnCodeBar.Size = new Size(75, 38);
            btnCodeBar.TabIndex = 8;
            btnCodeBar.Text = "GENERAR BARCODE";
            btnCodeBar.UseVisualStyleBackColor = true;
            btnCodeBar.Click += btnCodeBar_Click;
            // 
            // btnEliminar
            // 
            btnEliminar.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            btnEliminar.Font = new Font("Segoe UI", 9F, FontStyle.Bold, GraphicsUnit.Point, 0);
            btnEliminar.ForeColor = Color.Maroon;
            btnEliminar.Location = new Point(300, 583);
            btnEliminar.Name = "btnEliminar";
            btnEliminar.Size = new Size(83, 38);
            btnEliminar.TabIndex = 2;
            btnEliminar.Text = "ELIMINAR PRODUCTO";
            btnEliminar.UseVisualStyleBackColor = true;
            btnEliminar.Click += button3_Click;
            // 
            // btnEditar
            // 
            btnEditar.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            btnEditar.Font = new Font("Segoe UI", 9F, FontStyle.Bold, GraphicsUnit.Point, 0);
            btnEditar.Location = new Point(211, 583);
            btnEditar.Name = "btnEditar";
            btnEditar.Size = new Size(83, 38);
            btnEditar.TabIndex = 1;
            btnEditar.Text = "EDITAR PRODUCTO";
            btnEditar.UseVisualStyleBackColor = true;
            btnEditar.Click += btnEditar_Click;
            // 
            // btnAgregar
            // 
            btnAgregar.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            btnAgregar.Font = new Font("Segoe UI", 9F, FontStyle.Bold, GraphicsUnit.Point, 0);
            btnAgregar.ForeColor = Color.Green;
            btnAgregar.Location = new Point(121, 583);
            btnAgregar.Name = "btnAgregar";
            btnAgregar.Size = new Size(84, 38);
            btnAgregar.TabIndex = 0;
            btnAgregar.Text = "NUEVO PRODUCTO";
            btnAgregar.UseVisualStyleBackColor = true;
            btnAgregar.Click += btnAgregar_Click;
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Font = new Font("Segoe UI Semibold", 12F, FontStyle.Bold | FontStyle.Italic, GraphicsUnit.Point, 0);
            label2.ForeColor = Color.FromArgb(0, 0, 192);
            label2.Location = new Point(12, 9);
            label2.Name = "label2";
            label2.Size = new Size(104, 21);
            label2.TabIndex = 2;
            label2.Text = "PRODUCTOS";
            // 
            // label3
            // 
            label3.AutoSize = true;
            label3.Font = new Font("Segoe UI", 9F, FontStyle.Bold, GraphicsUnit.Point, 0);
            label3.Location = new Point(323, 15);
            label3.Name = "label3";
            label3.Size = new Size(58, 15);
            label3.TabIndex = 3;
            label3.Text = "Buscador";
            // 
            // txtBuscar
            // 
            txtBuscar.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left;
            txtBuscar.Location = new Point(387, 12);
            txtBuscar.Name = "txtBuscar";
            txtBuscar.PlaceholderText = "Buscar por nombre o codigo";
            txtBuscar.Size = new Size(264, 23);
            txtBuscar.TabIndex = 4;
            txtBuscar.TextChanged += textBox1_TextChanged;
            // 
            // btnVerInforme
            // 
            btnVerInforme.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            btnVerInforme.Location = new Point(12, 583);
            btnVerInforme.Name = "btnVerInforme";
            btnVerInforme.Size = new Size(104, 34);
            btnVerInforme.TabIndex = 5;
            btnVerInforme.Text = "Ver Informes";
            btnVerInforme.UseVisualStyleBackColor = true;
            btnVerInforme.Click += btnVerInforme_Click;
            // 
            // label1
            // 
            label1.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left;
            label1.AutoSize = true;
            label1.Font = new Font("Segoe UI", 9F, FontStyle.Bold, GraphicsUnit.Point, 0);
            label1.Location = new Point(657, 15);
            label1.Name = "label1";
            label1.Size = new Size(114, 15);
            label1.TabIndex = 6;
            label1.Text = "Agregar por codigo";
            // 
            // txtScanner
            // 
            txtScanner.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left;
            txtScanner.Location = new Point(777, 12);
            txtScanner.Name = "txtScanner";
            txtScanner.PlaceholderText = "Codigo + Enter";
            txtScanner.Size = new Size(174, 23);
            txtScanner.TabIndex = 7;
            txtScanner.KeyDown += txtScanner_KeyDown;
            // 
            // btnConfiguracion
            // 
            btnConfiguracion.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnConfiguracion.Location = new Point(1058, 9);
            btnConfiguracion.Name = "btnConfiguracion";
            btnConfiguracion.Size = new Size(26, 26);
            btnConfiguracion.TabIndex = 8;
            btnConfiguracion.UseVisualStyleBackColor = true;
            btnConfiguracion.Click += btnConfiguracion_Click;
            // 
            // lblDolar
            // 
            lblDolar.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            lblDolar.AutoSize = true;
            lblDolar.Location = new Point(552, 600);
            lblDolar.Name = "lblDolar";
            lblDolar.Size = new Size(64, 15);
            lblDolar.TabIndex = 9;
            lblDolar.Text = "Valor Dolar";
            // 
            // txtValorDolar
            // 
            txtValorDolar.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            txtValorDolar.Location = new Point(622, 592);
            txtValorDolar.Name = "txtValorDolar";
            txtValorDolar.Size = new Size(100, 23);
            txtValorDolar.TabIndex = 10;
            txtValorDolar.Leave += txtValorDolar_Leave;
            // 
            // btnGrupos
            // 
            btnGrupos.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            btnGrupos.Font = new Font("Segoe UI", 8.25F, FontStyle.Bold, GraphicsUnit.Point, 0);
            btnGrupos.Location = new Point(468, 583);
            btnGrupos.Name = "btnGrupos";
            btnGrupos.Size = new Size(85, 38);
            btnGrupos.TabIndex = 11;
            btnGrupos.Text = "GRUPO DE PRODUCTOS";
            btnGrupos.UseVisualStyleBackColor = true;
            btnGrupos.Click += btnGrupos_Click;
            // 
            // label4
            // 
            label4.AutoSize = true;
            label4.Font = new Font("Segoe UI Semibold", 9F, FontStyle.Bold, GraphicsUnit.Point, 0);
            label4.Location = new Point(147, 15);
            label4.Name = "label4";
            label4.Size = new Size(57, 15);
            label4.TabIndex = 12;
            label4.Text = "Cantidad:";
            // 
            // lblCantProd
            // 
            lblCantProd.AutoSize = true;
            lblCantProd.Location = new Point(213, 15);
            lblCantProd.Name = "lblCantProd";
            lblCantProd.Size = new Size(13, 15);
            lblCantProd.TabIndex = 13;
            lblCantProd.Text = "0";
            // 
            // StockMain
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1096, 629);
            Controls.Add(lblCantProd);
            Controls.Add(label4);
            Controls.Add(btnGrupos);
            Controls.Add(txtValorDolar);
            Controls.Add(lblDolar);
            Controls.Add(btnConfiguracion);
            Controls.Add(btnCodeBar);
            Controls.Add(txtScanner);
            Controls.Add(label1);
            Controls.Add(btnVerInforme);
            Controls.Add(txtBuscar);
            Controls.Add(label3);
            Controls.Add(btnEliminar);
            Controls.Add(label2);
            Controls.Add(btnEditar);
            Controls.Add(groupBox1);
            Controls.Add(btnAgregar);
            Controls.Add(dataGridView1);
            Name = "StockMain";
            Text = "StockMain";
            ((System.ComponentModel.ISupportInitialize)dataGridView1).EndInit();
            groupBox1.ResumeLayout(false);
            groupBox1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)dataGridView2).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private DataGridView dataGridView1;
        private GroupBox groupBox1;
        private Button brnCancelar;
        private Button btnCobrar;
        private Button btnEliminar;
        private Button btnEditar;
        private Button btnAgregar;
        private DataGridView dataGridView2;
        private Label txtTotal;
        private Label lblTotal;
        private Label label2;
        private Label label3;
        private TextBox txtBuscar;
        private Button btnVerInforme;
        private Label label1;
        private TextBox txtScanner;
        private Button btnConfiguracion;
        private Button btnCodeBar;
        private CheckBox chkCobroEnPesos;
        private Label lblDolar;
        private TextBox txtValorDolar;
        private ComboBox cbMetodosPago;
        private CheckBox chkMultiPago;
        private Button btnGrupos;
        private Label label4;
        private Label lblCantProd;
    }
}
