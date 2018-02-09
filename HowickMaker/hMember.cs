using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Geo = Autodesk.DesignScript.Geometry;
using Autodesk.DesignScript.Interfaces;
using Autodesk.DesignScript.Runtime;

namespace HowickMaker
{
    public class hMember : IGraphicItem
    {
        internal Geo.Line webAxis;
        internal Geo.Vector webNormal;
        internal int name;
        public List<hConnection> connections = new List<hConnection>();
        internal List<hOperation> operations = new List<hOperation>();


        //   ██████╗ ██████╗ ███╗   ██╗███████╗████████╗██████╗ 
        //  ██╔════╝██╔═══██╗████╗  ██║██╔════╝╚══██╔══╝██╔══██╗
        //  ██║     ██║   ██║██╔██╗ ██║███████╗   ██║   ██████╔╝
        //  ██║     ██║   ██║██║╚██╗██║╚════██║   ██║   ██╔══██╗
        //  ╚██████╗╚██████╔╝██║ ╚████║███████║   ██║   ██║  ██║
        //   ╚═════╝ ╚═════╝ ╚═╝  ╚═══╝╚══════╝   ╚═╝   ╚═╝  ╚═╝
        //                                                      


        internal hMember()
        {

        }

        [IsVisibleInDynamoLibrary(false)]
        public hMember(Geo.Line webAxis, Geo.Vector webNormal, int name = 0)
        {
            this.webAxis = webAxis;
            this.webNormal = webNormal;
            this.name = name;
        }

        [IsVisibleInDynamoLibrary(false)]
        public hMember(Geo.Line webAxis, int name = 0)
        {
            this.webAxis = webAxis;
            this.webNormal = null;
            this.name = name;
        }


        //  ██████╗ ██╗   ██╗██████╗      ██████╗███╗   ██╗███████╗████████╗██████╗ 
        //  ██╔══██╗██║   ██║██╔══██╗    ██╔════╝████╗  ██║██╔════╝╚══██╔══╝██╔══██╗
        //  ██████╔╝██║   ██║██████╔╝    ██║     ██╔██╗ ██║███████╗   ██║   ██████╔╝
        //  ██╔═══╝ ██║   ██║██╔══██╗    ██║     ██║╚██╗██║╚════██║   ██║   ██╔══██╗
        //  ██║     ╚██████╔╝██████╔╝    ╚██████╗██║ ╚████║███████║   ██║   ██║  ██║
        //  ╚═╝      ╚═════╝ ╚═════╝      ╚═════╝╚═╝  ╚═══╝╚══════╝   ╚═╝   ╚═╝  ╚═╝
        //          

        public static hMember ByLineVector(Geo.Line webAxis, Geo.Vector webNormal, int name = 0)
        {
            hMember member = new hMember(webAxis, webNormal, name);
            return member;
        }


        //  ██████╗ ██╗   ██╗██████╗  
        //  ██╔══██╗██║   ██║██╔══██╗ 
        //  ██████╔╝██║   ██║██████╔╝  
        //  ██╔═══╝ ██║   ██║██╔══██╗  
        //  ██║     ╚██████╔╝██████╔╝ 
        //  ╚═╝      ╚═════╝ ╚═════╝  
        //          


        public static hMember AddOperationByLocationType(hMember member, double location, string type)
        {
            hOperation op = new hOperation(location, (Operation)System.Enum.Parse(typeof(Operation), type));
            member.AddOperation(op);
            return member;
        }
        public static hMember AddOperationByPointType(hMember member, Geo.Point point, string type)
        {
            member.AddOperationByPointType(point, type);
            return member;
        }

        public static Geo.Line WebAxis(hMember member)
        {
            return member.webAxis;
        }

        public static List<Geo.Line> FlangeAxes(hMember member)
        {
            Geo.Point OP1 = member.webAxis.StartPoint;
            Geo.Point OP2 = member.webAxis.EndPoint;
            Geo.Vector webAxisVec = Geo.Vector.ByTwoPoints(OP1, OP2);
            Geo.Vector normal = member.webNormal.Normalized().Scale(0.75); ;
            Geo.Vector lateral = webAxisVec.Cross(normal).Normalized().Scale(1.75);
            Geo.Line flangeLine1 = Geo.Line.ByStartPointEndPoint(OP1.Add(normal.Add(lateral)), OP2.Add(normal.Add(lateral)));
            lateral = webAxisVec.Cross(normal).Normalized().Scale(-1.75);
            Geo.Line flangeLine2 = Geo.Line.ByStartPointEndPoint(OP2.Add(normal.Add(lateral)), OP2.Add(normal.Add(lateral)));
            return new List<Geo.Line> { flangeLine1, flangeLine2 };
        }


