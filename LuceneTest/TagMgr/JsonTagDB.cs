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
        Hashtable tagIdx = new Hashtable();
        HashSet<JTagInf> allTag = new HashSet<JTagInf>();
        public void Save(string tag)
        {
            
            using (StreamWriter w = new StreamWriter(CfgPath.TagDBPath_Json))
            {
                foreach(JTagInf j in allTag)
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
                    //foreach(string p in j.Parents)
                    //{
                    //    db.AddTag(p,j.Alias[0]);
                    //}
                    //foreach (string c in j.Children)
                    //{
                    //    db.AddTag(j.Alias[0], c);
                    //}
                    db.UpdateIndex(j);
                    db.allTag.Add(j);
                }
            }
            return db;
        }
        //////////////////////////////////////////////////////////
        private JTagInf NewTag(string stag)
        {
            JTagInf tag = tagIdx[stag] as JTagInf;
            if (tag == null)
            {
                tag = new JTagInf(stag);
                tagIdx.Add(stag, tag);
                allTag.Add(tag);
            }
            return tag;
        }
        private void DeleteTag(JTagInf j)
        {
            foreach (string a in j.Alias)
            {
                tagIdx.Remove(a);
            }
            allTag.Remove(j);
        }
        private void UpdateIndex(JTagInf j)
        {
            foreach (string a in j.Alias)
            {
                tagIdx.Remove(a);
                tagIdx[a] = j;
            }
        }
        //////////////////////////////////////////////////////////

        public int AddTag(string parent, string child)
        {
            JTagInf tag= tagIdx[parent] as JTagInf;
            if(tag==null)
            {
                tag = NewTag(parent);
            }
            tag.AddChild(child);

            JTagInf cTag = tagIdx[child] as JTagInf;
            if (cTag == null)
            {
                cTag = NewTag(child);
            }
            cTag.AddParent(parent);


            Save(parent);
            //Save(child);  parent保存实际上已经保存所有了，这儿就不需要保存了。
            return ITagDBConst.R_OK;
        }

        public void Dispose()
        {
            tagIdx.Clear();
        }

        public int GetTagChildrenCount(string tag)
        {
            JTagInf tmp = tagIdx[tag] as JTagInf;
            return tmp == null ? 0 : tmp.Children.Count;

        }
        
        
        public int MergeAliasTag(string tag1, string tag2)
        {
            JTagInf tmp1 = tagIdx[tag1] as JTagInf;
            if(tmp1==null)
            {
                tmp1 = NewTag(tag1);
            }
            JTagInf tmp2 = tagIdx[tag2] as JTagInf;
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
            foreach(string s in tagIdx.Keys)
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
            JTagInf tmp = tagIdx[tag] as JTagInf;
            return tmp == null ? EMPTY_LIST : tmp.Alias;
        }
        private static List<string> EMPTY_LIST = new List<string>();
        public List<string> QueryTagChildren(string tag)
        {
            JTagInf tmp = tagIdx[tag] as JTagInf;
            return tmp == null ? EMPTY_LIST : tmp.Children;
        }

        public List<string> QueryTagParent(string tag)
        {
            JTagInf tmp = tagIdx[tag] as JTagInf;
            return tmp == null ? EMPTY_LIST : tmp.Parents;
        }

        public int RemoveTag(string tag)
        {
            JTagInf tmp = tagIdx[tag] as JTagInf;
            if (tmp != null)
            {
                //所有别名索引页需要删除
                DeleteTag(tmp);
                Save(tag);
            }
            return ITagDBConst.R_OK;
        }

        //将child原来所有parent删除，并与新的parent建立关系
        public int ResetRelationOfChild(string parent, string child)
        {
            RemoveAllRelation(child);
            AddTag(parent, child);
            Save(parent);
            return ITagDBConst.R_OK;
        }

        private void RemoveAllRelation(string child)
        {
            JTagInf tmp = tagIdx[child] as JTagInf;
            if (tmp != null)
            {
                foreach (string oldP in tmp.Parents)
                {
                    JTagInf op = tagIdx[oldP] as JTagInf;
                    if (op != null)
                    {
                        op.RemoveChild(child);
                    }
                }
                tmp.Parents.Clear();
            }
        }
    }
    [Serializable]
    class JTagInf
    {
        public JTagInf(string n)
        {
            AddAlias(n);
        }
        public List<string> Alias = new List<string>();
        public List<string> Parents = new List<string>();
        public List<string> Children = new List<string>();
        public void Add(string s,List<string> l)
        {
            if (!l.Contains(s)) l.Add(s);
        }
        public void AddChild(string c)
        {
            Add(c, Children);
        }
        public void AddParent(string p)
        {
            Add(p, Parents);
        }
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
            foreach (string p in tag.Parents) AddParent(p);
        }
    }
}
