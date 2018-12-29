using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HowickMaker
{
    public class Line
    {
        internal Triple StartPoint;
        internal Triple EndPoint;

        internal Line(Triple start, Triple end)
        {
            this.StartPoint = start;
            this.EndPoint = end;
        }
        
        internal double Length {
            get
            {
                return Math.Sqrt(
                    Math.Pow(EndPoint.X - StartPoint.X, 2) +
                    Math.Pow(EndPoint.Y - StartPoint.Y, 2) +
                    Math.Pow(EndPoint.Z - StartPoint.Z, 2)
                    );
            }
        }

        internal Triple ToTriple()
        {
            return this.EndPoint - this.StartPoint;
        }
    }
}
