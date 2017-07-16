using System.Collections.Generic;
using System.Windows;
using TagExplorer.TagMgr;
using AnyTag.UI;
using TagExplorer.Utils;
using TagExplorer.TagLayout;
using TagExplorer;
using LuceneTest.TagCanvas;

namespace AnyTagNet
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
        private List<UIElement> lines = null, allTxt = null;

        public void Layout(ITagDB db, string tag)
        {
            double Top = GConfig.FontSize * 1.5;
            double Left = 0;
            
            //计算布局信息
            GObj gobj = GObj.LayoutTag(tag, db,Top,Left);
            layoutSize.Height = gobj.OuterBox.Height + GConfig.LayoutYPadding;
            layoutSize.Width = gobj.OuterBox.Width + GConfig.LayoutXPadding;
            rootPos.X = gobj.Content.X;
            rootPos.Y = gobj.Content.Y;

            //根据布局信息，生成UIElement控件并对控件的Style进行渲染。
            IGObjCollection gcanvas = new GObjCollection();
            gcanvas.AddGObjs(gobj.GetAll());
            gcanvas.AddEdge(gobj.GetEdges());
            
            lines = gcanvas.GetAllLines();
            allTxt = gcanvas.GetAllTextBlocks();

            LRUTag.Ins.Add(tag);
            List<string> tags = LRUTag.Ins.GetTags();
            double top = 0, left = 0;
            for(int i = 0;i<tags.Count;i++)
            {
                TagBox box = GStyle.Apply(left, top, tags[i]);
                allTxt.Add(box);
                left += box.Width1;
                left += 10;
            }
            

        }
    }
}