        //  ██╗███╗   ██╗████████╗███████╗██████╗ ███╗   ██╗ █████╗ ██╗     
        //  ██║████╗  ██║╚══██╔══╝██╔════╝██╔══██╗████╗  ██║██╔══██╗██║     
        //  ██║██╔██╗ ██║   ██║   █████╗  ██████╔╝██╔██╗ ██║███████║██║     
        //  ██║██║╚██╗██║   ██║   ██╔══╝  ██╔══██╗██║╚██╗██║██╔══██║██║     
        //  ██║██║ ╚████║   ██║   ███████╗██║  ██║██║ ╚████║██║  ██║███████╗
        //  ╚═╝╚═╝  ╚═══╝   ╚═╝   ╚══════╝╚═╝  ╚═╝╚═╝  ╚═══╝╚═╝  ╚═╝╚══════╝
        //                                                                  

        internal void AddOperationByPointType(Geo.Point pt, string type)
        {
            double location = webAxis.ParameterAtPoint(pt) * webAxis.Length;
            hOperation op = new hOperation(location, (Operation)System.Enum.Parse(typeof(Operation), type));
            AddOperation(op);
        }

        internal void AddOperationByLocationType(double location, string type)
        {
            hOperation op = new hOperation(location, (Operation)System.Enum.Parse(typeof(Operation), type));
            AddOperation(op);
        }


        internal void AddOperation(hOperation operation)
        {
            this.operations.Add(operation);
        }

        internal void AddConnection(hConnection connection)
        {
            this.connections.Add(connection);
        }


        /// <summary>
        /// Extend member by changing web axis start point. Adjust operations accordingly.
        /// </summary>
        /// <param name="newStartPoint"></param>
        internal void SetWebAxisStartPoint(Geo.Point newStartPoint)
        {
            // Create new axis
            Geo.Line newAxis = Geo.Line.ByStartPointEndPoint(newStartPoint, webAxis.EndPoint);

            // Compute new locations for operations relative to new axis
            foreach (hOperation op in operations)
            {
                Geo.Point opPoint = webAxis.PointAtParameter(op._loc / webAxis.Length);
                double newLoc = newAxis.ParameterAtPoint(opPoint) * newAxis.Length;
                op._loc = newLoc;
            }

            // Set new axis
            webAxis = newAxis;
        }

        /// <summary>
        /// Extend member by changing web axis end point. Adjust operations accordingly.
        /// </summary>
        /// <param name="newEndPoint"></param>
        internal void SetWebAxisEndPoint(Geo.Point newEndPoint)
        {
            // Create new axis
            Geo.Line newAxis = Geo.Line.ByStartPointEndPoint(webAxis.StartPoint, newEndPoint);

            // Compute new locations for operations relative to new axis
            foreach (hOperation op in operations)
            {
                Geo.Point opPoint = webAxis.PointAtParameter(op._loc / webAxis.Length);
                double newLoc = newAxis.ParameterAtPoint(opPoint) * newAxis.Length;
                op._loc = newLoc;
            }

            // Set new axis
            webAxis = newAxis;
        }

        internal void SortOperations()
        {
            operations = operations.OrderBy(op => op._loc).ToList();
        }

        //  ███████╗██╗  ██╗██████╗  ██████╗ ██████╗ ████████╗
        //  ██╔════╝╚██╗██╔╝██╔══██╗██╔═══██╗██╔══██╗╚══██╔══╝
        //  █████╗   ╚███╔╝ ██████╔╝██║   ██║██████╔╝   ██║   
        //  ██╔══╝   ██╔██╗ ██╔═══╝ ██║   ██║██╔══██╗   ██║   
        //  ███████╗██╔╝ ██╗██║     ╚██████╔╝██║  ██║   ██║   
        //  ╚══════╝╚═╝  ╚═╝╚═╝      ╚═════╝ ╚═╝  ╚═╝   ╚═╝   
        //                                   


