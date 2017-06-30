namespace LuceneTest.UriMgr
{
    public class UriDBFactory
    {
        public static IUriDB CreateUriDB()
        {
            return new LuceneUriDB();
        }
    }
}
