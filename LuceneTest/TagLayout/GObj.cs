using AnyTag.UI;
using AnyTagNet.BL;
using LuceneTest.TagMgr;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Media;

namespace AnyTagNet
{
    class GLayoutResult : IGLayoutResult
    {
        Hashtable tagDisplyMap = new Hashtable();
        HashSet<PathEdge> edge = new HashSet<PathEdge>();
        public void AddCalc(string tag)
        {
            if (!tagDisplyMap.Contains(tag.ToLower()))
            {
                tagDisplyMap.Add(tag.ToLower(), tag);
            }
        }

        public bool HasCalc(string tag)
        {
            return tagDisplyMap.Contains(tag.ToLower());
        }
        public string GetDisplaceTag(string tag)
        {
            if (!HasCalc(tag)) return tag;
            else return tagDisplyMap[tag] as string ;
        }
        public IEnumerable<PathEdge> GetEdges()
        {
            return edge;
        }
        public void AddEdge(string parent,string child)
        {
            PathEdge pe = new PathEdge();
            pe.Parent = parent;
            pe.Child = child;
            if(!edge.Contains(pe))
            {
                edge.Add(pe);
            }
        }

        public void Clear()
        {
            tagDisplyMap.Clear();
            edge.Clear();
        }
    }
    
    class GObj
    {
        //公有属性：
        public Rect Content = new Rect();
        public Rect InnerBox = new Rect();
        public Rect OuterBox = new Rect();
        public double FontSize = GConfig.FontSize;
        public string Tag;
        public int Level = 0;

        //私有属性
        private ITagDB db = null;
        private Size childrenSize = new Size(0, 0);
        private Size parentSize = new Size(0, 0);
        List<GObj> gChildList = new List<GObj>();
        List<GObj> gParentList = new List<GObj>();
        List<GObj> gBrotherList = new List<GObj>();
        double XPadding = GConfig.XPadding;
        double YPadding = GConfig.YPadding;

        
        IRectLayoutCalc calc = new RectlayoutCalcImpl();



        private void MeasureTextWidth(string text, double fontSize, string fontFamily)
        {
            
            FormattedText formattedText = new FormattedText(
                text,
                System.Globalization.CultureInfo.InvariantCulture,
                FlowDirection.LeftToRight,
                new Typeface(fontFamily.ToString()),
                fontSize,
                Brushes.Black
            );
            Content.Height = formattedText.Height + GConfig.YContentPadding;
            Content.Width = formattedText.WidthIncludingTrailingWhitespace+ GConfig.XContentPadding;
            
        }
        
        
        public void CalcSize()
        {
            for(int i = 0;i<Distance;i++)
            {
                FontSize /= GConfig.ScaleInRadio;
                XPadding /= GConfig.ScaleInRadio;
                YPadding /= GConfig.ScaleInRadio;
            }
            if(Distance==0)
            {
                FontSize*=1.4;
            }
            XPadding = Math.Max(XPadding, GConfig.MinXPadding);
            YPadding = Math.Max(YPadding, GConfig.MinYPadding);
            FontSize = Math.Max(FontSize, GConfig.MinFontSize);
            MeasureTextWidth(Tag, FontSize, GConfig.GFontName);
            InnerBox.Width = Content.Width + XPadding ;
            InnerBox.Height = Content.Height + YPadding;
        }
        
        public IEnumerable<GObj> GetAll()
        {
            List<GObj> all = new List<GObj>();//include self and p,c,b
            all.Add(this);
            foreach(GObj c in gChildList)
            {
                all.AddRange(c.GetAll());
            }
            foreach(GObj p in gParentList)
            {
                all.AddRange(p.GetAll());
            }
            return all;

        }
        public int Distance;
        public static GObj ParseOut(string tag, string fromParent, string fromChild, ITagDB db, IGLayoutResult result, int level, int distance)
        {
            GObj g = null;
            for(int curLevel =GConfig.MinLevel;curLevel<=GConfig.MaxLevel;curLevel++)
            {
                GConfig.CurLevel = curLevel;
                result.Clear();
                g = Parse_in(tag, fromParent, fromChild, db, result, level, distance);
                if (g.GetAll().Count<GObj>() > GConfig.MinTagCnt) return g;
            }
            return g;


        }
        private static GObj Parse_in(string tag,string fromParent,string fromChild,ITagDB db,IGLayoutResult result, int level,int distance)
        {
            if (distance > GConfig.CurLevel) return null;
            if (result.HasCalc(tag)) return null;
            result.AddCalc(tag);
            List<string> alias = db.QueryTagAlias(tag);
            foreach(string a in alias)
            {
                result.AddCalc(a);
            }
            GObj ret = new GObj();
            ret.db = db;
            ret.Tag = tag;
            ret.Level = level;
            ret.Distance = distance;

            //List<string> b = db.GetBrothers(tag);
            List<string> children = db.QueryTagChildren(tag);
            List<string> parent = db.QueryTagParent(tag);

            //递归计算Parent区域 
            foreach (string pTag in parent)
            {
                if (pTag != tag)
                {
                    GObj pg = GObj.Parse_in(pTag, null, tag,db,result,level-1,distance+1);
                    if (pg != null)
                    {
                        ret.gParentList.Add(pg);
                    }
                    result.AddEdge(pTag, tag);
                }
            }
            if(ret.gParentList.Count>0)
            {
                ret.parentSize = GConfig.Radio;
                ret.calc.Calc(ref ret.parentSize, ret.gParentList, LayoutOption.FixRadio);
            }
            //递归计算Child区域
            foreach (string cTag in children)
            {
                if (cTag != tag)
                {
                    GObj cg = GObj.Parse_in(cTag, tag, null,db,result,level+1,distance+1);
                    if (cg != null)
                    {
                        ret.gChildList.Add(cg);
                    }
                    result.AddEdge(tag, cTag);
                }
            }
            if (ret.gChildList.Count > 0)
            {
                ret.childrenSize = GConfig.Radio;
                ret.calc.Calc(ref ret.childrenSize, ret.gChildList, LayoutOption.FixRadio);
            }

            ret.CalcSize();

            ret.OuterBox.X = 0;
            ret.OuterBox.Y = 0;
            ret.OuterBox.Width = Math.Max(ret.parentSize.Width, ret.childrenSize.Width);
            ret.OuterBox.Width = Math.Max(ret.OuterBox.Width, ret.InnerBox.Width);
            ret.OuterBox.Height = ret.InnerBox.Height + ret.parentSize.Height + ret.childrenSize.Height;
            return ret;
        }
        public void AdjustXY(double x,double y)
        {
            //先确定好自身元素的位置
            OuterBox.X += x;
            OuterBox.Y += y;
            InnerBox.X = OuterBox.X + (OuterBox.Width - InnerBox.Width) / 2;
            InnerBox.Y = OuterBox.Y + parentSize.Height;
            Content.X = InnerBox.X + XPadding;
            Content.Y = InnerBox.Y + YPadding;

            double parentX, parentY;
            parentX = OuterBox.X + (OuterBox.Width - parentSize.Width) / 2;
            parentY = OuterBox.Y;
            foreach (GObj p in gParentList)
            {
                p.AdjustXY(parentX, parentY);
            }

            double childX, childY;
            childX = OuterBox.X + (OuterBox.Width - childrenSize.Width) / 2;
            childY = OuterBox.Y + parentSize.Height + InnerBox.Height;
            foreach (GObj c in gChildList)
            {
                c.AdjustXY(childX,childY);
            }
        }

    }
}
