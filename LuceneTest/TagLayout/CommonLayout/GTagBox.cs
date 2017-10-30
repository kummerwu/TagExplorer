using AnyTagNet;
using System;
using System.Collections;
using System.Windows;
using System.Windows.Media;
using TagExplorer.Utils;

namespace TagExplorer.TagLayout.LayoutCommon
{
    public class TagBoxSizeInf
    {
        //公有成员变量************************************************************
        public Size InnerBoxSize;
        public Size OutterBoxSize;
        //公有成员变量(只读)******************************************************
        public double FontSize { get; private set; }
        public int Distance { get; private set; } //本节点离根节点（树层次上的距离）
        public string Tag { get; private set; }
        public double InnerBoxXPadding { get; private set; }// GConfig.InnerBoxXPadding_MAX;
        public double InnerBoxYPadding { get; private set; }// GConfig.InnerBoxYPadding_MAX;
        //私有成员变量************************************************************
        //公有成员方法************************************************************
        public TagBoxSizeInf(string tag, int dis,string fname)
        {
            Tag = tag;
            Distance = dis;
            PreCalcBoxSize();
            CalcBoxSize(fname);
        }
        
        //根据Distance计算字体大小和Padding大小
        private void PreCalcBoxSize()
        {
            FontSize = StaticCfg.Ins.FontSize;
            InnerBoxXPadding = StaticCfg.Ins.InnerBoxXPadding_MAX;
            InnerBoxYPadding = StaticCfg.Ins.InnerBoxYPadding_MAX;
            for (int i = 0; i < Distance; i++)
            {
                FontSize /= StaticCfg.Ins.ScaleInRadio;
                InnerBoxXPadding /= StaticCfg.Ins.ScaleInRadio;
                InnerBoxYPadding /= StaticCfg.Ins.ScaleInRadio;
            }

            InnerBoxXPadding = Math.Max(InnerBoxXPadding, StaticCfg.Ins.InnerBoxXPadding_MIN);
            InnerBoxYPadding = Math.Max(InnerBoxYPadding, StaticCfg.Ins.InnerBoxYPadding_MIN);
            FontSize = Math.Max(FontSize, StaticCfg.Ins.MinFontSize);
        }

        
        //计算自身内容所占区域的大小（文本和着色区域）
        private void CalcBoxSize(string fname)
        {
            Size tmp = FCalc.Ins.Calc(Tag, FontSize, fname);
            InnerBoxSize.Width = tmp.Width;
            InnerBoxSize.Height = tmp.Height;

            OutterBoxSize.Width = InnerBoxSize.Width + InnerBoxXPadding;
            OutterBoxSize.Height = InnerBoxSize.Height + InnerBoxYPadding;


        }
    }

    //文本显示宽度和高度的快速计算（用了cache加速计算）
    public class FCalc
    {
        private static FCalc ins;
        Hashtable cache = new Hashtable();
        public static FCalc Ins
        {
            get
            {
                if (ins == null)
                {
                    ins = new FCalc();
                }
                return ins;
            }
        }
        public Size Calc(string tag,double fsize,string fname)
        {
            Size tmp;
            if (cache[tag] == null)
            {
                //cache内容过多，直接将所有数据全部老化。
                if(cache.Keys.Count>2000)
                {
                    cache.Clear();
                }
                FormattedText formattedText = new FormattedText(
                    tag,
                    System.Globalization.CultureInfo.InvariantCulture,
                    FlowDirection.LeftToRight,
                    new Typeface(fname),
                    StaticCfg.Ins.FontSize,
                    Brushes.Black
                );
                tmp = new Size(formattedText.WidthIncludingTrailingWhitespace, formattedText.Height);
                cache.Add(tag, tmp); 

            }
            tmp = (Size)cache[tag] ;
            tmp.Width = tmp.Width * fsize / StaticCfg.Ins.FontSize + StaticCfg.Ins.XContentPadding;
            tmp.Height = tmp.Height * fsize / StaticCfg.Ins.FontSize + StaticCfg.Ins.YContentPadding;
            return tmp;
        }
    }
    /// <summary>
    /// 根据配置的字体，大小，计算一个Tag所占区域的大小和颜色
    /// </summary>
    public class GTagBox
    {


        public bool IsRoot = false;
        public Point OutterBoxLeftTop = new Point();          //内部文字区域，加上外部空白边缘
        public Point InnerBoxLeftTop = new Point();           //内部文字区域
        readonly TagBoxSizeInf Inf;
        readonly public int Direct;

        //计算只读属性
        public Rect OutterBox{get{return new Rect(OutterBoxLeftTop, Inf.OutterBoxSize);}}
        public Rect InnerBox{get{return new Rect(InnerBoxLeftTop, Inf.InnerBoxSize);}}
        public string Tag{get { return Inf.Tag; }}
        public double FontSize{get { return Inf.FontSize; }}
        public int Distance { get { return Inf.Distance; } }
        public int Level { get { return Inf.Distance; } }
        
        public GTagBox(int distance,string tag,double x,double y,int direct)
        {
            //记录展开方向（1：向右 -1：向左）
            Direct = direct;

            //计算大小
            Inf = new TagBoxSizeInf(tag, distance, StaticCfg.Ins.GFontName);

            //*****计算位置*****************
            //计算OutterBox
            OutterBoxLeftTop.X = x;
            OutterBoxLeftTop.Y = y;
            if (Direct == -1) OutterBoxLeftTop.X -= Inf.OutterBoxSize.Width;

            //计算InnerBox在：OutterBox的中间
            InnerBoxLeftTop.X = OutterBoxLeftTop.X + (Inf.OutterBoxSize.Width - Inf.InnerBoxSize.Width) / 2;
            InnerBoxLeftTop.Y = OutterBoxLeftTop.Y + (Inf.OutterBoxSize.Height - Inf.InnerBoxSize.Height) / 2; ;
        }
        
       //将本TextBox移到指定矩形的正中间（仅移动Y坐标）
        public void CenterRootY(Rect rect)
        {
            OutterBoxLeftTop.Y = rect.Y + (rect.Height-Inf.OutterBoxSize.Height)/2;
            InnerBoxLeftTop.Y = rect.Y + (rect.Height - Inf.InnerBoxSize.Height) / 2;
        }
        /// <summary>
        /// 这个函数放在这儿不合适，需要优化 TODO
        /// </summary>
        /// <param name="OuterBox"></param>
        /// <param name="ParentBox"></param>
        public void CalcInnerBoxPos(Rect OuterBox, Size ParentBox)
        {
            OutterBoxLeftTop.X = OuterBox.X + (OuterBox.Width - Inf.OutterBoxSize.Width) / 2;
            OutterBoxLeftTop.Y = OuterBox.Y + ParentBox.Height;
            InnerBoxLeftTop.X = OutterBoxLeftTop.X + Inf.InnerBoxXPadding;
            InnerBoxLeftTop.Y = OutterBoxLeftTop.Y + Inf.InnerBoxYPadding;
        }

        internal void Move(double dx, double dy)
        {
            OutterBoxLeftTop.X += dx;
            OutterBoxLeftTop.Y += dy;
            InnerBoxLeftTop.X += dx;
            InnerBoxLeftTop.Y += dy;

        }
    }
}
