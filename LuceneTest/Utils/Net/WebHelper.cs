using System;
using System.IO;
using System.Net;
using System.Text.RegularExpressions;
using TagExplorer.UriInfList;

namespace TagExplorer.Utils
{



    public class WebHelper
    {
        public static string GetHtml(string url, string charSet)
        {
            string html = GetHtmlWebClient(url, charSet);
            if(string.IsNullOrEmpty(html))
            {
                html = GetHtmlHttpWebRequest(url, charSet);
            }
            return html;
        }
        private static string GetHtmlHttpWebRequest(string url, string charSet)
        {
            return null;
        }
        private static string GetHtmlWebClient(string url,string charSet)
        {
            string strWebData = string.Empty;
            try
            {
                WebClienTimeout myWebClient = new WebClienTimeout(50 * 1000); //创建WebClient实例
                myWebClient.Headers.Add("User-Agent", "Microsoft Internet Explorer");
                byte[] myDataBuffer = myWebClient.DownloadData(url);
                strWebData = System.Text.Encoding.Default.GetString(myDataBuffer);
                //获取网页字符编码描述信息 
                if (string.IsNullOrEmpty(charSet))
                {
                    Match charSetMatch = Regex.Match(strWebData, @"<meta([^><]*)(charset\s*=\s*(?<CHARSET>[a-zA-Z-0-9]+))", RegexOptions.IgnoreCase | RegexOptions.Multiline);
                    string webCharSet = charSetMatch.Groups["CHARSET"].Value.Trim().ToLower();
                    if (webCharSet != "gb2312" && webCharSet != "gbk")
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
                Logger.E(ex);
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
                string title = GetHtmlTitle(html);
                if(string.IsNullOrEmpty(title))
                {
                    int lastIdx = url.LastIndexOf('/');
                    if (lastIdx < 1) lastIdx = url.LastIndexOf('\\');

                    if (lastIdx > 1)
                    {
                        title = url.Substring(lastIdx + 1); //TODO 更好的获得http文档标题的方法
                    }
                    else
                    {
                        title = url;
                    }
                }

                if(!string.IsNullOrEmpty(title))
                {
                    title = title.Replace("\r", "");
                    title = title.Replace("\n", "");
                    title = WebUtility.HtmlDecode(title);
                }
                return title;
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
            String regex = @"<title[^>]*>(?<TITLE>.+?)</title>";

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

        /// <summary>
        /// 下载url文件，。
        /// </summary>
        /// <param name="url"></param>
        /// <param name="fileDst"></param>
        public static void Download(string url,string tag,string title)
        {
            if (PathHelper.IsValidHttp(url))
            {
                string d = CfgPath.GetDirByTag(tag,true);
                if (string.IsNullOrEmpty(title))
                {
                    title = SearchResultItem.GetTitle(url);
                }
                if (string.IsNullOrEmpty(title)) return;


                title = CfgPath.FormatPathName(title);

                string file = Path.Combine(d, title + ".note.pdf");
                string cmd = string.Format(@"""{0}"" ""{1}"" ""{2}""", 
                    CfgPath.DownlodPdfCmd(), url, file);
                PathHelper.RunCmd(cmd);
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
