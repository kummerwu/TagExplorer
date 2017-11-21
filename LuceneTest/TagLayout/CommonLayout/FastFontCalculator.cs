using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using TagExplorer.TagMgr;
using TagExplorer.Utils;

namespace TagExplorer.TagLayout.CommonLayout
{
    //文本显示宽度和高度的快速计算（用了cache加速计算）
    public class FastFontCalculator
    {
        #region 单实例
        private static FastFontCalculator ins;
        public static FastFontCalculator Ins
        {
            get
            {
                if (ins == null)
                {
                    ins = new FastFontCalculator();
                }
                return ins;
            }
        }
        #endregion

        #region 计算cache加速
        Hashtable cache = new Hashtable();
        public Size Calc(GUTag gutag, double fsize, string fname)
        {
            Size tmp;
            string tag = gutag.Title;
            if (cache[tag] == null)
            {
                //cache内容过多，直接将所有数据全部老化。
                if (cache.Keys.Count > 2000)
                {
                    cache.Clear();
                }
                FormattedText formattedText = new FormattedText(
                    tag,
                    System.Globalization.CultureInfo.InvariantCulture,
                    FlowDirection.LeftToRight,
                    new Typeface(fname),
                    StaticCfg.Ins.FontSize,
                    Brushes.Black
                );
                tmp = new Size(formattedText.WidthIncludingTrailingWhitespace, formattedText.Height);
                cache.Add(tag, tmp);

            }
            tmp = (Size)cache[tag];
            tmp.Width = tmp.Width * fsize / StaticCfg.Ins.FontSize + StaticCfg.Ins.XContentPadding;
            tmp.Height = tmp.Height * fsize / StaticCfg.Ins.FontSize + StaticCfg.Ins.YContentPadding;
            return tmp;
        }
        #endregion

    }
}
