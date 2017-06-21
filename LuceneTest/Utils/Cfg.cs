using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LuceneTest.Core
{
    class Cfg
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
        public string TagDB = @"D:\02-个人目录\LuceneTest\TagExplorer\DocumentBase\TagDB";
        public string UriDB = @"D:\02-个人目录\LuceneTest\TagExplorer\DocumentBase\UriDB";

        public int TAG_MAX_RELATION = 1000;
        public bool IsDbg = false;
    }
}
