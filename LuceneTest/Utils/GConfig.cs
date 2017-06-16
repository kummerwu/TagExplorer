using AnyTags.Net;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

namespace AnyTagNet
{
    class GConfig
    {
        //在图中显示两个tag之间的边时，使用Split将
        public static char ParentChildSplit = '`';

        //显示字体
        public static string GFontName = @"Microsoft YaHei";
        public static FontFamily GFontF = new FontFamily(GFontName);
        public static double FontSize = 24;
        public static double MinFontSize = 8;

        //Tag有向图线的粗细
        public static double StrokeThickness = 1.2;
        public static DoubleCollection StrokeDashArray = new DoubleCollection() { 2, 2 };

        //Tag有向图的大致比例关系，边缘空白大小
        public static Size Radio = new Size(80, 40);
        public static double XPadding = 40;
        public static double YPadding = 40;
        public static double MinXPadding = 20;
        public static double MinYPadding = 20;
        public static double XContentPadding = 10;
        public static double YContentPadding = 0;

        //每一层Tag显示大小缩放（底层显示更小一些）
        public static double ScaleInRadio = 1.2;

        public static string SpecialChar = "<>《》~?";

        //最小显示几层，最多几层，最少显示多少个tag
        public static int MinLevel = 2;
        public static int CurLevel = MinLevel;
        public static int MaxLevel = 8;
        public static int MinTagCnt = 40;
        
        //最后显示的Tag字符串
        public static string TitleDataFile = "title.dat";
        
        public static string DefaultTag = "Brain";
    }
}
