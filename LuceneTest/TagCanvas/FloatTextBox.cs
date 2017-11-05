using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using TagExplorer.TagMgr;
using TagExplorer.Utils;

namespace TagExplorer.TagCanvas
{
    class FloatTextBox
    {
        #region 公共事件：编辑完成后的TextChanged事件
        //公有事件*************************************
        public delegate void TextChanged(Canvas Parent,GUTag tag,string NewString);
        public TextChanged TextChangedCallback;
        #endregion

        #region 私有成员变量
        //私有成员*****************************************
        private TextBox Edit = null;        //用于编辑的TextBox
        private Canvas Parent = null;       //该编辑TextBox所在的Canvas
        private TagBox NoEdit = null;       //该编辑TextBox对应的TagBox（标签显示框）
        #endregion

        #region 初始化编辑框
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
        //初始化编辑框（仅仅初始化，并不显示编辑框）
        private void InitEdit()
        {
            Edit = new TextBox();
            Edit.BorderThickness = new Thickness(0);
            Edit.LostFocus += Edit_LostFocus;
            Edit.KeyDown += Edit_KeyDown;

        }
        #endregion

        #region 显示编辑框
        //显示编辑框
        public void ShowEdit(Canvas c, TagBox brother)
        {
            if (c == null || brother == null) return;

            ComplateEdit();
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
        //当前编辑框是否正在显示中
        public bool IsVisible
        {
            get
            {
                return Parent != null && NoEdit != null;
            }
        }
        #endregion

        #region 关闭编辑框
        private void CancelEdit()
        {
            if(Parent!=null)
            {
                Parent.Children.Remove(Edit);
            }
            Parent = null;
            NoEdit = null;
        }
        private void ComplateEdit()
        {
            if (!IsVisible) return;

            //合法性检查
            string err = CfgPath.CheckTagFormat(Edit.Text);
            if (err != null)
            {
                MessageBox.Show(err, "标签不合法，请重新输入", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }


            //第一步，先将编辑器隐藏起来，否则后面有TextChangedCallback导致业务流程处理后，
            //会有很多意想不到的流程发生，导致多次递归调用本函数(HideEdit)。
            //比如：HideEdit=>lostfocus=>HideEdit
            GUTag tag = NoEdit.GUTag;
            string oldTitle = NoEdit.Text;
            string newTitle = Edit.Text;
            Canvas parentBak = Parent;

            


            //将这两个置空，表示该编辑器不可见了（IsVisible = false）。
            Parent = null;
            NoEdit = null;

            try
            {
                //隐藏编辑框
                parentBak.Children.Remove(Edit);
                System.Diagnostics.Debug.WriteLine("HideFloat1");

                //如果文本内容发生修改，通知观察者
                
                if (oldTitle != newTitle && oldTitle != null && newTitle != null)
                {
                    TextChangedCallback?.Invoke(parentBak, tag, newTitle);
                    System.Diagnostics.Debug.WriteLine("HideFloat2");
                }
            }
            finally
            {
                parentBak?.Focus();   //因为编辑框失去焦点时会有相应的处理，所以这儿设置焦点的处理必须放在所有处理完成之后
                                //否则可能形成递归调用
            }
        }
        //改为KeyDown事件，如果使用KeyUp事件，会导致与输入法的回车选择冲突。
        private void Edit_KeyDown(object sender, KeyEventArgs e)
        {
            //编辑框中输入回车，表示输入结束，关闭编辑框并通知变更
            if (IsVisible &&
                (e.Key == Key.Enter) &&
                (e.KeyboardDevice.Modifiers & ModifierKeys.Control) != ModifierKeys.Control)
            {
                ComplateEdit();
                e.Handled = true;
            }
        }
        //编辑框失去焦点，表示输入结束，关闭编辑框并通知变更
        private void Edit_LostFocus(object sender, RoutedEventArgs e)
        {
            string err = CfgPath.CheckTagFormat(Edit.Text);
            if (err != null)
            {
                CancelEdit();
                MessageBox.Show(err, "标签不合法，请重新输入", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            else
            {
                ComplateEdit();
            }
        }
        #endregion


        

        

        
    }
}
