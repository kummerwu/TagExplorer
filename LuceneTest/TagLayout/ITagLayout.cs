using LuceneTest.TagMgr;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace LuceneTest.TagLayout
{
    interface ITagLayout
    {
        void Layout(ITagDB db, string root);
        Size Size { get; }
        Point RootPos { get; }
        IEnumerable<UIElement> Lines { get; }
        IEnumerable<UIElement> TagArea { get; }
    }
}
