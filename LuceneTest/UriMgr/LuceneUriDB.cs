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
        private void Commit()
        {
            writer.Flush(true, true, true);
            reader = writer.GetReader();
            search = new IndexSearcher(reader);
            dbChangedHandler?.Invoke();
        }
        public Document AddUriDocument(string Uri)
        {
            Document doc = GetDoc(Uri);
            if(doc==null)
            {
                doc = new Document();
                doc.Add(new Field(F_URI, Uri,Field.Store.YES, Field.Index.ANALYZED));
                doc.Add(new Field(F_KEY, Uri.ToLower(), Field.Store.YES, Field.Index.NOT_ANALYZED));
                writer.AddDocument(doc);
            }
            
            return doc;
        }

        public int AddUri(string Uri, List<string> tags)
        {
            Document doc = AddUriDocument(Uri, tags);
            Commit(Uri, doc);
            return 0;
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

        public int AddUri(string Uri, List<string> tags, string Title)
        {
            Document doc = AddUriDocument(Uri, tags, Title);
            Commit(Uri, doc);
            return 0;
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

            List<string> ret = new List<string>();
            try
            {
                query = parser.Parse(querystr);
                ScoreDoc[] docs = search.Search(query, Cfg.Ins.TAG_MAX_RELATION).ScoreDocs;
                for (int i = 0; i < docs.Length; i++)
                {
                    Document doc = search.Doc(docs[i].Doc);
                    ret.Add(doc.GetField(F_URI).StringValue);
                }
            }catch(Exception e)
            {

            }
            return ret;
        }

        public int AddUri(string Uri)
        {
            Document doc = AddUriDocument(Uri);
            Commit(Uri, doc);
            return 0;
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
    }
}
