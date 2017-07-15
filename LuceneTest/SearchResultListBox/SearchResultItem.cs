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
