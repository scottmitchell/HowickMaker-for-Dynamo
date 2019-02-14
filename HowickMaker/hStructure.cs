using System;
using System.Collections.Generic;
using System.Linq;

namespace HowickMaker
{
    //  ██╗  ██╗    ███████╗████████╗██████╗ ██╗   ██╗ ██████╗████████╗██╗   ██╗██████╗ ███████╗
    //  ██║  ██║    ██╔════╝╚══██╔══╝██╔══██╗██║   ██║██╔════╝╚══██╔══╝██║   ██║██╔══██╗██╔════╝
    //  ███████║    ███████╗   ██║   ██████╔╝██║   ██║██║        ██║   ██║   ██║██████╔╝█████╗  
    //  ██╔══██║    ╚════██║   ██║   ██╔══██╗██║   ██║██║        ██║   ██║   ██║██╔══██╗██╔══╝  
    //  ██║  ██║    ███████║   ██║   ██║  ██║╚██████╔╝╚██████╗   ██║   ╚██████╔╝██║  ██║███████╗
    //  ╚═╝  ╚═╝    ╚══════╝   ╚═╝   ╚═╝  ╚═╝ ╚═════╝  ╚═════╝   ╚═╝    ╚═════╝ ╚═╝  ╚═╝╚══════╝
    //                                                                                          

    /// <summary>
    /// A collection of connected hMembers 
    /// with associated connections, braces, etc.
    /// </summary>
    public class hStructure
    {
        public double _WEBHoleSpacing = (15.0 / 16);
        public double _StudHeight = 1.5;
        public double _StudWdith = 3.5;
        
        private double _tolerance = 0.001;
        private double _planarityTolerance = 0.001;
        private bool _firstConnectionIsFTF = false;
        private bool _threePieceBrace;
        private double _braceLength1;
        private double _braceLength2;

        private List<Line> _lines = new List<Line>();
        private Graph _g;
        public List<int> _solveOrder = new List<int>();
        public int[] _solvedBy;

        public hMember[] Members
        {
            get { return _members; }
        }
        private hMember[] _members;
        
        public List<hMember> BraceMembers
        {
            get { return _braceMembers; }
        }
        private List<hMember> _braceMembers = new List<hMember>();

        public List<hConnection> Connections
        {
            get { return _connections; }
        }
        private List<hConnection> _connections = new List<hConnection>();
        
        private List<string> _labels;
        private Dictionary<string, Triple> _webNormalsDict;
        private Dictionary<string, int> _priorityDict;
        private Dictionary<string, int> _extensionDict;


        internal hStructure(List<Line> lines, List<string> labels, double intersectionTolerance, double planarityTolerance, bool threePieceBrace, double braceLength1, double braceLength2, bool firstConnectionIsFTF)
        {
            _lines = lines;
            _tolerance = intersectionTolerance;
            _planarityTolerance = planarityTolerance;
            _threePieceBrace = threePieceBrace;
            _braceLength1 = braceLength1;
            _braceLength2 = braceLength2;
            _firstConnectionIsFTF = firstConnectionIsFTF;

            _members = new hMember[lines.Count];
            _solvedBy = new int[lines.Count];
            for (int i = 0; i < lines.Count; i++)
            {
                _members[i] = new hMember(lines[i], i.ToString());
                _members[i]._label = labels[i];
            }
        }


        /// <summary>
        /// Optional parameters, for use with hStructure.FromLines
        /// </summary>
        /// <param name="intersectionTolerance"></param>
        /// <param name="planarityTolerance"></param>
        /// <param name="generateBraces"></param>
        /// <param name="threePieceBrace"></param>
        /// <param name="braceLength1"></param>
        /// <param name="braceLength2"></param>
        /// <param name="firstConnectionIsFTF"></param>
        /// <returns name="options"></returns>
        public static string StructureOptions(
            double intersectionTolerance = 0.001, 
            double planarityTolerance = 0.001, 
            bool generateBraces = false, 
            bool threePieceBrace = false, 
            double braceLength1 = 6, 
            double braceLength2 = 3, 
            bool firstConnectionIsFTF = false
            )
        {
            string[] options = {
                intersectionTolerance.ToString(),
                planarityTolerance.ToString(),
                generateBraces.ToString(),
                threePieceBrace.ToString(),
                braceLength1.ToString(),
                braceLength2.ToString(),
                firstConnectionIsFTF.ToString()
            };
            string concat = String.Join(",", options.ToArray());
            return concat;
        }



        //  ██████╗ ████████╗███████╗
        //  ██╔══██╗╚══██╔══╝██╔════╝
        //  ██████╔╝   ██║   ███████╗
        //  ██╔══██╗   ██║   ╚════██║
        //  ██████╔╝   ██║   ███████║
        //  ╚═════╝    ╚═╝   ╚══════╝
        //                                                 
        
        /// <summary>
        /// Generate an hStructure from a set of input lines and other parameters
        /// </summary>
        /// <param name="lines"></param>
        /// <param name="names"></param>
        /// <param name="webNormalsDict"></param>
        /// <param name="priorityDict"></param>
        /// <param name="intersectionTolerance"></param>
        /// <param name="planarityTolerance"></param>
        /// <param name="generateBraces"></param>
        /// <param name="threePieceBraces"></param>
        /// <param name="braceLength1"></param>
        /// <param name="braceLength2"></param>
        /// <param name="firstConnectionIsFTF"></param>
        /// <returns></returns>
        public static hStructure StructureFromLines(
            List<Line> lines, 
            List<string> names, 
            Dictionary<string, Triple> webNormalsDict = null, 
            Dictionary<string, int> priorityDict = null, 
            Dictionary<string, int> extensionDict = null, 
            double intersectionTolerance = 0.001, 
            double planarityTolerance = 0.001, 
            bool generateBraces = false, 
            bool threePieceBraces = false, 
            double braceLength1 = 5, 
            double braceLength2 = 6, 
            bool firstConnectionIsFTF = false
            )
        {
            hStructure structure = new hStructure(lines, names, intersectionTolerance, planarityTolerance, threePieceBraces, braceLength1, braceLength2, firstConnectionIsFTF);
            structure._labels = names;
            structure._webNormalsDict = webNormalsDict != null ? webNormalsDict : new Dictionary<string, Triple>();
            structure._priorityDict = priorityDict != null ? priorityDict : new Dictionary<string, int>();
            structure._extensionDict = extensionDict != null ? extensionDict : new Dictionary<string, int>();
            
            // Build connectivity graph
            structure._g = graphFromLines(lines, structure._tolerance);

            // Find a starting node for each subgraph
            var startingNodes = structure._g.GetStartingNodes();

            // Make sure labels are good
            for (int i = 0; i < lines.Count; i++) { structure._members[i]._label = names[i]; }
            
            // Solve each subgraphs
            foreach (int start in startingNodes)
            {
                structure.BuildMembersAndConnectionsFromGraph(start);
            }
            
            // Make sure labels are good
            for (int i = 0; i < lines.Count; i++) { structure._members[i]._label = names[i]; }

            // Generate Braces
            if (generateBraces)
            {
                structure.GenerateBraces();
            }

            // Resolve all connections

            structure.ResolveBRConnections();
            structure.ResolveFTFConnections();
            structure.ResolveTConnections();
            structure.ResolvePTConnections();

            // Make sure labels are good
            for (int i = 0; i < lines.Count; i++) { structure._members[i]._label = names[i]; }

            return structure;
        }


        /// <summary>
        /// Creates a simple connectivity graph of a line network
        /// </summary>
        /// <param name="lines"> The list of lines we want to create a graph from </param>
        /// <returns> A connectivity graph </returns>
        internal static Graph graphFromLines(List<Line> lines, double tolerance)
        {
            // Create the connectivity graph
            Graph g = new Graph(lines.Count);
            
            // Iterate through each line...
            for (int i = 0; i < lines.Count; i++)
            {
                // ...vs every other line
                for (int j = i + 1; j < lines.Count; j++)
                {
                    // Check if the two lines intersect
                    if (lines[i].DistanceTo(lines[j]) <= tolerance)
                    {
                        // If so, i and j are neighbors
                        g.vertices[i].addNeighbor(j);
                        g.vertices[j].addNeighbor(i);
                    }
                }
            }
            return g;
        }


