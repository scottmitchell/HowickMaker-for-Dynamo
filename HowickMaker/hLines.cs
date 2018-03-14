using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Geo = Autodesk.DesignScript.Geometry;

namespace HowickMaker
{
    /// <summary>
    /// Tools for creating networks of lines that make valid steel stud structures.
    /// </summary>
    public class hLines
    {
        hMesh mesh;
        List<Geo.Line> lines = new List<Geo.Line>();
        double offset;


        internal hLines(hMesh mesh, double offset)
        {
            this.mesh = mesh;
            this.offset = offset;
        }




        #region Quad Strategy 01

        /// <summary>
        /// Create "Howick-able" lines on a planar quad mesh
        /// </summary>
        /// <param name="hMesh"></param>
        /// <param name="offset"></param>
        /// <returns name = "lines"></returns>
        public static List<Geo.Line> QuadStrategy_01(hMesh mesh, double offset)
        {
            mesh.Reset();

            hLines network = new hLines(mesh, offset);

            network.GenerateRightHand(0);

            return network.lines;
        }

        Geo.Point GetMemberPointOnEdge(int faceIndex, int edgeIndex, bool rightHand)
        {
            int direction;
            if (rightHand)
            {
                direction = 1;
            }
            else
            {
                direction = -1;
            }
            

            hVertex v1 = mesh.faces[faceIndex].vertices[edgeIndex];
            hVertex v2 = mesh.faces[faceIndex].vertices[(edgeIndex + 1) % 4];

            Geo.Point midPoint = Geo.Point.ByCoordinates((v1.x + v2.x) / 2, (v1.y + v2.y) / 2, (v1.z + v2.z) / 2);

            Geo.Vector move = Geo.Vector.ByTwoPoints(v1.ToPoint(), v2.ToPoint());
            move = move.Normalized();
            move = move.Scale(offset * direction);

            var returnPt = (Geo.Point)midPoint.Translate(move);

            // Dispose
            /*{
                midPoint.Dispose();
                move.Dispose();
            }*/

            return returnPt;
        }

        Geo.Line GetMemberLineR(int faceIndex, int edgeIndex, bool rightHand)
        {
            Geo.Point p1 = mesh.faces[faceIndex].vertices[edgeIndex].ToPoint();
            Geo.Point p2 = mesh.faces[faceIndex].vertices[(edgeIndex + 1) % 4].ToPoint();
            Geo.Point p3 = mesh.faces[faceIndex].vertices[(edgeIndex + 2) % 4].ToPoint();
            
            Geo.Point memberPoint = GetMemberPointOnEdge(faceIndex, edgeIndex, rightHand);
            Geo.Point nextMemberPoint = GetMemberPointOnEdge(faceIndex, (edgeIndex + 1) % 4, rightHand);
            
            Geo.Vector v1 = Geo.Vector.ByTwoPoints(p2, p1);
            Geo.Vector v2 = Geo.Vector.ByTwoPoints(p2, p3);

            double l = p2.DistanceTo(nextMemberPoint);
            double x = p2.DistanceTo(memberPoint);
            double a = 90 - v1.AngleWithVector(v2);
            a = (a * (Math.PI / 180));

            double d1 = x / (Math.Tan(a));
            double d2 = (x / Math.Sin(a)) - l;
            double d3 = Math.Cos(a);

            double length = d1 - (d2 / d3);

            Geo.Vector direction = v1.Cross(v1.Cross(v2));
            direction = direction.Reverse();

            var returnLine = Geo.Line.ByStartPointDirectionLength(memberPoint, direction, length);

            // Dispose
            /*{
                p1.Dispose();
                p2.Dispose();
                p3.Dispose();
                memberPoint.Dispose();
                nextMemberPoint.Dispose();
                v1.Dispose();
                v2.Dispose();
                direction.Dispose();
            }*/
            return returnLine;
        }

        Geo.Line GetMemberLineL(int faceIndex, int edgeIndex, bool rightHand)
        {
            Geo.Point p1 = mesh.faces[faceIndex].vertices[edgeIndex].ToPoint();
            Geo.Point p2 = mesh.faces[faceIndex].vertices[(edgeIndex + 3) % 4].ToPoint();
            Geo.Point p3 = mesh.faces[faceIndex].vertices[(edgeIndex + 1) % 4].ToPoint();

            Geo.Point memberPoint = GetMemberPointOnEdge(faceIndex, edgeIndex, rightHand);
            Geo.Point nextMemberPoint = GetMemberPointOnEdge(faceIndex, (edgeIndex + 3) % 4, rightHand);

            Geo.Vector v1 = Geo.Vector.ByTwoPoints(p1, p3);
            Geo.Vector v2 = Geo.Vector.ByTwoPoints(p1, p2);

            double l = p1.DistanceTo(nextMemberPoint);
            double x = p1.DistanceTo(memberPoint);
            double a = 90 - v1.AngleWithVector(v2);
            a = (a * (Math.PI / 180));

            double d1 = x / (Math.Tan(a));
            double d2 = (x / Math.Sin(a)) - l;
            double d3 = Math.Cos(a);

            double length = d1 - (d2 / d3);

            Geo.Vector direction = v1.Cross(v1.Cross(v2));
            direction = direction.Reverse();

            var returnLine = Geo.Line.ByStartPointDirectionLength(memberPoint, direction, length);

            // Dispose
            /*{
                p1.Dispose();
                p2.Dispose();
                p3.Dispose();
                memberPoint.Dispose();
                nextMemberPoint.Dispose();
                v1.Dispose();
                v2.Dispose();
                direction.Dispose();
            }*/
            return returnLine;
        }


        internal void GenerateRightHand(int faceIndex)
        {
            // Generate Lines
            for (int i = 0; i < 4; i++)
            {
                Geo.Line memberLine = GetMemberLineR(faceIndex, i, true);
                this.lines.Add(memberLine);

                // Dispose
                //memberLine.Dispose();
            }

            mesh.faces[faceIndex].visited = true;
            
            // Propogate
            for (int i = 0; i < 4; i++)
            {
                int adjacentFaceIndex = mesh.GetAdjacentFaceIndex(mesh.faces[faceIndex], i);

                if (adjacentFaceIndex >= 0 && !mesh.faces[adjacentFaceIndex].visited)
                {
                    GenerateLeftHand(adjacentFaceIndex);
                }
            }
        }

        internal void GenerateLeftHand(int faceIndex)
        {
            // Generate Lines
            for (int i = 0; i < 4; i++)
            {
                Geo.Line memberLine = GetMemberLineL(faceIndex, i, false);
                this.lines.Add(memberLine);

                // Dispose
                //memberLine.Dispose();
            }

            mesh.faces[faceIndex].visited = true;

            // Propogate
            for (int i = 0; i < 4; i++)
            {
                int adjacentFaceIndex = mesh.GetAdjacentFaceIndex(mesh.faces[faceIndex], i);

                if (adjacentFaceIndex >= 0 && !mesh.faces[adjacentFaceIndex].visited)
                {
                    GenerateRightHand(adjacentFaceIndex);
                }
            }
        }




        #endregion
    }
}
