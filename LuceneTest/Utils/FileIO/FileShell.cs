﻿using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows;

namespace TagExplorer.Utils
{
    public class FileShell
    {

        
        private static bool ListToSHStr(string [] srcList,string [] dstList,out string src,out string dst)
        {
            src = "";
            dst = "";


            if (srcList.Length != dstList.Length)
            {
                Logger.E("FileOperate scrList dstList 参数个数不匹配");
                return false;
            }
            
            int i = 0;
            for (i = 0; i < srcList.Length; i++)
            {
                if (srcList[i] != dstList[i])
                {
                    src = src + srcList[i] + '\0';
                    dst = dst + dstList[i] + '\0';
                }
            }
            //最后需要以_T0，双0结尾。
            src += '\0';
            dst += '\0';
            return true;
        }

        #region 公有方法
        /// <summary>
        /// 移动文件（如果文件比较多的话，会出现Window Shell的文件拷贝进度条）
        /// </summary>
        /// <param name="srcList"></param>
        /// <param name="dstList"></param>
        /// <returns></returns>
        public static bool SHMoveFiles(string[] srcList, string[] dstList)
        {

            string src, dst;
            if (!ListToSHStr(srcList, dstList, out src, out dst))
                return false;

            Logger.D(string.Format("moveFile: {0} => {1}", src, dst));

            SHFILEOPSTRUCT fileop = new SHFILEOPSTRUCT();
            fileop.hwnd = IntPtr.Zero;
            fileop.hNameMappings = IntPtr.Zero;
            fileop.wFunc = FileFuncFlags.FO_MOVE;
            fileop.pFrom = src + '\0' + '\0';
            fileop.pTo = dst + '\0' + '\0';
            fileop.lpszProgressTitle = "文件移动" + '\0' + '\0';
            fileop.fFlags = FILEOP_FLAGS.FOF_SIMPLEPROGRESS | FILEOP_FLAGS.FOF_MULTIDESTFILES;
            return SHFileOperation(ref fileop) == 0;
        }

        /// <summary>
        /// 移动文件（如果文件比较多的话，会出现Window Shell的文件拷贝进度条）
        /// </summary>
        /// <param name="srcList"></param>
        /// <param name="dstList"></param>
        /// <returns></returns>
        public static bool SHCopyFile(string[] srcList, string[] dstList)
        {
            string src, dst;
            if (!ListToSHStr(srcList, dstList, out src, out dst))
                return false;

            Logger.D(string.Format("CopyFile: {0} => {1}", src, dst));

            SHFILEOPSTRUCT fileop = new SHFILEOPSTRUCT();
            fileop.hwnd = IntPtr.Zero;
            fileop.hNameMappings = IntPtr.Zero;
            fileop.wFunc = FileFuncFlags.FO_COPY;
            fileop.pFrom = src + '\0' + '\0';
            fileop.pTo = dst + '\0' + '\0';
            fileop.lpszProgressTitle = "文件拷贝" + '\0' + '\0';
            fileop.fFlags = FILEOP_FLAGS.FOF_SIMPLEPROGRESS | FILEOP_FLAGS.FOF_MULTIDESTFILES;//注意，如果是多个文件，需要有| FILEOP_FLAGS.FOF_MULTIDESTFILES，否则会出现奇怪的拷贝出一个目录的现象
            return SHFileOperation(ref fileop) == 0;
        }

        /// <summary>
        /// 打开Explorer，并选中指定文件
        /// </summary>
        /// <param name="f"></param>
        public static void OpenExplorerByFile(string f)
        {

            if (PathHelper.IsValidFS(f))
            {
                f = '"' + f + '"';
                string cmd = "/select," + f;
                Logger.D("LocateFile: [{0}]", cmd);
                System.Diagnostics.Process.Start("explorer.exe", "/select," + f);
            }
            else if(PathHelper.IsValidWebLink(f))
            {
                OpenFile(f);
            }
            else
            {
                Logger.D(string.Format("LocateFile not validFileUrl: [{0}]", f));
            }
        }

        /// <summary>
        /// 打开Tag所在的目录
        /// </summary>
        /// <param name="title"></param>
        public static void OpenExplorerByTag(string title)
        {
            if (title != null)
            {
                string tag = title;
                string dir = CfgPath.GetDirByTag(tag,true);//打开目录，现在创建
                Logger.D("OpenTagDir {0} {1}", tag, dir);
                Process.Start(dir);
            }
        }
        private static void UpdateAccessTime(string file)
        {
            try
            {
                if (File.Exists(file))
                {
                    File.SetLastAccessTime(file, DateTime.Now);
                }
                if (Directory.Exists(file))
                {
                    Directory.SetLastAccessTime(file, DateTime.Now);
                }
            }
            catch (Exception e)
            {
                Logger.E(e);
            }
        }

