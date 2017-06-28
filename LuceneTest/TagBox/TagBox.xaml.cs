using System.Windows;
using System.Windows.Controls;

namespace LuceneTest
{
    /// <summary>
    /// TagBox.xaml 的交互逻辑
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

        public string Text { get {return txt.Text; } internal set {txt.Text = value; } }

        public TextAlignment TextAlignment
        { get { return txt.TextAlignment; }
            internal set { txt.TextAlignment = value; }
        }
    }
}
