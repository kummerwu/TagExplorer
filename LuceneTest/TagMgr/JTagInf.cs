using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace TagExplorer.TagMgr
{
    [Serializable]
    class JTagInf
    {
        public JTagInf(string n)
        {
            AddAlias(n);
        }
        [JsonIgnore]
        public string Title { get { return Alias.Count > 0 ? Alias[0] : ""; } }
        public List<string> Alias = new List<string>();
        //public List<string> Parents = new List<string>();
        public List<string> Children = new List<string>();
        public void ChangePos(string c, int direct)
        {
            int idx = Children.IndexOf(c);
            int newIdx = idx + direct;
            if (newIdx >= 0 && newIdx < Children.Count)
            {
                Children.RemoveAt(idx);
                Children.Insert(newIdx, c);
            }
        }
        public int GetPos(string c)
        {
            return Children.IndexOf(c);
        }
        private void Check()
        {
            foreach (string a in Alias)
            {
                //foreach(string p in Parents)
                //{
                //    System.Diagnostics.Debug.Assert(a != p);
                //}
                foreach (string c in Children)
                {
                    System.Diagnostics.Debug.Assert(a != c);
                }
            }
        }
        public void Add(string s, List<string> l)
        {
            if (!l.Contains(s)) l.Add(s);
            Check();
        }
        public void AddChild(string c)
        {
            if (Alias.Contains(c)) return;
            Add(c, Children);
        }
        //public void AddParent(string p)
        //{
        //    Add(p, Parents);
        //}
        public void AddAlias(string a)
        {
            if (Children.Contains(a))
            {
                Children.Remove(a);
            }
            Add(a, Alias);
        }
        public void SetMainName(string n)
        {
            if (Alias.Contains(n))
            {
                Alias.Remove(n);
            }
            Alias.Insert(0, n);
        }
        public void RemoveChild(string c)
        {
            if (Children.Contains(c)) Children.Remove(c);
        }
        public void Merge(JTagInf tag)
        {
            foreach (string a in tag.Alias) AddAlias(a);
            foreach (string c in tag.Children) AddChild(c);
            //foreach (string p in tag.Parents) AddParent(p);
        }

        public void UpdateChild(string oldChild, string newChild)
        {
            for (int i = 0; i < Children.Count; i++)
            {
                if (Children[i] == oldChild)
                {
                    Children[i] = newChild;
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
    }
}
