namespace LuceneTest.Core
{
    class Logger
    {
        private static log4net.ILog log = log4net.LogManager.GetLogger("fileLog");
        public static void Log(string s)
        {
            log.Debug(s);
        }
        public static void Log(string fmt ,params object[] par)
        {
            string s = string.Format(fmt, par);
            log.Debug(s);
        }
    }
}
