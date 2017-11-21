using System;
using System.Collections.Generic;
using System.Collections;
using TagExplorer.Utils;
using Newtonsoft.Json;
using System.IO;
using System.Diagnostics;
using TagExplorer.UriMgr;
using TagExplorer.AutoComplete;

namespace TagExplorer.TagMgr
{
    class JsonTagDB:ITagDB
    {
        DataChanged dbChangedHandler;
        public DataChanged TagDBChanged
        {
            get
            {
                return dbChangedHandler;
            }

            set
            {
                dbChangedHandler += value;
            }
        }


        //维护所有tag=》taginf（有可能有别名，存在多个tag对应一个tagInf）
        Hashtable id2Gutag = new Hashtable(); //Guid ==> Gutag
        private void ChangeNotify()
        {
            dbChangedHandler?.Invoke();
        }
        private void Save()
        {
            Save(null);
        }
        private void Save(GUTag tag)
        {
            
            using (StreamWriter w = new StreamWriter(CfgPath.TagDBPath_Json))
            {
                List<GUTag> all = new List<GUTag>();
                
                foreach(GUTag j in id2Gutag.Values)
                {
                    all.Add(j);
                }
                all.Sort((x, y) => x.Id.CompareTo(y.Id));
                foreach (GUTag j in all)
                {
                    w.WriteLine(JsonConvert.SerializeObject(j));
                }
                
            }

            ChangeNotify();
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
                    db.AddToHash(j);
                }
            }
            
            return db;
        }
        private void AssertValid(GUTag tag)
        {
            System.Diagnostics.Debug.Assert(id2Gutag[tag.Id] as GUTag == tag);
        }
        //////////////////////////////////////////////////////////
        public GUTag NewTag(string stag)
        {

            GUTag tag = new GUTag(stag);
            AddToHash(tag);
            return tag;
        }
        private void RemoveFromHash(GUTag j)
        {
            AssertValid(j);
            id2Gutag.Remove(j.Id);
            //AllTagSet.Remove(j);
        }
        private void AddToHash(GUTag j)
        {
            Debug.Assert(id2Gutag[j.Id] == null);
            id2Gutag[j.Id] = j;
            
        }
        //////////////////////////////////////////////////////////

        public int SetParent(GUTag parent, GUTag child)
        {
            //添加的tag必须是有效节点
            AssertValid(parent);
            AssertValid(child);
            GUTag pTag= id2Gutag[parent.Id] as GUTag;
            GUTag cTag = id2Gutag[child.Id] as GUTag;

            //保护性检查，防止调用无效
            if (pTag != null && cTag != null)
            {
                pTag.AddChild(cTag);
                Save(pTag);
                //Save(child);  parent保存实际上已经保存所有了，这儿就不需要保存了。
            }
            return ITagDBConst.R_OK;
        }

        public void Dispose()
        {
            id2Gutag.Clear();
        }

        
        
        
        public int MergeAlias(GUTag mainTag, GUTag aliasTag)
        {
            AssertValid(mainTag);
            AssertValid(aliasTag);
            RemoveFromHash(aliasTag);
            mainTag.Merge(aliasTag);
            AddToHash(mainTag);
            //allTag.Add(tag2, tmp1);//别名也需要快速索引
            Save();
            return ITagDBConst.R_OK;
        }


        #region  查询函数实现
        public int QueryChildrenCount(GUTag tag)
        {
            //AssertValid(tag);
            GUTag tmp = id2Gutag[tag.Id] as GUTag;
            return tmp == null ? 0 : tmp.Children.Count;

        }

        private string ParentHistory(GUTag a)
        {
            string ret = a.Title;
            
            while(a!=null)
            {
                var parents = QueryTagParent(a);
                if (parents.Count == 0) break;
                else
                {
                    a = parents[0];
                    ret = ret + ">" + a.Title;
                }
            }
            return ret;
        }
        public List<AutoCompleteTipsItem> QueryAutoComplete(string searchTerm,bool forceOne = false)
        {
            string ls = searchTerm.ToLower();
            List<AutoCompleteTipsItem> ret = new List<AutoCompleteTipsItem>();
            foreach(GUTag s in id2Gutag.Values)
            {
                if(s.Title.ToLower().Contains(ls))
                {
                    AutoCompleteTipsItem a = new AutoCompleteTipsItem();
                    a.Content = s.Title;
                    a.Tip = ParentHistory(s);
                    a.Data = s;

                    //完全匹配，奖励1000分
                    if (searchTerm == s.Title)
                    {
                        a.Score += 10000;
                    }
                    //惩罚：长度差越大，惩罚越多
                    a.Score -= Math.Abs(a.Content.Length - searchTerm.Length);
                    //惩罚：路径越长，惩罚越多
                    a.Score -= (a.Tip.Length)*10;
                    ret.Add(a);
                }
            }
            ret.Sort((x,y)=>y.Score.CompareTo(x.Score));//Score越大越好

            //如果没有找到对应Tag，而且需要保证非空时，返回一个非空的内容
            if(forceOne && ret.Count==0)
            {
                AutoCompleteTipsItem a = new AutoCompleteTipsItem();
                a.Content = searchTerm;
                a.Tip = searchTerm;
                //a.Data = GUTag.Parse(StaticCfg.Ins.DefaultTagID.ToString(), this);
                ret.Add(a);
            }
            return ret;
        }

        public List<string> QueryTagAlias(GUTag tag)
        {
            //AssertValid(tag);
            if (id2Gutag[tag.Id] as GUTag != tag) return new List<string>();
            else return tag.Alias;
        }
        
        public List<GUTag> QueryTagChildren(GUTag tag)
        {
            //AssertValid(tag);
            if (id2Gutag[tag.Id] as GUTag != tag) return new List<GUTag>();

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
            //AssertValid(tag); 由于有两个视图，可能会用一个已经失效的GUTag进行查询。
            if (id2Gutag[tag.Id] as GUTag != tag) return new List<GUTag>();


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
        public List<GUTag> QueryTags(string title)
        {
            List<GUTag> ret = new List<GUTag>();
            foreach (GUTag tag in id2Gutag.Values)
            {
                if (tag.Title == title) ret.Add(tag);
            }
            return ret;
        }
        #endregion


        public int RemoveTag(GUTag tag)
        {
            AssertValid(tag);
            RemoveParentsRef(tag);
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
            SetParent(parent, child);
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

        public int ChangeChildPos(GUTag tag, int direct)
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

        

        public GUTag ChangeTitle(GUTag tag, string newTitle)
        {
            AssertValid(tag);
            tag.ChangeTitle(newTitle);
            Save();
            return tag;
        }
    }
    
    
}
