using AnyTag.BL;
using AnyTags.Net;
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
using System.Windows.Input;
using System.Windows.Shapes;

namespace LuceneTest.TagGraph
{
    
    /// <summary>
    /// TagCanvas.xaml 的交互逻辑
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
        }
        private void NotifyList()
        {
            UriDB.Notify();
        }
        private void FileWather_Deleted(object sender, FileSystemEventArgs e)
        {
            while(File.Exists(e.FullPath) || Directory.Exists(e.FullPath))
            {
                System.Threading.Thread.Sleep(100);
            }
            this.Dispatcher.Invoke(new Action(NotifyList));
        }

        private void FileWather_Created(object sender, FileSystemEventArgs e)
        {
            AddFileInDoc(e.FullPath);
            //MessageBox.Show("Create");
        }

       

        private void FileWather_Renamed(object sender, RenamedEventArgs e)
        {
            AddFileInDoc(e.FullPath);
            //MessageBox.Show("rename");
        }
        private void AddFileInDoc_Invoke(string uri)
        {
            string tag = MyPath.GetTagByPath(uri);
            if (tag != null)
            {
                UriDB.AddUri(uri, new List<string>() { tag });
            }

        }
        
        private void AddFileInDoc(string uri)
        {
            if (!MyPath.FileWatcherFilter(uri))
            {
                this.Dispatcher.Invoke(new Action<string>(AddFileInDoc_Invoke), uri);
            }
        }
        public double CanvasMinHeight
        {
            set
            {
                canvasMinHeight = value;
                SetHeight();
                
            }
        }
        private double canvasMinHeight = 0;
        private double realHeight = 0;
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
        private string root;
        
        public void Update()
        {
            if(tagDB!=null && root!=null)
            {
                Update(tagDB, root);
            }
        }
        public void Update(string root)
        {
            if (tagDB != null)
            {
                Update(tagDB, root);
            }
        }
        public void Update(ITagDB tagDB,string root)
        {
            this.tagDB = tagDB;
            this.root = root;
            
            canvas.Children.Clear();
            ITagLayout tagLayout = TagLayoutFactory.CreateLayout();
            tagLayout.Layout(tagDB, root);
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
                t.ContextMenu = TagAreaMenu;
                t.MouseLeftButtonDown += T_MouseLeftButtonDown;
                t.MouseDoubleClick += T_MouseDoubleClick;
                canvas.Children.Add(t);
            }
            SetCurrentTag(root);
        }

        private void T_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            TagBox b = sender as TagBox;
            if(b!=null)
                Update(b.Text);
        }

        private void T_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if(sender is TagBox)
            {
                SetCurrentTag((sender as TagBox).Text);
            }
        }

        private void scrollViewer_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (e.OriginalSource is TagBox)
            {
                TagBox b = (TagBox)e.OriginalSource;
                Update(b.Text);
            }
        }

        private void canvas_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {

        }

        private void canvas_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {

        }

        private void canvas_Click(object sender, RoutedEventArgs e)
        {
            
        }
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
        Border border = null;
        private void SetBorder(string tag)
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
            
            //TextBlock tagBlock = null;

            //foreach(UIElement u in canvas.Children)
            //{
            //    if(u is TextBlock)
            //    {
            //        TextBlock t = u as TextBlock;
            //        if (t != null && t.Text == tag)
            //        {
            //            tagBlock = t;
            //        }
            //    }
                
            //}

            //if (tagBlock != null)
            //{
            //    TextBlock old = null;
            //    if (border == null)
            //    {
            //        border = new Border();
            //        border.BorderBrush = new SolidColorBrush(Colors.Black);
            //        border.BorderThickness = new Thickness(2);

            //    }
            //    else
            //    {
            //        old = border.Child as TextBlock;
            //        old.Margin = new Thickness(border.Margin.Left + 2, border.Margin.Top + 2, 0, 0);
            //        canvas.Children.Remove(border);
            //        border.Child = null;
            //        canvas.Children.Add(old);
            //    }

            //    canvas.Children.Remove(tagBlock);
            //    border.Width = tagBlock.Width + 4;
            //    border.Height = tagBlock.Height + 4;
            //    border.Margin = new Thickness(tagBlock.Margin.Left-2, tagBlock.Margin.Top-2, 0,0);
            //    tagBlock.Margin = new Thickness();
            //    border.Child = tagBlock;
            //    canvas.Children.Add(border);

            //}
        }
        private string currentTag = "";
        public void SetCurrentTag(string tag)
        {
            SetBorder(tag); //这一句必须放在下面检查并return之前，
                            //即无论currentTag是否变化，都需要更新一下border，否则会有bug；
                            //bug现象：在curtag没有变化的时候，重新绘制整个graph，
                            //会出现所有的tag都不显示边框（包括curtag），因为直接返回了。
            
            //if (currentTag == tag) return;
            string oldTag = currentTag;
            currentTag = tag;


            ShowCurrentTagInf();
            if(SelectedTagChanged!=null)
            {
                SelectedTagChanged(tag);
            }
        }
        private void ShowCurrentTagInf()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(currentTag);
            
            List<string> parents = tagDB.QueryTagParent(currentTag);
            if(parents.Count>0)
            {
                sb.Append(" Parent::= ");
                foreach (string s in parents) sb.Append(" " + s);
            }


            List<string> children = tagDB.QueryTagChildren(currentTag);
            if (children.Count > 0)
            {
                sb.Append(" Children::= ");
                foreach (string s in children) sb.Append(" " + s);
            }

            CurrentTagInf.Text = sb.ToString().Trim();
        }
        public delegate void CurrentTagChanged(string tag);
        public CurrentTagChanged SelectedTagChanged = null;

        public void miPaste_Click(object sender, RoutedEventArgs e)
        {
            UpdateCurrentTagByContextMenu();
            string[] token = Clipboard.GetText().Split(new char[] { '`' }, StringSplitOptions.RemoveEmptyEntries);
            
            if(token.Length==2)
            {
                string arg = token[1];
                string[] args = arg.Split(new char[] { '?' }, StringSplitOptions.RemoveEmptyEntries);
                switch (token[0])
                {
                    case KUMMERWU_TAG_COPY:
                        tagDB.AddTag(currentTag, arg); 
                        Update();
                        break;
                    case KUMMERWU_TAG_CUT:
                        tagDB.SetRelation(currentTag, arg); 
                        Update();
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

        //public class ClipOperation
        //{
        //    public string Tag;
        //    public enum OPT
        //    {
        //        Cut,Copy,Sel
        //    }
        //    public OPT opt = OPT.Sel;
        //    public static ClipOperation Parse(string txt)
        //    {
        //        string[] tokens = txt.Split('`');
        //        if(tokens.Length==3 && tokens[0]=="KUMMERWU")
        //        {
        //            ClipOperation opt = new ClipOperation();
        //            opt.Tag = tokens[1];
        //            opt.opt = (OPT)(int.Parse(tokens[2]));
        //            return opt;
        //        }
        //        return null;
        //    }
        //    public override string ToString()
        //    {
        //        return "KUMMERWU`" + Tag + "`" + ((int)(opt)).ToString();
        //    }
        //}
        public void miCopy_Click(object sender, RoutedEventArgs e)
        {
            UpdateCurrentTagByContextMenu();
            Clipboard.SetText(KUMMERWU_TAG_COPY + "`" + currentTag);
        }

        public void miCut_Click(object sender, RoutedEventArgs e)
        {
            UpdateCurrentTagByContextMenu();
            Clipboard.SetText(KUMMERWU_TAG_CUT+"`" + currentTag);
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
            Update("我的大脑");
        }
    }
}
