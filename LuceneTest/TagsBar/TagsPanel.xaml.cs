using LuceneTest.TagMgr;
using LuceneTest.UriMgr;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace LuceneTest.TagsBar
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
        private IUriDB db = null;
        private ITagDB tagsDB = null;
        private string curUri = null;
        public void UpdateUri(string uri,IUriDB db,ITagDB tagsDB)
        {
            this.db = db;
            this.tagsDB = tagsDB;
            autoTextBox.Search = tagsDB;
            curUri = uri;
            List<string> tags = db.GetTags(uri);
            parent.Children.Clear();
            foreach (string tag in tags)
            {
                AddTag(tag);
            }
        }
        private void AddTag(string tag)
        {
            TextBlock t = new TextBlock();
            t.Width = 60;
            t.Text = tag;
            t.Style = Resources["tagMouseOver"] as Style;
            
            parent.Children.Add(t);
        }
       

        private void autoTextBox_KeyUp(object sender, KeyEventArgs e)
        {
            if(e.Key == Key.Enter && curUri!=null && db!=null && tagsDB!=null)
            {
                db.AddUri(curUri, new List<string>() { autoTextBox.Text.Trim() });
                UpdateUri(curUri, db, tagsDB);
                autoTextBox.Text = "";
            }
        }

    }
}
