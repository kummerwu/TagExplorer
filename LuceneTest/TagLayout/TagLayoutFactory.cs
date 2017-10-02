namespace TagExplorer.TagLayout
{
    public class TagLayoutFactory
    {
        public static ITagLayout CreateLayout()
        {
            return new TagExplorer.BoxLayout.TagLayout();
            //return new TreeLayout.TreeLayoutImpl(); 
        }
    }
}