        /// <summary>
        /// Initiate recursive propogation across line network
        /// </summary>
        /// <param name="start"></param>
        /// <param name="lines"></param>
        internal void BuildMembersAndConnectionsFromGraph(int start = 0)
        {
            Vertex current = _g.vertices[start];
            
            hMember currentMember = new hMember(_lines[current.name], current.name.ToString());
            Line currentLine = _lines[start];
            Line nextLine = _lines[current.neighbors[0]];
            Triple normal = currentLine.Direction.Cross(nextLine.Direction);

            List<Triple> vectors;
            if (_firstConnectionIsFTF)
            {
                vectors = new List<Triple> {
                    normal,
                    normal.Reverse()
                };
            }
            else
            {
                vectors = new List<Triple> {
                    Triple.ByTwoPoints(currentLine.StartPoint, currentLine.EndPoint).Cross(normal),
                    Triple.ByTwoPoints(currentLine.StartPoint, currentLine.EndPoint).Cross(normal).Reverse()
                };
            }

            Triple choice = vectors[0];
            if (_webNormalsDict.ContainsKey(_members[start]._label))
            {
                var target = _webNormalsDict[_members[start]._label].Normalized();
                var vects = vectors.OrderBy(x => -1 * x.Normalized().Dot(target)).ToList();

                choice = vects[0];
            }

            currentMember.WebNormal = choice;

            _members[start] = currentMember;

            Propogate(start);
        }

        

        /// <summary>
        /// Recursive propogation across line network
        /// </summary>
        /// <param name="current"></param>
        /// <param name="lines"></param>
        internal void Propogate(int current)
        {
            _solveOrder.Add(current);
            hMember currentMember = _members[current];
            if (_g.vertices[current].neighbors.Count > 0)
            {
                var connections = new List<hConnection>();
                foreach (int i in _g.vertices[current].neighbors)
                {
                    connections.Add(new hConnection(GetConnectionType(current, i), new List<int> { i, current }));
                }

                var indices = connections.OrderBy(x => (int)x.type).Select(x => x.members[0]);

                // Iterate through each neighbor
                //foreach (int i in _g.vertices[current].neighbors)
                foreach (int i in indices)
                {
                    if (!(_g.vertices[i].neighbors.Count <= _members[i].Connections.Count))
                    {
                        hConnection con = new hConnection(GetConnectionType(current, i), new List<int> { i, current });

                        if (!_connections.Contains(con))
                        {
                            _connections.Add(con);
                        }
                        
                        _members[i].Connections.Add(con);
                        //_members[current].connections.Add(con);
                    }

                    // If we have not been to this vertex yet
                    if (!_g.vertices[i].visited)
                    {
                        List<Triple> vectors = GetValidNormalsForMember(i);
                        Triple choice = vectors[0];
                        if (_webNormalsDict.ContainsKey(_members[i]._label))
                        {
                            var target = _webNormalsDict[_members[i]._label];
                            var dots = new List<double>();
                            foreach (Triple v in vectors)
                            {
                                dots.Add(v.Normalized().Dot(target));
                            }
                            var vects = vectors.OrderBy(x => -1 * x.Dot(target)).ToList();
                            choice = vects[0];
                        }

                        if (vectors.Count > 0)
                        {
                            //vectors = (List<Geo.Vector>)vectors.OrderBy(x => Math.Abs(x.Dot(Geo.Vector.ByTwoPoints(currentMember.WebAxis.StartPoint, currentMember.WebAxis.EndPoint))));
                            _members[i].WebNormal = choice;
                            //_members[i]._webNormal = _webNormalsDict[_members[i]._label];
                        }


                        _g.vertices[current].visited = true;

                        // Dispose
                        /*{
                            foreach (Geo.Vector v in vectors)
                            {
                                v.Dispose();
                            }
                        }*/
                        _solvedBy[i] = current;
                        Propogate(i);
                    }
                }
            }
        }


        /// <summary>
        /// Determine valid connection type between these two members
        /// </summary>
        /// <param name="i"></param>
        /// <param name="j"></param>
        /// <returns></returns>
        internal Connection GetConnectionType(int i, int j)
        {
            Triple jointPlaneNormal = _lines[i].ToTriple().Cross(_lines[j].ToTriple());

            if (ParallelPlaneNormals(_members[i].WebNormal, jointPlaneNormal))
            {
                return Connection.FTF;
            }

            else if (PerpendicularPlaneNormals(_members[i].WebNormal, jointPlaneNormal))
            {
                if (_lines[i].StartPoint.DistanceTo(_lines[j]) < _tolerance || _lines[i].EndPoint.DistanceTo(_lines[j]) < _tolerance)
                {
                    if (_lines[j].StartPoint.DistanceTo(_lines[i]) < _tolerance || _lines[j].EndPoint.DistanceTo(_lines[i]) < _tolerance)
                    {
                        return Connection.BR;
                    }
                
                    else
                    {
                        return Connection.T;
                    }
                }

                else if (_lines[j].StartPoint.DistanceTo(_lines[i]) < _tolerance || _lines[j].EndPoint.DistanceTo(_lines[i]) < _tolerance)
                {
                    if (_lines[i].StartPoint.DistanceTo(_lines[j]) < _tolerance || _lines[i].EndPoint.DistanceTo(_lines[j]) < _tolerance)
                    {
                        return Connection.BR;
                    }

                    else
                    {
                        return Connection.T;
                    }
                }
                else
                {
                    return Connection.PT;
                }
            }
            else
            {
                return Connection.Invalid;
            }
        }

        
        /// <summary>
        /// Determine valid normals for this member, in regards to a particular connection
        /// </summary>
        /// <param name="connection"></param>
        /// <param name="memberIndex"></param>
        /// <returns></returns>
        internal List<Triple> GetValidNormalsForMemberForConnection(hConnection connection, int memberIndex)
        {
            int otherMemberIndex = connection.GetOtherIndex(memberIndex);
            
            if (connection.type == Connection.FTF)
            {
                Triple otherNormal = _members[otherMemberIndex].WebNormal;
                Triple reverseOtherNormal = otherNormal.Reverse();

                //Dispose
                /*{
                    otherNormal.Dispose();
                }*/

                return new List<Triple> { reverseOtherNormal };
            }

            else if (connection.type == Connection.BR)
            {
                Triple jointPlaneNormal = _members[memberIndex].WebAxis.ToTriple().Cross(_members[otherMemberIndex].WebAxis.ToTriple());
                Triple memberVector = Triple.ByTwoPoints(_members[memberIndex].WebAxis.StartPoint, _members[memberIndex].WebAxis.EndPoint);

                Triple webNormal1 = jointPlaneNormal.Cross(memberVector);
                return new List<Triple> { webNormal1, webNormal1.Reverse() };

                //return new List<Triple> { GetBRNormal(connection, memberIndex) };
            }

            else if (connection.type == Connection.Invalid)
            {
                return new List<Triple> { };
            }

            else
            {
                Triple jointNormal = _members[memberIndex].WebAxis.ToTriple().Cross(_members[otherMemberIndex].WebAxis.ToTriple());
                Triple memberVector = Triple.ByTwoPoints(_members[memberIndex].WebAxis.StartPoint, _members[memberIndex].WebAxis.EndPoint);

                Triple webNormal1 = jointNormal.Cross(memberVector);

                // Dispose
                /*{
                    jointPlane.Dispose();
                    memberVector.Dispose();
                }*/

                return new List<Triple> { webNormal1, webNormal1.Reverse() };
            }

        }


