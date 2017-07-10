using AnyTag.BL;
using AnyTagNet;
using LuceneTest.TagGraph;
using LuceneTest.TagMgr;
using LuceneTest.UriMgr;
using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Imaging;

namespace LuceneTest.UriInfList
{
    /// <summary>
    /// UriInfoPanel.xaml 的交互逻辑
    /// </summary>
    public partial class UriListBox : UserControl
    {
        public UriListBox()
        {
            InitializeComponent();
            
        }
        private IUriDB uriDB = null;
        private ITagDB tagsDB = null;
        public void UpdateResult(string query,IUriDB uriDB,ITagDB tagsDB)
        {
            this.uriDB = uriDB;
            this.tagsDB = tagsDB;
            lst.ItemsSource = SearchItemInf.GetFilesByTag(query, uriDB);
            if (lst.Items.Count > 0)
            {
                lst.SelectedIndex = 0;
                
            }
            UpdateCurrentUriByContextMenu();

            //自动调整列的宽度
            GridView gv = lst.View as GridView;
            if (gv != null)
            {
                foreach (GridViewColumn gvc in gv.Columns)
                {
                    gvc.Width = gvc.ActualWidth;
                    gvc.Width = Double.NaN;
                }
            }
        }
        

        private void lst_MouseDoubleClick_1(object sender, MouseButtonEventArgs e)
        {
            
            OpenSelectedUri();
        }
        private void ChangeCurrentUri(string uri)
        {
            if(CurrentUri!=uri)
            {
                CurrentUri = uri;
                
            }
            //List<string> tags = uriDB.GetTags(CurrentUri);
            tagsBar.UpdateUri(uri,uriDB,tagsDB);
        }
        private string CurrentUri = null;
        private void UpdateCurrentUriByContextMenu()
        {
            if(lst.SelectedItem as SearchItemInf!=null)
            {
                SearchItemInf it = lst.SelectedItem as SearchItemInf;
                if(FileOperator.isValidFileUrl(it.Detail))
                {
                    ChangeCurrentUri(it.Detail);
                }
            }
            else
            {
                ChangeCurrentUri(null);
            }
        }
        private void miOpen_Click(object sender, RoutedEventArgs e)
        {
            OpenSelectedUri();

        }
        private void lstItem_MouseDoubleClick(object sender,RoutedEventArgs e)
        {
            OpenSelectedUri();
        }
        private void OpenSelectedUri()
        {
            UpdateCurrentUriByContextMenu();
            if (FileOperator.isValidFileUrl(CurrentUri))
            {
                FileOperator.StartFile(CurrentUri);
            }
        }

        private void miOpenPath_Click(object sender, RoutedEventArgs e)
        {
            UpdateCurrentUriByContextMenu();
            if (FileOperator.isValidFileUrl(CurrentUri))
            {
                FileOperator.LocateFile(CurrentUri);
            }
        }

        private void lst_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            UpdateCurrentUriByContextMenu();
        }

        private void miCopy_Click(object sender, RoutedEventArgs e)
        {
            UpdateCurrentUriByContextMenu();
            Clipboard.SetText(TagCanvas.KUMMERWU_URI_COPY+"`" + GetSelUriList());
        }
        private string GetSelUriList()
        {
            string uris = "";
            foreach(SearchItemInf it in lst.SelectedItems)
            {
                uris += it.Detail + "?";
            }
            return uris.Trim('?');
        }
        private void miCut_Click(object sender, RoutedEventArgs e)
        {
            Clipboard.SetText(TagCanvas.KUMMERWU_URI_CUT+"`" + GetSelUriList());
        }
    }

    public class SearchItemInf
    {
        public string _Detail
        {
            set
            {
                name = Path.GetFileName(value);
                dir = Path.GetDirectoryName(value);
                all = value;
            }
        }

        private string name, dir,all;
        public BitmapSource _icon;
        public string Detail
        {
            get
            {
                return all;
            }
        }
        public BitmapSource icon
        {
            get
            {
                return _icon;
            }
        }
        public string Name
        {
            get
            {
                return name;
            }
        }
        public string Dir
        {
            get
            {
                return dir;
            }
        }

        public static List<SearchItemInf> GetFilesByTag(string tag,IUriDB db)
        {
            List<string> files = db.Query(tag);
            List<SearchItemInf> ret = new List<SearchItemInf>();
            foreach (string key in files)
            {
                if (FileOperator.isValidFileUrl(key))
                {
                    SearchItemInf it = new SearchItemInf();
                    it._Detail = key;
                    it._icon = GIconHelper.GetBitmapFromFile(key);
                    ret.Add(it);
                }
            }
            return ret;
        }
    }
}
