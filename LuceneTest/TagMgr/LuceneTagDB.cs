using AnyTagNet.BL;
using Contrib.Regex;
using Lucene.Net.Analysis;
using Lucene.Net.Documents;
using Lucene.Net.Index;
using Lucene.Net.Search;
using Lucene.Net.Store;
using LuceneTest.Core;
using System;
using System.Collections.Generic;

namespace LuceneTest.TagMgr
{
    class LuceneTagDB : ITagDB, IDisposable
    {
        const int R_OK = 0;
        const string F_TAGNAME = "tname";
        const string F_TAGCHILD = "tchildren";

        IndexWriter writer = null;
        IndexReader reader = null;
        IndexSearcher search = null;
        Directory dir = null;
        public LuceneTagDB()
        {
            bool create = true;
            if (Cfg.Ins.IsDbg)
            {
                dir = new RAMDirectory();
                create = true;
            }
            else
            {
                create = !System.IO.Directory.Exists(Cfg.Ins.TagDB);
                if (create)
                {
                    System.IO.Directory.CreateDirectory(Cfg.Ins.TagDB);
                }
                dir = FSDirectory.Open(Cfg.Ins.TagDB);
                
            }
            
            
            writer = new IndexWriter(dir,
                                    new KeywordAnalyzer(),
                                    create, 
                                    IndexWriter.MaxFieldLength.UNLIMITED);


            reader = writer.GetReader();
            search = new IndexSearcher(reader);
            
        }

        public void Dispose()
        {
            search.Dispose();
            reader.Dispose();
            writer.Dispose();
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

        public int AddTag(string parent, string child)
        {
            int ret = R_OK;
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
                        return R_OK;
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
            ScoreDoc[] docs = search.Search(query, Cfg.Ins.TAG_MAX_RELATION).ScoreDocs;
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
            return GetByField(tag, F_TAGNAME, F_TAGCHILD);
        }

        public List<string> QueryTagParent(string tag)
        {
            return GetByField(tag, F_TAGCHILD, F_TAGNAME);
        }

        public int RemoveTag(string tag)
        {
            List<string> alias = QueryTagAlias(tag);
            foreach(string n in alias)
            {
                DelTag(n);
            }
            Commit();
            return R_OK;
        }

        public int MergeAliasTag(string tag1,string tag2)
        {
            Document doc = new Document();
            HashSet<string> fields = new HashSet<string>();
            MergeDoc(tag1, doc, fields);
            MergeDoc(tag2, doc, fields);
            writer.DeleteDocuments(new Term(F_TAGNAME, tag1));
            writer.DeleteDocuments(new Term(F_TAGNAME, tag2));
            writer.AddDocument(doc);
            Commit();
            return R_OK;
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
            return R_OK;
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
    }
}
