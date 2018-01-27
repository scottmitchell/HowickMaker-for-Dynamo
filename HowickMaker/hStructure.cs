using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Geo = Autodesk.DesignScript.Geometry;

namespace HowickMaker
{
    public class hStructure
    {
        public hMember[] _members;
        public List<hConnection> _connections = new List<hConnection>();
        internal Graph _g;
        internal double _tolerance = 0.001;
        internal double _WEBHoleOffset = (15.0 / 16);
        List<Geo.Line> _lines = new List<Geo.Line>();
        List<hMember> _braceMembers = new List<hMember>();

        public hStructure()
        {

        }

        public hStructure(List<Geo.Line> lines, double tolerance)
        {
            _lines = lines;
            _tolerance = tolerance;
            _members = new hMember[lines.Count];
            for (int i = 0; i < lines.Count; i++)
            {
                _members[i] = new hMember(lines[i], i);
            }
        }

        internal void InitArrays(int num, List<Geo.Line> lines)
        {
            hMember[] mems = new hMember[num];
            for (int i = 0; i < num; i++)
            {
                mems[i] = new hMember(lines[i], i);
            }
            _members = mems;
        }


        //  ██████╗ ██╗   ██╗██████╗      ██████╗███╗   ██╗███████╗████████╗██████╗ 
        //  ██╔══██╗██║   ██║██╔══██╗    ██╔════╝████╗  ██║██╔════╝╚══██╔══╝██╔══██╗
        //  ██████╔╝██║   ██║██████╔╝    ██║     ██╔██╗ ██║███████╗   ██║   ██████╔╝
        //  ██╔═══╝ ██║   ██║██╔══██╗    ██║     ██║╚██╗██║╚════██║   ██║   ██╔══██╗
        //  ██║     ╚██████╔╝██████╔╝    ╚██████╗██║ ╚████║███████║   ██║   ██║  ██║
        //  ╚═╝      ╚═════╝ ╚═════╝      ╚═════╝╚═╝  ╚═══╝╚══════╝   ╚═╝   ╚═╝  ╚═╝
        //                                                                          


        /// <summary>
        /// 
        /// </summary>
        /// <param name="lines"></param>
        /// <returns></returns>
        public static List<hMember> FromLines(List<Geo.Line> lines, double tolerance)
        {
            hStructure structure = StructureFromLines(lines, tolerance);
            return structure._members.ToList();

        }

        public static List<hMember> BracesFromLines(List<Geo.Line> lines, double tolerance, int type)
        {
            hStructure structure = StructureFromLines(lines, tolerance);
            structure.GenerateBraces(7, 4, type);
            return structure._braceMembers;
        }



        //  ████████╗███████╗███████╗████████╗
        //  ╚══██╔══╝██╔════╝██╔════╝╚══██╔══╝
        //     ██║   █████╗  ███████╗   ██║   
        //     ██║   ██╔══╝  ╚════██║   ██║   
        //     ██║   ███████╗███████║   ██║   
        //     ╚═╝   ╚══════╝╚══════╝   ╚═╝   
        //                                    


        /// <summary>
        /// 
        /// </summary>
        /// <param name="lines"></param>
        /// <returns></returns>
        public static List<string> Test(List<Geo.Line> lines, double tolerance)
        {
            //hStructure structure = StructureFromLines(lines);

            Graph g = graphFromLines(lines, tolerance);
            List<string> s = new List<string>();
            foreach (Vertex v in g.vertices)
            {
                string x = v.name.ToString() + ": ";
                foreach (int i in v.neighbors)
                {
                    x += i.ToString() + ", ";
                }
                s.Add(x);
            }
            return s;
        }



