using System;
using System.Collections.Generic;

namespace LuceneTest.UriMgr
{
    public interface IUriDB:IDisposable
    {
        int AddUri(string Uri);
        int AddUri(string Uri, List<string> tags);
        int AddUri(string Uri, List<string> tags,string Title);
        int DelUri(string Uri,bool Delete);
        int DelUri(string Uri, List<string> tags);
        int UpdateUri(string Uri, string Title);
        List<string> Query(string query);
        DataChanged UriDBChanged { get; set; }
        string GetTitle(string Uri);
        List<string> GetTags(string Uri);
        
    }
    public delegate void DataChanged();
}
