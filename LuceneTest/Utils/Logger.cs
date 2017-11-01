using System;
using System.Collections.Generic;
using System.Diagnostics;
namespace TagExplorer.Utils
{

    public class Logger
    {
        public const bool on = true;
        #region 公有方法
        [Conditional("LOGGER")]
        public static void D(string s)
        {
            if (on)
            {
                log.Debug(PREFIX + s);
            }
        }
        
        public static void D(string fmt ,params object[] par)
        {
            if (on)
            {
                string s = string.Format(PREFIX + fmt, par);
                log.Debug(s);
            }
        }
        [Conditional("LOGGER")]
        public static void I(string fmt, params object[] par)
        {
            if (on)
            {
                string s = string.Format(PREFIX + fmt, par);
                log.Info(s);
            }
        }
        [Conditional("LOGGER")]
        public static void E(string fmt,params object[] par)
        {
            if (on)
            {
                log.Error(string.Format(fmt, par));
            }
        }
        [Conditional("LOGGER")]
        public static void E(Exception e)
        {
            if (on)
            {
                log.Error("\r\n" + e.StackTrace + "\r\n" + e.Source + "\r\n" + e.Message + "\r\n");
            }
        }
        [Conditional("LOGGER")]
        public static void IN(string INF)
        {
            if (on)
            {
                log.Debug("\r\n++++" + INF);
                deep++;
                stack.Push(INF);
                PREFIX = INF.PadRight(8) + new string(' ', deep * 8);
            }

        }
        [Conditional("LOGGER")]
        public static void OUT()
        {
            if (on)
            {
                deep--;
                string INF = stack.Pop();
                string tag = stack.Count == 0 ? "        " : stack.Peek().PadRight(8);
                PREFIX = tag + new string(' ', deep * 8);
                log.Debug("\r\n----" + INF);
            }
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
