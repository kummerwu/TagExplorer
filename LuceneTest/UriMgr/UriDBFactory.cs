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
