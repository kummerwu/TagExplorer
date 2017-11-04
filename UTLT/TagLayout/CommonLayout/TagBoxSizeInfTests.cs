using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Windows;
using TagExplorer.TagLayout.LayoutCommon;

namespace UTLT
{
    [TestClass()]
    public class TagBoxSizeInfTests
    {
        private void CheckInf(TagBoxSizeInf inf,Size inn,Size outt)
        {
            int delta = 2;
            Assert.IsTrue(inf.InnerBoxSize.Width > inn.Width - delta && inf.InnerBoxSize.Width < inn.Width + delta);
            Assert.IsTrue(inf.InnerBoxSize.Height > inn.Height - delta && inf.InnerBoxSize.Height < inn.Height + delta);

            Assert.IsTrue(inf.OutterBoxSize.Width > outt.Width - delta && inf.OutterBoxSize.Width < outt.Width + delta);
            Assert.IsTrue(inf.OutterBoxSize.Height > outt.Height - delta && inf.OutterBoxSize.Height < outt.Height + delta);

        }
        //[TestMethod()]
        //public void TagBoxSizeInf_Test()
        //{
        //    TagBoxSizeInf inf = null;
        //    inf = new TagBoxSizeInf("我的大脑",0, @"Microsoft YaHei");
        //    CheckInf(inf, new Size(77, 26), new Size(107, 38));

        //    inf = new TagBoxSizeInf("我的大脑", 1, @"Microsoft YaHei");
        //    CheckInf(inf, new Size(70, 24), new Size(97, 35));
        //}
    }
}