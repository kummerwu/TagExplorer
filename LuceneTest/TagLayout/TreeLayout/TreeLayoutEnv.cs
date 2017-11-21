using AnyTagNet;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Shapes;
using TagExplorer.TagLayout.LayoutCommon;
using TagExplorer.TagMgr;

namespace TagExplorer.TagLayout.TreeLayout
{
    public class TreeLayoutEnv
    {
        public Hashtable all = new Hashtable();


        public void Reset()
        {
            all.Clear();
            Lines.Clear();
        }
        public IEnumerable<GTagBoxTree> All
        {
            get { return all.Values.Cast<GTagBoxTree>(); }
        }
        
        public GTagBoxTree Get(GUTag tag)
        {
            return all[tag] as GTagBoxTree;
        }
        public void Add(GUTag tag, GTagBoxTree obj)
        {
            if (null == Get(tag))
            {
                all.Add(tag, obj);
            }
        }



        
        

        public static string StatInf {
            get {
                return "new reuse,ret tag = " + sttNewTag + " " + sttReuseTag + " " + sttRetTag
                    + "\r\n Line:new reuse ,ret = " + sttNewLine + " " + sttReuseLine + " " + sttRetLine;
                    ;
            }
        }

        #region TagBox Cache：由于WPF从Canvas中添加和删除UI元素性能比较低，为了优化，就不删除这些元素了，只是将这些元素置为不可见
        private List<TagBox> TagBoxGarbage = new List<TagBox>();
        private static int sttNewTag = 0;
        private static int sttRetTag = 0;
        private static int sttReuseTag = 0;


        public void Return(List<TagBox> lst)
        {
            sttRetTag += lst.Count;
            TagBoxGarbage.AddRange(lst);
        }
        public TagBox New(GTagBox g)
        {
            if(TagBoxGarbage.Count>0)
            {
                sttReuseTag++;
                TagBox b = TagBoxGarbage[0] as TagBox;
                TagBoxGarbage.RemoveAt(0);
                b.Visibility = System.Windows.Visibility.Visible;
                return b;
            }
            else
            {
                sttNewTag++;
                return new TagBox(g);
            }
        }

        public List<TagBox> GetAllTagBox()
        {
            List<TagBox> result = new List<TagBox>();
            //还处于垃圾箱中的TagBox，将其设置为不可见
            foreach (TagBox b in TagBoxGarbage)
            {
                b.Visibility = System.Windows.Visibility.Collapsed;
            }
            //对于每一个节点，创建TagBox（）
            foreach (GTagBoxTree obj in All)
            {
                result.Add(UIElementFactory.CreateTagBox(obj.GTagBox, this));
            }
            return result;
        }


        #endregion

        #region 节点之间的连线（B曲线）：由于WPF从Canvas中添加和删除UI元素性能比较低，为了优化，就不删除这些元素了，只是将这些元素置为不可见
        private List<Tuple<GTagBoxTree, GTagBoxTree, int>> Lines = new List<Tuple<GTagBoxTree, GTagBoxTree, int>>();
        public void AddLine(GTagBoxTree parent, GTagBoxTree child, int direct)
        {
            Lines.Add(new Tuple<GTagBoxTree, GTagBoxTree, int>(parent, child, direct));
        }
        ///////////////
        private static int sttNewLine = 0;
        private static int sttRetLine = 0;
        private static int sttReuseLine = 0;
        private List<Path> LineGarbage = new List<Path>();


        public void Return(List<Path> lst)
        {
            sttRetLine += lst.Count;
            LineGarbage.AddRange(lst);
        }
        
        public Path New(Tuple<GTagBoxTree, GTagBoxTree, int> p_c)
        {
            if (LineGarbage.Count > 0)
            {
                sttReuseLine++;
                Path b = LineGarbage[0] as Path;
                LineGarbage.RemoveAt(0);
                b.Visibility = System.Windows.Visibility.Visible;
                return b;
            }
            else
            {
                sttNewLine++;
                return new Path();
            }
        }
        public List<Path> GetAllLines()
        {
            List<Path> result = new List<Path>();
            foreach (Path b in LineGarbage)
            {
                b.Visibility = System.Windows.Visibility.Collapsed;
            }
            foreach (Tuple<GTagBoxTree, GTagBoxTree, int> p_c in Lines)
            {
                result.Add(UIElementFactory.CreateBezier(p_c, this));
            }
            return result;
        }
        #endregion

        // ///////////////////////////////





    }
}
