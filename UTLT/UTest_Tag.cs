using TagExplorer.TagMgr;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using TagExplorer.Utils;
using System.IO;
using TagExplorer.UriMgr;

namespace UTLT
{
    [TestClass]
    public class UTest_Tag
    {
        ITagDB db = null;
        [TestInitialize]
        public void setup()
        {
            UTestCfg.Ins.IsUTest = true;
            //if (!System.IO.Directory.Exists(Cfg.Ins.TagDB))
            //{
            //    System.IO.Directory.CreateDirectory(Cfg.Ins.TagDB);
            //}
            db = TagDBFactory.CreateTagDB("sql");
        }
        [TestCleanup]
        public void teardown()
        {
            IDisposableFactory.DisposeAll();
            db = null;
            //为了安全，直接硬编码，防止把真是数据删除
            Directory.Delete(@"B:\00TagExplorerBase", true);
            //Directory.Delete(CfgPath.DocBasePath);


            //if (System.IO.Directory.Exists(Cfg.Ins.TagDB))
            //    System.IO.Directory.Delete(Cfg.Ins.TagDB,true);
        }
        [TestMethod]
        public void ITagDB_Test_AddBase()//简单添加
        {
            GUTag p = db.NewTag("p");
            GUTag c1 = db.NewTag("c1");
            db.SetParent(p,c1);
           
            List<GUTag> c = db.QueryTagChildren(p);
            Assert.AreEqual(1, c.Count);
            Assert.AreEqual("c1", c[0].Title);
        }

        [TestMethod]
        public void ITagDB_Test_AddDel()//添加后删除
        {
            GUTag p = db.NewTag("p");
            GUTag c1 = db.NewTag("c1");
            db.SetParent(p, c1);

            List<string> a = db.QueryTagAlias(p);
            Assert.AreEqual(1, a.Count);
            Assert.AreEqual("p", a[0]);

            db.RemoveTag(p);
            a = db.QueryTagAlias(p);
            Assert.AreEqual(0, a.Count);

        }

        [TestMethod]
        public void ITagDB_Test_Remove()//添加后删除
        {
            GUTag p = db.NewTag("p");
            GUTag c1 = db.NewTag("c1");
            db.SetParent(p,c1);

            List<string> a = db.QueryTagAlias(c1);
            Assert.AreEqual(1, a.Count);
            Assert.AreEqual("c1", a[0]);

            db.RemoveTag(c1);
            a = db.QueryTagAlias(c1);
            Assert.AreEqual(0, a.Count);

            Assert.AreEqual(0, db.QueryTagParent(c1).Count);

        }
        //[TestMethod]
        //public void ITagDB_Test_Remove_一个子节点有两个父节点()//添加后删除
        //{
        //    //Sql不再支持多个父节点
        //    if (db.GetType().Name.Contains("Sql")) return;

        //    GUTag p1 = db.NewTag("p1");
        //    GUTag p2 = db.NewTag("p2");
        //    GUTag c1 = db.NewTag("c1");
        //    db.AddTag(p1, c1);
        //    db.AddTag(p2, c1);

        //    db.RemoveTag(c1);
        //    AssertListEqual(new List<string>(), db.QueryTagChildren(p1));
        //    AssertListEqual(new List<string>(), db.QueryTagChildren(p2));


        //}
        [TestMethod]
        public void ITagDB_Test_AddMultiChildren()//父节点，有多个子节点
        {
            GUTag p = db.NewTag("p");
            GUTag c1 = db.NewTag("c1");
            GUTag c2 = db.NewTag("c2");

            db.SetParent(p, c1);
            List<GUTag> c = db.QueryTagChildren(p);
            Assert.AreEqual(1, c.Count);
            Assert.AreEqual("c1", c[0].Title);


            db.SetParent(p, c2);
            c = db.QueryTagChildren(p);
            Assert.AreEqual(2, c.Count);
            Assert.AreEqual(c1, c[0]);
            Assert.AreEqual(c2, c[1]);
        }

        //[TestMethod]
        //public void ITagDB_Test_AddMultiParent()//子节点，有多个父节点
        //{
        //    GUTag p1 = db.NewTag("p1");
        //    GUTag p2 = db.NewTag("p2");
        //    GUTag c = db.NewTag("c");

        //    db.AddTag(p1, c);
        //    List<GUTag> p = db.QueryTagParent(c);
        //    Assert.AreEqual(1, p.Count);
        //    Assert.AreEqual(p1, p[0]);


        //    db.AddTag(p2, c);
        //    p = db.QueryTagParent(c);
        //    Assert.AreEqual(2, p.Count);
        //    Assert.AreEqual(p1, p[0]);
        //    Assert.AreEqual(p2, p[1]);
        //}

