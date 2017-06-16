using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LuceneTest.TagLayout
{
    class TagLayoutFactory
    {
        public static ITagLayout CreateLayout()
        {
            return new TagLayout();
        }
    }
}
