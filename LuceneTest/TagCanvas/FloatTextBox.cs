﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace TagExplorer.TagCanvas
{
    class FloatTextBox
    {
        public delegate void TextChanged(Canvas Parent,TagBox box,string NewString);
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

        private void HideEdit()
        {
            if (Parent != null )
            {
                if (Edit.Text != NoEdit.Text)
                {
                    TextChangedCallback?.Invoke(Parent, NoEdit, Edit.Text);
                }
                Parent.Children.Remove(Edit);
                Parent = null;
                NoEdit = null;
            }
        }
        private void InitEdit()
        {
            Edit = new TextBox();
            Edit.BorderThickness = new Thickness(0);
            Edit.LostFocus += Edit_LostFocus;
            Edit.KeyUp += Edit_KeyUp; 
            
        }

        private void Edit_KeyUp(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if((e.Key == Key.Enter || e.Key == Key.Return))
            {
                HideEdit();
            }
        }

        private void Edit_LostFocus(object sender, RoutedEventArgs e)
        {
            HideEdit();
        }
        public void ShowEdit(Canvas c, TagBox brother)
        {
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
