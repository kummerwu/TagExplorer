﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Configuration;
using TagExplorer.UriMgr;

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
            Field = Value;
            if (cfa.AppSettings.Settings[Name] == null)
            {
                cfa.AppSettings.Settings.Add(Name, Value);
            }
            else
            {
                cfa.AppSettings.Settings[Name].Value = Value;
            }
            Save();
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
    }
}