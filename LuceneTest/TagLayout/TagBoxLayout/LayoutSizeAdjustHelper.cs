using AnyTag.UI;
using System.Windows;

namespace AnyTagNet
{
    class LayoutSizeAdjustHelper
    {
        public static void  AdjustSize(ref Size s, LayoutOption o)
        {
            if(o == LayoutOption.FixHeight)
            {
                s.Width += 50;
            }
            else if(o == LayoutOption.FixWidth)
            {
                s.Height += 50;
            }
            else
            {
                s.Width *= 1.2;
                s.Height *= 1.2;
            }
        }
    }

    
}
