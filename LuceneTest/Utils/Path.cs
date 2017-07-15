using AnyTagNet;
using LuceneTest.Core;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text.RegularExpressions;

namespace AnyTags.Net
{
    public class MyTemplate
    {
        //模板管理，可以为每一个tag新建一个或多个描述文件，这些描述文件的格式
        //根据后缀名，获得该后缀名对应的模板文件
        public static string DefaultDocTemplate(string postfix)
        {
            postfix = postfix.TrimStart('.');
            string[] allTemplate = Directory.GetFiles(TemplateRoot, "*."+postfix);
            if (allTemplate.Length > 0) return allTemplate[0];
            else return null;
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
            string[] allTemplate = Directory.GetFiles(TemplateRoot,  "*.*");
            foreach (string f in allTemplate)
            {
                FileInfo fi = new FileInfo(f);
                ret.Add(fi.Extension.Trim('.'));
            }
            return ret;
        }
        public static string GetTemplateFileFilter()
        {
            string ret = "";
            string[] allTemplate = Directory.GetFiles(TemplateRoot, "*.*");
            foreach (string f in allTemplate)
            {
                FileInfo fi = new FileInfo(f);
                if(ret!="")
                {
                    ret += "|";
                }
                ret += (fi.Name+"|*"+fi.Extension);
            }
            return ret;
        }
    }
    public class MyPath
    {
        public static string DBFILE
        {
            get
            {
                return Path.Combine(MyPath.Root, "tags.db");
            }
        }

        public static string[] FilesRelocation(string [] srcs,string currentTag)
        {
            string[] dsts = new string[srcs.Length];
            for (int i = 0; i < srcs.Length; i++)
            {
                string s = srcs[i];
                string n = System.IO.Path.GetFileName(s);
                string d = System.IO.Path.Combine(MyPath.GetDirPath(currentTag), n);
                dsts[i] = d;
            }
            return dsts;
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
        public static string GetTagByPath(string path)
        {
            if (!path.Contains(DocRoot))
            {
                Logger.I("GetTagByPath Failed,File(Dir) !pathContains: {0}", path);
                return null; //根本不在doc目录中
            }
            //if (!File.Exists(path) && !Directory.Exists(path))
            //{
            //    Logger.I("GetTagByPath Failed,File(Dir) not Exist: {0}", path);
            //    return null;
            //}

            string[] dirs= path.Substring(DocRoot.Length).Split(
                new char[] { Path.DirectorySeparatorChar }, 
                System.StringSplitOptions.RemoveEmptyEntries);
            if (dirs.Length != 2)
            {
                Logger.I("GetTagByPath Failed,File(Dir) dirs!=2: {0}", path);
                return null;
            }
            else return dirs[0];
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

    }
}
