using Lucene.Net.Analysis;
using Lucene.Net.Documents;
using Lucene.Net.Index;
using Lucene.Net.QueryParsers;
using Lucene.Net.Search;
using Lucene.Net.Store;
using LuceneTest.Core;
using System;
using System.Collections.Generic;
using System.Text;

namespace LuceneTest.UriMgr
{
    class LuceneUriDB : IUriDB
    {
        const string F_URI = "furi";
        const string F_KEY = "key";
        const string F_ID = "guid";
        const string F_URI_TITLE = "ftitle";
        const string F_URI_TAGS = "ftags";
        const string F_CREATE_TIME = "fctime";
        const string F_ACCESS_TIME = "fatime";

        IndexWriter writer = null;
        IndexReader reader = null;
        IndexSearcher search = null;
        QueryParser parser;
        Query query;

        
        

        Directory dir = null;
        DataChanged dbChangedHandler;
        public DataChanged UriDBChanged
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

        public LuceneUriDB()
        {
            bool create = true;
            if (Cfg.Ins.IsDbg)
            {
                dir = new RAMDirectory();
                create = true;
            }
            else
            {
                create = !System.IO.Directory.Exists(Cfg.Ins.UriDB);
                if (create)
                {
                    System.IO.Directory.CreateDirectory(Cfg.Ins.UriDB);
                }
                dir = FSDirectory.Open(Cfg.Ins.UriDB);
                
            }
            
            writer = new IndexWriter(dir,
                                    new UriAnalyser(),
                                    create,
                                    IndexWriter.MaxFieldLength.UNLIMITED);

            
            reader = writer.GetReader();
            search = new IndexSearcher(reader);

            parser = new MultiFieldQueryParser(Lucene.Net.Util.Version.LUCENE_30,
                new string[] { F_URI, F_URI_TAGS, F_URI_TITLE },
                new UriQueryAnalyser());

            DBChanged();
            Dbg();
        }
        public void test()
        {
            //TEST
            AddUri(@"c:\a.txt");
            Document doc = GetDoc(@"c:\a.txt");
            List<string> ret = Query("a");
        }
        public static string dbg(string text,Analyzer analyzer)
        {
            //Analyzer analyzer = new UriAnalyser();
            System.IO.StringReader reader = new System.IO.StringReader(text);
            TokenStream tokenStream = analyzer.TokenStream("", reader);

            StringBuilder sb = new StringBuilder();
            // 递归处理所有语汇单元  
            while (tokenStream.IncrementToken())
            {
                string s = tokenStream.ToString();
                sb.AppendLine(s);
            }
            Console.Write(sb.ToString());
            return sb.ToString();
        }
        Document GetDoc(string uri)
        {
            Term term = new Term(F_KEY, uri.ToLower()); //kummer:能用分词器吗，这儿暂时没有找到方法，只好手工将uri转换为小写
            Query query = new TermQuery(term);
            ScoreDoc[] docs = search.Search(query, 1).ScoreDocs;
            Document doc = null;
            if (docs.Length == 1)
            {
                doc = search.Doc(docs[0].Doc);
            }
            return doc;
        }
        Document GetDoc(Guid id)
        {
            Term term = new Term(F_ID, id.ToString()); //kummer:能用分词器吗，这儿暂时没有找到方法，只好手工将uri转换为小写
            Query query = new TermQuery(term);
            ScoreDoc[] docs = search.Search(query, 1).ScoreDocs;
            
            Document doc = null;
            if (docs.Length == 1)
            {
                doc = search.Doc(docs[0].Doc);
            }
            return doc;
        }
        private void DBChanged()
        {
            TipsCenter.Ins.UriDBInf = "当前文件数据库中存在 ： " + reader.MaxDoc + " 已删除：" + reader.NumDeletedDocs;
        }
        private void Commit()
        {
            writer.Flush(true, true, true);
            reader = writer.GetReader();
            search = new IndexSearcher(reader);
            dbChangedHandler?.Invoke();
            DBChanged();
        }
        private Document GetDocEx(string Uri,out bool needUpdate,out Guid id)
        {
            Document doc = null;
            id = NtfsFileID.GetID(Uri);
            doc = GetDoc(id);
            if(doc==null)
            {
                doc = GetDoc(Uri);
            }
            if(doc==null)
            {
                needUpdate = true;
            }
            else
            {
                Field fid = doc.GetField(F_ID);
                Field fkey = doc.GetField(F_KEY);
                System.Diagnostics.Debug.Assert(fkey != null);
                needUpdate = ((fid == null || fid.StringValue != id.ToString())
                    || (fkey == null || fkey.StringValue != Uri.ToLower())
                    );
            }
            return doc;
        }
        public Document AddUriDocument(string Uri)
        {
            Guid id;
            bool needUpdate;
            Document doc = GetDocEx(Uri, out needUpdate, out id);
            if (!needUpdate) return doc;
           
            //doc确实不存在，说明是一个新的文件
            if (doc == null)
            {
                doc = new Document();
                
            }
            else //doc已经存在，需要更新
            {
                writer.DeleteDocuments(new Term(F_KEY, Uri.ToLower()));
                writer.DeleteDocuments(new Term(F_ID, id.ToString()));
                doc.RemoveField(F_ID);
                doc.RemoveField(F_KEY);
                doc.RemoveField(F_URI);
                
            }
            doc.Add(new Field(F_URI, Uri, Field.Store.YES, Field.Index.ANALYZED));
            doc.Add(new Field(F_KEY, Uri.ToLower(), Field.Store.YES, Field.Index.NOT_ANALYZED));
            doc.Add(new Field(F_ID, id.ToString(), Field.Store.YES, Field.Index.NOT_ANALYZED));
            writer.AddDocument(doc);
            return doc;
        }

