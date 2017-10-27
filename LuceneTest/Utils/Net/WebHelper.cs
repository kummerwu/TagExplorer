using System;
using System.Net;
using System.Text.RegularExpressions;

namespace TagExplorer.Utils
{



    public class WebHelper
    {
        public static string GetHtml(string url, string charSet)
        {
            string strWebData = string.Empty;
            try
            {
                WebClienTimeout myWebClient = new WebClienTimeout(50*1000); //创建WebClient实例
                byte[] myDataBuffer = myWebClient.DownloadData(url);
                strWebData = System.Text.Encoding.Default.GetString(myDataBuffer);
                //获取网页字符编码描述信息 
                if (string.IsNullOrEmpty(charSet))
                {
                    Match charSetMatch = Regex.Match(strWebData, "<meta([^>]*)charset=(\")?(.*)?\"", RegexOptions.IgnoreCase | RegexOptions.Multiline);
                    string webCharSet = charSetMatch.Groups[3].Value.Trim().ToLower();
                    if (webCharSet != "gb2312" && webCharSet!= "gbk" )
                    {
                        webCharSet = "utf-8";
                    }
                    if (System.Text.Encoding.GetEncoding(webCharSet) != System.Text.Encoding.Default)
                    {
                        strWebData = System.Text.Encoding.GetEncoding(webCharSet).GetString(myDataBuffer);
                    }
                }
            }
            catch (Exception ex)
            {
                return null;
            }
            return strWebData;
        }

        public static string GetWebTitle(String url)
        {
            try
            {
                //请求资源  
                string html = GetHtml(url, null);
                return GetHtmlTitle(html);
            }
            catch (Exception e)
            {
                Logger.E(e);
                return null;
            }
        }

        public static string GetHtmlTitle(string str)
        {
            
            //建立获取网页标题正则表达式  
            String regex = @"<title>(?<TITLE>.+)</title>";

            //返回网页标题  
            Match m = Regex.Match(str, regex,RegexOptions.IgnoreCase|RegexOptions.Singleline);
            if (m != null && m.Groups["TITLE"] != null)
            {
                return m.Groups["TITLE"].Value.Trim();
            }
            else
            {
                return null;
            }
        }

        public static void Save(string url,string dst)
        {
            //CDO.Message msg = new CDO.Message();
            
            //msg.CreateMHTMLBody(url, CDO.CdoMHTMLFlags.cdoSuppressAll, "", "");

            //msg.GetStream().SaveToFile(dst, ADODB.SaveOptionsEnum.adSaveCreateOverWrite);
        }
    }

    public class WebClienTimeout : WebClient
    {
        /// <summary>  
        /// 过期时间  
        /// </summary>  
        public int Timeout { get; set; }

        public WebClienTimeout(int timeout)
        {
            Timeout = timeout;
        }

        /// <summary>  
        /// 重写GetWebRequest,添加WebRequest对象超时时间  
        /// </summary>  
        /// <param name="address"></param>  
        /// <returns></returns>  
        protected override WebRequest GetWebRequest(Uri address)
        {
            HttpWebRequest request = (HttpWebRequest)base.GetWebRequest(address);
            request.Timeout = Timeout;
            request.ReadWriteTimeout = Timeout;
            return request;
        }
    }
}
