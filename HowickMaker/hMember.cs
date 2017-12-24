using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Geo = Autodesk.DesignScript.Geometry;
using Autodesk.DesignScript.Interfaces;
using Autodesk.DesignScript.Runtime;

namespace HowickMaker
{
    public class hMember : IGraphicItem
    {
        internal Geo.Line webAxis;
        internal Geo.Vector webNormal;
        internal int name;
        public List<hConnection> connections;
        public List<hOperation> operations;


        //   ██████╗ ██████╗ ███╗   ██╗███████╗████████╗██████╗ 
        //  ██╔════╝██╔═══██╗████╗  ██║██╔════╝╚══██╔══╝██╔══██╗
        //  ██║     ██║   ██║██╔██╗ ██║███████╗   ██║   ██████╔╝
        //  ██║     ██║   ██║██║╚██╗██║╚════██║   ██║   ██╔══██╗
        //  ╚██████╗╚██████╔╝██║ ╚████║███████║   ██║   ██║  ██║
        //   ╚═════╝ ╚═════╝ ╚═╝  ╚═══╝╚══════╝   ╚═╝   ╚═╝  ╚═╝
        //                                                      


        internal hMember()
        {
            
        }

        public hMember(Geo.Line webAxis, Geo.Vector webNormal, int name = 0)
        {
            this.webAxis = webAxis;
            this.webNormal = webNormal;
            this.name = name;
        }

        public hMember(Geo.Line webAxis, int name = 0)
        {
            this.webAxis = webAxis;
            this.webNormal = null;
            this.name = name;
        }



        internal void addOperation(hOperation operation)
        {
            this.operations.Add(operation);
        }



        //  ███████╗██╗  ██╗██████╗  ██████╗ ██████╗ ████████╗
        //  ██╔════╝╚██╗██╔╝██╔══██╗██╔═══██╗██╔══██╗╚══██╔══╝
        //  █████╗   ╚███╔╝ ██████╔╝██║   ██║██████╔╝   ██║   
        //  ██╔══╝   ██╔██╗ ██╔═══╝ ██║   ██║██╔══██╗   ██║   
        //  ███████╗██╔╝ ██╗██║     ╚██████╔╝██║  ██║   ██║   
        //  ╚══════╝╚═╝  ╚═╝╚═╝      ╚═════╝ ╚═╝  ╚═╝   ╚═╝   
        //                                   
        

        public static void exportToFile(string filePath, List<hMember> hMembers)
        {
            
        }








        //  ██╗   ██╗██╗███████╗
        //  ██║   ██║██║╚══███╔╝
        //  ██║   ██║██║  ███╔╝ 
        //  ╚██╗ ██╔╝██║ ███╔╝  
        //   ╚████╔╝ ██║███████╗
        //    ╚═══╝  ╚═╝╚══════╝
        //                      

        public static Geo.Mesh draw(hMember member)
        {
            Geo.Point OP1 = member.webAxis.StartPoint;
            Geo.Point OP2 = member.webAxis.EndPoint;

            Geo.Vector webAxis = Geo.Vector.ByTwoPoints(OP1, OP2);
            Geo.Vector normal = member.webNormal;
            Geo.Vector lateral = webAxis.Cross(normal);
            lateral = lateral.Normalized();
            lateral = lateral.Scale(1.75);
            normal = normal.Normalized();
            normal = normal.Scale(1.5);
            Geo.Vector lateralR = Geo.Vector.ByCoordinates(lateral.X * -1, lateral.Y * -1, lateral.Z * -1);

            Geo.Point p0 = OP1.Add(normal.Add(lateral));
            Geo.Point p1 = OP2.Add(normal.Add(lateral));
            Geo.Point p2 = OP1.Add(lateral);
            Geo.Point p3 = OP2.Add(lateral);
            Geo.Point p6 = OP1.Add(normal.Add(lateralR));
            Geo.Point p7 = OP2.Add(normal.Add(lateralR));
            Geo.Point p4 = OP1.Add(lateralR);
            Geo.Point p5 = OP2.Add(lateralR);

            Geo.Point[] pts = { p0, p1, p2,  p1, p2, p3,  p2, p3, p4,  p3, p4, p5,  p4, p5, p6,  p5, p6, p7 };


            Geo.IndexGroup g0 = Geo.IndexGroup.ByIndices(0, 1, 2);
            Geo.IndexGroup g1 = Geo.IndexGroup.ByIndices(3, 4, 5);
            Geo.IndexGroup g2 = Geo.IndexGroup.ByIndices(6, 7, 8);
            Geo.IndexGroup g3 = Geo.IndexGroup.ByIndices(9, 10, 11);
            Geo.IndexGroup g4 = Geo.IndexGroup.ByIndices(12, 13, 14);
            Geo.IndexGroup g5 = Geo.IndexGroup.ByIndices(15, 16, 17);

            Geo.IndexGroup[] ig = { g0, g1, g2, g3, g4, g5 };

            Geo.Mesh mesh = Geo.Mesh.ByPointsFaceIndices(pts, ig);

            return mesh;
        }


        #region IGraphicItem Interface

        [IsVisibleInDynamoLibrary(false)]
        public void Tessellate(IRenderPackage package, TessellationParameters parameters)
        {
            // This example contains information to draw a point
            package.AddPointVertex(0,0,0);
            package.AddPointVertexColor(255, 0, 0, 255);
        }
 
        #endregion
    }
}
