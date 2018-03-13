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

        public Graph(int c)
        {
            this.vertices = new Vertex[c];
            for (int i = 0; i < c; i++)
            {
                vertices[i] = new Vertex(i);
            }
        }


    }
}
