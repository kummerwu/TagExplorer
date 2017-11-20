using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using TagExplorer.TagLayout.CommonLayout;
using TagExplorer.TagMgr;

namespace TagExplorer.TagLayout.TreeLayout
{
    class TagLayoutBase: ITagLayout
    {
        protected LayoutMode myLayoutMode;
        public TagLayoutBase(LayoutMode mode)
        {
            myLayoutMode = mode;
        }
        public Size Size
        {
            get { return root.TotalRange.Size; }
        }

        public Point RootPos
        {
            get
            {
                return new Point(root.TotalRange.Left, (root.TotalRange.Top + root.TotalRange.Height / 2));
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
        protected ITagDB db = null;
        protected GTagBoxTree root = null;
        protected Size oriSize;
        public List<TagBox> tags = null;
        public IEnumerable<UIElement> lines = null;

        public virtual void Layout(ITagDB db, GUTag root, Size size, TreeLayoutEnv env)
        {
            throw new NotImplementedException();
        }
    }
    class TreeLayoutImpl : TagLayoutBase
    {
        public TreeLayoutImpl(LayoutMode mode) : base(mode)
        {
        }

        public override void Layout(ITagDB db, GUTag tag,Size size,TreeLayoutEnv env)
        {
            oriSize = size;
            env.Reset();
            tags = new List<TagBox>();
            this.db = db;
            root = GTagBoxTree.ExpandNode(tag, 0, db,0,0,1,oriSize,env,myLayoutMode);
            tags = env.GetAllTagBox();
            lines = env.GetAllLines().Cast<UIElement>();

        }
        
        
    }
    
    
}
