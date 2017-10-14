﻿using AnyTagNet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using TagExplorer.TagLayout.LayoutCommon;
using TagExplorer.TagLayout.TreeLayout;
using TagExplorer.TagMgr;
using TagExplorer.Utils;

namespace TagExplorer.TagLayout.LRTreeLayout
{
    class LRTreeLayoutImpl : ITagLayout
    {
        private Rect outterbox;
        
        public Size Size
        {
            get { return outterbox.Size; }
        }
        
        public Point RootPos
        {
            get
            {
                return new Point(outterbox.Left, (outterbox.Top + outterbox.Height / 2));
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

        private Size oriSize;
        public List<TagBox> tags = null;
        public IEnumerable<UIElement> lines = null;
        public void Layout(ITagDB db, string tag,Size size)
        {
            oriSize = size;
            GTagBoxTree subTree = null;
            double y = 0;
            List<string> allChild = db.QueryTagChildren(tag);
            
            TreeLayoutEnv.Ins.Reset();
            tags = new List<TagBox>();
            this.db = db;
            
            //double x = 600;
            double centerX = size.Width / 2;
            

            GTagBoxTree root = new GTagBoxTree();
            //TagBox box = UIElementFactory.CreateTagBox(tagbox);
            root.GTagBox = new GTagBox(0, tag, centerX, 0, 1);
            TreeLayoutEnv.Ins.Add(tag, root);
            double l, r;
            l = root.GTagBox.InnerBoxLeftTop.X - CfgTagGraph.LayoutXPadding*5;
            r = root.GTagBox.InnerBoxLeftTop.X + root.GTagBox.InnerBox.Width + CfgTagGraph.LayoutXPadding*5;
            //outterbox = root.GTagBox.InnerBox;
            outterbox = Rect.Empty;
            
            int idx = 0;
            int mid = (allChild.Count((item) => item != tag) + 1) / 2;
            if (mid < 2) mid = 2;
            foreach (string c in allChild)
            {
                if (c == tag) continue;//临时规避数据上的一个问题，有些节点自己成环了。
                int olddirect = idx <= mid ? 1 : -1;
                idx++;
                int direct = (idx) <= mid ? 1 : -1;
                if (olddirect==1 && direct==-1) y = 0; //显示从左边转到右边，将y重置

                subTree = GTagBoxTree.ExpandNode(c, 1, db, direct==1?r:l, y, direct);
                if(outterbox.IsEmpty)
                {
                    outterbox = subTree.TotalRange;
                }
                else
                {
                    outterbox.Union(subTree.TotalRange);
                }
                root.Children.Add(subTree);
                TreeLayoutEnv.Ins.AddLine(root, subTree, direct);
                y += subTree.TotalRange.Height;
            }
            outterbox.Union(root.GTagBox.OutterBox);
            root.TotalRange = outterbox;
            root.CenterItY();
            root.GTagBox.IsRoot = true;
            tags = TreeLayoutEnv.Ins.GetAllTagBox();
            lines = TreeLayoutEnv.Ins.GetAllLines().Cast<UIElement>();
        }
    }
}