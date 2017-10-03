using TagExplorer;
using System;
using System.Windows;
using System.Windows.Media;
using System.Windows.Shapes;
using TagExplorer.Utils;
using TagExplorer.TagLayout.LayoutCommon;

namespace AnyTagNet
{
    enum LAYOUT_COMPACT_MODE
    {
        TREE_NO_COMPACT,
        TREE_COMPACT,
        TREE_COMPACT_MORE,
        
        GRAPH_BEGIN,
    }
    class GStyle
    {
        private GStyle() { }

        public static LAYOUT_COMPACT_MODE mode = LAYOUT_COMPACT_MODE.TREE_COMPACT;
        //static enum COMPACT_MORE = true;//激进压缩模式，这样会在视觉上破坏层次关系
        //static bool COMPACT_MORE = true;//激进压缩模式，这样会在视觉上破坏层次关系

        static  Color[] colors1 = new Color[] {
               // Color.FromRgb(255,0,0      ),
                Color.FromRgb(255,51,0     ),
                //Color.FromRgb(255,102,0    ),
                //Color.FromRgb(255,153,0    ),
                Color.FromRgb(255,255,0    ),
                //Color.FromRgb(204,255,51),
                Color.FromRgb(153,255,0    ),
                Color.FromRgb(0,255,0      ),
                Color.FromRgb(0,255,255    ),
                Color.FromRgb(0,0,255      ),
                Color.FromRgb(102,0,255    ),
                Color.FromRgb(255,0,255    ),
                Color.FromRgb(255,0,102    ),
        };
        static Color[] colors2 = new Color[] {
            Color.FromRgb(255,0,0),
            //Color.FromRgb(255,255,0),
            Color.FromRgb(0,255,0),
            Color.FromRgb(0,255,255),
            Color.FromRgb(0,0,255),
            Color.FromRgb(255,0,255),

        };
        static Color C(int c) { return Color.FromRgb((byte)((c & 0xFF0000) >> 16), (byte)((c &0xFF00)>>8), (byte)(c &0xFF)); }
        static Color[] colors3 = new Color[] {
            C(0xFF6666),
            C(0x99CC00),
            C(0x99CCFF),
            C(0xFFFF33),
            C(0xFF99CC),
            C(0x9933FF),
            C(0x6699cc),

        };
        static Color[] colors = colors3;
        public static Color GetColor(int distance,int level)
        {
            int i = distance;
            if(level==-1)
            {
                i = 0;
            }
            i = i % colors.Length;
            i += colors.Length;
            i = i % colors.Length;
            return colors[i];
        } 
        public static void Apply(GTagLable g,TagBox b)
        {
            b.FontFamily = CfgTagGraph.GFontF;
            b.FontSize = g.FontSize;
            b.Height1 = g.ColorBox.Height;
            b.Width1 = g.ColorBox.Width;
            b.Margin = new Thickness(g.ColorBox.X, g.ColorBox.Y, 0, 0);
            b.TextAlignment = TextAlignment.Center;
            b.Text = g.Tag ;
            b.Background1 = new SolidColorBrush(GetColor(g.Distance,g.Level));
            
        }

        public static TagBox Apply(double x, double y, string text)
        {
            TagBox b =  Apply(4,4,x, y, text);
            b.Background1 = new SolidColorBrush(GetColor(6,6));
            b.Foreground1 = new SolidColorBrush(Colors.White);
            return b;
        }
        public static TagBox Apply(int distance,int level,double x,double y,string text)
        {
            TagBox b = new TagBox();
            b.FontFamily = CfgTagGraph.GFontF;
            double fontSize = CfgTagGraph.FontSize;
            for (int i = 0; i < distance; i++)
            {
                fontSize /= CfgTagGraph.ScaleInRadio;
            }
            b.FontSize = fontSize;

            FormattedText formattedText = new FormattedText(
               text,
               System.Globalization.CultureInfo.InvariantCulture,
               FlowDirection.LeftToRight,
               new Typeface(CfgTagGraph.GFontName),
               fontSize,
               Brushes.Black
           );

            b.Height1 = formattedText.Height + CfgTagGraph.YContentPadding; ;
            b.Width1 = formattedText.WidthIncludingTrailingWhitespace + CfgTagGraph.XContentPadding;
            b.Margin = new Thickness(x, y, 0, 0);
            b.TextAlignment = TextAlignment.Center;
            b.Text = text;
            b.Background1 = new SolidColorBrush(GetColor(distance, level));
            return b;
        }

        public static void ApplyLine(GTagLable parent, GTagLable child, Line l)
        {
            if (Math.Min(parent.Level, child.Level) == 0)
            {
                l.Stroke = new SolidColorBrush(GetColor(parent.Distance,parent.Level));
                l.StrokeThickness = CfgTagGraph.StrokeThickness*1.5;
                l.StrokeDashArray = CfgTagGraph.StrokeDashArray;
            }
            else
            {
                l.Stroke = new SolidColorBrush(GetColor(parent.Distance,parent.Level));
                l.StrokeThickness = CfgTagGraph.StrokeThickness;
                l.StrokeDashArray = CfgTagGraph.StrokeDashArray;
            }
        }

        public static void ApplyLine(GTagLable parent, GTagLable child, Path l)
        {
            if (Math.Min(parent.Level, child.Level) == 0)
            {
                l.Stroke = new SolidColorBrush(GetColor(parent.Distance, parent.Level));
                l.StrokeThickness = CfgTagGraph.StrokeThickness * 1.5;
                //l.StrokeDashArray = CfgTagGraph.StrokeDashArray;
            }
            else
            {
                l.Stroke = new SolidColorBrush(GetColor(parent.Distance, parent.Level));
                l.StrokeThickness = CfgTagGraph.StrokeThickness;
                //l.StrokeDashArray = CfgTagGraph.StrokeDashArray;
            }
        }
    }
}
