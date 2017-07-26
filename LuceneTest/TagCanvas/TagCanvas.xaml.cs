using TagExplorer.TagLayout;
using TagExplorer.TagMgr;
using TagExplorer.UriMgr;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Shapes;
using TagExplorer.Utils;
using AnyTagNet;
using TagExplorer.TagCanvas;

namespace TagExplorer
{

    /// <summary>
    /// 有向图的显示与文件的添加操作
    /// </summary>
    public partial class TagGraphCanvas : UserControl
    {
        


        FileSystemWatcher fileWather = null;
        public TagGraphCanvas()
        {
            InitializeComponent();
            fileWather = new FileSystemWatcher(PathHelper.DocDir);
            fileWather.IncludeSubdirectories = true;
            fileWather.Path = PathHelper.DocDir;
            fileWather.Renamed += FileWather_Renamed;
            fileWather.Created += FileWather_Created;
            fileWather.Deleted += FileWather_Deleted;
            fileWather.EnableRaisingEvents = true;

            //CurrentTagInf.SetBinding(TextBlock.TextProperty, new Binding("Tips") { Source = TipsCenter.Ins });
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

       
        //观察文件变化==重命名处理   TODO：重命名后，实际上需要删除原来老的doc，暂时没有处理
        private void FileWather_Renamed(object sender, RenamedEventArgs e)
        {
            Logger.I("file watch rename : {1}=>{0}", e.FullPath,e.OldFullPath);
            AddFileInDoc_BackThread(e.FullPath);
        }

        //由于文件变更通知是在一个后台线程中进行的，所以需要通过Invoke机制调用UI主线程中的函数
        private void AddFileInDoc_BackThread(string uri)
        {
            if (!PathHelper.NeedSkipThisUri(uri))//过滤一些不需要观察的文件
            {
                this.Dispatcher.Invoke(new Action<string>(AddFileInDoc), uri);
            }
        }

        //UI主线程中的方法调用
        private void AddFileInDoc(string uri)
        {
            Logger.I("AddFileInDoc={0}", uri);
            string tag = PathHelper.GetTagByPath(uri);
            if (tag != null)
            {
                UriDB.AddUri(uri, new List<string>() { tag });
            }
            else
            {
                Logger.E("观察到文件变化，但该文件不在文件仓库中=={0}", uri);
            }

        }

        private double canvasMinHeight = 0;
        private double layoutHeight = 0;
        public double CanvasMinHeight
        {
            set
            {
                canvasMinHeight = value;
                SetHeight();
            }
        }
        
        private void SetHeight()
        {
            if(double.IsNaN(canvasMinHeight) || double.IsInfinity(canvasMinHeight) )
            {
                canvasMinHeight = 0;
            }
            canvas.Height = Math.Max(layoutHeight, canvasMinHeight);
        }
        private ITagDB tagDB = null;
        public IUriDB UriDB = null;

        //currentTag和root的区别： 
        //root是当前有向图显示的中心节点，
        //currentTag是当前选中的节点
        //
        private string root;
        private string currentTag = "";
        public void Refresh()
        {
            if(tagDB!=null && root!=null)
            {
                ShowGraph(tagDB, root);
            }
        }
        public void ShowGraph(string root)
        {
            if (tagDB != null)
            {
                ShowGraph(tagDB, root);
            }
        }

        private void UpdateRecentTags(string tag)
        {
            List<UIElement> recentTags = new List<UIElement>();
            LRUTag.Ins.Add(tag);
            List<string> tags = LRUTag.Ins.GetTags();
            double top = 0, left = 0;
            for (int i = 0; i < tags.Count; i++)
            {
                TagBox box = GStyle.Apply(left, top, tags[i]);
                recentTags.Add(box);
                left += box.Width1;
                left += 10;
            }
            canvasRecentTags.Children.Clear();
            foreach (TagBox t in recentTags)
            {
                //设置每一个tag的上下文菜单和事件响应钩子
                t.ContextMenu = TagAreaMenu;
                t.MouseLeftButtonDown += Tag_MouseLeftButtonDown;
                t.MouseDoubleClick += Tag_MouseDoubleClick;
                canvasRecentTags.Children.Add(t);
            }
        }

        public void ShowGraph(ITagDB tagDB,string root)
        {
            Logger.I("ShowGraph at " + root);
            this.tagDB = tagDB;
            this.root = root;
            
            canvas.Children.Clear();
            canvasRecentTags.Children.Clear();

            //计算有向图布局
            ITagLayout tagLayout = TagLayoutFactory.CreateLayout();
            tagLayout.Layout(tagDB, root);

            //将有向图中的元素显示在界面上
            IEnumerable<UIElement> lines = tagLayout.Lines;
            IEnumerable<UIElement> allTxt = tagLayout.TagArea;
            
            canvas.Width = tagLayout.Size.Width;
            layoutHeight = tagLayout.Size.Height;
            SetHeight();
            
            foreach (Line l in lines)
            {
                canvas.Children.Add(l);
            }
            foreach (TagBox t in allTxt)
            {
                //设置每一个tag的上下文菜单和事件响应钩子
                t.ContextMenu = TagAreaMenu;
                t.MouseLeftButtonDown += Tag_MouseLeftButtonDown;
                t.MouseDoubleClick += Tag_MouseDoubleClick;
                canvas.Children.Add(t);
            }
            UpdateRecentTags(root);
            SetCurrentTag(root);
        }
        //双击tag，以该tag为根显示有向图
        private void Tag_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            TagBox b = sender as TagBox;
            if(b!=null)
                ShowGraph(b.Text);
        }
        //单击tag，将该tag改为选定状态  TODO，运行多个tag选中
        private void Tag_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if(sender is TagBox)
            {
                SetCurrentTag((sender as TagBox).Text);
            }
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
        ContextMenu TagAreaMenu
        {
            get
            {
                return Resources["tagAreaMenu"] as ContextMenu;
            }
        }
        private void miOpenTagDir_Click(object sender, RoutedEventArgs e)
        {
            UpdateCurrentTagByContextMenu();
            FileShell.OpenExplorerByTag(currentTag);
            
        }
        private void UpdateSelectedStatus(string tag)
        {
            foreach(UIElement u in canvas.Children)
            {
                TagBox tb = u as TagBox;

                if(tb!=null)
                {
                    if (tb.Selected)
                    {
                        tb.Selected = false;
                    }
                    if (tb.Text == tag)
                    {
                        tb.Selected = true;
                    }
                }
            }
            
        }
        
