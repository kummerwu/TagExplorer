using System;
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
        public string BackTaskInf { set { SetInf(value, 6); } }
        private DateTime lastTime = DateTime.MinValue;
        public string StartTime {
            set {
                string inf = value + DateTime.Now.ToString("===mm:ss.fff  ##");
                if (lastTime!=DateTime.MinValue)
                {
                    inf += (int)((DateTime.Now - lastTime).TotalMilliseconds) + "ms";
                }
                inf += "\r\n";
                lastTime = DateTime.Now;
                SetInf((infs[7]==null?"":infs[7] )+ inf, 7);
            }
        }
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
        private string[] infs = new string[60];
        private string MergeInf
        {
            get
            {
                string all = "";
                for (int i = 0; i < infs.Length; i++)
                {
                    if (!string.IsNullOrEmpty(infs[i]))
                    {
                        all += infs[i] + "\r\n";
                    }
                }
                return all;
            }
        }
        private void SetInf(string v, int idx) { infs[idx] = v; Tips = MergeInf; }

    }
}
