namespace TagExplorer.UriMgr
{
    public class UriDBFactory
    {
        public static IUriDB CreateUriDB()
        {
            Ins = IDisposableFactory.New<IUriDB>(new LuceneUriDB());
            return Ins;
        }
        public static IUriDB Ins = null;
    }
}
