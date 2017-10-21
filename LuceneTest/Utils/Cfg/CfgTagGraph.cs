using Newtonsoft.Json;
using System;
using System.IO;
using System.Windows;
using System.Windows.Media;

namespace TagExplorer.Utils
{
    class CfgTagGraph
    {
        public static CfgTagGraph ins = null;
        public static CfgTagGraph Ins
        {
            get
            {
                //先尝试从文件读取
                if(ins==null)
                {
                    try
                    {
                        if (File.Exists(CfgPath.CfgTagGraphFilePath))
                        {
                            string tmp = File.ReadAllText(CfgPath.CfgTagGraphFilePath);
                            ins = JsonConvert.DeserializeObject<CfgTagGraph>(tmp);
                        }
                    }catch(Exception e)
                    {
                        Logger.E(e);
                        ins = null;
                    }
                }
                //如果文件读取失败，尝试使用默认值
                if(ins == null)
                {
                    ins = new CfgTagGraph();//JsonConvert.DeserializeObject<CfgTagGraph>(App)
                    string tmp= JsonConvert.SerializeObject(ins,Formatting.Indented); //将默认值保存到json中去
                    File.WriteAllText(CfgPath.CfgTagGraphFilePath, tmp);
                }
                return ins;
            }
        }
        //在图中显示两个tag之间的边时，使用Split将
        public char ParentChildSplit = '`';
        public string SpecialChar = "<>《》~?";

        //显示字体
        public string GFontName = @"Microsoft YaHei";
        
        public double FontSize = 18;
        public double MinFontSize = 8;

        //Tag有向图线的粗细
        public double StrokeThickness = 1.2;
        public DoubleCollection StrokeDashArray = new DoubleCollection() { 2, 2 };

        //Tag有向图的大致比例关系，边缘空白大小
        public Size Radio = new Size(40, 40);
        //public static double InnerBoxXPadding_MAX = 30;
        //public static double InnerBoxYPadding_MAX = 24;
        //public static double InnerBoxXPadding_MIN = 20;
        //public static double InnerBoxYPadding_MIN = 18;
        //public static double XContentPadding = 10;
        //public static double YContentPadding = 6;
        //public static double LayoutXPadding = 10;
        //public static double LayoutYPadding = 10;

        public double RADIO = 0.5;
        public double InnerBoxXPadding_MAX = 30;
        public double InnerBoxYPadding_MAX = 24/2;
        public double InnerBoxXPadding_MIN = 20;
        public double InnerBoxYPadding_MIN = 18/2;
        public double XContentPadding = 10/2;
        public double YContentPadding = 6/2;
        public double LayoutXPadding = 10/2;
        public double LayoutYPadding = 10/2;




        //public static double LayoutInitWidth = 300;
        //public static double layoutInitHeight = 600;
        //每一层Tag显示大小缩放（底层显示更小一些）
        public double ScaleInRadio = 1.1;

        

        //最小显示几层，最多几层，最少显示多少个tag
        public int MinLevel = 4;
        public int CurLevel = 4;
        public int MaxLevel = 8;
        public int MinTagCnt = 80;

        //最后显示的Tag字符串
        //public static string TitleDataFile = "title.dat";
        
            //不需要保存的配置
        private FontFamily gfontf = null;
        [JsonIgnore]
        public FontFamily GFontF
        {
            get
            {
                if(gfontf==null)
                {
                    gfontf = new FontFamily(GFontName);
                }
                return gfontf;
            }
        }


    }
}
