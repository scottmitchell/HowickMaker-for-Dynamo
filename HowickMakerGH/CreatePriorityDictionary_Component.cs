using System;
using System.Collections.Generic;

using Grasshopper.Kernel;
using Rhino.Geometry;

namespace HowickMakerGH
{
    public class CreatePriorityDictionary_Component : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the CreatePriorityDictionary_Component class.
        /// </summary>
        public CreatePriorityDictionary_Component()
          : base("Create Priority Dictionary", "Priorities",
              "Create a dictionary of priorities to use with structure solver",
              "HowickMaker", "Structure")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddTextParameter("Names", "N", "Names of members to associate priorities with", GH_ParamAccess.list);
            pManager.AddIntegerParameter("Priorities", "P", "Priorities of members. Higher numbers are higher priority", GH_ParamAccess.list);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("Priorities Dictionary", "P", "Dictionary<string, int>", GH_ParamAccess.item);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            // Get names
            var names = new List<string>();
            if (!DA.GetDataList(0, names)) { return; }

            // Get priorities
            var priorities = new List<int>();
            if (!DA.GetDataList(1, priorities)) { return; }

            // There should be the same number of names and normals
            if (names.Count != priorities.Count) { return; }

            // Create dictionary
            var dictionary = new Dictionary<string, int>();
            for (int i = 0; i < names.Count; i++)
            {
                dictionary[names[i]] = priorities[i];
            }
            DA.SetData(0, dictionary);
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
            get { return new Guid("1e2ec8a4-48ea-4ea9-9f0a-95a7de0e24d8"); }
        }
    }
}