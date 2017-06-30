namespace LuceneTest.TagMgr
{
    public class TagDBFactory
    {
        public static ITagDB CreateTagDB()
        {
            return new LuceneTagDB();
        }
    }
}
