using TagExplorer;
using System;
using System.Windows;
using System.Windows.Media;
using System.Windows.Shapes;
using TagExplorer.Utils;
using TagExplorer.TagLayout.LayoutCommon;
using TagExplorer.BoxLayout;
using TagExplorer.TagLayout.TreeLayout;

namespace AnyTagNet
{

    class UIElementFactory
    {
        public static TagBox CreateTagBox(GTagBox g)
        {
            TagBox b = new TagBox(g);
            b.FontFamily = CfgTagGraph.GFontF;
            b.FontSize = g.FontSize;
            b.Height1 = g.InnerBox.Height;
            b.Width1 = g.InnerBox.Width;
            b.Margin = new Thickness(g.InnerBox.X-10, g.InnerBox.Y, 0, 0);
            b.TextAlignment = TextAlignment.Center;
            b.Text = g.Tag;
            b.Background1 = g.Distance;//new SolidColorBrush(GetColor(g.Distance,g.Level));
            if (g.Distance >= 5) b.Foreground1 = g.Distance;// new SolidColorBrush(Colors.White);
            return b;
        }


        public static Line CreateLine(GBoxObj parent, GBoxObj child)
        {
            Line l = new Line();
            l.X1 = parent.ColorBox.X + parent.ColorBox.Width / 2;
            l.Y1 = parent.ColorBox.Y + parent.ColorBox.Height;
            l.X2 = child.ColorBox.X + child.ColorBox.Width / 2;
            l.Y2 = child.ColorBox.Y;
            l.Tag = parent.Tag.ToString() + CfgTagGraph.ParentChildSplit + child.Tag.ToString();
            if (Math.Min(parent.GTagBox.Level, child.GTagBox.Level) == 0)
            {
                l.Stroke = new SolidColorBrush(GetColor(parent.GTagBox.Distance + 1, parent.GTagBox.Level + 1));
                l.StrokeThickness = CfgTagGraph.StrokeThickness * 1.5;
                l.StrokeDashArray = CfgTagGraph.StrokeDashArray;
            }
            else
            {
                l.Stroke = new SolidColorBrush(GetColor(parent.GTagBox.Distance + 1, parent.GTagBox.Level + 1));
                l.StrokeThickness = CfgTagGraph.StrokeThickness;
                l.StrokeDashArray = CfgTagGraph.StrokeDashArray;
            }
            return l;
        }
        public static Path CreateBezier(Tuple<GTagBoxTree, GTagBoxTree, int> p_c)
        {
            GTagBoxTree p = p_c.Item1;
            GTagBoxTree c = p_c.Item2;
            int direct = (int)p_c.Item3;
            System.Windows.Point p1 = new System.Windows.Point();
            System.Windows.Point p5 = new System.Windows.Point();
            System.Windows.Point p2 = new System.Windows.Point();
            System.Windows.Point p4 = new System.Windows.Point();
            System.Windows.Point p3 = new System.Windows.Point();

            if (direct == 1)
            {
                p1.X = p.GTagBox.InnerBox.Right;
                p5.X = c.GTagBox.InnerBox.Left;
            }
            else
            {
                p1.X = p.GTagBox.InnerBox.Left;
                p5.X = c.GTagBox.InnerBox.Right;
            }
            p1.Y = (p.GTagBox.InnerBox.Top + p.GTagBox.InnerBox.Bottom) / 2;
            p5.Y = (c.GTagBox.InnerBox.Top + c.GTagBox.InnerBox.Bottom) / 2;


            p3.X = (p1.X + p5.X) / 2;
            p3.Y = (p1.Y + p5.Y) / 2;

            const int N = 3;
            p2.X = (p1.X * N + p5.X) / (N + 1);
            p4.X = (p1.X + p5.X * N) / (N + 1);

            p2.Y = p1.Y;
            p4.Y = p5.Y;






            BezierSegment b1 = new BezierSegment(p1, p2, p3, true);
            b1.IsSmoothJoin = true;
            BezierSegment b2 = new BezierSegment(p3, p4, p5, true);
            b2.IsSmoothJoin = true;


            Path myPath = new Path();
            SetBezierStyle(p.GTagBox, c.GTagBox, myPath);
            PathGeometry pg = new PathGeometry();

            PathFigure pf = new PathFigure();

            //Path
            myPath.Data = pg;
            //PG
            pg.Figures = new PathFigureCollection();
            pg.Figures.Add(pf);
            //PF
            pf.StartPoint = p1;
            pf.Segments.Add(b1);
            pf.Segments.Add(b2);
            pg.Figures.Add(pf);


            myPath.Data = pg;

            return myPath;
        }


        /*************************************************************************************/
        private UIElementFactory() { }
        
        
        public static Color GetColor(int distance,int level)
        {
            Color[] FColor = AppCfg.Ins.TagBoxBackColor;
            int i = distance;
            if(level==-1)
            {
                i = 0;
            }
            i = i % FColor.Length;
            i += FColor.Length;
            i = i % FColor.Length;
            return FColor[i];
        }
        public static Color GetForeColor(int distance, int level)
        {
            Color[] FColor = AppCfg.Ins.TagBoxForeColor;
            int i = distance;
            if (level == -1)
            {
                i = 0;
            }
            i = i % FColor.Length;
            i += FColor.Length;
            i = i % FColor.Length;
            return FColor[i];
        }






        /// <summary>

        /// </summary>
        /// <param name="parent"></param>
        /// <param name="child"></param>
        /// <param name="l"></param>
        private static void SetBezierStyle(GTagBox parent, GTagBox child, Path l)
        {
            if (Math.Min(parent.Level, child.Level) == 0)
            {
                l.Stroke = new SolidColorBrush(GetColor(parent.Distance+1, parent.Level+1));
                l.StrokeThickness = CfgTagGraph.StrokeThickness * 1.5;
                //l.StrokeDashArray = CfgTagGraph.StrokeDashArray;
            }
            else
            {
                l.Stroke = new SolidColorBrush(GetColor(parent.Distance+1, parent.Level+1));
                l.StrokeThickness = CfgTagGraph.StrokeThickness;
                //l.StrokeDashArray = CfgTagGraph.StrokeDashArray;
            }
        }
    }
}
