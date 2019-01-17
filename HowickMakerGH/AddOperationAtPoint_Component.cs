using System;
using System.Collections.Generic;

using Grasshopper.Kernel;
using Rhino.Geometry;
using HM = HowickMaker;

namespace HowickMakerGH
{
    public class AddOperationAtPoint_Component : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the AddOperationAtPoint_Component class.
        /// </summary>
        public AddOperationAtPoint_Component()
          : base("AddOperationsAtPoints", "AddOps",
              "Add operations to the member at the specified points.",
              "HowickMaker", "Member")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("Member", "M", "member", GH_ParamAccess.item);
            pManager.AddPointParameter("Points", "P", "list of operation points", GH_ParamAccess.list);
            pManager.AddTextParameter("Types", "T", "list of operation types", GH_ParamAccess.list);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("Member", "M", "member", GH_ParamAccess.item);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            HM.hMember member = null;
            List<Point3d> points = new List<Point3d>();
            List<string> types = new List<string>();

            if (!DA.GetData(0, ref member)) { return; }
            if (!DA.GetDataList(1, points)) { return; }
            if (!DA.GetDataList(2, types)) { return; }

            if (points.Count != types.Count)
            {
                return;
            }

            var newMember = new HM.hMember(member);
            for (int i = 0; i < points.Count; i++)
            {
                newMember.AddOperationByPointType(HMGHUtil.PointToTriple(points[i]), types[i]);
            }

            DA.SetData(0, newMember);
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
            get { return new Guid("0d4e4406-5a28-4ccc-bb8b-6305a6a3a4e7"); }
        }
    }
}