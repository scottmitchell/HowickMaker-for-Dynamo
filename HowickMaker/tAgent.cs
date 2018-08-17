using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Geo = Autodesk.DesignScript.Geometry;
using Autodesk.DesignScript.Interfaces;
using Autodesk.DesignScript.Runtime;
using Mesh = Autodesk.Dynamo.MeshToolkit.Mesh;


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
            Random r = new Random();
            _currentParameter = 0.45;
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


        internal void SetLines()
        {
            _lineA = Geo.Line.ByStartPointDirectionLength(_edge.PointAtParameter(_currentParameter), _lineDirectionA, 1);
            _lineB = (_isNaked) ? null : Geo.Line.ByStartPointDirectionLength(_edge.PointAtParameter(_currentParameter), _lineDirectionB, 1);
        }


        /// <summary>
        /// Iterative Relaxtion
        /// </summary>
        /// <param name="agents"></param>
        /// <param name="amount"></param>
        /// <param name="limit"></param>
        /// <returns></returns>
        internal string Step(tAgent[] agents, double amount = 0.01, double limit = 0.1)
        {
            // Get value of moving back
            double paramMinus = new List<double> { limit, _currentParameter - amount }.Max();
            double valueMinus = GetStateValue(agents, paramMinus);

            // Get value of staying
            double valueCurrent = GetStateValue(agents, _currentParameter);

            // Get value of moving forward
            double paramPlus = new List<double> { 1.0 - limit, _currentParameter + amount }.Min();
            double valuePlus = GetStateValue(agents, paramPlus);

            // Get deltas
            double deltaMinus = valueMinus - valueCurrent;
            double deltaPlus = valuePlus - valueCurrent;

            // Choose
            if (deltaMinus < deltaPlus)
            {
                if (valueMinus < valueCurrent)
                {
                    _currentParameter = paramMinus;
                    SetLines();
                    return "vM = " + valueMinus + ", vC = " + valueCurrent + ", vP = " + valuePlus;
                }
            }
            else
            {
                if (valuePlus < valueCurrent)
                {
                    _currentParameter = paramPlus;
                    SetLines();
                    return "vM = " + valueMinus + ", vC = " + valueCurrent + ", vP = " + valuePlus;
                }
            }
            return "vM = " + valueMinus + ", vC = " + valueCurrent + ", vP = " + valuePlus;
        }



        /// <summary>
        /// 
        /// </summary>
        /// <param name="mesh"></param>
        /// <param name="parameter"></param>
        /// <param name="desiredOffset"></param>
        /// <param name="agents"></param>
        /// <returns></returns>
        internal double GetStateValue(tAgent[] agents, double parameter, double desiredOffset = 6.0)
        {
            int furtherAgentIndexA = -1;
            Geo.Point intersectA = GetFurtherIntersectionAtParameter(parameter, _faceIndexA, agents, out furtherAgentIndexA);
            Geo.Point edgeIntersectionA = HowickMaker.hStructure.ClosestPointToOtherLine(agents[furtherAgentIndexA]._edge, GetMemberLinesAtParameter(agents, parameter)[0]);
            double outOfBounds = _edge.PointAtParameter(parameter).DistanceTo(intersectA) - _edge.PointAtParameter(parameter).DistanceTo(edgeIntersectionA);
            double value1 = Math.Abs(desiredOffset - intersectA.DistanceTo(agents[furtherAgentIndexA]._edge));
            if (outOfBounds > desiredOffset)
            {
                value1 *= 100 * outOfBounds;
            }

            double value2 = 0;
            if (!_isNaked)
            {
                int furtherAgentIndexB = -1;
                Geo.Point intersectB = GetFurtherIntersectionAtParameter(parameter, _faceIndexB, agents, out furtherAgentIndexB);
                Geo.Point edgeIntersectionB = HowickMaker.hStructure.ClosestPointToOtherLine(agents[furtherAgentIndexB]._edge, GetMemberLinesAtParameter(agents, parameter)[1]);
                value2 = Math.Abs(desiredOffset - intersectB.DistanceTo(agents[furtherAgentIndexB]._edge));
                double outOfBounds2 = _edge.PointAtParameter(parameter).DistanceTo(intersectB) - _edge.PointAtParameter(parameter).DistanceTo(edgeIntersectionB);
                value2 = Math.Abs(desiredOffset - intersectB.DistanceTo(agents[furtherAgentIndexB]._edge));
                if (outOfBounds2 > desiredOffset)
                {
                    value2 *= 100 * outOfBounds2;
                }
            }

            return value1 + value2;
        }



        /// <summary>
        /// Draw the member lines given the agents current state
        /// </summary>
        /// <param name="agents"></param>
        /// <returns></returns>
        internal List<Geo.Line> GetMemberLines(tAgent[] agents)
        {
            var lines = new List<Geo.Line>();
            Geo.Point p = GetFurtherIntersection(_faceIndexA, agents);
            lines.Add(Geo.Line.ByStartPointEndPoint(_edge.PointAtParameter(_currentParameter), p));
            if (!_isNaked)
            {
                p = GetFurtherIntersection(_faceIndexB, agents);
                lines.Add(Geo.Line.ByStartPointEndPoint(_edge.PointAtParameter(_currentParameter), p));
            }

            return lines;
        }


        internal List<Geo.Line> GetMemberLinesAtParameter(tAgent[] agents, double parameter)
        {
            var lines = new List<Geo.Line>();
            Geo.Point p = GetFurtherIntersection(_faceIndexA, agents);
            lines.Add(Geo.Line.ByStartPointDirectionLength(_edge.PointAtParameter(parameter), _lineDirectionA, 1));
            if (!_isNaked)
            {
                p = GetFurtherIntersection(_faceIndexB, agents);
                lines.Add(Geo.Line.ByStartPointDirectionLength(_edge.PointAtParameter(parameter), _lineDirectionB, 1));
            }

            return lines;
        }


        /// <summary>
        /// Get the correct line from a neighboring agent
        /// </summary>
        /// <param name="faceIndex"></param>
        /// <returns></returns>
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


        /// <summary>
        /// Find the intersection with other two members of a given face that is further away from the agent
        /// </summary>
        /// <param name="faceIndex"></param>
        /// <param name="agents"></param>
        /// <returns></returns>
        internal Geo.Point GetFurtherIntersection(int faceIndex, tAgent[] agents)
        {
            Geo.Point p;
            Geo.Point p2;
            if (faceIndex == _faceIndexA)
            {
                p = HowickMaker.hStructure.ClosestPointToOtherLine(_lineA, agents[_neighborsA[0]].GetLine(_faceIndexA));
                p2 = HowickMaker.hStructure.ClosestPointToOtherLine(_lineA, agents[_neighborsA[1]].GetLine(_faceIndexA));
            }
            else if (faceIndex == _faceIndexB)
            {
                p = HowickMaker.hStructure.ClosestPointToOtherLine(_lineB, agents[_neighborsB[0]].GetLine(_faceIndexB));
                p2 = HowickMaker.hStructure.ClosestPointToOtherLine(_lineB, agents[_neighborsB[1]].GetLine(_faceIndexB));
            }
            else
            {
                throw new System.ArgumentException("Invalid faceIndex for this agent", _name.ToString());
            }

            if (_edge.PointAtParameter(_currentParameter).DistanceTo(p) > _edge.PointAtParameter(_currentParameter).DistanceTo(p2))
            {
                return p;
            }
            else
            {
                return p2;
            }
        }


        internal Geo.Point GetFurtherIntersectionAtParameter(double parameter, int faceIndex, tAgent[] agents, out int FurtherAgentIndex)
        {
            var linesAtParameter = GetMemberLinesAtParameter(agents, parameter);
            Geo.Point p;
            Geo.Point p2;
            if (faceIndex == _faceIndexA)
            {
                p = HowickMaker.hStructure.ClosestPointToOtherLine(linesAtParameter[0], agents[_neighborsA[0]].GetLine(_faceIndexA));
                p2 = HowickMaker.hStructure.ClosestPointToOtherLine(linesAtParameter[0], agents[_neighborsA[1]].GetLine(_faceIndexA));
            }
            else if (faceIndex == _faceIndexB)
            {
                p = HowickMaker.hStructure.ClosestPointToOtherLine(linesAtParameter[1], agents[_neighborsB[0]].GetLine(_faceIndexB));
                p2 = HowickMaker.hStructure.ClosestPointToOtherLine(linesAtParameter[1], agents[_neighborsB[1]].GetLine(_faceIndexB));
            }
            else
            {
                throw new System.ArgumentException("Invalid faceIndex for this agent", _name.ToString());
            }

            if (_edge.PointAtParameter(parameter).DistanceTo(p) > _edge.PointAtParameter(parameter).DistanceTo(p2))
            {
                FurtherAgentIndex = (faceIndex == _faceIndexA) ? _neighborsA[0] : _neighborsB[0];
                return p;
            }
            else
            {
                FurtherAgentIndex = (faceIndex == _faceIndexA) ? _neighborsA[1] : _neighborsB[1];
                return p2;
            }
        }


       


        /// <summary>
        /// Return relevant agent info as a string
        /// </summary>
        /// <returns></returns>
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
