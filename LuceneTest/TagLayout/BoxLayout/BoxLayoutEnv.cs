using TagExplorer;
using System.Collections;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Shapes;
using TagExplorer.Utils;
using AnyTagNet;

namespace TagExplorer.BoxLayout
{
    internal class BoxLayoutEnv : IBoxLayoutEnv
    {
        //GStyle style = new GStyle();
        List<UIElement> allTxt = new List<UIElement>();
        List<UIElement> allEdge = new List<UIElement>();
        Hashtable gobjMaps = new Hashtable();
        public BoxLayoutEnv()
        {
        }

        
        public void AddGObjs(IEnumerable<GBoxObj> all)
        {
            foreach (GBoxObj g in all)
            {
                if (gobjMaps[g.Tag] == null)
                {
                    gobjMaps.Add(g.Tag, g);
                    TagBox b = UIElementFactory.CreateTagBox(g.GTagBox,null);
                    allTxt.Add(b);
                }
            }
        }
        public void AddEdge(IEnumerable<PathEdge> edge)
        {
            foreach (PathEdge p in edge)
            {
                GBoxObj parent = gobjMaps[p.Parent] as GBoxObj;
                GBoxObj child = gobjMaps[p.Child] as GBoxObj;
                if (parent != null && child != null)
                {
                    allEdge.Add(UIElementFactory.CreateLine(parent,child));
                }
            }
        }


        public List<UIElement> GetAllTextBlocks()
        {
            return allTxt;
        }

        public List<UIElement> GetAllLines()
        {
            return allEdge;
        }
    }
}
