using System.ComponentModel;
namespace TagExplorer.Utils
{
    public class TipsCenter : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        public string TagInf { set { SetInf(value, 0); } }
        public string UriInf { set { SetInf(value, 1); } }
        public string MainInf { set { SetInf(value, 2); } }
        public string ListInf { set { SetInf(value, 3); } }
        public string TagDBInf { set { SetInf(value, 4); } }
        public string UriDBInf { set { SetInf(value, 5); } }
        public static TipsCenter Ins
        {
            get
            {
                if (_ins == null) _ins = new TipsCenter();
                return _ins;
            }
        }
        public string Tips
        {
            get { return tips; }
            set
            {
                tips = value;
                //触发事件
                if (PropertyChanged != null)
                {
                    PropertyChanged.Invoke(this, new PropertyChangedEventArgs("Tips"));
                }
            }
        }




        private string tips;
        private static TipsCenter _ins;
        private string[] infs = new string[6];
        private string MergeInf
        {
            get
            {
                string all = "";
                for (int i = 0; i < infs.Length; i++)
                {
                    all += infs[i] + "\r\n";
                }
                return all;
            }
        }
        private void SetInf(string v, int idx) { infs[idx] = v; Tips = MergeInf; }

    }
}
