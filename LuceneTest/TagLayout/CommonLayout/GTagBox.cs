using AnyTagNet;
using System;
using System.Windows;
using System.Windows.Media;
using TagExplorer.Utils;

namespace TagExplorer.TagLayout.LayoutCommon
{
    public class TagBoxSizeInf
    {
        public TagBoxSizeInf(string tag, int dis,string fname)
        {
            Tag = tag;
            Distance = dis;
            PreCalcBoxSize();
            CalcBoxSize(fname);
        }
        public double FontSize { get; private set; }
        //public int Level { get; private set; }
        public int Distance { get; private set; } //本节点离根节点（树层次上的距离）
        public double InnerBoxXPadding;// GConfig.InnerBoxXPadding_MAX;
        public double InnerBoxYPadding;// GConfig.InnerBoxYPadding_MAX;
        public string Tag { get; private set; }
        public Size InnerBoxSize;
        public Size OutterBoxSize;
        private void PreCalcBoxSize()
        {
            FontSize = CfgTagGraph.FontSize;
            InnerBoxXPadding = CfgTagGraph.InnerBoxXPadding_MAX;
            InnerBoxYPadding = CfgTagGraph.InnerBoxYPadding_MAX;
            for (int i = 0; i < Distance; i++)
            {
                FontSize /= CfgTagGraph.ScaleInRadio;
                InnerBoxXPadding /= CfgTagGraph.ScaleInRadio;
                InnerBoxYPadding /= CfgTagGraph.ScaleInRadio;
            }

            InnerBoxXPadding = Math.Max(InnerBoxXPadding, CfgTagGraph.InnerBoxXPadding_MIN);
            InnerBoxYPadding = Math.Max(InnerBoxYPadding, CfgTagGraph.InnerBoxYPadding_MIN);
            FontSize = Math.Max(FontSize, CfgTagGraph.MinFontSize);
        }
        //计算自身内容所占区域的大小（文本和着色区域）
        private void CalcBoxSize(string fname)
        {
            FormattedText formattedText = new FormattedText(
                Tag,
                System.Globalization.CultureInfo.InvariantCulture,
                FlowDirection.LeftToRight,
                new Typeface(fname),
                FontSize,
                Brushes.Black
            );
            Size tmp = new Size(formattedText.WidthIncludingTrailingWhitespace + CfgTagGraph.XContentPadding,
                                    formattedText.Height + CfgTagGraph.YContentPadding);
            InnerBoxSize.Width = tmp.Width;
            InnerBoxSize.Height = tmp.Height;

            OutterBoxSize.Width = InnerBoxSize.Width + InnerBoxXPadding;
            OutterBoxSize.Height = InnerBoxSize.Height + InnerBoxYPadding;


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
            Inf = new TagBoxSizeInf(tag, distance, AppCfg.Ins.FontName);

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
        public void CenterItY(Rect outter)
        {
            //TextBox.X = outter.X + (outter.Width - TextBox.Width) / 2;
            OutterBoxLeftTop.Y = outter.Y + (outter.Height-Inf.OutterBoxSize.Height)/2;
            //ColorBox.X = outter.X + (outter.Width - ColorBox.Width) / 2;
            InnerBoxLeftTop.Y = outter.Y + (outter.Height - Inf.InnerBoxSize.Height) / 2;
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
