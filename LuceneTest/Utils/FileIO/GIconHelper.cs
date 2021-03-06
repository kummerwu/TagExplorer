﻿using System;
using System.Collections;
using System.Drawing;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media.Imaging;

namespace TagExplorer.Utils
{
    class GIconHelper
    {
        #region 公有方法
        //TODO 如果获取失败，返回一个默认未知图标
        //TODO  支持http协议图标
        public static BitmapSource GetBitmapFromFile(string f)
        {
            if (iconCache.Count > 1000) iconCache.Clear();

            string type = FileToType(f);
            if (iconCache[type] == null)
            {
                BitmapSource s = GetBitmapFromFileNoCache(f,0);
                if (s != null)
                {
                    iconCache.Add(type, s);
                }
                else
                {
                    //TODO 获取失败怎么处理
                }
            }
            return iconCache[type] as BitmapSource;


        }
        #endregion

        #region 私有方法
        private Icon GetIconByFile(string full) 
        {
            return Icon.ExtractAssociatedIcon(full);
        }
        private static WriteableBitmap GetWriteableBitmapFromFile(string f)
        {
            var icon = GIconHelper.GetFileIcon(f, false).ToBitmap();
            IntPtr hBitmap = icon.GetHbitmap();
            BitmapSource bitmapSource = Imaging.CreateBitmapSourceFromHBitmap(hBitmap, IntPtr.Zero, Int32Rect.Empty,
            BitmapSizeOptions.FromEmptyOptions());
            bitmapSource.Freeze();
            WriteableBitmap writeableBmp = new WriteableBitmap(bitmapSource);
            icon.Dispose();
            return writeableBmp;
        }

        private static string FileToType(string f)
        {
            if (PathHelper.IsValidWebLink(f))
            {
                return ".http_icon";
            }
            else if (Directory.Exists(f))
            {
                return ".directory_icon";
            }
            else if(File.Exists(f))
            {
                string ext =  Path.GetExtension(f);
                if(ext==".exe")
                {
                    return f;
                }
                else
                {
                    return ext;
                }
            }
            else
            {
                return ".unknow_icon";
            }
        }

        private static Hashtable iconCache = new Hashtable();

        
        private static BitmapSource GetBitmapFromFileNoCache(string f,int level)
        {
            Logger.I("GetBitmapFromFileNoCache for {0} level {1}", f, level);
            if (level > 5) return null; //防止递归调用死循环，将堆栈耗尽
            if (PathHelper.IsValidWebLink(f))
            {
                return GetBitmapFromFileNoCache(CfgPath.Res_HTTP_Path,level+1);
            }
            else if(PathHelper.IsValidFS(f))
            {
                try
                {
                    Logger.I(" isvalidfs try GetBitmapFromFileNoCache for {0} level {1}", f, level);
                    Icon ic = GIconHelper.GetFileIcon(f, false);
                    if(ic==null)
                    {
                        Logger.I("GetBitmapFromFileNoCache failed: ic==null for {0} level {1}", f, level);
                        return null;
                    }
                    var icon = ic.ToBitmap();
                    IntPtr hBitmap = icon.GetHbitmap();
                    BitmapSource bitmapSource = Imaging.CreateBitmapSourceFromHBitmap(hBitmap, IntPtr.Zero, Int32Rect.Empty,
                    BitmapSizeOptions.FromEmptyOptions());
                    bitmapSource.Freeze();
                    icon.Dispose();
                    return bitmapSource;
                }
                catch (Exception e)
                {
                    Logger.E(e);
                    Logger.E("get icon failed!======{0}", f);
                    if (f != CfgPath.Res_UNKNOW_Path)
                    {
                        return GetBitmapFromFileNoCache(CfgPath.Res_UNKNOW_Path, level + 1);
                    }
                    else
                    {
                        return null;
                    }
                }
            }
            else
            {
                return GetBitmapFromFileNoCache(CfgPath.Res_UNKNOW_Path,level+1);
            }
        }


        /// 获取文件的默认图标 /// 
        /// /// 文件名。 
        /// /// 可以只是文件名，甚至只是文件的扩展名(.*)； 
        /// /// 如果想获得.ICO文件所表示的图标，则必须是文件的完整路径。 
        /// /// /// 是否大图标 
        /// /// 文件的默认图标 
        private static Icon GetFileIcon(string fileName, bool largeIcon)
        {
            if(PathHelper.IsValidWebLink(fileName))
            {
                fileName = CfgPath.Res_HTTP_Path;
            }
            SHFILEINFO info = new SHFILEINFO(true);
            int cbFileInfo = Marshal.SizeOf(info);
            SHGFI flags;
            if (largeIcon)
                flags = SHGFI.Icon | SHGFI.LargeIcon ;
            else flags = SHGFI.Icon | SHGFI.SmallIcon ;
            
            SHGetFileInfo(fileName, 256, out info, (uint)cbFileInfo, flags);
            if(info.hIcon == IntPtr.Zero)
            {
                Logger.E("getfileicon err: {0}", fileName);
                return null;
            }
            else return Icon.FromHandle(info.hIcon);
        }
        [DllImport("Shell32.dll")]
        private static extern int SHGetFileInfo(string pszPath, uint dwFileAttributes, out SHFILEINFO psfi, uint cbfileInfo, SHGFI uFlags);
        [StructLayout(LayoutKind.Sequential)]
        private struct SHFILEINFO
        {
            public SHFILEINFO(bool b)
            {
                hIcon = IntPtr.Zero;
                iIcon = 0;
                dwAttributes = 0;
                szDisplayName = "";
                szTypeName = "";
            }
            public IntPtr hIcon;
            public int iIcon;
            public uint dwAttributes;
            [MarshalAs(UnmanagedType.LPStr, SizeConst = 260)]
            public string szDisplayName;
            [MarshalAs(UnmanagedType.LPStr, SizeConst = 80)]
            public string szTypeName;
        };
        private enum SHGFI
        {
            SmallIcon = 0x00000001,
            LargeIcon = 0x00000000,
            Icon = 0x00000100,
            DisplayName = 0x00000200,
            Typename = 0x00000400,
            SysIconIndex = 0x00004000,
            UseFileAttributes = 0x00000010
        }

        private static string ShortTxt(string txt)
        {
            int MAXLEN = 48;
            int PRELEN = 5;
            if (txt.Length > MAXLEN)
            {
                txt = txt.Substring(0, PRELEN) + " ... " + txt.Substring(txt.Length - (MAXLEN - PRELEN));
            }
            return txt;
        }
        #endregion
    }
}
