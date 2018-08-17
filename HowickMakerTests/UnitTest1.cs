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
        public void MinDistanceTest1()
        {
            Geo.Point p1 = Geo.Point.ByCoordinates(0, 0, 0);
            Geo.Point p2 = Geo.Point.ByCoordinates(0, 1, 1);
            Geo.Point p3 = Geo.Point.ByCoordinates(1, 0, -9);
            Geo.Point p4 = Geo.Point.ByCoordinates(1, 1, 2);

            Geo.Line l1 = Geo.Line.ByStartPointEndPoint(p1, p2);
            Geo.Line l2 = Geo.Line.ByStartPointEndPoint(p3, p4);

            double d = hStructure.MinDistanceBetweenLines(l1, l2);


            Assert.AreEqual(d, 1);
        }

        [TestMethod]
        public void MinDistanceTest2()
        {
            Geo.Point p1 = Geo.Point.ByCoordinates(0, 0, 0);
            Geo.Point p2 = Geo.Point.ByCoordinates(1, 1, 1);
            Geo.Point p3 = Geo.Point.ByCoordinates(-1, 0, 0);
            Geo.Point p4 = Geo.Point.ByCoordinates(-2, -2, -2);

            Geo.Line l1 = Geo.Line.ByStartPointEndPoint(p1, p2);
            Geo.Line l2 = Geo.Line.ByStartPointEndPoint(p3, p4);

            double d = hStructure.MinDistanceBetweenLines(l1, l2);


            Assert.AreEqual(1, d);
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
