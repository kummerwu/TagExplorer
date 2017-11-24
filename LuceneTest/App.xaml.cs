using TagExplorer.UriMgr;
using System;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Windows;
using TagExplorer.Utils;

using System.IO;

namespace TagExplorer
{
    /// <summary>
    /// App.xaml 的交互逻辑
    /// </summary>
    public partial class App : Application
    {
        /// 该函数设置由不同线程产生的窗口的显示状态
        /// </summary>
        /// <param name="hWnd">窗口句柄</param>
        /// <param name="cmdShow">指定窗口如何显示。查看允许值列表，请查阅ShowWlndow函数的说明部分</param>
        /// <returns>如果函数原来可见，返回值为非零；如果函数原来被隐藏，返回值为零</returns>
        [DllImport("User32.dll")]
        private static extern bool ShowWindowAsync(IntPtr hWnd, int cmdShow);
        /// <summary>
        ///  该函数将创建指定窗口的线程设置到前台，并且激活该窗口。键盘输入转向该窗口，并为用户改各种可视的记号。
        ///  系统给创建前台窗口的线程分配的权限稍高于其他线程。 
        /// </summary>
        /// <param name="hWnd">将被激活并被调入前台的窗口句柄</param>
        /// <returns>如果窗口设入了前台，返回值为非零；如果窗口未被设入前台，返回值为零</returns>
        [DllImport("User32.dll")]
        private static extern bool SetForegroundWindow(IntPtr hWnd);
        public App()
        {
            DispatcherUnhandledException += App_DispatcherUnhandledException;
            Current.DispatcherUnhandledException += Current_DispatcherUnhandledException;
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
            Startup += App_Startup;
            Exit += App_Exit;
        }

        private void App_Exit(object sender, ExitEventArgs e)
        {
            IDisposableFactory.DisposeAll();
            Logger.E("App Exit:"+e.ApplicationExitCode);

        }

        private void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            IDisposableFactory.DisposeAll();
            Logger.E(e.ExceptionObject.ToString());
            MessageBox.Show("发生内部异常，程序即将关闭，如需分析，请查看相关日志！");

        }

        private void Current_DispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
        {
            IDisposableFactory.DisposeAll();
            Logger.E(e.Exception);
            //e.Handled = true;
            //MessageBox.Show("发生内部异常，程序即将关闭，如需分析，请查看相关日志！");

        }
        private void App_DispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
        {
            IDisposableFactory.DisposeAll();
            Logger.E(e.Exception);
            //e.Handled = true;
            //MessageBox.Show("发生内部异常，程序即将关闭，如需分析，请查看相关日志！");
            //MessageBox.Show(e.Exception.Message+"\r\n===============\r\n"+e.Exception.StackTrace);

        }
        private const int SW_SHOWNOMAL = 1;
        private static void BringToForeground(Process instance)
        {
            //ShowWindowAsync(instance.MainWindowHandle, SW_SHOWNOMAL);//显示
            SetForegroundWindow(instance.MainWindowHandle);//当到最前端
        }
        private static Process GetRuningInstance()
        {
            Process currentProcess = Process.GetCurrentProcess();
            Process[] Processes = Process.GetProcessesByName(currentProcess.ProcessName);
            foreach (Process process in Processes)
            {
                if (process.Id != currentProcess.Id)
                {
                    if (Assembly.GetExecutingAssembly().Location.Replace("/", "\\") == currentProcess.MainModule.FileName)
                    {
                        return process;
                    }
                }
            }
            return null;
        }

        private void App_Startup(object sender, StartupEventArgs e)
        {
        retry:
            if (CfgPath.RootPath == null)
            {
                System.Windows.Forms.FolderBrowserDialog fd = new System.Windows.Forms.FolderBrowserDialog();
                fd.ShowNewFolderButton = true;
                fd.ShowDialog();
                if (Directory.Exists(fd.SelectedPath))
                {
                    CfgPath.RootPath = fd.SelectedPath;
                }
                else
                {
                    if (System.Windows.MessageBox.Show("请选择一个文档存储的根目录", "首次运行设置", MessageBoxButton.OKCancel, MessageBoxImage.Question) == MessageBoxResult.OK)
                    {
                        goto retry;
                    }
                    else
                    {
                        System.Windows.Application.Current.Shutdown();
                    }
                }
            }


            Process process = GetRuningInstance();
            if(process!=null)
            {
                BringToForeground(process);
                Environment.Exit(0);
            }
        }
        


    }
}
