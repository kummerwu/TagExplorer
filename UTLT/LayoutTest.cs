using TagExplorer.TagLayout;
using TagExplorer.TagMgr;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;
using TagExplorer.Utils;
using AnyTagNet;
using TagExplorer.TagLayout.CommonLayout;

namespace UTLT
{
    [TestClass]
    public class LayoutTest
    {
        [TestInitialize]
        public void setup()
        {
            Cfg.Ins.IsUTest = true;
            
        }
        [TestMethod]
        public void TestLayout_Base()
        {
            GLayoutMode.mode = LayoutMode.GRAPH_UPDOWN;
            ITagDB tagdb = TagDBFactory.CreateTagDB();
            tagdb.AddTag("parent", "child");

            ITagLayout lay = TagLayoutFactory.CreateLayout();
            lay.Layout(tagdb, "parent",new System.Windows.Size(1000,1000),null);

            Assert.AreEqual(2, lay.Lines.Count());
            Assert.AreEqual(2, lay.TagArea.Count());
        }
    }
}
