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
    public class CurvedMember
    {
        public static List<Component> CurvedMemberFromPolyline(List<Geo.Line> lines)
        {
            var HMLines = lines.Select(l => HMDynamoUtil.DSLineToHMLine(l));
            var curvedMember = new HM.hCurvedMember(HMLines.ToList());
            return curvedMember.Segments.Select(m => new Component(m)).ToList();
        }
    }
}
