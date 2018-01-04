using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HowickMaker
{
    internal class hFace
    {
        internal List<hVertex> vertices = new List<hVertex>();
        internal bool visited = false;

        internal hFace(List<hVertex> vertices)
        {
            this.vertices = vertices;
        }



        public override string ToString()
        {
            string s = "F:: \n";
            foreach (hVertex v in vertices)
            {
                s += v.ToString();
                s += "\n";
            }
            return s;
        }
    }
}
