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
        public void GetWebTitleTest()
        {
            string t = WebHelper.GetWebTitle("http://www.baidu.com");
            Assert.AreEqual("百度一下，你就知道", t);
        }

        [TestMethod()]
        public void GetHtmlTitleTest()
        {
            string t = WebHelper.GetHtmlTitle(@"<title>百度一下，你就知道</title>");
            Assert.AreEqual("百度一下，你就知道", t);
        }
    }
}