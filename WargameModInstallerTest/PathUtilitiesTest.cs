
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using WargameModInstaller.Common.Utilities;

namespace WargameModInstallerTest
{
    
    
    /// <summary>
    ///This is a test class for PathUtilitiesTest and is intended
    ///to contain all PathUtilitiesTest Unit Tests
    ///</summary>
    [TestClass()]
    public class PathUtilitiesTest
    {


        private TestContext testContextInstance;

        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext
        {
            get
            {
                return testContextInstance;
            }
            set
            {
                testContextInstance = value;
            }
        }

        #region Additional test attributes
        // 
        //You can use the following additional attributes as you write your tests:
        //
        //Use ClassInitialize to run code before running the first test in the class
        //[ClassInitialize()]
        //public static void MyClassInitialize(TestContext testContext)
        //{
        //}
        //
        //Use ClassCleanup to run code after all tests in a class have run
        //[ClassCleanup()]
        //public static void MyClassCleanup()
        //{
        //}
        //
        //Use TestInitialize to run code before running each test
        //[TestInitialize()]
        //public void MyTestInitialize()
        //{
        //}
        //
        //Use TestCleanup to run code after each test has run
        //[TestCleanup()]
        //public void MyTestCleanup()
        //{
        //}
        //
        #endregion


        /// <summary>
        ///A test for IsValidAbsolutePath
        ///</summary>
        [TestMethod()]
        public void IsValidAbsolutePathTest()
        {
            string path = @"D:\Program Files\Steam\steamapps\common\Wargame Airland Battle";
            bool expected = true;
            bool actual;
            actual = PathUtilities.IsValidAbsolutePath(path);
            Assert.AreEqual(expected, actual);

            path = @"D:\\\\Program Files\\\\Steam\\\steamapps\\common\Wargame Airland Battle";
            expected = false;
            actual = PathUtilities.IsValidAbsolutePath(path);
            Assert.AreEqual(expected, actual);

            path = @"D:\Program Files\S:team\steam?apps\common\Wargame Airland Battle";
            expected = false;
            actual = PathUtilities.IsValidAbsolutePath(path);
            Assert.AreEqual(expected, actual);

            path = @"D:Program Files\Steam\steamapps\common\Wargame Airland Battle";
            expected = false;
            actual = PathUtilities.IsValidAbsolutePath(path);
            Assert.AreEqual(expected, actual);

            path = @"\Steam\steamapps\common\Wargame Airland Battle";
            expected = false;
            actual = PathUtilities.IsValidAbsolutePath(path);
            Assert.AreEqual(expected, actual);

            path = @"Steam\steamapps\common\Wargame Airland Battle";
            expected = false;
            actual = PathUtilities.IsValidAbsolutePath(path);
            Assert.AreEqual(expected, actual);

            path = @"D:";
            expected = false;
            actual = PathUtilities.IsValidAbsolutePath(path);
            Assert.AreEqual(expected, actual);

            path = @"D";
            expected = false;
            actual = PathUtilities.IsValidAbsolutePath(path);
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        ///A test for IsValidRelativePath
        ///</summary>
        [TestMethod()]
        public void IsValidRelativePathTest()
        {
            string path = @"..\Program Files\Steam\steamapps\common\Wargame Airland Battle";
            bool expected = true;
            bool actual;
            actual = PathUtilities.IsValidRelativePath(path);
            Assert.AreEqual(expected, actual);

            path = @"Program Files\Steam\steamapps\common\Wargame Airland Battle";
            expected = true;
            actual = PathUtilities.IsValidRelativePath(path);
            Assert.AreEqual(expected, actual);

            path = @"\Program Files\Steam\steamapps\common\Wargame Airland Battle";
            expected = false;
            actual = PathUtilities.IsValidRelativePath(path);
            Assert.AreEqual(expected, actual);

            path = @"Program Fi:les\Steam\steamapps\commo?n\Wargame Airland Battle";
            expected = false;
            actual = PathUtilities.IsValidRelativePath(path);
            Assert.AreEqual(expected, actual);

            path = @"D:\Program Files\Steam\steamapps\common\Wargame Airland Battle";
            expected = false;
            actual = PathUtilities.IsValidRelativePath(path);
            Assert.AreEqual(expected, actual);

            path = @"D:Program Files\Steam\steamapps\common\Wargame Airland Battle";
            expected = false;
            actual = PathUtilities.IsValidRelativePath(path);
            Assert.AreEqual(expected, actual);

        }
    }
}
