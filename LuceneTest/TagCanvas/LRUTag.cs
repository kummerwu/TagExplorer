using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TagExplorer.UriMgr;
using TagExplorer.Utils;

namespace LuceneTest.TagCanvas
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
            File.WriteAllText(PathHelper.IniFilePath, sb.ToString().Trim());
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
        public void Load()
        {
            if (File.Exists(PathHelper.IniFilePath))
            {
                tags.AddRange(File.ReadAllLines(PathHelper.IniFilePath));
            }
        }
        public void Add(string tag)
        {
            if(tags.Contains(tag))
            {
                tags.Remove(tag);
            }
            else if(tags.Count>=Cfg.Ins.LRU_MAX_CNT)
            {
                tags.RemoveAt(0);
            }
            
            tags.Add(tag);
        }
        
    }
}