        //[TestMethod]
        //public void ITagDB_Test_Add100Parent()//一个节点添加100个父节点
        //{
        //    int i = 0;
        //    GUTag c = db.NewTag("c");
        //    GUTag []ps = new GUTag[100];
        //    for (i = 0; i < 100; i++)
        //    {
        //        ps[i] = db.NewTag("p" + i);
        //        db.AddTag(ps[i], c);
        //        List<GUTag> p = db.QueryTagParent(c);
        //        Assert.AreEqual(i + 1, p.Count);
        //        for (int j = 0; j <= i; j++)
        //        {
        //            Assert.IsTrue(p.Contains(ps[j]));
                    
        //        }
        //    }


        //}


        [TestMethod]
        public void ITagDB_Test_Add100Child()//一个父节点，添加100个子节点
        {
            int i = 0;
            GUTag p = db.NewTag("p");
            GUTag[] cs = new GUTag[100];
            for (i = 0; i < 100; i++)
            {
                cs[i] = db.NewTag("c" + i);
                db.SetParent(p, cs[i]);
                List<GUTag> c = db.QueryTagChildren(p);
                Assert.AreEqual(i + 1, c.Count);
                for (int j = 0; j <= i; j++)
                {
                    Assert.IsTrue(c.Contains(cs[j]));
                }
            }


        }

        [TestMethod]
        public void ITagDB_Test_AddAlias()//添加别名
        {
            GUTag p1 = db.NewTag("p1");
            GUTag c1 = db.NewTag("c1");
            db.SetParent(p1, c1);
            List<string> alias = db.QueryTagAlias(p1);
            Assert.AreEqual(1, alias.Count);
            Assert.AreEqual(p1.Title, alias[0]);

            //db.MergeAliasTag("p1", "p2");
            // alias = db.QueryTagAlias("p1");
            //Assert.AreEqual(2, alias.Count);
            //Assert.AreEqual(true, alias.Contains("p1"));
            //Assert.AreEqual(true, alias.Contains("p2"));
        }
        [TestMethod]
        public void ITagDB_Test_Reopen()//关闭后重新打开
        {
            
            GUTag p1 = db.NewTag("p1");
            GUTag c1 = db.NewTag("c1");
            db.SetParent(p1, c1);
            List<string> alias = db.QueryTagAlias(p1);
            foreach (string a in alias)
            {
                Logger.D(a);
            }
            Assert.AreEqual(1, alias.Count);
            Assert.AreEqual("p1", alias[0]);
            Logger.D("end test reopen");

            IDisposableFactory.DisposeAll();
            db = null;

            db = TagDBFactory.CreateTagDB("sql");
            List<GUTag> p1new = db.QueryTags("p1");
            alias = db.QueryTagAlias(p1new[0]);
            Assert.AreEqual(1, alias.Count);
            Assert.AreEqual("p1", alias[0]);
        }
        [TestMethod]
        public void ITagDB_Test_AliasAddAndQuery()
        {
            GUTag p1 = db.NewTag("p1");
            GUTag p2 = db.NewTag("p2");
            db.MergeAlias(p1, p2);
            List<string> alias = db.QueryTagAlias(p1);
            Assert.AreEqual(2, alias.Count);
            Assert.AreEqual(true, alias.Contains("p1"));
            Assert.AreEqual(true, alias.Contains("p2"));

            alias = db.QueryTagAlias(p2);
            Assert.AreEqual(0, alias.Count);
            //Assert.AreEqual(true, alias.Contains("p1"));
            //Assert.AreEqual(true, alias.Contains("p2"));
        }

        [TestMethod]
        public void ITagDB_Test_AliasAddAndQuery2()
        {
            GUTag p1 = db.NewTag("p1");
            GUTag p2 = db.NewTag("p2");
            GUTag c1 = db.NewTag("c1");
            GUTag c2 = db.NewTag("c2");

            db.SetParent(p1, c1);
            db.SetParent(p2, c2);
            db.MergeAlias(p1, p2);
            List<string> alias = db.QueryTagAlias(p1);
            Assert.AreEqual(2, alias.Count);
            Assert.AreEqual(true, alias.Contains("p1"));
            Assert.AreEqual(true, alias.Contains("p2"));

            alias = db.QueryTagAlias(p2);
            Assert.AreEqual(0, alias.Count);
            //Assert.AreEqual(true, alias.Contains("p1"));
            //Assert.AreEqual(true, alias.Contains("p2"));

            List<GUTag> child = db.QueryTagChildren(p1);
            Assert.AreEqual(2, child.Count);
            Assert.AreEqual(true, child.Contains(c1));
            Assert.AreEqual(true, child.Contains(c2));


            child = db.QueryTagChildren(p2);
            Assert.AreEqual(0, child.Count);
            //Assert.AreEqual(true, child.Contains("c1"));
            //Assert.AreEqual(true, child.Contains("c2"));
        }

