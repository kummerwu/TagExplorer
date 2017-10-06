using TagExplorer.UriMgr;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Windows.Media.Imaging;
using TagExplorer.Utils;
using System;

namespace TagExplorer.UriInfList
{
    public class SearchResultItem : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        public void SetUri(string fullPath,IUriDB db)//TODO 支持http图标
        {
            all = fullPath;
            //http链接
            if (PathHelper.IsValidHttps(all))
            {
                string title = db.GetTitle(all);
                //指定了标题的http链接
                if (title != null && title.Length > 0)
                {
                    name = title;
                    dir = all;
                }
                //没有指定标题的http链接
                else
                {
                    System.Uri uri = new System.Uri(all); //TODO 获取http文档的名称和路径
                    int lastIdx = all.LastIndexOf('/');
                    if (lastIdx < 1) lastIdx = all.LastIndexOf('\\');

                    if (lastIdx > 1)
                    {
                        name = all.Substring(lastIdx + 1); //TODO 更好的获得http文档标题的方法
                        dir = all.Substring(0, lastIdx);
                    }
                    else
                    {
                        name = all;
                        dir = all;
                    }
                }
            }
            //文件
            else 
            {
                name = Path.GetFileName(all);
                dir = Path.GetDirectoryName(all);
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
                if(name.Length>64)
                {
                    name = name.Substring(0, 64)+"...";
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
        private DateTime lastAccessTime = DateTime.MinValue;
        private DateTime lastWriteTime = DateTime.MinValue;
        private DateTime InvalidTime = new DateTime(1);
        public DateTime LastAccessTime
        {
            get
            {
                if(lastAccessTime == DateTime.MinValue)
                {
                    if (PathHelper.IsValidFS(all))
                    {
                        lastAccessTime = File.GetLastAccessTime(all);
                    }
                    else
                    {
                        lastAccessTime = InvalidTime;
                    }
                }
                return lastAccessTime;
            }
        }

        public DateTime LastWriteTime
        {
            get
            {
                if (lastWriteTime == DateTime.MinValue)
                {
                    if (PathHelper.IsValidFS(all))
                    {
                        lastWriteTime = File.GetLastWriteTime(all);
                    }
                    else
                    {
                        lastWriteTime = InvalidTime;
                    }
                }
                return lastWriteTime;
            }
        }

        public static List<SearchResultItem> GetFilesByTag(string tag,IUriDB db)
        {
            List<string> files = db.Query(tag);
            List<SearchResultItem> ret = new List<SearchResultItem>();
            foreach (string uri in files)
            {
                if (PathHelper.IsValidUri(uri))
                {
                    SearchResultItem it = new SearchResultItem();
                    it.SetUri(uri,db);
                    it._icon = GIconHelper.GetBitmapFromFile(uri);
                    
                    ret.Add(it);
                }
            }
            return ret;
        }
    }
}
