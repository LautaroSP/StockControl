
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
            btnCerrarCaja = new Button();
            tabPage2 = new TabPage();
            chkTodos = new CheckBox();
            dtCajas = new DataGridView();
            label2 = new Label();
            ((System.ComponentModel.ISupportInitialize)dataGridView1).BeginInit();
            tabInformes.SuspendLayout();
            tabPage1.SuspendLayout();
            tabPage2.SuspendLayout();
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
            btnCerrarCajaAnterior.Location = new Point(457, 369);
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
            btnCopiarTicket.Location = new Point(586, 369);
            btnCopiarTicket.Name = "btnCopiarTicket";
            btnCopiarTicket.Size = new Size(95, 23);
            btnCopiarTicket.TabIndex = 5;
            btnCopiarTicket.Text = "Copiar ticket";
            btnCopiarTicket.UseVisualStyleBackColor = true;
            btnCopiarTicket.Click += btnCopiarTicket_Click;
            // 
            // btnCerrarCaja
            // 
            btnCerrarCaja.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            btnCerrarCaja.Location = new Point(687, 369);
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
            ((System.ComponentModel.ISupportInitialize)dtCajas).EndInit();
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
        private Button btnCerrarCajaAnterior;
    }
}