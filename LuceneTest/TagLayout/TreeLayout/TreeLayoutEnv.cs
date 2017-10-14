using AnyTagNet;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Shapes;

namespace TagExplorer.TagLayout.TreeLayout
{
    class TreeLayoutEnv
    {
        public void Reset()
        {
            all.Clear();
            Lines.Clear();
        }
        public IEnumerable<GTagBoxTree> All
        {
            get { return all.Values.Cast<GTagBoxTree>(); }
        }
        public Hashtable all = new Hashtable();
        public GTagBoxTree Get(string tag)
        {
            return all[tag] as GTagBoxTree;
        }
        public void Add(string tag, GTagBoxTree obj)
        {
            if (null == Get(tag))
            {
                all.Add(tag, obj);
            }
        }
        private List<Tuple<GTagBoxTree, GTagBoxTree,int>> Lines = new List<Tuple<GTagBoxTree, GTagBoxTree,int>>();
        public void AddLine(GTagBoxTree parent, GTagBoxTree child,int direct)
        {
            Lines.Add(new Tuple<GTagBoxTree, GTagBoxTree,int>(parent, child,direct));
        }
        private static TreeLayoutEnv ins = null;
        public static TreeLayoutEnv Ins
        {
            get
            {
                if (ins == null) ins = new TreeLayoutEnv();
                return ins;
            }
        }

        public List<TagBox> GetAllTagBox()
        {
            List<TagBox> result = new List<TagBox>();
            foreach (GTagBoxTree obj in TreeLayoutEnv.Ins.All)
            {
                result.Add(UIElementFactory.CreateTagBox(obj.GTagBox));
            }
            return result;
        }

        public List<Path> GetAllLines()
        {
            List<Path> result = new List<Path>();
            foreach (Tuple<GTagBoxTree, GTagBoxTree,int> p_c in TreeLayoutEnv.Ins.Lines)
            {
                result.Add(UIElementFactory.CreateBezier(p_c));
            }
            return result;
        }
    }
}
