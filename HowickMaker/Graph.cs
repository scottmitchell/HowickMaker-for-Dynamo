using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HowickMaker
{
    /// <summary>
    /// Super simple graph for keeping track of member adjacencies
    /// </summary>
    class Graph
    {
        internal Vertex[] vertices;
        private Queue<int> _queue;

        public Graph(int c)
        {
            this.vertices = new Vertex[c];
            for (int i = 0; i < c; i++)
            {
                vertices[i] = new Vertex(i);
            }
            _queue = new Queue<int>();
        }

        /// <summary>
        /// Reset this graph and its vertices
        /// </summary>
        internal void Reset()
        {
            foreach (Vertex v in vertices)
            {
                v.visited = false;
            }
            _queue = new Queue<int>();
        }

        /// <summary>
        /// Get the index of one node per each subgraph in this graph
        /// </summary>
        /// <returns></returns>
        internal List<int> GetStartingNodes()
        {
            // Initialize list of starting nodes
            var startingNodes = new List<int>();

            // Keep track of how many nodes we have visited
            var numVisited = 0;

            // While not all nodes have been visited
            while (numVisited < vertices.Length)
            {
                var current = vertices[0];
                // Find the next unvisited node
                foreach (Vertex v in vertices)
                {
                    if (!v.visited)
                    {
                        current = v;
                        break;
                    }
                }
                // This node hasn't been visited, and must be the start of a new subgraph
                startingNodes.Add(current.name);

                // Visit all nodes in this subgraph
                numVisited += Visit(current, 0);
            }
            Reset();
            return startingNodes;
        }

        /// <summary>
        /// Visits all nodes in the subgraph containing current
        /// </summary>
        /// <param name="current"></param>
        /// <param name="numVisited"></param>
        /// <returns>Number of nodes in the subgraph containing current</returns>
        internal int Visit(Vertex current, int numVisited)
        {
            // Add neighbors to queue if they haven't been added or visited
            foreach (int n in current.neighbors)
            {
                if (!_queue.Contains(n) && !vertices[n].visited)
                {
                    _queue.Enqueue(n);
                }
            }

            // Current node has been visited
            current.visited = true;
            numVisited ++;

            // Visit the next node in the subgraph
            if (_queue.Count > 0)
            {
                var next = _queue.Dequeue();
                numVisited = Visit(vertices[next], numVisited);
            }
            return numVisited;
        }
    }
}
