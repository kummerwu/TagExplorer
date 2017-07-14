using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;
namespace LuceneTest
{
    public class TipsCenter : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
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

        private string tips;
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

    }
}
namespace LuceneTest.Core
{
    
    public class Logger
    {
        
        private static log4net.ILog log = log4net.LogManager.GetLogger("fileLog");
        public static void D(string s)
        {
            //log.Debug(PREFIX+s);
        }
        public static void D(string fmt ,params object[] par)
        {
            //string s = string.Format(PREFIX+fmt, par);
            //log.Debug(s);
        }
        public static void E(string fmt,params object[] par)
        {
            log.Error(string.Format(fmt, par));
        }
        private static int deep = 0;
        static string PREFIX = "        ";
        static Stack<string> stack = new Stack<string>();
        public static void IN(string INF) {
            deep++;
            stack.Push(INF);
            PREFIX = INF.PadRight(8) + new string(' ', deep * 8);

        }
        public static void OUT() {
            deep--;
            stack.Pop();
            string tag = stack.Count == 0 ? "        " : stack.Peek().PadRight(8);
            PREFIX =tag+ new string(' ', deep * 8);
        }
    }
}
