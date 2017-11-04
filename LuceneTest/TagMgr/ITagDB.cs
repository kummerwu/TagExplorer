using TagExplorer.AutoComplete;
using System;
using System.Collections.Generic;

namespace TagExplorer.TagMgr
{

    public interface ITagDB:IDisposable, ISearchDataProvider
    {
        GUTag NewTag(string title);
        List<GUTag> GetTags(string title);
        GUTag GetTag(Guid id);
        int AddTag(GUTag parent, GUTag child);
        //int UpdateTag(GUTag oldChild, GUTag newChild);
        int RemoveTag(GUTag tag);
        //删除child的原来所有的parent，将parent切换到指定的新的parent
        int ResetParent(GUTag parent, GUTag child);
        int MergeAlias(GUTag tag1, GUTag tag2);
        int ChangePos(GUTag tag, int direct);

        List<GUTag> QueryTags(string title);
        List<GUTag> QueryTagChildren(GUTag tag);
        int          QueryChildrenCount(GUTag tag);
        List<GUTag> QueryTagParent(GUTag tag);
        List<string> QueryTagAlias(GUTag tag);
        int Import(string importInf);
    }
    public class ITagDBConst
    {
        public const int R_OK = 0;
    }
    
}
