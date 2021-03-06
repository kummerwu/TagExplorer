﻿using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
using System.Diagnostics;
using System.IO;
using TagExplorer.AutoComplete;
using TagExplorer.UriMgr;
using TagExplorer.Utils;

namespace TagExplorer.TagMgr
{
    public class SQLTagDB:IDisposable,ITagDB
    {

        #region 构造函数和析构函数
        public SQLTagDB()
        {
            if(StaticCfg.Ins.Opt.SqliteTagCacheOn)
            {
                id2TagCache = new Hashtable();
            }
        }
        public static SQLTagDB Load()
        {
            return IDisposableFactory.New<SQLTagDB>(new SQLTagDB());
            //数据库连接惰性打开
        }
        public void Dispose()
        {

            id2TagCache?.Clear();

            if (adduptCmd != null)
            {
                adduptCmd.Dispose();
                adduptCmd = null;
            }
            if (delCmd != null)
            {
                delCmd.Dispose();
                delCmd = null;
            }
            if (queryCmd != null)
            {
                queryCmd.Dispose();
                queryCmd = null;
            }
            if (con != null)
            {
                con.Close();
                con.Dispose();
                con = null;
            }
            if (SQLiteConnection.ConnectionPool != null)
            {
                SQLiteConnection.ConnectionPool.ClearAllPools();
            }
            SQLiteConnection.ClearAllPools();
            GC.Collect();
            GC.WaitForPendingFinalizers();
        }
        #endregion

        #region 变更通知
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
        private void ChangeNotify()
        {
            dbChangedHandler?.Invoke();
        }
        #endregion