        /// <summary>
        /// Determine valid normals for this member, in regards to all associated connections
        /// </summary>
        /// <param name="memberIndex"></param>
        /// <returns></returns>
        internal List<Triple> GetValidNormalsForMember(int memberIndex)
        {
            List<Triple> potentialVectors = GetValidNormalsForMemberForConnection(_members[memberIndex].Connections[0], memberIndex);
            //List < Geo.Vector > allVectors = GetValidNormalsForMemberForConnection(_members[memberIndex].connections[0], memberIndex);
            foreach (hConnection connection in _members[memberIndex].Connections)
            {
                List<Triple> checkVectors = GetValidNormalsForMemberForConnection(connection, memberIndex);
                List<Triple> newVectors = new List<Triple>();
                foreach (Triple pV in potentialVectors)
                {
                    foreach (Triple cV in checkVectors)
                    {
                        //allVectors.Add(cV);
                        if (pV.Normalized().Dot(cV.Normalized()) > 1 - _tolerance)
                        {
                            newVectors.Add(pV);
                        }
                    }
                }
                potentialVectors = newVectors;
            }
            //return allVectors;
            return potentialVectors;
        }

        #region Resolve BR Connections


        /// <summary>
        /// Update members involved in braced connections so that the connections actually happen
        /// </summary>
        internal void ResolveBRConnections()
        {
            var extensionLength = 0.6;

            foreach (hConnection connection in _connections)
            {
                if (connection.type == Connection.BR)
                {
                    // Get common and end points of lines
                    Triple common0;
                    Triple mem1End0;
                    Triple mem2End0;
                    Triple common1;
                    Triple mem1End1;
                    Triple mem2End1;

                    GetCommonAndEndPointsOfTwoLines(_members[connection.members[0]].WebAxis, _members[connection.members[1]].WebAxis, out common0, out mem1End0, out mem2End0);
                    GetCommonAndEndPointsOfTwoLines(_members[connection.members[1]].WebAxis, _members[connection.members[0]].WebAxis, out common1, out mem1End1, out mem2End1);

                    var common = new List<Triple> { common0, common1 };
                    var mem1End = new List<Triple> { mem1End0, mem1End1 };
                    var mem2End = new List<Triple> { mem2End0, mem2End1 };

                    for (int i = 0; i < 2; i++)
                    {
                        // Get indices of involved members
                        int index1 = connection.members[i];
                        int index2 = connection.members[(i + 1) % 2];

                        // Web axes as vectors
                        Triple vec1 = Triple.ByTwoPoints(common[i], mem1End[i]);
                        Triple vec2 = Triple.ByTwoPoints(common[i], mem2End[i]);

                        // Get angle between members
                        double angle = vec1.AngleWithVector(vec2) * (Math.PI / 180);

                        // Distance from DIMPLE to web (i.e. 0.5 * stud height)
                        double c1 = _StudHeight / 2.0;

                        // Compute extension to DIMPLE
                        double d1 = ((c1 / Math.Cos(angle)) + c1) / (Math.Tan(angle));
                        double d2 = ((c1 / Math.Cos(angle)) - c1) / (Math.Tan(angle));
                        Triple connectionPlaneNormal = vec1.Cross(vec2);

                        // Determine orientation of members to each other and adjust d accordingly
                        bool b1 = _members[index1].WebNormal.Dot(_members[index2].WebNormal) < 0; // They are (both facing in) || (both facing out)
                        bool b2 = _members[index2].WebNormal.Dot(vec1) > 0; // Other member is facing this member
                        bool b3 = vec1.Dot(vec2) < 0; // Angle is obtuse
                        bool b4 = ((b1 && !b3) || (!b1 && b3));
                        bool webOut = connectionPlaneNormal.Dot(vec1.Cross(_members[index1].WebNormal)) > 0;

                        double d = (b4) ? d1 : d2;

                        if (b4)
                        {
                            if (webOut)
                            {
                                d *= -1;
                            }
                        }
                        else
                        {
                            if (b2)
                            {
                                d *= -1;
                            }
                        }
                        
                        
                        // Compute translation vector for end point in question
                        Triple moveVector = vec1.Reverse();

                        // Get new end point
                        moveVector = moveVector.Normalized();
                        moveVector = moveVector.Scale(d + extensionLength);
                        Triple newEndPoint = common[i].Add(moveVector);

                        d = Math.Abs(d);

                        // Determine if we are at the start or end of the member
                        bool atMemberStart = SamePoints(common[i], _members[index1].WebAxis.StartPoint);
                        
                        // Extend member
                        if (atMemberStart) { _members[index1].SetWebAxisStartPoint(newEndPoint); }
                        else { _members[index1].SetWebAxisEndPoint(newEndPoint); }

                        // Add connection dimple and end truss
                        double l = _members[index1].WebAxis.Length;
                        _members[index1].AddOperationByLocationType((atMemberStart) ? 0.0 : l - 0.0, "END_TRUSS");
                        _members[index1].AddOperationByLocationType((atMemberStart) ? extensionLength : l - extensionLength, "DIMPLE");

                        // Determine interior and exterior operations for pass through of other member
                        string interiorOp = (webOut) ? "LIP_CUT" : "NOTCH";
                        string exteriorOp = (webOut) ? "NOTCH" : "LIP_CUT";

                        // Add exterior operation
                        //_members[index1].AddOperationByLocationType((atMemberStart) ? 0.75 : l - 0.75, exteriorOp);

                        // Add interior operations
                        double iLoc = 0.75;
                        while (iLoc < (Math.Abs(d1) + 0.25))
                        {
                            _members[index1].AddOperationByLocationType((atMemberStart) ? iLoc : l - iLoc, interiorOp);
                            iLoc += 1.25;
                        }

                        // Add final interior operation
                        _members[index1].AddOperationByLocationType((atMemberStart) ? (0.5 + Math.Abs(d1)) : l - (0.5 + Math.Abs(d1)), interiorOp);
                        
                    }
                }
            }
        }


        /// <summary>
        /// Determine the point that is common between two line segments that share an end point
        /// </summary>
        /// <param name="line1"></param>
        /// <param name="line2"></param>
        /// <param name="common"></param>
        /// <param name="line1End"></param>
        /// <param name="line2End"></param>
        internal void GetCommonAndEndPointsOfTwoLines(Line line1, Line line2, out Triple common, out Triple line1End, out Triple line2End)
        {
            if (SamePoints(line1.StartPoint, line2.StartPoint))
            {
                common = line1.StartPoint;
                line1End = line1.EndPoint;
                line2End = line2.EndPoint;
            }
            else if (SamePoints(line1.StartPoint, line2.EndPoint))
            {
                common = line1.StartPoint;
                line1End = line1.EndPoint;
                line2End = line2.StartPoint;
            }
            else if (SamePoints(line1.EndPoint, line2.StartPoint))
            {
                common = line1.EndPoint;
                line1End = line1.StartPoint;
                line2End = line2.EndPoint;
            }
            else
            {
                common = line1.EndPoint;
                line1End = line1.StartPoint;
                line2End = line2.StartPoint;
            }
        }


