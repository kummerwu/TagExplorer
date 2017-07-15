namespace TagExplorer.UriMgr
{
    public class UriDBFactory
    {
        public static IUriDB CreateUriDB()
        {
            return IDisposableFactory.New<IUriDB>(new LuceneUriDB());
        }
    }
}
