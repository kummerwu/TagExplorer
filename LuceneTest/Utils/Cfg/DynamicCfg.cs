using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TagExplorer.TagLayout.CommonLayout;
using TagExplorer.TagMgr;

namespace TagExplorer.Utils.Cfg
{
    public class DynamicCfg
    {
        private static DynamicCfg ins = null;
        public static DynamicCfg Ins
        {
            get
            {
                //先尝试从文件读取
                if (ins == null)
                {
                    try
                    {
                        if (File.Exists(CfgPath.DynamicCfg))
                        {
                            string tmp = File.ReadAllText(CfgPath.DynamicCfg);
                            ins = JsonConvert.DeserializeObject<DynamicCfg>(tmp);
                        }
                    }
                    catch (Exception e)
                    {
                        Logger.E(e);
                        ins = null;
                    }
                }
                //如果文件读取失败，尝试使用默认值
                if (ins == null)
                {
                    ins = new DynamicCfg();//JsonConvert.DeserializeObject<CfgTagGraph>(App)
                    ins.Save();
                }
                return ins;
            }
        }

        public void Save()
        {
            string tmp = JsonConvert.SerializeObject(this, Formatting.Indented); //将默认值保存到json中去
            File.WriteAllText(CfgPath.DynamicCfg, tmp);
        }

        public string MainCanvasHeight = "50*";
        public void ChangeMainCanvasHeight(string v)
        {
            MainCanvasHeight = v;
            Save();
        }

        public string MainCanvasRoot = "我的大脑";
        public void ChangeMainCanvasRoot(GUTag v)
        {
            MainCanvasRoot = v.ToString();
            Save();
        }
        public string SubCanvasRoot = "我的大脑";
        public void ChangeSubCanvasRoot(GUTag v)
        {
            SubCanvasRoot = v.ToString();
            Save();
        }

        public LayoutMode SubCanvasLayoutMode = LayoutMode.TREE_COMPACT;
        public void ChangeSubCanvasLayoutMode(LayoutMode v)
        {
            SubCanvasLayoutMode = v;
            Save();
        }

        public LayoutMode MainCanvasLayoutMode = LayoutMode.LRTREE_COMPACT_MORE;
        public void ChangeMainCanvasLayoutMode(LayoutMode v)
        {
            MainCanvasLayoutMode = v;
            Save();
        }
    }
}
