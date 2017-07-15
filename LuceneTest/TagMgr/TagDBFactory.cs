using LuceneTest.UriMgr;
using System.Collections.Generic;

namespace LuceneTest.TagMgr
{
    public class TagDBFactory
    {
        public static ITagDB CreateTagDB()
        {
           return IDisposableFactory.New<ITagDB>( new LuceneTagDB());
        }
        
    }
}
