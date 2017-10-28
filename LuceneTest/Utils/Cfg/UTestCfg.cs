namespace TagExplorer.Utils
{
    public class UTestCfg
    {
        private static UTestCfg _ins;
        public static UTestCfg Ins
        {
            get
            {
                if(_ins==null)
                {
                    _ins = new UTestCfg();
                }
                return _ins;
            }
        }
        
        

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
                    CfgPath.RootPath = @"B:\00TagExplorerBase";
                    
                }
            }
        }
        
        

    }
}
