using TagExplorer.TagMgr;
using TagExplorer.UriMgr;
using System.Collections.Generic;
using System.Windows.Controls;
using System.Windows.Input;
using TagExplorer.Utils;

namespace TagExplorer.TagsBar
{
    /// <summary>
    /// TagsPanel.xaml 的交互逻辑
    /// </summary>
    public partial class TagsPanel : UserControl
    {
        public TagsPanel()
        {
            InitializeComponent();
        }
        //私有变量///////////////////////////////////////////////
        private IUriDB uriDB = null;
        private ITagDB tagDB = null;
        private string currentUri = null;

        //公有方法///////////////////////////////////////////////
        public void ChangeCurrentUri(string uri,IUriDB uriDB,ITagDB tagDB)
        {
            string tips = "当前选中文件："+uri+" ";
            this.uriDB = uriDB;
            this.tagDB = tagDB;
            autoTextBox.SearchDataProvider = tagDB;
            currentUri = uri;
            List<string> tags = uriDB.GetTags(uri);
            parent.Children.Clear();
            foreach (string tag in tags)
            {
                ShowTagsList(tag);
                tips += tag + " ";
            }

            TipsCenter.Ins.UriInf = tips;
        }

        //私有方法///////////////////////////////////////////////
        private void ShowTagsList(string tag)
        {
            TextBlock t = new TextBlock();
            t.Text = tag;
            
            parent.Children.Add(t);
        }


        private void autoTextBox_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter && currentUri != null && uriDB != null && tagDB != null)
            {
                uriDB.AddUri(new List<string>(){ currentUri}, new List<string>() { autoTextBox.Text.Trim() });
                ChangeCurrentUri(currentUri, uriDB, tagDB);
                autoTextBox.Text = "";
            }
        }

    }
}
