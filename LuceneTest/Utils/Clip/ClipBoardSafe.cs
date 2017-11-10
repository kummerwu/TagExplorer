using System;
using System.Windows;
using System.Diagnostics;               // For Process class
using System.Runtime.InteropServices;   // For DllImport's
namespace TagExplorer.Utils
{
    //恶心的WPF剪贴板和Z，总是导致剪贴板操作失败
    class ClipBoardSafe
    {
        //为了解决WPF剪贴板总是设置失败的问题，增加了重试保护和多种设置方式
        public static bool SetText(string txt)
        {
            startTime = DateTime.Now;
            if (SetText(txt, 1)) return true;           //尝试多种方式，每种方式尝试1次
            else if (SetText(txt, 3)) return true;      //尝试多种方式，每一种方式尝试3次
            else if (SetText(txt, 5)) return true;      //尝试多种方式，每一种方式再次尝试5次
            else
            {
                ReportErr(txt);                         //失败了，记录失败原因返回
                return false;
            }
        }


        [DllImport("user32.dll")]
        static extern IntPtr GetOpenClipboardWindow();

        [DllImport("user32.dll", SetLastError = true)]
        static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint lpdwProcessId);

        static DateTime startTime;
        
        private static bool SetText(string txt,int retryCnt)
        {
            if (SetTextWinFormClip(txt, retryCnt)) return true;     //先尝试Window.Forms的SetText
            else if (SetTextWPFClip(txt, retryCnt)) return true;    //在尝试WPF中Window中的SetText
            else if (SetTextByData(txt, retryCnt)) return true;     //再尝试SetData方法
            else return false;
        }
        private delegate void SetClipTxt(string txt);
        private static bool SetTextTemplateFunc(string txt,int retryCnt,SetClipTxt func)
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
            return SetTextTemplateFunc(txt, retryCnt, System.Windows.Forms.Clipboard.SetText);
        }
        //使用WPF的剪贴板
        private static bool SetTextWPFClip(string txt, int retryCnt)
        {
            return SetTextTemplateFunc(txt, retryCnt, Clipboard.SetText);
        }
        //使用WPF的剪贴板 SetDataObject
        private static bool SetTextByData(string txt,int retryCnt)
        {
            return SetTextTemplateFunc(txt, retryCnt, Clipboard.SetDataObject);
        }

        //获得剪贴板访问错误信息：当前剪贴板被哪个进程占用？该进程窗口标题是什么？
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

        //报告错误信息
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
