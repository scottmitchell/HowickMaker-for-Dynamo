using System;
using System.Collections.Generic;

using Grasshopper.Kernel;
using Rhino.Geometry;

using HM = HowickMaker;

namespace HowickMakerGH
{
    public class MemberOperationDisplay_Component : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the MemberOperationDisplay_Component class.
        /// </summary>
        public MemberOperationDisplay_Component()
          : base("Operation Curves", "OpCrvs",
              "Get curves represetning member operations",
              "HowickMaker", "Member")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("Member", "M", "member", GH_ParamAccess.item);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddCurveParameter("Curve", "C", "Operations as Curves", GH_ParamAccess.list);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            // Get member from input
            HM.hMember member = null;
            if (!DA.GetData(0, ref member)) { return; }

            // Get member properties as Rhino Geometry
            var webAxisLine = HMGHUtil.HMLineToGHLine(member.WebAxis);
            var webAxisDirection = HMGHUtil.TripleToVector(member.WebAxis.Direction);
            var webNormal = HMGHUtil.TripleToVector(member.WebNormal.Normalized());
            var flangeNormal = HMGHUtil.TripleToVector(member.WebNormal.Cross(member.WebAxis.Direction).Normalized());

            // Get operations as curves
            var opCurves = new List<Curve>();
            foreach (HM.hOperation op in member.Operations)
            {
                // Get centerpoints
                var centerTriples = member.GetOperationPunchCenterPoint(op);
                var centerPoints = new List<Point3d>();
                foreach (HM.Triple t in centerTriples)
                {
                    centerPoints.Add(HMGHUtil.TripleToPoint(t));
                }

                switch (op._type)
                {
                    case HM.Operation.BOLT:
                        opCurves.Add(new Circle(new Plane(centerPoints[0], webNormal), 0.25).ToNurbsCurve());
                        break;
                    case HM.Operation.SERVICE_HOLE:
                        opCurves.Add(new Circle(new Plane(centerPoints[0], webNormal), 0.5).ToNurbsCurve());
                        break;
                    case HM.Operation.WEB:
                        foreach (Point3d p in centerPoints)
                        {
                            opCurves.Add(new Circle(new Plane(p, webNormal), 0.0625).ToNurbsCurve());
                        }
                        break;
                    case HM.Operation.DIMPLE:
                        foreach (Point3d p in centerPoints)
                        {
                            opCurves.Add(new Circle(new Plane(p, flangeNormal), 0.0625).ToNurbsCurve());
                        }
                        break;
                    case HM.Operation.NOTCH:
                        opCurves.Add(new Rectangle3d(new Plane(centerPoints[0], flangeNormal, webAxisDirection), new Interval(-1.75, 1.75), new Interval(-1.75 / 2, 1.75 / 2)).ToNurbsCurve());
                        break;
                    case HM.Operation.LIP_CUT:
                        foreach (Point3d p in centerPoints)
                        {
                            opCurves.Add(new Rectangle3d(new Plane(p, flangeNormal, webAxisDirection), new Interval(-0.25, 0.25), new Interval(-1.75/2, 1.75/2)).ToNurbsCurve());
                        }
                        break;
                }
            }
            DA.SetDataList(0, opCurves);

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
            get { return new Guid("dc407b21-0f84-498f-b62f-5c09efd85f2c"); }
        }
    }
}