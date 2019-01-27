using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Geo = Autodesk.DesignScript.Geometry;
using Autodesk.DesignScript.Interfaces;
using Autodesk.DesignScript.Runtime;
using Mesh = Autodesk.Dynamo.MeshToolkit.Mesh;

using HM = HowickMaker;


namespace HowickMakerDynamo
{
    internal class tAgent
    {
        internal int _name;
        internal double _tolerance = 0.001;
        internal int _faceIndexA;
        internal int _faceIndexB;
        internal int[] _neighborsA;
        internal int[] _neighborsB;
        internal Geo.Vector _faceNormalA;
        internal Geo.Vector _faceNormalB;
        internal Geo.Surface _faceSurfaceA;
        internal Geo.Surface _faceSurfaceB;
        internal Geo.Vector _lineDirectionA;
        internal Geo.Vector _lineDirectionB;
        internal Geo.Line _lineA;
        internal Geo.Line _lineB;
        internal double _currentParameter;
        internal Geo.Line _edge;
        internal bool _isNaked;
        internal bool _isFlat;

        internal tAgent(int name)
        {
            _name = name;
            _isNaked = true;
            _faceIndexA = -1;
            _faceIndexB = -1;
            _neighborsB = null;
        }

        internal void Setup()
        {
            _currentParameter = 0.5;

            Geo.Vector edgeVector = Geo.Vector.ByTwoPoints(_edge.StartPoint, _edge.EndPoint);
            _lineDirectionA = _faceNormalA.Cross(edgeVector);
            _lineDirectionB = (_isNaked) ? null : _faceNormalB.Cross(edgeVector);
            _lineA = Geo.Line.ByStartPointDirectionLength(_edge.PointAtParameter(_currentParameter), _lineDirectionA, 1);
            _lineB = (_isNaked) ? null : Geo.Line.ByStartPointDirectionLength(_edge.PointAtParameter(_currentParameter), _lineDirectionB, 1);

            _isFlat = (_isNaked) ? false : _faceNormalA.Normalized().Dot(_faceNormalB.Normalized()) == 1;
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
            int furtherAgentIndex = -1;
            int closerAgentIndex = -1;
            GetFurtherIntersectionAtParameter(parameter, _faceIndexA, agents, out Geo.Point furtherIntersect, out Geo.Point closerIntersect, out furtherAgentIndex, out closerAgentIndex);

            Geo.Point edgeIntersectionA = HMDynamoUtil.TripleToPoint(HowickMaker.hStructure.ClosestPointToOtherLine(
                HMDynamoUtil.DSLineToHMLine(agents[furtherAgentIndex]._edge),
                HMDynamoUtil.DSLineToHMLine(GetLinesAtParameter(agents, parameter)[0])
                ));

            double value1 = -1 * Math.Abs(furtherIntersect.DistanceTo(closerIntersect));
            double outOfBounds = 0;

            double outOfBoundsA1 = furtherIntersect.DistanceTo(_faceSurfaceA);
            double outOfBoundsA2 = closerIntersect.DistanceTo(_faceSurfaceA);
            if (outOfBoundsA1 > _tolerance) { outOfBounds += 1000 * outOfBoundsA1; }
            if (outOfBoundsA2 > _tolerance) { outOfBounds += 100 * outOfBoundsA2; }



            double value2 = 0;
            if (!_isNaked)
            {
                GetFurtherIntersectionAtParameter(parameter, _faceIndexB, agents, out furtherIntersect, out closerIntersect, out furtherAgentIndex, out closerAgentIndex);

                Geo.Point edgeIntersectionB = HMDynamoUtil.TripleToPoint(HowickMaker.hStructure.ClosestPointToOtherLine(
                    HMDynamoUtil.DSLineToHMLine(agents[furtherAgentIndex]._edge),
                    HMDynamoUtil.DSLineToHMLine(GetLinesAtParameter(agents, parameter)[1])
                    ));

                value2 = -1 * Math.Abs(furtherIntersect.DistanceTo(closerIntersect));

                double outOfBoundsB1 = furtherIntersect.DistanceTo(_faceSurfaceB);
                double outOfBoundsB2 = closerIntersect.DistanceTo(_faceSurfaceB);
                if (outOfBoundsB1 > _tolerance) { outOfBounds += 1000 * outOfBoundsB1; }
                if (outOfBoundsB2 > _tolerance) { outOfBounds += 100 * outOfBoundsB2; }
            }

            double value = (_isNaked) ? value1 + outOfBounds : new List<double> { value1, value2 }.Sum() + outOfBounds;

            return value;
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
                Geo.Point p2 = GetFurtherIntersection(_faceIndexB, agents);
                if (_isFlat)
                {
                    lines.Clear();
                    lines.Add(Geo.Line.ByStartPointEndPoint(p2, p));
                }
                else
                {
                lines.Add(Geo.Line.ByStartPointEndPoint(_edge.PointAtParameter(_currentParameter), p2));
                }
            }