        public void SetCurrentTag(string tag)
        {
            UpdateSelectedStatus(tag); //这一句必须放在下面检查并return之前，
                            //即无论currentTag是否变化，都需要更新一下border，否则会有bug；
                            //bug现象：在curtag没有变化的时候，重新绘制整个graph，
                            //会出现所有的tag都不显示边框（包括curtag），因为直接返回了。
            
            //if (currentTag == tag) return;  //原来在tag没有变化时不通知变更，导致有些问题，后面将该语句取消了。
            string oldTag = currentTag;
            currentTag = tag;


            ShowCurrentTagInf();
            SelectedTagChanged?.Invoke(tag);
        }

        private string GetTagInf(string tag,ITagDB db)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("当前选中标签：" + tag);

            List<string> parents = db.QueryTagParent(tag);
            if (parents.Count > 0)
            {
                sb.Append(" Parent::= ");
                foreach (string s in parents) sb.Append(" " + s);
            }


            List<string> children = tagDB.QueryTagChildren(tag);
            if (children.Count > 0)
            {
                sb.Append(" Children::= ");
                foreach (string s in children) sb.Append(" " + s);
            }
            return sb.ToString().Trim();
        }
        private void ShowCurrentTagInf()
        {
            TipsCenter.Ins.TagInf = GetTagInf(currentTag, tagDB);
            //TipsCenter.Ins.Tips = GetTagInf(currentTag,tagDB);
            //CurrentTagInf.Text = GetTagInf(currentTag, tagDB);
        }
        public delegate void CurrentTagChanged(string tag);
        public CurrentTagChanged SelectedTagChanged = null;
        


