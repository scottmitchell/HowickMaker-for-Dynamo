using System;
using System.Collections.Generic;

using Grasshopper.Kernel;
using Rhino.Geometry;

using HM = HowickMaker;

namespace HowickMakerGH
{
    public class CreateWebNormalsDictionary_Component : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the CreateWebNormalsDictionary_Component class.
        /// </summary>
        public CreateWebNormalsDictionary_Component()
          : base("Create WebNormals Dictionary", "webNormals",
              "Create a dictionary of web normal vectors to use with structure solver",
              "HowickMaker", "Structure")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddTextParameter("Names", "N", "Names of members to associate web normals with", GH_ParamAccess.list);
            pManager.AddVectorParameter("Vectors", "V", "Web normals of members", GH_ParamAccess.list);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("Normals Dictionary", "N", "Dictionary<string, vector>", GH_ParamAccess.item);
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

            // Get normals
            var vectors = new List<Vector3d>();
            if (!DA.GetDataList(1, vectors)) { return; }

            // There should be the same number of names and normals
            if (names.Count != vectors.Count) { return; }
            
            // Create dictionary
            var dictionary = new Dictionary<string, HM.Triple>();
            for (int i = 0; i < names.Count; i++)
            {
                dictionary[names[i]] = HMGHUtil.VectorToTriple(vectors[i]);
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
            get { return new Guid("02d59d75-e509-4276-88f1-580783a32c84"); }
        }
    }
}