using System.Collections.Generic;
using TagExplorer.UriMgr;

namespace TagExplorer.TagMgr
{
    public class TagDBFactory
    {
        public static ITagDB Ins = null;
        //public static ITagDB CreateTagDB()
        //{
        //    Ins = IDisposableFactory.New<ITagDB>(new LuceneTagDB());
        //    return Ins;
        //}
        public static ITagDB CreateTagDB()
        {
            Ins = IDisposableFactory.New<ITagDB>(SQLTagDB.Load());
            return Ins;
        }
        public static ITagDB CreateTagDB(string t)
        {
            if (t.Contains("json"))
            {
                Ins = IDisposableFactory.New<ITagDB>(JsonTagDB.Load());
            }
            else if(t.Contains("sql"))
            {
                Ins = IDisposableFactory.New<ITagDB>(SQLTagDB.Load());
            }
            return Ins;
        }
    }
}
