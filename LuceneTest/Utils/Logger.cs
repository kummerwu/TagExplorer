using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LuceneTest.Core
{
    class Logger
    {
        private static log4net.ILog log = log4net.LogManager.GetLogger("fileLog");
        public static void Log(string s)
        {
            log.Debug(s);
        }
    }
}
