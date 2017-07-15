using System.Collections.Generic;

namespace TagExplorer.AutoComplete
{


    public interface ISearchDataProvider
    {
        List<string> QueryAutoComplete(string searchTerm);
    }

    
}