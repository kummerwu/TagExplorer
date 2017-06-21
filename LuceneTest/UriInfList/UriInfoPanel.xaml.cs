﻿using AnyTag.BL;
using AnyTagNet;
using LuceneTest.TagMgr;
using LuceneTest.UriMgr;
using System;
using System.Collections;
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
        }
        private void lst_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {

        }

        private void lst_MouseDoubleClick_1(object sender, MouseButtonEventArgs e)
        {

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
    }

    public class SearchItemInf
    {
        public string _Detail;
        public BitmapSource _icon;
        public string Detail
        {
            get
            {
                return _Detail;
            }
        }
        public BitmapSource icon
        {
            get
            {
                return _icon;
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
