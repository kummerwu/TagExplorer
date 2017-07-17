namespace TagExplorer.Utils
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
        
        public int TAG_MAX_RELATION = 1000;

        //如果是单元测试，将文件系统使用一个内存文件系统做临时测试，创建删除文件
        public bool isUTest = false;
        public bool IsUTest
        {
            get { return isUTest; }
            set
            {
                isUTest = value;
                if(isUTest)
                {
                    PathHelper.RootPath = @"B:\00TagExplorerBase";
                    
                }
            }
        }
        public string DefaultTag { get { return "我的大脑"; } }
        public int LRU_MAX_CNT = 8;

    }
}
