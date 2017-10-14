using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TagExplorer.TagLayout.CommonLayout
{
    //布局模式
    public enum LayoutMode
    {
        //左右对称的树形布局模式
        LRTREE_NO_COMPACT = 0,
        LRTREE_COMPACT,
        LRTREE_COMPACT_MORE,

        //向右伸展的树形布局模式
        TREE_NO_COMPACT,
        TREE_COMPACT,
        TREE_COMPACT_MORE,

        //从上往下的图形布局
        GRAPH_UPDOWN,
    }

    //当前的布局模式选择
    public class GLayoutMode
    {
        public static LayoutMode mode = LayoutMode.LRTREE_COMPACT;
    }
}
