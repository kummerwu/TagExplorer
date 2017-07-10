using System;
using System.Collections.Generic;

namespace LuceneTest.AutoComplete
{
   

    public interface ISearchDataProvider
    {
        List<string> QueryAutoComplete(string searchTerm);
    }

    
}