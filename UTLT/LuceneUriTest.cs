
using TagExplorer.UriMgr;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using TagExplorer.Utils;

namespace UTLT
{
    [TestClass]
    public class LuceneUriTest
    {
        IUriDB db = null;
        [TestInitialize]
        public void setup()
        {
            UTestCfg.Ins.IsUTest = true;
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
        public static List<string> L(string s) { return new List<string>() { s }; }
        //两个URI之间有匹配关系，一个是另外一个的子串
        //先增加父串，再增加子串
        [TestMethod]
        public void TestUriMgr_PrefixSame1()
        {
            db.AddUri(new List<string>() { @"c:\aaaa" }, new List<string>() { "tag1", "tag2" });
            db.AddUri(L(@"c:\a"),new List<string>() { "tag1", "tag2" });
            Assert.AreEqual(2, db.Query("a").Count);
        }
        //两个URI之间有匹配关系，一个是另外一个的子串
        //先增加子串，再增加父串
        [TestMethod]
        public void TestUriMgr_PrefixSame2()
        {
            db.AddUri(L(@"c:\a"), new List<string>() { "tag1", "tag2" });
            db.AddUri(L(@"c:\aaaa"), new List<string>() { "tag1", "tag2" });
            
            Assert.AreEqual(2, db.Query("a").Count);
        }
        [TestMethod]
        public void TestUriMgr_Base()
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

        //TAG区分大小写
        [TestMethod]
        public void TestUriMgr_Base2()
        {
            db.AddUri(L(@"c:\a.txt"),new List<string>() { "tag1","tag2"});
            AssertIn(@"c:\a.txt", "tag1", "tag2", "TAG1", "TAG2","a.txt","tag");
            AssertNotIn("tag3");

            db.AddUri(L(@"c:\a.txt"), new List<string>() { "tag1", "tag2" });
            AssertIn(@"c:\a.txt", "tag1", "tag2", "TAG1", "TAG2", "a.txt", "tag");
            AssertNotIn("tag3");

        }

        [TestMethod]
        public void TestUriMgr_Base3()
        {
            db.AddUri(new List<string>() { @"c:\a.txt" }, new List<string>() { "tag1"});
            AssertIn(@"c:\a.txt", "tag1");

        }
        [TestMethod]
        public void TestUriMgr_Base4()//URI中有大小写的情况
        {
            db.AddUri(new List<string>() { @"c:\a.Txt" }, new List<string>() { "tag1" });
            AssertIn(@"c:\a.txt", "tag1");

        }

        [TestMethod]
        public void TestUriMgr_LongURI1()//一个失败的案例，Uri字符串超长
        {
            db.AddUri(new List<string>() { @"D:\00_工作备份\Work\ROSng软件架构及应用V1.1.pptx" }, new List<string>() { "parent1" });
            AssertIn(@"D:\00_工作备份\Work\ROSng软件架构及应用V1.1.pptx", "1");

        }
        [TestMethod]
        public void TestUriMgr_LongURI2()//一个失败的案例,Uri字符串超长
        {
            db.AddUri(new List<string>() { @"D:\00_工作备份\Work\ROSng软件架构及应用V1.1.pptx" }, new List<string>() { "parent1" });
            db.AddUri(new List<string>() { @"D:\00_工作备份\Work\ROSng软件架构及应用V1.1.pptx" }, new List<string>() { "parent1" });
            AssertIn(@"D:\00_工作备份\Work\ROSng软件架构及应用V1.1.pptx", "1");

        }

        [TestMethod]
        public void TestUriMgr_Del()//测试删除文件，好像案例3测试不通过的原因就是删除失败了。
        {
            db.AddUri(new List<string>() { @"D:\00_工作备份\Work\ROSng软件架构及应用V1.1.pptx" }, new List<string>() { "parent1" });
            AssertIn(@"D:\00_工作备份\Work\ROSng软件架构及应用V1.1.pptx", "parent1");
            db.DelUri(new List<string>() { @"D:\00_工作备份\Work\ROSng软件架构及应用V1.1.pptx" }, false);
            AssertNotIn(@"D:\00_工作备份\Work\ROSng软件架构及应用V1.1.pptx", "parent1");
        }

        [TestMethod]
        public void TestUriMgr_AddSameUri()//发现同一个文件连续添加两次会有两个文档在db中
        {
            db.AddUri(new List<string>() { @"D:\00_工作备份\Work\ROSng软件架构及应用V1.1.pptx" }, new List<string>() { "parent1" });
            AssertIn(@"D:\00_工作备份\Work\ROSng软件架构及应用V1.1.pptx", "parent1");
            db.AddUri(new List<string>() { @"D:\00_工作备份\Work\ROSng软件架构及应用V1.1.pptx" }, new List<string>() { "parent1" });
            AssertIn(@"D:\00_工作备份\Work\ROSng软件架构及应用V1.1.pptx", "parent1");
            db.AddUri(new List<string>() { @"D:\00_工作备份\Work\ROSng软件架构及应用V1.1.pptx" }, new List<string>() { "parent1" });
            AssertIn(@"D:\00_工作备份\Work\ROSng软件架构及应用V1.1.pptx", "parent1");
        }

        [TestMethod]
        public void TestUriMgr_AddSameUri2()//发现同一个文件连续添加两次会有两个文档在db中，
            //最终定位不是这个原因，是因为某个字段长度超过了50个字符，我们的解析器有问题（1-50 Ngram）
        {
            
            string dir = @"D:\00_工作备份\Work\ROSng软件架构及应用V1.1.pptx\Doc\child4";
            string tag = "child4";
            db.AddUri(new List<string>() { dir }, new List<string>() { tag });
            AssertIn(dir, tag);
            db.AddUri(new List<string>() { dir }, new List<string>() { tag });
            AssertIn(dir, tag);
        }

        [TestMethod]
        public void TestUriMgr_AddUriWithTitle()
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
        public void TestUriMgr_AddDel()
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
