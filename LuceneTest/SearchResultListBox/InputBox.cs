
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using TagExplorer.Utils;

namespace TagExplorer.SearchResultListBox
{
    public class InputBox
    {

        InputBoxWindow Box = new InputBoxWindow();
        
        public InputBox(string content,string defaultTxt)
        {
            try
            {
                Box.Tips.Content = content;
                Box.Input.Text = defaultTxt;

                Box.Input.Focus();
            }
            catch(Exception e) {
                Logger.E(e);
            }
            
        }
        
        public string ShowDialog()
        {
            Box.ShowDialog();
            return Box.Input.Text;
        }


    }
}
