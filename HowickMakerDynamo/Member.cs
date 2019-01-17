using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HM = HowickMaker;
using Geo = Autodesk.DesignScript.Geometry;
using Autodesk.DesignScript.Interfaces;
using Autodesk.DesignScript.Runtime;

namespace HowickMakerDynamo
{
    public class Member : HM.hMember, IGraphicItem
    {
        private Member(HM.Line webAxis, HM.Triple webNormal, string name = "0") : base(webAxis, webNormal, name)
        {
        }

        public Member(HM.hMember member) : base(member)
        {
        }

        //   __    ___   ____   __   _____  ____ 
        //  / /`  | |_) | |_   / /\   | |  | |_  
        //  \_\_, |_| \ |_|__ /_/--\  |_|  |_|__                  


        /// <summary>
        /// Creates an hMember with its web lying along the webAxis, facing webNormal
        /// </summary>
        /// <param name="webAxis"></param>
        /// <param name="webNormal"></param>
        /// <param name="name"></param>
        /// <returns name="hMember"></returns>
        public static Member ByLineVector(Geo.Line webAxis, Geo.Vector webNormal, string name = "0")
        {
            // DS to HM conversions
            var hWebAxis = HMDynamoUtil.DSLineToHMLine(webAxis);
            var hWebNormal = HMDynamoUtil.VectorToTriple(webNormal);

            var member = new Member(hWebAxis, hWebNormal.Normalized(), name);
            return member;
        }



        //    __    __   _____  _   ___   _     
        //   / /\  / /`   | |  | | / / \ | |\ | 
        //  /_/--\ \_\_,  |_|  |_| \_\_/ |_| \| 


        /// <summary>
        /// Add or change the name of an hMember. This will be used as the label on the steel stud.
        /// </summary>
        /// <param name="member"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        public static Member AddName(Member member, string name)
        {
            var newMember = new Member(member);
            newMember.Name = name;
            return newMember;
        }

        /// <summary>
        /// Adds operations to the member by specifying the locations and the types of operations
        /// </summary>
        /// <param name="member"></param>
        /// <param name="locations"></param>
        /// <param name="types"></param>
        /// <returns name="hMember"></returns>
        public static Member AddOperationByLocationType(Member member, List<double> locations, List<string> types)
        {
            var newMember = new Member(member);
            for (int i = 0; i < locations.Count; i++)
            {
                var op = new HM.hOperation(locations[i], (HM.Operation)System.Enum.Parse(typeof(HM.Operation), types[i]));
                newMember.AddOperation(op);
            }
            return newMember;
        }

        /// <summary>
        /// Adds operations to a member by specifiying the operation types and the points along the axis of the member at which they occur
        /// </summary>
        /// <param name="member"></param>
        /// <param name="points"></param>
        /// <param name="types"></param>
        /// <returns name="hMember">></returns>
        public static Member AddOperationByPointType(Member member, List<Geo.Point> points, List<string> types)
        {
            var newMember = new Member(member);
            for (int i = 0; i < points.Count; i++)
            {
                newMember.AddOperationByPointType(HMDynamoUtil.PointToTriple(points[i]), types[i]);
            }
            return newMember;
        }



        //   ___    _     ____  ___   _    
        //  / / \  | | | | |_  | |_) \ \_/ 
        //  \_\_\\ \_\_/ |_|__ |_| \  |_|  


        /// <summary>
        /// Gets the lines that run along the center of each flange of the member, parallel to the web axis
        /// </summary>
        /// <param name="member"></param>
        /// <returns></returns>
        [IsVisibleInDynamoLibrary(true)]
        public static List<Geo.Line> GetFlangeAxes(Member member)
        {
            var lines = member.FlangeAxes;
            return new List<Geo.Line> { HMDynamoUtil.HMLineToDSLine(lines[0]), HMDynamoUtil.HMLineToDSLine(lines[1]) };
        }



        //   _      _   __   _      __    _     _  ____  ____ 
        //  \ \  / | | ( (` | | |  / /\  | |   | |  / / | |_  
        //   \_\/  |_| _)_) \_\_/ /_/--\ |_|__ |_| /_/_ |_|__                


