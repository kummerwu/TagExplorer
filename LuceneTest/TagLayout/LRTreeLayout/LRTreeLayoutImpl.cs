using AnyTagNet;
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
        public List<TagBox> tags = new List<TagBox>();
        public IEnumerable<UIElement> lines = null;
        public void Layout(ITagDB db, string tag,Size size,TreeLayoutEnv env)
        {
            oriSize = size;
            this.db = db;

            env.Reset();
            tags.Clear();
            GTagBoxTree subTree = null;
            double y = 0;
            List<string> allChild = db.QueryTagChildren(tag);
            
            
            //double x = 600;
            double centerX = size.Width / 2;
            

            GTagBoxTree root = new GTagBoxTree();
            //TagBox box = UIElementFactory.CreateTagBox(tagbox);
            root.GTagBox = new GTagBox(0, tag, centerX, 0, 1);
            root.Move(-1 * root.GTagBox.InnerBox.Width / 2, 0);
            env.Add(tag, root);
            double l, r;
            l = root.GTagBox.InnerBoxLeftTop.X - StaticCfg.Ins.LayoutXPadding*5;
            r = root.GTagBox.InnerBoxLeftTop.X + root.GTagBox.InnerBox.Width + StaticCfg.Ins.LayoutXPadding*5;
            //outterbox = root.GTagBox.InnerBox;
            outterbox = Rect.Empty;
            
            int idx = 0;
            int cnt = allChild.Count((item) => item != tag);
            int mid = Math.Max((cnt + 1) / 2 , 2);
            GTagBoxTree[] children = new GTagBoxTree[cnt];

            int direct = 1;
            foreach (string c in allChild)
            {
                if (c == tag) continue;//临时规避数据上的一个问题，有些节点自己成环了。
                //半数放在左边，半数放在右边
                if (idx == mid)
                {
                    y = 0; //显示从左边转到右边，将y重置
                    direct = -1;
                }


                subTree = GTagBoxTree.ExpandNode(c, 1, db, direct==1?r:l, y, direct,size,env);
                children[idx] = subTree;
                if(idx==0)
                {
                    outterbox = subTree.TotalRange;
                }
                else
                {
                    outterbox.Union(subTree.TotalRange);
                }
                root.Children.Add(subTree);
                env.AddLine(root, subTree, direct);
                y += subTree.TotalRange.Height;
                idx++;
            }
            outterbox.Union(root.GTagBox.OutterBox);
            root.TotalRange = outterbox;
            root.CenterRootY();
            root.GTagBox.IsRoot = true;

            LRBanlance(children, mid);
            //如果有图形在坐标0的左边，将其往右移一些。
            if(outterbox.X<0)
            {
                root.Move(-outterbox.X, 0);
            }
            //ShowParent(root);
            tags = env.GetAllTagBox();
            lines = env.GetAllLines().Cast<UIElement>();
        }
        //private void ShowParent(GTagBoxTree root)
        //{
        //    string tag = root.GTagBox.Tag;
        //    string p1 = null, p2 = null;
        //    if(!string.IsNullOrEmpty(tag))
        //    {
        //        List<string> p1s = db.QueryTagParent(tag);
        //        if (p1s.Count > 0) p1 = p1s[0];
        //    }
        //    if (!string.IsNullOrEmpty(p1))
        //    {
        //        List<string> p2s = db.QueryTagParent(p1);
        //        if (p2s.Count > 0) p2 = p2s[0];
        //    }

        //    double dy = 3 * CfgTagGraph.Ins.InnerBoxYPadding_MAX;

        //    if (root.GTagBox.OutterBoxLeftTop.Y < dy)
        //    {
        //        root.Move(0, dy - root.GTagBox.InnerBoxLeftTop.Y+10);
        //    }

        //    double x = root.GTagBox.InnerBoxLeftTop.X;
        //    double y = root.GTagBox.InnerBoxLeftTop.Y;
            
            

        //    GTagBoxTree p1tree = null, p2tree = null;
        //    x = 0;
        //    y = 0;
        //    if (!string.IsNullOrEmpty(p2))
        //    {
        //        GTagBox p2box = null;
        //        p2box = new GTagBox(6, p2, x, y, 1);
        //        p2tree = new GTagBoxTree();
        //        p2tree.GTagBox = p2box;
        //        TreeLayoutEnv.Ins.Add(p2box.Tag, p2tree);
        //        //TreeLayoutEnv.Ins.AddLine(p2tree, p1tree, 1);
        //        x += p2tree.TotalRange.Width ;
        //    }
            
        //    if (!string.IsNullOrEmpty(p1))
        //    {
        //        GTagBox p1box = null;
        //        p1box  = new GTagBox(5, p1, x, y , 1);
        //        p1tree= new GTagBoxTree();
        //        p1tree.GTagBox = p1box;
        //        TreeLayoutEnv.Ins.Add(p1box.Tag, p1tree);
        //        //TreeLayoutEnv.Ins.AddLine(root, p1tree, 1);
        //        x += p1tree.TotalRange.Width;
        //    }

            
            


        //}
        private void LRBanlance(GTagBoxTree[] children, int mid)
        {
            if (children.Length <= mid) return;

            //先显示优先，再显示左边，右边有mid个，剩余的放左边
            double RightBottom = children[mid-1].TotalRange.Bottom;
            double LeftBottom = children[children.Length - 1].TotalRange.Bottom;
            int rightCnt = mid;
            int leftCnt = children.Length - rightCnt;
            //左边比较高，需要把右边的所有子树平均下移一些
            if(LeftBottom>RightBottom && rightCnt>1)
            {
                double dy = (LeftBottom - RightBottom) / (rightCnt-1);
                for(int i = 1;i<rightCnt;i++)
                {
                    children[i].Move(0, dy * i);
                }
            }
            if (RightBottom > LeftBottom && leftCnt > 1)
            {
                double dy = (RightBottom - LeftBottom ) / (leftCnt - 1);
                for (int i = 1; i < leftCnt; i++)
                {
                    children[mid+i].Move(0, dy * i);
                }
            }

        }
    }
}
