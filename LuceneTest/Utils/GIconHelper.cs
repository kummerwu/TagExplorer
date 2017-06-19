﻿using AnyTags.Net;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media.Imaging;

namespace AnyTagNet
{
    class GIconHelper
    {
        Icon GetIconByFile(string full)
        {
            return Icon.ExtractAssociatedIcon(full);
        }
        public static WriteableBitmap GetWriteableBitmapFromFile(string f)
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
        public static BitmapSource GetBitmapFromFile(string f)
        {
            var icon = GIconHelper.GetFileIcon(f, false).ToBitmap();
            IntPtr hBitmap = icon.GetHbitmap();
            BitmapSource bitmapSource = Imaging.CreateBitmapSourceFromHBitmap(hBitmap, IntPtr.Zero, Int32Rect.Empty,
            BitmapSizeOptions.FromEmptyOptions());
            bitmapSource.Freeze();
            icon.Dispose();
            return bitmapSource;
        }


        /// 获取文件的默认图标 /// 
        /// /// 文件名。 
        /// /// 可以只是文件名，甚至只是文件的扩展名(.*)； 
        /// /// 如果想获得.ICO文件所表示的图标，则必须是文件的完整路径。 
        /// /// /// 是否大图标 
        /// /// 文件的默认图标 
        public static Icon GetFileIcon(string fileName, bool largeIcon)
        {
            if(fileName.StartsWith("http:") || fileName.StartsWith("https:"))
            {
                fileName = Path.Combine(MyPath.Root,"icon.html");
            }
            SHFILEINFO info = new SHFILEINFO(true);
            int cbFileInfo = Marshal.SizeOf(info);
            SHGFI flags;
            if (largeIcon)
                flags = SHGFI.Icon | SHGFI.LargeIcon ;
            else flags = SHGFI.Icon | SHGFI.SmallIcon ;
            SHGetFileInfo(fileName, 256, out info, (uint)cbFileInfo, flags);
            return Icon.FromHandle(info.hIcon);
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

        public static string ShortTxt(string txt)
        {
            int MAXLEN = 48;
            int PRELEN = 5;
            if (txt.Length > MAXLEN)
            {
                txt = txt.Substring(0, PRELEN) + " ... " + txt.Substring(txt.Length - (MAXLEN - PRELEN));
            }
            return txt;
        }
    }
}