        [IsVisibleInDynamoLibrary(true)]
        [MultiReturn(new[] { "member", "operations" })]
        public static Dictionary<string, object> DrawAsMesh(Member member)
        {
            var DSWebAxis = HMDynamoUtil.HMLineToDSLine(member.WebAxis);
            var DSWebNormal = HMDynamoUtil.TripleToVector(member.WebNormal);

            Geo.Point OP1 = DSWebAxis.StartPoint;
            Geo.Point OP2 = DSWebAxis.EndPoint;

            Geo.Vector webAxis = Geo.Vector.ByTwoPoints(OP1, OP2);
            Geo.Vector normal = DSWebNormal;
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

            foreach (HM.hOperation op in member.Operations)
            {
                points.Add(HMDynamoUtil.TripleToPoint(member.WebAxis.PointAtParameter(op._loc / member.WebAxis.Length)));
            }

            return new Dictionary<string, object>
            {
                { "member", mesh },
                { "operations", points }
            };

        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="member"></param>
        /// <returns></returns>
        [IsVisibleInDynamoLibrary(true)]
        [MultiReturn(new[] { "member", "operations" })]
        public static Dictionary<string, object> DrawAsPolysurface(Member member)
        {
            var DSWebAxis = HMDynamoUtil.HMLineToDSLine(member.WebAxis);
            var DSWebNormal = HMDynamoUtil.TripleToVector(member.WebNormal);

            Geo.Point OP1 = DSWebAxis.StartPoint;
            Geo.Point OP2 = DSWebAxis.EndPoint;

            Geo.Vector webAxis = Geo.Vector.ByTwoPoints(OP1, OP2);
            Geo.Vector normal = DSWebNormal;
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


            Geo.Surface flange1 = Geo.Surface.ByPerimeterPoints(new Geo.Point[] { p0, p1, p3, p2 });
            Geo.Surface flange2 = Geo.Surface.ByPerimeterPoints(new Geo.Point[] { p4, p5, p7, p6 });
            Geo.Surface web = Geo.Surface.ByPerimeterPoints(new Geo.Point[] { p2, p3, p5, p4 });
            Geo.Surface lip1 = Geo.Surface.ByPerimeterPoints(new Geo.Point[] { p0, p1, p11, p10 });
            Geo.Surface lip2 = Geo.Surface.ByPerimeterPoints(new Geo.Point[] { p6, p7, p9, p8 });


            /*
            // Get operation curves
            var opCurves = new List<Geo.Curve>();
            foreach (hOperation op in member.operations)
            {
                opCurves.AddRange(member.GetOperationCutter(op));
            }*/

            // Build Polysurface
            var surfs = new Geo.Surface[] {
                flange1,
                flange2,
                web//,
                //lip1,
                //lip2
            };
            var stud = Geo.PolySurface.ByJoinedSurfaces(surfs);

            // Dispose
            {
                OP1.Dispose();
                OP2.Dispose();
                lateral.Dispose();
                lateralR.Dispose();
                webAxisR.Dispose();
                normalR.Dispose();
                p0.Dispose();
                p1.Dispose();
                p2.Dispose();
                p3.Dispose();
                p6.Dispose();
                p7.Dispose();
                p4.Dispose();
                p5.Dispose();
                p8.Dispose();
                p9.Dispose();
                p10.Dispose();
                p11.Dispose();
                flange1.Dispose();
                flange2.Dispose();
                web.Dispose();
                lip1.Dispose();
                lip2.Dispose();
            }

            return new Dictionary<string, object>
            {
                { "member", stud },
                { "operations", null }
            };
        }

        #region IGraphicItem Interface

        internal void TessellateOperation(IRenderPackage package, TessellationParameters parameters, HM.hOperation op)
        {
            package.RequiresPerVertexColoration = true;

            var DSWebAxis = HMDynamoUtil.HMLineToDSLine(this.WebAxis);
            var DSWebNormal = HMDynamoUtil.TripleToVector(this.WebNormal);

            Geo.Point OP1 = DSWebAxis.StartPoint;
            Geo.Point OP2 = DSWebAxis.EndPoint;

            Geo.Vector webAxis = Geo.Vector.ByTwoPoints(OP1, OP2);
            Geo.Vector normal = DSWebNormal;
            Geo.Vector lateral = webAxis.Cross(normal);
            lateral = lateral.Normalized();
            lateral = lateral.Scale(1.75);
            normal = normal.Normalized();
            normal = normal.Scale(1.5);
            Geo.Vector lateralR = Geo.Vector.ByCoordinates(lateral.X * -1, lateral.Y * -1, lateral.Z * -1);
            Geo.Vector webAxisR = Geo.Vector.ByCoordinates(webAxis.X * -1, webAxis.Y * -1, webAxis.Z * -1);
            Geo.Vector normalR = Geo.Vector.ByCoordinates(normal.X * -1, normal.Y * -1, normal.Z * -1);

            Geo.Point opPoint = HMDynamoUtil.TripleToPoint(this.WebAxis.PointAtParameter(op._loc / this.WebAxis.Length));
            byte r = 255;
            byte g = 66;
            byte b = 57;
            byte a = 255;
            switch (op._type)
            {
                case HM.Operation.WEB:
                    package.AddPointVertex(opPoint.X, opPoint.Y, opPoint.Z);
                    package.AddPointVertexColor(r, g, b, a);

                    lateral = lateral.Normalized().Scale(15.0 / 16.0);
                    lateralR = lateralR.Normalized().Scale(15.0 / 16.0);

                    package.AddPointVertex(opPoint.Add(lateral).X, opPoint.Add(lateral).Y, opPoint.Add(lateral).Z);
                    package.AddPointVertexColor(r, g, b, a);
                    package.AddPointVertex(opPoint.Add(lateralR).X, opPoint.Add(lateralR).Y, opPoint.Add(lateralR).Z);
                    package.AddPointVertexColor(r, g, b, a);
                    break;

                case HM.Operation.DIMPLE:
                    lateral = lateral.Normalized().Scale(1.0);
                    lateralR = lateralR.Normalized().Scale(1.0);
                    normal = normal.Normalized().Scale(0.75);

                    package.AddLineStripVertex(opPoint.Add(lateral.Add(normal)).X, opPoint.Add(lateral.Add(normal)).Y, opPoint.Add(lateral.Add(normal)).Z);
                    lateral = lateral.Normalized().Scale(2.5);
                    package.AddLineStripVertex(opPoint.Add(lateral.Add(normal)).X, opPoint.Add(lateral.Add(normal)).Y, opPoint.Add(lateral.Add(normal)).Z);
                    package.AddLineStripVertexColor(r, g, b, a);
                    package.AddLineStripVertexColor(r, g, b, a);
                    package.AddLineStripVertexCount(2);

                    lateral = lateral.Normalized().Scale(1.75);
                    package.AddPointVertex(opPoint.Add(lateral.Add(normal)).X, opPoint.Add(lateral.Add(normal)).Y, opPoint.Add(lateral.Add(normal)).Z);
                    package.AddPointVertexColor(r, g, b, a);

                    package.AddLineStripVertex(opPoint.Add(lateralR.Add(normal)).X, opPoint.Add(lateralR.Add(normal)).Y, opPoint.Add(lateralR.Add(normal)).Z);
                    lateralR = lateralR.Normalized().Scale(2.5);
                    package.AddLineStripVertex(opPoint.Add(lateralR.Add(normal)).X, opPoint.Add(lateralR.Add(normal)).Y, opPoint.Add(lateralR.Add(normal)).Z);
                    package.AddLineStripVertexColor(r, g, b, a);
                    package.AddLineStripVertexColor(r, g, b, a);
                    package.AddLineStripVertexCount(2);

                    lateralR = lateralR.Normalized().Scale(1.75);
                    package.AddPointVertex(opPoint.Add(lateralR.Add(normal)).X, opPoint.Add(lateralR.Add(normal)).Y, opPoint.Add(lateralR.Add(normal)).Z);
                    package.AddPointVertexColor(r, g, b, a);
                    break;

                case HM.Operation.SWAGE:
                    lateral = lateral.Normalized().Scale(1.25);
                    lateralR = lateralR.Normalized().Scale(1.25);
                    webAxis = webAxis.Normalized().Scale(.875);
                    webAxisR = webAxisR.Normalized().Scale(.875);

                    package.AddLineStripVertex(opPoint.Add(lateral.Add(webAxis)).X, opPoint.Add(lateral.Add(webAxis)).Y, opPoint.Add(lateral.Add(webAxis)).Z);
                    package.AddLineStripVertex(opPoint.Add(lateral.Add(webAxisR)).X, opPoint.Add(lateral.Add(webAxisR)).Y, opPoint.Add(lateral.Add(webAxisR)).Z);
                    package.AddLineStripVertexColor(r, g, b, a);
                    package.AddLineStripVertexColor(r, g, b, a);
                    package.AddLineStripVertexCount(2);

                    package.AddLineStripVertex(opPoint.Add(lateralR.Add(webAxis)).X, opPoint.Add(lateralR.Add(webAxis)).Y, opPoint.Add(lateralR.Add(webAxis)).Z);
                    package.AddLineStripVertex(opPoint.Add(lateralR.Add(webAxisR)).X, opPoint.Add(lateralR.Add(webAxisR)).Y, opPoint.Add(lateralR.Add(webAxisR)).Z);
                    package.AddLineStripVertexColor(r, g, b, a);
                    package.AddLineStripVertexColor(r, g, b, a);
                    package.AddLineStripVertexCount(2);
                    break;

                case HM.Operation.BOLT:
                    lateral = lateral.Normalized().Scale(0.25);
                    lateralR = lateralR.Normalized().Scale(0.25);
                    webAxis = webAxis.Normalized().Scale(0.25);
                    webAxisR = webAxisR.Normalized().Scale(0.25);
                    normal = normal.Normalized().Scale(0.01);
                    normalR = normalR.Normalized().Scale(0.01);

                    Geo.Point[] boltPts = { opPoint.Add(lateral.Add(webAxis.Add(normal))), opPoint.Add(lateral.Add(webAxisR.Add(normal))), opPoint.Add(lateralR.Add(webAxisR.Add(normal))), opPoint.Add(lateralR.Add(webAxis.Add(normal))) };
                    package.AddTriangleVertex(boltPts[0].X, boltPts[0].Y, boltPts[0].Z);
                    package.AddTriangleVertex(boltPts[1].X, boltPts[1].Y, boltPts[1].Z);
                    package.AddTriangleVertex(boltPts[2].X, boltPts[2].Y, boltPts[2].Z);
                    package.AddTriangleVertexColor(r, g, b, a);
                    package.AddTriangleVertexColor(r, g, b, a);
                    package.AddTriangleVertexColor(r, g, b, a);
                    package.AddTriangleVertexNormal(normal.X, normal.Y, normal.Z);
                    package.AddTriangleVertexNormal(normal.X, normal.Y, normal.Z);
                    package.AddTriangleVertexNormal(normal.X, normal.Y, normal.Z);
                    package.AddTriangleVertexUV(0, 0);
                    package.AddTriangleVertexUV(0, 0);
                    package.AddTriangleVertexUV(0, 0);

                    package.AddTriangleVertex(boltPts[0].X, boltPts[0].Y, boltPts[0].Z);
                    package.AddTriangleVertex(boltPts[2].X, boltPts[2].Y, boltPts[2].Z);
                    package.AddTriangleVertex(boltPts[3].X, boltPts[3].Y, boltPts[3].Z);
                    package.AddTriangleVertexColor(r, g, b, a);
                    package.AddTriangleVertexColor(r, g, b, a);
                    package.AddTriangleVertexColor(r, g, b, a);
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
                    package.AddTriangleVertexColor(r, g, b, a);
                    package.AddTriangleVertexColor(r, g, b, a);
                    package.AddTriangleVertexColor(r, g, b, a);
                    package.AddTriangleVertexNormal(normal.X, normal.Y, normal.Z);
                    package.AddTriangleVertexNormal(normal.X, normal.Y, normal.Z);
                    package.AddTriangleVertexNormal(normal.X, normal.Y, normal.Z);
                    package.AddTriangleVertexUV(0, 0);
                    package.AddTriangleVertexUV(0, 0);
                    package.AddTriangleVertexUV(0, 0);

                    package.AddTriangleVertex(boltPts[0].X, boltPts[0].Y, boltPts[0].Z);
                    package.AddTriangleVertex(boltPts[2].X, boltPts[2].Y, boltPts[2].Z);
                    package.AddTriangleVertex(boltPts[3].X, boltPts[3].Y, boltPts[3].Z);
                    package.AddTriangleVertexColor(r, g, b, a);
                    package.AddTriangleVertexColor(r, g, b, a);
                    package.AddTriangleVertexColor(r, g, b, a);
                    package.AddTriangleVertexNormal(normal.X, normal.Y, normal.Z);
                    package.AddTriangleVertexNormal(normal.X, normal.Y, normal.Z);
                    package.AddTriangleVertexNormal(normal.X, normal.Y, normal.Z);
                    package.AddTriangleVertexUV(0, 0);
                    package.AddTriangleVertexUV(0, 0);
                    package.AddTriangleVertexUV(0, 0);
                    break;

                case HM.Operation.SERVICE_HOLE:
                    lateral = lateral.Normalized().Scale(0.5);
                    lateralR = lateralR.Normalized().Scale(0.5);
                    webAxis = webAxis.Normalized().Scale(0.5);
                    webAxisR = webAxisR.Normalized().Scale(0.5);
                    normal = normal.Normalized().Scale(0.01);
                    normalR = normalR.Normalized().Scale(0.01);

                    Geo.Point[] servicePts = new Geo.Point[] { opPoint.Add(lateral.Add(webAxis.Add(normal))), opPoint.Add(lateral.Add(webAxisR.Add(normal))), opPoint.Add(lateralR.Add(webAxisR.Add(normal))), opPoint.Add(lateralR.Add(webAxis.Add(normal))) };
                    package.AddTriangleVertex(servicePts[0].X, servicePts[0].Y, servicePts[0].Z);
                    package.AddTriangleVertex(servicePts[1].X, servicePts[1].Y, servicePts[1].Z);
                    package.AddTriangleVertex(servicePts[2].X, servicePts[2].Y, servicePts[2].Z);
                    package.AddTriangleVertexColor(r, g, b, a);
                    package.AddTriangleVertexColor(r, g, b, a);
                    package.AddTriangleVertexColor(r, g, b, a);
                    package.AddTriangleVertexNormal(normal.X, normal.Y, normal.Z);
                    package.AddTriangleVertexNormal(normal.X, normal.Y, normal.Z);
                    package.AddTriangleVertexNormal(normal.X, normal.Y, normal.Z);
                    package.AddTriangleVertexUV(0, 0);
                    package.AddTriangleVertexUV(0, 0);
                    package.AddTriangleVertexUV(0, 0);

                    package.AddTriangleVertex(servicePts[0].X, servicePts[0].Y, servicePts[0].Z);
                    package.AddTriangleVertex(servicePts[2].X, servicePts[2].Y, servicePts[2].Z);
                    package.AddTriangleVertex(servicePts[3].X, servicePts[3].Y, servicePts[3].Z);
                    package.AddTriangleVertexColor(r, g, b, a);
                    package.AddTriangleVertexColor(r, g, b, a);
                    package.AddTriangleVertexColor(r, g, b, a);
                    package.AddTriangleVertexNormal(normal.X, normal.Y, normal.Z);
                    package.AddTriangleVertexNormal(normal.X, normal.Y, normal.Z);
                    package.AddTriangleVertexNormal(normal.X, normal.Y, normal.Z);
                    package.AddTriangleVertexUV(0, 0);
                    package.AddTriangleVertexUV(0, 0);
                    package.AddTriangleVertexUV(0, 0);

                    servicePts = new Geo.Point[] { opPoint.Add(lateral.Add(webAxis.Add(normalR))), opPoint.Add(lateral.Add(webAxisR.Add(normalR))), opPoint.Add(lateralR.Add(webAxisR.Add(normalR))), opPoint.Add(lateralR.Add(webAxis.Add(normalR))) };
                    package.AddTriangleVertex(servicePts[0].X, servicePts[0].Y, servicePts[0].Z);
                    package.AddTriangleVertex(servicePts[1].X, servicePts[1].Y, servicePts[1].Z);
                    package.AddTriangleVertex(servicePts[2].X, servicePts[2].Y, servicePts[2].Z);
                    package.AddTriangleVertexColor(r, g, b, a);
                    package.AddTriangleVertexColor(r, g, b, a);
                    package.AddTriangleVertexColor(r, g, b, a);
                    package.AddTriangleVertexNormal(normal.X, normal.Y, normal.Z);
                    package.AddTriangleVertexNormal(normal.X, normal.Y, normal.Z);
                    package.AddTriangleVertexNormal(normal.X, normal.Y, normal.Z);
                    package.AddTriangleVertexUV(0, 0);
                    package.AddTriangleVertexUV(0, 0);
                    package.AddTriangleVertexUV(0, 0);

                    package.AddTriangleVertex(servicePts[0].X, servicePts[0].Y, servicePts[0].Z);
                    package.AddTriangleVertex(servicePts[2].X, servicePts[2].Y, servicePts[2].Z);
                    package.AddTriangleVertex(servicePts[3].X, servicePts[3].Y, servicePts[3].Z);
                    package.AddTriangleVertexColor(r, g, b, a);
                    package.AddTriangleVertexColor(r, g, b, a);
                    package.AddTriangleVertexColor(r, g, b, a);
                    package.AddTriangleVertexNormal(normal.X, normal.Y, normal.Z);
                    package.AddTriangleVertexNormal(normal.X, normal.Y, normal.Z);
                    package.AddTriangleVertexNormal(normal.X, normal.Y, normal.Z);
                    package.AddTriangleVertexUV(0, 0);
                    package.AddTriangleVertexUV(0, 0);
                    package.AddTriangleVertexUV(0, 0);
                    break;


                case HM.Operation.NOTCH:
                    lateral = lateral.Normalized().Scale(1.75);
                    lateralR = lateralR.Normalized().Scale(1.75);
                    webAxis = webAxis.Normalized().Scale(0.875);
                    webAxisR = webAxisR.Normalized().Scale(0.875);
                    normal = normal.Normalized().Scale(0.01);
                    normalR = normalR.Normalized().Scale(0.01);

                    Geo.Point[] notchPts = { opPoint.Add(lateral.Add(webAxis.Add(normal))), opPoint.Add(lateral.Add(webAxisR.Add(normal))), opPoint.Add(lateralR.Add(webAxisR.Add(normal))), opPoint.Add(lateralR.Add(webAxis.Add(normal))) };
                    package.AddTriangleVertex(notchPts[0].X, notchPts[0].Y, notchPts[0].Z);
                    package.AddTriangleVertex(notchPts[1].X, notchPts[1].Y, notchPts[1].Z);
                    package.AddTriangleVertex(notchPts[2].X, notchPts[2].Y, notchPts[2].Z);
                    package.AddTriangleVertexColor(r, g, b, a);
                    package.AddTriangleVertexColor(r, g, b, a);
                    package.AddTriangleVertexColor(r, g, b, a);
                    package.AddTriangleVertexNormal(normal.X, normal.Y, normal.Z);
                    package.AddTriangleVertexNormal(normal.X, normal.Y, normal.Z);
                    package.AddTriangleVertexNormal(normal.X, normal.Y, normal.Z);
                    package.AddTriangleVertexUV(0, 0);
                    package.AddTriangleVertexUV(0, 0);
                    package.AddTriangleVertexUV(0, 0);

                    package.AddTriangleVertex(notchPts[0].X, notchPts[0].Y, notchPts[0].Z);
                    package.AddTriangleVertex(notchPts[2].X, notchPts[2].Y, notchPts[2].Z);
                    package.AddTriangleVertex(notchPts[3].X, notchPts[3].Y, notchPts[3].Z);
                    package.AddTriangleVertexColor(r, g, b, a);
                    package.AddTriangleVertexColor(r, g, b, a);
                    package.AddTriangleVertexColor(r, g, b, a);
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
                    package.AddTriangleVertexColor(r, g, b, a);
                    package.AddTriangleVertexColor(r, g, b, a);
                    package.AddTriangleVertexColor(r, g, b, a);
                    package.AddTriangleVertexNormal(normal.X, normal.Y, normal.Z);
                    package.AddTriangleVertexNormal(normal.X, normal.Y, normal.Z);
                    package.AddTriangleVertexNormal(normal.X, normal.Y, normal.Z);
                    package.AddTriangleVertexUV(0, 0);
                    package.AddTriangleVertexUV(0, 0);
                    package.AddTriangleVertexUV(0, 0);

                    package.AddTriangleVertex(notchPts[0].X, notchPts[0].Y, notchPts[0].Z);
                    package.AddTriangleVertex(notchPts[2].X, notchPts[2].Y, notchPts[2].Z);
                    package.AddTriangleVertex(notchPts[3].X, notchPts[3].Y, notchPts[3].Z);
                    package.AddTriangleVertexColor(r, g, b, a);
                    package.AddTriangleVertexColor(r, g, b, a);
                    package.AddTriangleVertexColor(r, g, b, a);
                    package.AddTriangleVertexNormal(normal.X, normal.Y, normal.Z);
                    package.AddTriangleVertexNormal(normal.X, normal.Y, normal.Z);
                    package.AddTriangleVertexNormal(normal.X, normal.Y, normal.Z);
                    package.AddTriangleVertexUV(0, 0);
                    package.AddTriangleVertexUV(0, 0);
                    package.AddTriangleVertexUV(0, 0);
                    break;

                case HM.Operation.LIP_CUT:
                    lateral = lateral.Normalized().Scale(1.75);
                    lateralR = lateralR.Normalized().Scale(1.75);
                    webAxis = webAxis.Normalized().Scale(0.875);
                    webAxisR = webAxisR.Normalized().Scale(0.875);
                    normal = normal.Normalized().Scale(1.51);
                    normalR = normalR.Normalized().Scale(0.01);
                    Geo.Vector lateral2 = lateral.Normalized().Scale(1.25);
                    Geo.Vector lateral2R = lateralR.Normalized().Scale(1.25);

                    Geo.Point[] lipPts = { opPoint.Add(webAxis).Add(lateral).Add(normal), opPoint.Add(webAxisR).Add(lateral).Add(normal), opPoint.Add(webAxis).Add(lateral2).Add(normal), opPoint.Add(webAxisR).Add(lateral2).Add(normal) };
                    package.AddTriangleVertex(lipPts[0].X, lipPts[0].Y, lipPts[0].Z);
                    package.AddTriangleVertex(lipPts[1].X, lipPts[1].Y, lipPts[1].Z);
                    package.AddTriangleVertex(lipPts[2].X, lipPts[2].Y, lipPts[2].Z);
                    package.AddTriangleVertexColor(r, g, b, a);
                    package.AddTriangleVertexColor(r, g, b, a);
                    package.AddTriangleVertexColor(r, g, b, a);
                    package.AddTriangleVertexNormal(normal.X, normal.Y, normal.Z);
                    package.AddTriangleVertexNormal(normal.X, normal.Y, normal.Z);
                    package.AddTriangleVertexNormal(normal.X, normal.Y, normal.Z);
                    package.AddTriangleVertexUV(0, 0);
                    package.AddTriangleVertexUV(0, 0);
                    package.AddTriangleVertexUV(0, 0);

                    package.AddTriangleVertex(lipPts[1].X, lipPts[1].Y, lipPts[1].Z);
                    package.AddTriangleVertex(lipPts[2].X, lipPts[2].Y, lipPts[2].Z);
                    package.AddTriangleVertex(lipPts[3].X, lipPts[3].Y, lipPts[3].Z);
                    package.AddTriangleVertexColor(r, g, b, a);
                    package.AddTriangleVertexColor(r, g, b, a);
                    package.AddTriangleVertexColor(r, g, b, a);
                    package.AddTriangleVertexNormal(normal.X, normal.Y, normal.Z);
                    package.AddTriangleVertexNormal(normal.X, normal.Y, normal.Z);
                    package.AddTriangleVertexNormal(normal.X, normal.Y, normal.Z);
                    package.AddTriangleVertexUV(0, 0);
                    package.AddTriangleVertexUV(0, 0);
                    package.AddTriangleVertexUV(0, 0);


                    lipPts = new Geo.Point[] { opPoint.Add(webAxis).Add(lateralR).Add(normal), opPoint.Add(webAxisR).Add(lateralR).Add(normal), opPoint.Add(webAxis).Add(lateral2R).Add(normal), opPoint.Add(webAxisR).Add(lateral2R).Add(normal) };
                    package.AddTriangleVertex(lipPts[0].X, lipPts[0].Y, lipPts[0].Z);
                    package.AddTriangleVertex(lipPts[1].X, lipPts[1].Y, lipPts[1].Z);
                    package.AddTriangleVertex(lipPts[2].X, lipPts[2].Y, lipPts[2].Z);
                    package.AddTriangleVertexColor(r, g, b, a);
                    package.AddTriangleVertexColor(r, g, b, a);
                    package.AddTriangleVertexColor(r, g, b, a);
                    package.AddTriangleVertexNormal(normal.X, normal.Y, normal.Z);
                    package.AddTriangleVertexNormal(normal.X, normal.Y, normal.Z);
                    package.AddTriangleVertexNormal(normal.X, normal.Y, normal.Z);
                    package.AddTriangleVertexUV(0, 0);
                    package.AddTriangleVertexUV(0, 0);
                    package.AddTriangleVertexUV(0, 0);

                    package.AddTriangleVertex(lipPts[1].X, lipPts[1].Y, lipPts[1].Z);
                    package.AddTriangleVertex(lipPts[2].X, lipPts[2].Y, lipPts[2].Z);
                    package.AddTriangleVertex(lipPts[3].X, lipPts[3].Y, lipPts[3].Z);
                    package.AddTriangleVertexColor(r, g, b, a);
                    package.AddTriangleVertexColor(r, g, b, a);
                    package.AddTriangleVertexColor(r, g, b, a);
                    package.AddTriangleVertexNormal(normal.X, normal.Y, normal.Z);
                    package.AddTriangleVertexNormal(normal.X, normal.Y, normal.Z);
                    package.AddTriangleVertexNormal(normal.X, normal.Y, normal.Z);
                    package.AddTriangleVertexUV(0, 0);
                    package.AddTriangleVertexUV(0, 0);
                    package.AddTriangleVertexUV(0, 0);
                    break;


                case HM.Operation.END_TRUSS:
                    lateral = lateral.Normalized().Scale(1.76);
                    lateralR = lateralR.Normalized().Scale(1.76);
                    if (opPoint.DistanceTo(HMDynamoUtil.TripleToPoint(this.WebAxis.StartPoint)) < opPoint.DistanceTo(HMDynamoUtil.TripleToPoint(this.WebAxis.EndPoint)))
                    {
                        webAxis = webAxis.Normalized().Scale(0.5);
                    }
                    else
                    {
                        webAxis = webAxisR.Normalized().Scale(0.5);
                    }

                    normal = normal.Normalized().Scale(1.51);
                    Geo.Vector normal2 = normal.Normalized().Scale(1.0);
                    Geo.Vector normal3 = normal.Normalized().Scale(0.5);
                    normalR = normalR.Normalized().Scale(0.01);

                    lateral2 = lateral.Normalized().Scale(1.25);
                    lateral2R = lateralR.Normalized().Scale(1.25);

                    // Outside
                    Geo.Point[] endTrussPts = { opPoint.Add(lateral).Add(normal2), opPoint.Add(lateral).Add(normal).Add(webAxis), opPoint.Add(lateral).Add(normal) };
                    package.AddTriangleVertex(endTrussPts[0].X, endTrussPts[0].Y, endTrussPts[0].Z);
                    package.AddTriangleVertex(endTrussPts[1].X, endTrussPts[1].Y, endTrussPts[1].Z);
                    package.AddTriangleVertex(endTrussPts[2].X, endTrussPts[2].Y, endTrussPts[2].Z);
                    package.AddTriangleVertexColor(r, g, b, a);
                    package.AddTriangleVertexColor(r, g, b, a);
                    package.AddTriangleVertexColor(r, g, b, a);
                    package.AddTriangleVertexNormal(lateral.X, lateral.Y, lateral.Z);
                    package.AddTriangleVertexNormal(lateral.X, lateral.Y, lateral.Z);
                    package.AddTriangleVertexNormal(lateral.X, lateral.Y, lateral.Z);
                    package.AddTriangleVertexUV(0, 0);
                    package.AddTriangleVertexUV(0, 0);
                    package.AddTriangleVertexUV(0, 0);

                    endTrussPts = new Geo.Point[] { opPoint.Add(lateral).Add(normal3), opPoint.Add(lateral).Add(webAxis), opPoint.Add(lateral) };
                    package.AddTriangleVertex(endTrussPts[0].X, endTrussPts[0].Y, endTrussPts[0].Z);
                    package.AddTriangleVertex(endTrussPts[1].X, endTrussPts[1].Y, endTrussPts[1].Z);
                    package.AddTriangleVertex(endTrussPts[2].X, endTrussPts[2].Y, endTrussPts[2].Z);
                    package.AddTriangleVertexColor(r, g, b, a);
                    package.AddTriangleVertexColor(r, g, b, a);
                    package.AddTriangleVertexColor(r, g, b, a);
                    package.AddTriangleVertexNormal(lateral.X, lateral.Y, lateral.Z);
                    package.AddTriangleVertexNormal(lateral.X, lateral.Y, lateral.Z);
                    package.AddTriangleVertexNormal(lateral.X, lateral.Y, lateral.Z);
                    package.AddTriangleVertexUV(0, 0);
                    package.AddTriangleVertexUV(0, 0);
                    package.AddTriangleVertexUV(0, 0);

                    endTrussPts = new Geo.Point[] { opPoint.Add(lateralR).Add(normal2), opPoint.Add(lateralR).Add(normal).Add(webAxis), opPoint.Add(lateralR).Add(normal) };
                    package.AddTriangleVertex(endTrussPts[0].X, endTrussPts[0].Y, endTrussPts[0].Z);
                    package.AddTriangleVertex(endTrussPts[1].X, endTrussPts[1].Y, endTrussPts[1].Z);
                    package.AddTriangleVertex(endTrussPts[2].X, endTrussPts[2].Y, endTrussPts[2].Z);
                    package.AddTriangleVertexColor(r, g, b, a);
                    package.AddTriangleVertexColor(r, g, b, a);
                    package.AddTriangleVertexColor(r, g, b, a);
                    package.AddTriangleVertexNormal(lateralR.X, lateralR.Y, lateralR.Z);
                    package.AddTriangleVertexNormal(lateralR.X, lateralR.Y, lateralR.Z);
                    package.AddTriangleVertexNormal(lateralR.X, lateralR.Y, lateralR.Z);
                    package.AddTriangleVertexUV(0, 0);
                    package.AddTriangleVertexUV(0, 0);
                    package.AddTriangleVertexUV(0, 0);

                    endTrussPts = new Geo.Point[] { opPoint.Add(lateralR).Add(normal3), opPoint.Add(lateralR).Add(webAxis), opPoint.Add(lateralR) };
                    package.AddTriangleVertex(endTrussPts[0].X, endTrussPts[0].Y, endTrussPts[0].Z);
                    package.AddTriangleVertex(endTrussPts[1].X, endTrussPts[1].Y, endTrussPts[1].Z);
                    package.AddTriangleVertex(endTrussPts[2].X, endTrussPts[2].Y, endTrussPts[2].Z);
                    package.AddTriangleVertexColor(r, g, b, a);
                    package.AddTriangleVertexColor(r, g, b, a);
                    package.AddTriangleVertexColor(r, g, b, a);
                    package.AddTriangleVertexNormal(lateralR.X, lateralR.Y, lateralR.Z);
                    package.AddTriangleVertexNormal(lateralR.X, lateralR.Y, lateralR.Z);
                    package.AddTriangleVertexNormal(lateralR.X, lateralR.Y, lateralR.Z);
                    package.AddTriangleVertexUV(0, 0);
                    package.AddTriangleVertexUV(0, 0);
                    package.AddTriangleVertexUV(0, 0);

                    endTrussPts = new Geo.Point[] { opPoint.Add(lateralR).Add(normal), opPoint.Add(lateralR).Add(webAxis).Add(normal), opPoint.Add(lateral2R).Add(normal) };
                    package.AddTriangleVertex(endTrussPts[0].X, endTrussPts[0].Y, endTrussPts[0].Z);
                    package.AddTriangleVertex(endTrussPts[1].X, endTrussPts[1].Y, endTrussPts[1].Z);
                    package.AddTriangleVertex(endTrussPts[2].X, endTrussPts[2].Y, endTrussPts[2].Z);
                    package.AddTriangleVertexColor(r, g, b, a);
                    package.AddTriangleVertexColor(r, g, b, a);
                    package.AddTriangleVertexColor(r, g, b, a);
                    package.AddTriangleVertexNormal(normal.X, normal.Y, normal.Z);
                    package.AddTriangleVertexNormal(normal.X, normal.Y, normal.Z);
                    package.AddTriangleVertexNormal(normal.X, normal.Y, normal.Z);
                    package.AddTriangleVertexUV(0, 0);
                    package.AddTriangleVertexUV(0, 0);
                    package.AddTriangleVertexUV(0, 0);

                    endTrussPts = new Geo.Point[] { opPoint.Add(lateralR).Add(webAxis).Add(normal), opPoint.Add(lateral2R).Add(normal), opPoint.Add(lateral2R).Add(normal).Add(webAxis) };
                    package.AddTriangleVertex(endTrussPts[0].X, endTrussPts[0].Y, endTrussPts[0].Z);
                    package.AddTriangleVertex(endTrussPts[1].X, endTrussPts[1].Y, endTrussPts[1].Z);
                    package.AddTriangleVertex(endTrussPts[2].X, endTrussPts[2].Y, endTrussPts[2].Z);
                    package.AddTriangleVertexColor(r, g, b, a);
                    package.AddTriangleVertexColor(r, g, b, a);
                    package.AddTriangleVertexColor(r, g, b, a);
                    package.AddTriangleVertexNormal(normal.X, normal.Y, normal.Z);
                    package.AddTriangleVertexNormal(normal.X, normal.Y, normal.Z);
                    package.AddTriangleVertexNormal(normal.X, normal.Y, normal.Z);
                    package.AddTriangleVertexUV(0, 0);
                    package.AddTriangleVertexUV(0, 0);
                    package.AddTriangleVertexUV(0, 0);

                    // Inside
                    lateral = lateral.Normalized().Scale(1.74);
                    lateralR = lateralR.Normalized().Scale(1.74);
                    endTrussPts = new Geo.Point[] { opPoint.Add(lateral).Add(normal2), opPoint.Add(lateral).Add(normal).Add(webAxis), opPoint.Add(lateral).Add(normal) };
                    package.AddTriangleVertex(endTrussPts[0].X, endTrussPts[0].Y, endTrussPts[0].Z);
                    package.AddTriangleVertex(endTrussPts[1].X, endTrussPts[1].Y, endTrussPts[1].Z);
                    package.AddTriangleVertex(endTrussPts[2].X, endTrussPts[2].Y, endTrussPts[2].Z);
                    package.AddTriangleVertexColor(r, g, b, a);
                    package.AddTriangleVertexColor(r, g, b, a);
                    package.AddTriangleVertexColor(r, g, b, a);
                    package.AddTriangleVertexNormal(lateral.X, lateral.Y, lateral.Z);
                    package.AddTriangleVertexNormal(lateral.X, lateral.Y, lateral.Z);
                    package.AddTriangleVertexNormal(lateral.X, lateral.Y, lateral.Z);
                    package.AddTriangleVertexUV(0, 0);
                    package.AddTriangleVertexUV(0, 0);
                    package.AddTriangleVertexUV(0, 0);

                    endTrussPts = new Geo.Point[] { opPoint.Add(lateral).Add(normal3), opPoint.Add(lateral).Add(webAxis), opPoint.Add(lateral) };
                    package.AddTriangleVertex(endTrussPts[0].X, endTrussPts[0].Y, endTrussPts[0].Z);
                    package.AddTriangleVertex(endTrussPts[1].X, endTrussPts[1].Y, endTrussPts[1].Z);
                    package.AddTriangleVertex(endTrussPts[2].X, endTrussPts[2].Y, endTrussPts[2].Z);
                    package.AddTriangleVertexColor(r, g, b, a);
                    package.AddTriangleVertexColor(r, g, b, a);
                    package.AddTriangleVertexColor(r, g, b, a);
                    package.AddTriangleVertexNormal(lateral.X, lateral.Y, lateral.Z);
                    package.AddTriangleVertexNormal(lateral.X, lateral.Y, lateral.Z);
                    package.AddTriangleVertexNormal(lateral.X, lateral.Y, lateral.Z);
                    package.AddTriangleVertexUV(0, 0);
                    package.AddTriangleVertexUV(0, 0);
                    package.AddTriangleVertexUV(0, 0);

                    endTrussPts = new Geo.Point[] { opPoint.Add(lateralR).Add(normal2), opPoint.Add(lateralR).Add(normal).Add(webAxis), opPoint.Add(lateralR).Add(normal) };
                    package.AddTriangleVertex(endTrussPts[0].X, endTrussPts[0].Y, endTrussPts[0].Z);
                    package.AddTriangleVertex(endTrussPts[1].X, endTrussPts[1].Y, endTrussPts[1].Z);
                    package.AddTriangleVertex(endTrussPts[2].X, endTrussPts[2].Y, endTrussPts[2].Z);
                    package.AddTriangleVertexColor(r, g, b, a);
                    package.AddTriangleVertexColor(r, g, b, a);
                    package.AddTriangleVertexColor(r, g, b, a);
                    package.AddTriangleVertexNormal(lateralR.X, lateralR.Y, lateralR.Z);
                    package.AddTriangleVertexNormal(lateralR.X, lateralR.Y, lateralR.Z);
                    package.AddTriangleVertexNormal(lateralR.X, lateralR.Y, lateralR.Z);
                    package.AddTriangleVertexUV(0, 0);
                    package.AddTriangleVertexUV(0, 0);
                    package.AddTriangleVertexUV(0, 0);

                    endTrussPts = new Geo.Point[] { opPoint.Add(lateralR).Add(normal3), opPoint.Add(lateralR).Add(webAxis), opPoint.Add(lateralR) };
                    package.AddTriangleVertex(endTrussPts[0].X, endTrussPts[0].Y, endTrussPts[0].Z);
                    package.AddTriangleVertex(endTrussPts[1].X, endTrussPts[1].Y, endTrussPts[1].Z);
                    package.AddTriangleVertex(endTrussPts[2].X, endTrussPts[2].Y, endTrussPts[2].Z);
                    package.AddTriangleVertexColor(r, g, b, a);
                    package.AddTriangleVertexColor(r, g, b, a);
                    package.AddTriangleVertexColor(r, g, b, a);
                    package.AddTriangleVertexNormal(lateralR.X, lateralR.Y, lateralR.Z);
                    package.AddTriangleVertexNormal(lateralR.X, lateralR.Y, lateralR.Z);
                    package.AddTriangleVertexNormal(lateralR.X, lateralR.Y, lateralR.Z);
                    package.AddTriangleVertexUV(0, 0);
                    package.AddTriangleVertexUV(0, 0);
                    package.AddTriangleVertexUV(0, 0);

                    endTrussPts = new Geo.Point[] { opPoint.Add(lateralR).Add(normal), opPoint.Add(lateralR).Add(webAxis).Add(normal), opPoint.Add(lateral2R).Add(normal) };
                    package.AddTriangleVertex(endTrussPts[0].X, endTrussPts[0].Y, endTrussPts[0].Z);
                    package.AddTriangleVertex(endTrussPts[1].X, endTrussPts[1].Y, endTrussPts[1].Z);
                    package.AddTriangleVertex(endTrussPts[2].X, endTrussPts[2].Y, endTrussPts[2].Z);
                    package.AddTriangleVertexColor(r, g, b, a);
                    package.AddTriangleVertexColor(r, g, b, a);
                    package.AddTriangleVertexColor(r, g, b, a);
                    package.AddTriangleVertexNormal(normal.X, normal.Y, normal.Z);
                    package.AddTriangleVertexNormal(normal.X, normal.Y, normal.Z);
                    package.AddTriangleVertexNormal(normal.X, normal.Y, normal.Z);
                    package.AddTriangleVertexUV(0, 0);
                    package.AddTriangleVertexUV(0, 0);
                    package.AddTriangleVertexUV(0, 0);

                    endTrussPts = new Geo.Point[] { opPoint.Add(lateralR).Add(webAxis).Add(normal), opPoint.Add(lateral2R).Add(normal), opPoint.Add(lateral2R).Add(normal).Add(webAxis) };
                    package.AddTriangleVertex(endTrussPts[0].X, endTrussPts[0].Y, endTrussPts[0].Z);
                    package.AddTriangleVertex(endTrussPts[1].X, endTrussPts[1].Y, endTrussPts[1].Z);
                    package.AddTriangleVertex(endTrussPts[2].X, endTrussPts[2].Y, endTrussPts[2].Z);
                    package.AddTriangleVertexColor(r, g, b, a);
                    package.AddTriangleVertexColor(r, g, b, a);
                    package.AddTriangleVertexColor(r, g, b, a);
                    package.AddTriangleVertexNormal(normal.X, normal.Y, normal.Z);
                    package.AddTriangleVertexNormal(normal.X, normal.Y, normal.Z);
                    package.AddTriangleVertexNormal(normal.X, normal.Y, normal.Z);
                    package.AddTriangleVertexUV(0, 0);
                    package.AddTriangleVertexUV(0, 0);
                    package.AddTriangleVertexUV(0, 0);

                    // LIP R
                    lateral = lateral.Normalized().Scale(1.76);
                    lateralR = lateralR.Normalized().Scale(1.76);
                    endTrussPts = new Geo.Point[] { opPoint.Add(lateral).Add(normal), opPoint.Add(lateral).Add(webAxis).Add(normal), opPoint.Add(lateral2).Add(normal) };
                    package.AddTriangleVertex(endTrussPts[0].X, endTrussPts[0].Y, endTrussPts[0].Z);
                    package.AddTriangleVertex(endTrussPts[1].X, endTrussPts[1].Y, endTrussPts[1].Z);
                    package.AddTriangleVertex(endTrussPts[2].X, endTrussPts[2].Y, endTrussPts[2].Z);
                    package.AddTriangleVertexColor(r, g, b, a);
                    package.AddTriangleVertexColor(r, g, b, a);
                    package.AddTriangleVertexColor(r, g, b, a);
                    package.AddTriangleVertexNormal(normal.X, normal.Y, normal.Z);
                    package.AddTriangleVertexNormal(normal.X, normal.Y, normal.Z);
                    package.AddTriangleVertexNormal(normal.X, normal.Y, normal.Z);
                    package.AddTriangleVertexUV(0, 0);
                    package.AddTriangleVertexUV(0, 0);
                    package.AddTriangleVertexUV(0, 0);
                    
                    endTrussPts = new Geo.Point[] { opPoint.Add(lateral).Add(webAxis).Add(normal), opPoint.Add(lateral2).Add(normal), opPoint.Add(lateral2).Add(normal).Add(webAxis) };
                    package.AddTriangleVertex(endTrussPts[0].X, endTrussPts[0].Y, endTrussPts[0].Z);
                    package.AddTriangleVertex(endTrussPts[1].X, endTrussPts[1].Y, endTrussPts[1].Z);
                    package.AddTriangleVertex(endTrussPts[2].X, endTrussPts[2].Y, endTrussPts[2].Z);
                    package.AddTriangleVertexColor(r, g, b, a);
                    package.AddTriangleVertexColor(r, g, b, a);
                    package.AddTriangleVertexColor(r, g, b, a);
                    package.AddTriangleVertexNormal(normal.X, normal.Y, normal.Z);
                    package.AddTriangleVertexNormal(normal.X, normal.Y, normal.Z);
                    package.AddTriangleVertexNormal(normal.X, normal.Y, normal.Z);
                    package.AddTriangleVertexUV(0, 0);
                    package.AddTriangleVertexUV(0, 0);
                    package.AddTriangleVertexUV(0, 0);

                    // LIP
                    endTrussPts = new Geo.Point[] { opPoint.Add(lateral).Add(normal), opPoint.Add(lateral).Add(webAxis).Add(normal), opPoint.Add(lateral2).Add(normal) };
                    package.AddTriangleVertex(endTrussPts[0].X, endTrussPts[0].Y, endTrussPts[0].Z);
                    package.AddTriangleVertex(endTrussPts[1].X, endTrussPts[1].Y, endTrussPts[1].Z);
                    package.AddTriangleVertex(endTrussPts[2].X, endTrussPts[2].Y, endTrussPts[2].Z);
                    package.AddTriangleVertexColor(r, g, b, a);
                    package.AddTriangleVertexColor(r, g, b, a);
                    package.AddTriangleVertexColor(r, g, b, a);
                    package.AddTriangleVertexNormal(normal.X, normal.Y, normal.Z);
                    package.AddTriangleVertexNormal(normal.X, normal.Y, normal.Z);
                    package.AddTriangleVertexNormal(normal.X, normal.Y, normal.Z);
                    package.AddTriangleVertexUV(0, 0);
                    package.AddTriangleVertexUV(0, 0);
                    package.AddTriangleVertexUV(0, 0);
                    
                    
                    endTrussPts = new Geo.Point[] { opPoint.Add(lateral).Add(webAxis).Add(normal), opPoint.Add(lateral2).Add(normal), opPoint.Add(lateral2).Add(normal).Add(webAxis) };
                    package.AddTriangleVertex(endTrussPts[0].X, endTrussPts[0].Y, endTrussPts[0].Z);
                    package.AddTriangleVertex(endTrussPts[1].X, endTrussPts[1].Y, endTrussPts[1].Z);
                    package.AddTriangleVertex(endTrussPts[2].X, endTrussPts[2].Y, endTrussPts[2].Z);
                    package.AddTriangleVertexColor(r, g, b, a);
                    package.AddTriangleVertexColor(r, g, b, a);
                    package.AddTriangleVertexColor(r, g, b, a);
                    package.AddTriangleVertexNormal(normal.X, normal.Y, normal.Z);
                    package.AddTriangleVertexNormal(normal.X, normal.Y, normal.Z);
                    package.AddTriangleVertexNormal(normal.X, normal.Y, normal.Z);
                    package.AddTriangleVertexUV(0, 0);
                    package.AddTriangleVertexUV(0, 0);
                    package.AddTriangleVertexUV(0, 0);

                    // Notch
                    endTrussPts = new Geo.Point[] { opPoint.Add(lateral).Add(normalR), opPoint.Add(lateral).Add(webAxis).Add(normalR), opPoint.Add(lateralR).Add(webAxis).Add(normalR) };
                    package.AddTriangleVertex(endTrussPts[0].X, endTrussPts[0].Y, endTrussPts[0].Z);
                    package.AddTriangleVertex(endTrussPts[1].X, endTrussPts[1].Y, endTrussPts[1].Z);
                    package.AddTriangleVertex(endTrussPts[2].X, endTrussPts[2].Y, endTrussPts[2].Z);
                    package.AddTriangleVertexColor(r, g, b, a);
                    package.AddTriangleVertexColor(r, g, b, a);
                    package.AddTriangleVertexColor(r, g, b, a);
                    package.AddTriangleVertexNormal(normal.X, normal.Y, normal.Z);
                    package.AddTriangleVertexNormal(normal.X, normal.Y, normal.Z);
                    package.AddTriangleVertexNormal(normal.X, normal.Y, normal.Z);
                    package.AddTriangleVertexUV(0, 0);
                    package.AddTriangleVertexUV(0, 0);
                    package.AddTriangleVertexUV(0, 0);

                    endTrussPts = new Geo.Point[] { opPoint.Add(lateral).Add(normalR), opPoint.Add(lateralR).Add(normalR), opPoint.Add(lateralR).Add(webAxis).Add(normalR) };
                    package.AddTriangleVertex(endTrussPts[0].X, endTrussPts[0].Y, endTrussPts[0].Z);
                    package.AddTriangleVertex(endTrussPts[1].X, endTrussPts[1].Y, endTrussPts[1].Z);
                    package.AddTriangleVertex(endTrussPts[2].X, endTrussPts[2].Y, endTrussPts[2].Z);
                    package.AddTriangleVertexColor(r, g, b, a);
                    package.AddTriangleVertexColor(r, g, b, a);
                    package.AddTriangleVertexColor(r, g, b, a);
                    package.AddTriangleVertexNormal(normal.X, normal.Y, normal.Z);
                    package.AddTriangleVertexNormal(normal.X, normal.Y, normal.Z);
                    package.AddTriangleVertexNormal(normal.X, normal.Y, normal.Z);
                    package.AddTriangleVertexUV(0, 0);
                    package.AddTriangleVertexUV(0, 0);
                    package.AddTriangleVertexUV(0, 0);

                    // Notch 2
                    normalR = normalR.Reverse();
                    endTrussPts = new Geo.Point[] { opPoint.Add(lateral).Add(normalR), opPoint.Add(lateral).Add(webAxis).Add(normalR), opPoint.Add(lateralR).Add(webAxis).Add(normalR) };
                    package.AddTriangleVertex(endTrussPts[0].X, endTrussPts[0].Y, endTrussPts[0].Z);
                    package.AddTriangleVertex(endTrussPts[1].X, endTrussPts[1].Y, endTrussPts[1].Z);
                    package.AddTriangleVertex(endTrussPts[2].X, endTrussPts[2].Y, endTrussPts[2].Z);
                    package.AddTriangleVertexColor(r, g, b, a);
                    package.AddTriangleVertexColor(r, g, b, a);
                    package.AddTriangleVertexColor(r, g, b, a);
                    package.AddTriangleVertexNormal(normal.X, normal.Y, normal.Z);
                    package.AddTriangleVertexNormal(normal.X, normal.Y, normal.Z);
                    package.AddTriangleVertexNormal(normal.X, normal.Y, normal.Z);
                    package.AddTriangleVertexUV(0, 0);
                    package.AddTriangleVertexUV(0, 0);
                    package.AddTriangleVertexUV(0, 0);

                    endTrussPts = new Geo.Point[] { opPoint.Add(lateral).Add(normalR), opPoint.Add(lateralR).Add(normalR), opPoint.Add(lateralR).Add(webAxis).Add(normalR) };
                    package.AddTriangleVertex(endTrussPts[0].X, endTrussPts[0].Y, endTrussPts[0].Z);
                    package.AddTriangleVertex(endTrussPts[1].X, endTrussPts[1].Y, endTrussPts[1].Z);
                    package.AddTriangleVertex(endTrussPts[2].X, endTrussPts[2].Y, endTrussPts[2].Z);
                    package.AddTriangleVertexColor(r, g, b, a);
                    package.AddTriangleVertexColor(r, g, b, a);
                    package.AddTriangleVertexColor(r, g, b, a);
                    package.AddTriangleVertexNormal(normal.X, normal.Y, normal.Z);
                    package.AddTriangleVertexNormal(normal.X, normal.Y, normal.Z);
                    package.AddTriangleVertexNormal(normal.X, normal.Y, normal.Z);
                    package.AddTriangleVertexUV(0, 0);
                    package.AddTriangleVertexUV(0, 0);
                    package.AddTriangleVertexUV(0, 0);
                    break;

                default:
                    package.AddPointVertex(opPoint.X, opPoint.Y, opPoint.Z);
                    package.AddPointVertexColor(r, g, b, a);
                    break;

            }
        }


        [IsVisibleInDynamoLibrary(false)]
        public void Tessellate(IRenderPackage package, TessellationParameters parameters)
        {

            package.RequiresPerVertexColoration = true;

            var DSWebAxis = HMDynamoUtil.HMLineToDSLine(this.WebAxis);
            var DSWebNormal = HMDynamoUtil.TripleToVector(this.WebNormal);

            Geo.Point OP1 = DSWebAxis.StartPoint;
            Geo.Point OP2 = DSWebAxis.EndPoint;

            /*var midPoint = this._webAxis.PointAtParameter(0.5);
            package.AddPointVertex(midPoint.X, midPoint.Y, midPoint.Z);
            package.Description = _label;*/

            Geo.Vector webAxis = Geo.Vector.ByTwoPoints(OP1, OP2);
            Geo.Vector normal = DSWebNormal;
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
            ////////////        //          //          //          //          //          //          //           //            //          //
            Geo.Point[] pts = { p0, p1, p2, p1, p2, p3, p2, p3, p4, p3, p4, p5, p4, p5, p6, p5, p6, p7, p0, p1, p10, p1, p10, p11, p6, p7, p8, p7, p8, p9 };
            Geo.Vector[] vectors = { lateral, normal, lateral.Reverse(), normal.Reverse(), normal.Reverse() };
            byte[] colors = { 100, 100, 100, 100, 110, 110, 110, 110, 110, 110 };

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
                package.AddTriangleVertexColor(colors[i], colors[i], colors[i], 255);
                package.AddTriangleVertexColor(colors[i], colors[i], colors[i], 255);
                package.AddTriangleVertexColor(colors[i], colors[i], colors[i], 255);
                package.AddTriangleVertexNormal(vectors[i / 2].X, vectors[i / 2].Y, vectors[i / 2].Z);
                package.AddTriangleVertexNormal(vectors[i / 2].X, vectors[i / 2].Y, vectors[i / 2].Z);
                package.AddTriangleVertexNormal(vectors[i / 2].X, vectors[i / 2].Y, vectors[i / 2].Z);
                package.AddTriangleVertexUV(0, 0);
                package.AddTriangleVertexUV(0, 0);
                package.AddTriangleVertexUV(0, 0);
            }

            foreach (HM.hOperation op in this.Operations)
            {
                TessellateOperation(package, parameters, op);
            }
        }
        #endregion
    }
}