        /// <summary>
        /// Generates brace hMembers for every braced connection (BR) in the structure
        /// </summary>
        /// <param name="braceLength"></param>
        internal void GenerateBraces()
        {
            foreach (hConnection connection in _connections)
            {
                if (connection.type == Connection.BR)
                {
                    // Adjust brace lengtsh to account for dimple offset
                    double dOffset = 0.45;
                    double b1 = _braceLength2 - (2 * dOffset);
                    double b2 = _braceLength1 - (2 * dOffset);

                    // Get indices for members involved in connetion
                    int mem1 = connection.members[0];
                    int mem2 = connection.members[1];

                    // Get common point and opposite end points
                    Triple common;
                    Triple mem1End;
                    Triple mem2End;
                    GetCommonAndEndPointsOfTwoLines(_members[mem1].WebAxis, _members[mem2].WebAxis, out common, out mem1End, out mem2End);


                    Triple v1 = Triple.ByTwoPoints(common, mem1End).Normalized();
                    Triple v2 = Triple.ByTwoPoints(common, mem2End).Normalized();
                    Triple bisector = v1.Add(v2);
                    bisector = bisector.Normalized();


                    // Determine orientation of members to each other
                    Triple connectionPlaneNormal = v1.Cross(v2);
                    bool webOut = connectionPlaneNormal.Dot(v1.Cross(_members[mem1].WebNormal)) > 0;

                    double a = v1.AngleWithVector(v2) / 2.0 * (Math.PI / 180);

                    double d = (_StudHeight / 2.0) / Math.Cos((Math.PI / 2) - a);
                    double d2 = (Math.Sin(Math.PI - (a + Math.Asin((Math.Sin(a) * b1) / b2))) * b2) / Math.Sin(a);
                    double memberOffset = (_StudHeight / 2.0) / Math.Tan((2 * a) - (Math.PI / 2));
                    double interiorBraceStartPointOffset = (webOut) ? dOffset - d : dOffset + d;
                    double interiorBraceEndPointOffset = _braceLength2 - interiorBraceStartPointOffset;

                    if (!_threePieceBrace)
                    {
                        double commonOffset = (webOut) ? 0 - d : 0 + d;
                        Triple elbowDimplePoint = common.Add( (bisector.Normalized().Scale(commonOffset)).Reverse() );

                        double armsOffset = (b2 / 2) / Math.Sin(a);

                        Triple dimplePoint1 = elbowDimplePoint.Add(v1.Normalized().Scale(armsOffset));
                        _members[mem1].AddOperationByPointType(dimplePoint1, "DIMPLE");
                        _members[mem1].AddOperationByPointType(dimplePoint1, "SWAGE");

                        Triple dimplePoint2 = elbowDimplePoint.Add(v2.Normalized().Scale(armsOffset));
                        _members[mem2].AddOperationByPointType(dimplePoint2, "DIMPLE");
                        _members[mem2].AddOperationByPointType(dimplePoint2, "SWAGE");

                        Triple braceVector = Triple.ByTwoPoints(dimplePoint1, dimplePoint2);
                        Triple braceStart = dimplePoint1.Add(FlipVector(braceVector).Normalized().Scale(dOffset)).Add(bisector.Normalized().Scale(_StudHeight/2.0));
                        Triple braceEnd = dimplePoint2.Add(braceVector.Normalized().Scale(dOffset)).Add(bisector.Normalized().Scale(_StudHeight / 2.0));
                        Line braceAxis = new Line(braceStart, braceEnd);
                        hMember brace = new hMember(braceAxis, FlipVector(bisector), "BR");
                        brace.AddOperationByLocationType(0, "END_TRUSS");
                        brace.AddOperationByLocationType(dOffset, "DIMPLE");
                        brace.AddOperationByLocationType(braceAxis.Length, "END_TRUSS");
                        brace.AddOperationByLocationType(braceAxis.Length - dOffset, "DIMPLE");
                        _braceMembers.Add(brace);
                    }

                    if (_threePieceBrace)
                    {
                        // Create interior brace
                        Triple interiorBraceNormal = new Triple(bisector.X, bisector.Y, bisector.Z);
                        interiorBraceNormal = interiorBraceNormal.Rotate(v1.Cross(v2), 90).Normalized().Scale(0.75);
                        Triple interiorBraceStart = common.Add(FlipVector(bisector.Normalized().Scale(interiorBraceStartPointOffset))).Add(interiorBraceNormal.Normalized().Scale(-_StudHeight / 2));
                        Triple interiorBraceEnd = common.Add(bisector.Normalized().Scale(interiorBraceEndPointOffset)).Add(interiorBraceNormal.Normalized().Scale(-_StudHeight / 2));
                        Line interiorBraceAxis = new Line(interiorBraceStart, interiorBraceEnd);
                        hMember interiorBrace = new hMember(interiorBraceAxis, interiorBraceNormal, "BR2");
                        interiorBrace.AddOperationByLocationType(0, "END_TRUSS");
                        interiorBrace.AddOperationByLocationType(dOffset, "DIMPLE");
                        interiorBrace.AddOperationByLocationType(dOffset, "SWAGE");
                        interiorBrace.AddOperationByLocationType(interiorBraceAxis.Length, "END_TRUSS");
                        interiorBrace.AddOperationByLocationType(interiorBraceAxis.Length - dOffset, "DIMPLE");
                        interiorBrace.AddOperationByLocationType(interiorBraceAxis.Length - dOffset, "SWAGE");

                        // Get some helpful points
                        Triple conDimplePoint = common.Add(bisector.Normalized().Scale((webOut) ? d : -d));
                        Triple braceDimplePoint = interiorBraceEnd.Add(Triple.ByTwoPoints(interiorBraceEnd, interiorBraceStart).Normalized().Scale(dOffset)).Add(interiorBraceNormal.Normalized().Scale(_StudHeight / 2));


                        // Create exterior brace 1
                        Triple exBrace1Dimple = conDimplePoint.Add(v1.Normalized().Scale(d2));
                        Triple exBrace1Vec = Triple.ByTwoPoints(exBrace1Dimple, braceDimplePoint).Normalized();
                        Triple exBrace1Normal = exBrace1Vec.Rotate(v2.Cross(v1), -90);
                        Triple exBrace1Start = exBrace1Dimple.Add(exBrace1Vec.Normalized().Scale(-dOffset)).Add(exBrace1Normal.Normalized().Scale(-_StudHeight / 2));
                        Triple exBrace1End = braceDimplePoint.Add(exBrace1Vec.Normalized().Scale(dOffset)).Add(exBrace1Normal.Normalized().Scale(-_StudHeight / 2));
                        Line exBrace1Axis = new Line(exBrace1Start, exBrace1End);
                        hMember exBrace1 = new hMember(exBrace1Axis, exBrace1Normal, "BR1");

                        exBrace1.AddOperationByLocationType(0, "END_TRUSS");
                        exBrace1.AddOperationByLocationType(dOffset, "DIMPLE");
                        exBrace1.AddOperationByLocationType(dOffset, "SWAGE");
                        exBrace1.AddOperationByLocationType(exBrace1Axis.Length, "END_TRUSS");
                        exBrace1.AddOperationByLocationType(exBrace1Axis.Length - dOffset, "DIMPLE");

                        exBrace1.AddOperationByLocationType(exBrace1Axis.Length - 0.5, "NOTCH");
                        exBrace1.AddOperationByLocationType(exBrace1Axis.Length - 0.75, "LIP_CUT");
                        exBrace1.AddOperationByLocationType(exBrace1Axis.Length - 1.5, "LIP_CUT");
                        exBrace1.AddOperationByLocationType(exBrace1Axis.Length - 2, "LIP_CUT");

                        _members[mem1].AddOperationByPointType(exBrace1Dimple, "DIMPLE");
                        //_members[mem1].AddOperationByPointType(exBrace1Dimple, "SWAGE");


                        // Create exterior brace 2
                        Triple exBrace2Dimple = conDimplePoint.Add(v2.Normalized().Scale(d2));
                        Triple exBrace2Vec = Triple.ByTwoPoints(exBrace2Dimple, braceDimplePoint).Normalized();
                        Triple exBrace2Normal = exBrace2Vec.Rotate(v1.Cross(v2), -90);
                        Triple exBrace2Start = exBrace2Dimple.Add(exBrace2Vec.Normalized().Scale(-dOffset)).Add(exBrace2Normal.Normalized().Scale(-_StudHeight / 2));
                        Triple exBrace2End = braceDimplePoint.Add(exBrace2Vec.Normalized().Scale(dOffset)).Add(exBrace2Normal.Normalized().Scale(-_StudHeight / 2));
                        Line exBrace2Axis = new Line(exBrace2Start, exBrace2End);
                        hMember exBrace2 = new hMember(exBrace2Axis, exBrace2Normal, "BR1");

                        exBrace2.AddOperationByLocationType(0, "END_TRUSS");
                        exBrace2.AddOperationByLocationType(dOffset, "DIMPLE");
                        exBrace2.AddOperationByLocationType(dOffset, "SWAGE");
                        exBrace2.AddOperationByLocationType(exBrace2Axis.Length, "END_TRUSS");
                        exBrace2.AddOperationByLocationType(exBrace2Axis.Length - dOffset, "DIMPLE");

                        exBrace2.AddOperationByLocationType(exBrace2Axis.Length - 0.5, "NOTCH");
                        exBrace2.AddOperationByLocationType(exBrace2Axis.Length - 0.75, "LIP_CUT");
                        exBrace2.AddOperationByLocationType(exBrace2Axis.Length - 1.5, "LIP_CUT");
                        exBrace2.AddOperationByLocationType(exBrace2Axis.Length - 2, "LIP_CUT");

                        _members[mem2].AddOperationByPointType(exBrace2Dimple, "DIMPLE");
                        //_members[mem2].AddOperationByPointType(exBrace2Dimple, "SWAGE");

                        _braceMembers.Add(interiorBrace);
                        _braceMembers.Add(exBrace1);
                        _braceMembers.Add(exBrace2);


                        // Add brace/member T operations
                        ResolveBraceT(exBrace1, mem1, exBrace1Dimple, webOut, a, conDimplePoint);
                        ResolveBraceT(exBrace2, mem2, exBrace2Dimple, webOut, a, conDimplePoint);
                        

                    }
                }
            }
        }

