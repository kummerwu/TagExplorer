using System;

namespace Log
{
    public class Livelog
    {
        public delegate void LogEventHandler(object sender, LogEventArgs e);
        public static event LogEventHandler LogChanged;

        public static string FullLog;

        public static void Log(string strText)
        {
            return;
            LogChanged(null, new LogEventArgs("\r\n" + strText));
            FullLog += "\r\n" + strText;

            //System.Diagnostics.Debug.Print(strText);
        }

    }

    public class LogEventArgs: EventArgs
    {
        private string mLogtext;

        public string Logtext
        {
            get { return mLogtext; }
        }

        public LogEventArgs(string strLogText)
        {
            mLogtext = strLogText;
        }
    }

}
