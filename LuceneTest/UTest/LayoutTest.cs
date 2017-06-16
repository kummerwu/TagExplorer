using LuceneTest.TagLayout;
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
    public class LayoutTest
    {
        [TestMethod]
        public void test()
        {
            ITagDB tagdb = TagDBFactory.CreateTagDB();
            tagdb.AddTag("parent", "child");

            ITagLayout lay = TagLayoutFactory.CreateLayout();
            lay.Layout(tagdb, "parent");

            Assert.AreEqual(2, lay.Lines.Count());
            Assert.AreEqual(2, lay.TagArea.Count());
        }
    }
}
