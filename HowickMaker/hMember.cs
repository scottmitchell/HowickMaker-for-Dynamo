using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace HowickMaker
{
    /// <summary>
    /// Represents a steel stud
    /// </summary>
    public class hMember
    {
        public List<hConnection> Connections {
            get
            {
                return connections;
            }
        }
        public List<hOperation> Operations
        {
            get
            {
                return operations;
            }
        }

        /// <summary>
        /// The web axis of a member
        /// </summary>
        /// <returns name="webAxis">The web axis of the member</returns>
        public Line WebAxis
        {
            get { return _webAxis; }
        }
        internal Line _webAxis;

        /// <summary>
        /// The web normal of a member
        /// </summary>
        public Triple WebNormal
        {
            get { return _webNormal; }
        }
        internal Triple _webNormal;

        /// <summary>
        /// The flange axes of a member
        /// </summary>
        public List<Line> FlangeAxes
        {
            get { return GetFlangeAxes(); }
        }
        
        /// <summary>
        /// The name of a member
        /// </summary>
        public string Name
        {
            get { return (_label == null) ? _name : _label; }
            set { _label = value; _name = value; }
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
        
        public hMember()
        {

        }
        
        public hMember(hMember member)
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
        
        public hMember(Line webAxis, Triple webNormal, string name = "0")
        {
            _webAxis = webAxis;
            _webNormal = webNormal;
            _name = name;
        }
        
        public hMember(Line webAxis, string name = "0")
        {
            _webAxis = webAxis;
            _webNormal = null;
            _name = name;
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
        public void AddOperationByPointType(Triple pt, string type)
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
        public void AddOperationByLocationType(double location, string type)
        {
            hOperation op = new hOperation(location, (Operation)System.Enum.Parse(typeof(Operation), type));
            AddOperation(op);
        }


        /// <summary>
        /// Add an hOperation to the member's list of operations
        /// </summary>
        /// <param name="operation"></param>
        public void AddOperation(hOperation operation)
        {
            this.operations.Add(operation);
        }


        /// <summary>
        /// Add an hConnection to the member's list of connections
        /// </summary>
        /// <param name="connection"></param>
        public void AddConnection(hConnection connection)
        {
            this.connections.Add(connection);
        }


        /// <summary>
        /// Gets the lines that run along the center of each flange of the member, parallel to the web axis
        /// </summary>
        /// <param name="member"></param>
        /// <returns></returns>
        public List<Line> GetFlangeAxes()
        {
            var OP1 = WebAxis.StartPoint;
            var OP2 = WebAxis.EndPoint;
            var webAxisVec = OP2 - OP1;
            var normal = WebNormal.Normalized().Scale(0.75); ;
            var lateral = webAxisVec.Cross(normal).Normalized().Scale(1.75);
            var flangeLine1 = new Line(OP1.Add(normal.Add(lateral)), OP2.Add(normal.Add(lateral)));
            lateral = webAxisVec.Cross(normal).Normalized().Scale(-1.75);
            var flangeLine2 = new Line(OP1.Add(normal.Add(lateral)), OP2.Add(normal.Add(lateral)));
            return new List<Line> { flangeLine1, flangeLine2 };
        }


        /// <summary>
        /// Extend member by changing web axis start point. Adjust operations accordingly.
        /// </summary>
        /// <param name="newStartPoint"></param>
        internal void SetWebAxisStartPoint(Triple newStartPoint)
        {
            // Create new axis
            Line newAxis = new Line(newStartPoint, _webAxis.EndPoint);

            // Compute new locations for operations relative to new axis
            foreach (hOperation op in operations)
            {
                Triple opPoint = _webAxis.PointAtParameter(op._loc / _webAxis.Length);
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
        internal void SetWebAxisEndPoint(Triple newEndPoint)
        {
            // Create new axis
            Line newAxis = new Line(_webAxis.StartPoint, newEndPoint);

            // Compute new locations for operations relative to new axis
            foreach (hOperation op in operations)
            {
                Triple opPoint = _webAxis.PointAtParameter(op._loc / _webAxis.Length);
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





        
    }
}
