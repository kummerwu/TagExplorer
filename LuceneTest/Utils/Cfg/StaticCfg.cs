using Newtonsoft.Json;
using System;
using System.IO;
using System.Windows;
using System.Windows.Media;

namespace TagExplorer.Utils
{
    public class StaticCfg
    {
        [JsonIgnore]
        public  const string CURRENT_VERSION = "1.1";

        #region 配置的保存与恢复
        private static StaticCfg ins = null;
        public static StaticCfg Ins
        {
            get
            {
                //先尝试从文件读取
                if(ins==null)
                {
                    try
                    {
                        if (File.Exists(CfgPath.StaticCfg))
                        {
                            string tmp = File.ReadAllText(CfgPath.StaticCfg);
                            ins = JsonConvert.DeserializeObject<StaticCfg>(tmp);
                            if(ins!=null && ins.Version!=CURRENT_VERSION)
                            {
                                ins.Save();
                            }
                        }
                    }catch(Exception e)
                    {
                        Logger.E(e);
                        ins = null;
                    }
                }
                //如果文件读取失败，尝试使用默认值
                if(ins == null)
                {
                    ins = new StaticCfg();//JsonConvert.DeserializeObject<CfgTagGraph>(App)
                    ins.Save();
                }
                return ins;
            }
        }

        private void Save()
        {
            this.Version = CURRENT_VERSION;
            string tmp = JsonConvert.SerializeObject(this, Formatting.Indented); //将默认值保存到json中去
            File.WriteAllText(CfgPath.StaticCfg, tmp);
        }
        #endregion

        #region 静态配置参数列表
        public string Version = CURRENT_VERSION;
        //在图中显示两个tag之间的边时，使用Split将
        public char ParentChildSplit = '`';
        public string SpecialChar = "<>《》~?";
        
        public double FontSize = 18;
        public double MinFontSize = 8;

        //Tag有向图线的粗细
        public double StrokeThickness = 1.2;
        public DoubleCollection StrokeDashArray = new DoubleCollection() { 2, 2 };

        //Tag有向图的大致比例关系，边缘空白大小
        public Size Radio = new Size(40, 40);
        
        public double RADIO = 0.5;

        //每一层Tag显示大小缩放（底层显示更小一些）
        public double ScaleInRadio = 1.1;
        public double InnerBoxXPadding_MAX = 30;
        public double InnerBoxYPadding_MAX = 24/2;
        public double InnerBoxXPadding_MIN = 20;
        public double InnerBoxYPadding_MIN = 18/2;

        public double XContentPadding = 10/2;
        public double YContentPadding = 6/2;
        public double LayoutXPadding = 10/2;
        public double LayoutYPadding = 10/2;

        //最小显示几层，最多几层，最少显示多少个tag
        public int MinLevel = 4;
        public int CurLevel = 4;
        public int MaxLevel = 8;
        public int MinTagCnt = 80;

        
        public int TAG_MAX_RELATION = 1000;
        public int LRU_MAX_CNT = 8;
        public int MAX_TAG_VDIR = 6;

        
        public string DefaultTag = "我的大脑";
        public Guid DefaultTagID = Guid.Empty;
        public string DefaultNewTag = "请输入标签名称";

        public int LR_TREE_LAYOUT_MAX_LEVEL = 2;
        public int TREE_LAYOUT_MAX_LEVEL = 4;

        


        


        public string RootDir = @"J:\00TagExplorerBase";
        
        



        //字体:存储字符串，转换为FontFamily
        public string GFontName = @"Microsoft YaHei";
        //不需要保存的配置
        private FontFamily gfontf = null;
        [JsonIgnore]
        public FontFamily GFontF
        {
            get
            {
                if(gfontf==null)
                {
                    gfontf = new FontFamily(GFontName);
                }
                return gfontf;
            }
        }
        #endregion

        #region Tag显示颜色相关的配置
        public Color[] TagBoxBackColor = new Color[] {
            C(0xFF6666),            C(0x99CC00),            C(0x99CCFF),            C(0xFFD39B),
            C(0xFF99CC),            C(0x9933FF),            C(0x0066CC),
        };

        public Color[] TagBoxForeColor = new Color[] {
            C(0x000000),            C(0x000000),            C(0x000000),            C(0x000000),
            C(0x000000),            C(0xFFFFFF),            C(0xFFFFFF),
        };
        #endregion

        #region 功能开关
        public class FuncOpt
        {
            public bool KeepVDir = false;
            public bool AutoDownloadUrl = false;
            public bool ShowMainCanvasRootPath = true;
            public bool ShowSubCanvasRootPath = true;
            public bool SqliteTagCacheOn = false;
        }
        public FuncOpt Opt = new FuncOpt();
        #endregion

        #region 辅助函数
        //颜色配置：
        private static Color C(int c) { return Color.FromRgb((byte)((c & 0xFF0000) >> 16), (byte)((c & 0xFF00) >> 8), (byte)(c & 0xFF)); }
        private Color[] ColorsParse(string s)
        {
            string[] cs = s.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            Color[] all = new Color[cs.Length];
            for (int i = 0; i < all.Length; i++)
            {
                all[i] = (Color)ColorConverter.ConvertFromString(cs[i]);
            }
            return all;

        }
        #endregion

    }


}
