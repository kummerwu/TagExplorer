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
                    _ins.CalcPath();

                }
                return _ins;
            }
        }
        public string Root = @"J:\00TagExplorerBase";
        public string TagDB;
        public string UriDB;
        public string DocBase;
        public int TAG_MAX_RELATION = 1000;
        public bool isUTest = false;
        public bool IsUTest
        {
            get { return isUTest; }
            set
            {
                isUTest = value;
                if(isUTest)
                {
                    Root = @"B:\00TagExplorerBase";
                    CalcPath();
                }
            }
        }

        private void CalcPath()
        {
            TagDB = Root + @"\TagDB";
            UriDB = Root + @"\UriDB";
            DocBase = Root + @"\DocumentBase";
        }
    }
}
