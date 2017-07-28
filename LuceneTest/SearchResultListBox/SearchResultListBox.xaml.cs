using TagExplorer.TagMgr;
using TagExplorer.UriMgr;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using TagExplorer.Utils;

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

        public void ShowQueryResult(string query, IUriDB uriDB, ITagDB tagsDB)
        {
            this.uriDB = uriDB;
            this.tagsDB = tagsDB;
            var datasource = SearchResultItem.GetFilesByTag(query, uriDB);
            lst.ItemsSource = datasource;
            TipsCenter.Ins.ListInf = "文件列表统计:" + query + " Found Files:" + datasource.Count;
            if (lst.Items.Count > 0)
            {
                lst.SelectedIndex = 0;

            }
            UpdateCurrentUriByContextMenu();
            AdjustGridColumnWidth();
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
    }
}
