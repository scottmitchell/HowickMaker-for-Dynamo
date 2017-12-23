using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HowickMaker
{
    public class hMesh
    {
        internal List<hFace> faces = new List<hFace>();

        internal hMesh(List<hFace> faces)
        {
            this.faces = faces;
        }


        public static hMesh hMeshFromOBJ(string filepath)
        {

            int[] rotate = { 0, 1, 0, 2, 1, 0, 1, 1, 0, 2, 2, 0, 1, 1 };
            string[] lines = System.IO.File.ReadAllLines(filepath);


            // Get Vertices
            List<hVertex> vertices = new List<hVertex>();
            foreach (string line in lines)
            {
                if (line.Length > 0 && line[0] == 'v')
                {
                    string[] values = line.Split(' ');
                    double x = Double.Parse(values[1]);
                    double y = Double.Parse(values[2]);
                    double z = Double.Parse(values[3]);
                    hVertex v = new hVertex(x, y, z);
                    vertices.Add(v);
                }
            }

            int i = 0;
            // Get Faces
            List<hFace> faces = new List<hFace>();
            foreach (string line in lines)
            {
                if (line.Length > 0 && line[0] == 'f')
                {
                    string[] values = line.Split(' ');
                    string[] values2 = { values[1], values[2], values[3]};

                    int a = int.Parse(values2[(0+rotate[i])%3].Split('/')[0]);
                    int b = int.Parse(values2[(1 + rotate[i]) % 3].Split('/')[0]);
                    int c = int.Parse(values2[(2 + rotate[i]) % 3].Split('/')[0]);
                    List<hVertex> verts = new List<hVertex>();
                    verts.Add(vertices[a-1]);
                    verts.Add(vertices[b-1]);
                    verts.Add(vertices[c-1]);

                    hFace f = new hFace(verts);
                    faces.Add(f);
                    i++;
                }
            }


            return new hMesh(faces);
        }

        public static string view(hMesh m)
        {
            return m.ToString();
        }


        public override string ToString()
        {
            string s = "";
            foreach (hFace f in faces)
            {
                s += f.ToString();
            }
            return s;
        }
    }
}
