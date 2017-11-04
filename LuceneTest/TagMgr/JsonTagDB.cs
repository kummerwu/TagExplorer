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
using System.Diagnostics;

namespace TagExplorer.TagMgr
{
    class JsonTagDB:ITagDB
    {
        //维护所有tag=》taginf（有可能有别名，存在多个tag对应一个tagInf）
        Hashtable id2Gutag = new Hashtable(); //Guid ==> Gutag
        
        private void Save()
        {
            Save(null);
        }
        private void Save(GUTag tag)
        {
            
            using (StreamWriter w = new StreamWriter(CfgPath.TagDBPath_Json))
            {
                foreach(GUTag j in id2Gutag.Values)
                {
                    w.WriteLine(JsonConvert.SerializeObject(j));
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
                    GUTag j = JsonConvert.DeserializeObject<GUTag>(ln);
                    db.AddToDB(j);
                }
            }
            
            return db;
        }
        private void AssertValid(GUTag tag)
        {
            System.Diagnostics.Debug.Assert(id2Gutag[tag.Id] == tag);
        }
        //////////////////////////////////////////////////////////
        public GUTag NewTag(string stag)
        {

            GUTag tag = new GUTag(stag);
            AddToDB(tag);
            return tag;
        }
        private void DeleteTag(GUTag j)
        {
            AssertValid(j);
            id2Gutag.Remove(j.Id);
            //AllTagSet.Remove(j);
        }
        private void AddToDB(GUTag j)
        {
            Debug.Assert(id2Gutag[j.Id] == null);
            id2Gutag[j.Id] = j;
            
        }
        //////////////////////////////////////////////////////////

        public int AddTag(GUTag parent, GUTag child)
        {
            AssertValid(parent);
            AssertValid(child);
            GUTag tag= id2Gutag[parent.Id] as GUTag;
            GUTag cTag = id2Gutag[child.Id] as GUTag;
            

            tag.AddChild(child);
            
            Save(parent);
            //Save(child);  parent保存实际上已经保存所有了，这儿就不需要保存了。
            return ITagDBConst.R_OK;
        }

        public void Dispose()
        {
            id2Gutag.Clear();
        }

        public int QueryChildrenCount(GUTag tag)
        {
            AssertValid(tag);
            GUTag tmp = id2Gutag[tag.Id] as GUTag;
            return tmp == null ? 0 : tmp.Children.Count;

        }
        
        
        public int MergeAlias(GUTag mainTag, GUTag aliasTag)
        {
            AssertValid(mainTag);
            AssertValid(aliasTag);
            DeleteTag(aliasTag);
            mainTag.Merge(aliasTag);
            AddToDB(mainTag);
            //allTag.Add(tag2, tmp1);//别名也需要快速索引
            Save();
            return ITagDBConst.R_OK;
        }

        public List<string> QueryAutoComplete(string searchTerm)
        {
            string ls = searchTerm.ToLower();
            List<string> ret = new List<string>();
            foreach(GUTag s in id2Gutag.Values)
            {
                if(s.Title.ToLower().Contains(ls))
                {
                    ret.Add(s.Title);
                }
            }
            return ret;
        }

        public List<string> QueryTagAlias(GUTag tag)
        {
            AssertValid(tag);
            if (id2Gutag[tag.Id] != tag) return new List<string>();
            else return tag.Alias;
        }
        private static List<string> EMPTY_LIST = new List<string>();
        public List<GUTag> QueryTagChildren(GUTag tag)
        {
            AssertValid(tag);
            if (id2Gutag[tag.Id] != tag) return new List<GUTag>();

            List<GUTag> gutagChildren= new List<GUTag>();
            foreach(Guid id in tag.Children)
            {
                GUTag c = id2Gutag[id] as GUTag;
                if(c!=null)
                {
                    gutagChildren.Add(c);
                }
            }
            return gutagChildren;
        }

