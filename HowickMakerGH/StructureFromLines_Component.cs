using System;
using System.Collections.Generic;

using Grasshopper.Kernel;
using Rhino.Geometry;
using HM = HowickMaker;

namespace HowickMakerGH
{
    public class StructureFromLines_Component : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the StructureFromLines_Component class.
        /// </summary>
        public StructureFromLines_Component()
          : base("StructureFromLines_Component", "FromLines",
              "Get members from a network of lines",
              "HowickMaker", "Structure")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddLineParameter("Lines", "L", "webAxis", GH_ParamAccess.list);
            pManager.AddBooleanParameter("FirstConnection", "B", "flip first connection", GH_ParamAccess.item);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("Members", "M", "members", GH_ParamAccess.list);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {

            // 1. Declare placeholder variables and assign initial invalid data.
            //    This way, if the input parameters fail to supply valid data, we know when to abort.
            List<Line> inputLines = new List<Line>();

            // 2. Retrieve input data.
            if (!DA.GetDataList(0, inputLines)) { return; }

            var HMLines = new List<HM.Line>();
            List<string> names = new List<string>();
            var i = 0;
            foreach (Line l in inputLines)
            {
                HMLines.Add(HMGHUtil.GHLineToHMLine(l));
                names.Add(i++.ToString());
            }

            

            var structure = HM.hStructure.StructureFromLines(HMLines, names);

            DA.SetDataList(0, structure.Members);
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
            get { return new Guid("bfac9ecc-aa9f-4a7d-8378-507d23c1a6f1"); }
        }
    }
}