using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HowickMaker
{
    public class Triple
    {
        public double X;
        public double Y;
        public double Z;

        public Triple(double X, double Y, double Z)
        {
            this.X = X;
            this.Y = Y;
            this.Z = Z;
        }

        public double Magnitude
        {
            get
            {
                return Math.Sqrt((this.X * this.X) + (this.Y * this.Y) + (this.Z * this.Z));
            }
        }

        public Triple Cross(Triple other)
        {
            var newX = this.Y * other.Z - this.Z * other.Y;
            var newY = this.Z * other.X - this.X * other.Z;
            var newZ = this.X * other.Y - this.Y * other.X;
            return new Triple(newX, newY, newZ);
        }

        public double Dot(Triple other)
        {
            return (this.X* other.X + this.Y * other.Y + this.Z * other.Z);
        }
        
        public double DistanceTo(Triple other)
        {
            return (other - this).Magnitude;
        }

        public double AngleWithVector(Triple other)
        {
            var x = this.Dot(other) / (this.Magnitude * other.Magnitude);
            return Math.Acos(x) * (180 / Math.PI);
        }

        public static Triple operator +(Triple t, Triple u)
        {
            return new Triple(t.X + u.X, t.Y + u.Y, t.Z + u.Z);
        }

        public Triple Add(Triple t)
        {
            return this + t;
        }

        public static Triple operator -(Triple t, Triple u)
        {
            return new Triple(t.X - u.X, t.Y - u.Y, t.Z - u.Z);
        }

        public Triple Normalized()
        {
            var length = Math.Sqrt((this.X * this.X) + (this.Y * this.Y) + (this.Z * this.Z));
            return new Triple(this.X / length, this.Y / length, this.Z / length);
        }

        public Triple Scale(double val)
        {
            return new Triple(this.X * val, this.Y * val, this.Z * val);
        }

        public Triple Reverse()
        {
            return new Triple(this.X * -1, this.Y * -1, this.Z * -1);
        }

        public static Triple ByTwoPoints(Triple t, Triple u)
        {
            return u - t;
        }

        public double DistanceTo(Line l)
        {
            return l.DistanceTo(this);
        }

        public Triple Rotate(Triple axis, float angle, Triple origin = null)
        {
            if (origin == null)
            {
                origin = new Triple(0, 0, 0);
            }
            Triple z = axis.Normalized();
            Triple x = z.GeneratePerpendicular().Normalized();
            Triple y = z.Cross(x).Normalized();

            Triple v = this - origin;

            double vx = x.Dot(v);
            double vy = y.Dot(v);
            double vz = z.Dot(v);

            double sin = Math.Sin(angle);
            double cos = Math.Cos(angle);

            double vx_ = cos * vx - sin * vy;
            double vy_ = sin * vx + cos * vy;

            return origin + x * vx_ + y * vy_ + z * vz;
        }

        public Triple GeneratePerpendicular()
            => X == 0f && Y == 0f && Z == 0f
                ? new Triple(0f, 0f, 0f)
                    : Math.Abs(X) < Math.Abs(Y)
                        ? Math.Abs(X) < Math.Abs(Z)
                            ? new Triple(0f, -Z, Y)
                            : new Triple(-Y, X, 0f)
                        : Math.Abs(Y) < Math.Abs(Z)
                            ? new Triple(Z, 0f, -X)
                            : new Triple(-Y, X, 0f);

        public static Triple operator *(Triple a, Triple b) => new Triple(a.X * b.X, a.Y * b.Y, a.Z * b.Z);
        public static Triple operator *(Triple a, float b) => new Triple(a.X * b, a.Y * b, a.Z * b);
        public static Triple operator *(Triple a, double b) => new Triple(a.X * (float)b, a.Y * (float)b, a.Z * (float)b);
        public static Triple operator *(float b, Triple a) => new Triple(a.X * b, a.Y * b, a.Z * b);
        public static Triple operator *(double b, Triple a) => new Triple(a.X * (float)b, a.Y * (float)b, a.Z * (float)b);
    }
}
