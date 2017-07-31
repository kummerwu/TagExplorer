using System;
using System.Diagnostics;
using System.Windows.Forms;

namespace ProcessStart
{
    public class ProcessOpen
    {

        /// <summary>
        /// Shows OpenWith Dialog for the File
        /// </summary>
        /// <param name="strFullFilename">Fullfilename (Path and Filename)</param>
        /// <returns>DialogResult.OK if Application selected</returns>
        public static DialogResult ShowDialog(string strFullFilename)
        {
            return ShowDialog(strFullFilename, "");
        }

        /// <summary>
        /// Shows OpenWith Dialog for the File
        /// </summary>
        /// <param name="strFullFilename">Fullfilename (Path and Filename)</param>
        /// <param name="strFormularTitle">Formular Text</param>
        /// <returns>DialogResult.OK if Application selected</returns>
        public static DialogResult ShowDialog(string strFullFilename, string strFormularTitle)
        {
            return ShowDialog(strFullFilename, strFormularTitle, false);
        }

        /// <summary>
        /// Shows OpenWith Dialog for the File
        /// </summary>
        /// <param name="strFullFilename">Fullfilename (Path and Filename)</param>
        /// <param name="strFormularTitle">Formular Text</param>
        /// <param name="bShowgridToolTip">Shows the Application Companyname as Grid Tooltip (default=False)</param>
        /// <returns>DialogResult.OK if Application selected</returns>
        public static DialogResult ShowDialog(string strFullFilename, string strFormularTitle, bool bShowgridToolTip)
        {
            if (!string.IsNullOrEmpty(strFullFilename))
            {
                DialogResult dlgReturn = DialogResult.No;

                using (frmOpenWithDialog frm = new frmOpenWithDialog())
                {
                    frm.Load(strFullFilename);

                    if (!string.IsNullOrEmpty(strFormularTitle))
                    {
                        frm.Text = strFormularTitle;
                    }
                    if (bShowgridToolTip)
                    {
                        frm.ShowGridToolTip = bShowgridToolTip;
                    }

                    dlgReturn = frm.ShowDialog();
                }

                return dlgReturn;
            }
            else
            {
                throw new ArgumentNullException("strFullFilename");
            }
        }

        /// <summary>
        /// Shows OpenWith Dialog for the File
        /// </summary>
        /// <param name="strFullFilename">Fullfilename (Path and Filename)</param>
        /// <param name="prc">Diagnositcs.Process for the Applcation</param>
        /// <param name="strFormularTitle">Formular Text</param>
        /// <param name="bShowgridToolTip">Shows the Application Companyname as Grid Tooltip (default=False)</param>
        /// <returns>DialogResult.OK if Application selected</returns>
        public static DialogResult ShowDialog(string strFullFilename, out System.Diagnostics.Process prc, string strFormularTitle, bool bShowgridToolTip)
        {
            if (!string.IsNullOrEmpty(strFullFilename))
            {
                DialogResult dlgReturn = DialogResult.No;

                using (frmOpenWithDialog frm = new frmOpenWithDialog())
                {
                    frm.Load(strFullFilename);

                    if (!string.IsNullOrEmpty(strFormularTitle))
                    {
                        frm.Text = strFormularTitle;
                    }
                    if (bShowgridToolTip)
                    {
                        frm.ShowGridToolTip = bShowgridToolTip;
                    }

                    dlgReturn = frm.ShowDialog();

                    prc = frm.ApplicationProcess;
                }

                return dlgReturn;
            }
            else
            {
                throw new ArgumentNullException("strFullFilename");
            }
        }

        /// <summary>
        /// Shows OpenWith Dialog for the File
        /// </summary>
        /// <param name="strFullFilename">Fullfilename (Path and Filename)</param>
        /// <param name="prc">Diagnositcs.Process for the Applcation</param>
        /// <param name="strFormularTitle">Formular Text</param>
        /// <returns>DialogResult.OK if Application selected</returns>
        public static DialogResult ShowDialog(string strFullFilename, out System.Diagnostics.Process prc, string strFormularTitle)
        {
            return ShowDialog(strFullFilename, out prc, strFullFilename, false);
        }

        /// <summary>
        /// Shows OpenWith Dialog for the File
        /// </summary>
        /// <param name="strFullFilename">Fullfilename (Path and Filename)</param>
        /// <param name="prc">Diagnositcs.Process for the Applcation</param>
        /// <returns>DialogResult.OK if Application selected</returns>
        public static DialogResult ShowDialog(string strFullFilename, out System.Diagnostics.Process prc)
        {
            return ShowDialog(strFullFilename,out prc, "");
        }

        /// <summary>
        /// Start an Application from Filelink with File 
        /// </summary>
        /// <param name="fl">Filelink for the Application</param>
        /// <param name="File">File to open</param>
        /// <returns>the Diagnostics.Process for the Application</returns>
        public static Process Start(ProcessStart.Filelink fl, string File)
        {
            if (fl != null)
            {
                return ProcessOpen.Start(fl.PreParams, fl.Filelocation, fl.Params, File);
            } else
            {
                throw new ArgumentNullException("fl", "ProcessStart.Filelink can't be Null when starting a Process");
            }
        }

        /// <summary>
        /// Start an Application from the Parameter with a File
        /// </summary>
        /// <param name="PreParam">Preparams for the Application (rundll32)</param>
        /// <param name="Path">Application Path</param>
        /// <param name="Params">Application parameter</param>
        /// <param name="File">File to open</param>
        /// <returns>the Diagnostics.Process for the Application</returns>
        public static Process Start(string PreParam, string Path, string Params, string File)
        {
            string strCommandPromt = "";
            bool bHavePreParams = false;

            //needed for .dll (rundll32)
            if (!string.IsNullOrEmpty(PreParam))
            {
                strCommandPromt =  PreParam ;
                bHavePreParams = true;
            } else
            {
                strCommandPromt = Path;
            }

            if (!string.IsNullOrEmpty(Params))
            {
                if (Params.Contains("%1"))
                {
                    if (bHavePreParams)
                    {
                        //problem with dll with qoute by the target file path
                        Params = "  \"" + Path + "\"  " + " " + Params.Replace("%1", " " + File + " ");
                    }
                    else
                    {
                        Params = Params.Replace("%1", "  \"" + File + "\"  ");
                    }
                }
                //Ignore other Verbs at this moment
                if (Params.Contains("%"))
                {
                    //Replace % and Wildcard with Empty
                    Params = System.Text.RegularExpressions.Regex.Replace(Params, @"%\d+", "");
                }
            } else
            {
                //add the file to the command
                Params = "  \"" + File + "\"";
            }

            ProcessStartInfo psi = new ProcessStartInfo(strCommandPromt);
            psi.Arguments = Params;

            return Process.Start(psi);
        }

    }
}
