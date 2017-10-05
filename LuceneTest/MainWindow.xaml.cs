using AnyTagNet;
using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using TagExplorer.TagMgr;
using TagExplorer.UriMgr;
using TagExplorer.Utils;
using Xceed.Wpf.AvalonDock.Layout.Serialization;

namespace TagExplorer
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window,IDisposable
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
            Logger.I(@"
////////////////////////////////////////////////////////////////////
//
//init main window
//
///////////////////////////////////////////////////////////////////
");

            InitializeComponent();
            
            Logger.I("InitializeComponent Finished!");
            tagCanvas.SelectedTagChanged += selectedTagChanged;
            autoTextBox.textBox.TextChanged += TextBox_TextChanged;
            uriDB.UriDBChanged += UpdateUriList;
            autoTextBox.SearchDataProvider = tagDB;
            Update(LRUTag.Ins.DefaultTag);
            uriList.CurrentUriChangedCallback += CurrentUriChanged;
            richTxt.Focus();
            
            IDisposableFactory.New<MainWindow>(this);

        }
        public void CurrentUriChanged(string uri)
        {
            //richTxt.Load(uri);  richtxt不与文件列表关联，而是与当前选中的tag关联。
            
        }

        public void UpdateUriList()
        {
            uriList.ShowQueryResult(autoTextBox.Text, uriDB,tagDB);
        }
        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            UpdateUriList();
        }

        public void selectedTagChanged(string tag)
        {
            autoTextBox.Text = tag;
            string uri = PathHelper.GetNoteFileByTag(tag);
            richTxt.Load(uri);
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
            Logger.I("Show Tag " + root);
            CalcCanvasHeight();
            tagCanvas.UriDB = uriDB;
            tagCanvas.ShowGraph(tagDB, root);
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

        private void Paste_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
        }

        private void Paste_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            //tagCanvas.PasteFiles();
            tagCanvas.miPasteFile_Click(sender, e);
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            IDisposableFactory.Dispose(tagDB);
            IDisposableFactory.Dispose(uriDB);
            //tagDB.Dispose();
            //uriDB.Dispose();
            tagDB = null;
            uriDB = null;
            Logger.I(@"
**********************************************************************
*
*Close main window
*
**********************************************************************
");
        }

        private void Cut_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
            return ;
        }

        private void Cut_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            tagCanvas.miCutTag_Click(sender, e);      
        }

        private void Copy_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
        }

        private void Copy_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            tagCanvas.miCopyTag_Click(sender, e);
        }
        private void Dead(string x)
        {
            //Dead(x);
        }
        private void BtForward_Click(object sender, RoutedEventArgs e)
        {
            //System.Diagnostics.Process.Start(@"C:\Program Files\Internet Explorer\iexplore.exe",
            //    @"D:\00TagExplorerBase\DocumentBase\Doc\分布式架构\Raft 为什么是更易理解的分布式一致性算法 - mindwind - 博客园.mht");
            if (MessageBox.Show("导出所有Uri信息？","",MessageBoxButton.OKCancel) == MessageBoxResult.OK)
            {
                //Dead("导出完成！");
                //throw new Exception("mock“）");
                (uriDB as LuceneUriDB)?.Dbg();
                MessageBox.Show("导出完成！");
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (GStyle.mode == LAYOUT_COMPACT_MODE.GRAPH_BEGIN)
                GStyle.mode = LAYOUT_COMPACT_MODE.TREE_NO_COMPACT;
            else GStyle.mode = (GStyle.mode + 1);
            Update(LRUTag.Ins.DefaultTag);
        }

        public void Dispose()
        {
            SaveLayout();
        }
        private void LoadLayout()
        {
            if (File.Exists(CfgPath.LayoutCfgFilePath))
            {
                var serializer = new XmlLayoutSerializer(dockingManager);
                using (var stream = new StreamReader(CfgPath.LayoutCfgFilePath))
                    serializer.Deserialize(stream);
                
            }

        }
        private void SaveLayout()
        {
            var serializer = new XmlLayoutSerializer(dockingManager);
            using (var stream = new StreamWriter(CfgPath.LayoutCfgFilePath))
                serializer.Serialize(stream);
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            LoadLayout();
        }
    }
}
