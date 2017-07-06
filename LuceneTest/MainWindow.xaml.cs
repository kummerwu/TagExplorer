using Lucene.Net.Analysis;
using Lucene.Net.Analysis.NGram;
using Lucene.Net.Documents;
using Lucene.Net.Index;
using Lucene.Net.QueryParsers;
using Lucene.Net.Search;
using Lucene.Net.Store;
using System;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.IO;
using LuceneTest.TagMgr;
using LuceneTest.UriMgr;
using LuceneTest.Core;

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

        public static RoutedCommand MyCommand = new RoutedCommand();
        

        
        public MainWindow()
        {
            InitializeComponent();
            tagCanvas.SelectedTagChanged += selectedTagChanged;
            autoTextBox.textBox.TextChanged += TextBox_TextChanged;
            uriDB.UriDBChanged += UpdateUriList;
            autoTextBox.SearchDataProvider = tagDB;
            Update("我的大脑");
            Logger.Log(@"
////////////////////////////////////////////////////////////////////
//
//init main window
//
///////////////////////////////////////////////////////////////////
");
        }
        public void UpdateUriList()
        {
            uriList.UpdateResult(autoTextBox.Text, uriDB,tagDB);
        }
        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            UpdateUriList();
        }

        public void selectedTagChanged(string tag)
        {
            autoTextBox.Text = tag;
            //修改text后，会自动触发 TextBox_TextChanged
            //进一步触发             UpdateUriList
        }
        //public void SearchDir(string dir,IndexWriter w)
        //{
        //    Document doc = new Document();
        //    doc.Add(new Field(LuceneConstants.FILE_PATH, dir, Field.Store.YES, Field.Index.NOT_ANALYZED));
        //    doc.Add(new Field(LuceneConstants.TYPE, "DIR", Field.Store.YES, Field.Index.NOT_ANALYZED));
        //    doc.Add(new Field(LuceneConstants.FILE_NAME, new System.IO.DirectoryInfo(dir).Name, Field.Store.YES, Field.Index.NOT_ANALYZED_NO_NORMS));
        //    w.AddDocument(doc);

        //    string[] files = System.IO.Directory.GetFiles(dir);
        //    foreach(string f in files)
        //    {
        //        Document docf = new Document();
        //        docf.Add(new Field(LuceneConstants.FILE_PATH, f, Field.Store.YES, Field.Index.NOT_ANALYZED));
        //        docf.Add(new Field(LuceneConstants.TYPE, "FILE", Field.Store.YES, Field.Index.NOT_ANALYZED));
        //        docf.Add(new Field(LuceneConstants.FILE_NAME, new System.IO.FileInfo(f).Name, Field.Store.YES, Field.Index.ANALYZED));
        //        w.AddDocument(docf); 
        //    }
        //    string[] dirs = System.IO.Directory.GetDirectories(dir);
        //    foreach(string subd in dirs)
        //    {
        //        SearchDir(subd, w);
        //    }
        //}




        public void Update(string root)
        {
            CalcCanvasHeight();
            tagCanvas.UriDB = uriDB;
            tagCanvas.Update(tagDB, root);
        }
        ITagDB tagDB = TagDBFactory.CreateTagDB();
        IUriDB uriDB = UriDBFactory.CreateUriDB();
        //ITagLayout tagLayout = TagLayoutFactory.CreateLayout();
        
        
        private void scrollViewer_MouseDoubleClick(object sender, MouseButtonEventArgs e)
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
            //canvas.Height = Math.Max(canvas.Height, rGrid.RenderSize.Height-10);
            tagCanvas.CanvasMinHeight = uriList.RenderSize.Height - 10;
        }
        private void Window_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            CalcCanvasHeight();
        }

        private void autoTextBox_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                if (e.KeyboardDevice.Modifiers == ModifierKeys.Control)
                {
                    string tag = tagDB.AddTag(autoTextBox.Text);
                    if (tag != null)
                    {
                        Update(tag);
                        autoTextBox.Text = "";
                    }
                    
                }
                else if(tagDB.QueryTagAlias(autoTextBox.Text).Count>0)
                {
                    Update(autoTextBox.Text);
                }
                //else if (autoTextBox.Text.IndexOfAny(GConfig.SpecialChar.ToArray<char>()) == -1)
                //{
                //    ShowTagGraph(autoTextBox.Text, true);
                //    autoTextBox.Text = "";
                //    ShowLstItem();
                //}
            }
        }

        private void CommandBinding_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
        }

        private void CommandBinding_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            tagCanvas.Paste();
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            tagDB.Dispose();
            uriDB.Dispose();
            tagDB = null;
            uriDB = null;
            Logger.Log(@"
**********************************************************************
*
*Close main window
*
**********************************************************************
");
        }
    }
}
