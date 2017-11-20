using AnyTagNet;
using System;
using TagExplorer.TagLayout.CommonLayout;

namespace TagExplorer.TagLayout
{
    public class TagLayoutFactory
    {
        public static ITagLayout CreateLayout(LayoutMode mode)
        {
            if (mode == LayoutMode.GRAPH_UPDOWN)
            {
                throw new NotSupportedException("该布局模式已经被删除");
                //return new TagExplorer.BoxLayout.TagLayout();
            }
            else if(mode >= LayoutMode.LRTREE_NO_COMPACT && mode<= LayoutMode.LRTREE_COMPACT_MORE)
            {
                return new LRTreeLayout.LRTreeLayoutImpl(mode);
            }
            else
            { 
                return new TreeLayout.TreeLayoutImpl(mode);
            }
        }
    }
}
