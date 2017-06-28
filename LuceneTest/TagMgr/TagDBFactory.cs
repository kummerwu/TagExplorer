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
