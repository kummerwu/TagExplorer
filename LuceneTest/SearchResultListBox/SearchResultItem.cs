using TagExplorer.UriMgr;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Windows.Media.Imaging;
using TagExplorer.Utils;

namespace TagExplorer.UriInfList
{
    public class SearchResultItem : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        public string _Detail //TODO 支持http图标
        {
            set
            {
                if (FileShell.IsValidHttps(value))
                {
                    System.Uri uri = new System.Uri(value); //TODO 获取http文档的名称和路径
                    int lastIdx = value.LastIndexOf('/');
                    if(lastIdx<1) lastIdx = value.LastIndexOf('\\');

                    if (lastIdx > 1)
                    {
                        name = value.Substring(lastIdx+1); //TODO 更好的获得http文档标题的方法
                        dir = value.Substring(0, lastIdx);
                    }
                    else
                    {
                        name = value;
                        dir = value;
                    }
                    all = value;
                }
                else 
                {
                    name = Path.GetFileName(value);
                    dir = Path.GetDirectoryName(value);
                    all = value;
                }
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
        public static List<SearchResultItem> GetFilesByTag(string tag,IUriDB db)
        {
            List<string> files = db.Query(tag);
            List<SearchResultItem> ret = new List<SearchResultItem>();
            foreach (string key in files)
            {
                if (FileShell.IsValidUri(key))
                {
                    SearchResultItem it = new SearchResultItem();
                    it._Detail = key;
                    it._icon = GIconHelper.GetBitmapFromFile(key);
                    ret.Add(it);
                }
            }
            return ret;
        }
    }
}
