using AnyTag.BL;
using AnyTags.Net;
using LuceneTest.TagLayout;
using LuceneTest.TagMgr;
using LuceneTest.UriMgr;
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

namespace LuceneTest.TagGraph
{
    /// <summary>
    /// TagCanvas.xaml 的交互逻辑
    /// </summary>
    public partial class TagCanvas : UserControl
    {
        public TagCanvas()
        {
            InitializeComponent();
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
        private void SetHeight() { canvas.Height = Math.Max(realHeight, canvasMinHeight); }
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
            SetCurrentTag(root);
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
            foreach (TextBlock t in allTxt)
            {
                t.ContextMenu = TagAreaMenu;
                t.MouseLeftButtonDown += T_MouseLeftButtonDown;
                canvas.Children.Add(t);
            }
        }

        private void T_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if(sender is TextBlock)
            {
                SetCurrentTag((sender as TextBlock).Text);
            }
        }

        private void scrollViewer_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (e.OriginalSource is TextBlock)
            {
                TextBlock b = (TextBlock)e.OriginalSource;
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
        private string currentTag = "";
        public void SetCurrentTag(string tag)
        {
            if (currentTag == tag) return;

            currentTag = tag;
            CurrentTagInf.Text = currentTag;
            if(TagChangedHandler!=null)
            {
                TagChangedHandler(tag);
            }
        }

        public delegate void CurrentTagChanged(string tag);
        public CurrentTagChanged TagChangedHandler = null;

        private void miPaste_Click(object sender, RoutedEventArgs e)
        {
            Paste();
        }
        public void Paste()
        {
            UpdateCurrentTagByContextMenu();
            AddUri(FileOperator.GetFileListFromClipboard());
        }
        private void UpdateCurrentTagByContextMenu()
        {
            TextBlock t = TagAreaMenu.PlacementTarget as TextBlock;
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
                UriDB.AddUri(f, tags);
            }
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
    }
}