        public static List<string> Test2(List<Geo.Line> lines, double tolerance)
        {
            hStructure structure = StructureFromLines(lines, tolerance);

            List<string> cons = new List<string>();

            foreach (hMember m in structure._members)
            {
                if (m != null)
                {
                    string s = m.name + ": ";
                    if (m.connections.Count > 0)
                    {
                        foreach (hConnection c in m.connections)
                        {

                            if (c.members.Count > 0)
                            {
                                foreach (int i in c.members)
                                {
                                    s += i.ToString() + ", ";
                                }
                            }
                            else
                            {
                                s += "NONE";
                            }

                        }
                    }
                    cons.Add(s);
                }
                else
                {
                    cons.Add("NULL");
                }
            }
            return cons;
        }



        #region Quad Strategy 01

        public static List<Geo.Line> QuadStrategy_01(hMesh mesh)
        {

            List<Geo.Line> lines = new List<Geo.Line>();
            foreach (hFace f in mesh.faces)
            {
                Geo.Point p1 = Geo.Point.ByCoordinates(f.vertices[0].x, f.vertices[0].y, f.vertices[0].z);
                Geo.Point p2 = Geo.Point.ByCoordinates(f.vertices[1].x, f.vertices[1].y, f.vertices[1].z);

                lines.Add(Geo.Line.ByStartPointEndPoint(p1, p2));
            }
            return lines;
        }




        #endregion



        #region Triangulation Strategy 01
        //  ████████╗██████╗ ██╗     ██████╗  ██╗
        //  ╚══██╔══╝██╔══██╗██║    ██╔═████╗███║
        //     ██║   ██████╔╝██║    ██║██╔██║╚██║
        //     ██║   ██╔══██╗██║    ████╔╝██║ ██║
        //     ██║   ██║  ██║██║    ╚██████╔╝ ██║
        //     ╚═╝   ╚═╝  ╚═╝╚═╝     ╚═════╝  ╚═╝
        //                                       

        public static List<Geo.Line> TriMeshStrategy_01(hMesh mesh)
        {

            List<Geo.Line> lines = new List<Geo.Line>();
            foreach (hFace f in mesh.faces)
            {
                Geo.Point p1 = Geo.Point.ByCoordinates(f.vertices[0].x, f.vertices[0].y, f.vertices[0].z);
                Geo.Point p2 = Geo.Point.ByCoordinates(f.vertices[1].x, f.vertices[1].y, f.vertices[1].z);

                lines.Add(Geo.Line.ByStartPointEndPoint(p1, p2));
            }
            return lines;
        }


        public static List<Geo.Line> TriMeshStrategy01_Wireframe(hMesh hMesh)
        {

            List<Geo.Line> lines = new List<Geo.Line>();
            foreach (hFace f in hMesh.faces)
            {
                List<Geo.Line> lines2 = getLines_Tri01(f);
                foreach (Geo.Line l in lines2)
                {
                    lines.Add(l);
                }
            }
            return lines;
        }

        public static List<hMember> TriMeshStrategy01_hMembers(hMesh hMesh)
        {

            List<Geo.Line> lines = new List<Geo.Line>();
            List<hMember> members = new List<hMember>();
            foreach (hFace f in hMesh.faces)
            {
                Geo.Plane facePlane = Geo.Plane.ByThreePoints(f.vertices[0].ToPoint(), f.vertices[1].ToPoint(), f.vertices[2].ToPoint());
                Geo.Vector faceNormal = facePlane.Normal;
                Geo.Vector faceNormalR = Geo.Vector.ByCoordinates(faceNormal.X*-1, faceNormal.Y * -1, faceNormal.Z * -1);

                List<Geo.Line> lines2 = getLines_Tri01(f);
                foreach (Geo.Line l in lines2)
                {
                    lines.Add(l);
                }

                members.Add(new hMember(lines2[0], faceNormalR));
                members.Add(new hMember(lines2[1], faceNormalR));
                members.Add(new hMember(lines2[2], faceNormal));
                members.Add(new hMember(lines2[3], faceNormal));
                members.Add(new hMember(lines2[4], faceNormal));
                members.Add(new hMember(lines2[5], faceNormal));
            }
            return members;
        }


