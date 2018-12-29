using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Geo = Autodesk.DesignScript.Geometry;
using Autodesk.DesignScript.Interfaces;
using Autodesk.DesignScript.Runtime;

namespace HowickMaker
{
    /// <summary>
    /// A collection of connected hMembers, 
    /// with associated connections, braces, etc.
    /// </summary>
    public class hStructure
    {
        public hMember[] _members;
        public List<hConnection> _connections = new List<hConnection>();
        internal Graph _g;
        internal bool _advanced = false;
        internal double _tolerance = 0.001;
        internal double _planarityTolerance = 0.001;
        internal bool _firstConnectionIsFTF = false;
        internal bool _threePieceBrace;
        internal double _braceLength1;
        internal double _braceLength2;

        internal double _WEBHoleSpacing = (15.0 / 16);
        internal double _StudHeight = 1.5;
        internal double _StudWdith = 3.5;

        List<string> _labels;
        
        List<Line> _lines = new List<Line>();
        List<hMember> _braceMembers = new List<hMember>();

        Dictionary<string, Triple> _webNormalsDict;
        Dictionary<string, int> _priorityDict;
        Dictionary<string, int> _extensionDict;

        List<List<double>> _dots = new List<List<double>>();


        [IsVisibleInDynamoLibrary(false)]
        internal hStructure(List<Line> lines, double intersectionTolerance, double planarityTolerance, bool threePieceBrace, double braceLength1, double braceLength2, bool firstConnectionIsFTF)
        {
            _lines = lines;
            _tolerance = intersectionTolerance;
            _planarityTolerance = planarityTolerance;
            _threePieceBrace = threePieceBrace;
            _braceLength1 = braceLength1;
            _braceLength2 = braceLength2;
            _firstConnectionIsFTF = firstConnectionIsFTF;

            _members = new hMember[lines.Count];
            for (int i = 0; i < lines.Count; i++)
            {
                _members[i] = new hMember(lines[i], i.ToString());
                _members[i]._label = i.ToString();
            }
        }

        [IsVisibleInDynamoLibrary(false)]
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
            for (int i = 0; i < lines.Count; i++)
            {
                _members[i] = new hMember(lines[i], i.ToString());
                _members[i]._label = labels[i];
            }
        }





        //  ██████╗ ██╗   ██╗██████╗      ██████╗███╗   ██╗███████╗████████╗██████╗ 
        //  ██╔══██╗██║   ██║██╔══██╗    ██╔════╝████╗  ██║██╔════╝╚══██╔══╝██╔══██╗
        //  ██████╔╝██║   ██║██████╔╝    ██║     ██╔██╗ ██║███████╗   ██║   ██████╔╝
        //  ██╔═══╝ ██║   ██║██╔══██╗    ██║     ██║╚██╗██║╚════██║   ██║   ██╔══██╗
        //  ██║     ╚██████╔╝██████╔╝    ╚██████╗██║ ╚████║███████║   ██║   ██║  ██║
        //  ╚═╝      ╚═════╝ ╚═════╝      ╚═════╝╚═╝  ╚═══╝╚══════╝   ╚═╝   ╚═╝  ╚═╝
        //               

        [MultiReturn(new[] { "members", "braces", "connections" })]
        public static Dictionary<string, object> FromLines_Advanced(
            List<Geo.Line> lines,
            [DefaultArgument("[]")] List<string> names,
            [DefaultArgument("{}")] Dictionary<string, Geo.Vector> webNormalsDict,
            [DefaultArgument("{}")] Dictionary<string, int> priorityDict,
            [DefaultArgument("{}")] Dictionary<string, int> extensionDict,
            [DefaultArgument("null")] string options)
        {
            // Parse Options
            string[] option = options.ToString().Split(',');
            double intersectionTolerance = (options == "null") ? 0.001 : Double.Parse(option[0]);
            double planarityTolerance = (options == "null") ? 0.001 : Double.Parse(option[1]);
            bool generateBraces = (options == "null") ? false : bool.Parse(option[2]);
            bool threePieceBrace = (options == "null") ? false : bool.Parse(option[3]);
            double braceLength1 = (options == "null") ? 6 : Double.Parse(option[4]);
            double braceLength2 = (options == "null") ? 3 : Double.Parse(option[5]);
            bool firstConnectionIsFTF = (options == "null") ? false : bool.Parse(option[6]);

            // Create empty dictionaries if unprovided
            if (webNormalsDict == null) { webNormalsDict = new Dictionary<string, Geo.Vector>(); }
            if (priorityDict == null) { priorityDict = new Dictionary<string, int>(); }
            if (extensionDict == null) { extensionDict = new Dictionary<string, int>(); }
            names = CompleteListOfNames(names, lines.Count);

            // Convert Lines
            var hLines = new List<Line>();
            foreach (Geo.Line l in lines)
            {
                hLines.Add(new Line( new Triple(l.StartPoint.X, l.StartPoint.Y, l.StartPoint.Z), new Triple(l.EndPoint.X, l.EndPoint.Y, l.EndPoint.Z)));
            }

            // Convert webNormals
            var hWebNormalsDict = new Dictionary<string, Triple>();
            foreach (string vectorName in webNormalsDict.Keys)
            {
                var vector = webNormalsDict[vectorName];
                hWebNormalsDict[vectorName] = new Triple(vector.X, vector.Y, vector.Z);
            }

            hStructure structure = StructureFromLines_Advanced(
                hLines,
                names, 
                hWebNormalsDict, 
                priorityDict, 
                extensionDict, 
                intersectionTolerance, 
                planarityTolerance,
                generateBraces, 
                threePieceBrace, 
                braceLength1, 
                braceLength2, 
                firstConnectionIsFTF);

            return new Dictionary<string, object>
            {
                { "members", structure._members.ToList() },
                { "braces", structure._braceMembers.ToList() },
                { "connections", structure._connections }
            };
        }
        

