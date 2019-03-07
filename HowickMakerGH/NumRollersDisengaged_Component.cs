using System;
using System.Collections.Generic;

using Grasshopper.Kernel;
using Rhino.Geometry;
using HM = HowickMaker;

namespace HowickMakerGH
{
    public class NumDisengagedRollers_Component : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the NumDisengagedRollers_Component class.
        /// </summary>
        public NumDisengagedRollers_Component()
          : base("Number Of Disengaged Rollers", "Rollers",
              "Find out the maximum number of rollers that would be disengaged during the run of these members on the Howick machine. 4 or more means trouble.",
              "HowickMaker", "Utility")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("Members", "M", "List of members to check.", GH_ParamAccess.list);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddNumberParameter("Number Disengaged", "D", "Maximum number of rollers disengaged.", GH_ParamAccess.item);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            List<HM.hMember> members = new List<HM.hMember>();

            if (!DA.GetDataList(0, members)) { return; }

            var maxNumDisengaged = HM.hUtility.CheckMaxNumRollersDisengaged(members);

            DA.SetData(0, maxNumDisengaged);
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
            get { return new Guid("93dd7b84-326d-4277-bd9f-7e5021d039b9"); }
        }
    }
}