using TagExplorer.TagMgr;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using TagExplorer.Utils;

namespace UTLT
{
    [TestClass]
    public class LuceneTest
    {
        ITagDB db = null;
        [TestInitialize]
        public void setup()
        {
            Cfg.Ins.IsUTest = true;
            //if (!System.IO.Directory.Exists(Cfg.Ins.TagDB))
            //{
            //    System.IO.Directory.CreateDirectory(Cfg.Ins.TagDB);
            //}
            db = TagDBFactory.CreateTagDB();
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
        public void TestTag_AddBase()//简单添加
        {
            db.AddTag("p", "c1");
           
            List<string> c = db.QueryTagChildren("p");
            Assert.AreEqual(1, c.Count);
            Assert.AreEqual("c1", c[0]);
        }

        [TestMethod]
        public void TestTag_AddDel()//添加后删除
        {
            db.AddTag("p", "c1");

            List<string>a = db.QueryTagAlias("p");
            Assert.AreEqual(1, a.Count);
            Assert.AreEqual("p", a[0]);

            db.RemoveTag("p");
            a = db.QueryTagAlias("p");
            Assert.AreEqual(0, a.Count);

        }

        [TestMethod]
        public void TestTag_Remove()//添加后删除
        {
            db.AddTag("p", "c1");

            List<string> a = db.QueryTagAlias("c1");
            Assert.AreEqual(1, a.Count);
            Assert.AreEqual("c1", a[0]);

            db.RemoveTag("c1");
            a = db.QueryTagAlias("c1");
            Assert.AreEqual(0, a.Count);

            Assert.AreEqual(0,db.QueryTagParent("c1").Count);

        }

        [TestMethod]
        public void TestTag_AddMultiChildren()//父节点，有多个子节点
        {
            Logger.D("0");
            db.AddTag("p", "c1");
            List<string> c = db.QueryTagChildren("p");
            Assert.AreEqual(1, c.Count);
            Assert.AreEqual("c1", c[0]);


            db.AddTag("p", "c2");
            c = db.QueryTagChildren("p");
            Assert.AreEqual(2, c.Count);
            Assert.AreEqual("c1", c[0]);
            Assert.AreEqual("c2", c[1]);
        }

        [TestMethod]
        public void TestTag_AddMultiParent()//子节点，有多个父节点
        {
            Logger.D("0");
            db.AddTag("p1", "c");
            List<string> p = db.QueryTagParent("c");
            Assert.AreEqual(1, p.Count);
            Assert.AreEqual("p1", p[0]);


            db.AddTag("p2", "c");
            p = db.QueryTagParent("c");
            Assert.AreEqual(2, p.Count);
            Assert.AreEqual("p1", p[0]);
            Assert.AreEqual("p2", p[1]);
        }

        [TestMethod]
        public void TestTag_Add100Parent()//一个节点添加100个父节点
        {
            int i = 0;
            for (i = 0; i < 100; i++)
            {
                db.AddTag("p"+i, "c");
                List<string> p = db.QueryTagParent("c");
                Assert.AreEqual(i+1, p.Count);
                for (int j = 0; j <= i; j++)
                {
                    Assert.AreEqual("p"+j, p[j]);
                }
            }

            
        }


        [TestMethod]
        public void TestTag_Add100Child()//一个父节点，添加100个子节点
        {
            int i = 0;
            for (i = 0; i < 100; i++)
            {
                db.AddTag("p" , "c"+i);
                List<string> c = db.QueryTagChildren("p");
                Assert.AreEqual(i + 1, c.Count);
                for (int j = 0; j <= i; j++)
                {
                    Assert.AreEqual("c" + j, c[j]);
                }
            }


        }

        [TestMethod]
        public void TestTag_AddAlias()//添加别名
        {
            db.AddTag("p1", "c1");
            List<string> alias = db.QueryTagAlias("p1");
            Assert.AreEqual(1, alias.Count);
            Assert.AreEqual("p1", alias[0]);

            db.MergeAliasTag("p1", "p2");
             alias = db.QueryTagAlias("p1");
            Assert.AreEqual(2, alias.Count);
            Assert.AreEqual(true, alias.Contains("p1"));
            Assert.AreEqual(true, alias.Contains("p2"));
        }
        [TestMethod]
        public void TestTag_Reopen()//关闭后重新打开
        {
            Logger.D("begin test reopen");
            db.AddTag("p1", "c1");
            List<string> alias = db.QueryTagAlias("p1");
            foreach(string a in alias)
            {
                Logger.D(a);
            }
            Assert.AreEqual(1, alias.Count);
            Assert.AreEqual("p1", alias[0]);
            Logger.D("end test reopen");

            //db = TagDBFactory.CreateTagDB();
            //alias = db.QueryTagAlias("p1");
            //Assert.AreEqual(1, alias.Count);
            //Assert.AreEqual("p1", alias[0]);
        }
        [TestMethod]
        public void TestTag_AliasAddAndQuery()
        {
            db.MergeAliasTag("p1", "p2");
            List<string>alias = db.QueryTagAlias("p1");
            Assert.AreEqual(2, alias.Count);
            Assert.AreEqual(true, alias.Contains("p1"));
            Assert.AreEqual(true, alias.Contains("p2"));

            alias = db.QueryTagAlias("p2");
            Assert.AreEqual(2, alias.Count);
            Assert.AreEqual(true, alias.Contains("p1"));
            Assert.AreEqual(true, alias.Contains("p2"));
        }

        [TestMethod]
        public void TestTag_AliasAddAndQuery2()
        {
            db.AddTag("p1", "c1");
            db.AddTag("p2", "c2");
            db.MergeAliasTag("p1", "p2");
            List<string>alias = db.QueryTagAlias("p1");
            Assert.AreEqual(2, alias.Count);
            Assert.AreEqual(true, alias.Contains("p1"));
            Assert.AreEqual(true, alias.Contains("p2"));

            alias = db.QueryTagAlias("p2");
            Assert.AreEqual(2, alias.Count);
            Assert.AreEqual(true, alias.Contains("p1"));
            Assert.AreEqual(true, alias.Contains("p2"));

            List<string> child = db.QueryTagChildren("p1");
            Assert.AreEqual(2, child.Count);
            Assert.AreEqual(true, child.Contains("c1"));
            Assert.AreEqual(true, child.Contains("c2"));


            child = db.QueryTagChildren("p2");
            Assert.AreEqual(2, child.Count);
            Assert.AreEqual(true, child.Contains("c1"));
            Assert.AreEqual(true, child.Contains("c2"));
        }
        public static void AssertListEqual(List<string> expect,List<string> real)
        {
            Assert.AreEqual(expect.Count, real.Count);
            foreach(string e in expect)
            {
                Assert.AreEqual(true,real.Contains(e));
            }

        }
        [TestMethod]
        public void TestTag_SetRelation()//多个切换到一个(不存在的)
        {
            db.AddTag("P1", "C1");
            db.AddTag("P2", "C1");
            List<string> parents = db.QueryTagParent("C1");
            AssertListEqual(parents, new List<string>() { "P1", "P2" });

            db.ResetRelationOfChild("P3", "C1");
            parents = db.QueryTagParent("C1");
            AssertListEqual(parents, new List<string>() { "P3" });
        }

        [TestMethod]
        public void TestTag_SetRelation2()//多个切换到一个（已存在的）
        {
            db.AddTag("P1", "C1");
            db.AddTag("P2", "C1");
            List<string> parents = db.QueryTagParent("C1");
            AssertListEqual(parents, new List<string>() { "P1", "P2" });

            db.ResetRelationOfChild("P2", "C1");
            parents = db.QueryTagParent("C1");
            AssertListEqual(parents, new List<string>() { "P2" });
        }

        [TestMethod]
        public void TestTag_SetRelation3() //添加不存在的节点
        {
            db.ResetRelationOfChild("P1", "C1");
            List<string> parents = db.QueryTagParent("C1");
            AssertListEqual(parents, new List<string>() { "P1" });

            db.ResetRelationOfChild("P1", "C2");
            parents = db.QueryTagParent("C2");
            AssertListEqual(parents, new List<string>() { "P1" });

        }

        [TestMethod]
        public void TestTag_SetRelation4() //添加不存在的节点
        {
            db.AddTag("P1", "C1");
            db.AddTag("P2", "C1");
            db.AddTag("P3", "C2");
            List<string> parents = db.QueryTagParent("C1");
            AssertListEqual(parents, new List<string>() { "P1","P2" });


            db.ResetRelationOfChild("P3", "C1");
            parents = db.QueryTagParent("C1");
            AssertListEqual(parents, new List<string>() { "P3" });

        }
    }
}
