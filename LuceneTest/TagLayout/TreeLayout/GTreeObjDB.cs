using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace TagExplorer.TagLayout.TreeLayout
{
    class GTreeObjDB
    {
        public void Reset()
        {
            all.Clear();
            Lines.Clear();
        }
        public IEnumerable<GTreeObj> All
        {
            get { return all.Values.Cast<GTreeObj>(); }
        }
        public Hashtable all = new Hashtable();
        public GTreeObj Get(string tag)
        {
            return all[tag] as GTreeObj;
        }
        public void Add(string tag, GTreeObj obj)
        {
            if (null == Get(tag))
            {
                all.Add(tag, obj);
            }
        }
        public List<Tuple<GTreeObj, GTreeObj>> Lines = new List<Tuple<GTreeObj, GTreeObj>>();
        public void AddLine(GTreeObj parent, GTreeObj child)
        {
            Lines.Add(new Tuple<GTreeObj, GTreeObj>(parent, child));
        }
        private static GTreeObjDB ins = null;
        public static GTreeObjDB Ins
        {
            get
            {
                if (ins == null) ins = new GTreeObjDB();
                return ins;
            }
        }
    }
}
