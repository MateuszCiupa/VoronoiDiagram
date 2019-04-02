using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ProjZesp;

namespace ProjZespUnitTestProject
{
    [TestClass]
    public class EdgeUnitTest
    {
        [TestMethod]
        public void NoPointAddedDoesPointExistTest()
        {
            // GIVEN
            Edge edge = new Edge();
            Point point = new Point(0.0, 0.0);

            // WHEN
            bool reslut = edge.DoesPointExist(point);
            bool expectedResult = false;

            // THEN
            Assert.AreEqual(reslut, expectedResult);
        }

        [TestMethod]
        public void OnePointAddedDoesPointThatWasAddedExistTest()
        {
            // GIVEN
            Edge edge = new Edge();
            Point point = new Point(0.0, 0.0);
            edge.AddPoint(point);

            // WHEN
            bool reslut = edge.DoesPointExist(point);
            bool expectedResult = true;

            // THEN
            Assert.AreEqual(reslut, expectedResult);
        }

        [TestMethod]
        public void OnePointAddedDoesPointThatWasntAddedExistTest()
        {
            // GIVEN
            Edge edge = new Edge();
            Point point = new Point(0.0, 0.0);
            Point point1 = new Point(0.1, 0.9);
            edge.AddPoint(point);

            // WHEN
            bool reslut = edge.DoesPointExist(point1);
            bool expectedResult = false;

            // THEN
            Assert.AreEqual(reslut, expectedResult);
        }

        [TestMethod]
        public void TwoPointsAddedDoesPointThatWasAddedExistTest()
        {
            // GIVEN
            Edge edge = new Edge();
            Point point = new Point(0.0, 0.0);
            Point point1 = new Point(0.1, 0.9);
            edge.AddPoint(point);
            edge.AddPoint(point1);

            // WHEN
            bool reslut = edge.DoesPointExist(point1);
            bool expectedResult = true;

            // THEN
            Assert.AreEqual(reslut, expectedResult);
        }

        [TestMethod]
        public void TwoPointsAddedDoesPointThatWasNotAddedExistTest()
        {
            // GIVEN
            Edge edge = new Edge();
            Point point = new Point(0.0, 0.0);
            Point point1 = new Point(0.1, 0.9);
            Point point2 = new Point(0.4, 124.65);
            edge.AddPoint(point);
            edge.AddPoint(point1);

            // WHEN
            bool reslut = edge.DoesPointExist(point2);
            bool expectedResult = false;

            // THEN
            Assert.AreEqual(reslut, expectedResult);
        }
    }

    [TestClass]
    public class HalfEdgeUnitTest
    {
        [TestMethod]
        public void ChangeButToBeChangedIsNullTest()
        {
            // GIVEN
            HalfEdge halfEdge = new HalfEdge(null);
            halfEdge.SetToBeChangedWith(new Point(0.0, 0.0), new Point(0.0, 0.0));

            // WHEN
            halfEdge.Change();
            Edge result = halfEdge.ToBeChanged;
            Edge expectdResult = null;

            // THEN
            Assert.AreEqual(result, expectdResult);
        }

        [TestMethod]
        public void ChangeButToBeChangedWithIsNullTest()
        {
            // GIVEN
            Edge toBeChanged = new Edge(new Point(0.0, 0.0), new Point(0.0, 0.0));
            Edge toBeChangedCopy = new Edge(new Point(0.0, 0.0), new Point(0.0, 0.0));
            HalfEdge halfEdge = new HalfEdge(toBeChanged);
            halfEdge.SetToBeChangedWith(null , null);

            // WHEN
            halfEdge.Change();
            bool result = halfEdge.IsToBeChangedEqual(toBeChangedCopy);
            bool expectdResult = true;

            // THEN
            Assert.AreEqual(result, expectdResult);
        }
    }

