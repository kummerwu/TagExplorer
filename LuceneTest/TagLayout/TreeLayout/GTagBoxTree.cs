﻿using AnyTagNet;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows;
using TagExplorer.TagLayout.CommonLayout;
using TagExplorer.TagLayout.LayoutCommon;
using TagExplorer.TagMgr;
using TagExplorer.Utils;

namespace TagExplorer.TagLayout.TreeLayout
{
    public class GTagBoxTree
    {
        public List<GTagBoxTree> Children = new List<GTagBoxTree>();
        private Rect totalRange = Rect.Empty;
        public Rect TotalRange
        {
            get
            {
                if(totalRange.IsEmpty)
                {
                    return GTagBox.OutterBox;
                }
                else
                {
                    return totalRange;
                }
            }
            set
            {
                totalRange = value;
            }
        }
        public GTagBox GTagBox = null;
        public GTagBoxTree() { }
        
        public void Move(double dx,double dy)
        {
            GTagBox.Move(dx, dy);
            foreach (GTagBoxTree c in Children) c.Move(dx, dy);
        }
        public void CenterRootY()
        {
            GTagBox.CenterRootY(totalRange);
        }
        public static GTagBoxTree ExpandNode(string tag, int level, ITagDB db, double x, double y,int direct,Size size,TreeLayoutEnv env)
        {
            

            Logger.IN(tag);
            Logger.D("Expand " + tag + " " + x + " " + y);
            //已经展开过，直接返回
            if (env.Get(tag) != null)
            {
                Logger.D("FOUND " + tag);
                Logger.OUT();
                return env.Get(tag);
            }


            //创建子树的根对象
            GTagBoxTree root = new GTagBoxTree();
            root.GTagBox = new GTagBox(level, tag,x,y,direct);
            root.totalRange = root.GTagBox.OutterBox;
            root.D("计算自身大小（不包括子节点）" + root.GTagBox.Tag);
            env.Add(tag, root);//这个特别需要注意，在递归展开之前，先要将该节点加入DB，否则可能会出现无限递归

            List<string> children = db.QueryTagChildren(tag);
            List<Size> childrenSize = new List<Size>();
            
            GTagBoxTree pre = null;
            GTagBoxTree cur = null;
            double childX = x + direct*root.GTagBox.OutterBox.Width;
            double childY = y;

            int MaxLevel = GLayoutMode.mode == LayoutMode.LRTREE_COMPACT || GLayoutMode.mode == LayoutMode.LRTREE_COMPACT_MORE ? 2 : 100;
            //double h = 0;
            //double w = 0;
            if (level < MaxLevel)
            {
                //遍历展开所有子节点
                foreach (string ctag in children)
                {
                    if (env.Get(ctag) != null) continue;
                    cur = null;

                    //GTreeObj child = ExpandNode(ctag, level + 1, db, x + root.box.InnerBox.Width, y + h);
                    //h += child.OutBox.Height;
                    //w = Math.Max(w, child.OutBox.Width);
                    switch (GLayoutMode.mode)
                    {
                        case LayoutMode.TREE_COMPACT_MORE:
                        case LayoutMode.LRTREE_COMPACT_MORE:
                            cur = ExpandChildMoreCompact(level, db, root, pre, ctag,direct,size,env);
                            break;
                        case LayoutMode.TREE_COMPACT:
                        case LayoutMode.LRTREE_COMPACT:
                            cur = ExpandChildCompact(level, db, root, pre, ctag,direct,size,env);
                            break;
                        case LayoutMode.TREE_NO_COMPACT:
                        case LayoutMode.LRTREE_NO_COMPACT:
                            cur = ExpandChildNoCompact(level, db, root, pre, ctag,direct,size,env);
                            break;
                        default:
                            break;
                    }



                    //h += cur.OutBox.Height;
                    //w = Math.Max(w, cur.OutBox.Width);
                    root.totalRange.Union(cur.totalRange);
                    root.Children.Add(cur);


                    pre = cur;
                }
            }

            //根据所有子节点所占区域的大小，计算自己的位置
            //root.OutBox.Width = w + root.box.InnerBox.Width;
            //root.OutBox.Height = Math.Max(h, root.box.InnerBox.Height);
            //root.GTagBox.InnerBox.Y = (root.TotalRange.Top + root.TotalRange.Bottom) / 2;
            root.GTagBox.CenterRootY(root.totalRange);

            root.D(null);
            Logger.OUT();
            return root;
        }

