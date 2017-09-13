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
using System.Windows.Shapes;

namespace TagExplorer.SearchResultListBox
{
    /// <summary>
    /// InputBoxWindow.xaml 的交互逻辑
    /// </summary>
    public partial class InputBoxWindow : Window
    {
        public InputBoxWindow()
        {
            InitializeComponent();
        }

        private void OKBt_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void CancelBt_Click(object sender, RoutedEventArgs e)
        {
            Input.Text = "";
            Close();
        }
    }
}
