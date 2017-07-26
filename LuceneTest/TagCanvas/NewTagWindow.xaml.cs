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

namespace TagExplorer.TagCanvas
{
    /// <summary>
    /// NewTagWindow.xaml 的交互逻辑
    /// </summary>
    public partial class NewTagWindow : Window
    {
        public NewTagWindow()
        {
            InitializeComponent();
            txtInput.Focus();
        }
        public string Tips
        {
            set
            {
                inf.Content = value;
            }
        }
        private string inputValue = null;
        public string Inputs
        {
            get
            {
                return inputValue;
            }
        }

        
        private void btOK_Click(object sender, RoutedEventArgs e)
        {
            inputValue = txtInput.Text;
            Close();
        }
    }
}
