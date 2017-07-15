namespace TagExplorer.TagLayout
{
    public class TagLayoutFactory
    {
        public static ITagLayout CreateLayout()
        {
            return new AnyTagNet.TagLayout();
        }
    }
}
