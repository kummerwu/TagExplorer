using Lucene.Net.Analysis;
using Lucene.Net.Documents;
using Lucene.Net.Index;
using Lucene.Net.QueryParsers;
using Lucene.Net.Search;
using Lucene.Net.Store;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using TagExplorer.Utils;

namespace TagExplorer.UriMgr
{
    
    class LuceneUriDB : IUriDB
    {
        
        #region 公有方法和接口实现
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
            Logger.I("Create LuceneUriDB Instance!");
            bool create = true;
            if (UTestCfg.Ins.IsUTest)
            {
                dir = new RAMDirectory();
                create = true;
            }
            else
            {
                Logger.I("DBPath = " + CfgPath.UriDBPath);
                create = !System.IO.Directory.Exists(CfgPath.UriDBPath);
                if (create)
                {
                    Logger.I("First Create !DBPath = " + CfgPath.UriDBPath);
                    System.IO.Directory.CreateDirectory(CfgPath.UriDBPath);
                }
                dir = FSDirectory.Open(CfgPath.UriDBPath);

            }

            writer = new IndexWriter(dir,
                                    new UriAnalyser(),
                                    create,
                                    IndexWriter.MaxFieldLength.UNLIMITED);


            reader = writer.GetReader();
            search = new IndexSearcher(reader);

            //搜索查询：从哪些字段中进行搜索查询
            parser = new MultiFieldQueryParser(Lucene.Net.Util.Version.LUCENE_30,
                new string[] { URIItem.F_URI, URIItem.F_URI_TAGS, URIItem.F_URI_TITLE },
                new UriQueryAnalyser());

            DBChanged();
            //Dbg();
        }
        public int AddUris(IEnumerable< string> Uris, List<string> tags)
        {
            lock (this)
            {
                foreach (string Uri in Uris)
                {
                    Document doc = AddUriDocument(Uri, tags);
                    if (doc != null)
                    {
                        writer.UpdateDocument(new Term(URIItem.F_KEY, Uri.ToLower()), doc); //没有指定分析器，导致大小写有bug，相同的Uri会存在两个
                    }
                }
                //Commit(Uri, doc);
                Commit();
                return 0;
            }
        }
        public int AddUriWithTitle(string Uri, List<string> tags, string Title)
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
        public int DelUris(IEnumerable<string> Uris, bool Delete)
        {
            foreach (string Uri in Uris)
            {
                writer.DeleteDocuments(new Term(URIItem.F_KEY, Uri.ToLower()));
            }
            Commit();
            if (Delete)
            {
                MoveToRecycle(Uris);
            }
            return 0;
        }

