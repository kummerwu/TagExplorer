using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Text.RegularExpressions;

namespace TagExplorer.Utils
{
    public class CfgPath
    {
        //public static string RootPath = @"J:\00TagExplorerBase";
        /*路径规划
         J:\00TagExplorerBase
            DocumentBase
               Doc
                   ....各种tag目录
               Template
                   ....各种模板文件
            TagDB
            UriDB

       */
        public static string ChangePathRoot(string fullPath)
        {
            //是链接或有效文件，直接返回
            if (!PathHelper.IsValidUri(fullPath))
            {
                //不是链接，并且该文件和目录不存在，尝试换根目录，看看能否找到。
                int idx = fullPath.IndexOf("\\DocumentBase\\Doc\\");
                if (idx > 0)
                {
                    string tmp = Path.Combine(RootPath, fullPath.Substring(idx));
                    if (PathHelper.IsValidUri(tmp)) return tmp;
                }
            }
            return fullPath;
        }
        private static string InvalidChar(string tag)
        {
            bool hasErr = false;
            string err = "标签不允许包含下面特殊字符\r\n   ";
            List<char> chars = new List<char>(Path.GetInvalidFileNameChars());
            chars.AddRange(Path.GetInvalidPathChars());
            foreach(char c in chars)
            {
                if(tag.IndexOf(c)!=-1)
                {
                    err += c;
                    hasErr = true;
                }
            }
            if (hasErr) return err;
            else return null;
            
        }

        public static string FormatPathName(string name)
        {
            string newN = name;
            char[] chars = Path.GetInvalidFileNameChars();
            foreach (char c in chars)
            {
                newN = newN.Replace(c, '_');
            }
            return newN;
        }
        //返回null，则是合法的，
        //非null，则表示错误提示信息
        public static string CheckTagFormat(string tag)
        {
            if (string.IsNullOrEmpty(tag)) return "标签不能为空";
            else if (tag.Length > 64) return "标签长度不能超过64个字符，具体内容建议记录在标签笔记中。";
            else return InvalidChar(tag);
            
        }
        private static string rootPath = null;
        public static string RootPath
        {
            get
            {
                if (rootPath == null)
                {
                    string tmp = ConfigurationManager.AppSettings["RootDir"];
                    string[] paths = tmp.Split(' ');
                    foreach(string p in paths)
                    {
                        if(Directory.Exists(p))
                        {
                            rootPath = p;
                            break;
                        }

                    }
                    return rootPath;

                }
                else return rootPath;
            }
            set
            {
                if (Directory.Exists(value))
                {
                    rootPath = value;
                    ConfigurationManager.AppSettings.Set("RootDir", value);

                    System.Configuration.Configuration config =
                    ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
                    config.AppSettings.Settings.Remove("RootDir");
                    config.AppSettings.Settings.Add("RootDir", value);
                    config.Save(ConfigurationSaveMode.Modified);
                    

                }
            }
        }
        //Root根目录下的一级目录下的文件
        public static string IniFilePath { get { return Path.Combine(RootPath, "TagExplorer.ini"); } }
        public static string LayoutCfgFilePath { get { return Path.Combine(RootPath, "TagExplorerLayout.xml"); } }
        public static string StaticCfg { get { return Path.Combine(RootPath, "StaticCfg.json"); } }
        public static string DynamicCfg { get { return Path.Combine(RootPath, "DynamicCfg.json"); } }

        //Root根目录下的一级目录下的文件夹
        public static string TagDBPath { get { return Path.Combine(RootPath, "TagDB"); } }
        public static string TagDBPath_Json {
            get
            {
                string tdbDir = RootSubDir("TagDBJson");
                return Path.Combine(tdbDir, "Tags3.json");
            }
        }
        public static string TagDBPath_Export
        {
            get
            {
                return Path.Combine(Exchange_Path, "Export-Tags.json");
            }
        }
        public static string UriDBPath_Export
        {
            get
            {
                return Path.Combine(Exchange_Path, "Export-Uris.json");
            }
        }
        public static string TagDBPath_SQLite
        {
            get
            {
                string tdbDir = RootSubDir("TagDBSQL");
                return Path.Combine(tdbDir, "sqlite.db");
            }
        }
        public static string UriDBPath { get { return Path.Combine(RootPath, "UriDB"); } }
        public static string VDir { get { return RootSubDir("VirtualDir"); } }
        public static string DocBasePath { get { return RootSubDir("DocumentBase"); } }

        //获得文档存放根路径（TODO，后面可能需要支持文档根路径有多个，类似于编译器的-I选项）
        public static string DocDir { get { return SubDir(DocBasePath, "Doc"); } }
        public static string RecycleDir { get { return RootSubDir("Recycle"); } }//{ get { return SubDir(DocBasePath, "Recycle"); } }
        public static string TemplatePath { get { return Path.Combine(DocBasePath, "Template"); } }
        public static string Res_Path { get { return SubDir(DocBasePath, "Res"); } }
        public static string Exchange_Path { get { return SubDir(DocBasePath, "Exchange"); } }

        public static string GetRecycleName(string name)
        {
            string dst = Path.Combine(CfgPath.RecycleDir, name);
            while (Directory.Exists(dst) || File.Exists(dst) )
            {
                dst = dst + "$" + Guid.NewGuid().ToString();
            }
            return dst;
        }
        public static string GetTemplateFileByTag(string tag,string dotPostfix)
        {
            string file = CfgPath.GetFileByTag(tag, "note"+dotPostfix);
            string tmplateFile = TemplateHelper.GetTemplateByExtension(dotPostfix);
            if (!File.Exists(file) && File.Exists(tmplateFile))
            {
                File.Copy(tmplateFile, file);
            }
            if(File.Exists(file))
            {
                return file;
            }
            else
            {
                return null;
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

        public static string DownlodPdfCmd()
        {
            string d = CfgPath.Res_Path;
            string cmd = Path.Combine(d, "pdfdownload", "pdfdownload.bat");
            if (File.Exists(cmd)) return cmd;
            else return null;
        }
        public static string GetWordPadExeFile()
        {
            string d = CfgPath.Res_Path;
            string f = Path.Combine(d, "wordpad", "wordpad.exe");
            if(File.Exists(f))
            {
                return f;
            }
            else
            {
                return "wordpad.exe";
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

            string[] dirs = path.Substring(CfgPath.DocDir.Length).Split(
                new char[] { Path.DirectorySeparatorChar },
                System.StringSplitOptions.RemoveEmptyEntries);
            if (dirs.Length != 2)
            {
                Logger.I("GetTagByPath Failed,File(Dir) dirs!=2: {0}", path);
                return null;
            }
            else return dirs[0];
        }

        /// <summary>
        /// 根据tag和后缀名获得文件路径
        /// </summary>
        /// <param name="tag">标签名称</param>
        /// <param name="postfix">文件后缀名（不带.) </param>
        /// <returns></returns>
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
            bool ret = Regex.IsMatch(name, @"(_files$)|(^~)|(.tmp$)|(\.note\.rtf)", RegexOptions.IgnoreCase);
            if (ret)
            {
                Logger.I("name match reg: {0}", uri);
            }
            return ret;
        }
        ///  辅助函数
        private static string RootSubDir(string name)
        {
            return SubDir(RootPath, name);
        }
        private static string SubDir(string parent, string name)
        {
            string subDir = Path.Combine(parent, name);
            if (!Directory.Exists(subDir))
            {
                Directory.CreateDirectory(subDir);
            }
            return subDir;
        }
    }
        
}
