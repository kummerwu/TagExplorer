using System.Collections.Generic;
using System.Windows;
using LuceneTest.TagMgr;
using AnyTag.UI;
using AnyTagNet;

namespace LuceneTest.TagLayout
{
    class TagLayout : ITagLayout
    {
        public IEnumerable<UIElement> Lines
        {
            get
            {
                return lines;
            }
        }

        public Point RootPos
        {
            get
            {
                return rootPos;
            }
        }

        public Size Size
        {
            get
            {
                return layoutSize;
            }
        }

        public IEnumerable<UIElement> TagArea
        {
            get
            {
                return allTxt;
            }
        }


        private Size layoutSize = new Size();
        private Point rootPos = new Point();
        private IEnumerable<UIElement> lines = null, allTxt = null;

        public void Layout(ITagDB db, string tag)
        {
            //计算布局信息
            GObj gobj = GObj.LayoutTag(tag, db);
            layoutSize.Height = gobj.OuterBox.Height + 10;
            layoutSize.Width = gobj.OuterBox.Width + GConfig.XContentPadding;
            rootPos.X = gobj.Content.X;
            rootPos.Y = gobj.Content.Y;

            //根据布局信息，生成UIElement控件并对控件的Style进行渲染。
            IGObjCollection gcanvas = new GObjCollection();
            gcanvas.AddGObjs(gobj.GetAll());
            gcanvas.AddEdge(gobj.GetEdges());
            
            lines = gcanvas.GetAllLines();
            allTxt = gcanvas.GetAllTextBlocks();
            
        }
    }
}
