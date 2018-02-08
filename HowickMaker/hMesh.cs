using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Geo = Autodesk.DesignScript.Geometry;
using Autodesk.DesignScript.Runtime;

namespace HowickMaker
{
    [IsVisibleInDynamoLibrary(false)]
    public class hMesh
    {
        internal List<hFace> faces = new List<hFace>();

        internal hMesh(List<hFace> faces)
        {
            this.faces = faces;
        }


        public static hMesh hMeshFromOBJ(string filepath)
        {
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
            
            // Get Faces
            List<hFace> faces = new List<hFace>();
            foreach (string line in lines)
            {
                if (line.Length > 0 && line[0] == 'f')
                {
                    string[] values = line.Split(' ');

                    List<hVertex> verts = new List<hVertex>();
                    for (int j = 1; j < values.Length; j++)
                    {
                        int index = int.Parse(values[j].Split('/')[0]);
                        verts.Add(vertices[index-1]);
                    }
                    
                    hFace f = new hFace(verts);
                    faces.Add(f);
                }
            }

            return new hMesh(faces);
        }



        internal int GetAdjacentFaceIndex(hFace currentFace, int edge)
        {
            hVertex edgeV1 = currentFace.vertices[edge];
            hVertex edgeV2 = currentFace.vertices[(edge + 1) % currentFace.vertices.Count];

            for (int i = 0; i < faces.Count; i++)
            {
                if (faces[i] != currentFace)
                {
                    if (faces[i].vertices.Contains(edgeV1) && faces[i].vertices.Contains(edgeV2))
                    {
                        return i;
                    }
                }
            }

            return -1;
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
