using System;
using System.Collections;
using System.Collections.Generic;
using System.Windows;
using TagExplorer.Utils;

namespace TagExplorer.BoxLayout
{
    class RectlayoutCalcImpl : IRectLayoutCalc
    {
        private void Clear(IEnumerable<GBoxObj> objs)
        {
            foreach (GBoxObj r in objs)
            {
                r.OuterBox.X = r.OuterBox.Y = 0;
            }
            putInObjs.Clear();
        }
        ArrayList putInObjs = new ArrayList();
        private int isValidPos(GBoxObj obj,Size rect)
        {
            if(obj.OuterBox.Width > rect.Width ||
                obj.OuterBox.Height>rect.Height)
            {
                return -1;//返回负数，大小不够
            }
            
            //先检查自己外边缘是否已经超过了rect的范围
            if (obj.OuterBox.X + obj.OuterBox.Width > rect.Width ||
                obj.OuterBox.Y + obj.OuterBox.Height > rect.Height)
            {
                return 0;
            }

            //再检查与已有的对象是否相交
            foreach(GBoxObj pr in putInObjs)
            {
                //if (obj.OuterBox.IntersectsWith(pr.OuterBox)) return false;
                if (obj.OverlayWith(pr)) return 0; //返回0，表示与已有的相交
            }
            return 1;//OK，是一个有效位置
        }

        /// <summary>
        ///  给定一个矩形区域，尝试将所有节点放入矩形区域。
        /// </summary>
        /// <param name="initSize">初始显示框大小</param>
        /// <param name="allChildren">所有子节点列表</param>
        /// <param name="opt">布局约束选项：保持宽度，高度，还是比例关系</param>
        public void Calc(ref Size initSize, IEnumerable<GBoxObj> allChildren,LayoutOption opt)
        {
            bool ret = false;

            //尝试把所有子节点在区域中布局，如果布不下，根据选项扩大区域再次尝试。
            do
            {
                ret = Calc_inner(ref initSize, allChildren);
                LayoutSizeAdjustHelper.AdjustSize(ref initSize, opt);
            } while (!ret);
            double w = 0, h = 0;
            foreach (GBoxObj r in allChildren)
            {
                w = Math.Max(w, r.OuterBox.X + r.OuterBox.Width);
                h = Math.Max(h, r.OuterBox.Y + r.OuterBox.Height);
            }
            initSize.Width = w;
            initSize.Height = h;
        }

        private bool Calc_inner(ref Size s, IEnumerable<GBoxObj> objs)
        {
            Clear(objs);
            bool ret = true;
            foreach (GBoxObj r in objs)
            {
                ret = Put(r, s);
                if (!ret) return false;
            }
            return true;

        }
        
        //尝试将对象r放到指定大小的矩形中
        private bool Put(GBoxObj obj,Size rect)
        {
            double dx = Math.Max(rect.Width / 50, 1);
            double dy = Math.Max(rect.Height / 50, 1);
            double x = 0;
            double y = 0;
            Logger.D("try put {0} in {1}", obj.Tag, rect);
            for (y = 0; y < rect.Height; y += dy)
            {
                for (x = 0; x < rect.Width; x += dx)
                {
                    //obj.OuterBox.X = x;
                    //obj.OuterBox.Y = y;
                    obj.SetOutterBoxPos(x, y);
                    int ret = isValidPos(obj, rect);
                    if (ret>0) //OK
                    {
                        putInObjs.Add(obj);
                        Logger.D("   Put OK!! {0} # {1}  ##{2}", obj.Tag, obj.TextBox, obj.OuterBox);
                        return true;
                    }
                    else if(ret==0)
                    {
                        //obj.OuterBox.X = obj.OuterBox.Y = 0;
                        obj.SetOutterBoxPos(0, 0);
                    }
                    else
                    {
                        return false;
                    }
                }
            }
            return false;
        }
    }
}
