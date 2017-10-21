using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Configuration;
using TagExplorer.UriMgr;
using System.Windows.Media;
using AnyTagNet;

namespace TagExplorer.Utils
{
    class AppCfg
    {

        Configuration cfa = null;
        private AppCfg()
        {
            cfa = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
        }
        
        private void Save()
        {
            cfa.Save();
        }
        private string LoadString(string Name,string DefaultValue,ref string Field)
        {
            //获得配置存储的值
            if (Field==null)
            {
                Field = cfa.AppSettings.Settings[Name]?.Value;
            }
            //如果没有存储值，使用默认值
            if (string.IsNullOrEmpty(Field))
            {
                Field = DefaultValue;
            }
            return Field;
        }
        private void SaveString(string Name,string Value,ref string Field)
        {
            bool needSave = true;
            Field = Value;
            if (cfa.AppSettings.Settings[Name] == null)
            {
                cfa.AppSettings.Settings.Add(Name, Value);
            }
            else
            {
                if (cfa.AppSettings.Settings[Name].Value != Value)
                {
                    cfa.AppSettings.Settings[Name].Value = Value;
                }
                else
                {
                    needSave = false;
                }
            }
            if (needSave)
            {
                Save();
            }
        }
        private static AppCfg ins = null;
        public static AppCfg Ins
        {
            get
            {
                if(ins==null)
                {
                    ins = new AppCfg();
                }
                return ins;
            }

        }


        ////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////

        private string mainCanvasRoot = null;
        public string MainCanvasRoot
        {
            get {return LoadString("MainCanvasRoot", Cfg.Ins.DefaultTag, ref mainCanvasRoot);}
            set { SaveString("MainCanvasRoot", value, ref mainCanvasRoot); }
        }

        private string subCanvasRoot = null;
        public string SubCanvasRoot
        {
            get { return LoadString("SubCanvasRoot", MainCanvasRoot, ref subCanvasRoot); }
            set { SaveString("SubCanvasRoot", value, ref subCanvasRoot); }
        }

        private string fontName = null;
        public string FontName
        {
            get { return LoadString("FontName", CfgTagGraph.GFontName, ref fontName); }
            set { SaveString("FontName", value, ref fontName); }
        }

        private string mainCanvasHeight = null;
        public string MainCanvasHeight
        {
            get { return LoadString("MainCanvasHeight", "50*", ref mainCanvasHeight); }
            set { SaveString("MainCanvasHeight", value, ref mainCanvasHeight); }
        }
        private string subCanvasHeight = null;
        public string SubCanvasHeight
        {
            get { return LoadString("SubCanvasHeight", "50*", ref mainCanvasHeight); }
            set { SaveString("SubCanvasHeight", value, ref mainCanvasHeight); }
        }
        //////////////////////////////////////////////////////////////////////////////////////
        private static Color C(int c) { return Color.FromRgb((byte)((c & 0xFF0000) >> 16), (byte)((c & 0xFF00) >> 8), (byte)(c & 0xFF)); }
        

        private Color[] ColorsParse(string s)
        {
            string []cs = s.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            Color[] all = new Color[cs.Length];
            for(int i = 0;i<all.Length;i++)
            {
                all[i] = (Color)ColorConverter.ConvertFromString(cs[i]) ;
            }
            return all;

        }
        private string ColorsToString(Color[] cs)
        {
            string s = "";
            foreach (Color c in cs)
            {
                s += " " + c.ToString();
            }
            return s.Trim();

        }
        
        private Color[] LoadColors(string Name,Color[] DefaultValue,ref Color[] Field)
        {
            if(Field==null)
            {
                string sColors = null;
                sColors = LoadString(Name, "", ref sColors);
                if (string.IsNullOrEmpty(sColors))
                {
                    Field = DefaultValue;
                    SaveColors(Name, DefaultValue, ref Field);
                }
                else
                {
                    Field = ColorsParse(sColors);
                }
            }
            return Field;
        }
        
        private void SaveColors(string Name, Color[] Value, ref Color[] Field)
        {
            Field = Value;
            string sColor = null;
            SaveString(Name, ColorsToString(Field), ref sColor);
        }
        //////////////////////////////////////////////////////////////////////////////////////
        public static Color[] TagBoxBackColorDefault = new Color[] {
            C(0xFF6666),            C(0x99CC00),            C(0x99CCFF),            C(0xFFD39B),
            C(0xFF99CC),            C(0x9933FF),            C(0x0066CC),
        };
        
        private Color[] tagBoxBackColor = null;
        public Color[] TagBoxBackColor
        {
            get{return LoadColors("TagBoxBackColors", TagBoxBackColorDefault, ref tagBoxBackColor);}
            set{SaveColors("TagBoxBackColors", value, ref tagBoxBackColor);}
        }

        public static Color[] TagBoxForeColorDefault = new Color[] {
            C(0x000000),            C(0x000000),            C(0x000000),            C(0x000000),
            C(0x000000),            C(0xFFFFFF),            C(0xFFFFFF),
        };
        private Color[] tagBoxForeColor = null;
        public Color[] TagBoxForeColor
        {
            get { return LoadColors("TagBoxForeColors", TagBoxForeColorDefault, ref tagBoxForeColor); }
            set { SaveColors("TagBoxForeColors", value, ref tagBoxForeColor); }
        }
    }
}
