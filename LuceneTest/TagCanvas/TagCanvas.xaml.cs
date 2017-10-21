using TagExplorer.TagMgr;
using TagExplorer.UriMgr;
using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using TagExplorer.Utils;
using AnyTagNet;
using TagExplorer.TagCanvas;
using TagExplorer.TagLayout.CommonLayout;

namespace TagExplorer
{

    /// <summary>
    /// 有向图的显示与文件的添加操作
    /// </summary>
    public partial class TagGraphCanvas : UserControl
    {

        public void InitDB(ITagDB db,IUriDB uridb)
        {
            tagDB = db;
            UriDB = uridb;
            MainCanvas.Initial(db,uridb,LayoutMode.LRTREE_COMPACT_MORE, LayoutCanvas.MAIN_CANVAS);
            SubCanvas.Initial(db,uridb,LayoutMode.TREE_COMPACT, LayoutCanvas.SUB_CANVAS);
        }
        FileWatcherSafe fileWather;
        //FileSystemWatcher fileWather = null;
        public TagGraphCanvas()
        {
            InitializeComponent();
            fileWather = new FileWatcherSafe(FileWather_Changed);

            MainCanvas.SelectedTagChanged += MainCanvasSelectedTagChanged_Callback;
            SubCanvas.SelectedTagChanged += SubCanvasSelectedTagChanged_Callback;


            GridLengthConverter c = new GridLengthConverter();
            mainGridRow.Height = (GridLength)c.ConvertFromString(AppCfg.Ins.MainCanvasHeight);
            //subGridRow.Height = (GridLength)c.ConvertFromString(AppCfg.Ins.SubCanvasHeight);



        }
        private void MainCanvasSelectedTagChanged_Callback(string tag)
        {
            SelectedTagChanged?.Invoke(tag);
            SubCanvas.ChangeRoot(tag,tag);
        }
        private void SubCanvasSelectedTagChanged_Callback(string tag)
        {
            SelectedTagChanged?.Invoke(tag);

        }


        private void NotifyList()
        {
            UriDB.Notify();
        }

        //观察文件变化==删除处理(TODO 真正的删除）
        private void FileWather_Deleted(object sender, FileSystemEventArgs e)
        {
            
            Logger.I("file watch delete : {0}", e.FullPath);
            int tryTimes = 0;
            while(File.Exists(e.FullPath) || Directory.Exists(e.FullPath))
            {
                System.Threading.Thread.Sleep(100);
                tryTimes++;
                if (tryTimes > 100)
                {
                    Logger.E("del file failed! -{0}", e.FullPath);
                    return; //尝试100次后，该文件仍然存在，放弃本次操作
                }
            }
            this.Dispatcher.Invoke(new Action(NotifyList));
        }
        //观察文件变化==添加处理
        private void FileWather_Created(object sender, FileSystemEventArgs e)
        {
            Logger.I("file watch create : {0}", e.FullPath);
            AddFileInDoc_BackThread(e.FullPath);
        }
        private bool FileWather_Changed( FileSystemEventArgs e)
        {
            switch(e.ChangeType)
            {
                case WatcherChangeTypes.Created: FileWather_Created(null, e);break;
                case WatcherChangeTypes.Deleted: FileWather_Deleted(null, e); break;
                case WatcherChangeTypes.Renamed: FileWather_Created(null, e); break;
            }

            return true;
        }

        //观察文件变化==重命名处理   TODO：重命名后，实际上需要删除原来老的doc，暂时没有处理
        private void FileWather_Renamed(object sender, RenamedEventArgs e)
        {
            Logger.I("file watch rename : {1}=>{0}", e.FullPath,e.OldFullPath);
            AddFileInDoc_BackThread(e.FullPath);
        }

        //由于文件变更通知是在一个后台线程中进行的，所以需要通过Invoke机制调用UI主线程中的函数
        private void AddFileInDoc_BackThread(string uri)
        {
            if (!CfgPath.NeedSkip(uri))//过滤一些不需要观察的文件
            {
                this.Dispatcher.Invoke(new Action<string>(AddFileInDoc), uri);
            }
            else
            {
                this.Dispatcher.Invoke(new Action(NotifyList));//有一个bug，当ie保存一个文件时，总是先create，然后delete，然后再次create。
            }
        }
        public void RedrawGraph()
        {
            MainCanvas.RedrawGraph();
            SubCanvas.RedrawGraph();
        }
        public void ShowGraph(string root,string sub)
        {
            if (root != null)
            {
                MainCanvas.ChangeRoot(root,sub);
            }
            if (sub != null)
            {
                MainCanvas.ClearSelected();
                SubCanvas.ChangeRoot(sub,sub);
            }
            
        }

