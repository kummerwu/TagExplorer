using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using TagExplorer.TagMgr;
using TagExplorer.UriMgr;
using TagExplorer.Utils;

namespace TagExplorer
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
        public enum Status
        {
            None,Selected,Copy,Cut
        }
        Status stat = Status.None;
        public Status Stat
        {
            get
            {
                return stat;
            }
            set
            {
                stat = value;
                switch(stat)
                {
                    case Status.None:
                        bdr.BorderThickness = new Thickness(0);
                        txt.Opacity = 1;
                        break;
                    case Status.Selected:
                        bdr.BorderThickness = new Thickness(2);
                        bdr.BorderBrush = new SolidColorBrush(Colors.Black);
                        txt.Opacity = 1;
                        break;
                    case Status.Cut:
                        bdr.BorderThickness = new Thickness(2);
                        bdr.BorderBrush = new SolidColorBrush(Colors.Green);
                        txt.Opacity = 0.3;
                        break;
                    case Status.Copy:
                        bdr.BorderThickness = new Thickness(2);
                        bdr.BorderBrush = new SolidColorBrush(Colors.Red);
                        txt.Opacity = 1;
                        break;
                }
                
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
                if(TagDBFactory.Ins!=null && TagDBFactory.Ins.GetTagChildrenCount(Text)==0)
                {
                    circle.Background = null;
                }
                else circle.Background = value;
            }
        }
        public SolidColorBrush Foreground1
        {
            set
            {
                txt.Foreground= value;
            }
        }
        
        public double Width1
        {
            set
            {
                bdr.Width = value;
                txt.Width = value - CfgTagGraph.XContentPadding/2;
            }
            get
            {
                return bdr.Width;
            }
        }
        public double Height1
        {
            set
            {
                bdr.Height = value;
                txt.Height = value - CfgTagGraph.YContentPadding/2;
            }
            get
            {
                return bdr.Height;
            }
        }

        private void ExpandSwitchButton_Click(object sender, RoutedEventArgs e)
        {
            TagSwitchDB.Ins.Swtich(Text);
        }
    }
}
