using TagExplorer.AutoComplete;
using System;
using System.Collections.Generic;

namespace TagExplorer.TagMgr
{

    public interface ITagDB:IDisposable, ISearchDataProvider
    {
        //删除child的原来所有的parent，将parent切换到指定的新的parent
        int ResetRelationOfChild(string parent, string child);
        int AddTag(string parent,string child);
        //string AddTag(string sentence);
        int RemoveTag(string tag);
        int MergeAliasTag(string tag1, string tag2);
        List<string> QueryTagChildren(string tag);
        int GetTagChildrenCount(string tag);
        List<string> QueryTagParent(string tag);
        List<string> QueryTagAlias(string tag);
    }
    public class ITagDBConst
    {
        public const int R_OK = 0;
    }
    
}
