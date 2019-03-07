using System;
using System.Collections.Generic;

using Grasshopper.Kernel;
using Rhino.Geometry;
using HM = HowickMaker;

namespace HowickMakerGH
{
    public class MemberDisplay_Component : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the MemberDisplay_Component class.
        /// </summary>
        public MemberDisplay_Component()
          : base("Display Member", "MemGeo",
              "Get a member as a brep.",
              "HowickMaker", "Member")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("Member", "M", "Member to get as brep.", GH_ParamAccess.item);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddBrepParameter("Brep", "B", "Member brep.", GH_ParamAccess.item);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            // 1. Declare placeholder variables and assign initial invalid data.
            //    This way, if the input parameters fail to supply valid data, we know when to abort.
            HM.hMember member = null;

            // 2. Retrieve input data.
            if (!DA.GetData(0, ref member)) { return; }

            // 3. Abort on invalid inputs.
            //if (!member.IsValid) { return; }

            var webAxisLine = member.WebAxis;
            var webNormal = member.WebNormal;

            var OP1 = webAxisLine.StartPoint;
            var OP2 = webAxisLine.EndPoint;

            var webAxis = HM.Triple.ByTwoPoints(OP1, OP2);
            var normal = webNormal;
            var lateral = webAxis.Cross(normal);
            lateral = lateral.Normalized();
            lateral = lateral.Scale(1.75);
            normal = normal.Normalized();
            normal = normal.Scale(1.5);
            var lateralR = new HM.Triple(lateral.X * -1, lateral.Y * -1, lateral.Z * -1);
            var webAxisR = new HM.Triple(webAxis.X * -1, webAxis.Y * -1, webAxis.Z * -1);
            var normalR = new HM.Triple(normal.X * -1, normal.Y * -1, normal.Z * -1);

            var p0 = HMGHUtil.TripleToPoint(OP1.Add(normal.Add(lateral)));
            var p1 = HMGHUtil.TripleToPoint(OP2.Add(normal.Add(lateral)));
            var p2 = HMGHUtil.TripleToPoint(OP1.Add(lateral));
            var p3 = HMGHUtil.TripleToPoint(OP2.Add(lateral));
            var p6 = HMGHUtil.TripleToPoint(OP1.Add(normal.Add(lateralR)));
            var p7 = HMGHUtil.TripleToPoint(OP2.Add(normal.Add(lateralR)));
            var p4 = HMGHUtil.TripleToPoint(OP1.Add(lateralR));
            var p5 = HMGHUtil.TripleToPoint(OP2.Add(lateralR));
            lateral = lateral.Normalized().Scale(1.25);
            lateralR = lateralR.Normalized().Scale(1.25);
            var p8 = HMGHUtil.TripleToPoint(OP1.Add(lateralR).Add(normal));
            var p9 = HMGHUtil.TripleToPoint(OP2.Add(lateralR).Add(normal));
            var p10 = HMGHUtil.TripleToPoint(OP1.Add(lateral).Add(normal));
            var p11 = HMGHUtil.TripleToPoint(OP2.Add(lateral).Add(normal));


            var flange1 = NurbsSurface.CreateFromCorners(p0, p1, p3, p2 );
            var flange2 = NurbsSurface.CreateFromCorners(p4, p5, p7, p6 );
            var web = NurbsSurface.CreateFromCorners(p2, p3, p5, p4 );
            var lip1 = NurbsSurface.CreateFromCorners(p0, p1, p11, p10 );
            var lip2 = NurbsSurface.CreateFromCorners(p6, p7, p9, p8 );

            var memGeo = new List<Brep>() { Brep.CreateFromSurface(flange1), Brep.CreateFromSurface(flange2), Brep.CreateFromSurface(web), Brep.CreateFromSurface(lip1), Brep.CreateFromSurface(lip2) };
            var brep = Brep.JoinBreps(memGeo, 0.001)[0];

            DA.SetData(0, brep);
        }

        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
        protected override System.Drawing.Bitmap Icon
        {
            get
            {
                //You can add image files to your project resources and access them like this:
                // return Resources.IconForThisComponent;
                return null;
            }
        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("ac31d227-f11d-4175-a2b4-c2f5174fd556"); }
        }
    }
}