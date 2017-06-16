using AnyTag.UI;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
        private bool isValid(GObj r,Size s)
        {
            if (r.OuterBox.X + r.OuterBox.Width > s.Width || r.OuterBox.Y + r.OuterBox.Height > s.Height) return false;
            foreach(GObj pr in putInObjs)
            {
                if (r.OuterBox.IntersectsWith(pr.OuterBox)) return false;
            }
            return true;
        }
        public void Calc(ref Size s, IEnumerable<GObj> objs,LayoutOption o)
        {
            bool ret = false;
            do
            {
                ret = Calc_inner(ref s, objs);
                LayoutSizeAdjustHelper.AdjustSize(ref s, o);
            } while (!ret);
            double w = 0, h = 0;
            foreach (GObj r in objs)
            {
                w = Math.Max(w, r.OuterBox.X + r.OuterBox.Width);
                h = Math.Max(h, r.OuterBox.Y + r.OuterBox.Height);
            }
            s.Width = w;
            s.Height = h;
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
        private bool Put(GObj r,Size s)
        {
            double dx = Math.Max(s.Width / 50, 1);
            double dy = Math.Max(s.Height / 50, 1);
            double x = 0;
            double y = 0;
            
            for (y = 0; y < s.Height; y += dy)
            {
                for (x = 0; x < s.Width; x += dx)
                {
                    r.OuterBox.X = x;
                    r.OuterBox.Y = y;
                    if (isValid(r,s))
                    {
                        putInObjs.Add(r);
                        return true;
                    }
                    else
                    {
                        r.OuterBox.X = r.OuterBox.Y = 0;
                    }
                }
            }
            return false;
        }
    }
}
