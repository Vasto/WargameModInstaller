using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using WargameModInstaller.Model.Edata;

namespace WargameSkinInstallerTest
{
    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        public void IsSelectFileEntryByPathWorkingTest()
        {
            string path = @"pc\textures\assets\file1.tgv";

            EdataDictDirSubPath a1 = new EdataDictDirSubPath() {
                    SubPath = @"pc\",
                };

            EdataDictDirSubPath a2 = new EdataDictDirSubPath()
            {
                SubPath = @"textures\",
            };

            EdataDictDirSubPath a2a = new EdataDictDirSubPath()
            {
                SubPath = @"textures2\",
            };

            EdataDictDirSubPath a3a = new EdataDictDirSubPath()
            {
                SubPath = @"assets2\",
            };

            EdataDictDirSubPath a3= new EdataDictDirSubPath()
            {
                SubPath = @"assets\",
            };

            EdataDictFileSubPath a4 = new EdataDictFileSubPath()
            {
                SubPath = @"file1.tgv",
            };

            EdataDictFileSubPath a4a = new EdataDictFileSubPath()
            {
                SubPath = @"file1.tgv",
            };

            a1.AddFollowingSubPath(a2a);
            a1.AddFollowingSubPath(a2);
            a2.AddFollowingSubPath(a3);
            a2a.AddFollowingSubPath(a3a);
            a3.AddFollowingSubPath(a4);
            a3a.AddFollowingSubPath(a4a);

            EdataDictSubPath expected1 = a4;
            EdataDictSubPath value1 = a1.SelectEntryByPath(path);
            Assert.AreEqual(value1, expected1);

            String expected2 = "file1.tgv";
            String value2 = a1.SelectEntryByPath(path).SubPath;
            Assert.AreEqual(value2, expected2);
        }
    }
}