        public void miPasteFile_Click(object sender, RoutedEventArgs e)
        {
            UpdateCurrentTagByContextMenu();
            

            string[] token = Clipboard.GetText().Split(new char[] { ClipboardConst.CommandSplitToken}, StringSplitOptions.RemoveEmptyEntries);
            
            if(token.Length==2)
            {
                string arg = token[1];
                
                string[] args = arg.Split(new char[] { ClipboardConst.ArgsSplitToken }, StringSplitOptions.RemoveEmptyEntries);
                switch (token[0])
                {
                    case ClipboardConst.KUMMERWU_TAG_COPY:
                        tagDB.AddTag(currentTag, arg); 
                        Refresh();
                        break;
                    case ClipboardConst.KUMMERWU_TAG_CUT:
                        tagDB.ResetRelationOfChild(currentTag, arg); 
                        Refresh();
                        break;
                    case ClipboardConst.KUMMERWU_URI_CUT:
                        MoveUris(args);
                        break;
                    case ClipboardConst.KUMMERWU_URI_COPY:
                        foreach (string uri in args)
                        {
                            UriDB.AddUri(uri, new List<string>() { currentTag });
                        }
                        break;
                    default:PasteFiles();break;
                }
            }
            else
            {
                PasteFiles();
            }
            
        }

        private void MoveUris(string[] args)
        {
            string[] src = args;
            string[] dst = PathHelper.MapFilesToTagDir(src, currentTag);
            FileShell.MoveFiles(src, dst);
            foreach (string uri in src)
            {
                UriDB.DelUri(uri, false); 
            }
            foreach(string uri in dst)
            {
                UriDB.AddUri(uri, new List<string>() { currentTag });
            }
        }
        public void PasteFiles() { PasteFiles(true); }
        public void PasteFiles(bool NeedCopy)
        {
            UpdateCurrentTagByContextMenu();
            AddUri(FileShell.GetFileListFromClipboard(),NeedCopy);
        }
        
        private void UpdateCurrentTagByContextMenu()
        {
            TagBox t = TagAreaMenu.PlacementTarget as TagBox;
            if (t != null && t.Text.Length > 0)
            {
                SetCurrentTag(t.Text);
            }
        }
        private void AddUri(List<string> files) { AddUri(files, true); }
        private void AddUri(List<string> files,bool NeedCopy)
        {
            if (UriDB == null || currentTag==null || currentTag.Length==0) return;

            List<string> tags = new List<string>() { currentTag };
            foreach(string f in files)
            {
                string dstFile = f;
                if (NeedCopy )
                {
                    dstFile = CopyToHouse(f, currentTag);
                }
                
                if (dstFile != null)
                {
                    UriDB.AddUri(dstFile, tags);
                }
            }
        }
        
        private string  CopyToHouse(string f, string tag)
        {
            if (FileShell.IsValidHttps(f)) return f;
            if(!FileShell.IsValidFS(f))
            {
                Logger.E("Copy To House: File not Exist " + f);
                return f;
            }
            FileInfo fi = new FileInfo(f);
            string dstDir = PathHelper.GetDirByTag(tag);
            string dstFile = System.IO.Path.Combine(dstDir, fi.Name);
            if(dstFile==f)
            {
                return f;
            }
            if (!System.IO.File.Exists(dstFile) && !System.IO.Directory.Exists(dstFile))//TODO 已经存在的需要提示覆盖、放弃、重命名
            {
                if(FileShell.CopyFile(f, dstFile))
                {
                    return dstFile;
                }
            }
            return null;
        }
        private void miCopyTagName_Click(object sender, RoutedEventArgs e)
        {
            UpdateCurrentTagByContextMenu();
            ClipBoardSafe.SetText(currentTag);
        }

        private void miCopyTagFullPath_Click(object sender, RoutedEventArgs e)
        {
            UpdateCurrentTagByContextMenu();
            ClipBoardSafe.SetText(PathHelper.GetDirByTag(currentTag));
        }

        private void tagAreaMenu_ContextMenuOpening(object sender, ContextMenuEventArgs e)
        {
            UpdateCurrentTagByContextMenu();
        }

        private void scrollViewer_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            double h = e.NewSize.Height;
            Grid p = scrollViewer.Parent as Grid;

