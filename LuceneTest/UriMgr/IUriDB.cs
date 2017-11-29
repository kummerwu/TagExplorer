using Newtonsoft.Json;
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
        int MoveUris(string[] SrcUri, string[] DstUri, string NewTag);
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
        URIItem GetInf(string Uri);

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

        //版本升级时使用该函数导入老版本的数据。
        int Import(string importFile);
        int Export(string exportFile);
    }
    public delegate void DataChanged();

    public class URIItem
    {
        //public Guid ID;
        public string Key;
        public string Uri;
        public string Title;
        public List<string> Tags = new List<string>();
        public DateTime CreateTime;
        public DateTime AccessTime;

        public const string F_ID = "fguid";
        public const string F_KEY = "key";
        public const string F_URI = "furi";
        public const string F_URI_TITLE = "ftitle";
        public const string F_URI_TAGS = "ftags";
        public const string F_CREATE_TIME = "fctime";
        public const string F_ACCESS_TIME = "fatime";

        [JsonIgnore]
        public string[] SearchFields = { F_KEY, F_URI, F_URI_TAGS, F_URI_TITLE };

        internal bool IsSame(URIItem oTag)
        {
            if (oTag.Key != Key) return false;
            if (oTag.Uri != Uri) return false;
            if (oTag.Title != Title) return false;
            //时间不参与比较，否则每次导入都需要有大量的更新，其实没有必要
            //if (oTag.CreateTime != CreateTime) return false;
            //if (oTag.AccessTime != AccessTime) return false;
            if (oTag.Tags.Count != Tags.Count) return false;
            for(int i = 0;i<Tags.Count;i++)
            {
                if (oTag.Tags[i] != Tags[i]) return false;
            }
            return true;
        }
        private void AddTag(string tag)
        {
            if(!Tags.Contains(tag))
            {
                Tags.Add(tag);
            }
        }
        internal static URIItem MergeTag(URIItem iTag, URIItem oTag)
        {
            URIItem it = new URIItem();
            it.Key = iTag.Key;
            it.Uri = string.IsNullOrEmpty(iTag.Uri)?oTag.Uri:iTag.Uri;
            it.Title = string.IsNullOrEmpty(iTag.Title) ? oTag.Title : iTag.Title;
            it.CreateTime = iTag.CreateTime > oTag.CreateTime ? iTag.CreateTime : oTag.CreateTime;
            it.AccessTime = iTag.AccessTime > oTag.AccessTime ? iTag.AccessTime : oTag.AccessTime;
            foreach(string s in iTag.Tags)
            {
                it.AddTag(s);
            }
            foreach(string s in oTag.Tags)
            {
                it.AddTag(s);
            }
            return it;


        }
    }
}
