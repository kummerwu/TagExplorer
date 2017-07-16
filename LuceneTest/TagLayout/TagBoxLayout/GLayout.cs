using AnyTag.UI;
using TagExplorer;
using System.Collections;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Shapes;
using TagExplorer.Utils;

namespace AnyTagNet
{
    class GObjCollection : IGObjCollection
    {
        GStyle style = new GStyle();
        List<UIElement> allTxt = new List<UIElement>();
        List<UIElement> allEdge = new List<UIElement>();
        Hashtable gobjMaps = new Hashtable();
        public GObjCollection()
        {
        }

        
        public void AddGObjs(IEnumerable<GObj> all)
        {
            foreach (GObj g in all)
            {
                if (gobjMaps[g.Tag] == null)
                {
                    gobjMaps.Add(g.Tag, g);
                    TagBox b = new TagBox();
                    style.Apply(g, b);
                    allTxt.Add(b);
                }
            }
        }
        public void AddEdge(IEnumerable<PathEdge> edge)
        {
            foreach (PathEdge p in edge)
            {
                GObj parent = gobjMaps[p.Parent] as GObj;
                GObj child = gobjMaps[p.Child] as GObj;
                if (parent != null && child != null)
                {
                    Line l = new Line();
                    l.X1 = parent.Content.X + parent.Content.Width / 2;
                    l.Y1 = parent.Content.Y + parent.Content.Height;
                    l.X2 = child.Content.X + child.Content.Width / 2;
                    l.Y2 = child.Content.Y;
                    style.ApplyLine(parent, child, l);

                    allEdge.Add(l);
                    l.Tag = parent.Tag.ToString()+GConfig.ParentChildSplit+child.Tag.ToString();
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
