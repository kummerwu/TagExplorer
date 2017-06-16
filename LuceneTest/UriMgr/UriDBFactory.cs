using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LuceneTest.UriMgr
{
    class UriDBFactory
    {
        public static IUriDB CreateUriDB()
        {
            return new LuceneUriDB();
        }
    }
}