        #region  SQLite数据库封装
        SQLiteConnection con = null;
        SQLiteConnection Conn
        {
            get
            {
                if(con==null)
                {
                    string file = CfgPath.TagDBPath_SQLite;
                    con = new SQLiteConnection("Data Source=" + file);
                    if (!File.Exists(file))
                    {
                        con.Open();
                        string sql = @"CREATE TABLE [Tags](
                                    [ID] GUID  primary key,     
                                    [Title] TEXT,     
                                    [Alias] TEXT,     
                                    [PID] GUID,     
                                    [Children] TEXT);
                                    ";
                        SQLiteCommand cmd = new SQLiteCommand(sql, con);
                        cmd.ExecuteNonQuery();

                        GUTag defaultTag = new GUTag(StaticCfg.Ins.DefaultTag, StaticCfg.Ins.DefaultTagID);
                        AddUptSqlDB(defaultTag);
                    }
                    else
                    {
                        con.Open();
                    }
                    
                }
                return con;
            }
        }
        SQLiteCommand adduptCmd = null;
        public void AddUptSqlDB(GUTag tag)
        {
            if(adduptCmd==null)
            {
                adduptCmd = new SQLiteCommand(@"REPLACE INTO Tags (ID,Title,Alias,PID,Children)
VALUES (@ID,@Title,@Alias,@PID,@Children)",Conn);
                adduptCmd.Parameters.AddRange(new []{
                    new SQLiteParameter("@ID", DbType.Guid),
                    new SQLiteParameter("@Title", DbType.String),
                    new SQLiteParameter("@Alias", DbType.String),
                    new SQLiteParameter("@PID", DbType.Guid),
                    new SQLiteParameter("@Children", DbType.String),
                    });
                
            }
            adduptCmd.Parameters[0].Value = tag.Id;
            adduptCmd.Parameters[1].Value = tag.Title;
            adduptCmd.Parameters[2].Value = tag.AliasString();
            adduptCmd.Parameters[3].Value = tag.PId;
            adduptCmd.Parameters[4].Value = tag.ChildrenString();
            adduptCmd.ExecuteNonQuery();

        }
        SQLiteCommand delCmd = null;
        public void DelSqlDB(GUTag tag)
        {
            if (delCmd == null)
            {
                delCmd = new SQLiteCommand(@"DELETE FROM Tags where (ID = @ID)", Conn);
                delCmd.Parameters.AddRange(new[]{
                    new SQLiteParameter("@ID", DbType.Guid),
                    
                    });

            }
            delCmd.Parameters[0].Value = tag.Id;
            delCmd.ExecuteNonQuery();
        }
        SQLiteCommand queryCmd = null;
        public GUTag QuerySqlDB(Guid id)
        {
            if(queryCmd==null)
            {
                queryCmd = new SQLiteCommand(@"SELECT * FROM Tags where (ID=@ID)", Conn);
                queryCmd.Parameters.AddRange(new[] {
                    new SQLiteParameter("@ID",DbType.Guid),
                    });
            }

            queryCmd.Parameters[0].Value = id;
            using (SQLiteDataReader r = queryCmd.ExecuteReader())
            {
                if(r.Read())
                {
                    GUTag tag = ReadGUTagFromR(r);
                    return tag;
                }
                else
                {
                    return null;
                }
            }
            
        }
        SQLiteCommand qTitleCmd = null;
        public List<GUTag> QueryTitleSqlDB(string title)
        {
            
            if (qTitleCmd == null)
            {
                qTitleCmd = new SQLiteCommand(@"SELECT * FROM Tags where (Title=@Title)", Conn);
                qTitleCmd.Parameters.AddRange(new[] {
                    new SQLiteParameter("@Title",DbType.String),
                    });
            }

            List<GUTag> ret = new List<GUTag>();
            qTitleCmd.Parameters[0].Value = title;
            using (SQLiteDataReader r = qTitleCmd.ExecuteReader())
            {
                
                while (r.Read())
                {
                    GUTag tag = ReadGUTagFromR(r);
                    ret.Add(tag);
                }
            }
            return ret;
        }

        private static GUTag ReadGUTagFromR(SQLiteDataReader r)
        {
            GUTag tag = new GUTag();
            //0. ID
            tag.Id = r.GetGuid(0);
            //1. Title
            tag.AddAlias(r.GetString(1));
            //2. Alias
            string alias = r.GetString(2);
            string[] aliasList = alias.Split(new char[] { '\n' }, StringSplitOptions.RemoveEmptyEntries);
            foreach (string a in aliasList)
            {
                tag.AddAlias(a);
            }

            //3. PID
            tag.PId = r.GetGuid(3);
            //4. Children
            string chilrend = r.GetString(4);
            string[] childList = chilrend.Split(new char[] { '\n' }, StringSplitOptions.RemoveEmptyEntries);
            foreach (string c in childList)
            {
                tag.AddChild(Guid.Parse(c));
            }

            return tag;
        }
        SQLiteCommand qWildCmd = null;
        public List<GUTag> QueryWildSql(string title)
        {

            if (qWildCmd == null)
            {
                qWildCmd = new SQLiteCommand(@"SELECT * FROM Tags where (Title like @Title or Alias like @Title)", Conn);
                qWildCmd.Parameters.AddRange(new[] {
                    new SQLiteParameter("@Title",DbType.String),
                    });
            }

            List<GUTag> ret = new List<GUTag>();
            qWildCmd.Parameters[0].Value = '%'+title+'%';
            using (SQLiteDataReader r = qWildCmd.ExecuteReader())
            {
                while (r.Read())
                {
                    GUTag tag = ReadGUTagFromR(r);
                    ret.Add(tag);
                }
            }
            return ret;
        }

        #endregion

        

        /// </summary>

        //维护所有tag=》taginf（有可能有别名，存在多个tag对应一个tagInf）
        Hashtable id2TagCache = null;// new Hashtable(); //Guid ==> Gutag
        
        
        //private void Save(GUTag tag)
        //{
        //    AddUptSqlDB(tag);
        //    ChangeNotify();
        //}
        
        private void AssertValid(GUTag tag)
        {
            System.Diagnostics.Debug.Assert(QueryTag(tag.Id)== tag);
        }
        #region 新建Tag
        public GUTag NewTag(string title)
        {
            GUTag tag = new GUTag(title);
            SaveAndUpdateCache(tag);
            //ChangeNotify();//这个地方可以不用notify，在设置父子关系的时候再notify
            return tag;
        }
        private void SaveAndUpdateCache(GUTag j)
        {
            //Debug.Assert(id2Gutag[j.Id] == null);
            if (id2TagCache!=null)
            {
                id2TagCache[j.Id] = j;
            }
            AddUptSqlDB(j);

        }
        #endregion

        #region 删除Tag
        private void RemoveFromHash(GUTag j)
        {
            AssertValid(j);
            
            id2TagCache?.Remove(j.Id);
            
            DelSqlDB(j);
            //AllTagSet.Remove(j);
        }
        public int RemoveTag(GUTag tag)
        {
            tag = QueryTag(tag.Id);
            if (tag == null) return ITagDBConst.R_OK;

            AssertValid(tag);
            RemoveChild(tag);
            id2TagCache?.Remove(tag.Id);
            DelSqlDB(tag);
            ChangeNotify();
            return ITagDBConst.R_OK;
        }
        #endregion
        //////////////////////////////////////////////////////////

        #region 修改Tag：父子关系
        //建立两个Tag之间的父子关系
        public int SetParent(GUTag parent, GUTag child)
        {
            //添加的tag必须是有效节点
            AssertValid(parent);
            AssertValid(child);
            GUTag pTag = QueryTag(parent.Id);
            GUTag cTag = QueryTag(child.Id);

            //保护性检查，防止调用无效
            if (pTag != null && cTag != null)
            {
                pTag.AddChild(cTag);
                System.Diagnostics.Debug.Assert(cTag.PId == pTag.Id);
                AddUptSqlDB(pTag);
                AddUptSqlDB(cTag);
                ChangeNotify();

                //Save(child);  parent保存实际上已经保存所有了，这儿就不需要保存了。
            }
            return ITagDBConst.R_OK;
        }
        //解除原来child所有parent，并与新的parent建立关系
        public int ResetParent(GUTag parent, GUTag child)
        {
            parent = QueryTag(parent.Id);
            child = QueryTag(child.Id);
            if (parent == null || child == null) return ITagDBConst.R_OK;
            AssertValid(parent);
            AssertValid(child);
            RemoveChild(child);
            SetParent(parent, child);
            AddUptSqlDB(parent);
            ChangeNotify();
            return ITagDBConst.R_OK;
        }

        private void RemoveChild(GUTag child)
        {
            AssertValid(child);
            GUTag pTag = QueryTag(child.PId);
            if (pTag != null)
            {
                pTag.RemoveChild(child);
                AddUptSqlDB(pTag);
            }
            child.PId = Guid.Empty;



        }

        public int ChangeChildPos(GUTag tag, int direct)
        {
            tag = QueryTag(tag.Id);
            if (tag == null) return ITagDBConst.R_OK;

            AssertValid(tag);
            List<GUTag> parents = QueryTagParent(tag);
            Debug.Assert(parents.Count == 1);
            if (parents.Count == 1)
            {
                GUTag parent = parents[0];
                parent.ChangePos(tag, direct);
                AddUptSqlDB(parent);
                AddUptSqlDB(tag);
                ChangeNotify();
            }
            return ITagDBConst.R_OK;

        }
        #endregion


        #region 修改Tag：标题和别名

        public int MergeAlias(GUTag mainTag, GUTag aliasTag)
        {
            AssertValid(mainTag);
            AssertValid(aliasTag);
            mainTag = QueryTag(mainTag.Id);
            aliasTag = QueryTag(aliasTag.Id);
            RemoveFromHash(aliasTag);
            mainTag.Merge(aliasTag);
            SaveAndUpdateCache(mainTag);
            //allTag.Add(tag2, tmp1);//别名也需要快速索引
            ChangeNotify();
            return ITagDBConst.R_OK;
        }
        public GUTag ChangeTitle(GUTag tag, string newTitle)
        {
            tag = QueryTag(tag.Id);
            if (tag == null) return null;

            AssertValid(tag);
            tag.ChangeTitle(newTitle);
            AddUptSqlDB(tag);
            ChangeNotify();
            return tag;
        }

        #endregion

        #region  查询函数实现
        public GUTag GetTag(Guid id)
        {
            return QueryTag(id);
        }
        public GUTag QueryTag(Guid id)
        {
            if (id2TagCache!=null)
            {
                GUTag tmp = id2TagCache[id] as GUTag;
                if (tmp == null)
                {
                    tmp = QuerySqlDB(id);
                    if (tmp != null)
                    {
                        id2TagCache[id] = tmp;
                    }
                }
                return tmp;
            }
            else
            {
                return QuerySqlDB(id);
            }
        }
        public int QueryChildrenCount(GUTag tag)
        {
            //AssertValid(tag);
            GUTag tmp = QueryTag(tag.Id);
            return tmp == null ? 0 : tmp.Children.Count;

        }

        private string ParentHistory(GUTag a)
        {
            a = QueryTag(a.Id);
            string ret = a.Title;

            while (a != null)
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

        //待优化，在SQLIte中怎样自动补充
        
        public List<AutoCompleteTipsItem> QueryAutoComplete(string searchTerm, bool forceOne = false)
        {
            string ls = searchTerm.ToLower();
            List<AutoCompleteTipsItem> ret = new List<AutoCompleteTipsItem>();
            List<GUTag> tags = QueryWildSql(ls);
            foreach (GUTag s in tags)
            {
                if (s.Title.ToLower().Contains(ls))
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
                    a.Score -= (a.Tip.Length) * 10;
                    ret.Add(a);
                }
            }
            ret.Sort((x, y) => y.Score.CompareTo(x.Score));//Score越大越好

            //如果没有找到对应Tag，而且需要保证非空时，返回一个非空的内容
            if (forceOne && ret.Count == 0)
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
            tag = QueryTag(tag.Id);
            if(tag== null) return new List<string>();
            
            else return tag.Alias;
        }

        public List<GUTag> QueryTagChildren(GUTag tag)
        {
            //AssertValid(tag);
            tag = QueryTag(tag.Id);
            if (tag == null) return new List<GUTag>();

            List<GUTag> gutagChildren = new List<GUTag>();
            foreach (Guid id in tag.Children)
            {
                GUTag c = QueryTag(id);
                if (c != null)
                {
                    gutagChildren.Add(c);
                }
            }
            return gutagChildren;
        }

        public List<GUTag> QueryTagParent(GUTag tag)
        {
            //AssertValid(tag); 由于有两个视图，可能会用一个已经失效的GUTag进行查询。
            tag = QueryTag(tag.Id);
            if (null== tag) return new List<GUTag>();
            if(tag.Id == StaticCfg.Ins.DefaultTagID) return new List<GUTag>();


            List<GUTag> ret = new List<GUTag>();
            //如果有ParentID，直接返回
            if (tag.PId != null)
            {
                GUTag ptag = QueryTag(tag.PId);
                ret.Add(ptag);

            }
            
            return ret;

        }
        public List<GUTag> QueryTags(string title)
        {
            List<GUTag> ret = QueryTitleSqlDB(title);
            return ret;
        }
        #endregion

        #region 从老的json数据导出数据库
        
        public int Import(string importInf)
        {
            if (!File.Exists(importInf)) return 0;

            int newCnt = 0, uptCnt = 0;
            Hashtable title2GUtag = new Hashtable();
            string[] lns = File.ReadAllLines(importInf);
            List<GUTag> oldGUTags = new List<GUTag>();
            foreach (string ln in lns)
            {
                GUTag iTag = JsonConvert.DeserializeObject<GUTag>(ln);
                if (iTag != null)
                {
                    GUTag oTag = QueryTag(iTag.Id);
                    if(oTag==null)
                    {
                        SaveAndUpdateCache(iTag);
                        newCnt++;
                    }
                    else if(!iTag.IsSame(oTag))
                    {
                        SaveAndUpdateCache(GUTag.MergeTag(iTag, oTag));
                        uptCnt++;
                    }
                    else
                    {
                        //两边完全相同，不用处理
                    }
                }

            }
            ChangeNotify();
            return 0;
        }

        public int Export(string exportFile)
        {

            using (StreamWriter w = new StreamWriter(exportFile))
            {
                List<GUTag> all = new List<GUTag>();
                SQLiteCommand q = new SQLiteCommand(@"SELECT * FROM Tags", Conn);
                using (SQLiteDataReader r = q.ExecuteReader())
                {
                    while (r.Read())
                    {
                        GUTag tag = ReadGUTagFromR(r);
                        if(tag!=null)
                        {
                            all.Add(tag);
                        }
                    }
                   
                }
                
                all.Sort((x, y) => x.Id.CompareTo(y.Id));
                foreach (GUTag j in all)
                {
                    w.WriteLine(JsonConvert.SerializeObject(j));
                }

            }
            return 0;
        }
        #endregion
    }
}