        public static void AssertListEqual(List<string> expect, List<string> real)
        {
            Assert.AreEqual(expect.Count, real.Count);
            foreach (string e in expect)
            {
                Assert.AreEqual(true, real.Contains(e));
            }
        }

        public static void AssertListEqual(List<string> expect, List<GUTag> real)
        {
            Assert.AreEqual(expect.Count, real.Count);
            foreach (GUTag r in real)
            {
                Assert.AreEqual(true, expect.Contains(r.Title));
            }
        }
        public static void AssertListEqual(IList<GUTag> expect, IList<GUTag> real)
        {
            Assert.AreEqual(expect.Count, real.Count);
            foreach (GUTag r in real)
            {
                Assert.AreEqual(true, expect.Contains(r));
            }
        }
        [TestMethod]
        public void ITagDB_Test_SetRelation()//多个切换到一个(不存在的)
        {
            GUTag P1 = db.NewTag("P1");
            GUTag P2 = db.NewTag("P2");
            GUTag P3 = db.NewTag("P3");
            GUTag C1 = db.NewTag("C1");
            GUTag C2 = db.NewTag("C2");

            db.SetParent(P1, C1);
            db.SetParent(P2, C1);
            List<GUTag> parents = db.QueryTagParent(C1);
            AssertListEqual(parents, new List<GUTag>() { P1, P2 });

            db.ResetParent(P3, C1);

            AssertListEqual(new List<string>() { "P3" }, db.QueryTagParent(C1));
            AssertListEqual(new List<string>() { }, db.QueryTagChildren(C1));

            AssertListEqual(new List<string>(), db.QueryTagParent(P1));
            AssertListEqual(new List<string>() { }, db.QueryTagChildren(P1));

            AssertListEqual(new List<string>(), db.QueryTagParent(P2));
            AssertListEqual(new List<string>() { }, db.QueryTagChildren(P2));

            AssertListEqual(new List<string>(), db.QueryTagParent(P3));
            AssertListEqual(new List<string>() { "C1" }, db.QueryTagChildren(P3));



        }

        [TestMethod]
        public void ITagDB_Test_SetRelation2()//多个切换到一个（已存在的）
        {
            GUTag P1 = db.NewTag("P1");
            GUTag P2 = db.NewTag("P2");
            GUTag P3 = db.NewTag("P3");
            GUTag C1 = db.NewTag("C1");
            GUTag C2 = db.NewTag("C2");


            db.SetParent(P1, C1);
            db.SetParent(P2, C1);
            List<GUTag> parents = db.QueryTagParent(C1);
            AssertListEqual( new List<string>() { "P1", "P2" },parents);

            db.ResetParent(P2, C1);

            AssertListEqual(new List<string>() { "P2" }, db.QueryTagParent(C1));
            AssertListEqual(new List<string>() { }, db.QueryTagChildren(C1));

            AssertListEqual(new List<string>(), db.QueryTagParent(P1));
            AssertListEqual(new List<string>() { }, db.QueryTagChildren(P1));

            AssertListEqual(new List<string>(), db.QueryTagParent(P2));
            AssertListEqual(new List<string>() { "C1" }, db.QueryTagChildren(P2));


        }

        [TestMethod]
        public void ITagDB_Test_SetRelation3() //添加不存在的节点
        {
            GUTag P1 = db.NewTag("P1");
            GUTag P2 = db.NewTag("P2");
            GUTag P3 = db.NewTag("P3");
            GUTag C1 = db.NewTag("C1");
            GUTag C2 = db.NewTag("C2");

            db.ResetParent(P1, C1);
            List<GUTag> parents = db.QueryTagParent(C1);
            AssertListEqual(parents, new List<GUTag>() { P1 });

            db.ResetParent(P1, C2);
            parents = db.QueryTagParent(C2);
            AssertListEqual(parents, new List<GUTag>() { P1 });

        }

        [TestMethod]
        public void ITagDB_Test_SetRelation4() //添加不存在的节点
        {
            GUTag P1 = db.NewTag("P1");
            GUTag P2 = db.NewTag("P2");
            GUTag P3 = db.NewTag("P3");
            GUTag C1 = db.NewTag("C1");
            GUTag C2 = db.NewTag("C2");

            db.SetParent(P1, C1);
            db.SetParent(P2, C1);
            db.SetParent(P3, C2);
            List<GUTag> parents = db.QueryTagParent(C1);
            AssertListEqual( new List<string>() { "P1", "P2" },parents);


            db.ResetParent(P3, C1);
            parents = db.QueryTagParent(C1);
            AssertListEqual(new List<string>() { "P3" }, parents);

        }

        [TestMethod]
        public void ITagDB_Test_Import()
        {
            db.Import(@"B:\Tags.json");
        }
    }
}
