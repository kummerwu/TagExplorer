﻿using System;
using System.Collections.Generic;

namespace TagExplorer.UriMgr
{
    public interface IUriDB:IDisposable
    {
        int AddUri(string Uri);
        int AddUri(IEnumerable<string> Uris, List<string> tags);
        int AddUri(string Uri, List<string> tags,string Title);
        int DelUri(IEnumerable<string> Uri,bool Delete);
        int DelUri(string Uri, List<string> tags);
        int UpdateUri(string Uri, string Title);
        List<string> Query(string query);
        DataChanged UriDBChanged { get; set; }
        string GetTitle(string Uri);
        List<string> GetTags(string Uri);
        void Notify();
        
    }
    public delegate void DataChanged();
}
