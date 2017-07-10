using LuceneTest.AutoComplete;
using System;
using System.Collections.Generic;

namespace LuceneTest.TagMgr
{

    public interface ITagDB:IDisposable, ISearchDataProvider
    {
        int ResetRelationOfChild(string parent, string child);
        int AddTag(string parent,string child);
        string AddTag(string sentence);
        int RemoveTag(string tag);
        int MergeAliasTag(string tag1, string tag2);
        List<string> QueryTagChildren(string tag);
        List<string> QueryTagParent(string tag);
        List<string> QueryTagAlias(string tag);
    }
}
