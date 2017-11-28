using TagExplorer.UriMgr;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Windows.Media.Imaging;
using TagExplorer.Utils;
using System;

namespace TagExplorer.UriInfList
{
    /// <summary>
    ///  查询获得文件列表，SearchResultItem是该文件列表中每一项的显示信息。
    /// </summary>
    public class SearchResultItem : INotifyPropertyChanged
    {
        //公有成员变量************************************************************
        public event PropertyChangedEventHandler PropertyChanged;

        //私有成员变量************************************************************
        private string fullUri;
        private BitmapSource _icon; 
        private string name;     //对于文件，该字段存储文件名，如果是链接，存储标题（两种情况，用户手工修改了标题，或者没有修改标题）   
        private string dir;
        
        private DateTime lastAccessTime = DateTime.MinValue;
        private DateTime createTime = DateTime.MinValue;
        private DateTime InvalidTime = new DateTime(1);
        private int status = 0;

        URIItem uriItem = null;
        //公有成员方法************************************************************
        public static List<SearchResultItem> QueryByTag(string tag, IUriDB db)
        {
            List<string> files = db.Query(tag);
            List<SearchResultItem> ret = new List<SearchResultItem>();
            foreach (string uri in files)
            {
                if (PathHelper.IsValidUri(uri))
                {
                    SearchResultItem it = new SearchResultItem();
                    it.Init(uri, db);
                    ret.Add(it);
                }
            }
            return ret;
        }
        public string FullUri { get { return fullUri; } }
        public BitmapSource Icon { get { return _icon; } }
        public string Name
        {
            get
            {
                if (name.Length > 64)
                {
                    name = name.Substring(0, 64) + "...";
                }
                return name;
            }
            set
            {
                name = value;
            }
        }
        public string Dir
        {
            get
            {
                return dir;
            }
        }
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
        public DateTime LastAccessTime
        {
            get
            {
                
                if (lastAccessTime == DateTime.MinValue)
                {
                    if (PathHelper.IsValidFS(fullUri))
                    {
                        lastAccessTime = File.GetLastAccessTime(fullUri);
                    }
                    else
                    {
                        lastAccessTime = InvalidTime;
                    }
                }
                if (uriItem != null && uriItem.AccessTime != null && lastAccessTime == InvalidTime)
                {
                    lastAccessTime =  uriItem.AccessTime;
                }
                return lastAccessTime;
            }
        }
        public DateTime CreateTime
        {
            get
            {
                
                if (createTime == DateTime.MinValue)
                {
                    if (PathHelper.IsValidFS(fullUri))
                    {
                        createTime = File.GetLastWriteTime(fullUri);
                    }
                    else
                    {
                        createTime = InvalidTime;
                    }
                }
                if (uriItem != null && uriItem.CreateTime != null && createTime == InvalidTime)
                {
                    createTime =  uriItem.CreateTime;
                }
                return createTime;
            }
        }
        //私有成员方法************************************************************
        private void Init(string fullPath, IUriDB db)//TODO 支持http图标
        {
            fullUri = fullPath;
            uriItem = db.GetInf(fullUri);
            if (uriItem == null) return;

            //http链接
            if (PathHelper.IsValidWebLink(fullUri))
            {
                string title = uriItem.Title;
                //指定了标题的http链接
                if (title != null && title.Length > 0)
                {
                    name = title;
                    dir = fullUri;
                }
                //没有指定标题的http链接
                else
                {
                    //System.Uri uri = new System.Uri(fullUri); //TODO 获取http文档的名称和路径
                    GetNameTitle();
                }
            }
            //文件
            else
            {
                string name2 = Path.GetFileName(fullUri);
                if (uriItem != null && !string.IsNullOrEmpty(uriItem.Title) && uriItem.Title!=name2)
                {
                    name = uriItem.Title + "("+name2+")";
                }
                else
                {
                    name = name2;
                }
                dir = Path.GetDirectoryName(fullUri);
            }
            _icon = GIconHelper.GetBitmapFromFile(fullUri);
        }

        private void GetNameTitle()
        {
            int lastIdx = fullUri.LastIndexOf('/');
            if (lastIdx < 1) lastIdx = fullUri.LastIndexOf('\\');

            if (lastIdx > 1)
            {
                name = fullUri.Substring(lastIdx + 1); //TODO 更好的获得http文档标题的方法
                dir = fullUri.Substring(0, lastIdx);
            }
            else
            {
                name = fullUri;
                dir = fullUri;
            }
        }

        public static string GetTitle(string fullUri)
        {
            string title = "";
            int lastIdx = fullUri.LastIndexOf('/');
            if (lastIdx < 1) lastIdx = fullUri.LastIndexOf('\\');

            if (lastIdx > 1)
            {
                title = fullUri.Substring(lastIdx + 1); //TODO 更好的获得http文档标题的方法
            }
            else
            {
                title = fullUri;
            }
            return title;
        }
    }
}
