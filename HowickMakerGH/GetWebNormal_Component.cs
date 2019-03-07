using System;
using System.Collections.Generic;

using Grasshopper.Kernel;
using Rhino.Geometry;

using HM = HowickMaker;

namespace HowickMakerGH
{
    public class GetWebNormal_Component : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the GetWebNormal_Component class.
        /// </summary>
        public GetWebNormal_Component()
          : base("Get Web Normal", "Normal",
              "Get the vector that is normal to the web of the given member.",
              "HowickMaker", "Member")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("Member", "M", "Member from which to get normal.", GH_ParamAccess.item);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddVectorParameter("Normal", "N", "Member web normal.", GH_ParamAccess.item);
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

            var webNormal = member.WebNormal;

            DA.SetData(0, HMGHUtil.TripleToVector(webNormal));
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
            get { return new Guid("c488eec2-dcdc-4803-a5e7-c1b9fffb3dd0"); }
        }
    }
}