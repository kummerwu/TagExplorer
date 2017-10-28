using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;
using TagExplorer.Utils;

namespace UTLT
{
    [TestClass]
    public class UnitTest1
    {
        [AssemblyInitialize()]
        public static void AssemblyInit(TestContext context)
        {
            UTestCfg.Ins.IsUTest = true;
            string d1 = CfgPath.DocBasePath;
            d1 = CfgPath.DocDir;
        }
        [TestInitialize]
        public void setup()
        {
            
        }
        [TestCleanup]
        public void teardown()
        {
            //Directory.Delete(Cfg.Ins.Root,true)
            if (Directory.Exists(@"B:\00TagExplorerBase"))
            {
                Directory.Delete(@"B:\00TagExplorerBase", true);//这边写死是防止危险，单元测试不能把正事的文档删除了
            }
        }
        private static void AssertFilter(bool r,string s)
        {
            Assert.AreEqual(r, CfgPath.NeedSkip(s));
        }
        [TestMethod]
        public void TestUtil_FileWatcherFilter()
        {
            AssertFilter(true, @"B:\00TagExplorerBase\DocumentBase\Doc\PAAS\20160108-Paas和组件化\参考资料\cloudfoundry\Cloud Foundry技术全貌及核心组件分析_files");
            AssertFilter(true, @"B:\00TagExplorerBase\DocumentBase\Doc\架构设计\微服务设计\~$BGP微服务v1.2.pptx");
            AssertFilter(false, @"B:\00TagExplorerBase\DocumentBase\Doc\架构设计\微服务设计\BGP微服务v1.2.pptx");
            AssertFilter(false, @"B:\00TagExplorerBase\DocumentBase\Doc\PAAS\20160108-Paas和组件化\参考资料\cloudfoundry\Cloud Foundry技术全貌及核心组件分析_files.pdf");
        }
        [TestMethod]
        public void TestUtil_GetTag()
        {
            AssertTag(@"B:\00TagExplorerBase\DocumentBase\Doc\编程技术\[C语言标准]ansi_c.pdf", "编程技术");
            AssertTag(@"B:\00TagExplorerBase\DocumentBase\Doc\二层协议\基于EAPS技术的以太网单环路保护技术要求.pdf", "二层协议");
            AssertTag(@"B:\00TagExplorerBase\DocumentBase\Doc\汇报素材\代码月走查", "汇报素材");
            AssertTag(@"B:\00TagExplorerBase\DocumentBase\Doc\汇报素材\代码月走查\", "汇报素材");

        }

        [TestMethod]
        public void TestUtil_GetTag2()
        {
            AssertTag(@"c:\00TagExplorerBase", null);
            AssertTag(@"B:\00TagExplorerBase", null);
            AssertTag(@"B:\00TagExplorerBase\", null);
            AssertTag(@"B:\00TagExplorerBase\DocumentBase", null);
            AssertTag(@"B:\00TagExplorerBase\DocumentBase\", null);
            AssertTag(@"B:\00TagExplorerBase\DocumentBase\Doc", null);
            AssertTag(@"B:\00TagExplorerBase\DocumentBase\Doc\", null);
            AssertTag(@"B:\00TagExplorerBase\DocumentBase\Doc\汇报素材\代码月走查\test", null);
            AssertTag(@"B:\00TagExplorerBase\DocumentBase\Doc\汇报素材\代码月走查\test.pdf", null);

            

        }

        [TestMethod]
        public void TestUtil_GetTag3()
        {
            AssertTag(@"B:\00TagExplorerBase\DocumentBase\Doc\ROSng微服务\20170714 - 吴道揆 - ROSng微服务相关工作~3AC25E.tmp", "ROSng微服务");
        }
        private static  void AssertTag(string file,string tag)
        {
            //createdir(file);
            Assert.AreEqual(tag, CfgPath.GetTagByPath(file));

        }

        
    }
}