        #endregion


        #region Resolve FTF Connections


        /// <summary>
        /// Update members involved in face-to-face connections so that the connections actually happen
        /// </summary>
        internal void ResolveFTFConnections()
        {
            foreach (hConnection connection in _connections)
            {
                if (connection.type == Connection.FTF)
                {
                    for (int i = 0; i < 2; i++)
                    {
                        // Get indices of involved members
                        int index1 = connection.members[i];
                        int index2 = connection.members[(i + 1) % 2];

                        // Get involved web axes
                        Line axis1 = _members[index1].WebAxis;
                        Line axis2 = _members[index2].WebAxis;

                        // Get intersection point
                        Triple intersectionPoint = ClosestPointToOtherLine(axis1, axis2);

                        // Get angle between members
                        double angle = (Math.PI/180) * Triple.ByTwoPoints(axis1.StartPoint, axis1.EndPoint).AngleWithVector(Triple.ByTwoPoints(axis2.StartPoint, axis2.EndPoint));
                        angle = (angle < (Math.PI/2)) ? angle : Math.PI - angle;
                        
                        // Get distance from centerline of other member to edge of other member, along axis of current member (fun sentence)
                        double subtract = (angle % (Math.PI / 2) == 0) ? 0 : _WEBHoleSpacing / Math.Tan(angle);
                        double extendForAllWebHoles = _WEBHoleSpacing / Math.Sin(angle) - subtract + ((2 * _WEBHoleSpacing) / Math.Tan(angle)) + .25;
                        double fullOverlap = (angle % (Math.PI / 2) == 0) ? 0 : (_StudWdith/2) / Math.Tan(angle) + (_StudWdith / 2) / Math.Sin(angle);
                        double noOverhang = (angle % (Math.PI / 2) == 0 || Math.Abs(angle - (Math.PI / 2)) < 0.01 || angle < 0.01) ? (_StudWdith / 2) : (_StudWdith / 2) / Math.Sin(angle) - (_StudWdith / 2) / Math.Tan(angle);

                        int type = 0;
                        if (this._extensionDict.ContainsKey(_members[index1]._label))
                        {
                            type = _extensionDict[_members[index1]._label];
                        }

                        double extension;
                        switch (type)
                        {
                            case 3:
                                extension = fullOverlap;
                                break;
                            case 2:
                                extension = extendForAllWebHoles;
                                break;
                            case 0:
                                extension = 0;
                                break;
                            case 1:
                            default:
                                extension = noOverhang;
                                break;
                        }


                        // Check start point
                        if (SamePoints(intersectionPoint, axis1.StartPoint) || intersectionPoint.DistanceTo(axis1.StartPoint) < extension)
                        {
                            // Extend
                            Triple moveVector = Triple.ByTwoPoints(axis1.EndPoint, axis1.StartPoint);
                            moveVector = moveVector.Normalized();
                            moveVector = moveVector.Scale(extension);
                            Triple newStartPoint = intersectionPoint.Add(moveVector);
                            _members[index1].SetWebAxisStartPoint(newStartPoint);
                        }

                        // Check end point
                        if (SamePoints(intersectionPoint, axis1.EndPoint) || intersectionPoint.DistanceTo(axis1.EndPoint) < extension)
                        {
                            // Extend
                            Triple moveVector = Triple.ByTwoPoints(axis1.StartPoint, axis1.EndPoint);
                            moveVector = moveVector.Normalized();
                            moveVector = moveVector.Scale(extension);
                            Triple newEndPoint = intersectionPoint.Add(moveVector);
                            _members[index1].SetWebAxisEndPoint(newEndPoint);
                        }
                        
                        // Compute intersection location and location of web holes for fasteners
                        double intersectionLoc = _members[index1].WebAxis.ParameterAtPoint(intersectionPoint) * _members[index1].WebAxis.Length;
                        double d1 = (angle % (Math.PI / 2) < _tolerance) ? ((_WEBHoleSpacing / Math.Cos(angle)) - _WEBHoleSpacing) / Math.Tan(angle) : ((_WEBHoleSpacing / Math.Cos(angle)) - _WEBHoleSpacing) / Math.Tan(angle);
                        double d2 = (angle % (Math.PI / 2) < _tolerance) ? (2 * _WEBHoleSpacing) / Math.Tan(angle) : (2 * _WEBHoleSpacing) / Math.Tan(angle);

                        // Add valid web holes
                        List<double> webHoleLocations = new List<double> { intersectionLoc - (d1 + d2), intersectionLoc - (d1), intersectionLoc + (d1), intersectionLoc + (d1 + d2) };
                        foreach (double loc in webHoleLocations)
                        {
                            if (loc >= 0 && loc <= _members[index1].WebAxis.Length)
                            {
                                _members[index1].AddOperationByLocationType(loc, "WEB");
                            }
                        }

                        // Add bolt hole for alignment
                        _members[index1].AddOperationByLocationType(intersectionLoc, "BOLT");
                        
                    }
                }
            }
        }

        #endregion


        #region Resolve T Connections


