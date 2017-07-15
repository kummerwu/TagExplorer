//namespace LuceneTest.AutoComplete
//{
//    public class AutoCompleteEntry
//    {
//        private string[] keywordStrings;    //允许输入的前缀
//        private string displayString;       //显示的名称

//        public string[] KeywordStrings
//        {
//            get
//            {
//                if (keywordStrings == null)
//                {
//                    keywordStrings = new string[] { displayString };
//                }
//                return keywordStrings;
//            }
//        }

//        public string DisplayName
//        {
//            get { return displayString; }
//            set { displayString = value; }
//        }

//        public AutoCompleteEntry(string name, params string[] keywords)
//        {
//            displayString = name;
//            keywordStrings = keywords;
//        }

//        public override string ToString()
//        {
//            return displayString;
//        }
//    }
//}
