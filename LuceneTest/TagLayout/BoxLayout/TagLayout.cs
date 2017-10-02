using System.Collections.Generic;
using System.Windows;
using TagExplorer.TagMgr;
using TagExplorer.Utils;
using TagExplorer.TagLayout;

namespace TagExplorer.BoxLayout
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
        public IEnumerable<UIElement> TagArea
        {
            get
            {
                return allTxt;
            }
        }
        public IEnumerable<UIElement> RecentTagBox
        {
            get
            {
                return recentTags;
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

        


        private Size layoutSize = new Size();
        private Point rootPos = new Point();
        private List<UIElement> lines = null, allTxt = null,recentTags = null;

        public void Layout(ITagDB db, string tag)
        {
            double Top = 0;// GConfig.FontSize;
            double Left = 0;

            //计算布局信息
            GObj gobj = GObj.LayoutTag(tag, db, Top, Left);
            layoutSize.Height = gobj.OuterBox.Height + CfgTagGraph.LayoutYPadding + Top;
            layoutSize.Width = gobj.OuterBox.Width + CfgTagGraph.LayoutXPadding + Left;
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
