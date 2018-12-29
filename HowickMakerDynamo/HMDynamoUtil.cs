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
    class HMDynamoUtil
    {
        public static HM.Triple PointToTriple(Geo.Point pt)
        {
            return new HM.Triple(pt.X, pt.Y, pt.Z);
        }

        public static HM.Triple VectorToTriple(Geo.Vector v)
        {
            return new HM.Triple(v.X, v.Y, v.Z);
        }

        public static HM.Line DSLineToHMLine(Geo.Line l)
        {
            return new HM.Line(PointToTriple(l.StartPoint), PointToTriple(l.EndPoint));
        }
    }
}
