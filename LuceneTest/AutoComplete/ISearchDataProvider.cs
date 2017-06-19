using System;
using System.Collections.Generic;

namespace LuceneTest.AutoComplete
{
   

    public interface ISearchDataProvider
    {
        List<string> QueryAutoComplete(string searchTerm);
    }

    public class SearchDemo1 : ISearchDataProvider
    {
        public List<string> QueryAutoComplete(string searchTerm)
        {
            if (searchTerm.Contains("0")) return new List<string>();
            else if (searchTerm.Contains("1")) return new List<string>() { searchTerm};
            else if (searchTerm.Contains("2")) return new List<string>() { searchTerm+"1",searchTerm+"2" };
            else
            {
                List<string> ret = new List<string>();
                for (int i = 0;i< 10;i++)
                {
                    ret.Add(searchTerm + "@"+i);
                    
                }
                return ret;
            }
        }
    }
}