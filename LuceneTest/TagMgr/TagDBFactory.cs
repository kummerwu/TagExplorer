using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LuceneTest.TagMgr
{
    class TagDBFactory
    {
        public static ITagDB CreateTagDB()
        {
            return new LuceneTagDB();
        }
    }
}
