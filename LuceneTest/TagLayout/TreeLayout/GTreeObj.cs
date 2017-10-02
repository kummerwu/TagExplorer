using System;
using System.Collections.Generic;
using System.Windows;
using TagExplorer.TagLayout.LayoutCommon;
using TagExplorer.TagMgr;

namespace TagExplorer.TagLayout.TreeLayout
{
    class GTreeObj
    {
        public List<GTreeObj> children = new List<GTreeObj>();
        public Rect OutBox = new Rect();
        public GTagLable box = null;

        public static GTreeObj ExpandNode(string tag, int level, ITagDB db, double x, double y)
        {
            if (GTreeObjDB.Ins.Get(tag) != null) return GTreeObjDB.Ins.Get(tag);



            GTreeObj root = new GTreeObj();
            root.box = new GTagLable(level, tag);

            GTreeObjDB.Ins.Add(tag, root);

            List<string> children = db.QueryTagChildren(tag);
            List<Size> childrenSize = new List<Size>();
            double h = 0;
            double w = 0;
            foreach (string ctag in children)
            {
                GTreeObj child = ExpandNode(ctag, level + 1, db, x + root.box.InnerBox.Width, y + h);
                h += child.OutBox.Height;
                w = Math.Max(w, child.OutBox.Width);

                root.children.Add(child);
                GTreeObjDB.Ins.Lines.Add(new Tuple<GTreeObj, GTreeObj>(root, child));
            }
            root.OutBox.Width = w + root.box.InnerBox.Width;
            root.OutBox.Height = Math.Max(h, root.box.InnerBox.Height);
            root.OutBox.X = x;
            root.OutBox.Y = y + root.OutBox.Height / 2;

            return root;
        }
    }
}
