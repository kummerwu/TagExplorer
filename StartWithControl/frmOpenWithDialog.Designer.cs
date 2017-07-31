namespace ProcessStart
{
    partial class frmOpenWithDialog
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
            this.ucOpenWith1 = new ProcessStart.ucOpenWith();
            this.SuspendLayout();
            // 
            // ucOpenWith1
            // 
            this.ucOpenWith1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.ucOpenWith1.Location = new System.Drawing.Point(0, 0);
            this.ucOpenWith1.Name = "ucOpenWith1";
            this.ucOpenWith1.Size = new System.Drawing.Size(323, 297);
            this.ucOpenWith1.TabIndex = 0;
            this.ucOpenWith1.ApplicationProcessStart += new System.EventHandler(this.ucOpenWith1_ApplicationProcessStart);
            // 
            // frmOpenWithDialog
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.White;
            this.ClientSize = new System.Drawing.Size(323, 297);
            this.Controls.Add(this.ucOpenWith1);
            this.Name = "frmOpenWithDialog";
            this.ShowIcon = false;
            this.Text = "Open with";
            this.ResumeLayout(false);

        }

        #endregion

        private ucOpenWith ucOpenWith1;
    }
}