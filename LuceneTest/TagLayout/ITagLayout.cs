using LuceneTest.TagMgr;
using System.Collections.Generic;
using System.Windows;

namespace LuceneTest.TagLayout
{
    public interface ITagLayout
    {
        void Layout(ITagDB db, string root);
        Size Size { get; }
        Point RootPos { get; }
        IEnumerable<UIElement> Lines { get; }
        IEnumerable<UIElement> TagArea { get; }
    }
}
