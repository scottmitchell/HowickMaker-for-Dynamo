using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.Linq;
using Autodesk.DesignScript.Geometry;
using Geo = Autodesk.DesignScript.Geometry;
using HowickMaker;

namespace HowickMakerTests
{
    [TestClass]
    public class UnitTest1
    {

        public TestContext testContext;

        public TestContext TestContext
        {
            get { return testContext; }
            set { testContext = value; }
        }

        [ClassInitialize]
        public static void ASMSetup(TestContext TestContext)
        {
            var installations = DynamoInstallDetective.Utilities.FindProductInstallations("Revit", "ASMAHL*.dll");

            string location = "";

            foreach (KeyValuePair<string, Tuple<int, int, int, int>> install in installations)
            {
                if (221 == install.Value.Item1)
                {
                    location = install.Key;
                }
            }
            
            HostFactory.Instance.PreloadAsmLibraries(location);
            Autodesk.DesignScript.Geometry.HostFactory.Instance.StartUp();
        }

        [TestMethod]
        public void TestMethod1()
        {
            Geo.Point pt = Geo.Point.ByCoordinates(0, 0, 0); 
            Assert.AreEqual(0, 0);
        }

        [TestMethod]
        public void TestMethod2()
        {
            Assert.AreEqual(0, 1);
        }
        
        [TestMethod]
        public void hConnectionEqualityTest()
        {
            List<int> members1 = new List<int> { 3, 5 };
            List<int> members2 = new List<int> { 5, 3 };

            hConnection con1 = new hConnection(Connection.BR, members1);
            hConnection con2 = new hConnection(Connection.BR, members2);

            Assert.AreEqual(con1, con2);
        }

        [TestMethod]
        public void hConnectionContainsTest()
        {
            List<int> members1 = new List<int> { 3, 5 };
            List<int> members2 = new List<int> { 5, 3 };

            hConnection con1 = new hConnection(HowickMaker.Connection.BR, members1);
            hConnection con2 = new hConnection(Connection.BR, members2);
            var connections = new List<hConnection> { con1 };
            Assert.IsTrue(connections.Contains(con2));
        }
    }
}