        /// <summary>
        /// 打开文件（让用户选择文件打开方式）
        /// </summary>
        /// <param name="file"></param>
        public static void OpenFileWith(string file)
        {
            if(File.Exists(file))
            {
                UpdateAccessTime(file);
                Process.Start("rundll32.exe", " shell32.dll,OpenAs_RunDLL " + file);
            }
            else
            {
                MessageBox.Show("目前仅支持文件选择打开方式!");
            }
        }
        //是不是标签笔记文件
        public static bool IsNoteFile(string file)
        {
            return file.EndsWith(".note.rtf");
        }
        /// <summary>
        /// 打开文件
        /// </summary>
        /// <param name="file"></param>
        public static void OpenFile(string file)
        {
            if (IsNoteFile(file))
            {
                string wordpad = CfgPath.GetWordPadExeFile();
                Process.Start(wordpad, file);
                return;
            }

            Logger.D("StartFile {0} ", file);
            if (!PathHelper.IsValidUri(file))
            {
                Logger.E("Start File ERROR,File isn't valid!~");
                return;
            }

            UpdateAccessTime(file);
            if (File.Exists(file))
            {
                //之所以搞下面这么复杂的流程，是因为zte的一个文档安全软件导致Process.Start(file);报错
                //最终原因貌似与观察文件变更有关系，现在在文件变更过程通过定时器来触发，似乎就没有问题了。
                Logger.D("StartFile {0} is valid", file);
                
                Process p = Process.Start(file);
                
                return;
            }
            else
            {
                Process.Start(file);
            }
        }

        /// <summary>
        /// 获得当前剪贴板中的文件列表
        /// </summary>
        /// <returns></returns>
        public static List<string> GetFileListFromClipboard()
        {
            List<string> files = new List<string>();

            string txt = Clipboard.GetText();
            txt = txt.Trim('"');
            txt = txt.Trim();
            if (PathHelper.IsValidUri(txt) && !files.Contains(txt))
            {
                files.Add(txt);
            }
            StringCollection sc = Clipboard.GetFileDropList();

            foreach (string f in sc)
            {

                txt = f.Trim('"');

                if (PathHelper.IsValidUri(txt) && !files.Contains(txt))
                {
                    files.Add(txt);
                }
            }

            return files;
        }
        

        
        #endregion


        #region 私有方法
        //const int FO_COPY = 0x2;
        //const int FOF_ALLOWUNDO = 0x44;
        //const int FOF_SILENT = 0x2;
        //const int FOF_SHOW = 0x100;

        //[StructLayout(LayoutKind.Sequential, CharSet=CharSet.Unicode)]
        //If you use the above you may encounter an invalid memory access exception (when using ANSI
        //or see nothing (when using unicode) when you use FOF_SIMPLEPROGRESS flag.
        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        private struct SHFILEOPSTRUCT
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
        private enum FILEOP_FLAGS : ushort
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
        private enum FileFuncFlags : uint
        {
            FO_MOVE = 0x1,
            FO_COPY = 0x2,
            FO_DELETE = 0x3,
            FO_RENAME = 0x4
        }

        [DllImport("shell32.dll", CharSet = CharSet.Unicode)]
        static extern int SHFileOperation([In] ref SHFILEOPSTRUCT lpFileOp);
        
        

        private static void TryOpenTagFile(string title, string postfix)
        {
            if (title != null && postfix != null)
            {
                string tag = title;
                string file = CfgPath.GetFileByTag(tag, postfix);//file可能为null
                //string file = System.IO.Path.Combine(MyPath.DocRoot, tag + GConfig.DefaultPostfix);

                //如果文件已经存在，直接打开
                if (file!=null &&File.Exists(file))
                {
                    System.Diagnostics.Process.Start(file);
                }
                else
                {
                    //如果文件不存在，尝试在其他路径上查找
                    string[] files = Directory.GetFiles(CfgPath.DocDir, tag + "." + postfix);
                    string dir = CfgPath.GetDirByTag(tag); //如果目录不存在，就不要再这儿创建了。
                    string[] files2 = null;

                    if (Directory.Exists(dir))
                    {
                        files2 = Directory.GetFiles(dir, tag + "." + postfix);
                    }

                    if (files.Length > 0)
                    {
                        file = files[0];
                        System.Diagnostics.Process.Start(file);
                    }
                    else if (files2!=null && files2.Length > 0)
                    {
                        file = files2[0];
                        System.Diagnostics.Process.Start(file);
                    }
                    else // 否则新建一个文件
                    {
                        if (File.Exists(TemplateHelper.GetTemplateByExtension(postfix)))
                        {
                            File.Copy(TemplateHelper.GetTemplateByExtension(postfix), file);
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
        #endregion
    }
}
