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
          : base("Structure From Lines", "FromLines",
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
            pManager.AddTextParameter("Names", "N", "names of members", GH_ParamAccess.list);
            pManager[1].Optional = true;
            pManager.AddBooleanParameter("First Connection Boolean", "B", "flip first connection", GH_ParamAccess.item);
            pManager[2].Optional = true;
            pManager.AddGenericParameter("Normal Dictionary", "N", "Dictionary<name, normal(Vector3d)> -- Use 'CreateWebNormalsDictionary' component", GH_ParamAccess.item);
            pManager[3].Optional = true;
            pManager.AddGenericParameter("Priority Dictionary", "P", "Dictionary<name, priority(int)> -- Use 'CreatePriotityDictionary' component", GH_ParamAccess.item);
            pManager[4].Optional = true;
            pManager.AddGenericParameter("Extension Dictionary", "E", "Dictionary<name, extensions(int)> -- Use 'CreateExtensionDictionary' component", GH_ParamAccess.item);
            pManager[5].Optional = true;
            pManager.AddNumberParameter("Tolerance", "T", "tolerance for solver", GH_ParamAccess.item);
            pManager[6].Optional = true;
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("Members", "M", "members", GH_ParamAccess.list);
            pManager.AddNumberParameter("Order", "O", "solve order", GH_ParamAccess.list);
            pManager.AddNumberParameter("SolvedBy", "S", "solved by", GH_ParamAccess.list);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {

            // Declare placeholder variables and assign initial invalid data.
            List<Line> inputLines = new List<Line>();
            List<string> names = new List<string>();
            bool firstConnectionIsFTF = false;
            Dictionary<string, HM.Triple> webNormalsDictionary = null;
            Dictionary<string, int> prioritiesDictionary = null;
            Dictionary<string, int> extensionDictionary = null;
            double tolerance = 0.001;

            // Retrieve input data.
            if (!DA.GetDataList(0, inputLines)) { return; }
            if (!DA.GetDataList(1, names)) { return; }
            if (!DA.GetData(2, ref firstConnectionIsFTF)) { firstConnectionIsFTF = false; }
            if (!DA.GetData(3, ref webNormalsDictionary)) { webNormalsDictionary = null; }
            if (!DA.GetData(4, ref prioritiesDictionary)) { prioritiesDictionary = null; }
            if (!DA.GetData(5, ref extensionDictionary)) { extensionDictionary = null; }
            if (!DA.GetData(6, ref tolerance)) { tolerance = 0.001; }

            // Convert GH lines to HM lines
            var HMLines = new List<HM.Line>();
            var i = 0;
            foreach (Line l in inputLines)
            {
                HMLines.Add(HMGHUtil.GHLineToHMLine(l));
            }

            // Create Structure
            var structure = HM.hStructure.StructureFromLines(
                HMLines, 
                names, 
                webNormalsDict: webNormalsDictionary,
                priorityDict: prioritiesDictionary,
                extensionDict: extensionDictionary,
                firstConnectionIsFTF: firstConnectionIsFTF,
                intersectionTolerance: tolerance,
                planarityTolerance: tolerance);

            DA.SetDataList(0, structure.Members);
            DA.SetDataList(1, structure._solveOrder);
            DA.SetDataList(2, structure._solvedBy);
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