        public List<GUTag> QueryTagParent(GUTag tag)
        {
            AssertValid(tag);
            if (id2Gutag[tag.Id] != tag) return new List<GUTag>();


            List<GUTag> ret = new List<GUTag>();
            
            foreach (GUTag j in id2Gutag.Values)
            {
                if(j.Children.Contains(tag.Id))
                {
                    ret.Add(j);
                }
            }
            return ret;
            
        }

        public int RemoveTag(GUTag tag)
        {
            AssertValid(tag);
            id2Gutag.Remove(tag.Id);
            Save(tag);
            return ITagDBConst.R_OK;
        }

        //将child原来所有parent删除，并与新的parent建立关系
        public int ResetParent(GUTag parent,GUTag child)
        {
            AssertValid(parent);
            AssertValid(child);
            RemoveParentsRef(child);
            AddTag(parent, child);
            Save(parent);
            return ITagDBConst.R_OK;
        }

        private void RemoveParentsRef(GUTag child)
        {
            AssertValid(child);
            foreach (GUTag j in id2Gutag.Values)
            {
                j.RemoveChild(child);
            }
            
        }

        public int ChangePos(GUTag tag, int direct)
        {
            AssertValid(tag);
            List<GUTag> parents = QueryTagParent(tag);
            Debug.Assert(parents.Count == 1);
            if(parents.Count==1)
            {
                GUTag parent = parents[0];
                parent.ChangePos(tag, direct);
                Save(tag);
            }
            return ITagDBConst.R_OK;

        }
        
        public int Import(string importInf)
        {
            int ret = 0;
            GUTag dtag = NewTag(StaticCfg.Ins.DefaultTag);

            Hashtable title2GUtag = new Hashtable();
            title2GUtag.Add(StaticCfg.Ins.DefaultTag, dtag);
            if (File.Exists(importInf))
            {
                string[] lns = File.ReadAllLines(importInf);
                //第一遍，恢复tag本身的信息
                foreach (string ln in lns)
                {
                    JTagInf j = JsonConvert.DeserializeObject<JTagInf>(ln);

                    GUTag tag = title2GUtag[j.Title] as GUTag;
                    if(tag==null)
                    {
                        tag = NewTag(j.Title);
                        if(title2GUtag[j.Title]==null)
                            title2GUtag.Add(j.Title, tag);
                    }
                    foreach (string a in j.Alias)
                    {
                        tag.AddAlias(a);
                        if (title2GUtag[a] == null)
                            title2GUtag.Add(a, tag);
                    }
                    //把子节点先创建出来
                    foreach (string c in j.Children)
                    {
                        if (title2GUtag[c] == null)
                        {
                            GUTag ctag = NewTag(c);
                            if (title2GUtag[c] == null)
                                title2GUtag.Add(c, ctag);
                        }
                    }
                    
                }
                //第二步，恢复child信息
                foreach (string ln in lns)
                {
                    JTagInf j = JsonConvert.DeserializeObject<JTagInf>(ln);

                    GUTag tag = title2GUtag[j.Title] as GUTag;
                    Debug.Assert(tag != null);
                    
                    //把子节点先创建出来
                    foreach (string c in j.Children)
                    {
                        GUTag ctag = title2GUtag[c] as GUTag;
                        Debug.Assert(ctag != null);
                        tag.AddChild(ctag);
                    }

                }
            }
            Save();
            return ret;
        }

        public GUTag GetTag(Guid id)
        {
            return id2Gutag[id] as GUTag;
        }

        public List<GUTag> QueryTags(string title)
        {
            List<GUTag> ret = new List<GUTag>();
            foreach (GUTag tag in id2Gutag.Values)
            {
                if (tag.Title == title) ret.Add(tag);
            }
            return ret;
        }

        public int ChangeTitle(GUTag tag, string newTitle)
        {
            AssertValid(tag);
            tag.ChangeTitle(newTitle);
            Save();
            return ITagDBConst.R_OK;
        }
    }
    
    
}
