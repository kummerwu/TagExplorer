using Microsoft.VisualStudio.TestTools.UnitTesting;
using TagExplorer.TagMgr;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TagExplorer.Utils;
using TagExplorer.UriMgr;
using System.IO;

namespace TagExplorer.TagMgr.Tests
{
    [TestClass()]
    public class SQLTagDBTests
    {
        SQLTagDB db = null;
        [TestInitialize]
        public void setup()
        {
            UTestCfg.Ins.IsUTest = true;
            //if (!System.IO.Directory.Exists(Cfg.Ins.TagDB))
            //{
            //    System.IO.Directory.CreateDirectory(Cfg.Ins.TagDB);
            //}
            db = IDisposableFactory.New<SQLTagDB>(new SQLTagDB());
        }
        [TestCleanup]
        public void teardown()
        {
            IDisposableFactory.DisposeAll();
            db = null;
            while (File.Exists(CfgPath.TagDBPath_SQLite))
            {
                try
                {
                    File.Delete(CfgPath.TagDBPath_SQLite);
                    System.Threading.Thread.Sleep(100);
                    break;
                }
                catch (Exception ee)
                {
                    Logger.E(ee);
                    System.Threading.Thread.Sleep(100);
                }
            }
            //为了安全，直接硬编码，防止把真是数据删除
            string dir = @"B:\00TagExplorerBase";
            while (Directory.Exists(dir))
            {
                try
                {
                    Directory.Delete(dir, true);
                    System.Threading.Thread.Sleep(100);
                    break;
                }
                catch (Exception ee)
                {
                    Logger.E(ee);
                    System.Threading.Thread.Sleep(100);
                }
            }


            //Directory.Delete(CfgPath.DocBasePath);


            //if (System.IO.Directory.Exists(Cfg.Ins.TagDB))
            //    System.IO.Directory.Delete(Cfg.Ins.TagDB,true);
        }
        public static void CheckListEqual<T>(List<T> expect, List<T> real)
        {
            Assert.AreEqual(expect.Count, real.Count);
            foreach (T t in expect)
            {
                Assert.IsTrue(real.Contains(t));
            }
        }
        [TestMethod]
        public void emty()
        {
            Guid id = Guid.NewGuid(), pid = Guid.NewGuid(), cid = Guid.NewGuid();
            GUTag tag = new GUTag();
            tag.Id = id;
            tag.PId = pid;
            tag.AddAlias("TITLE");
            tag.AddAlias("Alias");
            tag.AddChild(cid);

            db.AddUptSqlDB(tag);
        }

        [TestMethod()]
        public void AddUptSqlDBTest()
        {
            Guid id = Guid.NewGuid(), pid = Guid.NewGuid(), cid = Guid.NewGuid();
            GUTag tag = new GUTag();
            tag.Id = id;
            tag.PId = pid;
            tag.AddAlias("TITLE");
            tag.AddAlias("Alias");
            tag.AddChild(cid);

            db.AddUptSqlDB(tag);

            GUTag tag2 = db.QuerySqlDB(id);
            Assert.IsNotNull(tag2);
            Assert.AreEqual(tag2.Id, tag.Id);
            Assert.AreEqual(tag2.PId, tag.PId);
            Assert.AreEqual(tag2.Title, tag.Title);
            CheckListEqual(tag2.Children, tag.Children);
            CheckListEqual(tag2.Alias, tag.Alias);

        }


        [TestMethod()]
        public void AddUptSqlDBTest_2Child()
        {
            Guid id = Guid.NewGuid(), pid = Guid.NewGuid(), cid = Guid.NewGuid(), cid2 = Guid.NewGuid();
            GUTag tag = new GUTag();
            tag.Id = id;
            tag.PId = pid;
            tag.AddAlias("TITLE");
            tag.AddAlias("Alias");
            tag.AddAlias("Alias2");
            tag.AddChild(cid);
            tag.AddChild(cid2);

            db.AddUptSqlDB(tag);

            GUTag tag2 = db.QuerySqlDB(id);
            Assert.IsNotNull(tag2);
            Assert.AreEqual(tag2.Id, tag.Id);
            Assert.AreEqual(tag2.PId, tag.PId);
            Assert.AreEqual(tag2.Title, tag.Title);
            CheckListEqual(tag2.Children, tag.Children);
            CheckListEqual(tag2.Alias, tag.Alias);

        }

        [TestMethod()]
        public void DelSqlDBTest()
        {
            Guid id = Guid.NewGuid(), pid = Guid.NewGuid(), cid = Guid.NewGuid();
            GUTag tag = new GUTag();
            tag.Id = id;
            tag.PId = pid;
            tag.AddAlias("TITLE");
            tag.AddAlias("Alias");
            tag.AddChild(cid);

            db.AddUptSqlDB(tag);

            GUTag tag2 = db.QuerySqlDB(id);
            Assert.IsNotNull(tag2);
            Assert.AreEqual(tag2.Id, tag.Id);
            Assert.AreEqual(tag2.PId, tag.PId);
            Assert.AreEqual(tag2.Title, tag.Title);
            CheckListEqual(tag2.Children, tag.Children);
            CheckListEqual(tag2.Alias, tag.Alias);

            db.DelSqlDB(tag);
            Assert.IsNull(db.QuerySqlDB(id));
        }

        [TestMethod()]
        public void ImportTest()
        {
            db.Import(@"B:\Tags3.json");

        }
    }
}