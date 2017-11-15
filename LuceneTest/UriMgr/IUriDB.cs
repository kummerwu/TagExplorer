using System;
using System.Collections.Generic;

namespace TagExplorer.UriMgr
{
    public interface IUriDB:IDisposable
    {
        /// <summary>
        /// 添加URI
        /// </summary>
        /// <param name="Uri">uri：如果是文件，uri为全路径，如果是http，uri这位url</param>
        /// <returns></returns>
        int AddUri(string Uri);
        /// <summary>
        /// 添加一组URI
        /// </summary>
        /// <param name="Uris">uri：如果是文件，uri为全路径，如果是http，uri这位url</param>
        /// <param name="tags">为每一个uri添加tag列表</param>
        /// <returns></returns>
        int AddUris(IEnumerable<string> Uris, List<string> tags);
        /// <summary>
        /// 添加URI
        /// </summary>
        /// <param name="Uri">uri：如果是文件，uri为全路径，如果是http，uri这位url</param>
        /// <param name="tags">为每一个uri添加tag列表</param>
        /// <param name="Title">为该uri增加一个标题</param>
        /// <returns></returns>
        int AddUriWithTitle(string Uri, List<string> tags,string Title);

        /// <summary>
        /// 删除一组uri
        /// </summary>
        /// <param name="Uri">待删除的uri列表</param>
        /// <param name="Delete">是否删除对应的文件</param>
        /// <returns></returns>
        int DelUris(IEnumerable<string> Uri,bool Delete);

        /// <summary>
        /// 删除URI的tag
        /// </summary>
        /// <param name="Uri"></param>
        /// <param name="tags"></param>
        /// <returns></returns>
        int DelTags(string Uri, List<string> tags);

        /// <summary>
        /// 更新URI的标题
        /// </summary>
        /// <param name="Uri"></param>
        /// <param name="Title"></param>
        /// <returns></returns>
        int UpdateTitle(string Uri, string Title);

        /// <summary>
        /// 查询获得URI列表
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        List<string> Query(string query);

        /// <summary>
        /// 数据库变更通知
        /// </summary>
        DataChanged UriDBChanged { get; set; }

        /// <summary>
        /// 获得Uri的标题
        /// </summary>
        /// <param name="Uri"></param>
        /// <returns></returns>
        string GetTitle(string Uri);

        /// <summary>
        /// 获得URI的所有tag列表
        /// </summary>
        /// <param name="Uri"></param>
        /// <returns></returns>
        List<string> GetTags(string Uri);

        /// <summary>
        /// 主动触发数据库变更通知
        /// </summary>
        void Notify();
        
    }
    public delegate void DataChanged();
}
