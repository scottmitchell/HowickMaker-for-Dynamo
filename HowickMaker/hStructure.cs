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
        List<Geo.Line> _lines = new List<Geo.Line>();

        public hStructure()
        {

        }

        public hStructure(List<Geo.Line> lines)
        {
            _lines = lines;
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
        public static List<hMember> FromLines(List<Geo.Line> lines)
        {
            hStructure structure = StructureFromLines(lines);
            return structure._members.ToList();
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
        public static List<string> Test(List<Geo.Line> lines)
        {
            //hStructure structure = StructureFromLines(lines);

            Graph g = graphFromLines(lines);
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



        public static List<string> Test2(List<Geo.Line> lines)
        {
            hStructure structure = StructureFromLines(lines);

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
        internal static hStructure StructureFromLines(List<Geo.Line> lines)
        {
            hStructure structure = new hStructure(lines);

            structure._g = graphFromLines(lines);

            structure.BuildMembersAndConnectionsFromGraph();

            return structure;
        }

        /// <summary>
        /// Creates a simple connectivity graph of a line network
        /// </summary>
        /// <param name="lines"> The list of lines we want to create a graph from </param>
        /// <returns> A connectivity graph </returns>
        internal static Graph graphFromLines(List<Geo.Line> lines)
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
                        if (lines[i].DoesIntersect(lines[j]))
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


        internal bool ParallelPlaneNormals(Geo.Vector vec1, Geo.Vector vec2)
        {
            double similarity = vec1.Dot(vec2);

            return (Math.Abs(similarity) > 1 - _tolerance);
        }

        internal bool SamePoints(Geo.Point p1, Geo.Point p2)
        {
            bool x = Math.Abs(p1.X - p2.X) < _tolerance;
            bool y = Math.Abs(p1.Y - p2.Y) < _tolerance;
            bool z = Math.Abs(p1.Z - p2.Z) < _tolerance;

            return x && y && z;
        }

        internal bool LinesIntersectWithTolerance(Geo.Line line1, Geo.Line line2, double tolerance)
        {
            Geo.Point pt1 = line1.StartPoint;
            Geo.Point pt2 = line1.EndPoint;
            Geo.Point pt3 = line2.StartPoint;
            Geo.Point pt4 = line2.EndPoint;

            Geo.Vector vec1 = Geo.Vector.ByTwoPoints(line1.StartPoint, line1.EndPoint);
            Geo.Vector vec2 = Geo.Vector.ByTwoPoints(line2.StartPoint, line2.EndPoint);
            
            

            return true;
        }

    }
}
