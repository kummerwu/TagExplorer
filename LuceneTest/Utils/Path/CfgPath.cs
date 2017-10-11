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


        private static string rootPath = null;
        public static string RootPath
        {
            get
            {
                if (rootPath == null)
                {
                    rootPath = ConfigurationManager.AppSettings["RootDir"];
                    return rootPath;

                }
                else return rootPath;
            }
            set
            {
                rootPath = value;
            }
        }
        //Root根目录下的一级目录下的文件
        public static string IniFilePath { get { return Path.Combine(RootPath, "TagExplorer.ini"); } }
        public static string LayoutCfgFilePath { get { return Path.Combine(RootPath, "TagExplorerLayout.xml"); } }

        //Root根目录下的一级目录下的文件夹
        public static string TagDBPath { get { return Path.Combine(RootPath, "TagDB"); } }
        public static string TagDBPath_Json {
            get
            {
                string tdbDir = RootSubDir("TagDBJson");
                return Path.Combine(tdbDir, "Tags.json");
            }
        }
        public static string UriDBPath { get { return Path.Combine(RootPath, "UriDB"); } }
        public static string VDir { get { return RootSubDir("VirtualDir"); } }
        public static string DocBasePath { get { return RootSubDir("DocumentBase"); } }

        //获得文档存放根路径（TODO，后面可能需要支持文档根路径有多个，类似于编译器的-I选项）
        public static string DocDir { get { return SubDir(DocBasePath, "Doc"); } }
        public static string TemplatePath { get { return Path.Combine(DocBasePath, "Template"); } }
        public static string Res_Path { get { return SubDir(DocBasePath, "Res"); } }
        public static string GetNoteFileByTag(string tag)
        {
            string dir = GetDirByTag(tag);
            string file = Path.Combine(dir, tag + ".note.rtf");
            string temp = Path.Combine(CfgPath.TemplatePath, "RTF文档.rtf");
            //if (!File.Exists(temp)) return null;

            if (!File.Exists(file))
            {
                File.Copy(temp, file);
            }

            return file;

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
