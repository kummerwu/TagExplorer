using Microsoft.VisualStudio.TestTools.UnitTesting;
using TagExplorer.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TagExplorer.Utils.Tests
{
    [TestClass()]
    public class WebHelperTests
    {
        [TestMethod()]
        public void WebHelper_GetWebTitleTest()
        {
            string t = WebHelper.GetWebTitle("http://www.baidu.com");
            Assert.AreEqual("百度一下，你就知道", t);
        }

        [TestMethod()]
        public void WebHelper_GetHtmlTitleTest()
        {
            string t = WebHelper.GetHtmlTitle(@"<title>百度一下，你就知道</title>");
            Assert.AreEqual("百度一下，你就知道", t);
        }

        [TestMethod()]
        public void SaveTest()
        {
            WebHelper.Save(@"https://code.msdn.microsoft.com/windowsdesktop/Creating-a-MHTML-MIME-HTML-61cf5dd1", 
                @"B:\0001-msdn.mht");
            WebHelper.Save(@"https://superuser.com/questions/369232/how-to-save-a-web-page-as-mht-in-chrome",
                @"B:\0002-superuser.mht");
            WebHelper.Save(@"https://www.baidu.com",
                @"B:\0003-baidu.mht");
            WebHelper.Save(@"http://www.sina.com",
                @"B:\0004-sina.mht");

        }
    }
}