        /// <summary>
        /// Update members involved in T connections so that the connections actually happen
        /// </summary>
        internal void ResolveTConnections()
        {
            foreach (hConnection connection in _connections)
            {
                if (connection.type == Connection.T)
                {
                    // Get terminal and cross member axes
                    Line terminal;
                    Line cross;
                    int iTerminal;
                    int iCross;
                    GetTerminalAndCrossMember(connection, out terminal, out cross, out iTerminal, out iCross);

                    // Web axes as vectors
                    Triple vec1 = Triple.ByTwoPoints(terminal.StartPoint, terminal.EndPoint);
                    Triple vec2 = Triple.ByTwoPoints(cross.StartPoint, cross.EndPoint);

                    // Get angle between members
                    double angle = vec1.AngleWithVector(vec2) * (Math.PI / 180);
                    angle = (angle < (Math.PI/2.0)) ? angle : Math.PI - angle;

                    // Distance from DIMPLE to web (i.e. 0.5 * stud height)
                    double c1 = _StudHeight / 2.0;

                    // Compute extension to DIMPLE
                    double d1 = ((c1 / Math.Cos(angle)) + c1) / (Math.Tan(angle));
                    double d2 = ((c1 / Math.Cos(angle)) - c1) / (Math.Tan(angle));
                    bool b1 = _members[iCross].WebNormal.Dot(_members[iTerminal].WebNormal) < 0;
                    bool b2 = _members[iCross].WebNormal.Dot(vec1) > 0;
                    double d = (b1) ? d1 : d2;

                    // Determine orientation of members to each other and adjust d accordingly
                    if (b2)
                    {
                        d *= -1;
                    }

                    // Compute translation vector for end point in question
                    Triple moveVector = FlipVector(vec1);
                    moveVector = moveVector.Normalized();
                    moveVector = moveVector.Scale(d + 0.45);
                    Triple newEndPoint = terminal.StartPoint.Add(moveVector);
                    
                    // Extend member
                    bool atMemberStart = SamePoints(terminal.StartPoint, _members[iTerminal].WebAxis.StartPoint);
                    if (atMemberStart) { _members[iTerminal].SetWebAxisStartPoint(newEndPoint); }
                    else { _members[iTerminal].SetWebAxisEndPoint(newEndPoint); }
                    
                    // Add operations to terminal member of T joint
                    double l = _members[iTerminal].WebAxis.Length;
                    _members[iTerminal].AddOperationByLocationType((atMemberStart) ? 0.0 : l - 0.0, "END_TRUSS");
                    _members[iTerminal].AddOperationByLocationType((atMemberStart) ? 0.45 : l - 0.45, "DIMPLE");
                    _members[iTerminal].AddOperationByLocationType((atMemberStart) ? 0.75 : l - 0.75, "SWAGE");

                    // Find the dimple point and its location on the cross member of the T joint
                    Triple dimplePoint = _members[iTerminal].WebAxis.PointAtParameter(((atMemberStart) ? 0.45 : l - 0.45) / l);
                    double crossIntLoc = _members[iCross].WebAxis.ParameterAtPoint(dimplePoint) * _members[iCross].WebAxis.Length;
                    dimplePoint = dimplePoint.Add(_members[iTerminal].WebNormal.Normalized().Scale(0.75));
                    double crossDimpleLoc = _members[iCross].WebAxis.ParameterAtPoint(dimplePoint) * _members[iCross].WebAxis.Length;

                    // Add Dimple to cross member
                    _members[iCross].AddOperationByLocationType(crossDimpleLoc, "DIMPLE");
                    
                    // Compute range offsets for LIP_CUT or NOTCH so that terminal member can insert into cross member
                    double x1 = (c1 / Math.Sin(angle)) - (c1 / Math.Tan(angle));
                    double x2 = (c1 / Math.Sin(angle)) + (c1 / Math.Tan(angle));

                    // Entering through Lip or Web?
                    string op = (b2) ? "LIP_CUT" : "NOTCH";

                    // Compute range for insertion operation
                    double clearance = 0.25;
                    bool pos = Triple.ByTwoPoints(_members[iCross].WebAxis.StartPoint, _members[iCross].WebAxis.EndPoint).Dot(vec1) > 0;
                    double start = (!pos) ? crossDimpleLoc + x1 + clearance: crossDimpleLoc - x1 - clearance;
                    double end = (pos) ? crossDimpleLoc + x2 + clearance : crossDimpleLoc - x2 - clearance;
                    var range = new List<double> { start, end };
                    start = range.Min();
                    end = range.Max();
                    
                    // Add insertion operations to cross member
                    double loc = start + 0.75;
                    while (loc < end - 0.75)
                    {
                        _members[iCross].AddOperationByLocationType(loc, op);
                        loc += 1.25;
                    }
                    _members[iCross].AddOperationByLocationType(end - 0.75, op);
                    
                }
            }
        }


        internal void ResolveBraceT(hMember brace, int memberIndex, Triple dimplePoint, bool webOut, double BRAngle, Triple connectionDimple)
        {
            // Get terminal and cross member axes
            Line terminal = brace.WebAxis;
            Line cross = _members[memberIndex].WebAxis;
            int iCross = memberIndex;

            // Web axes as vectors
            Triple vec1 = Triple.ByTwoPoints(terminal.StartPoint, terminal.EndPoint);
            Triple vec2 = Triple.ByTwoPoints(cross.StartPoint, cross.EndPoint);

            // Get angle between members
            double angle = vec1.AngleWithVector(vec2) * (Math.PI / 180);
            angle = (angle < (Math.PI / 2.0)) ? angle : Math.PI - angle;

            // Distance from DIMPLE to web (i.e. 0.5 * stud height)
            double c1 = _StudHeight / 2.0;

            // Compute extension to DIMPLE
            double d1 = ((c1 / Math.Cos(angle)) + c1) / (Math.Tan(angle));
            double d2 = ((c1 / Math.Cos(angle)) - c1) / (Math.Tan(angle));
            bool b1 = _members[iCross].WebNormal.Dot(brace.WebNormal) < 0;
            bool b2 = _members[iCross].WebNormal.Dot(vec1) > 0;
            double d = (b1) ? d1 : d2;

            // Determine orientation of members to each other and adjust d accordingly
            if (b2)
            {
                d *= -1;
            }

            // Compute translation vector for end point in question
            Triple moveVector = FlipVector(vec1);
            moveVector = moveVector.Normalized();
            moveVector = moveVector.Scale(d + 0.45);
            Triple newEndPoint = terminal.StartPoint.Add(moveVector);
            

            // Find the dimple point and its location on the cross member of the T joint
            double crossIntLoc = _members[iCross].WebAxis.ParameterAtPoint(dimplePoint) * _members[iCross].WebAxis.Length;
            //dimplePoint = dimplePoint.Add(brace._webNormal.Normalized().Scale(0.75));
            double crossDimpleLoc = _members[iCross].WebAxis.ParameterAtPoint(dimplePoint) * _members[iCross].WebAxis.Length;

            // Add Dimple to cross member
            //_members[iCross].AddOperationByLocationType(crossDimpleLoc, "DIMPLE");

            // Compute range offsets for LIP_CUT or NOTCH so that terminal member can insert into cross member
            double x1 = (c1 / Math.Sin(angle)) - (c1 / Math.Tan(angle));
            double x2 = (c1 / Math.Sin(angle)) + (c1 / Math.Tan(angle));

            // Entering through Lip or Web?
            string op = (b2) ? "LIP_CUT" : "NOTCH";

            // Compute range for insertion operation
            double clearance = 0.25;
            bool pos = Triple.ByTwoPoints(_members[iCross].WebAxis.StartPoint, _members[iCross].WebAxis.EndPoint).Dot(vec1) > 0;
            double start = (!pos) ? crossDimpleLoc + x1 + clearance : crossDimpleLoc - x1 - clearance;
            double end = (pos) ? crossDimpleLoc + x2 + clearance : crossDimpleLoc - x2 - clearance;
            var range = new List<double> { start, end };
            start = range.Min();
            end = range.Max();

            // Add insertion operations to cross member
            double loc = start + 0.75;
            while (loc < end - 0.75)
            {
                _members[iCross].AddOperationByLocationType(loc, op);
                loc += 1.25;
            }
            _members[iCross].AddOperationByLocationType(end - 0.75, op);

            double off = (0.75 / Math.Sin(BRAngle)) + (0.75 / Math.Tan(BRAngle)) - 0.5;
            Triple conToBraceDimple = Triple.ByTwoPoints(connectionDimple, dimplePoint).Normalized().Scale(off);
            Triple extraOpPt = connectionDimple.Add(conToBraceDimple);


            bool atMemberStart = SamePoints(terminal.StartPoint, brace.WebAxis.StartPoint);
            string extraOp = (webOut) ? "LIP_CUT" : "NOTCH";
            // double offset = (webOut) ? 1.25 : 0.75;
            // double location = (atMemberStart) ? offset : _members[memberIndex].WebAxis.Length - offset;
            _members[memberIndex].AddOperationByPointType(extraOpPt, extraOp);
        }




