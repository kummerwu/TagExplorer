using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using AnyTags.Net;
using LuceneTest.UriMgr;
using System.IO;

namespace UTLT
{
    [TestClass]
    public class UnitTest1
    {
        //[TestMethod]
        public void TestUtil_GetID()
        {
            string f = @"D:\00TagExplorerBase\DocumentBase\Doc\工具集合\dddd";
            string lastF = f;
            Guid id = NtfsFileID.GetID(f);
            for(int i = 0;i<100;i++)
            {
                lastF = f + i;
                Directory.Move(f, lastF);
                Assert.AreEqual(id, NtfsFileID.GetID(lastF));
                f = lastF;
            }
        }

        private static void AssertFilter(bool r,string s)
        {
            Assert.AreEqual(r, MyPath.FileWatcherFilter(s));
        }
        [TestMethod]
        public void TestUtil_FileWatcherFilter()
        {
            AssertFilter(true, @"D:\00TagExplorerBase\DocumentBase\Doc\PAAS\20160108-Paas和组件化\参考资料\cloudfoundry\Cloud Foundry技术全貌及核心组件分析_files");
            AssertFilter(true, @"D:\00TagExplorerBase\DocumentBase\Doc\架构设计\微服务设计\~$BGP微服务v1.2.pptx");
            AssertFilter(false, @"D:\00TagExplorerBase\DocumentBase\Doc\架构设计\微服务设计\BGP微服务v1.2.pptx");
            AssertFilter(false, @"D:\00TagExplorerBase\DocumentBase\Doc\PAAS\20160108-Paas和组件化\参考资料\cloudfoundry\Cloud Foundry技术全貌及核心组件分析_files.pdf");
        }
        [TestMethod]
        public void TestUtil_GetTag()
        {
            AssertTag(@"D:\00TagExplorerBase\DocumentBase\Doc\编程技术\[C语言标准]ansi_c.pdf", "编程技术");
            AssertTag(@"D:\00TagExplorerBase\DocumentBase\Doc\二层协议\基于EAPS技术的以太网单环路保护技术要求.pdf", "二层协议");
            AssertTag(@"D:\00TagExplorerBase\DocumentBase\Doc\汇报素材\代码月走查", "汇报素材");
            AssertTag(@"D:\00TagExplorerBase\DocumentBase\Doc\汇报素材\代码月走查\", "汇报素材");

        }

        [TestMethod]
        public void TestUtil_GetTag2()
        {
            AssertTag(@"c:\00TagExplorerBase", null);
            AssertTag(@"D:\00TagExplorerBase", null);
            AssertTag(@"D:\00TagExplorerBase\", null);
            AssertTag(@"D:\00TagExplorerBase\DocumentBase", null);
            AssertTag(@"D:\00TagExplorerBase\DocumentBase\", null);
            AssertTag(@"D:\00TagExplorerBase\DocumentBase\Doc", null);
            AssertTag(@"D:\00TagExplorerBase\DocumentBase\Doc\", null);
            AssertTag(@"D:\00TagExplorerBase\DocumentBase\Doc\汇报素材\代码月走查\test", null);
            AssertTag(@"D:\00TagExplorerBase\DocumentBase\Doc\汇报素材\代码月走查\test.pdf", null);

        }
        private static  void AssertTag(string file,string tag)
        {
            Assert.AreEqual(tag, MyPath.GetTagByPath(file));
        }
    }
}
