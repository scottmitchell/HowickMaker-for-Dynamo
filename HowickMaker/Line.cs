using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HowickMaker
{
    public class Line
    {
        public Triple StartPoint { get; set; }
        public Triple EndPoint { get; set; }

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

        public Triple ClosestPointOnLine(Line other)
        {
            Triple startPt1 = this.StartPoint;
            Triple startPt2 = other.StartPoint;

            Triple vec1 = Triple.ByTwoPoints(this.StartPoint, this.EndPoint);
            Triple vec2 = Triple.ByTwoPoints(other.StartPoint, other.EndPoint);

            double x1 = startPt1.X;
            double y1 = startPt1.Y;
            double z1 = startPt1.Z;
            double x2 = startPt2.X;
            double y2 = startPt2.Y;
            double z2 = startPt2.Z;

            double a1 = vec1.X;
            double b1 = vec1.Y;
            double c1 = vec1.Z;
            double a2 = vec2.X;
            double b2 = vec2.Y;
            double c2 = vec2.Z;

            double minD = 0.00001;

            double f = a1 * a2 + b1 * b2 + c1 * c2;
            f = (f == 0) ? minD : f;
            double g = -(a1 * a1 + b1 * b1 + c1 * c1);
            double h = -(a1 * (x2 - x1) + b1 * (y2 - y1) + c1 * (z2 - z1));
            h = (h == 0) ? minD : h;
            double i = (a2 * a2 + b2 * b2 + c2 * c2);
            double j = -1 * f;
            double k = -(a2 * (x2 - x1) + b2 * (y2 - y1) + c2 * (z2 - z1));
            k = (k == 0) ? minD : k;

            double t = (k - (h * i / f)) / (j - (g * i / f));

            double xp = x1 + (a1 * t);
            double yp = y1 + (b1 * t);
            double zp = z1 + (c1 * t);

            return new Triple(xp, yp, zp);
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
            var closestPoint = this.ClosestPointOnLine(other);
            var t = this.ParameterAtPoint(closestPoint);
            closestPoint = other.ClosestPointOnLine(this);
            var u = other.ParameterAtPoint(closestPoint);
            if (t >= 0 && t <= 1 && u >= 0 && u <= 1 && d9 >= 0)
            {
                distances.Add(d9);
            }

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
