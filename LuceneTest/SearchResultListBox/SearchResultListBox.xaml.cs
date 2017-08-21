using TagExplorer.TagMgr;
using TagExplorer.UriMgr;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using TagExplorer.Utils;
using System.Collections.Generic;

namespace TagExplorer.UriInfList
{
    /// <summary>
    /// UriInfoPanel.xaml 的交互逻辑
    /// </summary>
    public partial class SearchResultListBox : UserControl
    {
        public delegate void CurrentUriChanged(string uri);
        public CurrentUriChanged CurrentUriChangedCallback;
        public SearchResultListBox()
        {
            InitializeComponent();
        }
        List<SearchResultItem> dataList = null;
        public void ShowQueryResult(string query, IUriDB uriDB, ITagDB tagsDB)
        {
            this.uriDB = uriDB;
            this.tagsDB = tagsDB;
            dataList = SearchResultItem.GetFilesByTag(query, uriDB);
            SortType = -1;
            SortBy("访问时间");
            TipsCenter.Ins.ListInf = "文件列表统计:" + query + " Found Files:" + dataList.Count;
            UpdateCurrentList();
            AdjustGridColumnWidth();
        }

        private void UpdateCurrentList()
        {
            lst.ItemsSource = null;
            lst.ItemsSource = dataList;
            
            if (lst.Items.Count > 0)
            {
                lst.SelectedIndex = 0;

            }
            UpdateCurrentUriByContextMenu();
            
        }

        private void AdjustGridColumnWidth()
        {
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

        private IUriDB uriDB = null;
        private ITagDB tagsDB = null;
        private void lst_MouseDoubleClick_1(object sender, MouseButtonEventArgs e)
        {
            
            OpenSelectedUri();
        }
        private void ChangeCurrentUri(string uri)
        {
            if (CurrentUri != uri)
            {
                CurrentUri = uri;
                CurrentUriChangedCallback?.Invoke(uri);
            }            
            tagsBar.ChangeCurrentUri(uri,uriDB,tagsDB);
        }
        private string CurrentUri = null;
        private void UpdateCurrentUriByContextMenu()
        {
            SearchResultItem it = lst.SelectedItem as SearchResultItem;
            if (it!=null && FileShell.IsValidUri(it.Detail))
            {
                ChangeCurrentUri(it.Detail);
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
            FileShell.StartFile(CurrentUri);
            
        }
        private void miOpenPath_Click(object sender, RoutedEventArgs e)
        {
            UpdateCurrentUriByContextMenu();
            FileShell.OpenExplorerByFile(CurrentUri);
            
        }

        private void lst_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            UpdateCurrentUriByContextMenu();
        }

        private void miCopy_Click(object sender, RoutedEventArgs e)
        {
            UpdateCurrentUriByContextMenu();
            ClipBoardSafe.SetText(ClipboardConst.KUMMERWU_URI_COPY+"`" + GetSelUriList(ClipboardConst.CO_COPY));
        }
        private string GetSelUriList(int status)
        {
            string uris = "";
            foreach(SearchResultItem it in lst.SelectedItems)
            {
                uris += it.Detail + ClipboardConst.ArgsSplitToken;
                it.Status = status;
            }
            return uris.Trim(ClipboardConst.ArgsSplitToken);
        }
        private void miCut_Click(object sender, RoutedEventArgs e)
        {
            ClipBoardSafe.SetText(ClipboardConst.KUMMERWU_URI_CUT + ClipboardConst.CommandSplitToken + GetSelUriList(ClipboardConst.CO_CUT));
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

        private void miOpenWith_Click(object sender, RoutedEventArgs e)
        {
            UpdateCurrentUriByContextMenu();

            FileShell.StartWithFile(CurrentUri);
        }

        private void miCopyPath_Click(object sender, RoutedEventArgs e)
        {
            UpdateCurrentUriByContextMenu();
            ClipBoardSafe.SetText(CurrentUri);
        }

        private void miCopyName_Click(object sender, RoutedEventArgs e)
        {
            UpdateCurrentUriByContextMenu();
            string name = "";
            if(FileShell.IsValidFS(CurrentUri))
            {
                name = System.IO.Path.GetFileName(CurrentUri);
            }
            
            ClipBoardSafe.SetText(name);
        }
        private int SortType = 1;
        private void lst_Click(object sender, RoutedEventArgs e)
        {
            var header = e.OriginalSource as GridViewColumnHeader;
            if (header == null) return;
            GridViewColumn col = header.Column;
            if(col!=null)
            {
                SortBy(col.Header.ToString());
                UpdateCurrentList();
            }


        }

        private void SortBy(string colName)
        {
            switch (colName)
            {
                case "名称":
                    dataList.Sort(delegate (SearchResultItem x, SearchResultItem y) { return SortType * x.Name.CompareTo(y.Name); });
                    break;
                case "路径":
                    dataList.Sort(delegate (SearchResultItem x, SearchResultItem y) { return SortType * x.Dir.CompareTo(y.Dir); });
                    break;
                case "访问时间":
                    dataList.Sort(delegate (SearchResultItem x, SearchResultItem y) { return SortType * x.LastAccessTime.CompareTo(y.LastAccessTime); });
                    break;
                case "修改时间":
                    dataList.Sort(delegate (SearchResultItem x, SearchResultItem y) { return SortType * x.LastWriteTime.CompareTo(y.LastWriteTime); });
                    break;
            }
            SortType *= -1;
        }
    }
}
