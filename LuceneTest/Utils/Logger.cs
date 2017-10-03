using System;
using System.Collections.Generic;

namespace TagExplorer.Utils
{

    public class Logger
    {

        #region 公有方法
        public static void D(string s)
        {
            //log.Debug(PREFIX+s);
        }
        public static void D(string fmt ,params object[] par)
        {
            //string s = string.Format(PREFIX+fmt, par);
            //log.Debug(s);
        }

        public static void I(string fmt, params object[] par)
        {
            string s = string.Format(PREFIX+fmt, par);
            log.Info(s);
        }
        public static void E(string fmt,params object[] par)
        {
            log.Error(string.Format(fmt, par));
        }
        public static void E(Exception e)
        {
            log.Error("\r\n"+e.StackTrace +"\r\n"+ e.Source + "\r\n" + e.Message+"\r\n");
        }
        public static void IN(string INF)
        {
            log.Debug("\r\n++++"+INF);
            deep++;
            stack.Push(INF);
            PREFIX = INF.PadRight(8) + new string(' ', deep * 8);

        }
        public static void OUT()
        {
            
            deep--;
            string INF = stack.Pop();
            string tag = stack.Count == 0 ? "        " : stack.Peek().PadRight(8);
            PREFIX = tag + new string(' ', deep * 8);
            log.Debug("\r\n----" + INF);
        }
        #endregion

        #region 私有方法
        private static int deep = 0;
        static string PREFIX = "        ";
        private static log4net.ILog log = log4net.LogManager.GetLogger("fileLog");
        static Stack<string> stack = new Stack<string>();
        #endregion
    }
}
