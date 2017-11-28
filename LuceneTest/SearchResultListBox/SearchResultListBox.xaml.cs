using TagExplorer.TagMgr;
using TagExplorer.UriMgr;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using TagExplorer.Utils;
using System.Collections.Generic;
using TagExplorer.SearchResultListBox;

namespace TagExplorer.UriInfList
{
    /// <summary>
    /// 搜索结果文件列表的UI界面
    /// </summary>
    public partial class SearchResultListBox : UserControl
    {
        //公有成员变量************************************************************
        public delegate void CurrentUriChanged(string uri);
        public CurrentUriChanged CurrentUriChangedCallback;

        //私有成员变量************************************************************
        private IUriDB uriDB = null;
        private ITagDB tagsDB = null;
        private List<SearchResultItem> dataList = null;
        private string CurrentUri = null;
        private int SortType = 1;
        //公有成员方法************************************************************
        public SearchResultListBox()
        {
            InitializeComponent();
        }
        
        public void ShowQueryResult(string query, IUriDB uriDB, ITagDB tagsDB)
        {
            this.uriDB = uriDB;
            this.tagsDB = tagsDB;
            dataList = SearchResultItem.QueryByTag(query, uriDB);
            SortType = -1;
            SortBy("访问时间");
            TipsCenter.Ins.ListInf = "文件列表统计:" + query + " Found Files:" + dataList.Count;
            ShowItemList();
            AdjustGridColumnWidth();
        }
        //私有成员方法************************************************************
        //如果dataList发生变化（数据变化，或者排序发生变化），通过该函数显示
        private void ShowItemList()
        {
            lst.ItemsSource = null; //这一句非常重要，如果不先制空，在显示上可能会错误。
            lst.ItemsSource = dataList;
            
            if (lst.Items.Count > 0)
            {
                lst.SelectedIndex = 0;

            }
            CheckSelectedItem();
            
        }
        //自动调整每一列的宽度
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
        //通知外界当前选中的Uri发生变化
        private void NotifyCurrentUri(string uri)
        {
            if (CurrentUri != uri)//如果确实发生了变化，通知所有观察者
            {
                CurrentUri = uri;
                CurrentUriChangedCallback?.Invoke(uri);
            }

        }
        //检查看，当前的uri是否已经发生变化？如果有变化，通知UI更新。
        private void CheckSelectedItem()
        {
            SearchResultItem it = lst.SelectedItem as SearchResultItem;
            if (it != null && PathHelper.IsValidUri(it.FullUri))
            {
                NotifyCurrentUri(it.FullUri);
                tagsBar.ChangeCurrentUri(it.FullUri, uriDB, tagsDB);
            }
            else
            {
                NotifyCurrentUri(null);
                tagsBar.ChangeCurrentUri(null, uriDB, tagsDB);
            }

        }
        
        private void miOpen_Click(object sender, RoutedEventArgs e)
        {
            OpenSelectedUri();
        }
        private void lstItem_MouseDoubleClick(object sender, RoutedEventArgs e)
        {
            OpenSelectedUri();
        }
        
        
        
        
        private void OpenSelectedUri()
        {
            CheckSelectedItem();
            FileShell.OpenFile(CurrentUri);
            
        }
        private void miOpenPath_Click(object sender, RoutedEventArgs e)
        {
            CheckSelectedItem();
            FileShell.OpenExplorerByFile(CurrentUri);
            
        }

        private void lst_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            CheckSelectedItem();
        }

        private void miCopy_Click(object sender, RoutedEventArgs e)
        {
            CheckSelectedItem();
            ClipBoardSafe.SetText(ClipboardConst.KUMMERWU_URI_COPY+"`" + GetSelUriList(ClipboardConst.CO_COPY));
        }
        private string GetSelUriList(int status)
        {
            string uris = "";
            foreach(SearchResultItem it in lst.SelectedItems)
            {
                uris += it.FullUri + ClipboardConst.ArgsSplitToken;
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
            CheckSelectedItem();
            FileShell.OpenFileWith(CurrentUri);
        }

        private void miCopyPath_Click(object sender, RoutedEventArgs e)
        {
            CheckSelectedItem();
            ClipBoardSafe.SetText(CurrentUri);
        }

        private void miCopyName_Click(object sender, RoutedEventArgs e)
        {
            CheckSelectedItem();
            string name = "";
            if(PathHelper.IsValidFS(CurrentUri))
            {
                name = System.IO.Path.GetFileName(CurrentUri);
            }
            
            ClipBoardSafe.SetText(name);
        }
        
        private void lst_Click(object sender, RoutedEventArgs e)
        {
            var header = e.OriginalSource as GridViewColumnHeader;
            if (header == null) return;
            GridViewColumn col = header.Column;
            if(col!=null)
            {
                SortBy(col.Header.ToString());
                ShowItemList();
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
                case "创建时间":
                    dataList.Sort(delegate (SearchResultItem x, SearchResultItem y) { return SortType * x.CreateTime.CompareTo(y.CreateTime); });
                    break;
            }
            SortType *= -1;
        }

        private void miRename_Click(object sender, RoutedEventArgs e)
        {
            CheckSelectedItem();
            string newTitle = InputBoxWindow.ShowDlg("条目重命名", CurrentUri, "");
            if (newTitle != null && newTitle.Length > 0)
            {
                uriDB.UpdateTitle(CurrentUri, newTitle);
            }
            
        }

        private void miDel_Click(object sender, RoutedEventArgs e)
        {
            int sel = lst.SelectedIndex;
            List<string> toBeDel = new List<string>();
            foreach (SearchResultItem it in lst.SelectedItems)
            {
                toBeDel.Add(it.FullUri);
            }
            uriDB.DelUris(toBeDel, true);
            if (lst.Items.Count > 0)
            {
                if (sel < 0) sel = 0;
                if (sel >= lst.Items.Count) sel = lst.Items.Count - 1;
                lst.SelectedIndex = sel;
                CheckSelectedItem();
            }
        }
    }
}
