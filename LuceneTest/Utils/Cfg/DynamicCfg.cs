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
        #region 配置的保存与恢复
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
        #endregion

        #region 所有动态配置属性（会在程序中动态修改，并需要保存恢复的信息）
        private string mainCanvasHeight = "50*";
        public string MainCanvasHeight { get { return mainCanvasHeight; } set { mainCanvasHeight = value;Save(); } }


        private string mainCanvasRoot = "我的大脑";
        public string MainCanvasRoot { get { return mainCanvasRoot; } set { mainCanvasRoot = value; Save(); } }

        private string subCanvasRoot = "我的大脑";
        public string SubCanvasRoot { get { return subCanvasRoot; } set { subCanvasRoot = value; Save(); } }

        private LayoutMode subCanvasLayoutMode = LayoutMode.TREE_COMPACT;
        public LayoutMode SubCanvasLayoutMode { get { return subCanvasLayoutMode; } set { subCanvasLayoutMode = value; Save(); } }

        private LayoutMode mainCanvasLayoutMode = LayoutMode.LRTREE_COMPACT_MORE;
        public LayoutMode MainCanvasLayoutMode { get { return mainCanvasLayoutMode; } set { mainCanvasLayoutMode = value; Save(); } }
        #endregion
    }
}
