using TagExplorer.UriMgr;

namespace TagExplorer.TagMgr
{
    public class TagDBFactory
    {
        public static ITagDB Ins = null;
        public static ITagDB CreateTagDB()
        {
            Ins =  IDisposableFactory.New<ITagDB>( new LuceneTagDB());
            return Ins; 
        }
        
    }
}