        //UI主线程中的方法调用
        private void AddFileInDoc(string uri)
        {
            Logger.I("AddFileInDoc={0}", uri);
            string tag = CfgPath.GetTagByPath(uri);
            if (tag != null)
            {
                UriDB.AddUri(new List<string>() { uri }, new List<string>() { tag });
            }
            else
            { 
                Logger.E("观察到文件变化，但该文件不在文件仓库中=={0}", uri);
            }

        }

        internal void SearchByTxt(string text)
        {
            //先在main中查找，如果有，切换焦点后返回
            if(MainCanvas.ChangeSelectd(text))
            {
                return;
            }
            //再在sub中查找，如果有，切换焦点后返回
            if(SubCanvas.ChangeSelectd(text))
            {
                MainCanvas.ClearSelected();
                return;
            }
            //如果不在视图中，但数据库中存在，TODO：如何有效的切换？？是一个需要考虑的问题
            if(tagDB.QueryTagAlias(text).Count>0)
            {
                List<string> p1s = null, p2s = null, p3s = null;
                string p1 = null, p2 = null, p3 = null;

                if (!string.IsNullOrEmpty(text))
                {
                    p1s = tagDB.QueryTagParent(text);
                    if (p1s.Count > 0) p1 = p1s[0];
                }
                if (!string.IsNullOrEmpty(p1))
                {
                    p2s = tagDB.QueryTagParent(text);
                    if (p2s.Count > 0) p2 = p2s[0];
                }
                if (!string.IsNullOrEmpty(p2))
                {
                    p3s = tagDB.QueryTagParent(p2);
                    if (p3s.Count > 0) p3 = p3s[0];
                }

                string ppp = p3 != null ? p3 : p2 != null ? p2 : p1 != null ? p1 : text;
                ShowGraph(ppp, text);
                
            }
            //不存在精确匹配的tag
            else
            {
                
            }
        }

        private ITagDB tagDB = null;
        public IUriDB UriDB = null;


        public CurrentTagChanged SelectedTagChanged = null;
        private void UpdateRecentTags(string tag)
        {
            List<UIElement> recentTags = new List<UIElement>();
            LRUTag.Ins.Add(tag);
            List<string> tags = LRUTag.Ins.GetTags();
            double top = 0, left = 0;
            for (int i = 0; i < tags.Count; i++)
            {
                TagBox box = UIElementFactory.CreateTagBox(new TagLayout.LayoutCommon.GTagBox(4,tags[i],left,top,1));//GStyle.Apply(left, top, tags[i]);
                recentTags.Add(box);
                left += box.Width1;
                left += 10;
            }
            //canvasRecentTags.Children.Clear();
            foreach (TagBox t in recentTags)
            {
                //TODO 是否还需要recent tag？
                //设置每一个tag的上下文菜单和事件响应钩子
                //t.ContextMenu = TagAreaMenu;
                //t.MouseLeftButtonDown += Tag_MouseLeftButtonDown;
                //t.MouseDoubleClick += Tag_MouseDoubleClick;
                //canvasRecentTags.Children.Add(t);
            }
        }

        private void MainCanvas_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            GridLengthConverter c = new GridLengthConverter();
            
            AppCfg.Ins.MainCanvasHeight = c.ConvertToString(new GridLength(mainGridRow.ActualHeight,GridUnitType.Pixel));
            //AppCfg.Ins.SubCanvasHeight = c.ConvertToString(new GridLength(subGridRow.ActualHeight, GridUnitType.Star));
        }

        public void UpTag()
        {
            MainCanvas.UpTag();
        }

        internal void HomeTag()
        {
            ShowGraph(Cfg.Ins.DefaultTag, null);
        }




        //private void scrollViewer_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        //{
        //    if (e.OriginalSource is TagBox)
        //    {
        //        TagBox b = (TagBox)e.OriginalSource;
        //        ShowGraph(b.Text);
        //    }
        //}

        //private void canvas_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        //{

        //}

        //private void canvas_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        //{

        //}

        //private void canvas_Click(object sender, RoutedEventArgs e)
        //{

        //}







    }
}
