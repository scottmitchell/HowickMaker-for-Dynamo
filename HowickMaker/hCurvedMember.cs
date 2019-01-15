using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HowickMaker
{
    public class hCurvedMember
    {
        public List<hMember> Segments = new List<hMember>();
        public Triple WebNormal;
        public string Name;
        public double StudWidth = 3.5;
        public double Tolerance = 0.001;

        public hCurvedMember(List<Line> lines, string name = "CurvedMember")
        {
            Name = name;
            Triple webNormal;
            if (!isPlanar(lines, out webNormal))
            {
                throw new Exception("These lines are not all co-planar. Curved Members must be planar.");
            }
            WebNormal = webNormal;

            GenerateSegmentsFromLines(lines);
        }

        internal bool isPlanar(List<Line> lines, out Triple currentNormal)
        {
            currentNormal = lines[0].Direction.Cross( lines[1].Direction ).Normalized();
            for (int i = 1; i < lines.Count - 1; i++)
            {
                var nextNormal = lines[i].Direction.Cross( lines[i+1].Direction ).Normalized();
                if (!(Math.Abs(currentNormal.Dot(nextNormal)) > 1 - Tolerance) )
                {
                    return false;
                }
                currentNormal = nextNormal;
            }
            return true;
        }

        internal void GenerateSegmentsFromLines(List<Line> lines)
        {
            foreach (Line line in lines)
            {
                Segments.Add(new hMember(line, WebNormal, Name));
            }

            for (int i = 0; i < Segments.Count - 1; i++)
            {
                ResolveSegmentConnection(i, i + 1);
            }
        }

        internal void ResolveSegmentConnection(int i, int j)
        {
            var angleD = 180 - Segments[i].WebAxis.Direction.AngleWithVector(Segments[j].WebAxis.Direction.Reverse());
            var angle = angleD * (Math.PI / 180);
            var x = (StudWidth / 2) * Math.Tan(angle / 2);

            var oldEndPoint = Segments[i].WebAxis.EndPoint;
            var newEndPoint = oldEndPoint.Add( Segments[i].WebAxis.Direction.Normalized().Scale(x) );
            var dimplePointA = oldEndPoint.Add(Segments[i].WebAxis.Direction.Normalized().Scale(x).Reverse());
            Segments[i].SetWebAxisEndPoint(newEndPoint);
            Segments[i].AddOperationByPointType(newEndPoint, "END_TRUSS");
            Segments[i].AddOperationByPointType(dimplePointA, "DIMPLE");

            var oldStartPoint = Segments[j].WebAxis.StartPoint;
            var newStartPoint = oldStartPoint.Add(Segments[j].WebAxis.Direction.Normalized().Scale(x).Reverse());
            var dimplePointB = oldStartPoint.Add(Segments[j].WebAxis.Direction.Normalized().Scale(x));
            Segments[j].SetWebAxisStartPoint(newStartPoint);
            Segments[j].AddOperationByPointType(newStartPoint, "END_TRUSS");
            Segments[j].AddOperationByPointType(dimplePointB, "DIMPLE");
        }
    }
}
