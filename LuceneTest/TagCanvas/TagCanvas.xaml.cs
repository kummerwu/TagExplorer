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
using TagExplorer.Utils.Cfg;
using TagExplorer.AutoComplete;

namespace TagExplorer
{

    /// <summary>
    /// 有向图的显示与文件的添加操作
    /// </summary>
    public partial class TagGraphCanvas : UserControl
    {
        #region 文件变更监听处理相关流程
        private FileWatcherSafe fileWather;
        //监听到文件变更后的处理总入口函数（BackThread为后缀的函数，都是在file变更观察后台线程中运行，）
        private bool FileChanged_BackThread(FileSystemEventArgs e)
        {
            switch (e.ChangeType)
            {
                case WatcherChangeTypes.Created: FileCreated_BackThread(null, e); break;
                case WatcherChangeTypes.Deleted: FileDeleted_BackThread(null, e); break;
                case WatcherChangeTypes.Renamed: FileCreated_BackThread(null, e); break;
            }
            return true;
        }
        //观察文件变化==添加处理
        private void FileCreated_BackThread(object sender, FileSystemEventArgs e)
        {
            Logger.I("file watch create : {0}", e.FullPath);
            AddFileToDB_BackThread(e.FullPath);
        }
        //观察文件变化==删除处理(TODO 真正的删除）
        private void FileDeleted_BackThread(object sender, FileSystemEventArgs e)
        {

            Logger.I("file watch delete : {0}", e.FullPath);
            int tryTimes = 0;
            while (File.Exists(e.FullPath) || Directory.Exists(e.FullPath))
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
        //观察文件变化==重命名处理   TODO：重命名后，实际上需要删除原来老的doc，暂时没有处理
        private void FileRename_BackThread(object sender, RenamedEventArgs e)
        {
            Logger.I("file watch rename : {1}=>{0}", e.FullPath, e.OldFullPath);
            AddFileToDB_BackThread(e.FullPath);
        }
        //由于文件变更通知是在一个后台线程中进行的，所以需要通过Invoke机制调用UI主线程中的函数
        private void AddFileToDB_BackThread(string uri)
        {
            if (!CfgPath.NeedSkip(uri))//过滤一些不需要观察的文件
            {
                this.Dispatcher.Invoke(new Action<string>(AddFileToDB_UIThread), uri);
            }
            else
            {
                this.Dispatcher.Invoke(new Action(NotifyList));//有一个bug，当ie保存一个文件时，总是先create，然后delete，然后再次create。
            }
        }
        //UI主线程中的方法调用
        private void AddFileToDB_UIThread(string uri)
        {
            Logger.I("AddFileInDoc={0}", uri);
            string tag = CfgPath.GetTagByPath(uri);
            if (tag != null)
            {
                UriDB.AddUris(new List<string>() { uri }, new List<string>() { tag });
            }
            else
            {
                Logger.E("观察到文件变化，但该文件不在文件仓库中=={0}", uri);
            }

        }
        #endregion




        #region 控件的私有成员和初始化
        private ITagDB TagDB = null;
        private IUriDB UriDB = null;
        public void InitDB(ITagDB tagDB,IUriDB uriDB)
        {
            TagDB = tagDB;
            UriDB = uriDB;
            MainCanvas.Initial(tagDB,uriDB, LayoutCanvas.MAIN_CANVAS);
            SubCanvas.Initial(tagDB,uriDB,LayoutCanvas.SUB_CANVAS);
        }
        public TagGraphCanvas()
        {
            InitializeComponent();
            fileWather = new FileWatcherSafe(FileChanged_BackThread);
            MainCanvas.SelectedTagChanged += MainCanvasSelectedTagChanged_Callback;
            SubCanvas.SelectedTagChanged += SubCanvasSelectedTagChanged_Callback;


            GridLengthConverter c = new GridLengthConverter();
            mainGridRow.Height = (GridLength)c.ConvertFromString(DynamicCfg.Ins.MainCanvasHeight);
            //subGridRow.Height = (GridLength)c.ConvertFromString(AppCfg.Ins.SubCanvasHeight);



        }
        #endregion


        #region 切换Tag，显示Tag
        public CurrentTagChanged SelectedTagChanged = null;
        private void MainCanvasSelectedTagChanged_Callback(GUTag tag)
        {
            SelectedTagChanged?.Invoke(tag);
            SubCanvas.ChangeMainSelected(tag);
            SubCanvas.ChangeRoot(tag,tag);
        }
        private void SubCanvasSelectedTagChanged_Callback(GUTag tag)
        {
            SelectedTagChanged?.Invoke(tag);

        }
        public void RedrawGraph()
        {
            MainCanvas.RedrawGraph();
            SubCanvas.RedrawGraph();
        }
        public void ChangeRoot(GUTag root, GUTag sub, GUTag subsel)
        {
            if (root != null)
            {
                MainCanvas.ChangeRoot(root, sub);
            }
            if (sub != null)
            {
                MainCanvas.ClearSelected();
                SubCanvas.ChangeRoot(sub, subsel);
            }

        }
        public void UpTag()
        {
            MainCanvas.UpTag();
        }
        internal void HomeTag()
        {
            GUTag defaultTag = GUTag.Parse(StaticCfg.Ins.DefaultTagID.ToString(), TagDB);
            ChangeRoot(defaultTag, null, null);
        }
        #endregion

        

        #region 搜索Tag，更新文件列表
        public void SearchByTxt(AutoCompleteTipsItem aItem)
        {
            //先在main中查找，如果有，切换焦点后返回
            if(MainCanvas.ChangeSelectedByTxt(aItem)!=null)
            {
                return;
            }
            //再在sub中查找，如果有，切换焦点后返回
            if(SubCanvas.ChangeSelectedByTxt(aItem)!=null)
            {
                MainCanvas.ClearSelected();
                return;
            }

            //如果item精确对应到一个GUTag，直接使用该GUTag
            GUTag tag = aItem.Data as GUTag;
            if (tag == null)
            {
                List<GUTag> tags = TagDB.QueryTags(aItem.Content);
                if (tags.Count > 0) tag = tags[0];
            }
            
            //如果不在视图中，但数据库中存在，TODO：如何有效的切换？？是一个需要考虑的问题
            if(tag!=null)
            {
                GUTag mainRoot = TagDB.GetTag(StaticCfg.Ins.DefaultTagID);
                GUTag subRoot = mainRoot;
                GUTag subSel = subRoot;
                List<GUTag> parents = QueryParentHistory(tag);
                int cnt = parents.Count;
                if(cnt>0)
                {
                    subSel = parents[0];
                    subRoot = parents[Math.Min(3, cnt - 1)];
                    mainRoot = parents[Math.Min(6, cnt - 1)];
                }
                
                ChangeRoot(mainRoot, subRoot,subSel);
                
            }
            //不存在精确匹配的tag
            else
            {
                
            }
        }
        private List<GUTag> QueryParentHistory(GUTag tag)
        {
            List<GUTag> ret = new List<GUTag>();
            int MAX = 6;
            GUTag child = tag;
            while (ret.Count < MAX)
            {
                ret.Add(child);
                List<GUTag> tmp = TagDB.QueryTagParent(child);
                if (tmp.Count > 0)
                {
                    child = tmp[0];
                }
                else
                {
                    break;
                }
            }
            return ret;
        }
        private void NotifyList()
        {
            UriDB.Notify();
        }
        #endregion



        #region 窗口大小变化，保存布局
        private void MainCanvas_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            GridLengthConverter c = new GridLengthConverter();

            DynamicCfg.Ins.MainCanvasHeight = (c.ConvertToString(new GridLength(mainGridRow.ActualHeight,GridUnitType.Pixel)));
            //AppCfg.Ins.SubCanvasHeight = c.ConvertToString(new GridLength(subGridRow.ActualHeight, GridUnitType.Star));
        }
        #endregion


    }
}
