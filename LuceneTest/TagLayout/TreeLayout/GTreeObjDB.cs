using AnyTagNet;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
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
        public List<Tuple<GTreeObj, GTreeObj>> Lines = new List<Tuple<GTreeObj, GTreeObj>>();
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

        public List<Line> GetAllLines()
        {
            List<Line> result = new List<Line>();
            foreach (Tuple<GTreeObj, GTreeObj> p_c in GTreeObjDB.Ins.Lines)
            {
                GTreeObj p = p_c.Item1;
                GTreeObj c = p_c.Item2;
                Line l = new Line();
                l.X1 = p.box.ColorBox.Right;
                l.Y1 = (p.box.ColorBox.Top + p.box.ColorBox.Bottom)/ 2;
                l.X2 = c.box.ColorBox.Left;
                l.Y2 = (c.box.ColorBox.Top + c.box.ColorBox.Bottom) / 2;
                GStyle.ApplyLine(p.box, c.box, l);


                result.Add(l);
            }
            return result;
        }
    }
}
