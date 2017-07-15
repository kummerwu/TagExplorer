using Microsoft.VisualStudio.TestTools.UnitTesting;
using AnyTags.Net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using AnyTag.BL;
using LuceneTest.UriMgr;

namespace AnyTags.Net.Tests
{
    [TestClass()]
    public class FileSytemTest
    {
        string dir = @"B:\utest";
        string dir1 = @"B:\utest\tag1";
        string dir2 = @"B:\utest\tag2";


        [TestInitialize]
        public void setup()
        {
            Directory.CreateDirectory(dir);
            Directory.CreateDirectory(dir1);
            Directory.CreateDirectory(dir2);

        }

        [TestCleanup]
        public void teardown()
        {
            Directory.Delete(dir, true);
        }
        [TestMethod]
        public void TestFS_FilesRelocationTest()
        {
            FilesRelocationTest(new string[] { MyPath.GetDirPath("test") + "\\hello" },
                                new string[] { @"c:\hello"},
                                "test");
        }


        public void FilesRelocationTest(string[] dst,string[] src,string tag)
        {
            string[] rel = MyPath.FilesRelocation(src,tag);
            string[] exp = dst;
            Assert.AreEqual(exp.Length , rel.Length);
            for(int i = 0;i<rel.Length;i++)
            {
                Assert.AreEqual(exp[i], rel[i]);
            }
        }

        public void TestFS_testMoveFiles(string [] names)
        {

            string[] filesSrc = new string[names.Length];
            string[] filesDst = new string[names.Length];
            
            //初始化文件和目录
            for(int i = 0;i<names.Length;i++)
            {
                filesSrc[i] = Path.Combine(dir1, names[i]);
                filesDst[i] = Path.Combine(dir2, names[i]);
                if(names[i].IndexOf('.')>=0)
                {
                    File.WriteAllText(filesSrc[i], "test");
                }
                else
                {
                    Directory.CreateDirectory(filesSrc[i]);
                    File.WriteAllText(Path.Combine(filesSrc[i],"xxx.dat"), "test");
                }
            } 

            //检查文件和目录是否存在
            for (int i = 0; i < filesSrc.Length; i++)
            {
                if (names[i].IndexOf('.') >= 0)
                {
                    Assert.IsTrue(File.Exists(filesSrc[i]));
                    Assert.IsFalse(File.Exists(filesDst[i]));
                }
                else
                {
                    Assert.IsTrue(Directory.Exists(filesSrc[i]));
                    Assert.IsFalse(Directory.Exists(filesDst[i]));
                }
            }

            FileShell.MoveFiles(filesSrc, filesDst);
            //检查文件和目录是否已经全部移到目的地
            for (int i = 0; i < filesSrc.Length; i++)
            {
                if (names[i].IndexOf('.') >= 0)
                {
                    Assert.IsFalse(File.Exists(filesSrc[i]));
                    Assert.IsTrue(File.Exists(filesDst[i]));
                }
                else
                {
                    Assert.IsFalse(Directory.Exists(filesSrc[i]));
                    Assert.IsTrue(Directory.Exists(filesDst[i]));
                }
            }

        }
        [TestMethod]
        public void TestFS_MoveFilesTest1()
        {
            TestFS_testMoveFiles(new string[]{"1.txt" });
            
        }


        [TestMethod]
        public void TestFS_MoveFilesTest2()
        {
            TestFS_testMoveFiles(new string[] { "1.txt","2.txt" });
        }

        [TestMethod]
        public void TestFS_MoveDirsTest3()
        {
            TestFS_testMoveFiles(new string[] { "1" });
        }

        [TestMethod]
        public void TestFS_MoveDirsTest4()
        {
            TestFS_testMoveFiles(new string[] { "1" ,"2"});
        }

        [TestMethod]
        public void TestFS_MoveDirsTest5()
        {
            TestFS_testMoveFiles(new string[] { "1", "2.txt" });
        }

        [TestMethod]
        public void TestFS_MoveDirsTest6()
        {
            TestFS_testMoveFiles(new string[] { "1", "2.txt","3","4.txt" });
        }

        [TestMethod]
        public void TestFS_TestFileFilter()
        {
            string file = Path.Combine(dir, "1.txt");
            FileStream fs = new FileStream(file, FileMode.CreateNew);
            fs.WriteByte(1);
            Assert.IsTrue( MyPath.NeedSkipThisUri(file));
            fs.Close();
            Assert.IsFalse(MyPath.NeedSkipThisUri(file));

        }


        [TestMethod]
        public void TestFS_GetID()
        {
            string f = Path.Combine(dir, "TestDir");
            Directory.CreateDirectory(f);
            string lastF = f;
            Guid id = NtfsFileID.GetID(f);
            for (int i = 0; i < 100; i++)
            {
                lastF = f + i;
                Directory.Move(f, lastF);
                Assert.AreEqual(id, NtfsFileID.GetID(lastF));
                f = lastF;
            }
        }
    }
}