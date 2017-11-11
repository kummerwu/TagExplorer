
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
using TagExplorer.Utils;

namespace TagExplorer.AutoComplete
{
    /// <summary>
    /// AutoCompleteCB.xaml 的交互逻辑
    /// </summary>
    public partial class AutoCompleteCB : UserControl
    {
        public AutoCompleteCB()
        {
            InitializeComponent();
            comboBoxStateChanged();
            //最小输入提示，小于2个字符，不给出提示
            searchThreshold = 2;        // default threshold to 2 char

            // 提示延时定时器
            keypressTimer = new System.Timers.Timer();
            keypressTimer.Elapsed += new System.Timers.ElapsedEventHandler(OnTimedEvent);
        }

        #region 私有变量
        //private VisualCollection controls;
        //public TextBox textBox;
        //private ComboBox comboBox;
        //private ObservableCollection<AutoCompleteEntry> autoCompletionList;
        private System.Timers.Timer keypressTimer;
        private delegate void TextChangedCallback();
        private bool insertText;
        private int delayTime = 100;        //延迟提示的时延（100ms）
        private int searchThreshold;        //至少输入几个字符后，开始自动提示
        ISearchDataProvider search;
        public ISearchDataProvider SearchDataProvider
        {
            set { search = value; }
        }
        #endregion

        //键盘操作的劫持，在ComboBox处于Open状态时，需要对键盘做一些特殊的相应
        //主要是UP、Down键盘操作转换为ComboBox不同项的选择操作
        //protected override void OnPreviewKeyDown(KeyEventArgs e)
        //{
        //    TextBox_KeyDown(null, e);//如果ComboBox处于Open状态，
        //                             //此时就不要再进行编辑了，而是将键盘操作映射到ComboBox的操作上去
        //    base.OnPreviewKeyDown(e);
        //}
        private void TextBox_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if(!comboBox.IsDropDownOpen)
            {
                if(e.Key == Key.Down)
                {
                    e.Handled = true;
                    TextChanged();
                }
            }
            TextBox_KeyDown(sender, e);
        }


        private void ComboBox_KeyDown(object sender, KeyEventArgs e)
        {
            TextBox_KeyDown(sender, e);
        }
        //在ComboBox处于Open状态时，劫持键盘操作
        private void TextBox_KeyDown(object sender, KeyEventArgs e)
        {

            if (!comboBox.IsDropDownOpen) return;

            //如果按了向下键,则把选中项下移
            if (e.Key == Key.Down)
            {
                System.Diagnostics.Debug.WriteLine(DateTime.Now.ToLongTimeString() + " DOWN");
                if (comboBox.SelectedIndex < 0)
                {
                    comboBox.SelectedIndex = 0;
                }
                else if (comboBox.SelectedIndex < comboBox.Items.Count - 1)
                {
                    comboBox.SelectedIndex++;
                    //comboBox.scr(lbUser.Items.CurrentItem);
                }
                e.Handled = true;
            }
            //如果按了向上键,则把选中项上移
            if (e.Key == Key.Up)
            {
                if (comboBox.SelectedIndex < 0) comboBox.SelectedIndex = 0;
                else if (comboBox.SelectedIndex == 0)
                {
                    comboBox.SelectedIndex = comboBox.Items.Count - 1;
                }
                if (comboBox.SelectedIndex > 0)
                {
                    comboBox.SelectedIndex--;
                    //lbUser.ScrollIntoView(lbUser.Items.CurrentItem);
                }
                e.Handled = true;
            }
        }
        private void ComboBox_DropDownClosed(object sender, EventArgs e)
        {
            if (comboBox.SelectedIndex != -1)
            {
                ComboBoxItem cbItem = (ComboBoxItem)comboBox.SelectedItem;
                SetCurrentToken(cbItem.Tag as AutoCompleteTipsItem);
                textBox.Focus();
            }
            comboBox.Width = 0;
        }
        public new void Focus()
        {
            bool ret = textBox.Focus();
            textBox.SelectAll();
        }

