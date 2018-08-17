using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Geo = Autodesk.DesignScript.Geometry;
using Autodesk.DesignScript.Interfaces;
using Autodesk.DesignScript.Runtime;


namespace Strategies
{
    internal class tAgent
    {
        internal int _name;
        internal int _faceIndexA;
        internal int _faceIndexB;
        internal int[] _neighborsA;
        internal int[] _neighborsB;
        internal Geo.Vector _faceNormalA;
        internal Geo.Vector _faceNormalB;
        internal Geo.Vector _lineDirectionA;
        internal Geo.Vector _lineDirectionB;
        internal Geo.Line _lineA;
        internal Geo.Line _lineB;
        internal double _currentParameter;
        internal Geo.Line _edge;
        internal bool _isNaked;

        internal tAgent(int name, Geo.Line edge, List<int> neighbors, int faceIndexA, int faceIndexB, Geo.Vector faceNormalA, Geo.Vector faceNormalB)
        {
            _name = name;
            if (neighbors.Count == 2)
            {
                _neighborsA = new int[] { neighbors[0], neighbors[1] };
                _neighborsB = null;
                _isNaked = true;
            }
            else if (neighbors.Count == 4)
            {
                _neighborsA = new int[] { neighbors[0], neighbors[1] };
                _neighborsB = new int[] { neighbors[2], neighbors[3] };
                _isNaked = false;
            }
            else
            {
                throw new System.ArgumentException("Neighboring edge list should be 2 (naked edge) "
                                                 + "or 4 (interior) for triangle meshes", "neighbors");
            }

            _edge = edge;
            _currentParameter = 0.5;
            _faceIndexA = faceIndexA;
            _faceIndexB = faceIndexB;
            _faceNormalA = faceNormalA;
            _faceNormalB = faceNormalB;

            Geo.Vector edgeVector = Geo.Vector.ByTwoPoints(_edge.StartPoint, _edge.EndPoint);
            _lineDirectionA = _faceNormalA.Cross(edgeVector);
            _lineDirectionB = (_isNaked) ? null : _faceNormalB.Cross(edgeVector);
            _lineA = Geo.Line.ByStartPointDirectionLength(_edge.PointAtParameter(_currentParameter), _lineDirectionA, 1);
            _lineB = (_isNaked) ? null : Geo.Line.ByStartPointDirectionLength(_edge.PointAtParameter(_currentParameter), _lineDirectionB, 1);
        }

        internal List<Geo.Line> GetMemberLines(tAgent[] agents)
        {
            var lines = new List<Geo.Line>();
            Geo.Point p = HowickMaker.hStructure.ClosestPointToOtherLine(_lineA, agents[_neighborsA[0]].GetLine(_faceIndexA));
            lines.Add(Geo.Line.ByStartPointEndPoint(_edge.PointAtParameter(_currentParameter), p));
            if (!_isNaked)
            {
                p = HowickMaker.hStructure.ClosestPointToOtherLine(_lineB, agents[_neighborsB[0]].GetLine(_faceIndexB));
                lines.Add(Geo.Line.ByStartPointEndPoint(_edge.PointAtParameter(_currentParameter), p));
            }

            return lines;
        }


        internal Geo.Line GetLine(int faceIndex)
        {
            if (faceIndex == _faceIndexA)
            {
                return _lineA;
            }
            else if (faceIndex == _faceIndexB)
            {
                return _lineB;
            }
            else
            {
                throw new System.ArgumentException("Invalid faceIndex for this agent", _name.ToString());
            }
        }

        public override string ToString()
        {
            var str = _name.ToString() + "____ NeighborsA: " + _neighborsA[0] + ", " + _neighborsA[1] +  "____";
            if (!_isNaked)
            {
                str += "NeighborsB: " + _neighborsB[0] + ", " + _neighborsB[1] + "____";
            }
            else
            {
                str += "NAKED";
            }
            return str;
        }

    }
}
