using TagExplorer.AutoComplete;
using System;
using System.Collections.Generic;

namespace TagExplorer.TagMgr
{

    public interface ITagDB:IDisposable, ISearchDataProvider
    {
        
        int AddTag(string parent, string child);
        int RemoveTag(string tag);
        //删除child的原来所有的parent，将parent切换到指定的新的parent
        int ResetParent(string parent, string child);
        int MergeAlias(string tag1, string tag2);
        int ChangePos(string tag, int direct);

        List<string> QueryTagChildren(string tag);
        int          QueryChildrenCount(string tag);
        List<string> QueryTagParent(string tag);
        List<string> QueryTagAlias(string tag);
    }
    public class ITagDBConst
    {
        public const int R_OK = 0;
    }
    
}
