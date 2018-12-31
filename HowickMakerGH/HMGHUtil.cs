using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Rhino.Geometry;
using HM = HowickMaker;

namespace HowickMakerGH
{
    class HMGHUtil
    {
        public static HM.Line GHLineToHMLine(Line l)
        {
            var start = PointToTriple(l.From);
            var end = PointToTriple(l.To);
            return new HM.Line(start, end);
        }

        public static HM.Triple PointToTriple(Point3d pt)
        {
            return new HM.Triple(pt.X, pt.Y, pt.Z);
        }

        public static Point3d TripleToPoint(HM.Triple t)
        {
            return new Point3d(t.X, t.Y, t.Z);
        }

        public static HM.Triple VectorToTriple(Vector3d v)
        {
            return new HM.Triple(v.X, v.Y, v.Z);
        }
    }
}
