using System;
using System.Windows;
using System.Diagnostics;               // For Process class
using System.Runtime.InteropServices;   // For DllImport's
namespace TagExplorer.Utils
{
    //恶心的WPF剪贴板和Z，总是导致剪贴板操作失败
    class ClipBoardSafe
    {
        [DllImport("user32.dll")]
        static extern IntPtr GetOpenClipboardWindow();

        [DllImport("user32.dll", SetLastError = true)]
        static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint lpdwProcessId);

        static DateTime startTime;
        public static bool SetText(string txt)
        {
            startTime = DateTime.Now;
            if (SetText(txt, 1)) return true;
            else if (SetText(txt, 3)) return true;
            else if (SetText(txt, 5)) return true;
            else
            {
                ReportErr(txt);
                return false;
            }
        }
        private static bool SetText(string txt,int retryCnt)
        {
            if (SetTextWinFormClip(txt, retryCnt)) return true;
            else if (SetTextWPFClip(txt, retryCnt)) return true;
            else if (SetTextByData(txt, retryCnt)) return true;
            else return false;
        }
        private delegate void SetClipTxt(string txt);
        private static bool SetTxtTry(string txt,int retryCnt,SetClipTxt func)
        {
            if ((DateTime.Now - startTime).TotalSeconds > 5)
            {
                return false;
            }
            for (int i = 0; i < retryCnt; i++)
            {
                try
                {
                    func(txt);
                    return true;
                }
                catch (Exception e)
                {
                    Logger.E(e);
                    System.Threading.Thread.Sleep(10);
                }
            }
            
            return false;
        }
        //使用window.form的剪贴板
        private static bool SetTextWinFormClip(string txt, int retryCnt)
        {
            return SetTxtTry(txt, retryCnt, System.Windows.Forms.Clipboard.SetText);
        }
        //使用WPF的剪贴板
        private static bool SetTextWPFClip(string txt, int retryCnt)
        {
            return SetTxtTry(txt, retryCnt, Clipboard.SetText);
        }
        //使用WPF的剪贴板 SetDataObject
        private static bool SetTextByData(string txt,int retryCnt)
        {
            return SetTxtTry(txt, retryCnt, Clipboard.SetDataObject);
        }
        private static Process ProcessHoldingClipboard()
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
        private static void ReportErr(string txt)
        {
            Process p = ProcessHoldingClipboard();
            if (p == null)
            {
                MessageBox.Show("将内容拷贝到剪贴板失败" + txt, "设置剪贴板错误", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            else
            {
                MessageBox.Show("将内容拷贝到剪贴板失败：\r\n占用剪贴板程序为：" +p.MainModule.FileName+"\r\n窗口标题为："+p.MainWindowTitle+"\r\n附-操作数据："+ txt, "设置剪贴板错误", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
