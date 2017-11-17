using System.Collections.Generic;
using System.IO;
using TagExplorer.Utils;

namespace TagExplorer.TagCanvas
{
    class TagVirtualDir
    {
        private static TagVirtualDir ins = null;
        public static TagVirtualDir Ins
        {
            get
            {
                if (ins == null) ins = new TagVirtualDir();
                return ins;
            }
        }
        private List<string> TagHistory = new List<string>();
        public void KeepVDir(string tag)
        {
            //检查功能开关是否关闭，关闭直接返回
            if (!StaticCfg.Ins.Opt.KeepVDir) return;

            //一个新的tag，添加在头部
            if (!TagHistory.Contains(tag))
            {
                while (TagHistory.Count >= StaticCfg.Ins.MAX_TAG_VDIR)
                {
                    TagHistory.RemoveAt(0);
                }
                TagHistory.Add(tag);
            }
            //一个已有的tag，换一下位置
            else
            {
                TagHistory.Remove(tag);
                TagHistory.Add(tag);
            }

            //新建虚拟目录
            foreach (string t in TagHistory)
            {
                string tagVDir = CfgPath.GetVDirByTag(t);
                string tagDir = CfgPath.GetDirByTag(t);
                PathHelper.LinkDir(tagVDir, tagDir);
            }

            //如果溢出的话，淘汰老的目录
            DirectoryInfo vroot = new DirectoryInfo(CfgPath.VDir);
            DirectoryInfo[] vdirs = vroot.GetDirectories();
            int vDirCount = vdirs.Length;
            foreach (DirectoryInfo v in vdirs)
            {
                if (!TagHistory.Contains(v.Name) && vDirCount > StaticCfg.Ins.MAX_TAG_VDIR)
                {
                    v.Delete();
                    vDirCount--;
                }
            }


        }
    }
}
