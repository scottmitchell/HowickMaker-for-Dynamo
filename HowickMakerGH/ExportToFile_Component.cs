using System;
using System.Collections.Generic;

using Grasshopper.Kernel;
using Rhino.Geometry;
using HM = HowickMaker;

namespace HowickMakerGH
{
    public class ExportToFile_Component : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the ExportToFile_Component class.
        /// </summary>
        public ExportToFile_Component()
          : base("ExportToFile", "Export",
              "Export members to a csv file",
              "HowickMaker", "Member")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("Members", "M", "members", GH_ParamAccess.list);
            pManager.AddTextParameter("Filepath", "F", "filepath", GH_ParamAccess.item);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            List<HM.hMember> members = new List<HM.hMember>();
            string path = null;

            if (!DA.GetDataList(0, members)) { return; }
            if (!DA.GetData(1, ref path)) { return; }

            HM.hMember.ExportToFile(path, members);
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
            get { return new Guid("e351524c-7531-4646-999f-d67fb676fe93"); }
        }
    }
}