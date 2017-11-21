using AnyTagNet;
using System;
using System.Collections;
using System.Windows;
using System.Windows.Media;
using TagExplorer.TagLayout.CommonLayout;
using TagExplorer.TagMgr;
using TagExplorer.Utils;

namespace TagExplorer.TagLayout.LayoutCommon
{
    
    /// <summary>
    /// 根据配置的字体，大小，计算一个Tag所占区域的大小和颜色
    /// </summary>
    public class GTagBox
    {


        public bool IsRoot = false;
        public Point OutterBoxLeftTop = new Point();          //内部文字区域，加上外部空白边缘
        public Point InnerBoxLeftTop = new Point();           //内部文字区域
        readonly TagBoxSizeInf Inf;
        readonly public int Direct;

        //计算只读属性
        public Rect OutterBox{get{return new Rect(OutterBoxLeftTop, Inf.OutterBoxSize);}}
        public Rect InnerBox{get{return new Rect(InnerBoxLeftTop, Inf.InnerBoxSize);}}
        public GUTag Tag{get { return Inf.Tag; }}
        public double FontSize{get { return Inf.FontSize; }}
        public int Distance { get { return Inf.Distance; } }
        public int Level { get { return Inf.Distance; } }
        
        public GTagBox(int distance,GUTag tag,double x,double y,int direct)
        {
            //记录展开方向（1：向右 -1：向左）
            Direct = direct;

            //计算大小
            Inf = new TagBoxSizeInf(tag, distance, StaticCfg.Ins.GFontName);

            //*****计算位置*****************
            //计算OutterBox
            OutterBoxLeftTop.X = x;
            OutterBoxLeftTop.Y = y;
            if (Direct == -1) OutterBoxLeftTop.X -= Inf.OutterBoxSize.Width;

            //计算InnerBox在：OutterBox的中间
            InnerBoxLeftTop.X = OutterBoxLeftTop.X + (Inf.OutterBoxSize.Width - Inf.InnerBoxSize.Width) / 2;
            InnerBoxLeftTop.Y = OutterBoxLeftTop.Y + (Inf.OutterBoxSize.Height - Inf.InnerBoxSize.Height) / 2; ;
        }
        
       //将本TextBox移到指定矩形的正中间（仅移动Y坐标）
        public void CenterRootY(Rect rect)
        {
            OutterBoxLeftTop.Y = rect.Y + (rect.Height-Inf.OutterBoxSize.Height)/2;
            InnerBoxLeftTop.Y = rect.Y + (rect.Height - Inf.InnerBoxSize.Height) / 2;
        }
        /// <summary>
        /// 这个函数放在这儿不合适，需要优化 TODO
        /// </summary>
        /// <param name="OuterBox"></param>
        /// <param name="ParentBox"></param>
        public void CalcInnerBoxPos(Rect OuterBox, Size ParentBox)
        {
            OutterBoxLeftTop.X = OuterBox.X + (OuterBox.Width - Inf.OutterBoxSize.Width) / 2;
            OutterBoxLeftTop.Y = OuterBox.Y + ParentBox.Height;
            InnerBoxLeftTop.X = OutterBoxLeftTop.X + Inf.InnerBoxXPadding;
            InnerBoxLeftTop.Y = OutterBoxLeftTop.Y + Inf.InnerBoxYPadding;
        }

        internal void Move(double dx, double dy)
        {
            OutterBoxLeftTop.X += dx;
            OutterBoxLeftTop.Y += dy;
            InnerBoxLeftTop.X += dx;
            InnerBoxLeftTop.Y += dy;

        }
    }
}
