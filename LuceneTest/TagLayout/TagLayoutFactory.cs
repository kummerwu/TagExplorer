using AnyTagNet;
using System;
using TagExplorer.TagLayout.CommonLayout;

namespace TagExplorer.TagLayout
{
    public class TagLayoutFactory
    {
        public static ITagLayout CreateLayout()
        {
            if (GLayoutMode.mode == LayoutMode.GRAPH_UPDOWN)
            {
                throw new NotSupportedException("该布局模式已经被删除");
                //return new TagExplorer.BoxLayout.TagLayout();
            }
            else if(GLayoutMode.mode >= LayoutMode.LRTREE_NO_COMPACT && GLayoutMode.mode<= LayoutMode.LRTREE_COMPACT_MORE)
            {
                return new LRTreeLayout.LRTreeLayoutImpl();
            }
            else
            { 
                return new TreeLayout.TreeLayoutImpl();
            }
        }
    }
}
