using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using TagExplorer.UriMgr;
using TagExplorer.Utils;

namespace TagExplorer
{
    public class LRUTag : IDisposable
    {
        public void Dispose()
        {
            Save();
            _ins = null;
        }
        public List<string> GetTags()
        {
            List<string> ret = new List<string>();
            ret.AddRange(tags);
            ret.Reverse();
            return ret;
        }
        private void Save()
        {
            StringBuilder sb = new StringBuilder();
            foreach (string tag in tags)
            {
                sb.AppendLine(tag);
            }
            File.WriteAllText(CfgPath.IniFilePath, sb.ToString().Trim());
        }

        private static LRUTag _ins = null;
        public static LRUTag Ins
        {
            get
            {
                if(_ins==null)
                {
                    _ins = IDisposableFactory.New<LRUTag>(new LRUTag());
                    _ins.Load();
                }
                return _ins;
            }
        }
        private List<string> tags = new List<string>();
        public string DefaultTag
        {
            get
            {
                if (tags.Count > 0)
                {
                    return tags[tags.Count - 1];
                }
                else
                {
                    return StaticCfg.Ins.DefaultTag;
                }
            }
        }
        public void Load()
        {
            if (File.Exists(CfgPath.IniFilePath))
            {
                tags.AddRange(File.ReadAllLines(CfgPath.IniFilePath));
            }
        }
        public void Add(string tag)
        {
            if(tags.Contains(tag))
            {
                tags.Remove(tag);
            }
            else if(tags.Count>=StaticCfg.Ins.LRU_MAX_CNT)
            {
                tags.RemoveAt(0);
            }
            
            tags.Add(tag);
        }
        
    }
}
