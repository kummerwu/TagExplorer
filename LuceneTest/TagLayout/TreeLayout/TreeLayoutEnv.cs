using AnyTagNet;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Shapes;
using TagExplorer.TagLayout.LayoutCommon;

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
        private List<Tuple<GTagBoxTree, GTagBoxTree, int>> Lines = new List<Tuple<GTagBoxTree, GTagBoxTree, int>>();
        public void AddLine(GTagBoxTree parent, GTagBoxTree child, int direct)
        {
            Lines.Add(new Tuple<GTagBoxTree, GTagBoxTree, int>(parent, child, direct));
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

        // ////
        private List<TagBox> reuseList = new List<TagBox>();
        public void Return(List<TagBox> lst)
        {
            retTag += lst.Count;
            reuseList.AddRange(lst);
        }
        public int newTag = 0;
        public int retTag = 0;
        public int reuseTag = 0;
        public TagBox New(GTagBox g)
        {
            if(reuseList.Count>0)
            {
                reuseTag++;
                TagBox b = reuseList[0] as TagBox;
                reuseList.RemoveAt(0);
                return b;
            }
            else
            {
                newTag++;
                return new TagBox(g);
            }
        }
        ///////////////
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
