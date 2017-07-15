using TagExplorer.UriMgr;

namespace TagExplorer.TagMgr
{
    public class TagDBFactory
    {
        public static ITagDB CreateTagDB()
        {
           return IDisposableFactory.New<ITagDB>( new LuceneTagDB());
        }
        
    }
}
