using System.Collections.Generic;
using System.Linq;
using System.Windows;
using TagExplorer.TagMgr;

namespace TagExplorer.TagLayout.TreeLayout
{
    class TreeLayoutImpl : ITagLayout
    {
        public Size Size
        {
            get { return root.TotalRange.Size; }
        }

        public Point RootPos
        {
            get
            {
                return new Point(root.TotalRange.Left, (root.TotalRange.Top+root.TotalRange.Height/2));
            }
        }

        public IEnumerable<UIElement> Lines
        {
            get { return lines; }
        }

        public IEnumerable<UIElement> TagArea
        {
            get { return tags; }
        }
        private ITagDB db = null;
        private GTagBoxTree root = null;
        private Size oriSize;
        public List<TagBox> tags = null;
        public IEnumerable<UIElement> lines = null;
        public void Layout(ITagDB db, string tag,Size size)
        {
            oriSize = size;
            TreeLayoutEnv.Ins.Reset();
            tags = new List<TagBox>();
            this.db = db;
            root = GTagBoxTree.ExpandNode(tag, 0, db,0,0,1);
            tags = TreeLayoutEnv.Ins.GetAllTagBox();
            lines = TreeLayoutEnv.Ins.GetAllLines().Cast<UIElement>();

        }
        
        
    }
    
    
}
