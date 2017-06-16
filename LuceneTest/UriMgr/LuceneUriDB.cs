using Lucene.Net.Analysis;
using Lucene.Net.Documents;
using Lucene.Net.Index;
using Lucene.Net.QueryParsers;
using Lucene.Net.Search;
using Lucene.Net.Store;
using LuceneTest.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LuceneTest.UriMgr
{
    class LuceneUriDB : IUriDB
    {
        const string F_URI = "furi";
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
        public LuceneUriDB()
        {
            dir = new RAMDirectory();
            dir = FSDirectory.Open(Cfg.Ins.UriDB);
            bool create = !System.IO.Directory.Exists(Cfg.Ins.UriDB);
            if (create)
            {
                System.IO.Directory.CreateDirectory(Cfg.Ins.UriDB);
            }
            create = true;
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
        Document GetDoc(string uri)
        {
            Term term = new Term(F_URI, uri);
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
        }
        public Document AddUriDocument(string Uri)
        {
            Document doc = GetDoc(Uri);
            if(doc==null)
            {
                doc = new Document();
                doc.Add(new Field(F_URI, Uri,Field.Store.YES, Field.Index.ANALYZED));
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
                writer.UpdateDocument(new Term(F_URI, Uri), doc);
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
            writer.DeleteDocuments(new Term(F_URI, Uri));
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
            query = parser.Parse(querystr);
            ScoreDoc[] docs = search.Search(query, Cfg.Ins.TAG_MAX_RELATION).ScoreDocs;
            for (int i = 0; i < docs.Length; i++)
            {
                Document doc = search.Doc(docs[i].Doc);
                ret.Add(doc.GetField(F_URI).StringValue);
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
    }
}