        public int AddUri(string Uri, List<string> tags)
        {
            lock (this)
            {
                Document doc = AddUriDocument(Uri, tags);
                Commit(Uri, doc);
                return 0;
            }
        }
        public int AddUri(string Uri, List<string> tags, string Title)
        {
            lock (this)
            {
                Document doc = AddUriDocument(Uri, tags, Title);
                Commit(Uri, doc);
                return 0;
            }
        }
        public int AddUri(string Uri)
        {
            lock (this)
            {
                Document doc = AddUriDocument(Uri);
                Commit(Uri, doc);
                return 0;
            }
        }
        private void Commit(string Uri, Document doc)
        {
            if (doc != null)
            {
                writer.UpdateDocument(new Term(F_KEY, Uri.ToLower()), doc); //没有指定分析器，导致大小写有bug，相同的Uri会存在两个
                //writer.UpdateDocument(new Term(F_URI, Uri), doc,new UriQueryAnalyser());

                //writer.DeleteDocuments(new Term(F_URI, Uri));
                Commit();
            }
        }

        private Document AddUriDocument(string Uri, List<string> tags)
        {
            Document doc = AddUriDocument(Uri);
            if (doc != null)
            {
                Field[] fs = doc.GetFields(F_URI_TAGS);
                HashSet<string> htag = new HashSet<string>();
                foreach (Field f in fs)
                {
                    htag.Add(f.StringValue);
                }
                foreach (string tag in tags)
                {
                    if (!htag.Contains(tag))
                    {
                        htag.Add(tag);
                        doc.Add(new Field(F_URI_TAGS, tag, Field.Store.YES, Field.Index.ANALYZED));
                    }
                }
            }
            
            return doc;
        }

        
        private Document AddUriDocument(string Uri, List<string> tags, string Title)
        {
            Document doc = AddUriDocument(Uri, tags);
            Field f = doc.GetField(F_URI_TITLE);
            if(f!=null)
            {
                doc.RemoveFields(F_URI_TITLE);
            }
            doc.Add(new Field(F_URI_TITLE, Title, Field.Store.YES, Field.Index.ANALYZED));
           
            return doc;

        }
        public int DelUri(string Uri, bool Delete)
        {
            writer.DeleteDocuments(new Term(F_KEY, Uri.ToLower()));
            Commit();
            if(Delete)
            {
                //TODO  删除文件
            }
            return 0;
        }

