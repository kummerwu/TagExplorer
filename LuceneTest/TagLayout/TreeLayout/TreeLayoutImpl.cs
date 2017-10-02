using AnyTagNet;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Shapes;
using TagExplorer.TagLayout.LayoutCommon;
using TagExplorer.TagMgr;

namespace TagExplorer.TagLayout.TreeLayout
{
    class TreeLayoutImpl : ITagLayout
    {
        public Size Size
        {
            get { return root.OutBox.Size; }
        }

        public Point RootPos
        {
            get
            {
                return new Point(root.OutBox.Left, (root.OutBox.Top+root.OutBox.Height/2));
            }
        }

        public IEnumerable<UIElement> Lines
        {
            get { return lines; }
        }

        public IEnumerable<UIElement> TagArea
        {
            get { return tags; }
        }
        private ITagDB db = null;
        private GTreeObj root = null;

        public List<TagBox> tags = new List<TagBox>();
        public List<Line> lines = new List<Line>();
        public void Layout(ITagDB db, string tag)
        {
            GTreeObjDB.Ins.Reset();
            tags = new List<TagBox>();
            this.db = db;
            root = GTreeObj.ExpandNode(tag, 0, db,0,0);
            foreach(GTreeObj obj in GTreeObjDB.Ins.All)
            {
                tags.Add(obj.box.ToTagBox(obj.OutBox.X,obj.OutBox.Y));
            }
            foreach(Tuple<GTreeObj ,GTreeObj> p_c in GTreeObjDB.Ins.Lines)
            {
                GTreeObj p = p_c.Item1;
                GTreeObj c = p_c.Item2;
                Line l = new Line();
                l.X1 = p.OutBox.X + p.box.ColorBox.Width ;
                l.Y1 = p.OutBox.Y + p.box.ColorBox.Height/2;
                l.X2 = c.OutBox.X ;
                l.Y2 = c.OutBox.Y + c.box.ColorBox.Height/2;
                GStyle.ApplyLine(p.box, c.box, l);


                lines.Add(l);
            }

        }
        
        
    }
    
    
}
