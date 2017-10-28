using AnyTagNet;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Shapes;
using TagExplorer.TagLayout.CommonLayout;
using TagExplorer.TagLayout.LayoutCommon;

namespace TagExplorer.TagLayout.TreeLayout
{
    public class TreeLayoutEnv
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
        //private static TreeLayoutEnv ins = null;
        //private static TreeLayoutEnv Ins
        //{
        //    get
        //    {
        //        if (ins == null) ins = new TreeLayoutEnv();
        //        return ins;
        //    }
        //}

        public static string StatInf {
            get {
                return "new reuse,ret tag = " + newTag + " " + reuseTag + " " + retTag
                    + "\r\n Line:new reuse ,ret = " + newLine + " " + reuseLine + " " + retLine;
                    ;
            }
        }

        // ////
        private List<TagBox> reuseList = new List<TagBox>();
        public void Return(List<TagBox> lst)
        {
            retTag += lst.Count;
            reuseList.AddRange(lst);
        }
        private static int newTag = 0;
        private static int retTag = 0;
        private static int reuseTag = 0;
        public TagBox New(GTagBox g)
        {
            if(reuseList.Count>0)
            {
                reuseTag++;
                TagBox b = reuseList[0] as TagBox;
                reuseList.RemoveAt(0);
                b.Visibility = System.Windows.Visibility.Visible;
                return b;
            }
            else
            {
                newTag++;
                return new TagBox(g);
            }
        }
        ///////////////
        private static int newLine = 0;
        private static int retLine = 0;
        private static int reuseLine = 0;

        private List<Path> reuseLineList = new List<Path>();
        public void Return(List<Path> lst)
        {
            retLine += lst.Count;
            reuseLineList.AddRange(lst);
        }
        
        public Path New(Tuple<GTagBoxTree, GTagBoxTree, int> p_c)
        {
            if (reuseLineList.Count > 0)
            {
                reuseLine++;
                Path b = reuseLineList[0] as Path;
                reuseLineList.RemoveAt(0);
                b.Visibility = System.Windows.Visibility.Visible;
                return b;
            }
            else
            {
                newLine++;
                return new Path();
            }
        }

        
        // ///////////////////////////////
        public List<TagBox> GetAllTagBox()
        {
            List<TagBox> result = new List<TagBox>();
            foreach(TagBox b in reuseList)
            {
                b.Visibility = System.Windows.Visibility.Collapsed;
            }
            foreach (GTagBoxTree obj in All)
            {
                result.Add(UIElementFactory.CreateTagBox(obj.GTagBox,this));
            }
            return result;
        }

        public List<Path> GetAllLines()
        {
            List<Path> result = new List<Path>();
            foreach (Path b in reuseLineList)
            {
                b.Visibility = System.Windows.Visibility.Collapsed;
            }
            foreach (Tuple<GTagBoxTree, GTagBoxTree,int> p_c in Lines)
            {
                result.Add(UIElementFactory.CreateBezier(p_c,this));
            }
            return result;
        }

        
    }
}