        public void Dispose()
        {
            search.Dispose();
            reader.Dispose();
            writer.Dispose();
        }

        public List<string> Query(string querystr)
        {
            int delete = 0;
            List<string> ret = new List<string>();
            try
            {
                query = parser.Parse(querystr);
                ScoreDoc[] docs = search.Search(query, Cfg.Ins.TAG_MAX_RELATION).ScoreDocs;
                for (int i = 0; i < docs.Length; i++)
                {
                    Document doc = search.Doc(docs[i].Doc);
                    if (!reader.IsDeleted(docs[i].Doc))
                    {
                        ret.Add(doc.GetField(F_URI).StringValue);
                    }
                    else
                    {
                        delete++;
                    }
                }
                TipsCenter.Ins.MainInf = "当前查询： "+querystr + " has found: " + docs.Length +" files,has Deleted "+delete;
            }catch(Exception e)
            {

            }
            return ret;
        }

        

        public int DelUri(string Uri, List<string> tags)
        {
            Document doc = GetDoc(Uri);
            Field[] fs = doc.GetFields(F_URI_TAGS);
            doc.RemoveFields(F_URI_TAGS);
            foreach(Field f in fs)
            {
                if (!tags.Contains(f.StringValue))
                {
                    doc.Add(f);
                }
            }

            Commit(Uri,doc);

            return 0;
        }

        public int UpdateUri(string Uri, string Title)
        {
            Document doc = GetDoc(Uri);
            doc.RemoveFields(F_URI_TITLE);
            doc.Add(new Field(F_URI_TITLE, Title, Field.Store.YES, Field.Index.ANALYZED));
            Commit(Uri, doc);
            return 0;
        }

        public string GetTitle(string Uri)
        {
            string ret = "";
            Document doc = GetDoc(Uri);
            if(doc!=null)
            {
                ret = doc.GetField(F_URI_TITLE).StringValue;
            }
            return ret;
        }

        public List<string> GetTags(string Uri)
        {
            List<string> ret = new List<string>();
            if (Uri == null) return ret;

            Document doc = GetDoc(Uri);
            if (doc != null)
            {
                Field[] fields = doc.GetFields(F_URI_TAGS);

                foreach(Field f in fields)
                {
                    ret.Add(f.StringValue);
                }
            }
            return ret;
        }
        //自动检查某个路径是否有文件发生变化,
        //root可以是一个文件，也可以是一个目录
        //如果是文件,deepth为0（检查一下）
        //如果是目录，deepth表示需要搜索的深度。
        public void AutoUpdate(string root,int deepth)
        {
            throw new NotImplementedException();
        }

        public void Notify()
        {
            dbChangedHandler?.Invoke();
        }
        public void Dbg()
        {
            return;
            System.IO.TextWriter w = new System.IO.StreamWriter(@"c:\dbg.csv");
            int max = search.MaxDoc;
            w.WriteLine(@"IDX,F_ID,F_KEY,F_URI,DEL,TAGS");
            for (int i = 0; i < max; i++)
            {
                Document doc = search.Doc(i);
                if(doc!=null )
                {
                    
                    w.Write(string.Format("{0},{1},{2},{3},{4}",
                                            i,  
                                            doc.Get(F_ID), 
                                            doc.Get(F_KEY), 
                                            doc.Get(F_URI),
                                            reader.IsDeleted(i)?"DEL":"OK"
                                            )
                                );
                    if(doc.Get(F_KEY)?.IndexOf("python")>0)
                    {
                        foreach (Field f in doc.GetFields(F_URI_TAGS))
                        {
                            w.Write("," + f.StringValue);
                        }
                    }
                    foreach (Field f in doc.GetFields(F_URI_TAGS))
                    {
                        w.Write(","+f.StringValue);
                    }
                }
                w.WriteLine();
            }
        }
    }
}
