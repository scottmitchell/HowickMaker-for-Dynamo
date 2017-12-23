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
        public List<hMember> members = new List<hMember>();
        public List<hConnection> connections = new List<hConnection>();
        internal Graph g;

        public hStructure()
        {

        }
        


        public static List<hMember> FromLines(List<Geo.Line> lines)
        {
            hStructure structure = StructureFromLines(lines);
            return null;
        }

        public static int Test(List<Geo.Line> lines)
        {
            //hStructure structure = StructureFromLines(lines);

            Graph g = graphFromLines(lines);

            return g.vertices.Count;
        }

        public static List<string> Test2(List<Geo.Line> lines)
        {
            hStructure structure = StructureFromLines(lines);

            List<string> cons = new List<string>();
            foreach (hConnection c in structure.connections)
            {
                string s = c.type + ": ";
                foreach (int t in c.members)
                {
                    s += t.ToString() + ", ";
                }
                cons.Add(s);
            }
            return cons;
        }

        public static hStructure StructureFromLines(List<Geo.Line> lines)
        {
            hStructure structure = new hStructure();
            structure.g = graphFromLines(lines);
            //structure.buildMembersAndConnectionsFromGraph();


            return structure;

        }


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




        //  ██████╗ ████████╗███████╗
        //  ██╔══██╗╚══██╔══╝██╔════╝
        //  ██████╔╝   ██║   ███████╗
        //  ██╔══██╗   ██║   ╚════██║
        //  ██████╔╝   ██║   ███████║
        //  ╚═════╝    ╚═╝   ╚══════╝
        //                                                 


        /// <summary>
        /// Creates a simple connectivity graph of a line network
        /// </summary>
        /// <param name="lines"> The list of lines we want to create a graph from </param>
        /// <returns> A connectivity graph </returns>
        internal static Graph graphFromLines(List<Geo.Line> lines)
        {
            // Create the connectivity graph
            Graph g = new Graph();

            // Iterate through each line...
            foreach (Geo.Line line in lines)
            {
                // Create a vertex for this line
                Vertex v = new Vertex(g.vertices.Count);

                // ...vs every other line
                for (int j = 0; j < lines.Count; j++)
                {
                    // Make sure we're not checking a line against itself
                    if (line != lines[j])
                    {
                        // Check if the two lines intersect
                        if (line.DoesIntersect(lines[j]))
                        {
                            // If so, add it as a neighbor
                            v.addNeighbor(j);
                        }
                    }
                }
                // Add the vertex to the graph
                g.vertices.Add(v);
            }

            return g;
        
        }

        public void buildMembersAndConnectionsFromGraph()
        {
            Vertex current = g.vertices[0];
            Propogate(current);

        }

        internal void Propogate(Vertex current)
        {
            // Iterate through each neighbor
            foreach(int i in current.neighbors)
            {
                // If we have not been to this vertex yet
                if (!g.vertices[i].visited)
                {
                    ///////////////////////////////////////
                    ///////////////////////////////////////
                    // Create an hConnection
                    hConnection con = new hConnection(Connection.FTF);
                    con.members.Add(current.name);
                    con.members.Add(i);
                    ///////////////////////////////////////
                    ///////////////////////////////////////

                    connections.Add(con);

                    current.visited = true;

                    Propogate(g.vertices[i]);
                }
            }
        }


       
    }
}
