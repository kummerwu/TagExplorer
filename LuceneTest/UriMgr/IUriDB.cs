using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LuceneTest.UriMgr
{
    interface IUriDB:IDisposable
    {
        int AddUri(string Uri);
        int AddUri(string Uri, List<string> tags);
        int AddUri(string Uri, List<string> tags,string Title);
        int DelUri(string Uri,bool Delete);
        int DelUri(string Uri, List<string> tags);
        int UpdateUri(string Uri, string Title);
        List<string> Query(string query);
    }
}
