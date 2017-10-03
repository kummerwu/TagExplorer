using AnyTagNet;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Shapes;
using TagExplorer.TagLayout.LayoutCommon;
using TagExplorer.TagMgr;

namespace TagExplorer.TagLayout.TreeLayout
{
    class TreeLayoutImpl : ITagLayout
    {
        public Size Size
        {
            get { return root.OutBox.Size; }
        }

        public Point RootPos
        {
            get
            {
                return new Point(root.OutBox.Left, (root.OutBox.Top+root.OutBox.Height/2));
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
        private GTreeObj root = null;

        public List<TagBox> tags = null;
        public List<Line> lines = null;
        public void Layout(ITagDB db, string tag)
        {
            GTreeObjDB.Ins.Reset();
            tags = new List<TagBox>();
            this.db = db;
            root = GTreeObj.ExpandNode(tag, 0, db,0,0);
            tags = GTreeObjDB.Ins.GetAllTagBox();
            lines = GTreeObjDB.Ins.GetAllLines();

        }
        
        
    }
    
    
}
