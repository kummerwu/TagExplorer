using System.Windows;
using System.Windows.Media;

namespace TagExplorer.Utils
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
        public static Size Radio = new Size(40, 40);
        public static double InnerBoxXPadding_MAX = 30;
        public static double InnerBoxYPadding_MAX = 24;
        public static double InnerBoxXPadding_MIN = 20;
        public static double InnerBoxYPadding_MIN = 18;
        public static double XContentPadding = 10;
        public static double YContentPadding = 6;
        public static double LayoutXPadding = 10;
        public static double LayoutYPadding = 10;
        //public static double LayoutInitWidth = 300;
        //public static double layoutInitHeight = 600;
        //每一层Tag显示大小缩放（底层显示更小一些）
        public static double ScaleInRadio = 1.2;

        public static string SpecialChar = "<>《》~?";

        //最小显示几层，最多几层，最少显示多少个tag
        public static int MinLevel = 4;
        public static int CurLevel = MinLevel;
        public static int MaxLevel = 8;
        public static int MinTagCnt = 80;
        
        //最后显示的Tag字符串
        public static string TitleDataFile = "title.dat";
        
        
    }
}
