using System;
using System.Collections.Generic;
using System.IO;

namespace TagExplorer.Utils
{
    public class TemplateHelper
    {
        //模板管理，可以为每一个tag新建一个或多个描述文件，这些描述文件的格式
        //根据后缀名，获得该后缀名对应的模板文件
        public static string GetTemplateByExtension(string postfix)
        {
            postfix = postfix.TrimStart('.');
            string[] allTemplate = Directory.GetFiles(TemplateDir, "*."+postfix);
            if (allTemplate.Length > 0) return allTemplate[0];
            
            else return null;
        }
        //获得所有模板文件的后缀名
        public static List<string> GetAllExtension()
        {
            List<string> ret = new List<string>();
            string[] allTemplate = Directory.GetFiles(TemplateDir,  "*.*");
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
            string[] allTemplate = Directory.GetFiles(TemplateDir, "*.*");
            foreach (string f in allTemplate)
            {
                FileInfo fi = new FileInfo(f);
                if(ret!="")
                {
                    ret += "|";
                }
                ret += (fi.Name+"|*"+fi.Extension);
            }
            ret += "|所有文件(*.*)|*.*";
            return ret;
        }


        //模板文件存放的根路径
        private static string TemplateDir
        {
            get
            {
                try
                {
                    string path = Path.Combine(PathHelper.DocBaseDir, "Template");
                    if (Directory.Exists(path))
                    {
                        Directory.CreateDirectory(path);
                    }
                    return path;
                }
                catch (Exception e)
                {
                    Logger.E(e);
                    return null;
                }

            }
        }
    }
}
