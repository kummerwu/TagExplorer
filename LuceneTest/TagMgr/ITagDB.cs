using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LuceneTest.TagMgr
{

    interface ITagDB:IDisposable
    {
        int AddTag(string parent,string child);
        int RemoveTag(string tag);
        int MergeAliasTag(string tag1, string tag2);
        List<string> QueryTagChildren(string tag);
        List<string> QueryTagParent(string tag);
        List<string> QueryTagAlias(string tag);
    }
}
