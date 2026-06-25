
namespace StockControl
{
    partial class frmInformeVentas
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
            dataGridView1 = new DataGridView();
            label1 = new Label();
            btnEliminar = new Button();
            btnSalir = new Button();
            tabInformes = new TabControl();
            tabPage1 = new TabPage();
            btnCerrarCajaAnterior = new Button();
            btnCopiarTicket = new Button();
            btnImprimirTicket = new Button();
            btnCerrarCaja = new Button();
            tabPage2 = new TabPage();
            chkTodos = new CheckBox();
            dtCajas = new DataGridView();
            label2 = new Label();
            tabPage3 = new TabPage();
            label3 = new Label();
            dtpMes = new DateTimePicker();
            btnGenerarResumen = new Button();
            dtResumen = new DataGridView();
            lblTotalMes = new Label();
            ((System.ComponentModel.ISupportInitialize)dataGridView1).BeginInit();
            ((System.ComponentModel.ISupportInitialize)dtResumen).BeginInit();
            tabInformes.SuspendLayout();
            tabPage1.SuspendLayout();
            tabPage2.SuspendLayout();
            tabPage3.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)dtCajas).BeginInit();
            SuspendLayout();
            // 
            // dataGridView1
            // 
            dataGridView1.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            dataGridView1.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dataGridView1.Location = new Point(6, 21);
            dataGridView1.Name = "dataGridView1";
            dataGridView1.Size = new Size(756, 342);
            dataGridView1.TabIndex = 0;
            dataGridView1.CellContentDoubleClick += dataGridView1_CellDoubleClick;
            dataGridView1.CellDoubleClick += dataGridView1_CellDoubleClick;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Font = new Font("Segoe UI Semibold", 9F, FontStyle.Bold | FontStyle.Italic, GraphicsUnit.Point, 0);
            label1.ForeColor = SystemColors.HotTrack;
            label1.Location = new Point(3, 3);
            label1.Name = "label1";
            label1.Size = new Size(104, 15);
            label1.TabIndex = 1;
            label1.Text = "Informes de venta";
            // 
            // btnEliminar
            // 
            btnEliminar.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            btnEliminar.Location = new Point(4, 369);
            btnEliminar.Name = "btnEliminar";
            btnEliminar.Size = new Size(75, 23);
            btnEliminar.TabIndex = 2;
            btnEliminar.Text = "Eliminar";
            btnEliminar.UseVisualStyleBackColor = true;
            btnEliminar.Click += btnEliminar_Click;
            // 
            // btnSalir
            // 
            btnSalir.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            btnSalir.Location = new Point(85, 369);
            btnSalir.Name = "btnSalir";
            btnSalir.Size = new Size(75, 23);
            btnSalir.TabIndex = 3;
            btnSalir.Text = "Salir";
            btnSalir.UseVisualStyleBackColor = true;
            btnSalir.Click += btnSalir_Click;
            // 
            // tabInformes
            // 
            tabInformes.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            tabInformes.Controls.Add(tabPage1);
            tabInformes.Controls.Add(tabPage2);
            tabInformes.Controls.Add(tabPage3);
            tabInformes.Location = new Point(7, 12);
            tabInformes.Name = "tabInformes";
            tabInformes.SelectedIndex = 0;
            tabInformes.Size = new Size(776, 426);
            tabInformes.TabIndex = 4;
            // 
            // tabPage1
            // 
            tabPage1.Controls.Add(btnCerrarCajaAnterior);
            tabPage1.Controls.Add(btnCopiarTicket);
            tabPage1.Controls.Add(btnImprimirTicket);
            tabPage1.Controls.Add(btnCerrarCaja);
            tabPage1.Controls.Add(label1);
            tabPage1.Controls.Add(btnSalir);
            tabPage1.Controls.Add(dataGridView1);
            tabPage1.Controls.Add(btnEliminar);
            tabPage1.Location = new Point(4, 24);
            tabPage1.Name = "tabPage1";
            tabPage1.Padding = new Padding(3);
            tabPage1.Size = new Size(768, 398);
            tabPage1.TabIndex = 0;
            tabPage1.Text = "Informes de Venta";
            tabPage1.UseVisualStyleBackColor = true;
            // 
            // btnCerrarCajaAnterior
            // 
            btnCerrarCajaAnterior.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            btnCerrarCajaAnterior.Location = new Point(350, 369);
            btnCerrarCajaAnterior.Name = "btnCerrarCajaAnterior";
            btnCerrarCajaAnterior.Size = new Size(123, 23);
            btnCerrarCajaAnterior.TabIndex = 6;
            btnCerrarCajaAnterior.Text = "Cerrar Caja Anterior";
            btnCerrarCajaAnterior.UseVisualStyleBackColor = true;
            btnCerrarCajaAnterior.Click += btnCerrarCajaAnterior_Click;
            // 
            // btnCopiarTicket
            // 
            btnCopiarTicket.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            btnCopiarTicket.Location = new Point(485, 369);
            btnCopiarTicket.Name = "btnCopiarTicket";
            btnCopiarTicket.Size = new Size(85, 23);
            btnCopiarTicket.TabIndex = 5;
            btnCopiarTicket.Text = "Copiar ticket";
            btnCopiarTicket.UseVisualStyleBackColor = true;
            btnCopiarTicket.Click += btnCopiarTicket_Click;
            // 
            // btnImprimirTicket
            // 
            btnImprimirTicket.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            btnImprimirTicket.Location = new Point(580, 369);
            btnImprimirTicket.Name = "btnImprimirTicket";
            btnImprimirTicket.Size = new Size(85, 23);
            btnImprimirTicket.TabIndex = 7;
            btnImprimirTicket.Text = "Imprimir ticket";
            btnImprimirTicket.UseVisualStyleBackColor = true;
            btnImprimirTicket.Click += btnImprimirTicket_Click;
            // 
            // btnCerrarCaja
            // 
            btnCerrarCaja.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            btnCerrarCaja.Location = new Point(675, 369);
            btnCerrarCaja.Name = "btnCerrarCaja";
            btnCerrarCaja.Size = new Size(75, 23);
            btnCerrarCaja.TabIndex = 4;
            btnCerrarCaja.Text = "Cerrar Caja";
            btnCerrarCaja.UseVisualStyleBackColor = true;
            btnCerrarCaja.Click += btnCerrarCaja_Click;
            // 
            // tabPage2
            // 
            tabPage2.Controls.Add(chkTodos);
            tabPage2.Controls.Add(dtCajas);
            tabPage2.Controls.Add(label2);
            tabPage2.Location = new Point(4, 24);
            tabPage2.Name = "tabPage2";
            tabPage2.Padding = new Padding(3);
            tabPage2.Size = new Size(768, 398);
            tabPage2.TabIndex = 1;
            tabPage2.Text = "Cajas Cerradas";
            tabPage2.UseVisualStyleBackColor = true;
            // 
            // chkTodos
            // 
            chkTodos.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            chkTodos.AutoSize = true;
            chkTodos.Location = new Point(664, 6);
            chkTodos.Name = "chkTodos";
            chkTodos.Size = new Size(95, 19);
            chkTodos.TabIndex = 4;
            chkTodos.Text = "Mostrar todo";
            chkTodos.UseVisualStyleBackColor = true;
            chkTodos.CheckedChanged += chkTodos_CheckedChanged;
            // 
            // dtCajas
            // 
            dtCajas.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            dtCajas.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dtCajas.Location = new Point(3, 36);
            dtCajas.Name = "dtCajas";
            dtCajas.Size = new Size(756, 342);
            dtCajas.TabIndex = 3;
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Font = new Font("Segoe UI Semibold", 9F, FontStyle.Bold | FontStyle.Italic, GraphicsUnit.Point, 0);
            label2.ForeColor = SystemColors.HotTrack;
            label2.Location = new Point(6, 3);
            label2.Name = "label2";
            label2.Size = new Size(77, 15);
            label2.TabIndex = 2;
            label2.Text = "Caja cerrada";
            // 
            // tabPage3
            // 
            tabPage3.Controls.Add(lblTotalMes);
            tabPage3.Controls.Add(btnGenerarResumen);
            tabPage3.Controls.Add(dtResumen);
            tabPage3.Controls.Add(dtpMes);
            tabPage3.Controls.Add(label3);
            tabPage3.Location = new Point(4, 24);
            tabPage3.Name = "tabPage3";
            tabPage3.Padding = new Padding(3);
            tabPage3.Size = new Size(768, 398);
            tabPage3.TabIndex = 2;
            tabPage3.Text = "Resumen Mensual";
            tabPage3.UseVisualStyleBackColor = true;
            // 
            // label3
            // 
            label3.AutoSize = true;
            label3.Location = new Point(6, 8);
            label3.Name = "label3";
            label3.Size = new Size(32, 15);
            label3.TabIndex = 0;
            label3.Text = "Mes:";
            // 
            // dtpMes
            // 
            dtpMes.Format = DateTimePickerFormat.Custom;
            dtpMes.CustomFormat = "MM/yyyy";
            dtpMes.Location = new Point(44, 4);
            dtpMes.Name = "dtpMes";
            dtpMes.ShowUpDown = true;
            dtpMes.Size = new Size(90, 23);
            dtpMes.TabIndex = 1;
            // 
            // btnGenerarResumen
            // 
            btnGenerarResumen.Location = new Point(140, 4);
            btnGenerarResumen.Name = "btnGenerarResumen";
            btnGenerarResumen.Size = new Size(110, 23);
            btnGenerarResumen.TabIndex = 2;
            btnGenerarResumen.Text = "Generar resumen";
            btnGenerarResumen.UseVisualStyleBackColor = true;
            btnGenerarResumen.Click += btnGenerarResumen_Click;
            // 
            // dtResumen
            // 
            dtResumen.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            dtResumen.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dtResumen.Location = new Point(6, 36);
            dtResumen.Name = "dtResumen";
            dtResumen.ReadOnly = true;
            dtResumen.Size = new Size(756, 300);
            dtResumen.TabIndex = 3;
            // 
            // lblTotalMes
            // 
            lblTotalMes.Anchor = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            lblTotalMes.Font = new Font("Segoe UI Semibold", 10F, FontStyle.Bold, GraphicsUnit.Point, 0);
            lblTotalMes.Location = new Point(6, 342);
            lblTotalMes.Name = "lblTotalMes";
            lblTotalMes.Size = new Size(756, 23);
            lblTotalMes.TabIndex = 4;
            lblTotalMes.Text = "Total del mes: $0,00";
            lblTotalMes.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // frmInformeVentas
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(800, 450);
            Controls.Add(tabInformes);
            Name = "frmInformeVentas";
            Text = "Informe de ventas";
            ((System.ComponentModel.ISupportInitialize)dataGridView1).EndInit();
            tabInformes.ResumeLayout(false);
            tabPage1.ResumeLayout(false);
            tabPage1.PerformLayout();
            tabPage2.ResumeLayout(false);
            tabPage2.PerformLayout();
            tabPage3.ResumeLayout(false);
            tabPage3.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)dtCajas).EndInit();
            ((System.ComponentModel.ISupportInitialize)dtResumen).EndInit();
            ResumeLayout(false);
        }

        #endregion

        private DataGridView dataGridView1;
        private Label label1;
        private Button btnEliminar;
        private Button btnSalir;
        private TabControl tabInformes;
        private TabPage tabPage1;
        private Button btnCerrarCaja;
        private TabPage tabPage2;
        private CheckBox chkTodos;
        private DataGridView dtCajas;
        private Label label2;
        private Button btnCopiarTicket;
        private Button btnImprimirTicket;
        private Button btnCerrarCajaAnterior;
        private TabPage tabPage3;
        private Label label3;
        private DateTimePicker dtpMes;
        private Button btnGenerarResumen;
        private DataGridView dtResumen;
        private Label lblTotalMes;
    }
}