using AnyTagNet;
using System;
using System.Windows;
using System.Windows.Media;
using TagExplorer.Utils;

namespace TagExplorer.TagLayout.LayoutCommon
{
    /// <summary>
    /// 根据配置的字体，大小，计算一个Tag所占区域的大小和颜色
    /// </summary>
    class GTagLable
    {
        public string Tag { get; private set; }
        public int Level { get; private set; }
        public int Distance { get; private set; } //本节点离根节点（树层次上的距离）
        public double FontSize { get; private set; }
        public Rect InnerBox;
        public Rect ColorBox;


        
        //只读属性
        public GTagLable(int d,string tag,double x,double y)
        {
            Init(d, tag,x,y);
        }
        
        double InnerBoxXPadding;// GConfig.InnerBoxXPadding_MAX;
        double InnerBoxYPadding;// GConfig.InnerBoxYPadding_MAX;

        
        
        public TagBox ToTagBox(double x,double y)
        {
            return GStyle.Apply(Level, Distance, x, y, Tag);
        }
        public TagBox ToTagBox()
        {
            return GStyle.Apply(Level, Distance, ColorBox.X, ColorBox.Y, Tag);
        }
        //计算InnerBox（自身内容（文本着色区域）+Padding）的大小
        private void Init(int Distance, string Tag,double x,double y)
        {
            ColorBox = new Rect();
            InnerBox = new Rect();
            this.Tag = Tag;
            this.Distance = Distance;
            this.Level = Distance;
            
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

            CalcColorBoxSize(Tag,  CfgTagGraph.GFontName,x,y);
            InnerBox.X = x;
            InnerBox.Y = y;
            InnerBox.Width = ColorBox.Width + InnerBoxXPadding;
            InnerBox.Height = ColorBox.Height + InnerBoxYPadding;
            return ;
        }
        //计算自身内容所占区域的大小（文本和着色区域）
        private void CalcColorBoxSize(string text,  string fontFamily,double x,double y)
        {

            FormattedText formattedText = new FormattedText(
                text,
                System.Globalization.CultureInfo.InvariantCulture,
                FlowDirection.LeftToRight,
                new Typeface(fontFamily.ToString()),
                FontSize,
                Brushes.Black
            );
            Size tmp = new Size(formattedText.WidthIncludingTrailingWhitespace + CfgTagGraph.XContentPadding,
                                    formattedText.Height + CfgTagGraph.YContentPadding);
            ColorBox.X = x + CfgTagGraph.XContentPadding;
            ColorBox.Y = y + CfgTagGraph.YContentPadding;
            ColorBox.Width = tmp.Width;
            ColorBox.Height = tmp.Height;
        }
        

       

        /// <summary>
        /// 这个函数放在这儿不合适，需要优化 TODO
        /// </summary>
        /// <param name="OuterBox"></param>
        /// <param name="ParentBox"></param>
        public void CalcInnerBoxPos(Rect OuterBox, Size ParentBox)
        {
            InnerBox.X = OuterBox.X + (OuterBox.Width - InnerBox.Width) / 2;
            InnerBox.Y = OuterBox.Y + ParentBox.Height;
            ColorBox.X = InnerBox.X + InnerBoxXPadding;
            ColorBox.Y = InnerBox.Y + InnerBoxYPadding;
        }

       
    }
}
