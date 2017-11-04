﻿using System;
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
        //公有事件*************************************
        public delegate void TextChanged(Canvas Parent,GUTag tag,string NewString);
        public TextChanged TextChangedCallback;

        //私有成员*****************************************
        private TextBox Edit = null;        //用于编辑的TextBox
        private Canvas Parent = null;       //该编辑TextBox所在的Canvas
        private TagBox NoEdit = null;       //该编辑TextBox对应的TagBox（标签显示框）

        //浮动TextBox是一个单实例控件
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
        //当前编辑框是否正在显示中
        public bool IsVisible
        {
            get
            {
                return Parent != null && NoEdit != null;
            }
        }
        private void HideEdit()
        {
            if (!IsVisible) return;


            try
            {
                //隐藏编辑框
                Parent.Children.Remove(Edit);
                System.Diagnostics.Debug.WriteLine("HideFloat1");

                //如果文本内容发生修改，通知观察者
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
                tmp?.Focus();   //因为编辑框失去焦点时会有相应的处理，所以这儿设置焦点的处理必须放在所有处理完成之后
                                //否则可能形成递归调用
            }
        }

        //初始化编辑框（仅仅初始化，并不显示编辑框）
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
            //编辑框中输入回车，表示输入结束，关闭编辑框并通知变更
            if ( IsVisible &&
                (e.Key == Key.Enter) && 
                (e.KeyboardDevice.Modifiers & ModifierKeys.Control) != ModifierKeys.Control)
            {
                HideEdit();
                e.Handled = true;
            }
        }
        //编辑框失去焦点，表示输入结束，关闭编辑框并通知变更
        private void Edit_LostFocus(object sender, RoutedEventArgs e)
        {
            HideEdit();
        }

        //显示编辑框
        public void ShowEdit(Canvas c, TagBox brother)
        {
            if (c == null || brother == null) return;
            
            HideEdit();
            Parent = c;
            NoEdit = brother;
            //设置好编辑框的各种属性
            Edit.Text = brother.Text;
            Edit.Width = Math.Max(500, brother.Width + 10);
            Thickness m = brother.Margin;
            Edit.Margin = new Thickness(m.Left + 20, m.Top + 5, 0, 0);
            Edit.FontFamily = brother.txt.FontFamily;
            Edit.FontSize = brother.txt.FontSize;
            Edit.FontStretch = brother.txt.FontStretch;
            Edit.FontStyle = brother.txt.FontStyle;
            //显示该编辑框
            Parent.Children.Add(Edit);
            Edit.Focus();
            Edit.SelectAll();
        }
    }
}