    [TestClass]
    public class LineEquasionUnitTest
    {
        [TestMethod]
        public void BuildPerpendicularBisectorButPointsAreNullTest()
        {
            // GIVEN
            Line line = new Line();
            Line line1 = new Line(0.0, 0.0, 0.0);

            // WHEN
            line.BuildPerpendicularBisector(null, null);
            bool result = line.AreLinesEqual(line1);
            bool expectedResult = true;

            // THEN
            Assert.AreEqual(result, expectedResult);
        }

        [TestMethod]
        public void BuildLineButPointsAreNullTest()
        {
            // GIVEN
            Line line = new Line(null, null);
            Line line1 = new Line(0.0, 0.0, 0.0);

            // WHEN
            bool result = line.AreLinesEqual(line1);
            bool expectedResult = true;

            // THEN
            Assert.AreEqual(result, expectedResult);
        }

        [TestMethod]
        public void BuildPerpendicularBisectorButPointsAreEqualTest()
        {
            // GIVEN
            Point point = new Point(0.0, 0.0);
            Line line = new Line();
            Line line1 = new Line(0.0, 0.0, 0.0);

            // WHEN
            line.BuildPerpendicularBisector(point, point);
            bool result = line.AreLinesEqual(line1);
            bool expectedResult = true;

            // THEN
            Assert.AreEqual(result, expectedResult);
        }

        [TestMethod]
        public void BuildLineButPointsAreEqualTest()
        {
            // GIVEN
            Point point = new Point(0.0, 0.0);
            Line line = new Line(point, point);
            Line line1 = new Line(0.0, 0.0, 0.0);

            // WHEN
            bool result = line.AreLinesEqual(line1);
            bool expectedResult = true;

            // THEN
            Assert.AreEqual(result, expectedResult);
        }

        [TestMethod]
        public void GetEdgeIntersectionWhenEdgeIsNullTest()
        {
            // GIVEN
            Line line = new Line(0.9, 0.0, 0.0);

            // WHEN
            Point result = line.GetEdgeIntersection(null);
            Point expectedResult = null;

            // THEN
            Assert.AreEqual(result, expectedResult);
        }

        [TestMethod]
        public void DoesEdgeIntersectWhenEdgeDoesIntersectTest()
        {
            // GIVEN
            Line line = new Line(1.0, -1.0, 0.0); // x - y + 0 = 0 <=> y = x
            Edge edge = new Edge(new Point(0.0, 1.0), new Point(1.0, 0.0));

            // WHEN
            int result = line.DoesEdgeIntersect(edge.A, edge.B);
            int expectedResult = 1;

            // THEN
            Assert.AreEqual(result, expectedResult);
        }

        [TestMethod]
        public void DoesEdgeIntersectWhenEdgeDoesNotIntersectTest()
        {
            // GIVEN
            Line line = new Line(1.0, -1.0, 0.0); // x - y + 0 = 0 <=> y = x
            Edge edge = new Edge(new Point(0.0, 1.0), new Point(0.0, 3.0));

            // WHEN
            int result = line.DoesEdgeIntersect(edge.A, edge.B);
            int expectedResult = 0;

            // THEN
            Assert.AreEqual(result, expectedResult);
        }

        [TestMethod]
        public void DoesEdgeIntersectWhenEdgeDoesIntersectButOneOfTheBoundaryPointsTest()
        {
            // GIVEN
            Line line = new Line(1.0, -1.0, 0.0); // x - y + 0 = 0 <=> y = x
            Edge edge = new Edge(new Point(0.0, 1.0), new Point(0.0, 0.0));

            // WHEN
            int result = line.DoesEdgeIntersect(edge.A, edge.B);
            int expectedResult = 2;

            // THEN
            Assert.AreEqual(result, expectedResult);
        }

        [TestMethod]
        public void GetDistanceFromPointWhenPointIsOnTheLineTest()
        {
            // GIVEN
            Line line = new Line(1.0, -1.0, 0.0); // x - y + 0 = 0 <=> y = x
            Point point = new Point(0.0, 0.0);

            // WHEN
            double result = line.GetDistanceFromPoint(point.X, point.Y);
            double expectedResult = 0.0;

            // THEN
            Assert.AreEqual(result, expectedResult);
        }
    }
}
