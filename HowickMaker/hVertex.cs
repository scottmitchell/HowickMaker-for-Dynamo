using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Geo = Autodesk.DesignScript.Geometry;

namespace HowickMaker
{
    /// <summary>
    /// Simple mesh vertex for use with HowickMaker
    /// </summary>
    internal class hVertex
    {
        internal double x;
        internal double y;
        internal double z;

        internal hVertex(double x, double y, double z)
        {
            this.x = x;
            this.y = y;
            this.z = z;
        }

        internal Geo.Point ToPoint()
        {
            return Geo.Point.ByCoordinates(x, y, z);
        }


        public override string ToString()
        {
            string s = "V:: x=" + this.x.ToString() + ", y=" + this.y.ToString() + ", z=" + this.z.ToString();
            return s;
        }
    }
}
