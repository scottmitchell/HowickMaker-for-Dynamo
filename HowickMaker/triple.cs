using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HowickMaker
{
    public class Triple
    {
        internal double X;
        internal double Y;
        internal double Z;

        internal Triple(double X, double Y, double Z)
        {
            this.X = X;
            this.Y = Y;
            this.Z = Z;
        }

        internal double Magnitude
        {
            get
            {
                return Math.Sqrt((this.X * this.X) + (this.Y * this.Y) + (this.Z * this.Z));
            }
        }

        internal Triple Cross(Triple other)
        {
            var newX = this.Y * other.Z - this.Z * other.Y;
            var newY = this.Z * other.X - this.X * other.Z;
            var newZ = this.X * other.Y - this.Y * other.X;
            return new Triple(newX, newY, newZ);
        }

        internal double Dot(Triple other)
        {
            return (this.X* other.X + this.Y * other.Y + this.Z * other.Z);
        }
        internal double AngleWithVector(Triple other)
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

        internal Triple Normalized()
        {
            var length = Math.Sqrt((this.X * this.X) + (this.Y * this.Y) + (this.Z * this.Z));
            return new Triple(this.X / length, this.Y / length, this.Z / length);
        }

        internal Triple Scale(double val)
        {
            return new Triple(this.X * val, this.Y * val, this.Z * val);
        }

        internal Triple Reverse()
        {
            return new Triple(this.X * -1, this.Y * -1, this.Z * -1);
        }

        internal static Triple ByTwoPoints(Triple t, Triple u)
        {
            return u - t;
        }
    }
}
