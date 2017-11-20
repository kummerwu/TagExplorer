using TagExplorer.AutoComplete;
using System;
using System.Collections.Generic;
using TagExplorer.UriMgr;

namespace TagExplorer.TagMgr
{

    public interface ITagDB:IDisposable, ISearchDataProvider
    {
        //增
        GUTag NewTag(string title);
        //删
        int RemoveTag(GUTag tag);
        //改：父子关系
        int AddTag(GUTag parent, GUTag child);
        //删除child的原来所有的parent，将parent切换到指定的新的parent
        int ResetParent(GUTag parent, GUTag child);
        //改：标题
        int ChangeTitle(GUTag tag, string newTitle);
        int MergeAlias(GUTag tag1, GUTag tag2); //GUTAG:merge语义有变化，现在是将tag2合并到tag1，并删除tag2
        //改：本节点在父节点所有孩子中的位置
        int ChangePos(GUTag tag, int direct);
        //查：
        GUTag GetTag(Guid id);
        List<GUTag> QueryTags(string title);
        List<GUTag> QueryTagChildren(GUTag tag);
        int          QueryChildrenCount(GUTag tag);
        List<GUTag> QueryTagParent(GUTag tag);
        List<string> QueryTagAlias(GUTag tag);

        int Import(string importInf);

        DataChanged TagDBChanged { get; set; }
    }
    public class ITagDBConst
    {
        public const int R_OK = 0;
    }
    
}
