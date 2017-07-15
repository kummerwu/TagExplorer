using AnyTag.BL;
using AnyTagNet;
using LuceneTest.Core;
using LuceneTest.TagGraph;
using LuceneTest.TagMgr;
using LuceneTest.UriMgr;
using LuceneTest.Utils;
using System;
using System.Collections.Generic;
using System.ComponentModel;
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
            var datasource = SearchItemInf.GetFilesByTag(query, uriDB);
            lst.ItemsSource = datasource;
            TipsCenter.Ins.ListInf ="文件列表统计:"+ query +" Found Files:" + datasource.Count;
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
                if(FileShell.isValidFileUrl(it.Detail))
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
            if (FileShell.isValidFileUrl(CurrentUri))
            {
                FileShell.StartFile(CurrentUri);
            }
        }

        private void miOpenPath_Click(object sender, RoutedEventArgs e)
        {
            UpdateCurrentUriByContextMenu();
            if (FileShell.isValidFileUrl(CurrentUri))
            {
                FileShell.LocateFile(CurrentUri);
            }
        }

        private void lst_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            UpdateCurrentUriByContextMenu();
        }

        private void miCopy_Click(object sender, RoutedEventArgs e)
        {
            UpdateCurrentUriByContextMenu();
            Clipboard.SetText(ClipboardOperator.KUMMERWU_URI_COPY+"`" + GetSelUriList(ClipboardOperator.CO_COPY));
        }
        private string GetSelUriList(int status)
        {
            string uris = "";
            foreach(SearchItemInf it in lst.SelectedItems)
            {
                uris += it.Detail + ClipboardOperator.ArgsSplitToken;
                it.Status = status;
            }
            return uris.Trim(ClipboardOperator.ArgsSplitToken);
        }
        private void miCut_Click(object sender, RoutedEventArgs e)
        {
            Clipboard.SetText(ClipboardOperator.KUMMERWU_URI_CUT + ClipboardOperator.CommandSplitToken + GetSelUriList(ClipboardOperator.CO_CUT));
        }

        private void Cut_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            miCut_Click(sender, e);
        }

        private void Cut_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
        }

        private void Copy_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
        }

        private void Copy_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            miCopy_Click(sender, e);
        }
    }

    public class SearchItemInf : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
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
        private int status = 0;
        public int Status
        {
            get
            {
                return status;
            }
            set
            {
                status = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Status"));
            }
        }
        public static List<SearchItemInf> GetFilesByTag(string tag,IUriDB db)
        {
            List<string> files = db.Query(tag);
            List<SearchItemInf> ret = new List<SearchItemInf>();
            foreach (string key in files)
            {
                if (FileShell.isValidFileUrl(key))
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