        /// <summary>
        /// Export a list of hMembers to csv file for Howick production
        /// </summary>
        /// <param name="filePath"></param>
        /// <param name="hMembers"></param>
        /// <param name="normalLabel"></param>
        public static void ExportToFile(string filePath, List<hMember> hMembers, bool normalLabel = true)
        {
            string csv = "";

            foreach (hMember member in hMembers)
            {
                csv += member.AsCSVLine(normalLabel);
                csv += "\n";
            }

            File.WriteAllText(filePath, csv);
        }

        /// <summary>
        /// Returns a string representing the hMember as a csv line to be produced on the Howick
        /// </summary>
        /// <param name="member"></param>
        /// <param name="normalLabel"></param>
        /// <returns></returns>
        public static string AsCSVLine(hMember member, bool normalLabel = true)
        {
            return member.AsCSVLine(normalLabel);
        }

        /// <summary>
        /// Returns a string representing the hMember as a csv line to be produced on the Howick
        /// </summary>
        /// <param name="normalLabel"></param>
        /// <returns></returns>
        internal string AsCSVLine(bool normalLabel = true)
        {
            string csv = "";
            csv += name.ToString() + "," + "COMPONET" + ",";
            csv += (normalLabel) ? "LABEL_NRM" + "," : "LABEL_INV" + ",";
            csv += Math.Round(webAxis.Length, 2).ToString() + ",";

            SortOperations();
            foreach (hOperation op in operations)
            {
                csv += op._type.ToString() + "," + Math.Round(op._loc, 2).ToString() + ",";
            }

            return csv;
        }







        //  ██╗   ██╗██╗███████╗
        //  ██║   ██║██║╚══███╔╝
        //  ██║   ██║██║  ███╔╝ 
        //  ╚██╗ ██╔╝██║ ███╔╝  
        //   ╚████╔╝ ██║███████╗
        //    ╚═══╝  ╚═╝╚══════╝
        //                      

        [MultiReturn(new[] { "member", "operations" })]
        public static Dictionary<string, object> Draw(hMember member)
        {
            Geo.Point OP1 = member.webAxis.StartPoint;
            Geo.Point OP2 = member.webAxis.EndPoint;

            Geo.Vector webAxis = Geo.Vector.ByTwoPoints(OP1, OP2);
            Geo.Vector normal = member.webNormal;
            Geo.Vector lateral = webAxis.Cross(normal);
            lateral = lateral.Normalized();
            lateral = lateral.Scale(1.75);
            normal = normal.Normalized();
            normal = normal.Scale(1.5);
            Geo.Vector lateralR = Geo.Vector.ByCoordinates(lateral.X * -1, lateral.Y * -1, lateral.Z * -1);

            Geo.Point p0 = OP1.Add(normal.Add(lateral));
            Geo.Point p1 = OP2.Add(normal.Add(lateral));
            Geo.Point p2 = OP1.Add(lateral);
            Geo.Point p3 = OP2.Add(lateral);
            Geo.Point p6 = OP1.Add(normal.Add(lateralR));
            Geo.Point p7 = OP2.Add(normal.Add(lateralR));
            Geo.Point p4 = OP1.Add(lateralR);
            Geo.Point p5 = OP2.Add(lateralR);

            Geo.Point[] pts = { p0, p1, p2, p1, p2, p3, p2, p3, p4, p3, p4, p5, p4, p5, p6, p5, p6, p7 };

            Geo.IndexGroup g0 = Geo.IndexGroup.ByIndices(0, 1, 2);
            Geo.IndexGroup g1 = Geo.IndexGroup.ByIndices(3, 4, 5);
            Geo.IndexGroup g2 = Geo.IndexGroup.ByIndices(6, 7, 8);
            Geo.IndexGroup g3 = Geo.IndexGroup.ByIndices(9, 10, 11);
            Geo.IndexGroup g4 = Geo.IndexGroup.ByIndices(12, 13, 14);
            Geo.IndexGroup g5 = Geo.IndexGroup.ByIndices(15, 16, 17);

            Geo.IndexGroup[] ig = { g0, g1, g2, g3, g4, g5 };

            Geo.Mesh mesh = Geo.Mesh.ByPointsFaceIndices(pts, ig);

            var points = new List<Geo.Point>();

            foreach (hOperation op in member.operations)
            {
                points.Add(member.webAxis.PointAtParameter(op._loc / member.webAxis.Length));
            }

            return new Dictionary<string, object>
            {
                { "member", mesh },
                { "operations", points }
            };

        }


        #region IGraphicItem Interface