        internal static List<Geo.Line> getLines_Tri01(hFace face)
        {

            Geo.Plane facePlane = Geo.Plane.ByThreePoints(face.vertices[0].ToPoint(), face.vertices[1].ToPoint(), face.vertices[2].ToPoint());
            Geo.Vector faceNormal = facePlane.Normal;

            double[] lengths = { 37.05, 37.05, 37.05, 16.84, 30.69, 23.77 };
            List<Geo.Line> lines = new List<Geo.Line>();
            for (int i = 0; i < 3; i++)
            {
                Geo.Vector edgeV = Geo.Vector.ByTwoPoints(face.vertices[i].ToPoint(), face.vertices[(i + 1) % 3].ToPoint());
                Geo.Line edge = Geo.Line.ByStartPointEndPoint(face.vertices[i].ToPoint(), face.vertices[(i + 1) % 3].ToPoint());

                Geo.Vector memberVector = faceNormal.Cross(edgeV);
                memberVector = memberVector.Reverse();

                edgeV = edgeV.Normalized();
                edgeV = edgeV.Scale((edge.Length / 2.0) - 6);

                Geo.Point p1 = face.vertices[i].ToPoint().Add(edgeV);
                edgeV = edgeV.Reverse();
                Geo.Point p2 = face.vertices[(i + 1) % 3].ToPoint().Add(edgeV);


                Geo.Line edge2 = Geo.Line.ByStartPointEndPoint(p1, p2);

                Geo.Line mem1 = Geo.Line.ByStartPointDirectionLength(p1, memberVector, lengths[i * 2]);
                Geo.Line mem2 = Geo.Line.ByStartPointDirectionLength(p2, memberVector, lengths[i * 2 + 1]);
                lines.Add(mem1);
                lines.Add(mem2);
            }
            return lines;
        }


        #endregion



        //  ██████╗ ████████╗███████╗
        //  ██╔══██╗╚══██╔══╝██╔════╝
        //  ██████╔╝   ██║   ███████╗
        //  ██╔══██╗   ██║   ╚════██║
        //  ██████╔╝   ██║   ███████║
        //  ╚═════╝    ╚═╝   ╚══════╝
        //                                                 

            
        /// <summary>
        /// 
        /// </summary>
        /// <param name="lines"></param>
        /// <returns></returns>
        internal static hStructure StructureFromLines(List<Geo.Line> lines, double tolerance)
        {
            hStructure structure = new hStructure(lines, tolerance);

            structure._g = graphFromLines(lines, structure._tolerance);

            structure.BuildMembersAndConnectionsFromGraph();
            structure.ResolveFTFConnections();

            return structure;
        }

        /// <summary>
        /// Creates a simple connectivity graph of a line network
        /// </summary>
        /// <param name="lines"> The list of lines we want to create a graph from </param>
        /// <returns> A connectivity graph </returns>
        internal static Graph graphFromLines(List<Geo.Line> lines, double tolerance)
        {
            // Create the connectivity graph
            Graph g = new Graph(lines.Count);
            
            // Iterate through each line...
            for (int i = 0; i < lines.Count; i++)
            {
                // ...vs every other line
                for (int j = 0; j < lines.Count; j++)
                {
                    // Make sure we're not checking a line against itself
                    if (i != j)
                    {
                        // Check if the two lines intersect
                        //if (lines[i].DoesIntersect(lines[j]))
                        //if (LinesIntersectWithTolerance(lines[i], lines[j], tolerance))
                        if (lines[i].DistanceTo(lines[j]) <= tolerance)
                        {
                            // If so, add j as a neighbor to i
                            g.vertices[i].addNeighbor(j);
                        }
                    }
                }
            }
            
            return g;
        }



        /// <summary>
        /// 
        /// </summary>
        /// <param name="lines"></param>
        internal void BuildMembersAndConnectionsFromGraph()
        {
            Vertex current = _g.vertices[0];
            
            hMember currentMember = new hMember(_lines[current.name], current.name);
            Geo.Line currentLine = _lines[0];
            Geo.Line nextLine = _lines[current.neighbors[0]];
            Geo.Plane currentPlane = ByTwoLines(currentLine, nextLine);

            currentMember.webNormal = currentPlane.Normal;
            _members[0] = currentMember;

            Propogate(0);
        }



