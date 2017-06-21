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
