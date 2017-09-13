using System.Configuration;
using System.IO;

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
                    return ConfigurationManager.AppSettings["RootDir"];
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
        public static string UriDBPath { get { return Path.Combine(RootPath, "UriDB"); } }
        public static string VDir { get { return RootSubDir("VirtualDir"); } }
        public static string DocBasePath { get { return RootSubDir("DocumentBase"); } }

        //获得文档存放根路径（TODO，后面可能需要支持文档根路径有多个，类似于编译器的-I选项）
        public static string DocDir { get { return SubDir(DocBasePath, "Doc"); } }
        public static string TemplatePath { get { return Path.Combine(DocBasePath, "Template"); } }
        public static string Res_Path { get { return SubDir(DocBasePath, "Res"); } }


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
