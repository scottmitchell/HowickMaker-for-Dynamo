using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HowickMaker
{
    /// <summary>
    /// Super simple vertex for use with super simple graph for keeping track of member adjacencies
    /// </summary>
    class Vertex
    {
        internal int name;
        internal List<int> neighbors;
        internal bool visited;

        internal Vertex(int name)
        {
            this.name = name;
            this.neighbors = new List<int>();
            this.visited = false;
        }

        internal void addNeighbor(int n)
        {
            neighbors.Add(n);
        }
    }
}
