using TagExplorer.TagMgr;
using System.Collections.Generic;
using System.Windows;
using TagExplorer.TagLayout.TreeLayout;

namespace TagExplorer.TagLayout
{
    public interface ITagLayout
    {
        void Layout(ITagDB db, string root,Size size,TreeLayoutEnv env);
        Size Size { get; }
        Point RootPos { get; }
        IEnumerable<UIElement> Lines { get; }
        IEnumerable<UIElement> TagArea { get; }
    }
}
