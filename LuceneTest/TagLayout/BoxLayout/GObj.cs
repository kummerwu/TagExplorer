using TagExplorer.TagMgr;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using TagExplorer.Utils;
using TagExplorer.TagLayout.LayoutCommon;

namespace TagExplorer.BoxLayout
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
    
    class GBoxObj
    {
        private void InitBox(string tag,int level,int distance)
        {
            GTagBox = new GTagBox(distance,tag,0,0,1);
        }
        //公有属性：
        //public Rect Content = new Rect();   //自身文字所占用的矩形大小（加了一个ContentPadding）
        //public Rect InnerBox = new Rect();  //自身文字所占的区域（再加了一个XPadding和YPadding）
        public GTagBox GTagBox ;

        public Rect OuterBox = new Rect();  //整个Gobj所占区域，包括三部分（Parent区域，自身区域，Children区域）


        public Rect ColorBox { get { return GTagBox.InnerBox; } }
        public Rect TextBox { get { return GTagBox.OutterBox; } }


        //私有属性

        private ITagDB db = null;
        private Size ChildBox = new Size(0, 0);
        private Size ParentBox = new Size(0, 0);
        List<GBoxObj> gChildList = new List<GBoxObj>();
        List<GBoxObj> gParentList = new List<GBoxObj>();
        List<GBoxObj> gBrotherList = new List<GBoxObj>();
        
        public string Tag { get { return GTagBox.Tag; } }
        

        IRectLayoutCalc calc = new RectlayoutCalcImpl();
        IGLayoutResult result = null;

        public bool OverlayWith(GBoxObj other)
        {
            //Logger.Log("{0}-{1}-{2}     ## {3}-{4}-{5}",
            //                this.Tag, this.InnerBox, this.OuterBox,
            //                other.Tag, other.InnerBox, other.OuterBox);
            return (this.OuterBox.IntersectsWith(other.OuterBox));


            if (this.Tag == other.Tag) return false;
            IEnumerable<GBoxObj> thisAll = GetAll();
            IEnumerable<GBoxObj> otherAll = other.GetAll();
            foreach(GBoxObj thisOne in thisAll)
            {
                foreach(GBoxObj otherOne in otherAll)
                {
                    if(thisOne.Tag!=otherOne.Tag && thisOne.TextBox.IntersectsWith(otherOne.TextBox))
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

        
        
        
        //返回所有对象，包括自己，父节点和子节点(以及递归的所有父子节点)
        //TODO：该函数可以优化，实际上并不需要所有节点，只需要指导当前整个图中有多少个节点（以便在节点太多的时候，停止继续遍历）
        public IEnumerable<GBoxObj> GetAll()
        {
            List<GBoxObj> all = new List<GBoxObj>();//include self and p,c,b
            all.Add(this);
            foreach(GBoxObj c in gChildList)
            {
                all.AddRange(c.GetAll());
            }
            foreach(GBoxObj p in gParentList)
            {
                all.AddRange(p.GetAll());
            }
            return all;

        }
        
        public static GBoxObj LayoutTag(string tag, ITagDB db,double Top,double Left)
        {
            Logger.D("+++Begin Layout Tag from {0}", tag);
            IGLayoutResult result = new GLayoutResult();
            GBoxObj g = null;
            for(int curLevel =CfgTagGraph.MinLevel;curLevel<=CfgTagGraph.MaxLevel;curLevel++)
            {
                CfgTagGraph.CurLevel = curLevel;
                result.Clear();
                g = Parse_in(tag, null, null, db, result, 0, 0);
                if (g.GetAll().Count<GBoxObj>() > CfgTagGraph.MinTagCnt) break; 
            }

            if (g != null)
            {
                g.CalcAllObjsPos(Left, Top);
                g.result = result;
            }
            Logger.D("---End Layout Tag from {0}", tag);
            return g;


        }
        private void ShowAll()
        {
            Logger.IN(Tag);
            Logger.D("   ShowAll@!! {0} # {1}  ##{2}", Tag, TextBox, OuterBox);
            foreach (GBoxObj o in GetAll())
            {
                Logger.D("   ShowAll!! {0} # {1}  ##{2}", o.Tag, o.TextBox, o.OuterBox);
            }
            Logger.OUT();
        }
        private static GBoxObj Parse_in(string tag,string fromParent,string fromChild,ITagDB db,IGLayoutResult result, int level,int distance)
        {
            Logger.IN(tag);
            Logger.D(@"Parse In Tag {0} from {1},level={2},distance = {3}", tag, fromParent, level, distance);
            
            if (!((level == -1 && distance == 1) || level == distance))
            {
                Logger.D("     !level distance not match,Skip!");
                Logger.OUT();
                return null;
            }
            if (distance > CfgTagGraph.CurLevel)
            {
                Logger.D("     !distance out,Skip!");
                Logger.OUT();
                return null;
            }
            if (result.HasCalc(tag))
            {
                Logger.D("     !not first visit,Skip!");
                Logger.OUT();
                return null;
            }

            
            result.AddCalc(tag);
            List<string> alias = db.QueryTagAlias(tag);
            foreach(string a in alias)
            {
                result.AddCalc(a);
            }
            
            GBoxObj ret = new GBoxObj();
            ret.db = db;
            ret.InitBox(tag, level, distance);
            //ret.box.db = db;
            //ret.box.Tag = tag;
            //ret.box.Level = level;
            //ret.box.Distance = distance;
            //ret.Tag = tag;
            //ret.Level = level;
            //ret.Distance = distance;

            //List<string> b = db.GetBrothers(tag);
            List<string> children = db.QueryTagChildren(tag);
            List<string> parent = db.QueryTagParent(tag);


            Logger.D("Begin Visit All Parent:{0}===>",tag);
            //递归计算Parent区域 
            foreach (string pTag in parent)
            {
                if (pTag != tag)
                {
                    GBoxObj pg = GBoxObj.Parse_in(pTag, null, tag,db,result,level-1,distance+1);
                    if (pg != null)
                    {
                        ret.gParentList.Add(pg);
                    }
                    result.AddEdge(pTag, tag);
                }
            }
            if(ret.gParentList.Count>0)
            {
                ret.ParentBox = CfgTagGraph.Radio;
                ret.calc.Calc(ref ret.ParentBox, ret.gParentList, LayoutOption.FixRadio);
            }
            Logger.D("End Visit All Parent :{0}<===", tag);



            //递归计算Child区域
            Logger.D("Begin Visit All children:{0}===>", tag);
            foreach (string cTag in children)
            {
                if (cTag != tag)
                {
                    GBoxObj cg = GBoxObj.Parse_in(cTag, tag, null,db,result,level+1,distance+1);
                    if (cg != null)
                    {
                        ret.gChildList.Add(cg);
                    }
                    result.AddEdge(tag, cTag);
                }
            }
            if (ret.gChildList.Count > 0)
            {
                ret.ChildBox = CfgTagGraph.Radio;
                ret.calc.Calc(ref ret.ChildBox, ret.gChildList, LayoutOption.FixRadio);
            }
            Logger.D("End Visit All children :{0}<===", tag);
            //ret.box.Init(distance, tag);
            ret.CalcOutterBoxSize();
            ret.GTagBox.CalcInnerBoxPos(ret.OuterBox,ret.ParentBox);
            ret.ShowAll();
            Logger.OUT();
            return ret;
        }
        private void CalcOutterBoxSize()
        {
            GBoxObj ret = this;
            //ret.OuterBox.X = 0;
            //ret.OuterBox.Y = 0;
            SetOutterBoxPos(0, 0);
            ret.OuterBox.Width = Math.Max(ret.ParentBox.Width, ret.ChildBox.Width);
            ret.OuterBox.Width = Math.Max(ret.OuterBox.Width, ret.TextBox.Width);
            ret.OuterBox.Height = ret.TextBox.Height + ret.ParentBox.Height + ret.ChildBox.Height;
        }
        public void SetOutterBoxPos(double x,double y)
        {
            OuterBox.X = x;
            OuterBox.Y = y;
            GTagBox.CalcInnerBoxPos(OuterBox,ParentBox);
        }
        
        //计算所有对象的位置信息
        private void CalcAllObjsPos(double Left,double Top)
        {
            //先确定好自身元素的位置
            OuterBox.X += Left;
            OuterBox.Y += Top;

            GTagBox.CalcInnerBoxPos(OuterBox,ParentBox);

            //调整所有父节点的位置
            double leftParentRange, topParentRange;
            leftParentRange = OuterBox.X + (OuterBox.Width - ParentBox.Width) / 2;
            topParentRange = OuterBox.Y;
            foreach (GBoxObj p in gParentList)
            {
                p.CalcAllObjsPos(leftParentRange, topParentRange);
            }

            //调整所有子节点的位置
            double leftChildRange, topChildRange;
            leftChildRange = OuterBox.X + (OuterBox.Width - ChildBox.Width) / 2;
            topChildRange = OuterBox.Y + ParentBox.Height + TextBox.Height;
            foreach (GBoxObj c in gChildList)
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