            // Dispose
            {
                p.Dispose();
            }
            return lines;
        }


        internal List<Geo.Line> GetLinesAtParameter(tAgent[] agents, double parameter)
        {
            var lines = new List<Geo.Line>();
            Geo.Point p = GetFurtherIntersection(_faceIndexA, agents);
            lines.Add(Geo.Line.ByStartPointDirectionLength(_edge.PointAtParameter(parameter), _lineDirectionA, 1));
            if (!_isNaked)
            {
                p = GetFurtherIntersection(_faceIndexB, agents);
                lines.Add(Geo.Line.ByStartPointDirectionLength(_edge.PointAtParameter(parameter), _lineDirectionB, 1));
            }

            // Dispose
            {
                p.Dispose();
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
                p = HMDynamoUtil.TripleToPoint(HowickMaker.hStructure.ClosestPointToOtherLine(
                    HMDynamoUtil.DSLineToHMLine(_lineA),
                    HMDynamoUtil.DSLineToHMLine(agents[_neighborsA[0]].GetLine(_faceIndexA))
                    ));
                p2 = HMDynamoUtil.TripleToPoint(HowickMaker.hStructure.ClosestPointToOtherLine(
                    HMDynamoUtil.DSLineToHMLine(_lineA),
                    HMDynamoUtil.DSLineToHMLine(agents[_neighborsA[1]].GetLine(_faceIndexA))
                    ));
            }
            else if (faceIndex == _faceIndexB)
            {
                p = HMDynamoUtil.TripleToPoint(HowickMaker.hStructure.ClosestPointToOtherLine(
                    HMDynamoUtil.DSLineToHMLine(_lineB),
                    HMDynamoUtil.DSLineToHMLine(agents[_neighborsB[0]].GetLine(_faceIndexB))
                    ));
                p2 = HMDynamoUtil.TripleToPoint(HowickMaker.hStructure.ClosestPointToOtherLine(
                    HMDynamoUtil.DSLineToHMLine(_lineB),
                    HMDynamoUtil.DSLineToHMLine(agents[_neighborsB[1]].GetLine(_faceIndexB))
                    ));
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


        internal void GetFurtherIntersectionAtParameter(double parameter, int faceIndex, tAgent[] agents, out Geo.Point furtherIntersect, out Geo.Point closerIntersect, out int furtherAgentIndex, out int closerAgentIndex)
        {
            var linesAtParameter = GetLinesAtParameter(agents, parameter);
            Geo.Point p;
            Geo.Point p2;
            if (faceIndex == _faceIndexA)
            {
                p = HMDynamoUtil.TripleToPoint(HowickMaker.hStructure.ClosestPointToOtherLine(
                    HMDynamoUtil.DSLineToHMLine(linesAtParameter[0]),
                    HMDynamoUtil.DSLineToHMLine(agents[_neighborsA[0]].GetLine(_faceIndexA))
                    ));
                p2 = HMDynamoUtil.TripleToPoint(HowickMaker.hStructure.ClosestPointToOtherLine(
                    HMDynamoUtil.DSLineToHMLine(linesAtParameter[0]),
                    HMDynamoUtil.DSLineToHMLine(agents[_neighborsA[1]].GetLine(_faceIndexA))
                    ));
            }
            else if (faceIndex == _faceIndexB)
            {
                p = HMDynamoUtil.TripleToPoint(HowickMaker.hStructure.ClosestPointToOtherLine(
                    HMDynamoUtil.DSLineToHMLine(linesAtParameter[1]),
                    HMDynamoUtil.DSLineToHMLine(agents[_neighborsB[0]].GetLine(_faceIndexB))
                    ));
                p2 = HMDynamoUtil.TripleToPoint(HowickMaker.hStructure.ClosestPointToOtherLine(
                    HMDynamoUtil.DSLineToHMLine(linesAtParameter[1]),
                    HMDynamoUtil.DSLineToHMLine(agents[_neighborsB[1]].GetLine(_faceIndexB))
                    ));
            }
            else
            {
                throw new System.ArgumentException("Invalid faceIndex for this agent", _name.ToString());
            }

            if (_edge.PointAtParameter(parameter).DistanceTo(p) > _edge.PointAtParameter(parameter).DistanceTo(p2))
            {
                furtherAgentIndex = (faceIndex == _faceIndexA) ? _neighborsA[0] : _neighborsB[0];
                closerAgentIndex = (faceIndex == _faceIndexA) ? _neighborsA[1] : _neighborsB[1];
                furtherIntersect = p;
                closerIntersect = p2;
            }
            else
            {
                furtherAgentIndex = (faceIndex == _faceIndexA) ? _neighborsA[1] : _neighborsB[1];
                closerAgentIndex = (faceIndex == _faceIndexA) ? _neighborsA[0] : _neighborsB[0];
                furtherIntersect = p2;
                closerIntersect = p;
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
