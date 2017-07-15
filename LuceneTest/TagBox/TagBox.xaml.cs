using AnyTagNet;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace LuceneTest
{
    /// <summary>
    /// 在标签有向图中，显示一个标签的空间（封装了一个textblock和一个border，border是为了显示状态信息）
    /// </summary>
    public partial class TagBox : UserControl
    {
        public TagBox()
        {
            InitializeComponent();
        }
        bool sel = false;
        public bool Selected
        {
            get
            {
                return sel;
            }
            set
            {
                sel = value;
                bdr.BorderThickness = new Thickness(sel?2:0);
            }
        }

        public string Text
        {
            get {return txt.Text; }
            internal set {txt.Text = value; }
        }

        public TextAlignment TextAlignment
        {
            get { return txt.TextAlignment; }
            internal set { txt.TextAlignment = value; }
        }

        public SolidColorBrush Background1
        {
            set
            {
                bdr.Background = value;
            }
        }
        
        public double Width1
        {
            set
            {
                bdr.Width = value;
                txt.Width = value - GConfig.XContentPadding/2;
            }
        }
        public double Height1
        {
            set
            {
                bdr.Height = value;
                txt.Height = value - GConfig.YContentPadding/2;
            }
        }
    }
}
