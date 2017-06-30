namespace LuceneTest.Core
{
    public class Cfg
    {
        private static Cfg _ins;
        public static Cfg Ins
        {
            get
            {
                if(_ins==null)
                {
                    _ins = new Cfg();

                }
                return _ins;
            }
        }
        private const string Root = @"D:\00TagExplorerBase";
        public string TagDB = Root+@"\TagDB";
        public string UriDB = Root+@"\UriDB";
        public string DocBase = Root+@"\DocumentBase";
        public int TAG_MAX_RELATION = 1000;
        public bool IsDbg = false;
    }
}
