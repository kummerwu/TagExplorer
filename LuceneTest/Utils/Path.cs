using AnyTagNet;
using LuceneTest.Core;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

namespace AnyTags.Net
{
    class MyTemplate
    {
        //模板管理，可以为每一个tag新建一个或多个描述文件，这些描述文件的格式
        //根据后缀名，获得该后缀名对应的模板文件
        public static string DefaultDocTemplate(string postfix)
        {
            return Path.Combine(TemplateRoot, GConfig.DefaultTag + "." + postfix);
        }
        //模板文件存放的根路径
        private static string TemplateRoot
        {
            get
            {
                string path = Path.Combine(MyPath.Root, "Template");
                if (Directory.Exists(path)) return path;
                else return null;
            }
        }
        //获得所有模板文件的后缀名
        public static List<string> GetPostfix()
        {
            List<string> ret = new List<string>();
            string[] allTemplate = Directory.GetFiles(TemplateRoot, GConfig.DefaultTag + ".*");
            foreach (string f in allTemplate)
            {
                FileInfo fi = new FileInfo(f);
                ret.Add(fi.Extension.Trim('.'));
            }
            return ret;
        }
    }
    class MyPath
    {
        public static string DBFILE
        {
            get
            {
                return Path.Combine(MyPath.Root, "tags.db");
            }
        }
        //路径规划
        //根路径
        //    --Doc      文档所在路径
        //    --Template 模板文件
        private static string root;
        public static string Root
        {
            get {
                if (root == null)
                {
                    string exe = Process.GetCurrentProcess().MainModule.FileName;
                    string exePath = Path.GetFileNameWithoutExtension(exe);
                    exePath =  Cfg.Ins.DocBase;
                    if(!Directory.Exists(exePath))
                    {
                        Directory.CreateDirectory(exePath);
                    }
                    root = exePath;
                }
                return root;
            }
        }
        private static string docRoot = null;
        //获得文档存放根路径（TODO，后面可能需要支持文档根路径有多个，类似于编译器的-I选项）
        public static string DocRoot
        {
            get
            {
                if (docRoot != null)
                    return docRoot;


                string dir = Path.Combine(Root, "Doc");
                if(!Directory.Exists(dir))
                {
                    Directory.CreateDirectory(dir);
                    
                }
                docRoot = dir;
                return docRoot;
            }
        }

        public static string GetFilePath(string tag, string postfix)
        {
            return System.IO.Path.Combine(GetDirPath(tag), tag + "." + postfix);
        }
        public static string GetDirPath(string tag)
        {
            string dir = System.IO.Path.Combine(MyPath.DocRoot, tag);
            if (!Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
            }
            return dir;
        }

    }
}
