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

            EdataDictionaryDirEntry a1 = new EdataDictionaryDirEntry(@"pc\");

            EdataDictionaryDirEntry a2 = new EdataDictionaryDirEntry(@"textures\");

            EdataDictionaryDirEntry a2a = new EdataDictionaryDirEntry(@"textures2\");

            EdataDictionaryDirEntry a3a = new EdataDictionaryDirEntry(@"assets2\");

            EdataDictionaryDirEntry a3 = new EdataDictionaryDirEntry(@"assets\");

            EdataDictionaryFileEntry a4 = new EdataDictionaryFileEntry(@"file1.tgv");

            EdataDictionaryFileEntry a4a = new EdataDictionaryFileEntry(@"file1.tgv");

            a1.AddFollowingEntry(a2a);
            a1.AddFollowingEntry(a2);
            a2.AddFollowingEntry(a3);
            a2a.AddFollowingEntry(a3a);
            a3.AddFollowingEntry(a4);
            a3a.AddFollowingEntry(a4a);

            EdataDictionaryPathEntry expected1 = a4;
            EdataDictionaryPathEntry value1 = a1.SelectEntryByPath(path);
            Assert.AreEqual(value1, expected1);

            String expected2 = "file1.tgv";
            String value2 = a1.SelectEntryByPath(path).PathPart;
            Assert.AreEqual(value2, expected2);
        }
    }
}
