using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Windows;
using TagExplorer.TagLayout.LayoutCommon;
namespace UTLT
{
    [TestClass()]
    public class GTagBoxTests
    {
        private void CheckBox(GTagBox box, Point inn,Point outt)
        {
            int delta = 2;
            Assert.IsTrue(box.InnerBoxLeftTop.X > inn.X - delta && box.InnerBoxLeftTop.X < inn.X + delta);
            Assert.IsTrue(box.InnerBoxLeftTop.Y > inn.Y - delta && box.InnerBoxLeftTop.Y < inn.Y + delta);

            Assert.IsTrue(box.OutterBoxLeftTop.X > outt.X - delta && box.OutterBoxLeftTop.X < outt.X + delta);
            Assert.IsTrue(box.OutterBoxLeftTop.Y > outt.Y - delta && box.OutterBoxLeftTop.Y < outt.Y + delta);

        }

        //[TestMethod()]
        //public void GTagBoxTest_一个节点()
        //{
        //    GTagBox box;
        //    box = new GTagBox(1,"我的大脑",0,0,1);
        //    CheckBox(box, new Point(13, 5), new Point(0, 0));

        //    box = new GTagBox(1, "我的大脑", 100, 100, 1);
        //    CheckBox(box, new Point(113, 105), new Point(100, 100));


        //    box = new GTagBox(1, "我的大脑", 0, 0, -1);
        //    CheckBox(box, new Point(-84, 5), new Point(-97, 0));

        //    box = new GTagBox(1, "我的大脑", 100, 100, -1);
        //    CheckBox(box, new Point(16, 105), new Point(3, 100));

        //}
    }
}