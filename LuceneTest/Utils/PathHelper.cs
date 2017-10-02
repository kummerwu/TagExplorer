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
                string n = Path.GetFileName(s);
                string d = Path.Combine(PathHelper.GetDirByTag(currentTag), n);
                dsts[i] = d;
            }
            return dsts;
        }

        
        private static void RunCmd(string str)
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
            string output = p.StandardOutput.ReadToEnd();

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
        public static string GetVDirByTag(string tag)
        {
            return Path.Combine(CfgPath.VDir, tag);
        }
        //public static string ResPath { get { return Path.Combine(PathHelper.DocBaseDir, "Res"); } }
        public static string Res_HTTP_Path
        {
            get
            {
                return GetResFile("http.html");
            }
        }
        private static string GetResFile(string name)
        {
            string d = CfgPath.Res_Path;
            string f = Path.Combine(d, name);
            if (!File.Exists(f))
            {
                File.Create(f).Close();
            }
            return f;
        }
        
        public static string Res_UNKNOW_Path
        {
            get
            {
                return GetResFile("Unknown");
                
            }
        }
        public static bool IsValidUri(string txt)
        {
            if (txt == null) return false;
            return IsValidFS(txt) || IsValidHttps(txt);
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
        public static bool IsValidHttps(string txt)
        {
            if (txt == null) return false;
            return (txt.StartsWith("http://", StringComparison.CurrentCultureIgnoreCase) ||
                    txt.StartsWith("https://", StringComparison.CurrentCultureIgnoreCase) ||
                    txt.StartsWith(@"onenote:///",StringComparison.CurrentCultureIgnoreCase));
        }



        public static string GetTagByPath(string path)
        {
            if (!path.Contains(CfgPath.DocDir))
            {
                Logger.I("GetTagByPath Failed,File(Dir) !pathContains: {0}", path);
                return null; //根本不在doc目录中
            }
            //if (!File.Exists(path) && !Directory.Exists(path))
            //{
            //    Logger.I("GetTagByPath Failed,File(Dir) not Exist: {0}", path);
            //    return null;
            //}

            string[] dirs= path.Substring(CfgPath.DocDir.Length).Split(
                new char[] { Path.DirectorySeparatorChar }, 
                System.StringSplitOptions.RemoveEmptyEntries);
            if (dirs.Length != 2)
            {
                Logger.I("GetTagByPath Failed,File(Dir) dirs!=2: {0}", path);
                return null;
            }
            else return dirs[0];
        }
        public static string GetFileByTag(string tag, string postfix)
        {
            return System.IO.Path.Combine(GetDirByTag(tag), tag + "." + postfix);
        }
        public static string GetDirByTag(string tag)
        {
            string dir = System.IO.Path.Combine(CfgPath.DocDir, tag);
            if (!Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
            }
            return dir;
        }

        public static bool NeedSkip(string uri)
        {
            string name = Path.GetFileName(uri);
            bool canAccess = CheckAccess(uri);
            
            if (!canAccess)
            {
                Logger.I("file exist,but can't access!：{0}", uri);
            }
            return NeedSkipByUri(uri) || !canAccess;
        }
        public static bool NeedSkipByUri(string uri)
        {
            string name = Path.GetFileName(uri);
            bool ret = Regex.IsMatch(name, @"(_files$)|(^~)|(.tmp$)", RegexOptions.IgnoreCase);
            if(ret)
            {
                Logger.I("name match reg: {0}", uri);
            }
            return ret;
        }
        public static bool CheckAccess(string uri)
        {
            return true;//不再检查是否可写，因为我已经不再使用guid来标识文件了。

            //bool canAccess = true;
            //if (File.Exists(uri)) //如果是文件的话，检测一下该文件是否被锁住？
            //{
            //    for (int i = 0; i < 5; i++)
            //    {
            //        try
            //        {
            //            FileStream fs = new FileStream(uri, FileMode.Open, FileAccess.Read, FileShare.None);
            //            fs.Close();
            //            canAccess = true;
            //            break;
            //        }
            //        catch
            //        {
            //            canAccess = false;
            //            Logger.I("file exist,but can't access! TRY AGAIN！{0}", uri);
            //            System.Threading.Thread.Sleep(20);
            //        }
            //    }
            //}

            //return canAccess;
        }

        #endregion

        
    }
}
