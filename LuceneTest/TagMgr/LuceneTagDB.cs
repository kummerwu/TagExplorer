using Contrib.Regex;
using Lucene.Net.Analysis;
using Lucene.Net.Documents;
using Lucene.Net.Index;
using Lucene.Net.Search;
using Lucene.Net.Store;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using TagExplorer.Utils;

namespace TagExplorer.TagMgr
{
    class LuceneTagDB : ITagDB, IDisposable
    {
        
        const string F_TAGNAME = "tname";
        const string F_TAGCHILD = "tchildren";

        IndexWriter writer = null;
        IndexReader reader = null;
        IndexSearcher search = null;
        Directory dir = null;
        public LuceneTagDB()
        {
            Logger.I("Create TagDB Instance!");
            bool create = true;
            if (Cfg.Ins.IsUTest)
            {
                dir = new RAMDirectory();
                create = true;
            }
            else
            {
                Logger.I("DBPath = " + CfgPath.TagDBPath);
                create = !System.IO.Directory.Exists(CfgPath.TagDBPath);
                if (create)
                {
                    Logger.I("First Create !DBPath = " + CfgPath.TagDBPath);
                    System.IO.Directory.CreateDirectory(CfgPath.TagDBPath);
                }
                dir = FSDirectory.Open(CfgPath.TagDBPath);
                
            }
            
            
            writer = new IndexWriter(dir,
                                    new KeywordAnalyzer(),
                                    create, 
                                    IndexWriter.MaxFieldLength.UNLIMITED);


            reader = writer.GetReader();
            search = new IndexSearcher(reader);
            DBChanged();
            
        }

        public void Dispose()
        {
            search.Dispose();
            reader.Dispose();
            writer.Dispose();
        }
        Document[] GetParentDocs(string child)
        {
            Term term = new Term(F_TAGCHILD, child);
            Query query = new TermQuery(term);
            ScoreDoc[] docs = search.Search(query, CfgPerformance.TAG_MAX_RELATION).ScoreDocs;
            Document []docArray = new Document[docs.Length];
            //foreach(ScoreDoc doc in docs)
            for(int i = 0;i<docs.Length;i++)
            {
                docArray[i] = search.Doc(docs[i].Doc);
            }
            return docArray;
        }
        Document GetDoc(string tag)
        {
            Term term = new Term(F_TAGNAME, tag);
            Query query = new TermQuery(term);
            ScoreDoc[] docs = search.Search(query, 1).ScoreDocs;
            Document doc = null;
            if(docs.Length==1)
            {
                doc = search.Doc(docs[0].Doc);
            }
            return doc;
        }

        //重置child的父节点为parent
        public int ResetParent(string parent,string child)
        {
            RemoveAllRelation( child);
            AddTag(parent, child);
            Commit();
            foreach (string p in QueryTagParent(child))
            {
                System.Diagnostics.Debug.WriteLine(QueryTagParent(child));
            }
            System.Diagnostics.Debug.Assert(QueryTagParent(child).Count == 1);
            System.Diagnostics.Debug.Assert(QueryTagChildren(parent).Contains(child));
            return 0;
        }

        private void RemoveAllRelation(string child)
        {
            Document[] parents = GetParentDocs(child);
            foreach (Document doc in parents)
            {
                //if (doc.Get(F_TAGNAME) == parent) continue;//这个文档不用修改,前面AddTag已经添加了。

                Document newDoc = new Document();
                newDoc.Add(doc.GetField(F_TAGNAME));
                foreach (Field f in doc.GetFields(F_TAGCHILD))
                {
                    if (f.StringValue != child)
                    {
                        newDoc.Add(f);
                    }
                }
                writer.UpdateDocument(new Term(F_TAGNAME, doc.Get(F_TAGNAME)), newDoc);
            }
            Commit();
        }
        private void DBChanged()
        {
            TipsCenter.Ins.TagDBInf = "当前标签数据库中存在 ： "+ reader.MaxDoc +" 已删除："+reader.NumDeletedDocs;
        }
        public int AddTag(string parent, string child)
        {
            int ret = ITagDBConst.R_OK;
            if (parent == null || child == null) return ret;


            Document parentDoc = GetDoc(parent);
            if(parentDoc==null)
            {
                parentDoc = new Document();
                parentDoc.Add(new Field(F_TAGNAME, parent, Field.Store.YES, Field.Index.NOT_ANALYZED));
                parentDoc.Add(new Field(F_TAGCHILD, child, Field.Store.YES, Field.Index.NOT_ANALYZED));
                writer.AddDocument(parentDoc);
            }
            else
            {
                Field[] fs = parentDoc.GetFields(F_TAGCHILD);
                foreach(Field f in fs)
                {
                    if(f.StringValue == child)
                    {
                        return ITagDBConst.R_OK;
                    }
                }
                parentDoc.Add(new Field(F_TAGCHILD, child, Field.Store.YES, Field.Index.NOT_ANALYZED));
                Term term = new Term(F_TAGNAME, parent);
                writer.UpdateDocument(term, parentDoc);
            }

            Document childDoc = GetDoc(child);
            if(childDoc==null)
            {
                childDoc = new Document();
                childDoc.Add(new Field(F_TAGNAME, child, Field.Store.YES, Field.Index.NOT_ANALYZED));
                writer.AddDocument(childDoc);
            }
            Commit();
            return ret;
        }
        private void Commit()
        {
            //Logger.Log("Commit");
            //writer.Flush(true,true,true);
            writer.Commit(); //kummer： 原来调用了Flush，但是没有调用Commit，程序重启以后，所有数据似乎都丢失了。
            //Logger.Log("getRader");
            reader = writer.GetReader();
            //Logger.Log("newSearch");
            search = new IndexSearcher(reader);
            DBChanged();
            
            //Logger.Log("FinishedCommit");


        }
        private List<string> GetByField(string tag, string queryFieldName,string fieldName)
        {
            
            Term term = new Term(queryFieldName, tag);
            Query query = new TermQuery(term);
            return GetByQuery(fieldName,  query);
        }

