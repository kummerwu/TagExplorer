using AnyTagNet;
using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using TagExplorer.TagLayout.CommonLayout;
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

        


        ITagDB tagDB = null;
        IUriDB uriDB = null;
        //ITagLayout tagLayout = TagLayoutFactory.CreateLayout();
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

            //URI DB相关部分
            Logger.I("InitializeComponent Finished!，init uridb");
            uriDB = UriDBFactory.CreateUriDB();
            uriDB.UriDBChanged += ShowUrlListByText;

            //URI 相关部分
            //uriList.CurrentUriChangedCallback += CurrentUriChanged;

            //TAG DB相关部分
            Logger.I("InitializeComponent Finished!，init tagdb");
            tagDB = TagDBFactory.CreateTagDB();
            autoTextBox.textBox.TextChanged += TextChanged_Callback;
            autoTextBox.SearchDataProvider = tagDB;
            tagCanvas.InitDB(tagDB,uriDB);
            tagCanvas.SelectedTagChanged += SelectedTagChanged_Callback;
            ShowTagGraph(AppCfg.Ins.MainCanvasRoot,AppCfg.Ins.SubCanvasRoot);

            IDisposableFactory.New<MainWindow>(this);

            richTxt.Focus();

        }
        //public void CurrentUriChanged(string uri)
        //{
        //    //richTxt.Load(uri);  richtxt不与文件列表关联，而是与当前选中的tag关联。
            
        //}

        public void ShowUrlListByText()
        {
            uriList.ShowQueryResult(autoTextBox.Text, uriDB,tagDB);
        }
        private void TextChanged_Callback(object sender, TextChangedEventArgs e)
        {
            //ShowUrlListByText();
        }

        public void SelectedTagChanged_Callback(string tag)
        {
            autoTextBox.Text = tag;
            //现在自己的这个richtextbox非常不好用，将其暂时废除，除非有一个好用的再说
            //string uri = CfgPath.GetNoteFileByTag(tag);
            //richTxt.Load(uri);
            ShowUrlListByText();
            //修改text后，会自动触发 TextBox_TextChanged
            //进一步触发             ShowUrlListByText
        }

        public void ShowTagGraph(string root,string subroot)
        {
            Logger.I("Show Tag " + root);
            //CalcCanvasHeight();
            tagCanvas.UriDB = uriDB;
            tagCanvas.ShowGraph(root,subroot);
        }




        //private void CalcCanvasHeight()
        //{
        //    //canvas.Height = Math.Max(canvas.Height, rGrid.RenderSize.Height-10);
        //    //tagCanvas.CanvasMinHeight = uriList.RenderSize.Height - 10;
        //}
        private void Window_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            //CalcCanvasHeight();
            //MessageBox.Show(tagCanvas.ActualWidth + " " + tagCanvas.ActualHeight);
        }

        private void TextBoxKeyUp_Callback(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                if (e.KeyboardDevice.Modifiers == ModifierKeys.Control)
                {
                    //string tag = tagDB.AddTag(autoTextBox.Text);
                    //if (tag != null)
                    //{
                    //    Update(tag);
                    //    autoTextBox.Text = "";
                    //}
                    
                }
                else 
                {
                    SearchByTxt();
                }

            }
        }

        private void SearchByTxt()
        {
            if (tagDB.QueryTagAlias(autoTextBox.Text).Count > 0)
            {
                ShowTagGraph(autoTextBox.Text, null);
            }
            ShowUrlListByText();
        }

        private void Paste_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
        }

        private void Paste_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            //tagCanvas.PasteFiles();
            tagCanvas.MainCanvas.miPasteFile_Click(sender, e);
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
            tagCanvas.MainCanvas.miCutTag_Click(sender, e);      
        }

        private void Copy_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
        }

        private void Copy_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            tagCanvas.MainCanvas.miCopyTag_Click(sender, e);
        }
        private void Dead(string x)
        {
            //Dead(x);
        }
        private void BtForward_Click(object sender, RoutedEventArgs e)
        {
            //System.Diagnostics.Process.Start(@"C:\Program Files\Internet Explorer\iexplore.exe",
            //    @"D:\00TagExplorerBase\DocumentBase\Doc\分布式架构\Raft 为什么是更易理解的分布式一致性算法 - mindwind - 博客园.mht");
            if (MessageBox.Show("导出所有Uri信息和tag信息？","",MessageBoxButton.OKCancel) == MessageBoxResult.OK)
            {
                //Dead("导出完成！");
                //throw new Exception("mock“）");
                (uriDB as LuceneUriDB)?.Dbg();
                (tagDB as LuceneTagDB)?.TranslateToJson();
                if(!(tagDB is LuceneTagDB))
                {
                    LuceneTagDB ltb = new LuceneTagDB();
                    ltb.TranslateToJson();
                }
                MessageBox.Show("导出完成！");
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (GLayoutMode.mode == LayoutMode.GRAPH_UPDOWN)
                GLayoutMode.mode = 0;
            else GLayoutMode.mode = (GLayoutMode.mode + 1);
            ShowTagGraph(AppCfg.Ins.MainCanvasRoot,AppCfg.Ins.SubCanvasRoot);
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

        private void tagCanvas_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            System.Diagnostics.Debug.WriteLine(tagCanvas.ActualWidth + " " + tagCanvas.ActualHeight);
        }

        private void btSearch_Click(object sender, RoutedEventArgs e)
        {
            SearchByTxt();
        }
    }
}
