using AnyTag.UI;
using AnyTagNet.BL;
using LuceneTest.Core;
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
        public Rect Content = new Rect();   //自身文字所占用的矩形大小（加了一个ContentPadding）
        public Rect InnerBox = new Rect();  //自身文字所占的区域（再加了一个XPadding和YPadding）

        public Rect OuterBox = new Rect();  //整个Gobj所占区域，包括三部分（Parent区域，自身区域，Children区域）


        public double FontSize = GConfig.FontSize;
        public string Tag;
        public int Level = 0;
        public int Distance; //本节点离当前节点（跟节点的距离）

        //私有属性
        private ITagDB db = null;
        private Size ChildBox = new Size(0, 0);
        private Size ParentBox = new Size(0, 0);
        List<GObj> gChildList = new List<GObj>();
        List<GObj> gParentList = new List<GObj>();
        List<GObj> gBrotherList = new List<GObj>();
        double InnerBoxXPadding = double.NaN;// GConfig.InnerBoxXPadding_MAX;
        double InnerBoxYPadding = double.NaN;// GConfig.InnerBoxYPadding_MAX;
        

        IRectLayoutCalc calc = new RectlayoutCalcImpl();
        IGLayoutResult result = null;

        public bool OverlayWith(GObj other)
        {
            //Logger.Log("{0}-{1}-{2}     ## {3}-{4}-{5}",
            //                this.Tag, this.InnerBox, this.OuterBox,
            //                other.Tag, other.InnerBox, other.OuterBox);
            return (this.OuterBox.IntersectsWith(other.OuterBox));


            if (this.Tag == other.Tag) return false;
            IEnumerable<GObj> thisAll = GetAll();
            IEnumerable<GObj> otherAll = other.GetAll();
            foreach(GObj thisOne in thisAll)
            {
                foreach(GObj otherOne in otherAll)
                {
                    if(thisOne.Tag!=otherOne.Tag && thisOne.InnerBox.IntersectsWith(otherOne.InnerBox))
                    {
                        //Logger.Log("{0}-{1}-{2}     @@ {3}-{4}-{5}",
                        //    thisOne.Tag, thisOne.InnerBox, thisOne.OuterBox,
                        //    otherOne.Tag, otherOne.InnerBox, otherOne.OuterBox);
                        return true;
                    }
                }
            }
            return false;
        }

        //计算自身内容所占区域的大小
        private void CalcContentSize(string text, double fontSize, string fontFamily)
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
        
        //计算InnerBox（自身内容+Padding）的大小
        public void CalcInnerBoxSize()
        {
            InnerBoxXPadding = GConfig.InnerBoxXPadding_MAX;
            InnerBoxYPadding = GConfig.InnerBoxYPadding_MAX;
            for (int i = 0;i<Distance;i++)
            {
                FontSize /= GConfig.ScaleInRadio;
                InnerBoxXPadding /= GConfig.ScaleInRadio;
                InnerBoxYPadding /= GConfig.ScaleInRadio;
            }
            if(Distance==0)
            {
                FontSize*=1.4;
            }
            InnerBoxXPadding = Math.Max(InnerBoxXPadding, GConfig.InnerBoxXPadding_MIN);
            InnerBoxYPadding = Math.Max(InnerBoxYPadding, GConfig.InnerBoxYPadding_MIN);
            FontSize = Math.Max(FontSize, GConfig.MinFontSize);
            CalcContentSize(Tag, FontSize, GConfig.GFontName);
            InnerBox.Width = Content.Width + InnerBoxXPadding ;
            InnerBox.Height = Content.Height + InnerBoxYPadding;
        }
        //返回所有对象，包括自己，父节点和子节点(以及递归的所有父子节点)
        //TODO：该函数可以优化，实际上并不需要所有节点，只需要指导当前整个图中有多少个节点（以便在节点太多的时候，停止继续遍历）
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
        
        public static GObj LayoutTag(string tag, ITagDB db)
        {
            Logger.Log("+++Begin Layout Tag from {0}", tag);
            IGLayoutResult result = new GLayoutResult();
            GObj g = null;
            for(int curLevel =GConfig.MinLevel;curLevel<=GConfig.MaxLevel;curLevel++)
            {
                GConfig.CurLevel = curLevel;
                result.Clear();
                g = Parse_in(tag, null, null, db, result, 0, 0);
                if (g.GetAll().Count<GObj>() > GConfig.MinTagCnt) break; 
            }

            if (g != null)
            {
                g.CalcAllObjsPos(0, 0);
                g.result = result;
            }
            Logger.Log("---End Layout Tag from {0}", tag);
            return g;


        }
        private void ShowAll()
        {
            Logger.IN(Tag);
            Logger.Log("   ShowAll@!! {0} # {1}  ##{2}", Tag, InnerBox, OuterBox);
            foreach (GObj o in GetAll())
            {
                Logger.Log("   ShowAll!! {0} # {1}  ##{2}", o.Tag, o.InnerBox, o.OuterBox);
            }
            Logger.OUT();
        }
        private static GObj Parse_in(string tag,string fromParent,string fromChild,ITagDB db,IGLayoutResult result, int level,int distance)
        {
            Logger.IN(tag);
            Logger.Log(@"Parse In Tag {0} from {1},level={2},distance = {3}", tag, fromParent, level, distance);
            
            if (!((level == -1 && distance == 1) || level == distance))
            {
                Logger.Log("     !level distance not match,Skip!");
                Logger.OUT();
                return null;
            }
            if (distance > GConfig.CurLevel)
            {
                Logger.Log("     !distance out,Skip!");
                Logger.OUT();
                return null;
            }
            if (result.HasCalc(tag))
            {
                Logger.Log("     !not first visit,Skip!");
                Logger.OUT();
                return null;
            }

            
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


            Logger.Log("Begin Visit All Parent:{0}===>",tag);
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
                ret.ParentBox = GConfig.Radio;
                ret.calc.Calc(ref ret.ParentBox, ret.gParentList, LayoutOption.FixRadio);
            }
            Logger.Log("End Visit All Parent :{0}<===", tag);



            //递归计算Child区域
            Logger.Log("Begin Visit All children:{0}===>", tag);
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
                ret.ChildBox = GConfig.Radio;
                ret.calc.Calc(ref ret.ChildBox, ret.gChildList, LayoutOption.FixRadio);
            }
            Logger.Log("End Visit All children :{0}<===", tag);
            ret.CalcInnerBoxSize();
            ret.CalcOutterBoxSize();
            ret.CalcInnerBoxPos();
            ret.ShowAll();
            Logger.OUT();
            return ret;
        }
        private void CalcOutterBoxSize()
        {
            GObj ret = this;
            //ret.OuterBox.X = 0;
            //ret.OuterBox.Y = 0;
            SetOutterBoxPos(0, 0);
            ret.OuterBox.Width = Math.Max(ret.ParentBox.Width, ret.ChildBox.Width);
            ret.OuterBox.Width = Math.Max(ret.OuterBox.Width, ret.InnerBox.Width);
            ret.OuterBox.Height = ret.InnerBox.Height + ret.ParentBox.Height + ret.ChildBox.Height;
        }
        public void SetOutterBoxPos(double x,double y)
        {
            OuterBox.X = x;
            OuterBox.Y = y;
            CalcInnerBoxPos();
        }
        private void CalcInnerBoxPos()
        {
            InnerBox.X = OuterBox.X + (OuterBox.Width - InnerBox.Width) / 2;
            InnerBox.Y = OuterBox.Y + ParentBox.Height;
            Content.X = InnerBox.X + InnerBoxXPadding;
            Content.Y = InnerBox.Y + InnerBoxYPadding;
        }
        //计算所有对象的位置信息
        private void CalcAllObjsPos(double Left,double Top)
        {
            //先确定好自身元素的位置
            OuterBox.X += Left;
            OuterBox.Y += Top;

            CalcInnerBoxPos();

            //调整所有父节点的位置
            double leftParentRange, topParentRange;
            leftParentRange = OuterBox.X + (OuterBox.Width - ParentBox.Width) / 2;
            topParentRange = OuterBox.Y;
            foreach (GObj p in gParentList)
            {
                p.CalcAllObjsPos(leftParentRange, topParentRange);
            }

            //调整所有子节点的位置
            double leftChildRange, topChildRange;
            leftChildRange = OuterBox.X + (OuterBox.Width - ChildBox.Width) / 2;
            topChildRange = OuterBox.Y + ParentBox.Height + InnerBox.Height;
            foreach (GObj c in gChildList)
            {
                c.CalcAllObjsPos(leftChildRange,topChildRange);
            }
        }

        internal IEnumerable<PathEdge> GetEdges()
        {
            if (result != null) return result.GetEdges();
            else return new PathEdge[0];
        }
    }
}
