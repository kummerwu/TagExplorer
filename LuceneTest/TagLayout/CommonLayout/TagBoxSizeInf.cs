using System;
using System.Windows;
using TagExplorer.TagLayout.LayoutCommon;
using TagExplorer.TagMgr;
using TagExplorer.Utils;

namespace TagExplorer.TagLayout.CommonLayout
{
    public class TagBoxSizeInf
    {
        //公有成员变量************************************************************
        public Size InnerBoxSize;
        public Size OutterBoxSize;
        //公有成员变量(只读)******************************************************
        public double FontSize { get; private set; }
        public int Distance { get; private set; } //本节点离根节点（树层次上的距离）
        public GUTag Tag { get; private set; }
        public double InnerBoxXPadding { get; private set; }// GConfig.InnerBoxXPadding_MAX;
        public double InnerBoxYPadding { get; private set; }// GConfig.InnerBoxYPadding_MAX;
        //私有成员变量************************************************************
        //公有成员方法************************************************************
        public TagBoxSizeInf(GUTag tag, int dis, string fname)
        {
            Tag = tag;
            Distance = dis;
            PreCalcBoxSize();
            CalcBoxSize(fname);
        }

        //根据Distance计算字体大小和Padding大小
        private void PreCalcBoxSize()
        {
            FontSize = StaticCfg.Ins.FontSize;
            InnerBoxXPadding = StaticCfg.Ins.InnerBoxXPadding_MAX;
            InnerBoxYPadding = StaticCfg.Ins.InnerBoxYPadding_MAX;
            for (int i = 0; i < Distance; i++)
            {
                FontSize /= StaticCfg.Ins.ScaleInRadio;
                InnerBoxXPadding /= StaticCfg.Ins.ScaleInRadio;
                InnerBoxYPadding /= StaticCfg.Ins.ScaleInRadio;
            }

            InnerBoxXPadding = Math.Max(InnerBoxXPadding, StaticCfg.Ins.InnerBoxXPadding_MIN);
            InnerBoxYPadding = Math.Max(InnerBoxYPadding, StaticCfg.Ins.InnerBoxYPadding_MIN);
            FontSize = Math.Max(FontSize, StaticCfg.Ins.MinFontSize);
        }


        //计算自身内容所占区域的大小（文本和着色区域）
        private void CalcBoxSize(string fname)
        {
            Size tmp = FastFontCalculator.Ins.Calc(Tag, FontSize, fname);
            InnerBoxSize.Width = tmp.Width;
            InnerBoxSize.Height = tmp.Height;

            OutterBoxSize.Width = InnerBoxSize.Width + InnerBoxXPadding;
            OutterBoxSize.Height = InnerBoxSize.Height + InnerBoxYPadding;


        }
    }
}
