using System;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Text.RegularExpressions;

namespace TagExplorer.Utils
{
    public class PathHelper
    {
        #region 公有方法
        public static string[] MapFilesToTagDir(string [] srcs,string currentTag)
        {
            string[] dsts = new string[srcs.Length];
            for (int i = 0; i < srcs.Length; i++)
            {
                
                string s = srcs[i];
                if (PathHelper.IsValidWebLink(s))
                {
                    dsts[i] = s;
                }
                else
                {
                    System.Diagnostics.Debug.Assert(PathHelper.IsValidFS(s));
                    string n = Path.GetFileName(s);
                    string d = Path.Combine(CfgPath.GetDirByTag(currentTag), n);
                    dsts[i] = d;
                }
                
            }
            return dsts;
        }

        
        public static void RunCmd(string str)
        {
            Process p = new Process();
            p.StartInfo.FileName = "cmd.exe";
            p.StartInfo.UseShellExecute = false;    //是否使用操作系统shell启动
            p.StartInfo.RedirectStandardInput = true;//接受来自调用程序的输入信息
            p.StartInfo.RedirectStandardOutput = true;//由调用程序获取输出信息
            p.StartInfo.RedirectStandardError = true;//重定向标准错误输出
            p.StartInfo.CreateNoWindow = true;//不显示程序窗口
            p.Start();//启动程序

            //向cmd窗口发送输入信息
            p.StandardInput.WriteLine(str + "&exit");

            p.StandardInput.AutoFlush = true;
            //p.StandardInput.WriteLine("exit");
            //向标准输入写入要执行的命令。这里使用&是批处理命令的符号，表示前面一个命令不管是否执行成功都执行后面(exit)命令，如果不执行exit命令，后面调用ReadToEnd()方法会假死
            //同类的符号还有&&和||前者表示必须前一个命令执行成功才会执行后面的命令，后者表示必须前一个命令执行失败才会执行后面的命令



            //获取cmd窗口的输出信息
            //string output = p.StandardOutput.ReadToEnd();//Bug：有时候这个地方会挂死
            while(p.StandardOutput.Peek()>-1)
            {
                p.StandardOutput.ReadLine();
            }

            //StreamReader reader = p.StandardOutput;
            //string line=reader.ReadLine();
            //while (!reader.EndOfStream)
            //{
            //    str += line + "  ";
            //    line = reader.ReadLine();
            //}

            p.WaitForExit();//等待程序执行完退出进程
            p.Close();
        }
        public static void LinkDir(string tagVDir, string tagDir)
        {
            if (!Directory.Exists(tagVDir))
            {
                string cmd = string.Format(@"mklink /j ""{0}"" ""{1}""", tagVDir, tagDir);
                //System.Diagnostics.Process.Start("cmd.exe", cmd);
                RunCmd(cmd);
            }
            else
            {
                
            }
        }

        

        
        
        public static bool IsValidUri(string txt)
        {
            if (txt == null) return false;
            return IsValidFS(txt) || IsValidWebLink(txt);
        }
        public static bool IsValidFS(string txt)
        {
            if (txt == null) return false;
            return File.Exists(txt) || Directory.Exists(txt);
        }
        public static bool IsValidFile(string txt)
        {
            if (txt == null) return false;
            return File.Exists(txt);
        }
        public static bool IsValidDir(string txt)
        {
            if (txt == null) return false;
            return Directory.Exists(txt);
        }
        public static bool IsValidWebLink(string txt)
        {
            if (txt == null) return false;
            return (txt.StartsWith("http://", StringComparison.CurrentCultureIgnoreCase) ||
                    txt.StartsWith("https://", StringComparison.CurrentCultureIgnoreCase) ||
                    txt.StartsWith(@"onenote:///",StringComparison.CurrentCultureIgnoreCase));
        }

        public static bool IsValidHttp(string txt)
        {
            if (txt == null) return false;
            return (txt.StartsWith("http://", StringComparison.CurrentCultureIgnoreCase) ||
                    txt.StartsWith("https://", StringComparison.CurrentCultureIgnoreCase) );
        }

        

        #endregion

        
    }
}
