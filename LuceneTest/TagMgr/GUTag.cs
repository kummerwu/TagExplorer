using System;
using System.Collections.Generic;

namespace TagExplorer.TagMgr
{
    [Serializable]
    public class GUTag
    {
        public Guid Id;
        public List<string> Alias = new List<string>();
        public List<Guid> Children = new List<Guid>();

        public string Title { get { return Alias.Count > 0 ? Alias[0] : ""; } }
        
        public GUTag(string title)
        {
            Id = Guid.NewGuid();
            Alias.Add(title);
        }
        public GUTag(string title,Guid id)
        {
            Id = id;
            Alias.Add(title);
        }
        public void AddAlias(string title)
        {
            if(!Alias.Contains(title))
            {
                Alias.Add(title);
            }
        }
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

        public int GetChildPos(GUTag child)
        {
            return Children.IndexOf(child.Id);
        }

        public void AddChild(GUTag c)
        {
            AddChild(c.Id);
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

        public void UpdateChild(GUTag oldChild, GUTag newChild)
        {
            for (int i = 0; i < Children.Count; i++)
            {
                if (Children[i] == oldChild.Id)
                {
                    Children[i] = newChild.Id;
                }
            }
        }
        public void UpdateAlias(string oldTag, string newTag)
        {
            if (Alias.Contains(oldTag))
            {
                for (int i = 0; i < Alias.Count; i++)
                {
                    if (Alias[i] == oldTag)
                    {
                        Alias[i] = newTag;
                    }
                }
            }
            else
            {
                Alias.Add(newTag);
            }
        }


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
    }
}