        /***********************************************************
         * 有可能TextBox中输入的不是一个光秃秃的Tag，而是一个复杂的表达式
         * 这个时候需要用#隔开每一个TAG
         ***********************************************************/
        private string GetCurrentToken()
        {
            int s = textBox.SelectionStart;
            int e = textBox.SelectionStart + textBox.SelectionLength;
            int first = textBox.Text.LastIndexOf('#', e - 1);
            string matchToken = textBox.Text;
            if (first != -1 && e > first + 1)
            {
                matchToken = textBox.Text.Substring(first + 1, e - first - 1);
            }
            return matchToken;
        }

        private AutoCompleteTipsItem item = null;
        public AutoCompleteTipsItem Item {
            get
            {
                if(item==null)
                {
                    List < AutoCompleteTipsItem > words = search.QueryAutoComplete(Text,true);
                    if (words.Count > 0) item = words[0];
                }
                return item;
            }
            private set { item = value;  } }
        private void SetCurrentToken(AutoCompleteTipsItem t)
        {
            Item = t;
            int s = textBox.SelectionStart;
            int e = textBox.SelectionStart + textBox.SelectionLength;
            int first = -1;
            if (e > 1)
            {
                first = textBox.Text.LastIndexOf('#', e - 1);
            }
            if (first == -1)
            {
                textBox.Text = t.Content;
            }
            else
            {
                textBox.Text = textBox.Text.Substring(0, first + 1) + t;
            }
            textBox.Select(textBox.Text.Length, 0);
        }




        #region Methods
        public string Text
        {
            get { return textBox.Text.Replace("#", ""); }
            set
            {
                insertText = true;
                textBox.Text = value;
            }
        }

        public int DelayTime
        {
            get { return delayTime; }
            set { delayTime = value; }
        }

        public int Threshold
        {
            get { return searchThreshold; }
            set { searchThreshold = value; }
        }

        //public void AddItem(AutoCompleteEntry entry)
        //{
        //    autoCompletionList.Add(entry);
        //}

        private void comboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (null != comboBox.SelectedItem)
            {
                insertText = true;
                ComboBoxItem cbItem = (ComboBoxItem)comboBox.SelectedItem;
                //textBox.Text = cbItem.Content.ToString();
                SetCurrentToken(cbItem.Tag as AutoCompleteTipsItem);
            }
        }
        private void comboBoxStateChanged()
        {
            comboBox.Width = comboBox.IsDropDownOpen ? textBox.Width : 0;
        }
        //在文本发生变化时，及时显示ComboBox提示信息
        private void TextChanged()
        {
            try
            {
                comboBox.Items.Clear();
                string matchToken = GetCurrentToken();

                if (matchToken.Length >= searchThreshold)
                {
                    List<AutoCompleteTipsItem> words = search.QueryAutoComplete(matchToken);
                    foreach (AutoCompleteTipsItem w in words)
                    {
                        ComboBoxItem cbItem = new ComboBoxItem();
                        cbItem.Content = w.Tip;
                        cbItem.Tag = w;
                        comboBox.Items.Add(cbItem);
                    }
                    comboBox.IsDropDownOpen = comboBox.HasItems;
                }
                else
                {
                    comboBox.IsDropDownOpen = false;
                }
                comboBoxStateChanged();
            }
            catch(Exception ee) { Logger.E(ee); }
        }

        private void OnTimedEvent(object source, System.Timers.ElapsedEventArgs e)
        {
            keypressTimer.Stop();
            Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Normal,
                new TextChangedCallback(this.TextChanged));
        }

        private void textBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            // text was not typed, do nothing and consume the flag
            if (insertText == true) insertText = false;

            // if the delay time is set, delay handling of text changed
            else
            {
                if (delayTime > 0)
                {
                    keypressTimer.Interval = delayTime;
                    keypressTimer.Start();
                }
                else TextChanged();
            }
        }

        //protected override Size ArrangeOverride(Size arrangeSize)
        //{
        //    textBox.Arrange(new Rect(arrangeSize));
        //    comboBox.Arrange(new Rect(arrangeSize));
        //    return base.ArrangeOverride(arrangeSize);
        //}

        //protected override Visual GetVisualChild(int index)
        //{
        //    return controls[index];
        //}

        //protected override int VisualChildrenCount
        //{
        //    get { return controls.Count; }
        //}
        #endregion

    }
}
