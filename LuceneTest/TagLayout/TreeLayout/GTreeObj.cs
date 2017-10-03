using AnyTagNet;
using System;
using System.Collections.Generic;
using System.Windows;
using TagExplorer.TagLayout.LayoutCommon;
using TagExplorer.TagMgr;
using TagExplorer.Utils;

namespace TagExplorer.TagLayout.TreeLayout
{
    class GTreeObj
    {
        public List<GTreeObj> children = new List<GTreeObj>();
        public Rect OutBox = new Rect();
        public GTagLable box = null;
        private GTreeObj() { }

        private void MoveTo(double dx,double dy)
        {
            
            OutBox.X += dx;
            OutBox.Y += dy;
            box.InnerBox.X += dx;
            box.InnerBox.Y += dy;
            box.ColorBox.X += dx;
            box.ColorBox.Y += dy;

            foreach(GTreeObj o in children)
            {
                o.MoveTo(dx, dy);
            }

        }
        public static GTreeObj ExpandNode(string tag, int level, ITagDB db, double x, double y)
        {

            Logger.IN(tag);
            Logger.D("Expand " + tag + " " + x + " " + y);
            //已经展开过，直接返回
            if (GTreeObjDB.Ins.Get(tag) != null)
            {
                Logger.D("FOUND " + tag);
                Logger.OUT();
                return GTreeObjDB.Ins.Get(tag);
            }


            //新对象，创建一个
            GTreeObj root = new GTreeObj();
            root.box = new GTagLable(level, tag,x,y);
            root.OutBox = root.box.InnerBox;
            root.D("计算自身大小（不包括子节点）" + root.box.Tag);
            GTreeObjDB.Ins.Add(tag, root);//这个特别需要注意，在递归展开之前，先要将该节点加入DB，否则可能会出现无限递归

            List<string> children = db.QueryTagChildren(tag);
            List<Size> childrenSize = new List<Size>();
            
            GTreeObj pre = null;
            GTreeObj cur = null;
            double childX = x + root.box.InnerBox.Width;
            double childY = y;

            //double h = 0;
            //double w = 0;

            //遍历展开所有子节点
            foreach (string ctag in children)
            {
                if (GTreeObjDB.Ins.Get(ctag) != null) continue;
                cur = null;

                //GTreeObj child = ExpandNode(ctag, level + 1, db, x + root.box.InnerBox.Width, y + h);
                //h += child.OutBox.Height;
                //w = Math.Max(w, child.OutBox.Width);
                switch(GStyle.mode)
                {
                    case LAYOUT_COMPACT_MODE.TREE_COMPACT_MORE:
                        cur = ExpandChildMoreCompact(level, db, root, pre, ctag);
                        break;
                    case LAYOUT_COMPACT_MODE.TREE_COMPACT:
                        cur = ExpandChildCompact(level, db, root, pre, ctag);
                        break;
                    case LAYOUT_COMPACT_MODE.TREE_NO_COMPACT:
                        cur = ExpandChildNoCompact(level, db, root, pre, ctag);
                        break;
                    default:
                        break;
                }
                


                //h += cur.OutBox.Height;
                //w = Math.Max(w, cur.OutBox.Width);
                root.OutBox.Union(cur.OutBox);
                root.children.Add(cur);
                GTreeObjDB.Ins.Lines.Add(new Tuple<GTreeObj, GTreeObj>(root, cur));

                pre = cur;
            }

            //根据所有子节点所占区域的大小，计算自己的位置
            //root.OutBox.Width = w + root.box.InnerBox.Width;
            //root.OutBox.Height = Math.Max(h, root.box.InnerBox.Height);
            root.box.ColorBox.Y = (root.OutBox.Top + root.OutBox.Bottom) / 2;
            root.D(null);
            Logger.OUT();
            return root;
        }

