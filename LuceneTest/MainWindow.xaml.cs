using AnyTagNet;
using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;

using System.Windows.Input;
using TagExplorer.TagLayout.CommonLayout;
using TagExplorer.TagLayout.TreeLayout;
using TagExplorer.TagMgr;
using TagExplorer.UriMgr;
using TagExplorer.Utils;
using TagExplorer.Utils.Cfg;
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

        #region 窗口的总体初始化与销毁
        public MainWindow()
        {
            Logger.I(@"init main window");

            InitializeComponent();

            //URI DB初始化
            Logger.I("InitializeComponent Finished!，init uridb");
            uriDB = UriDBFactory.CreateUriDB();
            uriDB.UriDBChanged += ShowUrlListByText;
            
            //TAG DB初始化
            Logger.I("InitializeComponent Finished!，init tagdb");
            tagDB = TagDBFactory.CreateTagDB();
            tagDB.TagDBChanged += TagDBChanged;

            //查询输入框初始化
            SearchBox.textBox.TextChanged += SearchBoxTextChanged_Callback;
            SearchBox.SearchDataProvider = tagDB;

            //Tag视图初始化
            tagCanvas.InitDB(tagDB,uriDB);
            tagCanvas.SelectedTagChanged += SelectedTagChanged_Callback;
            GUTag mroot = GUTag.Parse(DynamicCfg.Ins.MainCanvasRoot,tagDB);
            GUTag sroot = GUTag.Parse(DynamicCfg.Ins.SubCanvasRoot,tagDB);
            ShowTagGraph(mroot,sroot);

            IDisposableFactory.New<MainWindow>(this);

            richTxt.Focus();

        }
        private void Window_Closed(object sender, EventArgs e)
        {
            IDisposableFactory.Dispose(tagDB);
            IDisposableFactory.Dispose(uriDB);
            //tagDB.Dispose();
            //uriDB.Dispose();
            tagDB = null;
            uriDB = null;
            Logger.I(@"Close main window");
        }
        private void Window_SizeChanged(object sender, SizeChangedEventArgs e)
        {

        }
        #endregion

        #region 查询输入框相关的处理流程
        //输入框回车，开始查询
        private void SearchBoxKeyUp_Callback(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                SearchByTxt();
            }
        }
        //TODO:这个地方需要优化，用户任意搜索一个单词，就直接把root tag切换过去，实际上不合理
        private void SearchByTxt()
        {
            tagCanvas.SearchByTxt(SearchBox.Item);
            ShowUrlListByText();
        }
        private void SearchBoxTextChanged_Callback(object sender, TextChangedEventArgs e)
        {
            //ShowUrlListByText();
        }
        private void btSearch_Click(object sender, RoutedEventArgs e)
        {
            SearchByTxt();
        }
        #endregion


        private void TagDBChanged()
        {
            tagCanvas.RedrawGraph();
        }
        public void ShowUrlListByText()
        {
            uriList.ShowQueryResult(SearchBox.Text, uriDB,tagDB);
        }
        #region Tag相关的处理
        public void SelectedTagChanged_Callback(GUTag tag)
        {
            SearchBox.Text = tag.Title;
            //现在自己的这个richtextbox非常不好用，将其暂时废除，除非有一个好用的再说
            string uri = CfgPath.GetFileByTag(tag.Title,"note.rtf");
            richTxt.Load(uri);
            ShowUrlListByText();
            //修改text后，会自动触发 TextBox_TextChanged
            //进一步触发             ShowUrlListByText
        }

        public void ShowTagGraph(GUTag root,GUTag subroot)
        {
            Logger.I("Show Tag " + root);
            tagCanvas.ChangeRoot(root,subroot,subroot);
        }


        #endregion







        #region 命令处理相关
        private void Paste_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
        }

        private void Paste_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            //tagCanvas.PasteFiles();
            tagCanvas.MainCanvas.miPasteFile_Click(sender, e);
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
        private void btExport_Click(object sender, RoutedEventArgs e)
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
            //GUTODO:将字符串转换为gutag
            GUTag mroot = GUTag.Parse(DynamicCfg.Ins.MainCanvasRoot, tagDB);
            GUTag sroot = GUTag.Parse(DynamicCfg.Ins.SubCanvasRoot, tagDB);
            ShowTagGraph(mroot,sroot);
        }
#region 布局相关的处理
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
#endregion
        private void tagCanvas_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            System.Diagnostics.Debug.WriteLine(tagCanvas.ActualWidth + " " + tagCanvas.ActualHeight);
        }

        private void Import1_0()
        {
            var fd = new System.Windows.Forms.OpenFileDialog();
            fd.Title = "导入Tag文件";
            if (fd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                int cnt = tagDB.Import(fd.FileName);
                tagCanvas.RedrawGraph();
                MessageBox.Show("成功导入" + cnt + "条Tag关系", "Tag关系导入", MessageBoxButton.OK);
            }
        }
        private void Import1_1()
        {
            tagDB.Import(CfgPath.TagDBPath_Json);
        }
        private void btImport_Click(object sender, RoutedEventArgs e)
        {
            switch(StaticCfg.CURRENT_VERSION)
            {
                case "1.0":Import1_0();break;
                case "1.1":Import1_1();break;
            }
            
        }

        private void test_Click(object sender, RoutedEventArgs e)
        {
            if (MessageBox.Show("开始20次show根节点","测试",MessageBoxButton.OKCancel) == MessageBoxResult.OK)
            {
                DateTime t1 = DateTime.Now;
                for (int i = 0; i < 20; i++)
                {
                    GUTag tag = GUTag.Parse(StaticCfg.Ins.DefaultTagID.ToString(), tagDB);
                    ShowTagGraph(tag,tag);
                }
                DateTime t2 = DateTime.Now;
                MessageBox.Show("总共耗时:" + (t2 - t1).TotalSeconds+TreeLayoutEnv.StatInf);
            }
        }

        private void btUp_Click(object sender, RoutedEventArgs e)
        {
            tagCanvas.UpTag();
        }

        private void btHome_Click(object sender, RoutedEventArgs e)
        {
            tagCanvas.HomeTag();
        }
        #endregion
    }
}
