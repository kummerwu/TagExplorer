using System.Windows;

namespace TagExplorer.TagCanvas
{
    /// <summary>
    /// NewTagWindow.xaml 的交互逻辑，新建标签嵌入式编辑后，该类废弃
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
