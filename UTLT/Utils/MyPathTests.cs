﻿using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.IO;
using TagExplorer;
using TagExplorer.Utils;

namespace AnyTags.Net.Tests
{
    [TestClass()]
    public class FileSytemTest
    {
        string dir = @"B:\utest";
        string dir1 = @"B:\utest\tag1";
        string dir2 = @"B:\utest\tag2";
        string docBase = @"B:\00TagExplorerBase";

        [TestInitialize]
        public void setup()
        {
            Directory.CreateDirectory(dir);
            Directory.CreateDirectory(dir1);
            Directory.CreateDirectory(dir2);
            string docBase = CfgPath.DocBasePath;

        }

        [TestCleanup]
        public void teardown()
        {
            Directory.Delete(dir, true);
            Directory.Delete(docBase,true);//删除B目录下的文件，用于单元测试

        }
        [TestMethod]
        public void TestFS_FilesRelocationTest()
        {
            FilesRelocationTest(new string[] { PathHelper.GetDirByTag("test") + "\\hello" },
                                new string[] { @"c:\hello"},
                                "test");
        }


        public void FilesRelocationTest(string[] dst,string[] src,string tag)
        {
            string[] rel = PathHelper.MapFilesToTagDir(src,tag);
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

            FileShell.SHMoveFiles(filesSrc, filesDst);
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
            Assert.IsTrue( PathHelper.NeedSkip(file));
            fs.Close();
            Assert.IsFalse(PathHelper.NeedSkip(file));

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

        [TestMethod]
        public void TestFS_LRUTag1()
        {
            CfgPerformance.LRU_MAX_CNT = 4;
            UTLT.UTest_Tag.AssertListEqual(LRUTag.Ins.GetTags(),new List<string>());
            LRUTag.Ins.Add("tag1");
            UTLT.UTest_Tag.AssertListEqual(LRUTag.Ins.GetTags(), 
                new List<string>() { "tag1" });


            LRUTag.Ins.Add("tag2");
            UTLT.UTest_Tag.AssertListEqual(LRUTag.Ins.GetTags(),
                new List<string>() { "tag1", "tag2", });

            LRUTag.Ins.Add("tag3");
            UTLT.UTest_Tag.AssertListEqual(LRUTag.Ins.GetTags(),
                new List<string>() { "tag1", "tag2", "tag3", });


            LRUTag.Ins.Add("tag4");
            UTLT.UTest_Tag.AssertListEqual(LRUTag.Ins.GetTags(),
                new List<string>() { "tag1", "tag2", "tag3", "tag4", });

            LRUTag.Ins.Add("tag5");
            UTLT.UTest_Tag.AssertListEqual(LRUTag.Ins.GetTags(),
                new List<string>() { "tag2", "tag3", "tag4", "tag5", });
        }
        [TestMethod]
        public void TestFS_LRUTag2()
        {
            CfgPerformance.LRU_MAX_CNT = 4;
            TestFS_LRUTag1();
            LRUTag.Ins.Dispose();
            UTLT.UTest_Tag.AssertListEqual(LRUTag.Ins.GetTags(),
                new List<string>() { "tag2", "tag3", "tag4", "tag5", });

            LRUTag.Ins.Add("tag1");
            UTLT.UTest_Tag.AssertListEqual(LRUTag.Ins.GetTags(),
                new List<string>() { "tag3", "tag4", "tag5", "tag1" });
        }

        [TestMethod]
        public void TestFS_LRUTag3()
        {
            CfgPerformance.LRU_MAX_CNT = 4;
            UTLT.UTest_Tag.AssertListEqual(LRUTag.Ins.GetTags(), new List<string>());
            LRUTag.Ins.Add("tag1");
            UTLT.UTest_Tag.AssertListEqual(LRUTag.Ins.GetTags(),
                new List<string>() { "tag1" });


            LRUTag.Ins.Add("tag1");
            UTLT.UTest_Tag.AssertListEqual(LRUTag.Ins.GetTags(),
                new List<string>() { "tag1" });


            LRUTag.Ins.Add("tag2");
            UTLT.UTest_Tag.AssertListEqual(LRUTag.Ins.GetTags(),
                new List<string>() { "tag1", "tag2", });

            LRUTag.Ins.Add("tag2");
            UTLT.UTest_Tag.AssertListEqual(LRUTag.Ins.GetTags(),
                new List<string>() { "tag1", "tag2", });


            LRUTag.Ins.Add("tag3");
            UTLT.UTest_Tag.AssertListEqual(LRUTag.Ins.GetTags(),
                new List<string>() { "tag1", "tag2", "tag3", });

            LRUTag.Ins.Add("tag3");
            UTLT.UTest_Tag.AssertListEqual(LRUTag.Ins.GetTags(),
                new List<string>() { "tag1", "tag2", "tag3", });

            LRUTag.Ins.Add("tag4");
            UTLT.UTest_Tag.AssertListEqual(LRUTag.Ins.GetTags(),
                new List<string>() { "tag1", "tag2", "tag3", "tag4", });


            LRUTag.Ins.Add("tag4");
            UTLT.UTest_Tag.AssertListEqual(LRUTag.Ins.GetTags(),
                new List<string>() { "tag1", "tag2", "tag3", "tag4", });


            LRUTag.Ins.Add("tag5");
            UTLT.UTest_Tag.AssertListEqual(LRUTag.Ins.GetTags(),
                new List<string>() { "tag2", "tag3", "tag4", "tag5", });


            LRUTag.Ins.Add("tag5");
            UTLT.UTest_Tag.AssertListEqual(LRUTag.Ins.GetTags(),
                new List<string>() { "tag2", "tag3", "tag4", "tag5", });
        }
    }
}