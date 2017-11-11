using System.Collections.Generic;
using TagExplorer.TagMgr;

namespace TagExplorer.AutoComplete
{


    public interface ISearchDataProvider
    {
        List<AutoCompleteTipsItem> QueryAutoComplete(string searchTerm);
    }

    public class AutoCompleteTipsItem
    {
        public string Tip;
        public string Content;
        public object Data;
        public int Score;
    }
}