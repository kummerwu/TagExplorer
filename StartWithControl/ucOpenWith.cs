using System;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace ProcessStart
{
    public partial class ucOpenWith : UserControl
    {
        /// <summary>
        /// Fired if the Application Process is started
        /// </summary>
        public event EventHandler ApplicationProcessStart;

        private ProcessStart.OpenWith openW;
        private DataTable dtAppslist;
        private string mFullfilename;
        private Process mPrc;
        private bool mShowToolTip = false;

        /// <summary>
        /// Show the Application Company as Tooltip
        /// </summary>
        public bool ShowGridToolTip
        {
            get { return mShowToolTip; }
            set { mShowToolTip = value; }
        }

        /// <summary>
        /// Diagnositcs.Process for the Applcation 
        /// </summary>
        public Process ApplicationProcess
        {
            get { return mPrc; }
        }

        public ucOpenWith()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Load Application Data for the File (Extension)
        /// </summary>
        /// <param name="strFullfilename">Fullfilename (Path and Filename)</param>
        public new void Load(string strFullfilename)
        {
            if (!string.IsNullOrEmpty(strFullfilename))
            {
                mFullfilename = strFullfilename;
                InitOpenWith(Path.GetExtension(strFullfilename));
            } else
            {
                throw new ArgumentNullException("strExtension");
            }
        }

        private void InitOpenWith(string strExtension)
        {
            try
            {
                dtAppslist = new DataTable("Application");
                dtAppslist.Columns.Add("Icon", typeof(Icon));
                dtAppslist.Columns.Add("Name");
                dtAppslist.Columns.Add("Path");
                dtAppslist.Columns.Add("Company");
                dtAppslist.Columns.Add("regname");
                dtAppslist.Columns.Add("StartPath");
                dtAppslist.Columns.Add("StartVerb");
                dtAppslist.Columns.Add("StartPreParam");

                openW = new ProcessStart.OpenWith(strExtension);

                if (openW != null)
                {
                    foreach (ProcessStart.cApplicationData data in openW.Applicationlist.Values)
                    {
                        if (data.Havefilelinks)
                        {
                            DataRow newRow = dtAppslist.NewRow();
                            newRow["Icon"] = data.ApplicationIcon;
                            newRow["Name"] = data.Productname;
                            newRow["Path"] = data.Filenamelink;
                            newRow["Company"] = data.Company;
                            newRow["regname"] = data.RegistryName;

                            if (data.OpenFilenameLink != null)
                            {
                                newRow["StartPath"] = data.OpenFilenameLink.Filelocation;
                                newRow["StartVerb"] = data.OpenFilenameLink.Params;
                                newRow["StartPreParam"] = data.OpenFilenameLink.PreParams;
                            } else if (data.EditFilenameLink != null)
                            {
                                newRow["StartPath"] = data.EditFilenameLink.Filelocation;
                                newRow["StartVerb"] = data.EditFilenameLink.Params;
                                newRow["StartPreParam"] = data.EditFilenameLink.PreParams;
                            } else
                            {
                                newRow["StartPath"] = data.Filenamelink;
                                newRow["StartVerb"] = "";
                                newRow["StartPreParam"] = "";
                            }

                            dtAppslist.Rows.Add(newRow);                          
                        }
                    }
                } else
                {
                    //No Registry Entries for this Extension or Error
                }             
            }
            catch (Exception ex)
            {
                Log.Livelog.Log("Error in 'InitOpenWith', Ex:'" + ex.Message.ToString() + "'");
            }
            finally
            {
                dtAppslist.DefaultView.Sort = "Name";
                dgvAppslist.DataSource = dtAppslist;
                FormatGrid();
            }
        }

        public virtual void FormatGrid()
        {
            try
            {
                dgvAppslist.Columns["Name"].Width = 250;
                dgvAppslist.Columns["Path"].Visible = false;
                dgvAppslist.Columns["Company"].Visible = false;
                dgvAppslist.Columns["regname"].Visible = false;
                dgvAppslist.Columns["StartPath"].Visible = false;
                dgvAppslist.Columns["StartVerb"].Visible = false;
                dgvAppslist.Columns["StartPreParam"].Visible = false;
                dgvAppslist.Columns["Icon"].Width = 50;
            }
            catch (Exception ex)
            {
                Log.Livelog.Log("Error in 'FormatGrid', Ex:'" + ex.Message.ToString() + "'");
            }
        }

        /// <summary>
        /// Start a the Application with the File
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void dgvAppslist_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            string strStartPath = "";
            string strStartVerb = "";
            string strStartPreParam = "";
            try
            {
                if (e.RowIndex >= 0 && e.RowIndex < dgvAppslist.Rows.Count)
                {
                    strStartPath = dgvAppslist.Rows[e.RowIndex].Cells["StartPath"].Value.ToString();
                    strStartVerb = dgvAppslist.Rows[e.RowIndex].Cells["StartVerb"].Value.ToString();
                    strStartPreParam = dgvAppslist.Rows[e.RowIndex].Cells["StartPreParam"].Value.ToString();
                    mPrc = ProcessStart.ProcessOpen.Start(strStartPreParam, strStartPath, strStartVerb, mFullfilename);

                    ApplicationProcessStart(this, new EventArgs());
                }
            } catch (Exception ex)
            {
                Log.Livelog.Log(string.Format("Error in 'dgvAppslist_CellClick', ApplicationPath:'{0}', ApplicationVerb:'{1}', ApplicationPreParams:'{2}', Ex: '", strStartPath, strStartVerb, strStartPreParam) + ex.Message.ToString() + "'");
            }
        }

        private void dgvAppslist_CellMouseEnter(object sender, DataGridViewCellEventArgs e)
        {
            if (dgvAppslist != null && dgvAppslist.Rows.Count > e.RowIndex)
            {
                dgvAppslist.Rows[e.RowIndex].Selected = true;
                dgvAppslist.Rows[e.RowIndex].DefaultCellStyle.SelectionBackColor = Color.GhostWhite;
                if (mShowToolTip)
                {
                    dgvAppslist.ShowCellToolTips = true;
                    dgvAppslist.Rows[e.RowIndex].Cells["Name"].ToolTipText = dgvAppslist.Rows[e.RowIndex].Cells["Company"].Value.ToString();
                }
            }
        }

        private void dgvAppslist_CellMouseLeave(object sender, DataGridViewCellEventArgs e)
        {
            if (dgvAppslist != null && dgvAppslist.Rows.Count > e.RowIndex)
            {
                dgvAppslist.Rows[e.RowIndex].Selected = false;
                dgvAppslist.Rows[e.RowIndex].DefaultCellStyle.SelectionBackColor = Color.White;
            }
        }

    }
}
