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
        //维护所有tag=》taginf（有可能有别名，存在多个tag对应一个tagInf）
        Hashtable Str2TagIdx = new Hashtable();
        HashSet<JTagInf> GetAllJTagInf()
        {
            HashSet<JTagInf> ret = new HashSet<JTagInf>();
            foreach(JTagInf j in Str2TagIdx.Values)
            {
                if (!ret.Contains(j)) ret.Add(j);
            }
            return ret;
        }
        private void Save()
        {
            Save(null);
        }
        private void Save(string tag)
        {
            HashSet<JTagInf> AllTagSet = GetAllJTagInf();
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
                    //db.AllTagSet.Add(j);
                }
            }
            HashSet<JTagInf> AllTagSet = db.GetAllJTagInf();
            //叶子节点没有存储，但内存索引中需要，在这儿自动恢复出来
            List<string> leafNotExist = new List<string>();
            foreach(JTagInf j in AllTagSet)
            {
                foreach(string a in j.Alias)
                {
                    if (!db.Str2TagIdx.Contains(a))
                    {
                        leafNotExist.Add(a);
                    }
                }
                foreach(string c in j.Children)
                {
                    if (!db.Str2TagIdx.Contains(c))
                    {
                        leafNotExist.Add(c);
                    }
                }
            }
            foreach(string leaf in leafNotExist)
            {
                db.NewTag(leaf);
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
                //AllTagSet.Add(tag);
            }
            return tag;
        }
        private void DeleteTag(JTagInf j)
        {
            foreach (string a in j.Alias)
            {
                Str2TagIdx.Remove(a);
            }
            //AllTagSet.Remove(j);
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
        
        
        public int MergeAlias(string mainTag, string aliasTag)
        {
            JTagInf jMain = Str2TagIdx[mainTag] as JTagInf;
            if(jMain==null)
            {
                jMain = NewTag(mainTag);
            }
            JTagInf jAlias = Str2TagIdx[aliasTag] as JTagInf;
            if (jAlias == null)
            {
                jAlias = NewTag(aliasTag);
            }

            
            DeleteTag(jAlias);
            jMain.Merge(jAlias);
            UpdateIndex(jMain);
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
            var AllTagSet = GetAllJTagInf();
            foreach (JTagInf j in AllTagSet)
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
                
            }
            //删除所有Parent的索引
            RemoveParentsRef(tag);
            Save(tag);
            return ITagDBConst.R_OK;
        }

        //将child原来所有parent删除，并与新的parent建立关系
        public int ResetParent(string parent, string child)
        {
            RemoveParentsRef(child);
            AddTag(parent, child);
            Save(parent);
            return ITagDBConst.R_OK;
        }

        private void RemoveParentsRef(string child)
        {
            var AllTagSet = GetAllJTagInf();
            foreach (JTagInf j in AllTagSet)
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
                    Save(tag);
                }

            }
            return ITagDBConst.R_OK;

        }

        public int Import(string importInf)
        {
            int ret = 0;
            if (File.Exists(importInf))
            {
                string[] lns = File.ReadAllLines(importInf);
                foreach (string ln in lns)
                {
                    JTagInf j = JsonConvert.DeserializeObject<JTagInf>(ln);
                    if(j.Alias.Count>0)
                    {
                        string title = j.Alias[0];
                        foreach(string a in j.Alias)
                        {
                            if(a!=title)
                            {
                                ret++;
                                MergeAlias(title, a);
                            }
                        }
                        foreach(string c in j.Children)
                        {
                            ret++;
                            AddTag(title, c);

                        }
                    }
                }
            }
            Save();
            return ret;
        }

        public int UpdateTag(string oldTag, string newTag)
        {
            
            List<string> parents = QueryTagParent(oldTag);
            if (parents.Count == 0) return ITagDBConst.R_OK;


            string parent = parents[0];
            JTagInf jParent = Str2TagIdx[parent] as JTagInf;
            if (jParent == null) return ITagDBConst.R_OK;


            JTagInf jOld = Str2TagIdx[oldTag] as JTagInf;
            if (jOld == null) return ITagDBConst.R_OK;

            jParent.UpdateChild(oldTag, newTag);
            jOld.UpdateAlias(oldTag, newTag);
            Str2TagIdx.Remove(oldTag);
            UpdateIndex(jOld);

            Save();
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
        [JsonIgnore]
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
        public int GetPos(string c)
        {
            return Children.IndexOf(c);
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
            if (Alias.Contains(c)) return;
            Add(c, Children);
        }
        //public void AddParent(string p)
        //{
        //    Add(p, Parents);
        //}
        public void AddAlias(string a)
        {
            if(Children.Contains(a))
            {
                Children.Remove(a);
            }
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

        public void UpdateChild(string oldChild, string newChild)
        {
            for (int i = 0;i<Children.Count;i++)
            {
                if(Children[i]==oldChild)
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