        private static GTreeObj ExpandChildMoreCompact(int level, ITagDB db, GTreeObj root, GTreeObj pre, string ctag)
        {
            GTreeObj cur;
            Logger.D("ChoosePos:pre:{0}-{1}-{2} cur:{0}-{1},",
                    pre?.box.Tag,
                    pre == null ? 0 : db.QueryTagChildren(pre.box.Tag).Count,
                    pre == null ? 0 : pre.OutBox.Right,
                    ctag, db.QueryTagChildren(ctag).Count);
            //只有满足严格条件的情况下，才放在兄弟节点的后面，否则在父节点后展开
            if (pre != null &&
                (db.QueryTagChildren(ctag).Count == 0 /*&& db.QueryTagChildren(pre.box.Tag).Count == 0*/) &&
                pre.OutBox.Right < 800)
            {
                Logger.D("Place {0} after {1}:follow", ctag, pre.box.Tag);
                cur = ExpandNode(ctag, level + 1, db, pre.OutBox.Right, pre.OutBox.Top);

            }

            //第一个节点，没有兄弟节点，放在父节点后面   ==== > 放在父节点的同一行
            //非叶子节点，也直接放在父节点后面 or        ==== > 放在父节点的下一行
            //前一个兄弟已经把这一行占完了               ==== > 放在父节点的下一行
            else
            {
                if (pre == null)
                {
                    Logger.D("Place {0} after {1}:follow", ctag, root.box.Tag);
                    cur = ExpandNode(ctag, level + 1, db, root.box.InnerBox.Right, root.OutBox.Top);
                }
                else
                {
                    Logger.D("Place {0} after {1}:newline", ctag, root.box.Tag);
                    cur = ExpandNode(ctag, level + 1, db, root.box.InnerBox.Right, root.OutBox.Bottom);
                }
                
            }

            return cur;
        }
        private static GTreeObj ExpandChildCompact(int rootLevel, ITagDB db, GTreeObj root, GTreeObj pre, string ctag)
        {
            GTreeObj cur;
            Logger.D("ChoosePos:pre:{0}-{1}-{2} cur:{0}-{1},",
                    pre?.box.Tag,
                    pre == null ? 0 : db.QueryTagChildren(pre.box.Tag).Count,
                    pre == null ? 0 : pre.OutBox.Right,
                    ctag, db.QueryTagChildren(ctag).Count);
            //只有满足严格条件的情况下，才放在兄弟节点的后面，否则在父节点后展开
            if (pre != null &&
                (db.QueryTagChildren(pre.box.Tag).Count == 0) &&
                pre.OutBox.Right < 800)
            {
                Logger.D("Place {0} after {1}:follow", ctag, pre.box.Tag);
                cur = ExpandNode(ctag, rootLevel + 1, db, pre.OutBox.Right, pre.OutBox.Top);

            }

            //第一个节点，没有兄弟节点，放在父节点后面   ==== > 放在父节点的同一行
            //非叶子节点，也直接放在父节点后面 or        ==== > 放在父节点的下一行
            //前一个兄弟已经把这一行占完了               ==== > 放在父节点的下一行
            else
            {
                if (pre == null)
                {
                    Logger.D("Place {0} after {1}:follow", ctag, root.box.Tag);
                    cur = ExpandNode(ctag, rootLevel + 1, db, root.box.InnerBox.Right, root.OutBox.Top);
                }
                else
                {
                    Logger.D("Place {0} after {1}:newline", ctag, root.box.Tag);
                    cur = ExpandNode(ctag, rootLevel + 1, db, root.box.InnerBox.Right, root.OutBox.Bottom);
                }

            }

            return cur;
        }

        private static GTreeObj ExpandChildNoCompact(int level, ITagDB db, GTreeObj root, GTreeObj pre, string ctag)
        {
            GTreeObj cur;
            Logger.D("ChoosePos:pre:{0}-{1}-{2} cur:{0}-{1},",
                    pre?.box.Tag,
                    pre == null ? 0 : db.QueryTagChildren(pre.box.Tag).Count,
                    pre == null ? 0 : pre.OutBox.Right,
                    ctag, db.QueryTagChildren(ctag).Count);
            
            if (pre == null)
            {
                Logger.D("Place {0} after {1}:follow", ctag, root.box.Tag);
                cur = ExpandNode(ctag, level + 1, db, root.box.InnerBox.Right, root.OutBox.Top);
            }
            else
            {
                Logger.D("Place {0} after {1}:newline", ctag, root.box.Tag);
                cur = ExpandNode(ctag, level + 1, db, root.box.InnerBox.Right, root.OutBox.Bottom);
            }


            return cur;
        }

        private void D(string tip)
        {
            if(tip!=null)Logger.D(tip);
            Logger.D("ColorBox:" + box.Tag + " " + box.ColorBox.Left + " " + box.ColorBox.Right + " " + box.ColorBox.Top + " " + box.ColorBox.Bottom + " ");
            Logger.D("InnerBox:" + box.Tag + " " + box.InnerBox.Left + " " + box.InnerBox.Right + " " + box.InnerBox.Top + " " + box.InnerBox.Bottom + " ");
            Logger.D("OutterBox:" + box.Tag + " " + OutBox.Left + " " + OutBox.Right + " " + OutBox.Top + " " + OutBox.Bottom + " ");
        }
    }
}
