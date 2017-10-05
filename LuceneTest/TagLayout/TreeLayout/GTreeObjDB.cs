using AnyTagNet;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using System.Windows.Shapes;

namespace TagExplorer.TagLayout.TreeLayout
{
    class GTreeObjDB
    {
        public void Reset()
        {
            all.Clear();
            Lines.Clear();
        }
        public IEnumerable<GTreeObj> All
        {
            get { return all.Values.Cast<GTreeObj>(); }
        }
        public Hashtable all = new Hashtable();
        public GTreeObj Get(string tag)
        {
            return all[tag] as GTreeObj;
        }
        public void Add(string tag, GTreeObj obj)
        {
            if (null == Get(tag))
            {
                all.Add(tag, obj);
            }
        }
        private List<Tuple<GTreeObj, GTreeObj>> Lines = new List<Tuple<GTreeObj, GTreeObj>>();
        public void AddLine(GTreeObj parent, GTreeObj child)
        {
            Lines.Add(new Tuple<GTreeObj, GTreeObj>(parent, child));
        }
        private static GTreeObjDB ins = null;
        public static GTreeObjDB Ins
        {
            get
            {
                if (ins == null) ins = new GTreeObjDB();
                return ins;
            }
        }

        public List<TagBox> GetAllTagBox()
        {
            List<TagBox> result = new List<TagBox>();
            foreach (GTreeObj obj in GTreeObjDB.Ins.All)
            {
                result.Add(obj.box.ToTagBox());
            }
            return result;
        }

        public List<Path> GetAllLines()
        {
            
            List<Path> result = new List<Path>();
            foreach (Tuple<GTreeObj, GTreeObj> p_c in GTreeObjDB.Ins.Lines)
            {
                GTreeObj p = p_c.Item1;
                GTreeObj c = p_c.Item2;
                System.Windows.Point p1 = new System.Windows.Point();
                System.Windows.Point p5 = new System.Windows.Point();
                System.Windows.Point p2 = new System.Windows.Point();
                System.Windows.Point p4 = new System.Windows.Point();
                System.Windows.Point p3 = new System.Windows.Point();

                p1.X = p.box.ColorBox.Right+5;
                p1.Y = (p.box.ColorBox.Top + p.box.ColorBox.Bottom)/ 2;

                p5.X = c.box.ColorBox.Left;
                p5.Y = (c.box.ColorBox.Top + c.box.ColorBox.Bottom) / 2;

                //方案1
                //p2.X = (p1.X + p5.X) / 2;
                //p4.X = (p1.X + p5.X) / 2;

                //p2.Y = p1.Y;
                //p4.Y = p5.Y;

                //p3.X = p2.X;
                //p3.Y = (p2.Y+p4.Y) / 2;


                //方案2
                p3.X = (p1.X + p5.X) / 2;
                p3.Y = (p1.Y + p5.Y) / 2;

                const int N = 3;
                p2.X = (p1.X*N + p5.X) / (N+1);
                p4.X = (p1.X + p5.X*N) / (N+1);

                p2.Y = p1.Y;
                p4.Y = p5.Y;






                BezierSegment b1 = new BezierSegment(p1, p2, p3, true);
                b1.IsSmoothJoin = true;
                BezierSegment b2 = new BezierSegment(p3, p4, p5, true);
                b2.IsSmoothJoin = true;


                Path myPath = new Path();
                GStyle.ApplyLine(p.box, c.box, myPath);
                //myPath.Stroke = System.Windows.Media.Brushes.Black;
                //myPath.Fill = System.Windows.Media.Brushes.MediumSlateBlue;
                //myPath.StrokeThickness = 1;
                //myPath.HorizontalAlignment = HorizontalAlignment.Left;
                //myPath.VerticalAlignment = VerticalAlignment.Center;
                //EllipseGeometry myEllipseGeometry = new EllipseGeometry();
                //myEllipseGeometry.Center = new System.Windows.Point(50, 50);
                //myEllipseGeometry.RadiusX = 25;
                //myEllipseGeometry.RadiusY = 25;

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
                
                //myGrid.Children.Add(myPath);


                result.Add(myPath);


                
            }
            return result;
        }
    }
}
