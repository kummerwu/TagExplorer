using System;
using System.Windows;
using System.Diagnostics;               // For Process class
using System.Runtime.InteropServices;   // For DllImport's
namespace TagExplorer.Utils
{
    class ClipBoardSafe
    {
        [DllImport("user32.dll")]
        static extern IntPtr GetOpenClipboardWindow();

        [DllImport("user32.dll", SetLastError = true)]
        static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint lpdwProcessId);

        public static bool SetText(string txt)
        {
            DateTime start = DateTime.Now;
            for(int i = 0;i<20;i++)
            {
                try
                {
                    Clipboard.SetText(txt);
                    return true;
                }
                catch(Exception e)
                {
                    if ((DateTime.Now - start).TotalSeconds > 5)
                    {
                        ReportErr(txt);
                        return false;
                    }
                    System.Threading.Thread.Sleep(10);
                }
            }
            ReportErr(txt);
            return false;
        }
        public static Process ProcessHoldingClipboard()
        {
            Process theProc = null;

            IntPtr hwnd = GetOpenClipboardWindow();

            if (hwnd != IntPtr.Zero)
            {
                uint processId;
                uint threadId = GetWindowThreadProcessId(hwnd, out processId);

                Process[] procs = Process.GetProcesses();
                foreach (Process proc in procs)
                {
                    IntPtr handle = proc.MainWindowHandle;

                    if (handle == hwnd)
                    {
                        theProc = proc;
                    }
                    else if (processId == proc.Id)
                    {
                        theProc = proc;
                    }
                }
            }

            return theProc;
        }
        public static void ReportErr(string txt)
        {
            Process p = ProcessHoldingClipboard();
            if (p == null)
            {
                MessageBox.Show("将内容拷贝到剪贴板失败" + txt, "设置剪贴板错误", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            else
            {
                MessageBox.Show("将内容拷贝到剪贴板失败" +p.MainModule.FileName+"\r\n"+p.MainWindowTitle+"\r\n"+ txt, "设置剪贴板错误", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