        private List<string> GetByQuery(string fieldName,  Query query)
        {
            List<string> ret = new List<string>();
            ScoreDoc[] docs = search.Search(query, CfgPerformance.TAG_MAX_RELATION).ScoreDocs;
            Document doc = null;
            foreach (ScoreDoc sdoc in docs)
            {
                doc = search.Doc(sdoc.Doc);
                Field[] fs = doc.GetFields(fieldName);
                foreach (Field f in fs)
                {
                    ret.Add(f.StringValue);

                }

            }
            return ret;
        }

        public List<string> QueryTagAlias(string tag)
        {
            return GetByField(tag,F_TAGNAME,F_TAGNAME);
        }

        

        public List<string> QueryTagChildren(string tag)
        {
            if (!TagSwitchDB.Ins.Get(tag)) return new List<string>();

            //TODO 自己不能关联自己,现在临时规避一下，正式方案需要在用户输入时保证不会出现这种情况
            List<string> result = GetByField(tag, F_TAGNAME, F_TAGCHILD);
            result.Remove(tag);
            return result;
            
        }

        public List<string> QueryTagParent(string tag)
        {
            //TODO 自己不能关联自己,现在临时规避一下，正式方案需要在用户输入时保证不会出现这种情况
            List<string> result = GetByField(tag, F_TAGCHILD, F_TAGNAME);
            result.Remove(tag);
            return result;
        }

        public int RemoveTag(string tag)//TODO:bug，没有清除该tag所有parent对其的引用
        {
            RemoveAllRelation(tag);
            List<string> alias = QueryTagAlias(tag);
            DelTag(tag);
            foreach (string n in alias)
            {
                DelTag(n);
            }
            Commit();
            return ITagDBConst.R_OK;
        }

        public int MergeAlias(string tag1,string tag2)
        {
            Document doc = new Document();
            HashSet<string> fields = new HashSet<string>();
            MergeDoc(tag1, doc, fields);
            MergeDoc(tag2, doc, fields);
            writer.DeleteDocuments(new Term(F_TAGNAME, tag1));
            writer.DeleteDocuments(new Term(F_TAGNAME, tag2));
            writer.AddDocument(doc);
            Commit();
            return ITagDBConst.R_OK;
        }

        private  void MergeDoc(string tag, Document doc, HashSet<string> fields)
        {
            Document doc1 = GetDoc(tag);
            if (doc1 != null)
            {
                foreach (Field f in doc1.GetFields())
                {
                    if (!fields.Contains(f.StringValue))
                    {
                        doc.Add(f);
                        fields.Add(f.StringValue);
                    }
                }
            }
            else
            {
                doc.Add((new Field(F_TAGNAME, tag, Field.Store.YES, Field.Index.NOT_ANALYZED)));
            }
        }

        private int DelTag(string tag)
        {
            Query query= new TermQuery(new Term(F_TAGNAME, tag));
            writer.DeleteDocuments(query);
            Commit();
            return ITagDBConst.R_OK;
        }

        public List<string> QueryAutoComplete(string searchTerm)
        {
            Query query = new RegexQuery(new Term(F_TAGNAME, ".*" + searchTerm + ".*"));
            return GetByQuery(F_TAGNAME, query);
        }

        public string AddTag(string sentence)
        {
            TagInf tagInf = TagInf.ParseTRG(sentence);
            string tag = null;
            if (tagInf != null)
            {

                if (tagInf.GetAlias().Count > 0)
                {
                    tag = tagInf.GetAlias()[0];
                    if (tagInf.GetChdren().Count == 0)
                    {
                        AddTag(tag, null);
                    }
                    foreach (string c in tagInf.GetChdren())
                    {
                        AddTag(tag, c);
                    }
                    foreach (string a in tagInf.GetAlias())
                    {
                        AddTag(tag, a);
                    }
                    //Update(tag);
                }

            }
            return tag;
        }

        public int QueryChildrenCount(string tag)
        {
            return GetByField(tag, F_TAGNAME, F_TAGCHILD).Count;
        }


        public void Dbg()
        {
            //return;
            System.IO.TextWriter w = new System.IO.StreamWriter(@".\tagdb.json");
            int max = search.MaxDoc;
            
            for (int i = 0; i < max; i++)
            {
                Document doc = search.Doc(i);
                if (doc != null)
                {
                    string mainTag = doc.Get(F_TAGNAME);
                    JTagInf tag = new JTagInf(mainTag);
                    foreach (Field f in doc.GetFields(F_TAGCHILD))
                    {
                        tag.AddChild(f.StringValue);
                    }
                    w.WriteLine(JsonConvert.SerializeObject(tag));
                    
                }
            }
            w.Close();

        }
    }
}
