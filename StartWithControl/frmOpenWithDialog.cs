using System;
using System.Windows.Forms;

namespace ProcessStart
{
    public partial class frmOpenWithDialog : Form
    {
        private System.Diagnostics.Process mPrc;

        /// <summary>
        /// Show the Application Company as Tooltip in the Grid
        /// </summary>
        public bool ShowGridToolTip
        {
            get 
            {
                if (ucOpenWith1 != null && !ucOpenWith1.IsDisposed)
                {
                    return ucOpenWith1.ShowGridToolTip;
                } else
                {
                    return false;
                }
            }
            set
            {
                if (ucOpenWith1 != null && !ucOpenWith1.IsDisposed)
                {
                    ucOpenWith1.ShowGridToolTip = value;
                }
            }
        }

        /// <summary>
        /// Diagnositcs.Process for the Applcation 
        /// </summary>
        public System.Diagnostics.Process ApplicationProcess
        {
            get { return mPrc; }
        }

        public frmOpenWithDialog()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Load Form with Application Data for the File (Extension)
        /// </summary>
        /// <param name="strFullfilename">Fullfilename (Path and Filename)</param>
        public new void Load(string strFullfilename)
        {
            if (!string.IsNullOrEmpty(strFullfilename))
            {
                ucOpenWith1.Load(strFullfilename);
            } else
            {
                throw new ArgumentNullException("strExtension");
            }
        }

        private void ucOpenWith1_ApplicationProcessStart(object sender, EventArgs e)
        {
            if (ucOpenWith1 != null)
            {
                mPrc = ucOpenWith1.ApplicationProcess;
                ucOpenWith1.Dispose();
                this.DialogResult = System.Windows.Forms.DialogResult.OK;
            }
        }
    }
}
