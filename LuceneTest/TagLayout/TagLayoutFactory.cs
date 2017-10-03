using AnyTagNet;

namespace TagExplorer.TagLayout
{
    public class TagLayoutFactory
    {
        public static ITagLayout CreateLayout()
        {
            if (GStyle.mode == LAYOUT_COMPACT_MODE.GRAPH_BEGIN)
            {
                return new TagExplorer.BoxLayout.TagLayout();
            }
            else
            { 
                return new TreeLayout.TreeLayoutImpl();
            }
        }
    }
}
