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

        public Line(Triple start, Triple end)
        {
            this.StartPoint = start;
            this.EndPoint = end;
        }
        
        public double Length {
            get
            {
                return Math.Sqrt(
                    Math.Pow(EndPoint.X - StartPoint.X, 2) +
                    Math.Pow(EndPoint.Y - StartPoint.Y, 2) +
                    Math.Pow(EndPoint.Z - StartPoint.Z, 2)
                    );
            }
        }

        public Triple ToTriple()
        {
            return this.EndPoint - this.StartPoint;
        }

        public Triple Direction
        {
            get { return this.EndPoint - this.StartPoint; }
        }

        public double ParameterAtPoint(Triple pt)
        {
            Triple v = this.Direction;
            Triple u = this.StartPoint - pt;
            var t = -1 * v.Dot(u) / v.Dot(v);
            return t;
        }

        public Triple PointAtParameter(double parameter)
        {
            return this.StartPoint + this.Direction.Scale(parameter);
        }

        public Triple ClosestPointOnLine(Triple pt)
        {
            var closestPointParameter = ParameterAtPoint(pt);
            return PointAtParameter(closestPointParameter);
        }

        public double DistanceTo(Triple pt)
        {
            var t = ParameterAtPoint(pt);

            if (t < 0)
            {
                return (pt.DistanceTo(this.StartPoint));
            }
            else if (t >= 0 && t <= 1)
            {
                return (ClosestPointOnLine(pt) - pt).Magnitude;
            }
            else
            {
                return (pt.DistanceTo(this.EndPoint));
            }
        }

        public double DistanceTo(Line other)
        {
            var distances = new List<double>();
            
            distances.Add( other.DistanceTo(this.StartPoint) );
            
            distances.Add( other.DistanceTo(this.EndPoint) );
            
            distances.Add( this.DistanceTo(other.StartPoint) );

            distances.Add( this.DistanceTo(other.EndPoint) );
            
            distances.Add( this.StartPoint.DistanceTo(other.StartPoint) );
            
            distances.Add( this.StartPoint.DistanceTo(other.EndPoint) );
            
            distances.Add( this.EndPoint.DistanceTo(other.StartPoint) );
            
            distances.Add( this.EndPoint.DistanceTo(other.EndPoint) );

            double d9 = LineToLineDistance(other);
            if (d9 > 0) { distances.Add(d9); }

            return distances.Min();
        }

        public double LineToLineDistance(Line other)
        {
            var n = this.Direction.Cross(other.Direction).Normalized();
            var temp = other.StartPoint - this.StartPoint;
            return n.Dot(temp);
        }
    }
}