            canvasMinHeight = p.ActualHeight-60;
            SetHeight();
        }

        private void miNewFile_Click(object sender, RoutedEventArgs e)
        {
            UpdateCurrentTagByContextMenu();
            string initDir = PathHelper.GetDirByTag(currentTag);
            SaveFileDialog sf = new SaveFileDialog();
            sf.InitialDirectory = initDir;

            sf.Filter = TemplateHelper.GetTemplateFileFilter();//"One文件(*.one)|*.one|Mind文件(*.xmind)|*.xmind";
            if(sf.ShowDialog()==true)
            {
                if(File.Exists(sf.FileName))
                {
                    MessageBox.Show("该文件已经存在"+sf.FileName,"文件名冲突",MessageBoxButton.OK,MessageBoxImage.Error);
                    return;
                }
                else
                {
                    FileInfo fi = new FileInfo(sf.FileName);
                    string tmplateFile = TemplateHelper.GetTemplateByExtension(fi.Extension);
                    if (tmplateFile != null &&  File.Exists(tmplateFile))
                    {
                        File.Copy(tmplateFile, sf.FileName);
                        AddUri(new List<string>() { sf.FileName });
                        FileShell.StartFile(sf.FileName);
                    }
                }
            }
        }

        public void miCopyTag_Click(object sender, RoutedEventArgs e)
        {
            UpdateCurrentTagByContextMenu();
            ClipBoardSafe.SetText(ClipboardConst.KUMMERWU_TAG_COPY + ClipboardConst.CommandSplitToken + currentTag);
        }

        public void miCutTag_Click(object sender, RoutedEventArgs e)
        {
            UpdateCurrentTagByContextMenu();
            ClipBoardSafe.SetText(ClipboardConst.KUMMERWU_TAG_CUT + ClipboardConst.CommandSplitToken + currentTag);
        }

        private void miDeleteTag_Click(object sender, RoutedEventArgs e)
        {
            UpdateCurrentTagByContextMenu();
            if (tagDB.QueryTagChildren(currentTag).Count==0)
            {
                tagDB.RemoveTag(currentTag);
            }
            else
            {
                MessageBox.Show(string.Format("[{0}]下还有其他子节点，如果确实需要删除该标签，请先删除所有子节点", currentTag), "提示：", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            ShowGraph(Cfg.Ins.DefaultTag);
        }

        private void miLinkInFile_Click(object sender, RoutedEventArgs e)
        {
            PasteFiles(false);
        }
        public string[] ParseTags(string input)
        {
            return input.Split(new char[] {' ',',','，' }, StringSplitOptions.RemoveEmptyEntries);
        }
        private void miNewTag_Click(object sender, RoutedEventArgs e)
        {
            UpdateCurrentTagByContextMenu();
            if (currentTag == null || currentTag.Trim() == "") return;

            NewTagWindow w = new NewTagWindow();
            w.Title = "创建子标签";
            w.Tips = string.Format("创建{0}子标签，多个子标签可以用空格隔开", currentTag);
            w.ShowDialog();
            string input = w.Inputs;
            if(input!=null && input.Trim().Length>0)
            {
                string[] tags = ParseTags(input);
                foreach (string tag in tags)
                {
                    tagDB.AddTag(currentTag, tag);
                }
                Refresh();
            }
            

        }

        private void miPasteTag_Click(object sender, RoutedEventArgs e)
        {
            UpdateCurrentTagByContextMenu();


            string[] token = Clipboard.GetText().Split(new char[] { ClipboardConst.CommandSplitToken }, StringSplitOptions.RemoveEmptyEntries);

            if (token.Length == 2)
            {
                string arg = token[1];

                string[] args = arg.Split(new char[] { ClipboardConst.ArgsSplitToken }, StringSplitOptions.RemoveEmptyEntries);
                switch (token[0])
                {
                    case ClipboardConst.KUMMERWU_TAG_COPY:
                        tagDB.AddTag(currentTag, arg);
                        Refresh();
                        break;
                    case ClipboardConst.KUMMERWU_TAG_CUT:
                        tagDB.ResetRelationOfChild(currentTag, arg);
                        Refresh();
                        break;
                }
            }
            
        }
    }
}
