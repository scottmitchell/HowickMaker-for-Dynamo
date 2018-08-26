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
        List<List<Geo.Line>> states = new List<List<Geo.Line>>();

        public List<string> _test = new List<string>();

        /// <summary>
        /// Initiates a Triangle Strategy solver from a MeshTookkit mesh
        /// </summary>
        /// <param name="mesh"></param>
        /// <param name="iterations"></param>
        public hTriangleStrategy(Mesh mesh, int iterations)
        {
            this._mesh = mesh;
            this._agents = new tAgent[mesh.EdgeCount];
            CreateAgents();

            for(int i = 0; i < iterations; i++)
            {
                var state = new List<Geo.Line>();
                foreach (tAgent agent in _agents)
                {
                    _test.Add(agent.Step(_agents));
                    state.Add(agent.GetMemberLines(_agents)[0]);
                    if (!agent._isNaked)
                    {
                        state.Add(agent.GetMemberLines(_agents)[1]);
                    }
                }
                states.Add(state);
            }
        }


        /// <summary>
        /// 
        /// </summary>
        internal void CreateAgents()
        {
            var agentsDict = new Dictionary<HashSet<int>, tAgent>(HashSet<int>.CreateSetComparer());

            var indices = _mesh.VertexIndicesByTri();
            var faces = SplitList(indices);
            var normals = _mesh.TriangleNormals();

            // Loop through every face
            for (int i = 0; i < faces.Count; i++)
            {
                for (int j = 0; j < faces[i].Count; j++)
                {
                    int start = faces[i][j];
                    int end = faces[i][(j + 1) % faces[i].Count];
                    var key = new HashSet<int> { start, end };

                    if (!agentsDict.ContainsKey(key))
                    {
                        agentsDict[key] = new tAgent(agentsDict.Count);
                        agentsDict[key]._edge = Geo.Line.ByStartPointEndPoint(_mesh.Vertices()[start], _mesh.Vertices()[end]);
                    }
                }
            }

            // Loop through every face
            for (int i = 0; i < faces.Count; i++)
            {
                // Loop through every edge of every face
                for (int j = 0; j < faces[i].Count; j++)
                {
                    int start = faces[i][j];
                    int end = faces[i][(j + 1) % faces[i].Count];
                    int third = faces[i][(j + 2) % faces[i].Count];
                    var key = new HashSet<int> { start, end };

                    if (agentsDict[key]._faceIndexA != -1)
                    {
                        agentsDict[key]._isNaked = false;
                        agentsDict[key]._faceIndexB = i;
                        agentsDict[key]._faceNormalB = normals[i];
                        var neighborsB1 = agentsDict[new HashSet<int> { start, third }]._name;
                        var neighborsB2 = agentsDict[new HashSet<int> { end, third }]._name;
                        agentsDict[key]._neighborsB = new int[] { neighborsB1, neighborsB2 };
                    }
                    else
                    {
                        agentsDict[key]._faceIndexA = i;
                        agentsDict[key]._faceNormalA = normals[i];
                        var neighborsA1 = agentsDict[new HashSet<int> { start, third }]._name;
                        var neighborsA2 = agentsDict[new HashSet<int> { end, third }]._name;
                        agentsDict[key]._neighborsA = new int[] { neighborsA1, neighborsA2 };
                    }
                }
            }

            _agents = agentsDict.Values.ToArray();

            foreach (tAgent agent in _agents)
            {
                agent.Setup();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="mesh"></param>
        /// <param name="iterations"></param>
        /// <returns></returns>
        public static List<List<Geo.Line>> FromMesh(Mesh mesh, int iterations)
        {
            var solver = new hTriangleStrategy(mesh, iterations);
            return solver.states;
        }


        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public List<Geo.Point> AgentsAsPoints()
        {
            var agents = new List<Geo.Point>();
            foreach (tAgent agent in _agents)
            {
                agents.Add(agent._edge.PointAtParameter(agent._currentParameter));
            }
            return agents;
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
                throw new System.ArgumentException("List length must be divisible by split size", "list");          
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
