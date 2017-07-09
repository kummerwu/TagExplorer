using System.Collections.Generic;

namespace LuceneTest.Core
{
    public class Logger
    {
        private static log4net.ILog log = log4net.LogManager.GetLogger("fileLog");
        public static void Log(string s)
        {
            log.Debug(PREFIX+s);
        }
        public static void Log(string fmt ,params object[] par)
        {
            //string s = string.Format(PREFIX+fmt, par);
            //log.Debug(s);
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
