using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Serialization;
using System.Collections;
using TagExplorer.Utils;
using Newtonsoft.Json;
using System.IO;
using System.Xml.Serialization;

namespace TagExplorer.TagMgr
{
    class JsonTagDB:ITagDB
    {
        Hashtable Str2TagIdx = new Hashtable();
        HashSet<JTagInf> AllTagSet = new HashSet<JTagInf>();
        public void Save(string tag)
        {
            
            using (StreamWriter w = new StreamWriter(CfgPath.TagDBPath_Json))
            {
                foreach(JTagInf j in AllTagSet)
                {
                    if (j.Children.Count > 0)
                    {
                        w.WriteLine(JsonConvert.SerializeObject(j));
                    }
                }
            }
            

        }
        public static JsonTagDB Load()
        {
            string jFile = CfgPath.TagDBPath_Json;
            JsonTagDB db =  new JsonTagDB();
            if (File.Exists(jFile))
            {
                string[] lns = File.ReadAllLines(jFile);
                foreach(string ln in lns)
                {
                    JTagInf j = JsonConvert.DeserializeObject<JTagInf>(ln);
                    db.UpdateIndex(j);
                    db.AllTagSet.Add(j);
                }
            }
            
            return db;
        }
        
        //////////////////////////////////////////////////////////
        private JTagInf NewTag(string stag)
        {
            JTagInf tag = Str2TagIdx[stag] as JTagInf;
            if (tag == null)
            {
                tag = new JTagInf(stag);
                Str2TagIdx.Add(stag, tag);
                AllTagSet.Add(tag);
            }
            return tag;
        }
        private void DeleteTag(JTagInf j)
        {
            foreach (string a in j.Alias)
            {
                Str2TagIdx.Remove(a);
            }
            AllTagSet.Remove(j);
        }
        private void UpdateIndex(JTagInf j)
        {
            foreach (string a in j.Alias)
            {
                Str2TagIdx.Remove(a);
                Str2TagIdx[a] = j;
            }
        }
        //////////////////////////////////////////////////////////

        public int AddTag(string parent, string child)
        {
            JTagInf tag= Str2TagIdx[parent] as JTagInf;
            if(tag==null)
            {
                tag = NewTag(parent);
            }
            tag.AddChild(child);

            JTagInf cTag = Str2TagIdx[child] as JTagInf;
            if (cTag == null)
            {
                cTag = NewTag(child);
            }
            


            Save(parent);
            //Save(child);  parent保存实际上已经保存所有了，这儿就不需要保存了。
            return ITagDBConst.R_OK;
        }

        public void Dispose()
        {
            Str2TagIdx.Clear();
        }

        public int QueryChildrenCount(string tag)
        {
            JTagInf tmp = Str2TagIdx[tag] as JTagInf;
            return tmp == null ? 0 : tmp.Children.Count;

        }
        
        
        public int MergeAlias(string tag1, string tag2)
        {
            JTagInf tmp1 = Str2TagIdx[tag1] as JTagInf;
            if(tmp1==null)
            {
                tmp1 = NewTag(tag1);
            }
            JTagInf tmp2 = Str2TagIdx[tag2] as JTagInf;
            if (tmp2 == null)
            {
                tmp2 = NewTag(tag2);
            }

            
            DeleteTag(tmp2);
            tmp1.Merge(tmp2);
            UpdateIndex(tmp1);
            //allTag.Add(tag2, tmp1);//别名也需要快速索引
            return ITagDBConst.R_OK;
        }

        public List<string> QueryAutoComplete(string searchTerm)
        {
            string ls = searchTerm.ToLower();
            List<string> ret = new List<string>();
            foreach(string s in Str2TagIdx.Keys)
            {
                if(s.ToLower().Contains(ls))
                {
                    ret.Add(s);
                }
            }
            return ret;
        }

        public List<string> QueryTagAlias(string tag)
        {
            JTagInf tmp = Str2TagIdx[tag] as JTagInf;
            return tmp == null ? EMPTY_LIST : tmp.Alias;
        }
        private static List<string> EMPTY_LIST = new List<string>();
        public List<string> QueryTagChildren(string tag)
        {
            JTagInf tmp = Str2TagIdx[tag] as JTagInf;
            return tmp == null ? EMPTY_LIST : tmp.Children;
        }

        public List<string> QueryTagParent(string tag)
        {
            List<string> ret = new List<string>();
            foreach(JTagInf j in AllTagSet)
            {
                if(j.Children.Contains(tag))
                {
                    ret.Add(j.Title);
                }
            }
            return ret;
            //return tmp == null ? EMPTY_LIST : tmp.Parents;
        }

        public int RemoveTag(string tag)
        {
            JTagInf tmp = Str2TagIdx[tag] as JTagInf;
            if (tmp != null)
            {
                //所有自己的别名索引页需要删除
                DeleteTag(tmp);
                //删除所有Parent的索引
                List<string> parents = QueryTagParent(tag);
                foreach(string p in parents)
                {
                    JTagInf pj = Str2TagIdx[p] as JTagInf;
                    if(pj!=null)
                    {
                        pj.RemoveChild(tag);
                    }
                }
                Save(tag);
            }
            return ITagDBConst.R_OK;
        }

        //将child原来所有parent删除，并与新的parent建立关系
        public int ResetParent(string parent, string child)
        {
            RemoveAllRelation(child);
            AddTag(parent, child);
            Save(parent);
            return ITagDBConst.R_OK;
        }

        private void RemoveAllRelation(string child)
        {
            foreach(JTagInf j in AllTagSet)
            {
                j.RemoveChild(child);
            }
            //JTagInf tmp = tagIdx[child] as JTagInf;
            //if (tmp != null)
            //{
            //    foreach (string oldP in tmp.Parents)
            //    {
            //        JTagInf op = tagIdx[oldP] as JTagInf;
            //        if (op != null)
            //        {
            //            op.RemoveChild(child);
            //        }
            //    }
            //    tmp.Parents.Clear();
            //}
        }

        public int ChangePos(string tag, int direct)
        {
            List<string> parents = QueryTagParent(tag);
            if(parents.Count==1)
            {
                JTagInf parent = Str2TagIdx[parents[0]] as JTagInf;
                if(parent!=null)
                {
                    parent.ChangePos(tag, direct);
                }

            }
            return ITagDBConst.R_OK;

        }
    }
    [Serializable]
    class JTagInf
    {
        public JTagInf(string n)
        {
            AddAlias(n);
        }
        public string Title { get { return Alias.Count>0?Alias[0]:""; } }
        public List<string> Alias = new List<string>();
        //public List<string> Parents = new List<string>();
        public List<string> Children = new List<string>();
        public void ChangePos(string c,int direct)
        {
            int idx = Children.IndexOf(c);
            int newIdx= idx + direct;
            if(newIdx>=0 && newIdx<Children.Count)
            {
                Children.RemoveAt(idx);
                Children.Insert(newIdx, c);
            }
        }
        private void Check()
        {
            foreach(string a in Alias)
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
        public void Add(string s,List<string> l)
        {
            if (!l.Contains(s)) l.Add(s);
            Check();
        }
        public void AddChild(string c)
        {
            Add(c, Children);
        }
        //public void AddParent(string p)
        //{
        //    Add(p, Parents);
        //}
        public void AddAlias(string a)
        {
            Add(a, Alias);
        }
        public void SetMainName(string n)
        {
            if(Alias.Contains(n))
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
    }
}
