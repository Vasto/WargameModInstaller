using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using WargameModInstaller.Model.Containers.Edata;

namespace WargameSkinInstallerTest
{
    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        public void IsSelectFileEntryByPathWorkingTest()
        {
            string path = @"pc\textures\assets\file1.tgv";

            EdataDirSubPath a1 = new EdataDirSubPath(@"pc\");

            EdataDirSubPath a2 = new EdataDirSubPath(@"textures\");

            EdataDirSubPath a2a = new EdataDirSubPath(@"textures2\");

            EdataDirSubPath a3a = new EdataDirSubPath(@"assets2\");

            EdataDirSubPath a3 = new EdataDirSubPath(@"assets\");

            EdataFileSubPath a4 = new EdataFileSubPath(@"file1.tgv");

            EdataFileSubPath a4a = new EdataFileSubPath(@"file1.tgv");

            a1.AddFollowingSubPath(a2a);
            a1.AddFollowingSubPath(a2);
            a2.AddFollowingSubPath(a3);
            a2a.AddFollowingSubPath(a3a);
            a3.AddFollowingSubPath(a4);
            a3a.AddFollowingSubPath(a4a);

            EdataSubPath expected1 = a4;
            EdataSubPath value1 = a1.SelectEntryByPath(path);
            Assert.AreEqual(value1, expected1);

            String expected2 = "file1.tgv";
            String value2 = a1.SelectEntryByPath(path).SubPath;
            Assert.AreEqual(value2, expected2);
        }
    }
}
