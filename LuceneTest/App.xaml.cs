using System;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Windows;

namespace LuceneTest
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
            Startup += App_Startup;
        }
        private const int SW_SHOWNOMAL = 1;
        private static void HandleRunningInstance(Process instance)
        {
            //ShowWindowAsync(instance.MainWindowHandle, SW_SHOWNOMAL);//显示
            SetForegroundWindow(instance.MainWindowHandle);//当到最前端
        }
        private static Process RuningInstance()
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
            Process process = RuningInstance();
            if(process!=null)
            {
                //MessageBox.Show("应用程序已经在运行中。。。");

                HandleRunningInstance(process);
                Environment.Exit(0);
            }
        }

        private void App_DispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
        {
            MessageBox.Show(e.Exception.Message+"\r\n===============\r\n"+e.Exception.StackTrace);
        }

    }
}
