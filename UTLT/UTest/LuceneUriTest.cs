using LuceneTest.UriMgr;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;

namespace LuceneTest.UTest
{
    [TestClass]
    public class LuceneUriTest
    {
        IUriDB db = null;
        [TestInitialize]
        public void setup()
        {
            //if (!System.IO.Directory.Exists(Cfg.Ins.TagDB))
            //{
            //    System.IO.Directory.CreateDirectory(Cfg.Ins.TagDB);
            //}
            db = UriDBFactory.CreateUriDB();
        }
        [TestCleanup]
        public void teardown()
        {
            db.Dispose();
            db = null;
            //if (System.IO.Directory.Exists(Cfg.Ins.TagDB))
            //    System.IO.Directory.Delete(Cfg.Ins.TagDB,true);
        }

        [TestMethod]
        public void test1()
        {
            db.AddUri(@"c:\a.txt");
            List<string> uris = db.Query("a");
            Assert.AreEqual(1, uris.Count);
            Assert.AreEqual(@"c:\a.txt", uris[0]);


            uris = db.Query("a.txt");
            Assert.AreEqual(1, uris.Count);
            Assert.AreEqual(@"c:\a.txt", uris[0]);

            uris = db.Query("b.txt");
            Assert.AreEqual(0, uris.Count);
        }


        [TestMethod]
        public void test2()
        {
            db.AddUri(@"c:\a.txt",new List<string>() { "tag1","tag2"});
            AssertIn(@"c:\a.txt", "tag1", "tag2", "TAG1", "TAG2","a.txt","tag");
            AssertNotIn("tag3");

            db.AddUri(@"c:\a.txt", new List<string>() { "tag1", "tag2" });
            AssertIn(@"c:\a.txt", "tag1", "tag2", "TAG1", "TAG2", "a.txt", "tag");
            AssertNotIn("tag3");

        }

        [TestMethod]
        public void test20170617_1()
        {
            db.AddUri(@"c:\a.txt", new List<string>() { "tag1"});
            AssertIn(@"c:\a.txt", "tag1");

        }
        [TestMethod]
        public void test20170617_2()//URI中有大小写的情况
        {
            db.AddUri(@"c:\a.Txt", new List<string>() { "tag1" });
            AssertIn(@"c:\a.txt", "tag1");

        }

        [TestMethod]
        public void test20170617_3()//一个失败的案例
        {
            db.AddUri(@"D:\00_工作备份\Work\ROSng软件架构及应用V1.1.pptx", new List<string>() { "parent1" });
            AssertIn(@"D:\00_工作备份\Work\ROSng软件架构及应用V1.1.pptx", "1");

        }
        [TestMethod]
        public void test20170617_3_1()//一个失败的案例
        {
            db.AddUri(@"D:\00_工作备份\Work\ROSng软件架构及应用V1.1.pptx", new List<string>() { "parent1" });
            db.AddUri(@"D:\00_工作备份\Work\ROSng软件架构及应用V1.1.pptx", new List<string>() { "parent1" });
            AssertIn(@"D:\00_工作备份\Work\ROSng软件架构及应用V1.1.pptx", "1");

        }

        [TestMethod]
        public void test20170617_4()//测试删除文件，好像案例3测试不通过的原因就是删除失败了。
        {
            db.AddUri(@"D:\00_工作备份\Work\ROSng软件架构及应用V1.1.pptx", new List<string>() { "parent1" });
            AssertIn(@"D:\00_工作备份\Work\ROSng软件架构及应用V1.1.pptx", "parent1");
            db.DelUri(@"D:\00_工作备份\Work\ROSng软件架构及应用V1.1.pptx", false);
            AssertNotIn(@"D:\00_工作备份\Work\ROSng软件架构及应用V1.1.pptx", "parent1");
        }

        [TestMethod]
        public void test20170619_1()//发现同一个文件连续添加两次会有两个文档在db中
        {
            db.AddUri(@"D:\00_工作备份\Work\ROSng软件架构及应用V1.1.pptx", new List<string>() { "parent1" });
            AssertIn(@"D:\00_工作备份\Work\ROSng软件架构及应用V1.1.pptx", "parent1");
            db.AddUri(@"D:\00_工作备份\Work\ROSng软件架构及应用V1.1.pptx", new List<string>() { "parent1" });
            AssertIn(@"D:\00_工作备份\Work\ROSng软件架构及应用V1.1.pptx", "parent1");
            db.AddUri(@"D:\00_工作备份\Work\ROSng软件架构及应用V1.1.pptx", new List<string>() { "parent1" });
            AssertIn(@"D:\00_工作备份\Work\ROSng软件架构及应用V1.1.pptx", "parent1");
        }

        [TestMethod]
        public void test20170619_2()//发现同一个文件连续添加两次会有两个文档在db中，
            //最终定位不是这个原因，是因为某个字段长度超过了50个字符，我们的解析器有问题（1-50 Ngram）
        {
            //string dir = @"D:\02-个人目录\LuceneTest\TagExplorer\DocumentBase\Doc\child4";
            string dir = @"D:\00_工作备份\Work\ROSng软件架构及应用V1.1.pptx\Doc\child4";
            string tag = "child4";
            db.AddUri(dir, new List<string>() { tag });
            AssertIn(dir, tag);
            db.AddUri(dir, new List<string>() { tag });
            AssertIn(dir, tag);
        }

        [TestMethod]
        public void test3()
        {
            string uri = @"c:\a.txt";
            db.AddUri(uri, new List<string>() { "tag1", "tag2" },"TITLE1");
            AssertIn(uri, "TITLE1", "a","a.txt","tag","tag1","tag2","TITLE","TITLE1","title1");
            AssertNotIn("b", "TITLE2","title2");
            
        }

        private void AssertIn(string uri,string title,params string[] tag)
        {
            List<string> uris = null;
            foreach(string t in tag)
            {
                uris = db.Query(t);
                Assert.AreEqual(1, uris.Count);
                Assert.AreEqual(uri, uris[0],true);
            }
            uris = db.Query(title);
            Assert.AreEqual(1, uris.Count);
            Assert.AreEqual(uri, uris[0],true);

            
        }

        private void AssertNotIn(params string[] tag)
        {
            List<string> uris = null;
            foreach (string t in tag)
            {
                uris = db.Query(t);
                Assert.AreEqual(0, uris.Count);
            }
            
        }
        [TestMethod]
        public void test4()
        {
            string uri = @"c:\a.txt";
            db.AddUri(uri, new List<string>() { "tag1", "tag2" }, "TITLE1");
            AssertIn(uri, "TITLE1","tag1", "tag2");

            db.AddUri(@"c:\a.txt", new List<string>() { "tag3", "tag4" }, "TITLE2");
            AssertIn(uri, "TITLE2","tag1", "tag2","tag3","tag4");



            db.DelUri(uri, new List<string>() { "tag1" });
            AssertNotIn("tag1");
            AssertIn(uri, "tag2", "tag3", "tag4");


            db.DelUri(uri, new List<string>() { "tag2" });
            AssertNotIn("tag1","tag2");
            AssertIn(uri, "tag3", "tag4","TITLE2");

        }
    }
}