        private static GTagBoxTree ExpandChildMoreCompact(int level, ITagDB db, GTagBoxTree root, GTagBoxTree pre, string ctag,int direct,Size size,TreeLayoutEnv env)
        {
            GTagBoxTree cur;
            Logger.D("ChoosePos:pre:{0}-{1}-{2} cur:{0}-{1},",
                    pre?.GTagBox.Tag,
                    pre == null ? 0 : db.QueryTagChildren(pre.GTagBox.Tag).Count,
                    pre == null ? 0 : pre.totalRange.Right,
                    ctag, db.QueryTagChildren(ctag).Count);
            //只有满足严格条件的情况下，才放在兄弟节点的后面，否则在父节点后展开
            if (pre != null &&
                /*(db.QueryTagChildren(ctag).Count == 0 && db.QueryTagChildren(pre.box.Tag).Count == 0)*/
                pre.totalRange.Right < size.Width - 70 && pre.totalRange.Left>70)
            {
                Logger.D("Place {0} after {1}:follow", ctag, pre.GTagBox.Tag);
                cur = ExpandNode(ctag, level + 1, db, 
                    direct==1?pre.totalRange.Right:pre.totalRange.Left, 
                    pre.totalRange.Top,direct,size,env);

            }

            //第一个节点，没有兄弟节点，放在父节点后面   ==== > 放在父节点的同一行
            //非叶子节点，也直接放在父节点后面 or        ==== > 放在父节点的下一行
            //前一个兄弟已经把这一行占完了               ==== > 放在父节点的下一行
            else
            {
                if (pre == null)
                {
                    Logger.D("Place {0} after {1}:follow", ctag, root.GTagBox.Tag);
                    cur = ExpandNode(ctag, level + 1, db,
                        direct == 1 ? root.GTagBox.OutterBox.Right : root.GTagBox.OutterBox.Left,
                        root.totalRange.Top,direct,size,env);
                }
                else
                {
                    Logger.D("Place {0} after {1}:newline", ctag, root.GTagBox.Tag);
                    cur = ExpandNode(ctag, level + 1, db,
                        direct == 1 ? root.GTagBox.OutterBox.Right : root.GTagBox.OutterBox.Left,
                        root.totalRange.Bottom,direct,size,env);
                }
                env.AddLine(root, cur,direct);
            }

            return cur;
        }
        private static GTagBoxTree ExpandChildCompact(int rootLevel, ITagDB db, GTagBoxTree root, GTagBoxTree pre, string ctag,int direct,Size size,TreeLayoutEnv env)
        {
            GTagBoxTree cur;
            Logger.D("ChoosePos:pre:{0}-{1}-{2} cur:{0}-{1},",
                    pre?.GTagBox.Tag,
                    pre == null ? 0 : db.QueryTagChildren(pre.GTagBox.Tag).Count,
                    pre == null ? 0 : pre.totalRange.Right,
                    ctag, db.QueryTagChildren(ctag).Count);
            //只有满足严格条件的情况下，才放在兄弟节点的后面，否则在父节点后展开
            if (pre != null &&
                (db.QueryTagChildren(pre.GTagBox.Tag).Count == 0) &&
                pre.totalRange.Right< size.Width-70)
            {
                Logger.D("Place {0} after {1}:follow", ctag, pre.GTagBox.Tag);
                cur = ExpandNode(ctag, rootLevel + 1, db,
                    direct == 1 ? pre.totalRange.Right : pre.totalRange.Left,
                    pre.totalRange.Top,direct,size,env);

            }

            //第一个节点，没有兄弟节点，放在父节点后面   ==== > 放在父节点的同一行
            //非叶子节点，也直接放在父节点后面 or        ==== > 放在父节点的下一行
            //前一个兄弟已经把这一行占完了               ==== > 放在父节点的下一行
            else
            {
                if (pre == null)
                {
                    Logger.D("Place {0} after {1}:follow", ctag, root.GTagBox.Tag);
                    cur = ExpandNode(ctag, rootLevel + 1, db,
                        direct == 1 ? root.GTagBox.OutterBox.Right : root.GTagBox.OutterBox.Left,
                        root.totalRange.Top,direct,size,env);
                }
                else
                {
                    Logger.D("Place {0} after {1}:newline", ctag, root.GTagBox.Tag);
                    cur = ExpandNode(ctag, rootLevel + 1, db,
                        direct == 1 ? root.GTagBox.OutterBox.Right : root.GTagBox.OutterBox.Left,
                        root.totalRange.Bottom,direct,size,env);
                }
                env.AddLine(root, cur,direct);
            }

            return cur;
        }

        private static GTagBoxTree ExpandChildNoCompact(int level, ITagDB db, GTagBoxTree root, GTagBoxTree pre, string ctag,int direct,Size size,TreeLayoutEnv env)
        {
            GTagBoxTree cur;
            Logger.D("ChoosePos:pre:{0}-{1}-{2} cur:{0}-{1},",
                    pre?.GTagBox.Tag,
                    pre == null ? 0 : db.QueryTagChildren(pre.GTagBox.Tag).Count,
                    pre == null ? 0 : pre.totalRange.Right,
                    ctag, db.QueryTagChildren(ctag).Count);
            
            if (pre == null)
            {
                Logger.D("Place {0} after {1}:follow", ctag, root.GTagBox.Tag);
                cur = ExpandNode(ctag, level + 1, db,
                    direct == 1 ? root.GTagBox.OutterBox.Right : root.GTagBox.OutterBox.Left, 
                    root.totalRange.Top,direct,size,env);
            }
            else
            {
                Logger.D("Place {0} after {1}:newline", ctag, root.GTagBox.Tag);
                cur = ExpandNode(ctag, level + 1, db,
                    direct == 1 ? root.GTagBox.OutterBox.Right : root.GTagBox.OutterBox.Left,
                    root.totalRange.Bottom,direct,size,env);
            }

            env.AddLine(root, cur,direct);
            return cur;
        }
        [Conditional("LOGGER")]
        private void D(string tip)
        {
            if (Logger.on)
            {
                if (tip != null) Logger.D(tip);
                Logger.D("ColorBox:" + GTagBox.Tag + " " + GTagBox.InnerBox.Left + " " + GTagBox.InnerBox.Right + " " + GTagBox.InnerBox.Top + " " + GTagBox.InnerBox.Bottom + " ");
                Logger.D("InnerBox:" + GTagBox.Tag + " " + GTagBox.OutterBox.Left + " " + GTagBox.OutterBox.Right + " " + GTagBox.OutterBox.Top + " " + GTagBox.OutterBox.Bottom + " ");
                Logger.D("OutterBox:" + GTagBox.Tag + " " + totalRange.Left + " " + totalRange.Right + " " + totalRange.Top + " " + totalRange.Bottom + " ");
            }
        }
    }
}
