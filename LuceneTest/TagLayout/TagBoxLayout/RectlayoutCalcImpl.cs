using AnyTag.UI;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Windows;

namespace AnyTagNet
{
    class RectlayoutCalcImpl : IRectLayoutCalc
    {
        private void Clear(IEnumerable<GObj> objs)
        {
            foreach (GObj r in objs)
            {
                r.OuterBox.X = r.OuterBox.Y = 0;
            }
            putInObjs.Clear();
        }
        ArrayList putInObjs = new ArrayList();
        private bool isValidPos(GObj obj,Size rect)
        {
            //先检查自己外边缘是否已经超过了rect的范围
            if (obj.OuterBox.X + obj.OuterBox.Width > rect.Width ||
                obj.OuterBox.Y + obj.OuterBox.Height > rect.Height)
            {
                return false;
            }

            //再检查与已有的对象是否相交
            foreach(GObj pr in putInObjs)
            {
                //if (obj.OuterBox.IntersectsWith(pr.OuterBox)) return false;
                if (obj.OverlayWith(pr)) return false;
            }
            return true;
        }

        /// <summary>
        ///  给定一个矩形区域，尝试将所有节点放入矩形区域。
        /// </summary>
        /// <param name="initSize">初始显示框大小</param>
        /// <param name="allChildren">所有子节点列表</param>
        /// <param name="opt">布局约束选项：保持宽度，高度，还是比例关系</param>
        public void Calc(ref Size initSize, IEnumerable<GObj> allChildren,LayoutOption opt)
        {
            bool ret = false;

            //尝试把所有子节点在区域中布局，如果布不下，根据选项扩大区域再次尝试。
            do
            {
                ret = Calc_inner(ref initSize, allChildren);
                LayoutSizeAdjustHelper.AdjustSize(ref initSize, opt);
            } while (!ret);
            double w = 0, h = 0;
            foreach (GObj r in allChildren)
            {
                w = Math.Max(w, r.OuterBox.X + r.OuterBox.Width);
                h = Math.Max(h, r.OuterBox.Y + r.OuterBox.Height);
            }
            initSize.Width = w;
            initSize.Height = h;
        }

        private bool Calc_inner(ref Size s, IEnumerable<GObj> objs)
        {
            Clear(objs);
            bool ret = true;
            foreach (GObj r in objs)
            {
                ret = Put(r, s);
                if (!ret) return false;
            }
            return true;

        }

        //尝试将对象r放到指定大小的矩形中
        private bool Put(GObj obj,Size rect)
        {
            double dx = Math.Max(rect.Width / 50, 1);
            double dy = Math.Max(rect.Height / 50, 1);
            double x = 0;
            double y = 0;
            
            for (y = 0; y < rect.Height; y += dy)
            {
                for (x = 0; x < rect.Width; x += dx)
                {
                    obj.OuterBox.X = x;
                    obj.OuterBox.Y = y;
                    if (isValidPos(obj,rect))
                    {
                        putInObjs.Add(obj);
                        return true;
                    }
                    else
                    {
                        obj.OuterBox.X = obj.OuterBox.Y = 0;
                    }
                }
            }
            return false;
        }
    }
}
