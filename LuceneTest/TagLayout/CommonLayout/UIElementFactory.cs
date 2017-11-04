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

        public static TagBox CreateTagBox(GTagBox g,TreeLayoutEnv env)
        {
            TagBox b = (env==null)?new TagBox(g): env.New(g);
            
            b.FontFamily = StaticCfg.Ins.GFontF;
            b.FontSize = g.FontSize;
            b.Height1 = g.InnerBox.Height;
            b.Width1 = g.InnerBox.Width;
            b.Margin = new Thickness(g.InnerBox.X-10, g.InnerBox.Y, 0, 0);
            b.TextAlignment = TextAlignment.Center;
            b.GUTag = g.Tag;
            b.Background1 = g.Distance;//new SolidColorBrush(GetColor(g.Distance,g.Level));
            //if (g.Distance >= 5) b.Foreground1 = g.Distance;// new SolidColorBrush(Colors.White);
            b.Foreground1 = g.Distance;
            return b;
        }


        //public static Line CreateLine(GBoxObj parent, GBoxObj child)
        //{
        //    Line l = new Line();
        //    l.X1 = parent.ColorBox.X + parent.ColorBox.Width / 2;
        //    l.Y1 = parent.ColorBox.Y + parent.ColorBox.Height;
        //    l.X2 = child.ColorBox.X + child.ColorBox.Width / 2;
        //    l.Y2 = child.ColorBox.Y;
        //    l.Tag = parent.Tag.ToString() + StaticCfg.Ins.ParentChildSplit + child.Tag.ToString();
        //    if (Math.Min(parent.GTagBox.Level, child.GTagBox.Level) == 0)
        //    {
        //        l.Stroke = new SolidColorBrush(GetColor(parent.GTagBox.Distance + 1, parent.GTagBox.Level + 1));
        //        l.StrokeThickness = StaticCfg.Ins.StrokeThickness * 1.5;
        //        l.StrokeDashArray = StaticCfg.Ins.StrokeDashArray;
        //    }
        //    else
        //    {
        //        l.Stroke = new SolidColorBrush(GetColor(parent.GTagBox.Distance + 1, parent.GTagBox.Level + 1));
        //        l.StrokeThickness = StaticCfg.Ins.StrokeThickness;
        //        l.StrokeDashArray = StaticCfg.Ins.StrokeDashArray;
        //    }
        //    return l;
        //}
        public static Path CreateBezier(Tuple<GTagBoxTree, GTagBoxTree, int> p_c,TreeLayoutEnv env)
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



            Path path = env.New(p_c);
           

            if (path.Data == null)
            { 
                PathGeometry pg = new PathGeometry();
                PathFigureCollection pfc = new PathFigureCollection();
                PathFigure pf = new PathFigure();
                BezierSegment seg1 = new BezierSegment(p1, p2, p3, true);
                seg1.IsSmoothJoin = true;
                BezierSegment seg2 = new BezierSegment(p3, p4, p5, true);
                seg2.IsSmoothJoin = true;

                //Path->PathGeometry->PathFigureCollection->PathFigure->PathSegmentCollection->BezierSegment
                path.Data = pg;
                //PathGeometry
                pg.Figures = pfc;
                //PathFigureCollection
                pfc.Add(pf);
            
                //PF
                pf.StartPoint = p1;
                pf.Segments.Add(seg1);
                pf.Segments.Add(seg2);
                pg.Figures.Add(pf);


            }
            else
            {
                PathGeometry pg = path.Data as PathGeometry;
                PathFigureCollection pfc = pg.Figures as PathFigureCollection;
                PathFigure pf = pfc[0] as PathFigure;
                pf.StartPoint = p1;
                BezierSegment seg1 = pf.Segments[0] as BezierSegment;
                BezierSegment seg2 = pf.Segments[1] as BezierSegment;
                seg1.Point1 = p1;   seg1.Point2 = p2;   seg1.Point3 = p3;
                seg2.Point1 = p3;   seg2.Point2 = p4;   seg2.Point3 = p5;
                //pg.Figures.Add(pf);
            }
            SetBezierStyle(p.GTagBox, c.GTagBox, path);
            return path;
        }


        /*************************************************************************************/
        private UIElementFactory() { }
        
        
        public static Color GetColor(int distance,int level)
        {
            Color[] FColor = StaticCfg.Ins.TagBoxBackColor;
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
            Color[] FColor = StaticCfg.Ins.TagBoxForeColor;
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
                l.StrokeThickness = StaticCfg.Ins.StrokeThickness * 1.5;
                //l.StrokeDashArray = CfgTagGraph.Ins.StrokeDashArray;
            }
            else
            {
                l.Stroke = new SolidColorBrush(GetColor(parent.Distance+1, parent.Level+1));
                l.StrokeThickness = StaticCfg.Ins.StrokeThickness;
                //l.StrokeDashArray = CfgTagGraph.Ins.StrokeDashArray;
            }
        }
    }
}
