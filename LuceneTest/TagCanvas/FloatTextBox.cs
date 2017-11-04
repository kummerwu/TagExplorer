using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using TagExplorer.TagMgr;

namespace TagExplorer.TagCanvas
{
    class FloatTextBox
    {
        public delegate void TextChanged(Canvas Parent,GUTag tag,string NewString);
        public TextChanged TextChangedCallback;


        private TextBox Edit = null;
        private Canvas Parent = null;
        private TagBox NoEdit = null;
        private static FloatTextBox ins = null;
        public static FloatTextBox Ins
        {
            get
            {
                if (ins == null)
                {
                    ins = new FloatTextBox();
                    ins.InitEdit();
                }
                return ins;
            }
        }
        public bool IsVisible
        {
            get
            {
                return Parent != null && NoEdit != null;
            }
        }
        private void HideEdit()
        {
            if (Parent == null || Edit == null || NoEdit == null) return;


            try
            {
                Parent.Children.Remove(Edit);
                System.Diagnostics.Debug.WriteLine("HideFloat1");
                string oldTitle = NoEdit.Text;
                string newTitle = Edit.Text;
                if (oldTitle != newTitle && oldTitle != null && newTitle != null)
                {
                    
                    TextChangedCallback?.Invoke(Parent, NoEdit.GUTag, newTitle);
                    System.Diagnostics.Debug.WriteLine("HideFloat2");
                }
            }
            finally
            {
                Canvas tmp = Parent;
                NoEdit = null;
                Parent = null;
                tmp?.Focus();
            }
        }
        private void InitEdit()
        {
            Edit = new TextBox();
            Edit.BorderThickness = new Thickness(0);
            Edit.LostFocus += Edit_LostFocus;
        
            Edit.KeyDown += Edit_KeyDown;
            
        }

        //改为KeyDown事件，如果使用KeyUp事件，会导致与输入法的回车选择冲突。
        private void Edit_KeyDown(object sender, KeyEventArgs e)
        {
            if ((e.Key == Key.Enter) && (e.KeyboardDevice.Modifiers & ModifierKeys.Control) != ModifierKeys.Control)
            {
                HideEdit();
                e.Handled = true;
            }
        }

        

        private void Edit_LostFocus(object sender, RoutedEventArgs e)
        {
            HideEdit();
        }
        public void ShowEdit(Canvas c, TagBox brother)
        {
            if (c == null || brother == null) return;

            InitEdit();
            HideEdit();
            Parent = c;
            NoEdit = brother;
            Edit.Text = brother.Text;
            Edit.Width = Math.Max(500, brother.Width + 10);
            Thickness m = brother.Margin;
            Edit.Margin = new Thickness(m.Left + 20, m.Top + 5, 0, 0);
            Edit.FontFamily = brother.txt.FontFamily;
            Edit.FontSize = brother.txt.FontSize;
            Edit.FontStretch = brother.txt.FontStretch;
            Edit.FontStyle = brother.txt.FontStyle;
            Parent.Children.Add(Edit);
            Edit.Focus();
            Edit.SelectAll();
        }
    }
}
