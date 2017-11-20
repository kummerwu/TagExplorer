using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using TagExplorer.Utils;

namespace TagExplorer.TagMgr
{
    [Serializable]
    public class GUTag
    {
        #region 构造函数和简单数据
        //自身信息：ID，Title，别名（其中Title在实现上就是Alias[0]
        public Guid Id;
        //父节点信息
        public Guid PId;

        public GUTag() { }
        public GUTag(string title)
        {
            if (title == StaticCfg.Ins.DefaultTag) Id = StaticCfg.Ins.DefaultTagID;
            else Id = Guid.NewGuid();
            Alias.Add(title);
        }
        public GUTag(string title, Guid id)
        {
            Id = id;
            Alias.Add(title);
        }
        #endregion

        #region 别名和标题
        public List<string> Alias = new List<string>();
        [JsonIgnore]
        public string Title { get { return Alias.Count > 0 ? Alias[0] : ""; } }

        public string AliasString()
        {
            string ret = "";
            for(int i = 1;i<Alias.Count;i++)
            {
                ret += Alias[i] + "\n";
            }
            return ret.Trim();
        }

        //添加一个别名
        public void AddAlias(string title)
        {
            if (!Alias.Contains(title))
            {
                Alias.Add(title);
            }
        }
        //修改Title
        public void ChangeTitle(string title)
        {
            if (title == Title) return;
            if (Alias.Count > 0) Alias.RemoveAt(0);
            if (Alias.Contains(title))
            {
                Alias.Remove(title);
            }
            Alias.Insert(0, title);
        }
        #endregion

        #region 子节点列表管理
        //子节点信息
        public List<Guid> Children = new List<Guid>();
        public string ChildrenString()
        {
            string ret = "";
            for (int i = 0; i < Children.Count; i++)
            {
                ret += Children[i] + "\n";
            }
            return ret.Trim();
        }
        
        
        

        //修改Child节点的位置（direct=-1：下移一个，1：上移一个）
        public void ChangePos(GUTag child, int direct)
        {
            int idx = Children.IndexOf(child.Id);
            int newIdx = idx + direct;
            if (newIdx >= 0 && newIdx < Children.Count)
            {
                Children.RemoveAt(idx);
                Children.Insert(newIdx, child.Id);
            }
        }

        private int GetChildPos(GUTag child)
        {
            return Children.IndexOf(child.Id);
        }

        public void AddChild(GUTag c)
        {
            AddChild(c.Id);
            c.PId = Id;
        }
        public void AddChild(Guid cid)
        {
            if (Children.Contains(cid)) return;
            Children.Add(cid);
        }


        public void RemoveChild(GUTag c)
        {
            if (Children.Contains(c.Id)) Children.Remove(c.Id);
        }

        public void Merge(GUTag other)
        {
            foreach (string a in other.Alias) AddAlias(a);
            foreach (Guid c in other.Children) AddChild(c);
            //foreach (string p in tag.Parents) AddParent(p);
        }
        #endregion

        #region 序列化和反序列化
        const char SplitToken = '→';
        public override string ToString()
        {
            return  Id.ToString()+ SplitToken + Title;
        }

        public static GUTag Parse(string strGutag,ITagDB db)
        {
            Guid id = Guid.Empty;

            if (strGutag.IndexOf(SplitToken)!=-1)
            {
                string sID = strGutag.Split(SplitToken)[0];
                id = Guid.Parse(sID);
            }
            return db.GetTag(id);
        }
        #endregion

        #region 比较函数
        public override bool Equals(object obj)
        {
            return this == obj as GUTag;
        }
        public override int GetHashCode()
        {
            return Id.GetHashCode();
        }
        public static bool operator == (GUTag l,GUTag r)
        {
            //需要强制转换，否则==会递归死循环。
            if (null == (object)l && null == (object)r) return true;
            else if (null != (object)l && null != (object)r) return l.Id == r.Id;
            else return false;
        }
        public static bool operator != (GUTag l,GUTag r)
        {
            return !(l==r);
        }
        #endregion
    }
}