        /// <summary>
        /// Determine which member terminates at the T and which does not
        /// </summary>
        /// <param name="con"></param>
        /// <param name="terminal"></param>
        /// <param name="cross"></param>
        /// <param name="iTerminal"></param>
        /// <param name="iCross"></param>
        internal void GetTerminalAndCrossMember(hConnection con, out Line terminal, out Line cross, out int iTerminal, out int iCross)
        {
            // Initialize all variables. These initial values should never be returned.. If my assumptions are correct
            terminal = null;
            cross = null;
            iTerminal = -1;
            iCross = -1;

            foreach(int mem in con.members)
            {
                var pts = new List<Triple> { _members[mem].WebAxis.StartPoint, _members[mem].WebAxis.EndPoint };

                for (int i = 0; i < 2; i++)
                {
                    if (pts[i].DistanceTo(_members[con.members[(con.members.IndexOf(mem) + 1) % 2]].WebAxis) < _tolerance)
                    {
                        terminal = new Line(pts[i], pts[(i + 1) % 2]);
                        cross = new Line(_members[con.members[(con.members.IndexOf(mem) + 1) % 2]].WebAxis.StartPoint, _members[con.members[(con.members.IndexOf(mem) + 1) % 2]].WebAxis.EndPoint);
                        iTerminal = mem;
                        iCross = con.members[(con.members.IndexOf(mem) + 1) % 2];
                        return;
                    }
                }
            }
        }

        #endregion


        #region Resolve PT Connections


        /// <summary>
        /// Update members involved in T connections so that the connections actually happen
        /// </summary>
        internal void ResolvePTConnections()
        {
            foreach (hConnection connection in _connections)
            {
                if (connection.type == Connection.PT)
                {
                    // Get terminal and cross member axes
                    Line inside;
                    Line outside;
                    int iInside;
                    int iOutisde;
                    GetInsideAndOutsideMember(connection, out inside, out outside, out iInside, out iOutisde);

                    Triple intersectionPoint = ClosestPointToOtherLine(inside, outside);

                    // Web axes as vectors
                    Triple vec1 = Triple.ByTwoPoints(inside.StartPoint, inside.EndPoint);
                    Triple vec2 = Triple.ByTwoPoints(outside.StartPoint, outside.EndPoint);

                    // Get angle between members
                    double angle = vec1.AngleWithVector(vec2) * (Math.PI / 180);
                    angle = (angle < (Math.PI / 2.0)) ? angle : Math.PI - angle;

                    // Distance from DIMPLE to web (i.e. 0.5 * stud height)
                    double c1 = _StudHeight / 2.0;

                    // Compute extension to DIMPLE
                    double d1 = ((c1 / Math.Cos(angle)) + c1) / (Math.Tan(angle));
                    double d2 = ((c1 / Math.Cos(angle)) - c1) / (Math.Tan(angle));
                    double x1 = (c1 / Math.Sin(angle)) - (c1 / Math.Tan(angle));
                    double x2 = (c1 / Math.Sin(angle)) + (c1 / Math.Tan(angle));
                    bool b1 = _members[iOutisde].WebNormal.Dot(_members[iInside].WebNormal) < 0;
                    bool b2 = _members[iOutisde].WebNormal.Dot(vec1) > 0;
                    double d = (b1) ? x2 : x1;

                    // Determine orientation of members to each other and adjust d accordingly
                    if (!b2)
                    {
                        d *= -1;
                    }
                    intersectionPoint = intersectionPoint.Add(vec1.Normalized().Scale(d));

                    // Add operations to terminal member of T joint
                    double l = _members[iInside].WebAxis.Length;
                    _members[iInside].AddOperationByPointType(intersectionPoint, "DIMPLE");
                    _members[iInside].AddOperationByPointType(intersectionPoint, "SWAGE");

                    // Find the dimple point and its location on the cross member of the T joint
                    Triple dimplePoint = intersectionPoint;
                    double crossIntLoc = _members[iOutisde].WebAxis.ParameterAtPoint(dimplePoint) * _members[iOutisde].WebAxis.Length;
                    dimplePoint = dimplePoint.Add(_members[iInside].WebNormal.Normalized().Scale(0.75));
                    double crossDimpleLoc = _members[iOutisde].WebAxis.ParameterAtPoint(dimplePoint) * _members[iOutisde].WebAxis.Length;

                    // Add Dimple to cross member
                    _members[iOutisde].AddOperationByLocationType(crossDimpleLoc, "DIMPLE");

                    // Compute range offsets for LIP_CUT or NOTCH so that terminal member can insert into cross member
                    

                    // Entering through Lip or Web?
                    string op = (b2) ? "LIP_CUT" : "NOTCH";
                    string op2 = (b2) ? "NOTCH" : "LIP_CUT";

                    // Compute range for insertion operation
                    double clearance = 0.25;
                    bool pos = Triple.ByTwoPoints(_members[iOutisde].WebAxis.StartPoint, _members[iOutisde].WebAxis.EndPoint).Dot(vec1) > 0;
                    double start = (!pos) ? crossDimpleLoc + x1 + clearance : crossDimpleLoc - x1 - clearance;
                    double start2 = (pos) ? crossDimpleLoc + x1 + clearance : crossDimpleLoc - x1 - clearance;
                    double end = (pos) ? crossDimpleLoc + x2 + clearance : crossDimpleLoc - x2 - clearance;
                    double end2 = (!pos) ? crossDimpleLoc + x2 + clearance : crossDimpleLoc - x2 - clearance;
                    var range = new List<double> { start, end };
                    start = range.Min();
                    end = range.Max();
                    var range2 = new List<double> { start2, end2 };
                    start2 = range2.Min();
                    end2 = range2.Max();

                    // Add insertion operations to cross member
                    double loc = start + 0.75;
                    double loc2 = start2 + 0.75;
                    while (loc < end - 0.75)
                    {
                        _members[iOutisde].AddOperationByLocationType(loc, op);
                        loc += 1.25;
                    }
                    _members[iOutisde].AddOperationByLocationType(end - 0.75, op);

                    while (loc2 < end2 - 0.75)
                    {
                        _members[iOutisde].AddOperationByLocationType(loc2, op2);
                        loc2 += 1.25;
                    }
                    _members[iOutisde].AddOperationByLocationType(end2 - 0.75, op2);
                }
            }
        }

        internal void GetInsideAndOutsideMember(hConnection con, out Line inside, out Line outside, out int iInside, out int iOutside)
        {
            string label1 = _members[con.members[0]]._label;
            string label2 = _members[con.members[1]]._label;

            int priority1 = (_priorityDict.ContainsKey(_members[con.members[0]]._label)) ? _priorityDict[_members[con.members[0]]._label] : 0;
            int priority2 = (_priorityDict.ContainsKey(_members[con.members[1]]._label)) ? _priorityDict[_members[con.members[1]]._label] : 0;

            bool yes = false;
            if (_priorityDict.ContainsKey(_members[con.members[0]]._label) && _priorityDict.ContainsKey(_members[con.members[1]]._label)){
                yes = _priorityDict[_members[con.members[0]]._label] > _priorityDict[_members[con.members[1]]._label];
            }

            if (yes)
            {
                inside = _members[con.members[0]].WebAxis;
                outside = _members[con.members[1]].WebAxis;
                iInside = con.members[0];
                iOutside = con.members[1];
            }
            else
            {
                inside = _members[con.members[1]].WebAxis;
                outside = _members[con.members[0]].WebAxis;
                iInside = con.members[1];
                iOutside = con.members[0];
            }
            
        }

        #endregion


        //  ██╗   ██╗████████╗██╗██╗     
        //  ██║   ██║╚══██╔══╝██║██║     
        //  ██║   ██║   ██║   ██║██║     
        //  ██║   ██║   ██║   ██║██║     
        //  ╚██████╔╝   ██║   ██║███████╗
        //   ╚═════╝    ╚═╝   ╚═╝╚══════╝
        //                               
        