        /// <summary>
        /// Adds member names if any are missing
        /// </summary>
        /// <param name="names"></param>
        /// <param name="numMembers"></param>
        /// <returns></returns>
        [IsVisibleInDynamoLibrary(false)]
        internal static List<string> CompleteListOfNames(List<string> names, int numMembers)
        {
            if (names.Count > numMembers)
            {
                throw new System.ArgumentException("List of names cannot be longer than list of member lines.", "names");
            }
            else if (names.Count == numMembers)
            {
                return names;
            }
            else
            {
                var fillNum = numMembers - names.Count;
                for (int i = 0; i < fillNum; i++)
                {
                    names.Add(i.ToString());
                }
                return names;
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
        internal static hStructure StructureFromLines_Advanced(
            List<Line> lines, 
            List<string> names, 
            Dictionary<string, Triple> webNormalsDict, 
            Dictionary<string, int> priorityDict, 
            Dictionary<string, int> extensionDict, 
            double intersectionTolerance, 
            double planarityTolerance, 
            bool generateBraces, 
            bool threePieceBraces, 
            double braceLength1, 
            double braceLength2, 
            bool firstConnectionIsFTF
            )
        {
            hStructure structure = new hStructure(lines, names, intersectionTolerance, planarityTolerance, threePieceBraces, braceLength1, braceLength2, firstConnectionIsFTF);
            structure._advanced = true;
            structure._labels = names;
            structure._webNormalsDict = webNormalsDict;
            structure._priorityDict = priorityDict;
            structure._extensionDict = extensionDict;
            

            structure._g = graphFromLines(lines, structure._tolerance);

            var startingNodes = structure._g.GetStartingNodes();
            for (int i = 0; i < lines.Count; i++)
            {
                structure._members[i]._label = names[i];
            }
            foreach (int start in startingNodes)
            {
                structure.BuildMembersAndConnectionsFromGraph(start);
            }
            
            for (int i = 0; i < lines.Count; i++)
            {
                structure._members[i]._label = names[i];
            }

            if (generateBraces)
            {
                structure.GenerateBraces();
            }

            structure.ResolveFTFConnections();
            structure.ResolveBRConnections();
            structure.ResolveTConnections();
            structure.ResolvePTConnections();

            for (int i = 0; i < lines.Count; i++)
            {
                structure._members[i]._label = names[i];
            }

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
            Geo.Plane currentPlane = ByTwoLines(currentLine, nextLine);
            Triple normal = currentPlane.Normal;

            var vectors = new List<Triple> { Triple.ByTwoPoints(currentLine.StartPoint, currentLine.EndPoint).Cross(normal), Triple.ByTwoPoints(currentLine.StartPoint, currentLine.EndPoint).Cross(normal).Reverse() };
            Triple choice = vectors[0];
            if (_webNormalsDict.ContainsKey(_members[start]._label))
            {
                var target = _webNormalsDict[_members[start]._label].Normalized();
                var vects = vectors.OrderBy(x => -1 * x.Normalized().Dot(target)).ToList();

                choice = vects[0];
            }

            currentMember._webNormal = (_firstConnectionIsFTF) ? normal : choice;

            _members[start] = currentMember;

            // Dispose
            /*{
                currentLine.Dispose();
                nextLine.Dispose();
                currentPlane.Dispose();
                normal.Dispose();
            }*/

            Propogate(start);
        }

        

        /// <summary>
        /// Recursive propogation across line network
        /// </summary>
        /// <param name="current"></param>
        /// <param name="lines"></param>
        internal void Propogate(int current)
        {
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
                    if (!(_g.vertices[i].neighbors.Count <= _members[i].connections.Count))
                    {
                        hConnection con = new hConnection(GetConnectionType(current, i), new List<int> { i, current });

                        if (!_connections.Contains(con))
                        {
                            _connections.Add(con);
                        }
                        
                        _members[i].connections.Add(con);
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
                            _members[i]._webNormal = choice;
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

            if (ParallelPlaneNormals(_members[i]._webNormal, jointPlaneNormal))
            {
                return Connection.FTF;
            }
            else if (PerpendicularPlaneNormals(_members[i]._webNormal, jointPlaneNormal))
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
                Triple otherNormal = _members[otherMemberIndex]._webNormal;
                Triple reverseOtherNormal = otherNormal.Reverse();

                //Dispose
                /*{
                    otherNormal.Dispose();
                }*/

                return new List<Triple> { reverseOtherNormal };
            }

            else if (connection.type == Connection.BR)
            {
                Triple jointPlaneNormal = _members[memberIndex]._webAxis.ToTriple().Cross(_members[otherMemberIndex]._webAxis.ToTriple());
                Triple memberVector = Triple.ByTwoPoints(_members[memberIndex]._webAxis.StartPoint, _members[memberIndex]._webAxis.EndPoint);

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
                Triple jointNormal = _members[memberIndex]._webAxis.ToTriple().Cross(_members[otherMemberIndex]._webAxis.ToTriple());
                Triple memberVector = Triple.ByTwoPoints(_members[memberIndex]._webAxis.StartPoint, _members[memberIndex]._webAxis.EndPoint);

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
            List<Triple> potentialVectors = GetValidNormalsForMemberForConnection(_members[memberIndex].connections[0], memberIndex);
            //List < Geo.Vector > allVectors = GetValidNormalsForMemberForConnection(_members[memberIndex].connections[0], memberIndex);
            foreach (hConnection connection in _members[memberIndex].connections)
            {
                List<Triple> checkVectors = GetValidNormalsForMemberForConnection(connection, memberIndex);
                List<Triple> newVectors = new List<Triple>();
                foreach (Triple pV in potentialVectors)
                {
                    foreach (Triple cV in checkVectors)
                    {
                        //allVectors.Add(cV);
                        if (pV.Dot(cV) > 1 - _tolerance)
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

                    GetCommonAndEndPointsOfTwoLines(_members[connection.members[0]]._webAxis, _members[connection.members[1]]._webAxis, out common0, out mem1End0, out mem2End0);
                    GetCommonAndEndPointsOfTwoLines(_members[connection.members[1]]._webAxis, _members[connection.members[0]]._webAxis, out common1, out mem1End1, out mem2End1);

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
                        double d = ((c1 / Math.Cos(angle)) + c1) / (Math.Tan(angle));
                        Triple connectionPlaneNormal = vec1.Cross(vec2);

                        // Determine orientation of members to each other and adjust d accordingly
                        bool webOut = connectionPlaneNormal.Dot(vec1.Cross(_members[index1]._webNormal)) > 0;
                        if (webOut)
                        {
                            d *= -1;
                        }
                        
                        // Compute translation vector for end point in question
                        Triple moveVector = vec1.Reverse();

                        // Get new end point
                        moveVector = moveVector.Normalized();
                        moveVector = moveVector.Scale(d + 0.75);
                        Triple newEndPoint = common[i].Add(moveVector);

                        d = Math.Abs(d);

                        // Determine if we are at the start or end of the member
                        bool atMemberStart = SamePoints(common[i], _members[index1]._webAxis.StartPoint);
                        
                        // Extend member
                        if (atMemberStart) { _members[index1].SetWebAxisStartPoint(newEndPoint); }
                        else { _members[index1].SetWebAxisEndPoint(newEndPoint); }

                        // Add connection dimple and end truss
                        double l = _members[index1]._webAxis.Length;
                        _members[index1].AddOperationByLocationType((atMemberStart) ? 0.0 : l - 0.0, "END_TRUSS");
                        _members[index1].AddOperationByLocationType((atMemberStart) ? 0.75 : l - 0.75, "DIMPLE");

                        // Determine interior and exterior operations for pass through of other member
                        string interiorOp = (webOut) ? "LIP_CUT" : "NOTCH";
                        string exteriorOp = (webOut) ? "NOTCH" : "LIP_CUT";

                        // Add exterior operation
                        _members[index1].AddOperationByLocationType((atMemberStart) ? 0.75 : l - 0.75, exteriorOp);

                        // Add interior operations
                        double iLoc = 0.75;
                        while (iLoc < (Math.Abs(d) + 0.25))
                        {
                            _members[index1].AddOperationByLocationType((atMemberStart) ? iLoc : l - iLoc, interiorOp);
                            iLoc += 1.25;
                        }

                        // Add final interior operation
                        _members[index1].AddOperationByLocationType((atMemberStart) ? (0.5 + Math.Abs(d)) : l - (0.5 + Math.Abs(d)), interiorOp);
                        
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
                    GetCommonAndEndPointsOfTwoLines(_members[mem1]._webAxis, _members[mem2]._webAxis, out common, out mem1End, out mem2End);


                    Triple v1 = Triple.ByTwoPoints(common, mem1End).Normalized();
                    Triple v2 = Triple.ByTwoPoints(common, mem2End).Normalized();
                    Triple bisector = v1.Add(v2);
                    bisector = bisector.Normalized();


                    // Determine orientation of members to each other
                    Triple connectionPlaneNormal = v1.Cross(v2);
                    bool webOut = connectionPlaneNormal.Dot(v1.Cross(_members[mem1]._webNormal)) > 0;

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
                        Geo.Vector interiorBraceNormal = Geo.Vector.ByCoordinates(bisector.X, bisector.Y, bisector.Z);
                        interiorBraceNormal = interiorBraceNormal.Rotate(v1.Cross(v2), 90).Normalized().Scale(0.75);
                        Geo.Point interiorBraceStart = common.Add(FlipVector(bisector.Normalized().Scale(interiorBraceStartPointOffset))).Add(interiorBraceNormal.Normalized().Scale(-_StudHeight / 2));
                        Geo.Point interiorBraceEnd = common.Add(bisector.Normalized().Scale(interiorBraceEndPointOffset)).Add(interiorBraceNormal.Normalized().Scale(-_StudHeight / 2));
                        Geo.Line interiorBraceAxis = Geo.Line.ByStartPointEndPoint(interiorBraceStart, interiorBraceEnd);
                        hMember interiorBrace = new hMember(interiorBraceAxis, interiorBraceNormal, "BR2");
                        interiorBrace.AddOperationByLocationType(0, "END_TRUSS");
                        interiorBrace.AddOperationByLocationType(dOffset, "DIMPLE");
                        interiorBrace.AddOperationByLocationType(dOffset, "SWAGE");
                        interiorBrace.AddOperationByLocationType(interiorBraceAxis.Length, "END_TRUSS");
                        interiorBrace.AddOperationByLocationType(interiorBraceAxis.Length - dOffset, "DIMPLE");
                        interiorBrace.AddOperationByLocationType(interiorBraceAxis.Length - dOffset, "SWAGE");

                        // Get some helpful points
                        Geo.Point conDimplePoint = common.Add(bisector.Normalized().Scale((webOut) ? d : -d));
                        Geo.Point braceDimplePoint = interiorBraceEnd.Add(Geo.Vector.ByTwoPoints(interiorBraceEnd, interiorBraceStart).Normalized().Scale(dOffset)).Add(interiorBraceNormal.Normalized().Scale(_StudHeight / 2));


                        // Create exterior brace 1
                        Geo.Point exBrace1Dimple = conDimplePoint.Add(v1.Normalized().Scale(d2));
                        Geo.Vector exBrace1Vec = Geo.Vector.ByTwoPoints(exBrace1Dimple, braceDimplePoint).Normalized();
                        Geo.Vector exBrace1Normal = exBrace1Vec.Rotate(v2.Cross(v1), -90);
                        Geo.Point exBrace1Start = exBrace1Dimple.Add(exBrace1Vec.Normalized().Scale(-dOffset)).Add(exBrace1Normal.Normalized().Scale(-_StudHeight / 2));
                        Geo.Point exBrace1End = braceDimplePoint.Add(exBrace1Vec.Normalized().Scale(dOffset)).Add(exBrace1Normal.Normalized().Scale(-_StudHeight / 2));
                        Geo.Line exBrace1Axis = Geo.Line.ByStartPointEndPoint(exBrace1Start, exBrace1End);
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
                        Geo.Point exBrace2Dimple = conDimplePoint.Add(v2.Normalized().Scale(d2));
                        Geo.Vector exBrace2Vec = Geo.Vector.ByTwoPoints(exBrace2Dimple, braceDimplePoint).Normalized();
                        Geo.Vector exBrace2Normal = exBrace2Vec.Rotate(v1.Cross(v2), -90);
                        Geo.Point exBrace2Start = exBrace2Dimple.Add(exBrace2Vec.Normalized().Scale(-dOffset)).Add(exBrace2Normal.Normalized().Scale(-_StudHeight / 2));
                        Geo.Point exBrace2End = braceDimplePoint.Add(exBrace2Vec.Normalized().Scale(dOffset)).Add(exBrace2Normal.Normalized().Scale(-_StudHeight / 2));
                        Geo.Line exBrace2Axis = Geo.Line.ByStartPointEndPoint(exBrace2Start, exBrace2End);
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
                        Geo.Line axis1 = _members[index1]._webAxis;
                        Geo.Line axis2 = _members[index2]._webAxis;

                        // Get intersection point
                        Geo.Point intersectionPoint = ClosestPointToOtherLine(axis1, axis2);

                        // Get angle between members
                        double angle = (Math.PI/180) * Geo.Vector.ByTwoPoints(axis1.StartPoint, axis1.EndPoint).AngleWithVector(Geo.Vector.ByTwoPoints(axis2.StartPoint, axis2.EndPoint));
                        angle = (angle < (Math.PI/2)) ? angle : Math.PI - angle;
                        
                        // Get distance from centerline of other member to edge of other member, along axis of current member (fun sentence)
                        double subtract = (angle % (Math.PI / 2) == 0) ? 0 : _WEBHoleSpacing / Math.Tan(angle);
                        double minExtension = _WEBHoleSpacing / Math.Sin(angle) - subtract + ((2 * _WEBHoleSpacing) / Math.Tan(angle)) + 1;
                        double mE = minExtension;
                        double extendToMaxCoverage = (angle % (Math.PI / 2) == 0) ? 0 : (_StudWdith/2) / Math.Tan(angle) + (_StudWdith / 2) / Math.Sin(angle);
                        double noExtra = (angle % (Math.PI / 2) == 0 || Math.Abs(angle - (Math.PI / 2)) < 0.01 || angle < 0.01) ? (_StudWdith / 2) : (_StudWdith / 2) / Math.Sin(angle) - (_StudWdith / 2) / Math.Tan(angle);
                        int type = 0;
                        if (this._extensionDict.ContainsKey(_members[index1]._label))
                        {
                            type = _extensionDict[_members[index1]._label];
                        }

                        minExtension = noExtra + 1.5;

                        if (type == 1)
                        {
                            minExtension -= 1.5;
                        }
                        else
                        {
                            minExtension = mE - 0.25;
                        }


                        // Check start point
                        if (SamePoints(intersectionPoint, axis1.StartPoint) || intersectionPoint.DistanceTo(axis1.StartPoint) < minExtension)
                        {
                            // Extend
                            Geo.Vector moveVector = Geo.Vector.ByTwoPoints(axis1.EndPoint, axis1.StartPoint);
                            moveVector = moveVector.Normalized();
                            moveVector = moveVector.Scale(minExtension);
                            Geo.Point newStartPoint = intersectionPoint.Add(moveVector);
                            _members[index1].SetWebAxisStartPoint(newStartPoint);
                        }

                        // Check end point
                        if (SamePoints(intersectionPoint, axis1.EndPoint) || intersectionPoint.DistanceTo(axis1.EndPoint) < minExtension)
                        {
                            // Extend
                            Geo.Vector moveVector = Geo.Vector.ByTwoPoints(axis1.StartPoint, axis1.EndPoint);
                            moveVector = moveVector.Normalized();
                            moveVector = moveVector.Scale(minExtension);
                            Geo.Point newEndPoint = intersectionPoint.Add(moveVector);
                            _members[index1].SetWebAxisEndPoint(newEndPoint);
                        }
                        
                        // Compute intersection location and location of web holes for fasteners
                        double intersectionLoc = _members[index1]._webAxis.ParameterAtPoint(intersectionPoint) * _members[index1]._webAxis.Length;
                        double d1 = (angle % (Math.PI / 2) < _tolerance) ? ((_WEBHoleSpacing / Math.Cos(angle)) - _WEBHoleSpacing) / Math.Tan(angle) : ((_WEBHoleSpacing / Math.Cos(angle)) - _WEBHoleSpacing) / Math.Tan(angle);
                        double d2 = (angle % (Math.PI / 2) < _tolerance) ? (2 * _WEBHoleSpacing) / Math.Tan(angle) : (2 * _WEBHoleSpacing) / Math.Tan(angle);

                        // Add valid web holes
                        List<double> webHoleLocations = new List<double> { intersectionLoc - (d1 + d2), intersectionLoc - (d1), intersectionLoc + (d1), intersectionLoc + (d1 + d2) };
                        foreach (double loc in webHoleLocations)
                        {
                            if (loc >= 0 && loc <= _members[index1]._webAxis.Length)
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
                    Geo.Line terminal;
                    Geo.Line cross;
                    int iTerminal;
                    int iCross;
                    GetTerminalAndCrossMember(connection, out terminal, out cross, out iTerminal, out iCross);

                    // Web axes as vectors
                    Geo.Vector vec1 = Geo.Vector.ByTwoPoints(terminal.StartPoint, terminal.EndPoint);
                    Geo.Vector vec2 = Geo.Vector.ByTwoPoints(cross.StartPoint, cross.EndPoint);

                    // Get angle between members
                    double angle = vec1.AngleWithVector(vec2) * (Math.PI / 180);
                    angle = (angle < (Math.PI/2.0)) ? angle : Math.PI - angle;

                    // Distance from DIMPLE to web (i.e. 0.5 * stud height)
                    double c1 = _StudHeight / 2.0;

                    // Compute extension to DIMPLE
                    double d1 = ((c1 / Math.Cos(angle)) + c1) / (Math.Tan(angle));
                    double d2 = ((c1 / Math.Cos(angle)) - c1) / (Math.Tan(angle));
                    bool b1 = _members[iCross]._webNormal.Dot(_members[iTerminal]._webNormal) < 0;
                    bool b2 = _members[iCross]._webNormal.Dot(vec1) > 0;
                    double d = (b1) ? d1 : d2;

                    // Determine orientation of members to each other and adjust d accordingly
                    if (b2)
                    {
                        d *= -1;
                    }

                    // Compute translation vector for end point in question
                    Geo.Vector moveVector = FlipVector(vec1);
                    moveVector = moveVector.Normalized();
                    moveVector = moveVector.Scale(d + 0.45);
                    Geo.Point newEndPoint = terminal.StartPoint.Add(moveVector);
                    
                    // Extend member
                    bool atMemberStart = SamePoints(terminal.StartPoint, _members[iTerminal]._webAxis.StartPoint);
                    if (atMemberStart) { _members[iTerminal].SetWebAxisStartPoint(newEndPoint); }
                    else { _members[iTerminal].SetWebAxisEndPoint(newEndPoint); }
                    
                    // Add operations to terminal member of T joint
                    double l = _members[iTerminal]._webAxis.Length;
                    _members[iTerminal].AddOperationByLocationType((atMemberStart) ? 0.0 : l - 0.0, "END_TRUSS");
                    _members[iTerminal].AddOperationByLocationType((atMemberStart) ? 0.45 : l - 0.45, "DIMPLE");
                    _members[iTerminal].AddOperationByLocationType((atMemberStart) ? 0.75 : l - 0.75, "SWAGE");

                    // Find the dimple point and its location on the cross member of the T joint
                    Geo.Point dimplePoint = _members[iTerminal]._webAxis.PointAtParameter(((atMemberStart) ? 0.45 : l - 0.45) / l);
                    double crossIntLoc = _members[iCross]._webAxis.ParameterAtPoint(dimplePoint) * _members[iCross]._webAxis.Length;
                    dimplePoint = dimplePoint.Add(_members[iTerminal]._webNormal.Normalized().Scale(0.75));
                    double crossDimpleLoc = _members[iCross]._webAxis.ParameterAtPoint(dimplePoint) * _members[iCross]._webAxis.Length;

                    // Add Dimple to cross member
                    _members[iCross].AddOperationByLocationType(crossDimpleLoc, "DIMPLE");
                    
                    // Compute range offsets for LIP_CUT or NOTCH so that terminal member can insert into cross member
                    double x1 = (c1 / Math.Sin(angle)) - (c1 / Math.Tan(angle));
                    double x2 = (c1 / Math.Sin(angle)) + (c1 / Math.Tan(angle));

                    // Entering through Lip or Web?
                    string op = (b2) ? "LIP_CUT" : "NOTCH";

                    // Compute range for insertion operation
                    double clearance = 0.25;
                    bool pos = Geo.Vector.ByTwoPoints(_members[iCross]._webAxis.StartPoint, _members[iCross]._webAxis.EndPoint).Dot(vec1) > 0;
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


        internal void ResolveBraceT(hMember brace, int memberIndex, Geo.Point dimplePoint, bool webOut, double BRAngle, Geo.Point connectionDimple)
        {
            // Get terminal and cross member axes
            Geo.Line terminal = brace.WebAxis;
            Geo.Line cross = _members[memberIndex].WebAxis;
            int iCross = memberIndex;

            // Web axes as vectors
            Geo.Vector vec1 = Geo.Vector.ByTwoPoints(terminal.StartPoint, terminal.EndPoint);
            Geo.Vector vec2 = Geo.Vector.ByTwoPoints(cross.StartPoint, cross.EndPoint);

            // Get angle between members
            double angle = vec1.AngleWithVector(vec2) * (Math.PI / 180);
            angle = (angle < (Math.PI / 2.0)) ? angle : Math.PI - angle;

            // Distance from DIMPLE to web (i.e. 0.5 * stud height)
            double c1 = _StudHeight / 2.0;

            // Compute extension to DIMPLE
            double d1 = ((c1 / Math.Cos(angle)) + c1) / (Math.Tan(angle));
            double d2 = ((c1 / Math.Cos(angle)) - c1) / (Math.Tan(angle));
            bool b1 = _members[iCross]._webNormal.Dot(brace._webNormal) < 0;
            bool b2 = _members[iCross]._webNormal.Dot(vec1) > 0;
            double d = (b1) ? d1 : d2;

            // Determine orientation of members to each other and adjust d accordingly
            if (b2)
            {
                d *= -1;
            }

            // Compute translation vector for end point in question
            Geo.Vector moveVector = FlipVector(vec1);
            moveVector = moveVector.Normalized();
            moveVector = moveVector.Scale(d + 0.45);
            Geo.Point newEndPoint = terminal.StartPoint.Add(moveVector);
            

            // Find the dimple point and its location on the cross member of the T joint
            double crossIntLoc = _members[iCross]._webAxis.ParameterAtPoint(dimplePoint) * _members[iCross]._webAxis.Length;
            //dimplePoint = dimplePoint.Add(brace._webNormal.Normalized().Scale(0.75));
            double crossDimpleLoc = _members[iCross]._webAxis.ParameterAtPoint(dimplePoint) * _members[iCross]._webAxis.Length;

            // Add Dimple to cross member
            //_members[iCross].AddOperationByLocationType(crossDimpleLoc, "DIMPLE");

            // Compute range offsets for LIP_CUT or NOTCH so that terminal member can insert into cross member
            double x1 = (c1 / Math.Sin(angle)) - (c1 / Math.Tan(angle));
            double x2 = (c1 / Math.Sin(angle)) + (c1 / Math.Tan(angle));

            // Entering through Lip or Web?
            string op = (b2) ? "LIP_CUT" : "NOTCH";

            // Compute range for insertion operation
            double clearance = 0.25;
            bool pos = Geo.Vector.ByTwoPoints(_members[iCross]._webAxis.StartPoint, _members[iCross]._webAxis.EndPoint).Dot(vec1) > 0;
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
            Geo.Vector conToBraceDimple = Geo.Vector.ByTwoPoints(connectionDimple, dimplePoint).Normalized().Scale(off);
            Geo.Point extraOpPt = connectionDimple.Add(conToBraceDimple);


            bool atMemberStart = SamePoints(terminal.StartPoint, brace._webAxis.StartPoint);
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
        internal void GetTerminalAndCrossMember(hConnection con, out Geo.Line terminal, out Geo.Line cross, out int iTerminal, out int iCross)
        {
            // Initialize all variables. These initial values should never be returned.. If my assumptions are correct
            terminal = null;
            cross = null;
            iTerminal = -1;
            iCross = -1;

            foreach(int mem in con.members)
            {
                var pts = new List<Geo.Point> { _members[mem]._webAxis.StartPoint, _members[mem]._webAxis.EndPoint };

                for (int i = 0; i < 2; i++)
                {
                    if (pts[i].DistanceTo(_members[con.members[(con.members.IndexOf(mem) + 1) % 2]]._webAxis) < _tolerance)
                    {
                        terminal = Geo.Line.ByStartPointEndPoint(pts[i], pts[(i + 1) % 2]);
                        cross = Geo.Line.ByStartPointEndPoint(_members[con.members[(con.members.IndexOf(mem) + 1) % 2]]._webAxis.StartPoint, _members[con.members[(con.members.IndexOf(mem) + 1) % 2]]._webAxis.EndPoint);
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
                    Geo.Line inside;
                    Geo.Line outside;
                    int iInside;
                    int iOutisde;
                    GetInsideAndOutsideMember(connection, out inside, out outside, out iInside, out iOutisde);
                    
                    Geo.Point intersectionPoint = ClosestPointToOtherLine(inside, outside);
                    
                    // Web axes as vectors
                    Geo.Vector vec1 = Geo.Vector.ByTwoPoints(inside.StartPoint, inside.EndPoint);
                    Geo.Vector vec2 = Geo.Vector.ByTwoPoints(outside.StartPoint, outside.EndPoint);

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
                    bool b1 = _members[iOutisde]._webNormal.Dot(_members[iInside]._webNormal) < 0;
                    bool b2 = _members[iOutisde]._webNormal.Dot(vec1) > 0;
                    double d = (b1) ? x2 : x1;

                    // Determine orientation of members to each other and adjust d accordingly
                    if (!b2)
                    {
                        d *= -1;
                    }
                    intersectionPoint = intersectionPoint.Add(vec1.Normalized().Scale(d));

                    // Add operations to terminal member of T joint
                    double l = _members[iInside]._webAxis.Length;
                    _members[iInside].AddOperationByPointType(intersectionPoint, "DIMPLE");
                    _members[iInside].AddOperationByPointType(intersectionPoint, "SWAGE");
                    
                    // Find the dimple point and its location on the cross member of the T joint
                    Geo.Point dimplePoint = intersectionPoint;
                    double crossIntLoc = _members[iOutisde]._webAxis.ParameterAtPoint(dimplePoint) * _members[iOutisde]._webAxis.Length;
                    dimplePoint = dimplePoint.Add(_members[iInside]._webNormal.Normalized().Scale(0.75));
                    double crossDimpleLoc = _members[iOutisde]._webAxis.ParameterAtPoint(dimplePoint) * _members[iOutisde]._webAxis.Length;

                    // Add Dimple to cross member
                    _members[iOutisde].AddOperationByLocationType(crossDimpleLoc, "DIMPLE");

                    // Compute range offsets for LIP_CUT or NOTCH so that terminal member can insert into cross member
                    

                    // Entering through Lip or Web?
                    string op = (b2) ? "LIP_CUT" : "NOTCH";
                    string op2 = (b2) ? "NOTCH" : "LIP_CUT";

                    // Compute range for insertion operation
                    double clearance = 0.25;
                    bool pos = Geo.Vector.ByTwoPoints(_members[iOutisde]._webAxis.StartPoint, _members[iOutisde]._webAxis.EndPoint).Dot(vec1) > 0;
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

        internal void GetInsideAndOutsideMember(hConnection con, out Geo.Line inside, out Geo.Line outside, out int iInside, out int iOutside)
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
        /// Find the plane that best fits 2 lines
        /// </summary>
        /// <param name="line1"></param>
        /// <param name="line2"></param>
        /// <returns>plane of best fit</returns>
        internal Geo.Plane ByTwoLines(Geo.Line line1, Geo.Line line2)
        {
            Geo.Vector vec1 = Geo.Vector.ByTwoPoints(line1.StartPoint, line1.EndPoint);
            Geo.Vector vec2 = Geo.Vector.ByTwoPoints(line2.StartPoint, line2.EndPoint);
            Geo.Vector normal = vec1.Cross(vec2);

            Geo.Plane p = Geo.Plane.ByOriginNormal(line1.StartPoint, normal);

            return p;
        }


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
        /// Determines whether two planes are parallel
        /// </summary>
        /// <param name="plane1"></param>
        /// <param name="plane2"></param>
        /// <returns>true if planes are parallel; false if planes are not parallel</returns>
        internal bool ParallelPlanes(Geo.Plane plane1, Geo.Plane plane2)
        {
            Geo.Vector vec1 = plane1.Normal;
            Geo.Vector vec2 = plane2.Normal;

            double similarity = vec1.Dot(vec2);

            return (Math.Abs(similarity) > 1 - _tolerance);
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

        [MultiReturn(new[] { "f", "g", "h", "i", "j", "k" })]
        [IsVisibleInDynamoLibrary(false)]
        public static Dictionary<string, object> ClosestPointToOtherLineTest(Geo.Line line, Geo.Line other)
        {
            Geo.Point startPt1 = line.StartPoint;
            Geo.Point startPt2 = other.StartPoint;

            Geo.Vector vec1 = Geo.Vector.ByTwoPoints(line.StartPoint, line.EndPoint);
            Geo.Vector vec2 = Geo.Vector.ByTwoPoints(other.StartPoint, other.EndPoint);

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

            double f = a1 * a2 + b1 * b2 + c1 * c2;
            double g = -(a1 * a1 + b1 * b1 + c1 * c1);
            double h = -(a1 * (x2 - x1) + b1 * (y2 - y1) + c1 * (z2 - z1));
            double i = (a2 * a2 + b2 * b2 + c2 * c2);
            double j = -1 * f;
            double k = -(a2 * (x2 - x1) + b2 * (y2 - y1) + c2 * (z2 - z1));

            double t = (k - (h * i / f)) / (j - (g * i / f));

            double xp = x1 + (a1 * t);
            double yp = y1 + (b1 * t);
            double zp = z1 + (c1 * t);

            return new Dictionary<string, object>
            {
                { "f", f },
                { "g", g },
                { "h", h },
                { "i", i },
                { "j", j },
                { "k", k }


            };

            //return Geo.Point.ByCoordinates(xp, yp, zp);
        }



        [IsVisibleInDynamoLibrary(false)]
        public static Geo.Point ClosestPointToOtherLine(Geo.Line line, Geo.Line other)
        {
            Geo.Point startPt1 = line.StartPoint;
            Geo.Point startPt2 = other.StartPoint;

            Geo.Vector vec1 = Geo.Vector.ByTwoPoints(line.StartPoint, line.EndPoint);
            Geo.Vector vec2 = Geo.Vector.ByTwoPoints(other.StartPoint, other.EndPoint);

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
     
            return Geo.Point.ByCoordinates(xp, yp, zp);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="line1"></param>
        /// <param name="line2"></param>
        /// <returns></returns>
        [IsVisibleInDynamoLibrary(false)]
        public static double LineToLineDistance(Geo.Line line1, Geo.Line line2)
        {
            Geo.Point pt1 = line1.StartPoint;
            Geo.Point pt2 = line1.EndPoint;
            Geo.Point pt3 = line2.StartPoint;
            Geo.Point pt4 = line2.EndPoint;

            Geo.Vector vec1 = Geo.Vector.ByTwoPoints(line1.StartPoint, line1.EndPoint);
            Geo.Vector vec2 = Geo.Vector.ByTwoPoints(line2.StartPoint, line2.EndPoint);

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
        [IsVisibleInDynamoLibrary(false)]
        public static double PointToLineDistance(Geo.Point point, Geo.Line line)
        {
            Geo.Point p = line.StartPoint;
            Geo.Vector v = Geo.Vector.ByTwoPoints(line.StartPoint, line.EndPoint);

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
        [IsVisibleInDynamoLibrary(false)]
        public static double MinDistanceBetweenLines(Geo.Line line1, Geo.Line line2)
        {
            var distances = new List<double>();

            Geo.Point pt1 = line1.StartPoint;
            Geo.Point pt2 = line1.EndPoint;
            Geo.Point pt3 = line2.StartPoint;
            Geo.Point pt4 = line2.EndPoint;

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

        /// <summary>
        /// 
        /// </summary>
        /// <param name="line1"></param>
        /// <param name="line2"></param>
        /// <param name="tolerance"></param>
        /// <returns></returns>
        [IsVisibleInDynamoLibrary(false)]
        public static bool LinesIntersectWithTolerance(Geo.Line line1, Geo.Line line2, double tolerance)
        {
            return MinDistanceBetweenLines(line1, line2) <= tolerance;
        }

        #endregion
    }
}
