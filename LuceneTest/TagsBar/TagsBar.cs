using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace LuceneTest.TagsBar
{
    class TagsBar:ToolBar
    {
        public void AddTag(string tag)
        {
            Items.Add(tag);
        }
    }
}
