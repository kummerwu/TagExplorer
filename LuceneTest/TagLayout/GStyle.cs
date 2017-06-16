using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace AnyTagNet
{
    class GStyle
    {
        Color[] colors = new Color[] {
               // Color.FromRgb(255,0,0      ),
                Color.FromRgb(255,51,0     ),
                Color.FromRgb(255,102,0    ),
                Color.FromRgb(255,153,0    ),
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
        public Color GetColor(int i)
        {
            i = i % colors.Length;
            i += colors.Length;
            i = i % colors.Length;
            return colors[i];
        } 
        public void Apply(GObj g,TextBlock b)
        {
            b.FontFamily = GConfig.GFontF;
            b.FontSize = g.FontSize;
            b.Height = g.Content.Height;
            b.Width = g.Content.Width;
            b.Margin = new Thickness(g.Content.X, g.Content.Y, 0, 0);
            b.TextAlignment = TextAlignment.Center;
            b.Text = g.Tag ;
            b.Background = new SolidColorBrush(GetColor(g.Distance));
            
        }


        public void ApplyLine(GObj parent, GObj child, Line l)
        {
            if (Math.Min(parent.Level, child.Level) == 0)
            {
                l.Stroke = new SolidColorBrush(GetColor(parent.Distance));
                l.StrokeThickness = GConfig.StrokeThickness*1.5;
                l.StrokeDashArray = GConfig.StrokeDashArray;
            }
            else
            {
                l.Stroke = new SolidColorBrush(GetColor(parent.Distance));
                l.StrokeThickness = GConfig.StrokeThickness;
                l.StrokeDashArray = GConfig.StrokeDashArray;
            }
        }
    }
}
