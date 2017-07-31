namespace ProcessStart
{
    partial class ucOpenWith
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

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.dgvAppslist = new System.Windows.Forms.DataGridView();
            ((System.ComponentModel.ISupportInitialize)(this.dgvAppslist)).BeginInit();
            this.SuspendLayout();
            // 
            // dgvAppslist
            // 
            this.dgvAppslist.AllowUserToAddRows = false;
            this.dgvAppslist.AllowUserToDeleteRows = false;
            this.dgvAppslist.AllowUserToResizeRows = false;
            this.dgvAppslist.BackgroundColor = System.Drawing.Color.White;
            this.dgvAppslist.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvAppslist.ColumnHeadersVisible = false;
            this.dgvAppslist.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dgvAppslist.GridColor = System.Drawing.Color.White;
            this.dgvAppslist.Location = new System.Drawing.Point(0, 0);
            this.dgvAppslist.MultiSelect = false;
            this.dgvAppslist.Name = "dgvAppslist";
            this.dgvAppslist.ReadOnly = true;
            this.dgvAppslist.RowHeadersVisible = false;
            this.dgvAppslist.RowTemplate.DefaultCellStyle.SelectionBackColor = System.Drawing.Color.White;
            this.dgvAppslist.RowTemplate.DefaultCellStyle.SelectionForeColor = System.Drawing.Color.Black;
            this.dgvAppslist.RowTemplate.Height = 36;
            this.dgvAppslist.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.dgvAppslist.ShowCellErrors = false;
            this.dgvAppslist.ShowCellToolTips = false;
            this.dgvAppslist.ShowEditingIcon = false;
            this.dgvAppslist.ShowRowErrors = false;
            this.dgvAppslist.Size = new System.Drawing.Size(343, 294);
            this.dgvAppslist.TabIndex = 0;
            this.dgvAppslist.CellClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.dgvAppslist_CellClick);
            this.dgvAppslist.CellMouseEnter += new System.Windows.Forms.DataGridViewCellEventHandler(this.dgvAppslist_CellMouseEnter);
            this.dgvAppslist.CellMouseLeave += new System.Windows.Forms.DataGridViewCellEventHandler(this.dgvAppslist_CellMouseLeave);
            // 
            // ucOpenWith
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.dgvAppslist);
            this.Name = "ucOpenWith";
            this.Size = new System.Drawing.Size(343, 294);
            ((System.ComponentModel.ISupportInitialize)(this.dgvAppslist)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.DataGridView dgvAppslist;
    }
}