        /// <summary>
        /// 
        /// </summary>
        /// <param name="current"></param>
        /// <param name="lines"></param>
        internal void Propogate(int current)
        {
            hMember currentMember = _members[current];

            if (_g.vertices[current].neighbors.Count > 0)
            {
                // Iterate through each neighbor
                foreach (int i in _g.vertices[current].neighbors)
                {
                    // If we have not been to this vertex yet
                    if (!_g.vertices[i].visited)
                    {
                        hConnection con = new hConnection(GetConnectionType(current, i), new List<int> { i, current });

                        _connections.Add(con);

                        _members[i].connections.Add(con);
                        _members[current].connections.Add(con);

                        List<Geo.Vector> vectors = GetValidNormalsForMember(i);
                        _members[i].webNormal = vectors[0];

                        _g.vertices[current].visited = true;
                        Propogate(i);
                    }
                }
            }
        }


        internal Connection GetConnectionType(int i, int j)
        {
            Geo.Plane jointPlane = ByTwoLines(_lines[i], _lines[j]);

            if (ParallelPlaneNormals(_members[i].webNormal, jointPlane.Normal))
            {
                return Connection.FTF;
            }
            else
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
        }

        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="connection"></param>
        /// <param name="memberIndex"></param>
        /// <returns></returns>
        internal List<Geo.Vector> GetValidNormalsForMemberForConnection(hConnection connection, int memberIndex)
        {
            int otherMemberIndex = connection.GetOtherIndex(memberIndex);
            
            if (connection.type == Connection.FTF)
            {
                Geo.Vector otherNormal = _members[otherMemberIndex].webNormal;
                return new List<Geo.Vector> { FlipVector(otherNormal) };
            }

            else if (connection.type == Connection.BR)
            {
                Geo.Plane jointPlane = ByTwoLines(_members[memberIndex].webAxis, _members[otherMemberIndex].webAxis);
                Geo.Vector memberVector = Geo.Vector.ByTwoPoints(_members[memberIndex].webAxis.StartPoint, _members[memberIndex].webAxis.EndPoint);

                Geo.Vector webNormal1 = jointPlane.Normal.Cross(memberVector);
                
                return new List<Geo.Vector> { GetBRNormal(connection, memberIndex) };
            }

            else
            {
                Geo.Plane jointPlane = ByTwoLines(_members[memberIndex].webAxis, _members[otherMemberIndex].webAxis);
                Geo.Vector memberVector = Geo.Vector.ByTwoPoints(_members[memberIndex].webAxis.StartPoint, _members[memberIndex].webAxis.EndPoint);

                Geo.Vector webNormal1 = jointPlane.Normal.Cross(memberVector);

                return new List<Geo.Vector> { webNormal1, FlipVector(webNormal1) };
            }

        }



        /// <summary>
        /// 
        /// </summary>
        /// <param name="memberIndex"></param>
        /// <returns></returns>
        internal List<Geo.Vector> GetValidNormalsForMember(int memberIndex)
        {
            List<Geo.Vector> potentialVectors = GetValidNormalsForMemberForConnection(_members[memberIndex].connections[0], memberIndex);

            foreach (hConnection connection in _members[memberIndex].connections)
            {
                List<Geo.Vector> checkVectors = GetValidNormalsForMemberForConnection(connection, memberIndex);
                List<Geo.Vector> newVectors = new List<Geo.Vector>();
                foreach (Geo.Vector pV in potentialVectors)
                {
                    foreach (Geo.Vector cV in checkVectors)
                    {
                        if (ParallelPlaneNormals(pV, cV))
                    {
                            newVectors.Add(pV);
                        }
                    }
                    
                }
            }
            
            return potentialVectors;
        }

        #region Resolve BR Connections

        internal void ResolveBRConnections()
        {

        }