        /// <summary>
        /// Get the normal for a member involeved in a BR connection
        /// </summary>
        /// <param name="connection"></param>
        /// <param name="memberIndex"></param>
        /// <returns></returns>
        /*internal Triple GetBRNormal(hConnection connection, int memberIndex)
        {
            Line memberAxis = _members[memberIndex]._webAxis;
            Line otherMemberAxis = _members[connection.GetOtherIndex(memberIndex)]._webAxis;

            Triple memberVector = Triple.ByTwoPoints(memberAxis.StartPoint, memberAxis.EndPoint);
            Triple otherMemberVector = null;

            Triple otherMemberNormal = _members[connection.GetOtherIndex(memberIndex)]._webNormal;

            Triple center = (Geo.Point)memberAxis.Intersect(otherMemberAxis)[0];
            
            // Flip other member vector if needed
            if ( SamePoints(memberAxis.StartPoint, otherMemberAxis.StartPoint) || SamePoints(memberAxis.EndPoint, otherMemberAxis.EndPoint) )
            {
                otherMemberVector = Geo.Vector.ByTwoPoints(otherMemberAxis.StartPoint, otherMemberAxis.EndPoint);
            }

            else
            {
                otherMemberVector = Geo.Vector.ByTwoPoints(otherMemberAxis.EndPoint, otherMemberAxis.StartPoint);
            }

            Geo.Vector xAxis = memberVector.Normalized().Add(otherMemberVector.Normalized());
            Geo.Vector yAxis = memberVector.Cross(otherMemberVector);
            Geo.Plane bisectingPlane = Geo.Plane.ByOriginXAxisYAxis(Geo.Point.ByCoordinates(0,0,0), xAxis, yAxis);

            Geo.Point normalP = (Geo.Point)otherMemberNormal.AsPoint().Mirror(bisectingPlane);

            Geo.Vector normal = Geo.Vector.ByCoordinates(normalP.X, normalP.Y, normalP.Z);

            return normal;
        }*/


        /// <summary>
        /// Returns a new vector which is the input vector reversed
        /// </summary>
        /// <param name="vec"></param>
        /// <returns>reversed vector</returns>
        internal Triple FlipVector(Triple vec)
        {
            return new Triple(vec.X * -1, vec.Y * -1, vec.Z * -1);
        }


        /// <summary>
        /// Determines whether two planes are parallel by comparing normals
        /// </summary>
        /// <param name="vec1"></param>
        /// <param name="vec2"></param>
        /// <returns></returns>
        internal bool ParallelPlaneNormals(Triple vec1, Triple vec2)
        {
            double similarity = vec1.Dot(vec2);

            return (Math.Abs(similarity) > 1 - _tolerance);
        }


        /// <summary>
        /// Determines whether two planes are perpendicular by comparing normals
        /// </summary>
        /// <param name="vec1"></param>
        /// <param name="vec2"></param>
        /// <returns></returns>
        internal bool PerpendicularPlaneNormals(Triple vec1, Triple vec2)
        {
            double similarity = vec1.Dot(vec2);

            return (Math.Abs(similarity) < _tolerance);
        }


        /// <summary>
        /// Determine if two points are the same, with tolerance
        /// </summary>
        /// <param name="p1"></param>
        /// <param name="p2"></param>
        /// <returns></returns>
        internal bool SamePoints(Triple p1, Triple p2)
        {
            bool x = Math.Abs(p1.X - p2.X) < _tolerance;
            bool y = Math.Abs(p1.Y - p2.Y) < _tolerance;
            bool z = Math.Abs(p1.Z - p2.Z) < _tolerance;

            return x && y && z;
        }
        

        #region Line Segment Distances

        public static Triple ClosestPointToOtherLine(Line line, Line other)
        {
            Triple startPt1 = line.StartPoint;
            Triple startPt2 = other.StartPoint;

            Triple vec1 = Triple.ByTwoPoints(line.StartPoint, line.EndPoint);
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

        /// <summary>
        /// 
        /// </summary>
        /// <param name="line1"></param>
        /// <param name="line2"></param>
        /// <returns></returns>
        public static double LineToLineDistance(Line line1, Line line2)
        {
            var pt1 = line1.StartPoint;
            var pt2 = line1.EndPoint;
            var pt3 = line2.StartPoint;
            var pt4 = line2.EndPoint;

            var vec1 = Triple.ByTwoPoints(line1.StartPoint, line1.EndPoint);
            var vec2 = Triple.ByTwoPoints(line2.StartPoint, line2.EndPoint);

            double x1 = pt1.X;
            double y1 = pt1.Y;
            double z1 = pt1.Z;
            double x2 = pt3.X;
            double y2 = pt3.Y;
            double z2 = pt3.Z;

            double a1 = vec1.X;
            double b1 = vec1.Y;
            double c1 = vec1.Z;
            double a2 = vec2.X;
            double b2 = vec2.Y;
            double c2 = vec2.Z;

            double f = a1 * a2 + b1 * b2 + c1 * c2;
            f = (f == 0) ? 0.1 : f;

            double g = -(a1 * a1 + b1 * b1 + c1 * c1);
            double h = -(a1 * (x2 - x1) + b1 * (y2 - y1) + c1 * (z2 - z1));
            double i = (a2 * a2 + b2 * b2 + c2 * c2);
            double j = -1 * f;
            double k = -(a2 * (x2 - x1) + b2 * (y2 - y1) + c2 * (z2 - z1));

            

            double t = (k - (h * i / f)) / (j - (g * i / f));
            double s = (h - (k * f / i)) / (g - (j * f / i));

            if (t >= 0 && t <= 1 && s >= 0 && s <= 1)
            {
                double xp = x1 + (a1 * t);
                double yp = y1 + (b1 * t);
                double zp = z1 + (c1 * t);
                double xq = x2 + (a2 * s);
                double yq = y2 + (b2 * s);
                double zq = z2 + (c2 * s);

                double d9 = Math.Sqrt(Math.Pow((xq - xp), 2) + Math.Pow((yq - yp), 2) + Math.Pow((zq - zp), 2));
                return d9;
            }
            else
            {
                return -1;
            }
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="point"></param>
        /// <param name="line"></param>
        /// <returns></returns>
        public static double PointToLineDistance(Triple point, Line line)
        {
            var p = line.StartPoint;
            var v = Triple.ByTwoPoints(line.StartPoint, line.EndPoint);

            double xp = point.X;
            double yp = point.Y;
            double zp = point.Z;

            double x1 = p.X;
            double y1 = p.Y;
            double z1 = p.Z;

            double a1 = v.X;
            double b1 = v.Y;
            double c1 = v.Z;

            double t = (-1 * (a1 * (x1 - xp) + b1 * (y1 - yp) + c1 * (z1 - zp))) / (a1 * a1 + b1 * b1 + c1 * c1);

            if (t >= 0 && t <= 1)
            {
                double xq = x1 + (a1 * t);
                double yq = y1 + (b1 * t);
                double zq = z1 + (c1 * t);
                double d = Math.Sqrt(Math.Pow((xq - xp), 2) + Math.Pow((yq - yp), 2) + Math.Pow((zq - zp), 2));
                return d;
            }
            else
            {
                return -1;
            }
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="line1"></param>
        /// <param name="line2"></param>
        /// <returns></returns>
        public static double MinDistanceBetweenLines(Line line1, Line line2)
        {
            var distances = new List<double>();

            var pt1 = line1.StartPoint;
            var pt2 = line1.EndPoint;
            var pt3 = line2.StartPoint;
            var pt4 = line2.EndPoint;

            double d1 = PointToLineDistance(pt1, line2);
            if (d1 > 0) { distances.Add(d1); }

            double d2 = PointToLineDistance(pt2, line2);
            if (d2 > 0) { distances.Add(d2); }

            double d3 = PointToLineDistance(pt3, line1);
            if (d3 > 0) { distances.Add(d3); }

            double d4 = PointToLineDistance(pt4, line1);
            if (d4 > 0) { distances.Add(d4); }

            double d5 = pt1.DistanceTo(pt3);
            distances.Add(d5);

            double d6 = pt1.DistanceTo(pt4);
            distances.Add(d6);

            double d7 = pt2.DistanceTo(pt3);
            distances.Add(d7);

            double d8 = pt2.DistanceTo(pt4);
            distances.Add(d8);

            double d9 = LineToLineDistance(line1, line2);
            if (d9 > 0) { distances.Add(d9); }

            return distances.Min();
        }
        
        #endregion
    }
}
