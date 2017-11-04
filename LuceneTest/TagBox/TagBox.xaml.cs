using AnyTagNet;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using TagExplorer.TagLayout.LayoutCommon;
using TagExplorer.TagMgr;
using TagExplorer.UriMgr;
using TagExplorer.Utils;

namespace TagExplorer
{
    /// <summary>
    /// 在标签有向图中，显示一个标签的控件（封装了一个textblock和一个border，border是为了显示状态信息）
    /// </summary>
    public partial class TagBox : UserControl
    {
        GTagBox tagInf;
        public TagBox(GTagBox tagInf)
        {
            this.tagInf = tagInf;
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
        private GUTag tagID;
        public GUTag GUTag
        {
            get
            {
                return tagID;
            }
            internal set
            {
                tagID = value;
                txt.Text = value.Title;
            }
        }
        public string Text
        {
            get { return txt.Text; }
        }
        public TextAlignment TextAlignment
        {
            get { return txt.TextAlignment; }
            internal set { txt.TextAlignment = value; }
        }

        public int Background1
        {
            set
            {
                //确定自己的颜色
                SolidColorBrush b = new SolidColorBrush(UIElementFactory.GetColor(value, value));
                bdr.Background = b;

                //确定小圆圈的颜色
                int childCount = TagDBFactory.Ins == null ? 10 : TagDBFactory.Ins.QueryChildrenCount(GUTag);
                
                if(childCount==0)//无child，不显示小圆圈
                {
                    circleLeft.Background = null;
                    circle.Background = null;
                }
                else if(tagInf.IsRoot) //根节点，可能显示两个小圆圈
                {
                    circle.Background = new SolidColorBrush(UIElementFactory.GetColor(value + 1, value + 1));
                    if(childCount>2)
                    {
                        circleLeft.Background = circle.Background;
                    }
                    else
                    {
                        circleLeft.Background = null;
                    }
                    
                }
                else if (tagInf.Direct==1) //否则，显示一个小圆圈
                {
                    circleLeft.Background = null;
                    circle.Background = new SolidColorBrush(UIElementFactory.GetColor(value + 1, value + 1));
                    
                }
                else if(tagInf.Direct==-1)
                {
                    circle.Background = null;
                    circleLeft.Background = new SolidColorBrush(UIElementFactory.GetColor(value + 1, value + 1));
                    
                }
                
                
            }
        }
        public int Foreground1
        {
            set
            {
                txt.Foreground= new SolidColorBrush(UIElementFactory.GetForeColor(value,value));
            }
        }
        
        public double Width1
        {
            set
            {
                bdr.Width = value;
                txt.Width = value - StaticCfg.Ins.XContentPadding/2;
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
                txt.Height = value - StaticCfg.Ins.YContentPadding/2;
            }
            get
            {
                return bdr.Height;
            }
        }

        private void ExpandSwitchButton_Click(object sender, RoutedEventArgs e)
        {
            TagSwitchDB.Ins.Swtich(GUTag);
        }
    }
}
