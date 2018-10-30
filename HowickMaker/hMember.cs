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
    /// <summary>
    /// Represents a steel stud
    /// </summary>
    public class hMember : IGraphicItem
    {
        /// <summary>
        /// The web axis of a member
        /// </summary>
        /// <returns name="webAxis">The web axis of the member</returns>
        public Geo.Line WebAxis
        {
            get { return _webAxis; }
        }
        internal Geo.Line _webAxis;

        /// <summary>
        /// The web normal of a member
        /// </summary>
        public Geo.Vector WebNormal
        {
            get { return _webNormal; }
        }
        internal Geo.Vector _webNormal;

        /// <summary>
        /// The flange axes of a member
        /// </summary>
        public List<Geo.Line> FlangeAxes
        {
            get { return GetFlangeAxes(); }
        }
        
        /// <summary>
        /// The name of a member
        /// </summary>
        public string Name
        {
            get { return (_label == null) ? _name : _label; }
        }
        internal string _name;

        /// <summary>
        /// The length of a member
        /// </summary>
        public double Length
        {
            get { return WebAxis.Length; }
        }

        
        public List<hConnection> connections = new List<hConnection>();
        internal List<hOperation> operations = new List<hOperation>();
        internal string _label;
        




        //   ██████╗ ██████╗ ███╗   ██╗███████╗████████╗██████╗ 
        //  ██╔════╝██╔═══██╗████╗  ██║██╔════╝╚══██╔══╝██╔══██╗
        //  ██║     ██║   ██║██╔██╗ ██║███████╗   ██║   ██████╔╝
        //  ██║     ██║   ██║██║╚██╗██║╚════██║   ██║   ██╔══██╗
        //  ╚██████╗╚██████╔╝██║ ╚████║███████║   ██║   ██║  ██║
        //   ╚═════╝ ╚═════╝ ╚═╝  ╚═══╝╚══════╝   ╚═╝   ╚═╝  ╚═╝
        //                                                      


        [IsVisibleInDynamoLibrary(false)]
        internal hMember()
        {

        }


        [IsVisibleInDynamoLibrary(false)]
        internal hMember(hMember member)
        {
            _webAxis = member._webAxis;
            _webNormal = member._webNormal;
            _name = member._name;
            _label = member._label;
            foreach (hConnection con in member.connections)
            {
                this.connections.Add(con);
            }
            foreach (hOperation op in member.operations)
            {
                this.operations.Add(op);
            }
        }


        [IsVisibleInDynamoLibrary(false)]
        internal hMember(Geo.Line webAxis, Geo.Vector webNormal, string name = "0")
        {
            _webAxis = webAxis;
            _webNormal = webNormal;
            _name = name;
        }
        

        [IsVisibleInDynamoLibrary(false)]
        internal hMember(Geo.Line webAxis, string name = "0")
        {
            _webAxis = webAxis;
            _webNormal = null;
            _name = name;
        }





        //  ██████╗ ██╗   ██╗██████╗      ██████╗███╗   ██╗███████╗████████╗██████╗ 
        //  ██╔══██╗██║   ██║██╔══██╗    ██╔════╝████╗  ██║██╔════╝╚══██╔══╝██╔══██╗
        //  ██████╔╝██║   ██║██████╔╝    ██║     ██╔██╗ ██║███████╗   ██║   ██████╔╝
        //  ██╔═══╝ ██║   ██║██╔══██╗    ██║     ██║╚██╗██║╚════██║   ██║   ██╔══██╗
        //  ██║     ╚██████╔╝██████╔╝    ╚██████╗██║ ╚████║███████║   ██║   ██║  ██║
        //  ╚═╝      ╚═════╝ ╚═════╝      ╚═════╝╚═╝  ╚═══╝╚══════╝   ╚═╝   ╚═╝  ╚═╝
        //          


        /// <summary>
        /// Creates an hMember with its web lying along the webAxis, facing webNormal
        /// </summary>
        /// <param name="webAxis"></param>
        /// <param name="webNormal"></param>
        /// <param name="name"></param>
        /// <returns name="hMember"></returns>
        public static hMember ByLineVector(Geo.Line webAxis, Geo.Vector webNormal, string name = "0")
        {
            hMember member = new hMember(webAxis, webNormal.Normalized(), name);
            return member;
        }





        //  ██████╗ ██╗   ██╗██████╗  
        //  ██╔══██╗██║   ██║██╔══██╗ 
        //  ██████╔╝██║   ██║██████╔╝  
        //  ██╔═══╝ ██║   ██║██╔══██╗  
        //  ██║     ╚██████╔╝██████╔╝ 
        //  ╚═╝      ╚═════╝ ╚═════╝  
        //          

        /// <summary>
        /// Add or change the name of an hMember. This will be used as the label on the steel stud.
        /// </summary>
        /// <param name="member"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        public hMember AddName(hMember member, string name)
        {
            hMember newMember = new hMember(this);
            newMember._name = name;
            return newMember;
        }

        /// <summary>
        /// Adds operations to the member by specifying the locations and the types of operations
        /// </summary>
        /// <param name="member"></param>
        /// <param name="locations"></param>
        /// <param name="types"></param>
        /// <returns name="hMember"></returns>
        public hMember AddOperationByLocationType(List<double> locations, List<string> types)
        {
            hMember newMember = new hMember(this);
            for (int i = 0; i < locations.Count; i++)
            {
                hOperation op = new hOperation(locations[i], (Operation)System.Enum.Parse(typeof(Operation), types[i]));
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
        public hMember AddOperationByPointType(List<Geo.Point> points, List<string> types)
        {
            hMember newMember = new hMember(this);
            for (int i = 0; i < points.Count; i++)
            {
                newMember.AddOperationByPointType(points[i], types[i]);
            }
            return newMember;
        }

        /*
        /// <summary>
        /// Gets the web axis of the member
        /// </summary>
        /// <returns></returns>
        public Geo.Line WebAxis()
        {
            return _webAxis;
        }*/


        /// <summary>
        /// Gets the lines that run along the center of each flange of the member, parallel to the web axis
        /// </summary>
        /// <param name="member"></param>
        /// <returns></returns>
        [IsVisibleInDynamoLibrary(false)]
        public List<Geo.Line> GetFlangeAxes()
        {
            Geo.Point OP1 = _webAxis.StartPoint;
            Geo.Point OP2 = _webAxis.EndPoint;
            Geo.Vector webAxisVec = Geo.Vector.ByTwoPoints(OP1, OP2);
            Geo.Vector normal = _webNormal.Normalized().Scale(0.75); ;
            Geo.Vector lateral = webAxisVec.Cross(normal).Normalized().Scale(1.75);
            Geo.Line flangeLine1 = Geo.Line.ByStartPointEndPoint(OP1.Add(normal.Add(lateral)), OP2.Add(normal.Add(lateral)));
            lateral = webAxisVec.Cross(normal).Normalized().Scale(-1.75);
            Geo.Line flangeLine2 = Geo.Line.ByStartPointEndPoint(OP1.Add(normal.Add(lateral)), OP2.Add(normal.Add(lateral)));
            return new List<Geo.Line> { flangeLine1, flangeLine2 };
        }

        public List<hMember> SplitAtLocation(hMember member, double loc)
        {
            var webAxes = member.WebAxis.SplitByParameter(loc / member.WebAxis.Length);
            var mem1 = new hMember(Geo.Line.ByStartPointEndPoint(webAxes[0].StartPoint, webAxes[0].EndPoint), member.WebNormal, member.Name + "-1");
            var mem2 = new hMember(Geo.Line.ByStartPointEndPoint(webAxes[1].StartPoint, webAxes[1].EndPoint), member.WebNormal, member.Name + "-2");
            mem1._label = member.Name + "-1";
            mem2._label = member.Name + "-2";

            foreach (hOperation op in member.operations)
            {
                if (op._loc < loc)
                {
                    mem1.AddOperationByLocationType(op._loc, op._type.ToString());
                }
                else
                {
                    var newLoc = op._loc - loc;
                    mem2.AddOperationByLocationType(newLoc, op._type.ToString());
                }
            }

            return (new List<hMember> { mem1, mem2 });
        }




        //  ██╗███╗   ██╗████████╗███████╗██████╗ ███╗   ██╗ █████╗ ██╗     
        //  ██║████╗  ██║╚══██╔══╝██╔════╝██╔══██╗████╗  ██║██╔══██╗██║     
        //  ██║██╔██╗ ██║   ██║   █████╗  ██████╔╝██╔██╗ ██║███████║██║     
        //  ██║██║╚██╗██║   ██║   ██╔══╝  ██╔══██╗██║╚██╗██║██╔══██║██║     
        //  ██║██║ ╚████║   ██║   ███████╗██║  ██║██║ ╚████║██║  ██║███████╗
        //  ╚═╝╚═╝  ╚═══╝   ╚═╝   ╚══════╝╚═╝  ╚═╝╚═╝  ╚═══╝╚═╝  ╚═╝╚══════╝
        //                                                                  

        
        /// <summary>
        /// Add an operation to the member by specifiying the type of operation and the point at which the operation should occur
        /// </summary>
        /// <param name="pt"></param>
        /// <param name="type"></param>
        internal void AddOperationByPointType(Geo.Point pt, string type)
        {
            double location = _webAxis.ParameterAtPoint(pt) * _webAxis.Length;
            hOperation op = new hOperation(location, (Operation)System.Enum.Parse(typeof(Operation), type));
            AddOperation(op);
        }


        /// <summary>
        /// AAdd an operation to the member by specifiying the type of operation and the location along the member at which the operation should occur
        /// </summary>
        /// <param name="location"></param>
        /// <param name="type"></param>
        internal void AddOperationByLocationType(double location, string type)
        {
            hOperation op = new hOperation(location, (Operation)System.Enum.Parse(typeof(Operation), type));
            AddOperation(op);
        }


        /// <summary>
        /// Add an hOperation to the member's list of operations
        /// </summary>
        /// <param name="operation"></param>
        internal void AddOperation(hOperation operation)
        {
            this.operations.Add(operation);
        }


        /// <summary>
        /// Add an hConnection to the member's list of connections
        /// </summary>
        /// <param name="connection"></param>
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
            Geo.Line newAxis = Geo.Line.ByStartPointEndPoint(newStartPoint, _webAxis.EndPoint);

            // Compute new locations for operations relative to new axis
            foreach (hOperation op in operations)
            {
                Geo.Point opPoint = _webAxis.PointAtParameter(op._loc / _webAxis.Length);
                double newLoc = newAxis.ParameterAtPoint(opPoint) * newAxis.Length;
                op._loc = newLoc;
            }

            // Set new axis
            _webAxis = newAxis;
        }


        /// <summary>
        /// Extend member by changing web axis end point. Adjust operations accordingly.
        /// </summary>
        /// <param name="newEndPoint"></param>
        internal void SetWebAxisEndPoint(Geo.Point newEndPoint)
        {
            // Create new axis
            Geo.Line newAxis = Geo.Line.ByStartPointEndPoint(_webAxis.StartPoint, newEndPoint);

            // Compute new locations for operations relative to new axis
            foreach (hOperation op in operations)
            {
                Geo.Point opPoint = _webAxis.PointAtParameter(op._loc / _webAxis.Length);
                double newLoc = newAxis.ParameterAtPoint(opPoint) * newAxis.Length;
                op._loc = newLoc;
            }

            // Set new axis
            _webAxis = newAxis;
        }
        

        /// <summary>
        /// Sort the member's operations by operation location
        /// </summary>
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
        /// Export a list of hMembers to .hmk
        /// </summary>
        /// <param name="filePath"></param>
        /// <param name="hMembers"></param>
        /// <param name="normalLabel"></param>
        [IsVisibleInDynamoLibrary(true)]
        public static void ExportToHMK(string filePath, List<hMember> hMembers, bool normalLabel = true)
        {
            string csv = "";

            foreach (hMember member in hMembers)
            {
                csv += member.AsHMKLine(normalLabel);
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
        /// Returns a string representing the hMember as a hmk line to be produced on the Howick
        /// </summary>
        /// <param name="member"></param>
        /// <param name="normalLabel"></param>
        /// <returns></returns>
        internal string AsHMKLine(bool normalLabel = true)
        {
            string csv = "";
            var tempName = (_label == null) ? _name : _label;
            csv += "COMPONENT" + "," + tempName + ",";
            csv += (normalLabel) ? "LABEL_NRM" + "," : "LABEL_INV" + ",";
            csv += "1,";
            csv += Math.Round(_webAxis.Length, 2).ToString() + ",";

            SortOperations();
            foreach (hOperation op in operations)
            {
                csv += op._type.ToString() + "," + Math.Round(op._loc, 2).ToString() + ",";
            }

            csv += ",";
            csv += "WA_S_X," + this.WebAxis.StartPoint.X + ",";
            csv += "WA_S_Y," + this.WebAxis.StartPoint.Y + ",";
            csv += "WA_S_Z," + this.WebAxis.StartPoint.Z + ",";

            csv += "WA_E_X," + this.WebAxis.EndPoint.X + ",";
            csv += "WA_E_Y," + this.WebAxis.EndPoint.Y + ",";
            csv += "WA_E_Z," + this.WebAxis.EndPoint.Z + ",";

            csv += "WN_X," + this.WebNormal.X + ",";
            csv += "WN_Y," + this.WebNormal.Y + ",";
            csv += "WN_Z," + this.WebNormal.Z;

            return csv;
        }


        /// <summary>
        /// Returns a string representing the hMember as a csv line that can be sent to the Howick
        /// </summary>
        /// <param name="normalLabel"></param>
        /// <returns></returns>
        internal string AsCSVLine(bool normalLabel = true)
        {
            string csv = "";
            csv += "COMPONENT" + "," + _label + ",";
            csv += (normalLabel) ? "LABEL_NRM" + "," : "LABEL_INV" + ",";
            csv += "1,";
            csv += Math.Round(_webAxis.Length, 2).ToString();

            SortOperations();
            foreach (hOperation op in operations)
            {
                csv += "," + op._type.ToString() + "," + Math.Round(op._loc, 2).ToString();
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


        [IsVisibleInDynamoLibrary(true)]
        [MultiReturn(new[] { "member", "operations" })]
        public static Dictionary<string, object> DrawAsMesh(hMember member)
        {
            Geo.Point OP1 = member._webAxis.StartPoint;
            Geo.Point OP2 = member._webAxis.EndPoint;

            Geo.Vector webAxis = Geo.Vector.ByTwoPoints(OP1, OP2);
            Geo.Vector normal = member._webNormal;
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
                points.Add(member._webAxis.PointAtParameter(op._loc / member._webAxis.Length));
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
        public static Dictionary<string, object> DrawAsPolysurface(hMember member)
        {
            Geo.Point OP1 = member._webAxis.StartPoint;
            Geo.Point OP2 = member._webAxis.EndPoint;

            Geo.Vector webAxis = Geo.Vector.ByTwoPoints(OP1, OP2);
            Geo.Vector normal = member._webNormal;
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


        internal List<Geo.Curve> GetOperationCutter(hOperation op)
        {
            Geo.Point OP1 = this._webAxis.StartPoint;
            Geo.Point OP2 = this._webAxis.EndPoint;

            Geo.Vector webAxis = Geo.Vector.ByTwoPoints(OP1, OP2);
            Geo.Vector normal = _webNormal;
            Geo.Vector lateral = webAxis.Cross(normal);
            lateral = lateral.Normalized();
            lateral = lateral.Scale(1.75);
            normal = normal.Normalized();
            normal = normal.Scale(1.5);
            Geo.Vector lateralR = Geo.Vector.ByCoordinates(lateral.X * -1, lateral.Y * -1, lateral.Z * -1);
            Geo.Vector webAxisR = Geo.Vector.ByCoordinates(webAxis.X * -1, webAxis.Y * -1, webAxis.Z * -1);
            Geo.Vector normalR = Geo.Vector.ByCoordinates(normal.X * -1, normal.Y * -1, normal.Z * -1);

            Geo.Point opPoint = this._webAxis.PointAtParameter(op._loc / this._webAxis.Length);

            switch (op._type)
            {
                case Operation.WEB:
                    Geo.Circle center = Geo.Circle.ByCenterPointRadiusNormal(Geo.Point.ByCoordinates(opPoint.X, opPoint.Y, opPoint.Z), 0.125, normal);
                    // Dispose
                    { 
                        OP1.Dispose();
                        OP2.Dispose();
                        lateral.Dispose();
                        lateralR.Dispose();
                        webAxisR.Dispose();
                        normalR.Dispose();
                        opPoint.Dispose();
                    }
                    return new List<Geo.Curve> { center };

                default:
                    return null;
                /*case Operation.DIMPLE:
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

                case Operation.SWAGE:
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

                case Operation.BOLT:
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

                case Operation.SERVICE_HOLE:
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


                case Operation.NOTCH:
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

                case Operation.LIP_CUT:
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


                case Operation.END_TRUSS:
                    lateral = lateral.Normalized().Scale(1.76);
                    lateralR = lateralR.Normalized().Scale(1.76);
                    if (opPoint.DistanceTo(this._webAxis.StartPoint) < opPoint.DistanceTo(this._webAxis.EndPoint))
                    {
                        webAxis = webAxis.Normalized().Scale(0.5);
                    }
                    else
                    {
                        webAxis = webAxisR.Normalized().Scale(0.5);
                    }

                    normal = normal.Normalized().Scale(1.50);
                    Geo.Vector normal2 = normal.Normalized().Scale(1.0);
                    Geo.Vector normal3 = normal.Normalized().Scale(0.5);
                    normalR = normalR.Normalized().Scale(0.01);
                    lateral2 = lateral.Normalized().Scale(1.25);
                    lateral2R = lateralR.Normalized().Scale(1.25);

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
                    break;

                default:
                    package.AddPointVertex(opPoint.X, opPoint.Y, opPoint.Z);
                    package.AddPointVertexColor(r, g, b, a);
                    break;*/

            }
        }

        #region IGraphicItem Interface

        internal void TessellateOperation(IRenderPackage package, TessellationParameters parameters, hOperation op)
        {
            package.RequiresPerVertexColoration = true;

            Geo.Point OP1 = this._webAxis.StartPoint;
            Geo.Point OP2 = this._webAxis.EndPoint;

            Geo.Vector webAxis = Geo.Vector.ByTwoPoints(OP1, OP2);
            Geo.Vector normal = _webNormal;
            Geo.Vector lateral = webAxis.Cross(normal);
            lateral = lateral.Normalized();
            lateral = lateral.Scale(1.75);
            normal = normal.Normalized();
            normal = normal.Scale(1.5);
            Geo.Vector lateralR = Geo.Vector.ByCoordinates(lateral.X * -1, lateral.Y * -1, lateral.Z * -1);
            Geo.Vector webAxisR = Geo.Vector.ByCoordinates(webAxis.X * -1, webAxis.Y * -1, webAxis.Z * -1);
            Geo.Vector normalR = Geo.Vector.ByCoordinates(normal.X * -1, normal.Y * -1, normal.Z * -1);

            Geo.Point opPoint = this._webAxis.PointAtParameter(op._loc / this._webAxis.Length);
            byte r = 255;
            byte g = 66;
            byte b = 57;
            byte a = 255;
            switch (op._type)
            {
                case Operation.WEB:
                    package.AddPointVertex(opPoint.X, opPoint.Y, opPoint.Z);
                    package.AddPointVertexColor(r, g, b, a);

                    lateral = lateral.Normalized().Scale(15.0 / 16.0);
                    lateralR = lateralR.Normalized().Scale(15.0 / 16.0);

                    package.AddPointVertex(opPoint.Add(lateral).X, opPoint.Add(lateral).Y, opPoint.Add(lateral).Z);
                    package.AddPointVertexColor(r, g, b, a);
                    package.AddPointVertex(opPoint.Add(lateralR).X, opPoint.Add(lateralR).Y, opPoint.Add(lateralR).Z);
                    package.AddPointVertexColor(r, g, b, a);
                    break;

                case Operation.DIMPLE:
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

                case Operation.SWAGE:
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

                case Operation.BOLT:
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

                case Operation.SERVICE_HOLE:
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


                case Operation.NOTCH:
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

                case Operation.LIP_CUT:
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


                case Operation.END_TRUSS:
                    lateral = lateral.Normalized().Scale(1.76);
                    lateralR = lateralR.Normalized().Scale(1.76);
                    if (opPoint.DistanceTo(this._webAxis.StartPoint) < opPoint.DistanceTo(this._webAxis.EndPoint))
                    {
                        webAxis = webAxis.Normalized().Scale(0.5);
                    }
                    else
                    {
                        webAxis = webAxisR.Normalized().Scale(0.5);
                    }

                    normal = normal.Normalized().Scale(1.50);
                    Geo.Vector normal2 = normal.Normalized().Scale(1.0);
                    Geo.Vector normal3 = normal.Normalized().Scale(0.5);
                    normalR = normalR.Normalized().Scale(0.01);
                    lateral2 = lateral.Normalized().Scale(1.25);
                    lateral2R = lateralR.Normalized().Scale(1.25);

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

            Geo.Point OP1 = this._webAxis.StartPoint;
            Geo.Point OP2 = this._webAxis.EndPoint;

            Geo.Vector webAxis = Geo.Vector.ByTwoPoints(OP1, OP2);
            Geo.Vector normal = _webNormal;
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

            foreach (hOperation op in operations)
            {
                TessellateOperation(package, parameters, op);
            }
        }

 
        #endregion
    }
}