        private static void MoveToRecycle(IEnumerable<string> Uris)
        {
            List<string> srcs = new List<string>();
            srcs.AddRange(Uris);
            List<string> dstList = new List<string>();
            List<string> srcList = new List<string>();
            foreach (string s in srcs)
            {
                if (PathHelper.IsValidFS(s))
                {
                    string dst = CfgPath.GetRecycleByPath(s);
                    dstList.Add(dst);
                    srcList.Add(s);
                }
            }
            FileShell.SHMoveFiles(srcList.ToArray(), dstList.ToArray());
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
                ScoreDoc[] docs = search.Search(query, StaticCfg.Ins.TAG_MAX_RELATION).ScoreDocs;
                for (int i = 0; i < docs.Length; i++)
                {
                    Document doc = search.Doc(docs[i].Doc);
                    if (!reader.IsDeleted(docs[i].Doc))
                    {
                        ret.Add(doc.GetField(URIItem.F_URI).StringValue);
                    }
                    else
                    {
                        delete++;
                    }
                }
                TipsCenter.Ins.MainInf = "当前查询： " + querystr + " has found: " + docs.Length + " files,has Deleted " + delete;
            }
            catch (Exception e)
            {
                Logger.E(e);
            }
            return ret;
        }
        public int DelTags(string Uri, List<string> tags)
        {
            Document doc = GetDoc(Uri);
            Field[] fs = doc.GetFields(URIItem.F_URI_TAGS);
            doc.RemoveFields(URIItem.F_URI_TAGS);
            foreach (Field f in fs)
            {
                if (!tags.Contains(f.StringValue))
                {
                    doc.Add(f);
                }
            }

            Commit(Uri, doc);

            return 0;
        }
        public int UpdateTitle(string Uri, string Title)
        {
            Document doc = GetDoc(Uri);
            doc.RemoveFields(URIItem.F_URI_TITLE);
            doc.Add(new Field(URIItem.F_URI_TITLE, Title, Field.Store.YES, Field.Index.ANALYZED));
            Commit(Uri, doc);
            return 0;
        }
        private URIItem DocToURIItem(Document doc)
        {
            URIItem item = new URIItem();
            item.Title = doc.GetField(URIItem.F_URI_TITLE)?.StringValue;
            item.Key = doc.Get(URIItem.F_KEY);
            item.Uri = doc.Get(URIItem.F_URI);
            item.Tags = GetDocTags(doc);
            string createT = doc.Get(URIItem.F_CREATE_TIME);
            string accessT = doc.Get(URIItem.F_ACCESS_TIME);
            if (string.IsNullOrEmpty(createT))
            {
                item.CreateTime = DateTime.Now;
            }
            else
            {
                item.CreateTime = DateTime.ParseExact(createT, TIME_FMT, CultureInfo.InvariantCulture);
            }
            if (string.IsNullOrEmpty(accessT))
            {
                item.AccessTime = DateTime.Now;
            }
            else
            {
                item.AccessTime = DateTime.ParseExact(accessT, TIME_FMT, CultureInfo.InvariantCulture);
            }
            if (!string.IsNullOrEmpty(item.Key) || !string.IsNullOrEmpty(item.Uri))
            {
                if (string.IsNullOrEmpty(item.Key))
                {
                    item.Key = item.Uri.ToLower();
                }
                else if (string.IsNullOrEmpty(item.Uri))
                {
                    item.Uri = item.Key;
                }
                //all.Add(item);
            }
            return item;
        }
        private void AddUpdate(URIItem item)
        {
            if(string.IsNullOrEmpty(item.Title))
            {
                AddUris(new string[] {item.Uri }, item.Tags);
            }
            else
            {
                AddUriWithTitle(item.Uri, item.Tags, item.Title);
            }
        }
        public URIItem GetInf(string Uri)
        {
            URIItem ret = null;
            Document doc = GetDoc(Uri);
            if (doc != null)
            {
                ret = DocToURIItem(doc);
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
                Field[] fields = doc.GetFields(URIItem.F_URI_TAGS);

                foreach (Field f in fields)
                {
                    ret.Add(f.StringValue);
                }
            }
            return ret;
        }
        public void Notify()
        {
            dbChangedHandler?.Invoke();
        }
        #endregion


        #region 私有方法
        

        private const string TIME_FMT = "yyyy-MM-dd HH:mm:ss";
        IndexWriter writer = null;
        IndexReader reader = null;
        IndexSearcher search = null;
        QueryParser parser;
        Query query;
        Directory dir = null;
        DataChanged dbChangedHandler;
        private void test()
        {
            //TEST
            AddUri(@"c:\a.txt");
            Document doc = GetDoc(@"c:\a.txt");
            List<string> ret = Query("a");
        }
        private static string dbg(string text,Analyzer analyzer)
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
        private Document GetDoc(string uri)
        {
            Term term = new Term(URIItem.F_KEY, uri.ToLower()); //kummer:能用分词器吗，这儿暂时没有找到方法，只好手工将uri转换为小写
            Query query = new TermQuery(term);
            ScoreDoc[] docs = search.Search(query, 1).ScoreDocs;
            Document doc = null;
            if (docs.Length == 1)
            {
                doc = search.Doc(docs[0].Doc);
            }
            return doc;
        }
        //private Document GetDoc(Guid id)
        //{
        //    Term term = new Term(F_ID, id.ToString()); //kummer:能用分词器吗，这儿暂时没有找到方法，只好手工将uri转换为小写
        //    Query query = new TermQuery(term);
        //    ScoreDoc[] docs = search.Search(query, 1).ScoreDocs;
            
        //    Document doc = null;
        //    if (docs.Length == 1)
        //    {
        //        doc = search.Doc(docs[0].Doc);
        //    }
        //    return doc;
        //}
        private void DBChanged()
        {
            TipsCenter.Ins.UriDBInf = "当前文件数据库中存在 ： " + reader.MaxDoc + " 已删除：" + reader.NumDeletedDocs;
        }
        private void Commit()
        {
            if (!SuspendCommit)
            {
                writer.Commit();
                reader = writer.GetReader();
                search = new IndexSearcher(reader);
                dbChangedHandler?.Invoke();
                DBChanged();
            }
        }
        private Document GetDocEx(string Uri,out bool needUpdate)
        {
            Document doc = null;
            //id = NtfsFileID.GetID(Uri);
            doc = GetDoc(Uri);
            
            if(doc==null)
            {
                needUpdate = true;
            }
            else
            {
                //Field fid = doc.GetField(F_ID);
                Field fkey = doc.GetField(URIItem.F_KEY);
                System.Diagnostics.Debug.Assert(fkey != null);
                needUpdate = (fkey == null || fkey.StringValue != Uri.ToLower());
            }
            return doc;
        }
        private Document AddUriDocument(string Uri)
        {
            //Guid id;
            bool needUpdate;
            Document doc = GetDocEx(Uri, out needUpdate);
            if (!needUpdate) return doc;
           
            //doc确实不存在，说明是一个新的文件
            if (doc == null)
            {
                doc = new Document();
                doc.Add(new Field(URIItem.F_CREATE_TIME, DateTime.Now.ToString(TIME_FMT, CultureInfo.InvariantCulture), Field.Store.YES, Field.Index.NOT_ANALYZED));
                doc.Add(new Field(URIItem.F_ACCESS_TIME, DateTime.Now.ToString(TIME_FMT, CultureInfo.InvariantCulture), Field.Store.YES, Field.Index.NOT_ANALYZED));
            }
            else //doc已经存在，需要更新
            {
                writer.DeleteDocuments(new Term(URIItem.F_KEY, Uri.ToLower()));
                //writer.DeleteDocuments(new Term(F_ID, id.ToString()));
                //doc.RemoveField(F_ID);
                doc.RemoveField(URIItem.F_KEY);
                doc.RemoveField(URIItem.F_URI);
                
            }
            doc.Add(new Field(URIItem.F_URI, Uri, Field.Store.YES, Field.Index.ANALYZED));
            doc.Add(new Field(URIItem.F_KEY, Uri.ToLower(), Field.Store.YES, Field.Index.NOT_ANALYZED));
            //doc.Add(new Field(F_ID, id.ToString(), Field.Store.YES, Field.Index.NOT_ANALYZED));
            writer.AddDocument(doc);
            return doc;
        }
        private void Commit(string Uri, Document doc)
        {
            if (doc != null)
            {
                writer.UpdateDocument(new Term(URIItem.F_KEY, Uri.ToLower()), doc); //没有指定分析器，导致大小写有bug，相同的Uri会存在两个
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
                Field[] fs = doc.GetFields(URIItem.F_URI_TAGS);
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
                        doc.Add(new Field(URIItem.F_URI_TAGS, tag, Field.Store.YES, Field.Index.ANALYZED));
                    }
                }
            }
            
            return doc;
        }
        private Document AddUriDocument(string Uri, List<string> tags, string Title)
        {
            Document doc = AddUriDocument(Uri, tags);
            Field f = doc.GetField(URIItem.F_URI_TITLE);
            if(f!=null)
            {
                doc.RemoveFields(URIItem.F_URI_TITLE);
            }
            doc.Add(new Field(URIItem.F_URI_TITLE, Title, Field.Store.YES, Field.Index.ANALYZED));
           
            return doc;

        }
        public void Dbg()
        {
            //return;
            System.IO.TextWriter w = new System.IO.StreamWriter(@".\dbg.csv");
            int max = search.MaxDoc;
            w.WriteLine(@"IDX,F_ID,F_KEY,F_URI,DEL,TAGS");
            for (int i = 0; i < max; i++)
            {
                Document doc = search.Doc(i);
                if(doc!=null )
                {
                    
                    w.Write(string.Format("{0},{1},{2},{3}",
                                            i,  
                                            doc.Get(URIItem.F_KEY), 
                                            doc.Get(URIItem.F_URI),
                                            reader.IsDeleted(i)?"DEL":"OK"
                                            )
                                );
                    
                    foreach (Field f in doc.GetFields(URIItem.F_URI_TAGS))
                    {
                        w.Write(","+f.StringValue);
                    }

                    string path = doc.Get(URIItem.F_URI);
                    //if(!System.IO.File.Exists(path) && !System.IO.Directory.Exists(path))
                    //{
                    //    reader.DeleteDocument(i);
                        
                    //}
                }
                w.WriteLine();

            }
            w.Close();
            
        }

        public int MoveUris(string[] SrcUri, string[] DstUri, string NewTag)
        {
            if (SrcUri!=null && DstUri!=null &&  SrcUri.Length != DstUri.Length) return 0;

            for (int i = 0; i < SrcUri.Length; i++)
            {
                Document doc = GetDoc(SrcUri[i]);
                if (doc == null) continue;

                if (DstUri != null)
                {
                    doc.RemoveFields(URIItem.F_KEY);
                    doc.RemoveFields(URIItem.F_URI);
                    doc.Add(new Field(URIItem.F_KEY, DstUri[i].ToLower(), Field.Store.YES, Field.Index.NOT_ANALYZED));
                    doc.Add(new Field(URIItem.F_URI, DstUri[i], Field.Store.YES, Field.Index.ANALYZED));
                }
                if(NewTag!=null)
                {
                    doc.RemoveFields(URIItem.F_URI_TAGS);
                    doc.Add(new Field(URIItem.F_URI_TAGS, NewTag, Field.Store.YES, Field.Index.ANALYZED));

                }
                writer.UpdateDocument(new Term(URIItem.F_KEY, SrcUri[i].ToLower()), doc); //没有指定分析器，导致大小写有bug，相同的Uri会存在两个
            }
            Commit();
            return 0;
        }

        
        private List<string> GetDocTags(Document doc)
        {
            List<string> tags = new List<string>();
            foreach (Field f in doc.GetFields(URIItem.F_URI_TAGS))
            {
                tags.Add(f.StringValue);
            }
            return tags;
        }
        private bool SuspendCommit = false;
        
        public int Import(string importFile)
        {
            if (!System.IO.File.Exists(importFile)) return 0;


            int newCnt = 0, uptCnt = 0;
            SuspendCommit = true;
            string[] lns = System.IO.File.ReadAllLines(importFile);
            foreach (string ln in lns)
            {
                URIItem importURI = JsonConvert.DeserializeObject<URIItem>(ln);
                if (importURI != null)
                {
                    URIItem myURI = GetInf(importURI.Key);
                    if (myURI == null)
                    {
                        AddUpdate(importURI);
                        newCnt++;
                    }
                    else if (!importURI.IsSame(myURI))
                    {
                        AddUpdate(URIItem.MergeTag(importURI, myURI));
                        uptCnt++;
                    }
                    else
                    {
                        //两边完全相同，不用处理
                    }
                }

            }
            SuspendCommit = false;
            Commit();
            DBChanged();
            return 0;
        }
        public int Export(string exportFile)
        {
            List<URIItem> all = new List<URIItem>();
            using (System.IO.TextWriter w = new System.IO.StreamWriter(exportFile))
            {
                int max = search.MaxDoc;
                //w.WriteLine(@"IDX,F_ID,F_KEY,F_URI,DEL,TAGS");
                for (int i = 0; i < max; i++)
                {
                    if (reader.IsDeleted(i)) continue;
                    Document doc = search.Doc(i);
                    if (doc != null)
                    {

                        URIItem item = DocToURIItem(doc);
                        if(item!=null && !string.IsNullOrEmpty(item.Key) && !string.IsNullOrEmpty(item.Uri))
                        {
                            all.Add(item);
                        }
                    }
                }

                all.Sort((x, y) => x.Key.CompareTo(y.Key));
                foreach (URIItem item in all)
                {
                    w.WriteLine(JsonConvert.SerializeObject(item));
                }

            }
            return 0;
        }
        #endregion
    }
}
