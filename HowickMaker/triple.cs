using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Geo = Autodesk.DesignScript.Geometry;
using Autodesk.DesignScript.Interfaces;
using Autodesk.DesignScript.Runtime;
using Mesh = Autodesk.Dynamo.MeshToolkit.Mesh;

namespace HowickMaker
{
    class triple
    {
        double x;
        double y;
        double z;

        internal triple(Geo.Point pt)
        {
            this.x = pt.X;
            this.y = pt.Y;
            this.z = pt.Z;
        }

        public override int GetHashCode()
        {
            return (int)(31 * this.x + 17 * this.y + 11 * this.z);
        }

        public override bool Equals(object obj)
        {
            return obj is triple && Equals((triple)obj);
        }

        public bool Equals(triple p)
        {
            return x == p.x && y == p.y && z == p.z;
        }
    }
}