        /// <summary>
        /// Generates brace hMembers for every braced connection (BR) in the structure
        /// </summary>
        /// <param name="braceLength"></param>
        internal void GenerateBraces(double braceLength, double braceLength2, int type)
        {
            foreach (hConnection connection in _connections)
            {
                if (connection.type == Connection.BR)
                {
                    int mem1 = connection.members[0];
                    int mem2 = connection.members[1];

                    Geo.Point common;
                    Geo.Point mem1End;
                    Geo.Point mem2End;

                    if (SamePoints(_members[mem1].webAxis.StartPoint, _members[mem2].webAxis.StartPoint))
                    {
                        common = _members[mem1].webAxis.StartPoint;
                        mem1End = _members[mem1].webAxis.EndPoint;
                        mem2End = _members[mem2].webAxis.EndPoint;
                    }
                    else if (SamePoints(_members[mem1].webAxis.StartPoint, _members[mem2].webAxis.EndPoint))
                    {
                        common = _members[mem1].webAxis.StartPoint;
                        mem1End = _members[mem1].webAxis.EndPoint;
                        mem2End = _members[mem2].webAxis.StartPoint;
                    }
                    else if (SamePoints(_members[mem1].webAxis.EndPoint, _members[mem2].webAxis.StartPoint))
                    {
                        common = _members[mem1].webAxis.EndPoint;
                        mem1End = _members[mem1].webAxis.StartPoint;
                        mem2End = _members[mem2].webAxis.EndPoint;
                    }
                    else
                    {
                        common = _members[mem1].webAxis.EndPoint;
                        mem1End = _members[mem1].webAxis.StartPoint;
                        mem2End = _members[mem2].webAxis.StartPoint;
                    }

                    Geo.Vector v1 = Geo.Vector.ByTwoPoints(common, mem1End).Normalized();
                    Geo.Vector v2 = Geo.Vector.ByTwoPoints(common, mem2End).Normalized();
                    Geo.Vector bisector = v1.Add(v2);
                    bisector = bisector.Normalized();
                    bisector = bisector.Scale(braceLength2);

                    Geo.Vector brace3Normal = bisector.Rotate(v1.Cross(v2), 90).Normalized().Scale(0.75);
                    Geo.Vector brace3Move = FlipVector(brace3Normal);
                    Geo.Point b3End = common.Add(bisector);
                    Geo.Line brace3Axis = Geo.Line.ByStartPointDirectionLength(common, bisector, braceLength2);
                    hMember brace3 = new hMember(brace3Axis, brace3Normal);
                    
                    double a = v1.AngleWithVector(v2)/2.0 * (Math.PI/180);

                    double z = braceLength2 * Math.Cos(a) + braceLength * Math.Sin(Math.Acos((braceLength2 * Math.Sin(a) / braceLength)));
                    v1 = v1.Scale(z);
                    v2 = v2.Scale(z);

                    Geo.Point b1End = Geo.Point.ByCoordinates(common.X + v1.X, common.Y + v1.Y, common.Z + v1.Z);
                    Geo.Point b2End = Geo.Point.ByCoordinates(common.X + v2.X, common.Y + v2.Y, common.Z + v2.Z);
                    Geo.Vector brace1Normal = Geo.Vector.ByTwoPoints(brace3Axis.EndPoint, b1End).Rotate(v1.Cross(v2), -90);
                    Geo.Vector brace2Normal = Geo.Vector.ByTwoPoints(brace3Axis.EndPoint, b2End).Rotate(v2.Cross(v1), -90);

                    hMember brace1 = new hMember(Geo.Line.ByStartPointEndPoint(brace3Axis.EndPoint, b1End), brace1Normal);
                    hMember brace2 = new hMember(Geo.Line.ByStartPointEndPoint(brace3Axis.EndPoint, b2End), brace2Normal);
                    
                    hMember brace4 = new hMember(Geo.Line.ByStartPointEndPoint(b1End, b2End), FlipVector(bisector));

                    if (type == 0)
                    {
                        _braceMembers.Add(brace1);
                        _braceMembers.Add(brace2);
                        _braceMembers.Add(brace3);
                    }

                    else
                    {
                        _braceMembers.Add(brace4);
                    }
                }
            }
        }

