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
    public class Component : IGraphicItem
    {
        




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
        public static HM.hMember ByLineVector(Geo.Line webAxis, Geo.Vector webNormal, string name = "0")
        {
            // DS to HM conversions
            var hWebAxis = HMDynamoUtil.DSLineToHMLine(webAxis);
            var hWebNormal = HMDynamoUtil.VectorToTriple(webNormal);

            var member = new HM.hMember(hWebAxis, hWebNormal.Normalized(), name);
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
        public HM.hMember AddName(HM.hMember member, string name)
        {
            HM.hMember newMember = new HM.hMember(member);
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
        public HM.hMember AddOperationByLocationType(HM.hMember member, List<double> locations, List<string> types)
        {
            HM.hMember newMember = new HM.hMember(this);
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
                newMember.AddOperationByPointType(Triple.FromPoint(points[i]), types[i]);
            }
            return newMember;
        }


        /// <summary>
        /// Gets the lines that run along the center of each flange of the member, parallel to the web axis
        /// </summary>
        /// <param name="member"></param>
        /// <returns></returns>
        [IsVisibleInDynamoLibrary(false)]
        public List<Line> GetFlangeAxes()
        {
            Triple OP1 = _webAxis.StartPoint;
            Triple OP2 = _webAxis.EndPoint;
            Triple webAxisVec = OP2 - OP1;
            Triple normal = _webNormal.Normalized().Scale(0.75); ;
            Triple lateral = webAxisVec.Cross(normal).Normalized().Scale(1.75);
            Line flangeLine1 = new Line(OP1.Add(normal.Add(lateral)), OP2.Add(normal.Add(lateral)));
            lateral = webAxisVec.Cross(normal).Normalized().Scale(-1.75);
            Line flangeLine2 = new Line(OP1.Add(normal.Add(lateral)), OP2.Add(normal.Add(lateral)));
            return new List<Line> { flangeLine1, flangeLine2 };
        }
    }
}
