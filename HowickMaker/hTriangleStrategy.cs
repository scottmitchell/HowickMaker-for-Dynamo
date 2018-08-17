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
    public class hTriangleStrategy
    {
        public Mesh _mesh;
        private tAgent[] _agents;

        public List<string> _test = new List<string>();

        /// <summary>
        /// Initiates a Triangle Strategy solver from a MeshTookkit mesh
        /// </summary>
        /// <param name="mesh"></param>
        public hTriangleStrategy(Mesh mesh)
        {
            this._mesh = mesh;
            this._agents = new tAgent[mesh.EdgeCount];
            CreateAgents();
        }

        internal void CreateAgents()
        {
            var indices = _mesh.VertexIndicesByTri();
            var faces = SplitList(indices);

            // Loop through every face
            for (int i = 0; i < faces.Count; i++)
            {
                // Loop through every edge of every face
                for (int j = 0; j < faces[i].Count; j++)
                {
                    int start = faces[i][j];
                    int end = faces[i][(j + 1) % faces[i].Count];
                    int agentIndex = GetAgentIndex(start, end);

                    // Check to make sure we haven't already created an agent for this edge
                    if (_agents[agentIndex] == null)
                    {
                        int thirdVertex = faces[i][(j + 2) % faces[i].Count];
                        int adjacentFace = -1;
                        int adjacentFaceThirdVertex = -1;
                        Geo.Line currentEdge = Geo.Line.ByStartPointEndPoint(_mesh.Vertices()[start], _mesh.Vertices()[end]);

                        // Loop through every face again
                        for (int k = 0; k < faces.Count; k++)
                        {
                            // Only look at faces that aren't this face (face[i])
                            if (k != i)
                            {
                                // Find the other face that contains this edge, if such a face exists
                                if (faces[k].Contains(start) && faces[k].Contains(end))
                                {
                                    adjacentFace = k;

                                    // Find the vertex on the adjacent face that is not associated with this edge
                                    var currentIndex = 0;
                                    while (faces[k][currentIndex] == start || faces[k][currentIndex] == end)
                                    {
                                        currentIndex++;
                                    }
                                    adjacentFaceThirdVertex = faces[k][currentIndex];
                                }
                            }
                        }

                        // Determine neighboring agent indices
                        var neighbors = new List<int> { GetAgentIndex(start, thirdVertex), GetAgentIndex(end, thirdVertex) };
                        if (adjacentFace != -1)
                        {
                            neighbors.Add(GetAgentIndex(start, adjacentFaceThirdVertex));
                            neighbors.Add(GetAgentIndex(end, adjacentFaceThirdVertex));
                        }

                        // Create new agent
                        _agents[agentIndex] = new tAgent(agentIndex, currentEdge, neighbors, i, adjacentFace, _mesh.TriangleNormals()[i], (adjacentFace != -1) ? _mesh.TriangleNormals()[adjacentFace] : null);

                        var edgeStats = _agents[agentIndex].ToString();
                        _test.Add(edgeStats);
                    }
                }
            }
        }
        


        internal int GetAgentIndex(int start, int end, double tolerance = 0.001)
        {
            var startPoint = _mesh.Vertices()[start];
            var endPoint = _mesh.Vertices()[end];

            for (int i = 0; i < _mesh.EdgeCount; i++)
            {
                if (_mesh.Edges()[i].StartPoint.DistanceTo(startPoint) < tolerance || _mesh.Edges()[i].StartPoint.DistanceTo(endPoint) < tolerance)
                {
                    if (_mesh.Edges()[i].EndPoint.DistanceTo(startPoint) < tolerance || _mesh.Edges()[i].EndPoint.DistanceTo(endPoint) < tolerance)
                    {
                        return i;
                    }
                }
            }
            return -1;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="mesh"></param>
        /// <returns></returns>
        public static List<Geo.Line> FromMesh(Mesh mesh)
        {
            var solver = new hTriangleStrategy(mesh);
            return solver.GetSolvedWebAxes();
        }


        

        internal void GetAgentNeighbors(tAgent agent)
        {
            var startPoint = agent._edge.StartPoint;
            var endPoint = agent._edge.EndPoint;
        }


        public List<string> vertices()
        {
            return this._test;
        }

        internal List<Geo.Line> GetSolvedWebAxes()
        {
            var pts = new List<Geo.Line>();
            foreach (tAgent agent in _agents)
            {
                foreach (Geo.Line l in agent.GetMemberLines(_agents))
                {
                    pts.Add(l);
                }
            }
            return pts;
        }



        /// <summary>
        /// Chops a list into sublists
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="list"></param>
        /// <param name="size"></param>
        /// <returns></returns>
        internal static List<List<T>> SplitList<T>(List<T> list, int size = 3)
        {
            if (list.Count % size != 0)
            {
                throw new System.ArgumentException("List length must be divisible by split size", "lsit");          
            }

            var splitList = new List<List<T>>();
            var temp = new List<T>();

            for (int i = 0; i < list.Count; i++)
            {
                temp.Add(list[i]);
                if (i % size == size-1)
                {
                    splitList.Add(temp);
                    temp = new List<T>();
                }
            }

            return splitList;
        }

    }
}
