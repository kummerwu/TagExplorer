using TagExplorer.AutoComplete;
using System;
using System.Collections.Generic;
using TagExplorer.UriMgr;

namespace TagExplorer.TagMgr
{

    public interface ITagDB:IDisposable, ISearchDataProvider
    {
        //增：新增一个tag
        GUTag NewTag(string title);
        //删：删除一个tag
        int RemoveTag(GUTag tag);
        //改：建立父子关系
        int SetParent(GUTag parent, GUTag child);
        //改：删除child的原来所有的parent，将parent切换到指定的新的parent
        int ResetParent(GUTag parent, GUTag child);
        //改：标题
        GUTag ChangeTitle(GUTag tag, string newTitle);
        int MergeAlias(GUTag tag1, GUTag tag2); //GUTAG:merge语义有变化，现在是将tag2合并到tag1，并删除tag2
        //改：本节点在父节点所有孩子中的位置
        int ChangeChildPos(GUTag tag, int direct);
        //查：根据id查tag
        GUTag GetTag(Guid id);
        //查：根据标题查tag
        List<GUTag> QueryTags(string title);
        //查：获得tag的所有子节点
        List<GUTag> QueryTagChildren(GUTag tag);
        int          QueryChildrenCount(GUTag tag);
        //查：获得tag的所有父节点（最早设计时允许有多个父节点，实际上后来改为只运行有一个父节点）
        //该接口后面有空整理一下
        List<GUTag> QueryTagParent(GUTag tag);
        //查找tag的所有别名
        List<string> QueryTagAlias(GUTag tag);
        //版本升级时使用该函数导入老版本的数据。
        int Import(string importFile);
        int Export(string exportFile);
        //通知：数据库变更通知
        DataChanged TagDBChanged { get; set; }
    }
    public class ITagDBConst
    {
        public const int R_OK = 0;
    }
    
}
