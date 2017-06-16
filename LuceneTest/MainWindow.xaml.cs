using Lucene.Net.Analysis;
using Lucene.Net.Analysis.NGram;
using Lucene.Net.Analysis.Standard;
using Lucene.Net.Documents;
using Lucene.Net.Index;
using Lucene.Net.QueryParsers;
using Lucene.Net.Search;
using Lucene.Net.Store;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.IO;
using Contrib.Regex;
using LuceneTest.TagMgr;
using LuceneTest.TagLayout;
using LuceneTest.AutoComplete;

namespace LuceneTest
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        public class LuceneConstants
        {
           public  const string TYPE="type";
           public  const string FILE_PATH ="path";
           public  const string FILE_NAME ="name";
           public  const int MAX_SEARCH = 10;
        }

        
        public MainWindow()
        {
            InitializeComponent();
            MyAnalyzer.dbg(".txt");
            test();
            //test2();
        }
        string PATH = @"D:\02TagSpaces\index";
        string INPUT_PATH = @"D:\02TagSpaces\test";
        public void SearchDir(string dir,IndexWriter w)
        {
            Document doc = new Document();
            doc.Add(new Field(LuceneConstants.FILE_PATH, dir, Field.Store.YES, Field.Index.NOT_ANALYZED));
            doc.Add(new Field(LuceneConstants.TYPE, "DIR", Field.Store.YES, Field.Index.NOT_ANALYZED));
            doc.Add(new Field(LuceneConstants.FILE_NAME, new System.IO.DirectoryInfo(dir).Name, Field.Store.YES, Field.Index.NOT_ANALYZED_NO_NORMS));
            w.AddDocument(doc);

            string[] files = System.IO.Directory.GetFiles(dir);
            foreach(string f in files)
            {
                Document docf = new Document();
                docf.Add(new Field(LuceneConstants.FILE_PATH, f, Field.Store.YES, Field.Index.NOT_ANALYZED));
                docf.Add(new Field(LuceneConstants.TYPE, "FILE", Field.Store.YES, Field.Index.NOT_ANALYZED));
                docf.Add(new Field(LuceneConstants.FILE_NAME, new System.IO.FileInfo(f).Name, Field.Store.YES, Field.Index.ANALYZED));
                w.AddDocument(docf); 
            }
            string[] dirs = System.IO.Directory.GetDirectories(dir);
            foreach(string subd in dirs)
            {
                SearchDir(subd, w);
            }
        }

        public class MyCharTokenizer : CharTokenizer
        {
            public MyCharTokenizer(TextReader input):base(input)
            {
            }
            
            protected override bool IsTokenChar(char c)
            {
                return true;
            }
        }

        class MyAnalyzer : Analyzer
        {
            public override TokenStream TokenStream(string fieldName, TextReader reader)
            {

                //NGramTokenizer source = new NGramTokenizer(reader,1,3);
                //Tokenizer source = new StandardTokenizer( Lucene.Net.Util.Version.LUCENE_30,reader);
                Tokenizer source = new KeywordTokenizer(reader);
                //TokenFilter filter = new LowerCaseFilter(source);
                //filter = new NGramTokenFilter(filter, 1, 50);

                return source;
            }
            public /*override*/  TokenStream TokenStream1(string fieldName, TextReader reader)
            {

                //NGramTokenizer source = new NGramTokenizer(reader,1,3);
                //Tokenizer source = new StandardTokenizer( Lucene.Net.Util.Version.LUCENE_30,reader);
                Tokenizer source = new MyCharTokenizer(reader);
                TokenFilter filter = new LowerCaseFilter(source);
                filter = new NGramTokenFilter(filter, 1, 50);

                return filter;
            }
            public static string dbg(string text)
            {
                Analyzer analyzer = new MyAnalyzer();
                StringReader reader = new StringReader(text);
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
        }

        //       Analyzer analyzer = new Analyzer() {
        //@Override
        // protected TokenStreamComponents createComponents(String fieldName, Reader reader)
        //       {
        //           KeywordTokenizer source = new KeywordTokenizer(reader);
        //           LowercaseFilter filter = new LowercaseFilter(source);
        //           filter = new EdgeNGramTokenFilter(filter, EdgeNGramTokenFilter.Side.BACK, 2, 50);
        //           return new TokenStreamComponents(source, filter);
        //       }
        //   };

        ITagDB tagDB = TagDBFactory.CreateTagDB();
        ITagLayout tagLayout = TagLayoutFactory.CreateLayout();
        public void test()
        {
            autoTextBox.Search = new SearchDemo();
            for (int i = 0; i < 2; i++)
            {
                tagsBar.AddTag("Hello"+i); //tagsBar.Items.Add(new Separator());
                tagsBar.AddTag("World"+i); //tagsBar.Items.Add(new Separator());
            }

            tagDB.AddTag("parent1", "child1");
            tagDB.AddTag("parent1", "child2");
            tagDB.AddTag("parent1", "child3");
            tagDB.AddTag("parent1", "child4");

            tagDB.MergeAliasTag("parent1", "parent11");

            tagDB.AddTag("parent2", "child1");
            tagDB.AddTag("parent2", "child2");
            tagDB.AddTag("parent2", "child3");
            tagDB.AddTag("parent2", "child4");
            tagLayout.Layout(tagDB, "parent11");
            canvas.Children.Clear();

            IEnumerable<UIElement> lines = tagLayout.Lines;
            IEnumerable<UIElement> allTxt = tagLayout.TagArea;
            canvas.Width = tagLayout.Size.Width;
            canvas.Height = tagLayout.Size.Height;
            CalcCanvasHeight();
            foreach (Line l in lines)
            {
                canvas.Children.Add(l);
            }
            foreach (TextBlock t in allTxt)
            {
                canvas.Children.Add(t);
            }
            //IndexWriter writer = new IndexWriter(FSDirectory.Open(PATH),
            //    //new StandardAnalyzer(Lucene.Net.Util.Version.LUCENE_30), 
            //    //new SimpleAnalyzer(),
            //    new MyAnalyzer(),
            //    true,IndexWriter.MaxFieldLength.UNLIMITED);

            //SearchDir(INPUT_PATH, writer);

            //writer.Optimize();
            //writer.Dispose();


        }
        Lucene.Net.Store.Directory dir;
        IndexSearcher idx;
        QueryParser parser;
        Query query;
        public void test2()
        {
            
            dir = FSDirectory.Open(PATH);
            idx= new IndexSearcher(dir);
            parser = new QueryParser( Lucene.Net.Util.Version.LUCENE_30, LuceneConstants.FILE_NAME, 
                //new StandardAnalyzer(Lucene.Net.Util.Version.LUCENE_30)
                new KeywordAnalyzer()
                //new MyAnalyzer()
                );
            
        }

        private void txt_TextChanged(object sender, TextChangedEventArgs e)
        {
            //if (txt.Text.Trim() == "") return;
            //txt2.Clear();
            
            ////query = parser.Parse(txt.Text+"*");
            //query = new RegexQuery(new Term(LuceneConstants.FILE_NAME, txt.Text + ".*"));
            //System.Diagnostics.Debug.WriteLine(query.ToString());
            //ScoreDoc[] docs = idx.Search(query, 100).ScoreDocs;
            //for (int i = 0; i < docs.Length; i++)
            //{
            //    Document doc = idx.Doc(docs[i].Doc);
            //    txt2.AppendText(doc.Get(LuceneConstants.TYPE) + " " + doc.Get(LuceneConstants.FILE_NAME) + " " + doc.Get(LuceneConstants.FILE_PATH)+"\r\n");
            //    //MessageBox.Show(doc.Get("title"));
            //}
            //idx.Dispose();
        }

        private void scrollViewer_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {

        }

        private void canvas_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {

        }

        private void canvas_Click(object sender, RoutedEventArgs e)
        {

        }

        private void canvas_MouseRightButtonDown_1(object sender, MouseButtonEventArgs e)
        {

        }

        private void canvas_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {

        }

        private void autoTextBox_KeyDown(object sender, KeyEventArgs e)
        {

        }

        private void lst_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {

        }
        private void CalcCanvasHeight()
        {
            canvas.Height = Math.Max(canvas.Height, rGrid.RenderSize.Height-10);
        }
        private void Window_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            CalcCanvasHeight();
        }
    }
}
