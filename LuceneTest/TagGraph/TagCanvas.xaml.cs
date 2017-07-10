using AnyTag.BL;
using AnyTags.Net;
using LuceneTest.Core;
using LuceneTest.TagLayout;
using LuceneTest.TagMgr;
using LuceneTest.UriMgr;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Shapes;

namespace LuceneTest.TagGraph
{
    
    /// <summary>
    /// 有向图的显示与文件的添加操作
    /// </summary>
    public partial class TagCanvas : UserControl
    {
        public const string KUMMERWU_TAG_COPY = "KUMMERWU_TAG_COPY";
        public const string KUMMERWU_TAG_CUT = "KUMMERWU_TAG_CUT";

        public const string KUMMERWU_URI_COPY = "KUMMERWU_URI_COPY";
        public const string KUMMERWU_URI_CUT = "KUMMERWU_URI_CUT";


        FileSystemWatcher fileWather = null;
        public TagCanvas()
        {
            InitializeComponent();
            fileWather = new FileSystemWatcher(MyPath.DocRoot);
            fileWather.IncludeSubdirectories = true;
            fileWather.Path = MyPath.DocRoot;
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

        //观察文件变化==删除处理
        private void FileWather_Deleted(object sender, FileSystemEventArgs e)
        {
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
            AddFileInDoc_BackThread(e.FullPath);
        }

       
        //观察文件变化==重命名处理   TODO：重命名后，实际上需要删除原来老的doc，暂时没有处理
        private void FileWather_Renamed(object sender, RenamedEventArgs e)
        {
            AddFileInDoc_BackThread(e.FullPath);
        }

        //由于文件变更通知是在一个后台线程中进行的，所以需要通过Invoke机制调用UI主线程中的函数
        private void AddFileInDoc_BackThread(string uri)
        {
            if (!MyPath.FileWatcherFilter(uri))//过滤一些不需要观察的文件
            {
                this.Dispatcher.Invoke(new Action<string>(AddFileInDoc), uri);
            }
        }

        //UI主线程中的方法调用
        private void AddFileInDoc(string uri)
        {
            string tag = MyPath.GetTagByPath(uri);
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
        private double realHeight = 0;
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
            canvas.Height = Math.Max(realHeight, canvasMinHeight);
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
        public void ShowGraph(ITagDB tagDB,string root)
        {
            this.tagDB = tagDB;
            this.root = root;
            
            canvas.Children.Clear();

            //计算有向图布局
            ITagLayout tagLayout = TagLayoutFactory.CreateLayout();
            tagLayout.Layout(tagDB, root);

            //将有向图中的元素显示在界面上
            IEnumerable<UIElement> lines = tagLayout.Lines;
            IEnumerable<UIElement> allTxt = tagLayout.TagArea;
            canvas.Width = tagLayout.Size.Width;
            realHeight = tagLayout.Size.Height;
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
        private void tagAreaNode_Click(object sender, RoutedEventArgs e)
        {
            UpdateCurrentTagByContextMenu();
            FileOperator.OpenTagDir(currentTag);
            
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
        public static char CommandSplitToken = '`';
        public static char ArgsSplitToken = '?';
        public void miPaste_Click(object sender, RoutedEventArgs e)
        {
            UpdateCurrentTagByContextMenu();
            

            string[] token = Clipboard.GetText().Split(new char[] { CommandSplitToken}, StringSplitOptions.RemoveEmptyEntries);
            
            if(token.Length==2)
            {
                string arg = token[1];
                
                string[] args = arg.Split(new char[] { ArgsSplitToken}, StringSplitOptions.RemoveEmptyEntries);
                switch (token[0])
                {
                    case KUMMERWU_TAG_COPY:
                        tagDB.AddTag(currentTag, arg); 
                        Refresh();
                        break;
                    case KUMMERWU_TAG_CUT:
                        tagDB.ResetRelationOfChild(currentTag, arg); 
                        Refresh();
                        break;
                    case KUMMERWU_URI_CUT:
                        foreach (string uri in args)
                        {
                            UriDB.DelUri(uri, false); UriDB.AddUri(uri, new List<string>() { currentTag }); 
                        }
                        break;
                    case KUMMERWU_URI_COPY:
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

        
        public void PasteFiles()
        {
            UpdateCurrentTagByContextMenu();
            AddUri(FileOperator.GetFileListFromClipboard());
        }
        private void UpdateCurrentTagByContextMenu()
        {
            TagBox t = TagAreaMenu.PlacementTarget as TagBox;
            if (t != null && t.Text.Length > 0)
            {
                SetCurrentTag(t.Text);
            }
        }

        private void AddUri(List<string> files)
        {
            if (UriDB == null || currentTag==null || currentTag.Length==0) return;

            List<string> tags = new List<string>() { currentTag };
            foreach(string f in files)
            {
                string dstFile = CopyToHouse(f, currentTag);
                if (dstFile != null)
                {
                    UriDB.AddUri(dstFile, tags);
                }
            }
        }
        
        private string  CopyToHouse(string f, string tag)
        {
            System.IO.FileInfo fi = new System.IO.FileInfo(f);
            string dstDir = MyPath.GetDirPath(tag);
            string dstFile = System.IO.Path.Combine(dstDir, fi.Name);
            if(dstFile==f)
            {
                return f;
            }
            if (!System.IO.File.Exists(dstFile) && !System.IO.Directory.Exists(dstFile))//TODO 已经存在的需要提示覆盖、放弃、重命名
            {
                if(FileOperator.CopyFile(f, dstFile))
                {
                    return dstFile;
                }
            }
            return null;
        }
        private void copyAreaNode_Click(object sender, RoutedEventArgs e)
        {
            UpdateCurrentTagByContextMenu();
            Clipboard.SetText(currentTag);
        }

        private void copyFullPath_Click(object sender, RoutedEventArgs e)
        {
            UpdateCurrentTagByContextMenu();
            Clipboard.SetText(MyPath.GetDirPath(currentTag));
        }

        private void tagAreaMenu_ContextMenuOpening(object sender, ContextMenuEventArgs e)
        {
            UpdateCurrentTagByContextMenu();
        }

        private void scrollViewer_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            double h = e.NewSize.Height;
            Grid p = scrollViewer.Parent as Grid;

            canvasMinHeight = p.ActualHeight;
            SetHeight();
        }

        private void miNew_Click(object sender, RoutedEventArgs e)
        {
            UpdateCurrentTagByContextMenu();
            string initDir = MyPath.GetDirPath(currentTag);
            SaveFileDialog sf = new SaveFileDialog();
            sf.InitialDirectory = initDir;

            sf.Filter = MyTemplate.GetTemplateFileFilter();//"One文件(*.one)|*.one|Mind文件(*.xmind)|*.xmind";
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
                    string tmplateFile = MyTemplate.DefaultDocTemplate(fi.Extension);
                    if (tmplateFile != null &&  File.Exists(tmplateFile))
                    {
                        File.Copy(tmplateFile, sf.FileName);
                        AddUri(new List<string>() { sf.FileName });
                        FileOperator.StartFile(sf.FileName);
                    }
                }
            }
        }

        public void miCopy_Click(object sender, RoutedEventArgs e)
        {
            UpdateCurrentTagByContextMenu();
            Clipboard.SetText(KUMMERWU_TAG_COPY + CommandSplitToken+ currentTag);
        }

        public void miCut_Click(object sender, RoutedEventArgs e)
        {
            UpdateCurrentTagByContextMenu();
            Clipboard.SetText(KUMMERWU_TAG_CUT+CommandSplitToken + currentTag);
        }

        private void miDelete_Click(object sender, RoutedEventArgs e)
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
            ShowGraph("我的大脑");
        }
    }
}
