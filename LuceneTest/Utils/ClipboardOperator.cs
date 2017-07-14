using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LuceneTest.Utils
{
    class ClipboardOperator
    {
        public static char CommandSplitToken = '`';
        public static char ArgsSplitToken = '?';


        public const string KUMMERWU_TAG_COPY = "KUMMERWU_TAG_COPY";
        public const string KUMMERWU_TAG_CUT = "KUMMERWU_TAG_CUT";

        public const string KUMMERWU_URI_COPY = "KUMMERWU_URI_COPY";
        public const string KUMMERWU_URI_CUT = "KUMMERWU_URI_CUT";

        public static int CO_CUT = 1;
        public static int CO_COPY = 2;
        public static int CO_SELECTED = 3;
    }
}
