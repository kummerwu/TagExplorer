using LuceneTest;
using System;
using System.Windows;
using System.Windows.Media;
using System.Windows.Shapes;

namespace AnyTagNet
{
    class GStyle
    {
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

        };
        Color[] colors = colors3;
        public Color GetColor(int distance,int level)
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
        public void Apply(GObj g,TagBox b)
        {
            b.FontFamily = GConfig.GFontF;
            b.FontSize = g.FontSize;
            b.Height1 = g.Content.Height;
            b.Width1 = g.Content.Width;
            b.Margin = new Thickness(g.Content.X, g.Content.Y, 0, 0);
            b.TextAlignment = TextAlignment.Center;
            b.Text = g.Tag ;
            b.Background1 = new SolidColorBrush(GetColor(g.Distance,g.Level));
            
        }


        public void ApplyLine(GObj parent, GObj child, Line l)
        {
            if (Math.Min(parent.Level, child.Level) == 0)
            {
                l.Stroke = new SolidColorBrush(GetColor(parent.Distance,parent.Level));
                l.StrokeThickness = GConfig.StrokeThickness*1.5;
                l.StrokeDashArray = GConfig.StrokeDashArray;
            }
            else
            {
                l.Stroke = new SolidColorBrush(GetColor(parent.Distance,parent.Level));
                l.StrokeThickness = GConfig.StrokeThickness;
                l.StrokeDashArray = GConfig.StrokeDashArray;
            }
        }
    }
}
