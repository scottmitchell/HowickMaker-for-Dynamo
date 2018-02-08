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

        public hMember(Geo.Line webAxis, Geo.Vector webNormal, int name = 0)
        {
            this.webAxis = webAxis;
            this.webNormal = webNormal;
            this.name = name;
        }

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

            foreach(hOperation op in member.operations)
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
            // This example contains information to draw a point
            package.AddPointVertex(0,0,0);
            package.AddPointVertexColor(255, 0, 0, 255);
        }
 
        #endregion
    }
}
