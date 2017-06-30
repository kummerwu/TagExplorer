using LuceneTest.TagLayout;
using LuceneTest.TagMgr;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;

namespace UTLT
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
