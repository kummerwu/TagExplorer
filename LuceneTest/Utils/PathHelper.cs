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

        //public static string RootPath = @"J:\00TagExplorerBase";
        private static string rootPath = null;
        public static string RootPath
        {
            get
            {
                if (rootPath == null)
                    return ConfigurationManager.AppSettings["RootDir"];
                else return rootPath;
            }
            set
            {
                rootPath = value;
            }
        }
        public static string TagDBPath { get { return Path.Combine(RootPath , @"TagDB"); } }
        public static string UriDBPath { get { return Path.Combine(RootPath, @"UriDB"); } }
        public static string IniFilePath { get { return Path.Combine(RootPath, "TagExplorer.ini"); } }


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
        
        public static string DocBaseDir
        {
            get {
                if (docBase == null)
                {
                    string exe = Process.GetCurrentProcess().MainModule.FileName;
                    string exePath = Path.GetFileNameWithoutExtension(exe);
                    exePath =  PathHelper._DocBaseDir;
                    if(!Directory.Exists(exePath))
                    {
                        Directory.CreateDirectory(exePath);
                    }
                    docBase = exePath;
                }
                return docBase;
            }
        }
        
        //获得文档存放根路径（TODO，后面可能需要支持文档根路径有多个，类似于编译器的-I选项）
        public static string DocDir
        {
            get
            {
                if (doc != null)
                    return doc;


                string dir = Path.Combine(DocBaseDir, "Doc");
                if(!Directory.Exists(dir))
                {
                    Directory.CreateDirectory(dir);
                    
                }
                doc = dir;
                return doc;
            }
        }
        public static string GetTagByPath(string path)
        {
            if (!path.Contains(DocDir))
            {
                Logger.I("GetTagByPath Failed,File(Dir) !pathContains: {0}", path);
                return null; //根本不在doc目录中
            }
            //if (!File.Exists(path) && !Directory.Exists(path))
            //{
            //    Logger.I("GetTagByPath Failed,File(Dir) not Exist: {0}", path);
            //    return null;
            //}

            string[] dirs= path.Substring(DocDir.Length).Split(
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
            string dir = System.IO.Path.Combine(PathHelper.DocDir, tag);
            if (!Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
            }
            return dir;
        }

        public static bool NeedSkipThisUri(string uri)
        {
            string name = Path.GetFileName(uri);
            bool canAccess = true;
            if (File.Exists(uri)) //如果是文件的话，检测一下该文件是否被锁住？
            {
                try
                {
                    FileStream fs = new FileStream(uri, FileMode.Open, FileAccess.Read, FileShare.None);
                    fs.Close();
                }
                catch
                {
                    canAccess = false;
                }
            }
            return Regex.IsMatch(name, @"(_files$)|(^~)|(.tmp$)",RegexOptions.IgnoreCase) || 
                !canAccess;
        }

        #endregion

        #region 私有方法
        private static string _DocBaseDir { get { return Path.Combine(RootPath, @"DocumentBase"); } }
        private static string docBase;
        private static string doc = null;
        #endregion
    }
}
