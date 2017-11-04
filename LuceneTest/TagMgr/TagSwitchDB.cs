using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TagExplorer.TagMgr
{
    public delegate void SwitchChanged();
    class TagSwitchDB
    {
        public SwitchChanged SwitchChanged { get; set; }
        Hashtable switchs = new Hashtable();
        public void Swtich(GUTag tag)
        {
            if (switchs[tag] == null)
            {
                switchs[tag] = false;
            }
            else
            {
                bool s = (bool)switchs[tag];
                switchs.Remove(tag);
                switchs[tag] = !s;
            }
            if (null != SwitchChanged) SwitchChanged();
        }

        public bool Get(GUTag tag)
        {
            object o = switchs[tag];
            if(o==null)
            {
                return true;
            }
            else
            {
                return (bool)o;
            }
        }


        private static TagSwitchDB ins;
        public static TagSwitchDB Ins
        {
            get
            {
                if(ins==null)
                {
                    ins = new TagSwitchDB();
                }
                return ins;
            }
        }
    }
}
