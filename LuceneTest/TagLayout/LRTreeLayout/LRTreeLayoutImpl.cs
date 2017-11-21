using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using TagExplorer.TagLayout.CommonLayout;
using TagExplorer.TagLayout.LayoutCommon;
using TagExplorer.TagLayout.TreeLayout;
using TagExplorer.TagMgr;
using TagExplorer.Utils;

namespace TagExplorer.TagLayout.LRTreeLayout
{
    class LRTreeLayoutImpl : TagLayoutBase
    {
        public LRTreeLayoutImpl(LayoutMode mode) : base(mode)
        {
        }

        

        private int CalcMid(List<GUTag> allChild,ITagDB db)
        {
            int rootChildrenCount = allChild.Count;
            int total = 0;
            //计算所有节点数量
            foreach(GUTag c in allChild)
            {
                total +=Math.Max(1, db.QueryChildrenCount(c));//没有子节点的算作1
            }
            //找到子节点中间分割线
            int tmpTotal = 0;
            double best = 1;
            int bestMid = 0;

            for(int i = 0;i<rootChildrenCount;i++)
            {
                tmpTotal += Math.Max(1, db.QueryChildrenCount(allChild[i]));
                double radio = ((double)tmpTotal )/ total;
                if(Math.Abs(radio-0.5) < best )//离一半最近
                {
                    best = Math.Abs(radio - 0.5);
                    bestMid = i + 1;
                }
            }
            
            return Math.Max(1,bestMid);
        }
        
        public override void Layout(ITagDB db, GUTag rootTag,Size size,TreeLayoutEnv env)
        {
            //初始化准备工作
            GTagBoxTree subTree = null;
            double y = 0;
            oriSize = size;
            this.db = db;
            env.Reset();
            tags = new List<TagBox>();
            
            

            //计算出Root节点的位置信息
            double rootTagBoxX = size.Width / 2;
            root = new GTagBoxTree();
            root.GTagBox = new GTagBox(0, rootTag, rootTagBoxX, 0, 1);
            root.Move(-1 * root.GTagBox.InnerBox.Width*3 / 4, 0);
            env.Add(rootTag, root);

            //计算左右子节点的开始位置（估算）
            double l, r;
            l = root.GTagBox.InnerBoxLeftTop.X - StaticCfg.Ins.LayoutXPadding*5;
            r = root.GTagBox.InnerBoxLeftTop.X + root.GTagBox.InnerBox.Width + StaticCfg.Ins.LayoutXPadding*5;
            Rect outterbox = Rect.Empty;

            List<GUTag> allChild = db.QueryTagChildren(rootTag);
            allChild.Remove(rootTag);

            int idx = 0;
            int mid = CalcMid(allChild, db);
            GTagBoxTree[] children = new GTagBoxTree[allChild.Count];

            int direct = 1;
            //希望现实是按照顺时针方向现实
            List<GUTag> Left = new List<GUTag>();
            List<GUTag> Right = new List<GUTag>();
            
            for(int i= 0;i<allChild.Count;i++)
            {
                if (i < mid)
                {
                    Right.Add(allChild[i]);
                }
                else
                {
                    Left.Add(allChild[i]);
                }
            }
            List<GUTag> all = new List<GUTag>();
            all.AddRange(Right);
            Left.Reverse();
            all.AddRange(Left);

            foreach (GUTag c in all)
            {
                if (c == rootTag) continue;//临时规避数据上的一个问题，有些节点自己成环了。
                //确定当前子节点时放在左边，还是放在右边：半数放在左边，半数放在右边
                if (idx == mid)
                {
                    y = 0; //显示从左边转到右边，将y重置
                    direct = -1;
                }
                //展开第idx个子节点
                subTree = GTagBoxTree.ExpandNode(c, 1, db, direct==1?r:l, y, direct,size,env,myLayoutMode);
                children[idx] = subTree;

                //更新整个显示区的大小。(outterBox)
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
