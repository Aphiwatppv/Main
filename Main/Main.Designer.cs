namespace Main
{
    partial class Main
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
            dgvServerConfig = new DataGridView();
            label1 = new Label();
            label2 = new Label();
            label3 = new Label();
            comboBoxFilter = new ComboBox();
            labelStatus = new Label();
            buttonAdd = new Button();
            buttonDelete = new Button();
            buttonEdit = new Button();
            buttonExportCsv = new Button();
            ((System.ComponentModel.ISupportInitialize)dgvServerConfig).BeginInit();
            SuspendLayout();
            // 
            // dgvServerConfig
            // 
            dgvServerConfig.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dgvServerConfig.Location = new Point(33, 64);
            dgvServerConfig.Name = "dgvServerConfig";
            dgvServerConfig.Size = new Size(978, 490);
            dgvServerConfig.TabIndex = 0;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new Point(33, 29);
            label1.Name = "label1";
            label1.Size = new Size(110, 15);
            label1.TabIndex = 1;
            label1.Text = "Server Informations";
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Location = new Point(358, 29);
            label2.Name = "label2";
            label2.Size = new Size(33, 15);
            label2.TabIndex = 2;
            label2.Text = "Filter";
            // 
            // label3
            // 
            label3.AutoSize = true;
            label3.Location = new Point(1030, 64);
            label3.Name = "label3";
            label3.Size = new Size(49, 15);
            label3.TabIndex = 3;
            label3.Text = "Services";
            // 
            // comboBoxFilter
            // 
            comboBoxFilter.FormattingEnabled = true;
            comboBoxFilter.Location = new Point(406, 26);
            comboBoxFilter.Name = "comboBoxFilter";
            comboBoxFilter.Size = new Size(121, 23);
            comboBoxFilter.TabIndex = 4;
            // 
            // labelStatus
            // 
            labelStatus.AutoSize = true;
            labelStatus.Location = new Point(33, 564);
            labelStatus.Name = "labelStatus";
            labelStatus.Size = new Size(64, 15);
            labelStatus.TabIndex = 5;
            labelStatus.Text = "labelStatus";
            // 
            // buttonAdd
            // 
            buttonAdd.Location = new Point(1039, 115);
            buttonAdd.Name = "buttonAdd";
            buttonAdd.Size = new Size(186, 35);
            buttonAdd.TabIndex = 6;
            buttonAdd.Text = "Add New Server";
            buttonAdd.UseVisualStyleBackColor = true;
            buttonAdd.Click += buttonAdd_Click;
            // 
            // buttonDelete
            // 
            buttonDelete.Location = new Point(1039, 177);
            buttonDelete.Name = "buttonDelete";
            buttonDelete.Size = new Size(186, 35);
            buttonDelete.TabIndex = 7;
            buttonDelete.Text = "Delete Server";
            buttonDelete.UseVisualStyleBackColor = true;
            buttonDelete.Click += buttonDelete_Click;
            // 
            // buttonEdit
            // 
            buttonEdit.Location = new Point(1039, 241);
            buttonEdit.Name = "buttonEdit";
            buttonEdit.Size = new Size(186, 35);
            buttonEdit.TabIndex = 8;
            buttonEdit.Text = "Edit Server";
            buttonEdit.UseVisualStyleBackColor = true;
            buttonEdit.Click += buttonEdit_Click;
            // 
            // buttonExportCsv
            // 
            buttonExportCsv.Location = new Point(1039, 302);
            buttonExportCsv.Name = "buttonExportCsv";
            buttonExportCsv.Size = new Size(186, 35);
            buttonExportCsv.TabIndex = 9;
            buttonExportCsv.Text = "Export Csv";
            buttonExportCsv.UseVisualStyleBackColor = true;
            buttonExportCsv.Click += buttonExportCsv_Click;
            // 
            // Main
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = Color.White;
            ClientSize = new Size(1291, 631);
            Controls.Add(buttonExportCsv);
            Controls.Add(buttonEdit);
            Controls.Add(buttonDelete);
            Controls.Add(buttonAdd);
            Controls.Add(labelStatus);
            Controls.Add(comboBoxFilter);
            Controls.Add(label3);
            Controls.Add(label2);
            Controls.Add(label1);
            Controls.Add(dgvServerConfig);
            Name = "Main";
            Text = "Form1";
            Load += Main_Load;
            ((System.ComponentModel.ISupportInitialize)dgvServerConfig).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private DataGridView dgvServerConfig;
        private Label label1;
        private Label label2;
        private Label label3;
        private ComboBox comboBoxFilter;
        private Label labelStatus;
        private Button buttonAdd;
        private Button buttonDelete;
        private Button buttonEdit;
        private Button buttonExportCsv;
    }
}
