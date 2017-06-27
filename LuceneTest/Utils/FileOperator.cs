using AnyTags.Net;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace AnyTag.BL
{
    class FileOperator
    {
        //const int FO_COPY = 0x2;
        //const int FOF_ALLOWUNDO = 0x44;
        //const int FOF_SILENT = 0x2;
        //const int FOF_SHOW = 0x100;

        //[StructLayout(LayoutKind.Sequential, CharSet=CharSet.Unicode)]
        //If you use the above you may encounter an invalid memory access exception (when using ANSI
        //or see nothing (when using unicode) when you use FOF_SIMPLEPROGRESS flag.
        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        struct SHFILEOPSTRUCT
        {
            public IntPtr hwnd;
            public FileFuncFlags wFunc;
            [MarshalAs(UnmanagedType.LPWStr)]
            public string pFrom;
            [MarshalAs(UnmanagedType.LPWStr)]
            public string pTo;
            public FILEOP_FLAGS fFlags;
            [MarshalAs(UnmanagedType.Bool)]
            public bool fAnyOperationsAborted;
            public IntPtr hNameMappings;
            [MarshalAs(UnmanagedType.LPWStr)]
            public string lpszProgressTitle;
        }
        [Flags]
        public enum FILEOP_FLAGS : ushort
        {
            FOF_MULTIDESTFILES = 0x1,
            FOF_CONFIRMMOUSE = 0x2,
            /// <summary>
            /// Don't create progress/report
            /// </summary>
            FOF_SILENT = 0x4,
            FOF_RENAMEONCOLLISION = 0x8,
            /// <summary>
            /// Don't prompt the user.
            /// </summary>
            FOF_NOCONFIRMATION = 0x10,
            /// <summary>
            /// Fill in SHFILEOPSTRUCT.hNameMappings.
            /// Must be freed using SHFreeNameMappings
            /// </summary>
            FOF_WANTMAPPINGHANDLE = 0x20,
            FOF_ALLOWUNDO = 0x40,
            /// <summary>
            /// On *.*, do only files
            /// </summary>
            FOF_FILESONLY = 0x80,
            /// <summary>
            /// Don't show names of files
            /// </summary>
            FOF_SIMPLEPROGRESS = 0x100,
            /// <summary>
            /// Don't confirm making any needed dirs
            /// </summary>
            FOF_NOCONFIRMMKDIR = 0x200,
            /// <summary>
            /// Don't put up error UI
            /// </summary>
            FOF_NOERRORUI = 0x400,
            /// <summary>
            /// Dont copy NT file Security Attributes
            /// </summary>
            FOF_NOCOPYSECURITYATTRIBS = 0x800,
            /// <summary>
            /// Don't recurse into directories.
            /// </summary>
            FOF_NORECURSION = 0x1000,
            /// <summary>
            /// Don't operate on connected elements.
            /// </summary>
            FOF_NO_CONNECTED_ELEMENTS = 0x2000,
            /// <summary>
            /// During delete operation, 
            /// warn if nuking instead of recycling (partially overrides FOF_NOCONFIRMATION)
            /// </summary>
            FOF_WANTNUKEWARNING = 0x4000,
            /// <summary>
            /// Treat reparse points as objects, not containers
            /// </summary>
            FOF_NORECURSEREPARSE = 0x8000
        }
        public enum FileFuncFlags : uint
        {
            FO_MOVE = 0x1,
            FO_COPY = 0x2,
            FO_DELETE = 0x3,
            FO_RENAME = 0x4
        }

        [DllImport("shell32.dll", CharSet = CharSet.Unicode)]
        static extern int SHFileOperation([In] ref SHFILEOPSTRUCT lpFileOp);

        public static bool CopyFile(string src,string dst)
        {
            SHFILEOPSTRUCT fileop = new SHFILEOPSTRUCT();
            fileop.hwnd = IntPtr.Zero;
            fileop.hNameMappings = IntPtr.Zero;
            fileop.wFunc = FileFuncFlags.FO_COPY;
            fileop.pFrom = src+'\0'+'\0';
            fileop.pTo = dst + '\0' + '\0';
            fileop.lpszProgressTitle = "文件拷贝" + '\0' + '\0';
            fileop.fFlags = FILEOP_FLAGS.FOF_SIMPLEPROGRESS;
            return SHFileOperation(ref fileop) == 0;
        }
        public static void LocateFile(string f)
        {
            if(isValidFileUrl(f))
            {
                System.Diagnostics.Process.Start("explorer.exe", "/select," + f);
            }
        }
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
