using System;
using System.Collections.Generic;

using Grasshopper.Kernel;
using Rhino.Geometry;

namespace HowickMakerGH
{
    public class CreateExtensionDictionary_Component : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the CreatePriorityDictionary_Component class.
        /// </summary>
        public CreateExtensionDictionary_Component()
          : base("Create Extension Dictionary", "Extensions",
              "Create a dictionary of Face-To-Face Joint extension types to use with structure solver",
              "HowickMaker", "Structure")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddTextParameter("Names", "N", "Names of members to associate extension types with", GH_ParamAccess.list);
            pManager.AddIntegerParameter("Extensions", "E", "Extension types of members", GH_ParamAccess.list);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("Extension Dictionary", "E", "Dictionary<string, int>", GH_ParamAccess.item);
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

            // Get extension types
            var extensions = new List<int>();
            if (!DA.GetDataList(1, extensions)) { return; }

            // There should be the same number of names and normals
            if (names.Count != extensions.Count) { return; }

            // Create dictionary
            var dictionary = new Dictionary<string, int>();
            for (int i = 0; i < names.Count; i++)
            {
                dictionary[names[i]] = extensions[i];
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
            get { return new Guid("6c7a7208-2bb1-4293-88c4-db762f973873"); }
        }
    }
}