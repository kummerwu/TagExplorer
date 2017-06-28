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
            IGLayoutResult result = new GLayoutResult();
            IRectLayoutCalc c = new RectlayoutCalcImpl();
            IEnumerable<GObj> objs = null;
            GObj gobj = null;


            IGObjCollection gcanvas = new GObjCollection();

            gobj = GObj.ParseOut(tag, null, null, db, result, 0, 0);
            gobj.AdjustXY(0, 0);
            objs = gobj.GetAll();
            gcanvas.AddGObjs(objs);
            gcanvas.AddEdge(result.GetEdges());
            layoutSize.Height = gobj.OuterBox.Height + 10;
            layoutSize.Width = gobj.OuterBox.Width + GConfig.XContentPadding;
            rootPos.X = gobj.Content.X;
            rootPos.Y = gobj.Content.Y;
            //scrollViewer.ScrollToHorizontalOffset(gobj.Content.X);
            //scrollViewer.ScrollToVerticalOffset(gobj.Content.Y);
            //return gcanvas;

            lines = gcanvas.GetAllLines();
            allTxt = gcanvas.GetAllTextBlocks();
            
        }
    }
}
