namespace StockControl
{
    partial class frmBuscarGrupo
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
            txtBuscar = new TextBox();
            btnSalir = new Button();
            btnNuevoGrupo = new Button();
            ((System.ComponentModel.ISupportInitialize)dataGridView1).BeginInit();
            SuspendLayout();
            // 
            // dataGridView1
            // 
            dataGridView1.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dataGridView1.Location = new Point(12, 47);
            dataGridView1.Name = "dataGridView1";
            dataGridView1.Size = new Size(340, 366);
            dataGridView1.TabIndex = 0;
            // 
            // txtBuscar
            // 
            txtBuscar.Location = new Point(12, 12);
            txtBuscar.Name = "txtBuscar";
            txtBuscar.Size = new Size(192, 23);
            txtBuscar.TabIndex = 1;
            // 
            // btnSalir
            // 
            btnSalir.Location = new Point(277, 419);
            btnSalir.Name = "btnSalir";
            btnSalir.Size = new Size(75, 23);
            btnSalir.TabIndex = 2;
            btnSalir.Text = "Salir";
            btnSalir.UseVisualStyleBackColor = true;
            btnSalir.Click += btnSalir_Click;
            // 
            // btnNuevoGrupo
            // 
            btnNuevoGrupo.Location = new Point(12, 419);
            btnNuevoGrupo.Name = "btnNuevoGrupo";
            btnNuevoGrupo.Size = new Size(91, 23);
            btnNuevoGrupo.TabIndex = 3;
            btnNuevoGrupo.Text = "Nuevo Grupo";
            btnNuevoGrupo.UseVisualStyleBackColor = true;
            btnNuevoGrupo.Click += btnNuevoGrupo_Click;
            // 
            // frmBuscarGrupo
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(364, 450);
            Controls.Add(btnNuevoGrupo);
            Controls.Add(btnSalir);
            Controls.Add(txtBuscar);
            Controls.Add(dataGridView1);
            MaximizeBox = false;
            Name = "frmBuscarGrupo";
            Text = "Buscar Grupo";
            ((System.ComponentModel.ISupportInitialize)dataGridView1).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private DataGridView dataGridView1;
        private TextBox txtBuscar;
        private Button btnSalir;
        private Button btnNuevoGrupo;
    }
}