        #endregion

        #region Resolve FTF Connections

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
                        Geo.Line axis1 = _members[index1].webAxis;
                        Geo.Line axis2 = _members[index2].webAxis;

                        // Get intersection point
                        Geo.Point intersectionPoint = ClosestPointToOtherLine(axis1, axis2);

                        // Get angle between members
                        double angle = (Math.PI/180) * Geo.Vector.ByTwoPoints(axis1.StartPoint, axis1.EndPoint).AngleWithVector(Geo.Vector.ByTwoPoints(axis2.StartPoint, axis2.EndPoint));
                        angle = (angle < (Math.PI/2)) ? angle : Math.PI - angle;

                        // Get distance from centerline of other member to edge of other member, along axis of current member (fun sentence)
                        double minExtension = _WEBHoleOffset / Math.Sin(angle) + _WEBHoleOffset / Math.Tan(angle) + 1;

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
                    }
                }
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


        


        internal Geo.Vector GetBRNormal(hConnection connection, int memberIndex)
        {
            Geo.Line memberAxis = _members[memberIndex].webAxis;
            Geo.Line otherMemberAxis = _members[connection.GetOtherIndex(memberIndex)].webAxis;

            Geo.Vector memberVector = Geo.Vector.ByTwoPoints(memberAxis.StartPoint, memberAxis.EndPoint);
            Geo.Vector otherMemberVector = null;

            Geo.Vector otherMemberNormal = _members[connection.GetOtherIndex(memberIndex)].webNormal;

            Geo.Point center = (Geo.Point)memberAxis.Intersect(otherMemberAxis)[0];
            
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
        }




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
        internal Geo.Vector FlipVector(Geo.Vector vec)
        {
            return Geo.Vector.ByCoordinates(vec.X * -1, vec.Y * -1, vec.Z * -1);
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
        /// 
        /// </summary>
        /// <param name="vec1"></param>
        /// <param name="vec2"></param>
        /// <returns></returns>
        internal bool ParallelPlaneNormals(Geo.Vector vec1, Geo.Vector vec2)
        {
            double similarity = vec1.Dot(vec2);

            return (Math.Abs(similarity) > 1 - _tolerance);
        }
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="p1"></param>
        /// <param name="p2"></param>
        /// <returns></returns>
        internal bool SamePoints(Geo.Point p1, Geo.Point p2)
        {
            bool x = Math.Abs(p1.X - p2.X) < _tolerance;
            bool y = Math.Abs(p1.Y - p2.Y) < _tolerance;
            bool z = Math.Abs(p1.Z - p2.Z) < _tolerance;

            return x && y && z;
        }




        #region Line Segment Distances

        public static Geo.Point ClosestPointToOtherLine(Geo.Line line, Geo.Line other)
        {
            Geo.Point pt1 = line.StartPoint;
            Geo.Point pt3 = other.StartPoint;

            Geo.Vector vec1 = Geo.Vector.ByTwoPoints(line.StartPoint, line.EndPoint);
            Geo.Vector vec2 = Geo.Vector.ByTwoPoints(other.StartPoint, other.EndPoint);

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
            double g = -(a1 * a1 + b1 * b1 + c1 * c1);
            double h = -(a1 * (x2 - x1) + b1 * (y2 - y1) + c1 * (z2 - z1));
            double i = (a2 * a2 + b2 * b2 + c2 * c2);
            double j = -1 * f;
            double k = -(a2 * (x2 - x1) + b2 * (y2 - y1) + c2 * (z2 - z1));

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
            public static bool LinesIntersectWithTolerance(Geo.Line line1, Geo.Line line2, double tolerance)
        {
            return MinDistanceBetweenLines(line1, line2) <= tolerance;
        }

        #endregion
    }
}
