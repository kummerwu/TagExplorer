using AnyTags.Net;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace AnyTag.BL
{
    class FileOperator
    {
        public static void OpenTagDir(string title)
        {
            if (title != null)
            {
                string tag = title;
                string dir = MyPath.GetDirPath(tag);
                Process.Start(dir);
            }
        }
        public static void StartFile(string file)
        {
            if(isValidFileUrl(file))
            {
                Process.Start(file);
            }
        }
        public static List<string> GetFileListFromClipboard()
        {
            List<string> files = new List<string>();

            string txt = Clipboard.GetText();
            txt = txt.Trim('"');
            txt = txt.Trim();
            if (FileOperator.isValidFileUrl(txt) && !files.Contains(txt))
            {
                files.Add(txt);
            }
            StringCollection sc = Clipboard.GetFileDropList();
            foreach (string f in sc)
            {

                txt = f.Trim('"');

                if (FileOperator.isValidFileUrl(txt) && !files.Contains(txt))
                {
                    files.Add(txt);
                }
            }

            return files;
        }
        public static bool isValidFileUrl(string txt)
        {
            return File.Exists(txt) || Directory.Exists(txt) ||
                            (txt.StartsWith("http://", StringComparison.CurrentCultureIgnoreCase) ||
                            txt.StartsWith("https://", StringComparison.CurrentCultureIgnoreCase));
        }

        public static void TryOpenTagFile(string title, string postfix)
        {
            if (title != null && postfix != null)
            {
                string tag = title;
                string file = MyPath.GetFilePath(tag, postfix);
                //string file = System.IO.Path.Combine(MyPath.DocRoot, tag + GConfig.DefaultPostfix);

                //如果文件已经存在，直接打开
                if (File.Exists(file))
                {
                    System.Diagnostics.Process.Start(file);
                }
                else
                {
                    //如果文件不存在，尝试在其他路径上查找
                    string[] files = Directory.GetFiles(MyPath.DocRoot, tag + "." + postfix);
                    string[] files2 = Directory.GetFiles(MyPath.GetDirPath(tag), tag + "." + postfix);

                    if (files.Length > 0)
                    {
                        file = files[0];
                        System.Diagnostics.Process.Start(file);
                    }
                    else if (files2.Length > 0)
                    {
                        file = files2[0];
                        System.Diagnostics.Process.Start(file);
                    }
                    else // 否则新建一个文件
                    {
                        if (File.Exists(MyTemplate.DefaultDocTemplate(postfix)))
                        {
                            File.Copy(MyTemplate.DefaultDocTemplate(postfix), file);
                        }
                        else
                        {
                            File.Create(file).Close();
                        }
                        //db.AddTagFile(tag, file, tag);
                        System.Diagnostics.Process.Start(file);
                    }
                }

            }
        }
    }
}
