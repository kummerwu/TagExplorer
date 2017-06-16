using LuceneTest.Core;
using LuceneTest.TagMgr;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LuceneTest.UTest
{
    [TestClass]
    public class LuceneTest
    {
        ITagDB db = null;
        [TestInitialize]
        public void setup()
        {
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
        public void testBaseAdd()
        {
            db.AddTag("p", "c1");
           
            List<string> c = db.QueryTagChildren("p");
            Assert.AreEqual(1, c.Count);
            Assert.AreEqual("c1", c[0]);
        }

        public void testBaseRemove()
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
        public void testChildren()
        {
            Logger.Log("0");
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
        public void testParent()
        {
            Logger.Log("0");
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
        public void testParent100()
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
        public void testChild100()
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
        public void testAlias()
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
        public void testAlias_Single2()
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
        public void testAlias2()
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
    }
}
