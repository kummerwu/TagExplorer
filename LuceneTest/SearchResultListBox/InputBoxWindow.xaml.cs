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
using TagExplorer.Utils;

namespace TagExplorer.SearchResultListBox
{
    /// <summary>
    /// 一个通用对话框，用于获取用户的文本输入
    /// </summary>
    public partial class InputBoxWindow : Window
    {
        //公有成员变量************************************************************
        //私有成员变量************************************************************
        //公有成员方法************************************************************
        public static string ShowDlg(string Title, string Question, string DefaultTxt)
        {
            InputBoxWindow Box = new InputBoxWindow();
            try
            {
                Box.Title = Title;
                Box.Tips.Content = Question;
                Box.Input.Text = DefaultTxt;
                Box.Input.SelectAll();
                Box.Input.Focus();
                Box.ShowDialog();
                return Box.Input.Text;
            }
            catch (Exception e)
            {
                Logger.E(e);
                return "";
            }
        }

        //私有成员方法************************************************************
        private InputBoxWindow()
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