        [IsVisibleInDynamoLibrary(false)]
        public void Tessellate(IRenderPackage package, TessellationParameters parameters)
        {
            package.RequiresPerVertexColoration = true;

            Geo.Point OP1 = this.webAxis.StartPoint;
            Geo.Point OP2 = this.webAxis.EndPoint;

            Geo.Vector webAxis = Geo.Vector.ByTwoPoints(OP1, OP2);
            Geo.Vector normal = webNormal;
            Geo.Vector lateral = webAxis.Cross(normal);
            lateral = lateral.Normalized();
            lateral = lateral.Scale(1.75);
            normal = normal.Normalized();
            normal = normal.Scale(1.5);
            Geo.Vector lateralR = Geo.Vector.ByCoordinates(lateral.X * -1, lateral.Y * -1, lateral.Z * -1);
            Geo.Vector webAxisR = Geo.Vector.ByCoordinates(webAxis.X * -1, webAxis.Y * -1, webAxis.Z * -1);
            Geo.Vector normalR = Geo.Vector.ByCoordinates(normal.X * -1, normal.Y * -1, normal.Z * -1);

            Geo.Point p0 = OP1.Add(normal.Add(lateral));
            Geo.Point p1 = OP2.Add(normal.Add(lateral));
            Geo.Point p2 = OP1.Add(lateral);
            Geo.Point p3 = OP2.Add(lateral);
            Geo.Point p6 = OP1.Add(normal.Add(lateralR));
            Geo.Point p7 = OP2.Add(normal.Add(lateralR));
            Geo.Point p4 = OP1.Add(lateralR);
            Geo.Point p5 = OP2.Add(lateralR);
            lateral = lateral.Normalized().Scale(1.25);
            lateralR = lateralR.Normalized().Scale(1.25);
            Geo.Point p8 = OP1.Add(lateralR).Add(normal);
            Geo.Point p9 = OP2.Add(lateralR).Add(normal);
            Geo.Point p10 = OP1.Add(lateral).Add(normal);
            Geo.Point p11 = OP2.Add(lateral).Add(normal);

            Geo.Point[] pts = { p0, p1, p2, p1, p2, p3, p2, p3, p4, p3, p4, p5, p4, p5, p6, p5, p6, p7, p0, p1, p10, p1, p10, p11, p6, p7, p8, p7, p8, p9 };
            Geo.Vector[] vectors = { lateral, normal, lateral.Reverse(), normal.Reverse(), normal.Reverse() };

            var faces = new List<List<int>>
            {
                new List<int> { 0, 1, 2 },
                new List<int> { 3, 4, 5 },
                new List<int> { 6, 7, 8 },
                new List<int> { 9, 10, 11 },
                new List<int> { 12, 13, 14 },
                new List<int> { 15, 16, 17 },
                new List<int> { 18, 19, 20 },
                new List<int> { 21, 22, 23 },
                new List<int> { 24, 25, 26 },
                new List<int> { 27, 28, 29 }
            };

            for (int i = 0; i < faces.Count; i++)
            {
                package.AddTriangleVertex(pts[faces[i][0]].X, pts[faces[i][0]].Y, pts[faces[i][0]].Z);
                package.AddTriangleVertex(pts[faces[i][1]].X, pts[faces[i][1]].Y, pts[faces[i][1]].Z);
                package.AddTriangleVertex(pts[faces[i][2]].X, pts[faces[i][2]].Y, pts[faces[i][2]].Z);
                package.AddTriangleVertexColor(100, 100, 100, 255);
                package.AddTriangleVertexColor(100, 100, 100, 255);
                package.AddTriangleVertexColor(100, 100, 100, 255);
                package.AddTriangleVertexNormal(vectors[i / 2].X, vectors[i / 2].Y, vectors[i / 2].Z);
                package.AddTriangleVertexNormal(vectors[i / 2].X, vectors[i / 2].Y, vectors[i / 2].Z);
                package.AddTriangleVertexNormal(vectors[i / 2].X, vectors[i / 2].Y, vectors[i / 2].Z);
                package.AddTriangleVertexUV(0, 0);
                package.AddTriangleVertexUV(0, 0);
                package.AddTriangleVertexUV(0, 0);
            }

            foreach (hOperation op in operations)
            {
                Geo.Point opPoint = this.webAxis.PointAtParameter(op._loc / this.webAxis.Length);

                switch (op._type)
                {
                    case Operation.WEB:
                        package.AddPointVertex(opPoint.X, opPoint.Y, opPoint.Z);
                        package.AddPointVertexColor(255, 0, 0, 255);

                        lateral = lateral.Normalized().Scale(15.0 / 16.0);
                        lateralR = lateralR.Normalized().Scale(15.0 / 16.0);

                        package.AddPointVertex(opPoint.Add(lateral).X, opPoint.Add(lateral).Y, opPoint.Add(lateral).Z);
                        package.AddPointVertexColor(255, 0, 0, 255);
                        package.AddPointVertex(opPoint.Add(lateralR).X, opPoint.Add(lateralR).Y, opPoint.Add(lateralR).Z);
                        package.AddPointVertexColor(255, 0, 0, 255);
                        break;

                    case Operation.DIMPLE:
                        lateral = lateral.Normalized().Scale(1.0);
                        lateralR = lateralR.Normalized().Scale(1.0);
                        normal = normal.Normalized().Scale(0.75);
                        
                        package.AddLineStripVertex(opPoint.Add(lateral.Add(normal)).X, opPoint.Add(lateral.Add(normal)).Y, opPoint.Add(lateral.Add(normal)).Z);
                        lateral = lateral.Normalized().Scale(2.5);
                        package.AddLineStripVertex(opPoint.Add(lateral.Add(normal)).X, opPoint.Add(lateral.Add(normal)).Y, opPoint.Add(lateral.Add(normal)).Z);
                        package.AddLineStripVertexColor(255, 0, 0, 255);
                        package.AddLineStripVertexColor(255, 0, 0, 255);
                        package.AddLineStripVertexCount(2);

                        lateral = lateral.Normalized().Scale(1.75);
                        package.AddPointVertex(opPoint.Add(lateral.Add(normal)).X, opPoint.Add(lateral.Add(normal)).Y, opPoint.Add(lateral.Add(normal)).Z);
                        package.AddPointVertexColor(255, 0, 0, 255);

                        package.AddLineStripVertex(opPoint.Add(lateralR.Add(normal)).X, opPoint.Add(lateralR.Add(normal)).Y, opPoint.Add(lateralR.Add(normal)).Z);
                        lateralR = lateralR.Normalized().Scale(2.5);
                        package.AddLineStripVertex(opPoint.Add(lateralR.Add(normal)).X, opPoint.Add(lateralR.Add(normal)).Y, opPoint.Add(lateralR.Add(normal)).Z);
                        package.AddLineStripVertexColor(255, 0, 0, 255);
                        package.AddLineStripVertexColor(255, 0, 0, 255);
                        package.AddLineStripVertexCount(2);

                        lateralR = lateralR.Normalized().Scale(1.75);
                        package.AddPointVertex(opPoint.Add(lateralR.Add(normal)).X, opPoint.Add(lateralR.Add(normal)).Y, opPoint.Add(lateralR.Add(normal)).Z);
                        package.AddPointVertexColor(255, 0, 0, 255);
                        break;

                    case Operation.BOLT:
                        lateral = lateral.Normalized().Scale(0.25);
                        lateralR = lateralR.Normalized().Scale(0.25);
                        webAxis = webAxis.Normalized().Scale(0.25);
                        webAxisR = webAxisR.Normalized().Scale(0.25);
                        normal = normal.Normalized().Scale(0.05);
                        normalR = normalR.Normalized().Scale(0.05);

                        Geo.Point[] boltPts = { opPoint.Add(lateral.Add(webAxis.Add(normal))), opPoint.Add(lateral.Add(webAxisR.Add(normal))), opPoint.Add(lateralR.Add(webAxisR.Add(normal))), opPoint.Add(lateralR.Add(webAxis.Add(normal))) };
                        package.AddTriangleVertex(boltPts[0].X, boltPts[0].Y, boltPts[0].Z);
                        package.AddTriangleVertex(boltPts[1].X, boltPts[1].Y, boltPts[1].Z);
                        package.AddTriangleVertex(boltPts[2].X, boltPts[2].Y, boltPts[2].Z);
                        package.AddTriangleVertexColor(255, 0, 0, 255);
                        package.AddTriangleVertexColor(255, 0, 0, 255);
                        package.AddTriangleVertexColor(255, 0, 0, 255);
                        package.AddTriangleVertexNormal(normal.X, normal.Y, normal.Z);
                        package.AddTriangleVertexNormal(normal.X, normal.Y, normal.Z);
                        package.AddTriangleVertexNormal(normal.X, normal.Y, normal.Z);
                        package.AddTriangleVertexUV(0, 0);
                        package.AddTriangleVertexUV(0, 0);
                        package.AddTriangleVertexUV(0, 0);
                        
                        package.AddTriangleVertex(boltPts[0].X, boltPts[0].Y, boltPts[0].Z);
                        package.AddTriangleVertex(boltPts[2].X, boltPts[2].Y, boltPts[2].Z);
                        package.AddTriangleVertex(boltPts[3].X, boltPts[3].Y, boltPts[3].Z);
                        package.AddTriangleVertexColor(255, 0, 0, 255);
                        package.AddTriangleVertexColor(255, 0, 0, 255);
                        package.AddTriangleVertexColor(255, 0, 0, 255);
                        package.AddTriangleVertexNormal(normal.X, normal.Y, normal.Z);
                        package.AddTriangleVertexNormal(normal.X, normal.Y, normal.Z);
                        package.AddTriangleVertexNormal(normal.X, normal.Y, normal.Z);
                        package.AddTriangleVertexUV(0, 0);
                        package.AddTriangleVertexUV(0, 0);
                        package.AddTriangleVertexUV(0, 0);

                        boltPts = new Geo.Point[] { opPoint.Add(lateral.Add(webAxis.Add(normalR))), opPoint.Add(lateral.Add(webAxisR.Add(normalR))), opPoint.Add(lateralR.Add(webAxisR.Add(normalR))), opPoint.Add(lateralR.Add(webAxis.Add(normalR))) };
                        package.AddTriangleVertex(boltPts[0].X, boltPts[0].Y, boltPts[0].Z);
                        package.AddTriangleVertex(boltPts[1].X, boltPts[1].Y, boltPts[1].Z);
                        package.AddTriangleVertex(boltPts[2].X, boltPts[2].Y, boltPts[2].Z);
                        package.AddTriangleVertexColor(255, 0, 0, 255);
                        package.AddTriangleVertexColor(255, 0, 0, 255);
                        package.AddTriangleVertexColor(255, 0, 0, 255);
                        package.AddTriangleVertexNormal(normal.X, normal.Y, normal.Z);
                        package.AddTriangleVertexNormal(normal.X, normal.Y, normal.Z);
                        package.AddTriangleVertexNormal(normal.X, normal.Y, normal.Z);
                        package.AddTriangleVertexUV(0, 0);
                        package.AddTriangleVertexUV(0, 0);
                        package.AddTriangleVertexUV(0, 0);

                        package.AddTriangleVertex(boltPts[0].X, boltPts[0].Y, boltPts[0].Z);
                        package.AddTriangleVertex(boltPts[2].X, boltPts[2].Y, boltPts[2].Z);
                        package.AddTriangleVertex(boltPts[3].X, boltPts[3].Y, boltPts[3].Z);
                        package.AddTriangleVertexColor(255, 0, 0, 255);
                        package.AddTriangleVertexColor(255, 0, 0, 255);
                        package.AddTriangleVertexColor(255, 0, 0, 255);
                        package.AddTriangleVertexNormal(normal.X, normal.Y, normal.Z);
                        package.AddTriangleVertexNormal(normal.X, normal.Y, normal.Z);
                        package.AddTriangleVertexNormal(normal.X, normal.Y, normal.Z);
                        package.AddTriangleVertexUV(0, 0);
                        package.AddTriangleVertexUV(0, 0);
                        package.AddTriangleVertexUV(0, 0);
                        break;

                    case Operation.NOTCH:
                        lateral = lateral.Normalized().Scale(1.75);
                        lateralR = lateralR.Normalized().Scale(1.75);
                        webAxis = webAxis.Normalized().Scale(0.875);
                        webAxisR = webAxisR.Normalized().Scale(0.875);
                        normal = normal.Normalized().Scale(0.05);
                        normalR = normalR.Normalized().Scale(0.05);

                        Geo.Point[] notchPts = { opPoint.Add(lateral.Add(webAxis.Add(normal))), opPoint.Add(lateral.Add(webAxisR.Add(normal))), opPoint.Add(lateralR.Add(webAxisR.Add(normal))), opPoint.Add(lateralR.Add(webAxis.Add(normal))) };
                        package.AddTriangleVertex(notchPts[0].X, notchPts[0].Y, notchPts[0].Z);
                        package.AddTriangleVertex(notchPts[1].X, notchPts[1].Y, notchPts[1].Z);
                        package.AddTriangleVertex(notchPts[2].X, notchPts[2].Y, notchPts[2].Z);
                        package.AddTriangleVertexColor(255, 0, 0, 255);
                        package.AddTriangleVertexColor(255, 0, 0, 255);
                        package.AddTriangleVertexColor(255, 0, 0, 255);
                        package.AddTriangleVertexNormal(normal.X, normal.Y, normal.Z);
                        package.AddTriangleVertexNormal(normal.X, normal.Y, normal.Z);
                        package.AddTriangleVertexNormal(normal.X, normal.Y, normal.Z);
                        package.AddTriangleVertexUV(0, 0);
                        package.AddTriangleVertexUV(0, 0);
                        package.AddTriangleVertexUV(0, 0);

                        package.AddTriangleVertex(notchPts[0].X, notchPts[0].Y, notchPts[0].Z);
                        package.AddTriangleVertex(notchPts[2].X, notchPts[2].Y, notchPts[2].Z);
                        package.AddTriangleVertex(notchPts[3].X, notchPts[3].Y, notchPts[3].Z);
                        package.AddTriangleVertexColor(255, 0, 0, 255);
                        package.AddTriangleVertexColor(255, 0, 0, 255);
                        package.AddTriangleVertexColor(255, 0, 0, 255);
                        package.AddTriangleVertexNormal(normal.X, normal.Y, normal.Z);
                        package.AddTriangleVertexNormal(normal.X, normal.Y, normal.Z);
                        package.AddTriangleVertexNormal(normal.X, normal.Y, normal.Z);
                        package.AddTriangleVertexUV(0, 0);
                        package.AddTriangleVertexUV(0, 0);
                        package.AddTriangleVertexUV(0, 0);


                        notchPts = new Geo.Point[] { opPoint.Add(lateral.Add(webAxis.Add(normalR))), opPoint.Add(lateral.Add(webAxisR.Add(normalR))), opPoint.Add(lateralR.Add(webAxisR.Add(normalR))), opPoint.Add(lateralR.Add(webAxis.Add(normalR))) };
                        package.AddTriangleVertex(notchPts[0].X, notchPts[0].Y, notchPts[0].Z);
                        package.AddTriangleVertex(notchPts[1].X, notchPts[1].Y, notchPts[1].Z);
                        package.AddTriangleVertex(notchPts[2].X, notchPts[2].Y, notchPts[2].Z);
                        package.AddTriangleVertexColor(255, 0, 0, 255);
                        package.AddTriangleVertexColor(255, 0, 0, 255);
                        package.AddTriangleVertexColor(255, 0, 0, 255);
                        package.AddTriangleVertexNormal(normal.X, normal.Y, normal.Z);
                        package.AddTriangleVertexNormal(normal.X, normal.Y, normal.Z);
                        package.AddTriangleVertexNormal(normal.X, normal.Y, normal.Z);
                        package.AddTriangleVertexUV(0, 0);
                        package.AddTriangleVertexUV(0, 0);
                        package.AddTriangleVertexUV(0, 0);

                        package.AddTriangleVertex(notchPts[0].X, notchPts[0].Y, notchPts[0].Z);
                        package.AddTriangleVertex(notchPts[2].X, notchPts[2].Y, notchPts[2].Z);
                        package.AddTriangleVertex(notchPts[3].X, notchPts[3].Y, notchPts[3].Z);
                        package.AddTriangleVertexColor(255, 0, 0, 255);
                        package.AddTriangleVertexColor(255, 0, 0, 255);
                        package.AddTriangleVertexColor(255, 0, 0, 255);
                        package.AddTriangleVertexNormal(normal.X, normal.Y, normal.Z);
                        package.AddTriangleVertexNormal(normal.X, normal.Y, normal.Z);
                        package.AddTriangleVertexNormal(normal.X, normal.Y, normal.Z);
                        package.AddTriangleVertexUV(0, 0);
                        package.AddTriangleVertexUV(0, 0);
                        package.AddTriangleVertexUV(0, 0);
                        break;

                    default:
                        package.AddPointVertex(opPoint.X, opPoint.Y, opPoint.Z);
                        package.AddPointVertexColor(255, 0, 0, 255);
                        break;

                }

            }
        }

 
        #endregion
    }
}
