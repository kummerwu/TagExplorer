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

namespace LuceneTest.TagsBar
{
    /// <summary>
    /// TagsPanel.xaml 的交互逻辑
    /// </summary>
    public partial class TagsPanel : UserControl
    {
        public TagsPanel()
        {
            InitializeComponent();
        }
        public void AddTag(string tag)
        {
            TextBlock t = new TextBlock();
            t.Width = 60;
            t.Text = tag;
            parent.Children.Add(t);
        }
    }
}
