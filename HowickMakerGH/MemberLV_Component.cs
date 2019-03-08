using System;
using System.Collections.Generic;

using Grasshopper.Kernel;
using Rhino.Geometry;

using HM = HowickMaker;

// In order to load the result of this wizard, you will also need to
// add the output bin/ folder of this project to the list of loaded
// folder in Grasshopper.
// You can use the _GrasshopperDeveloperSettings Rhino command for that.

namespace HowickMakerGH
{
    public class MemberLV_Component : GH_Component
    {
        /// <summary>
        /// Each implementation of GH_Component must provide a public 
        /// constructor without any arguments.
        /// Category represents the Tab in which the component will appear, 
        /// Subcategory the panel. If you use non-existing tab or panel names, 
        /// new tabs/panels will automatically be created.
        /// </summary>
        public MemberLV_Component()
          : base("Create Member", "Create",
              "Create a member from a line and a vector.",
              "HowickMaker", "Member")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddLineParameter("Line", "L", "The line representing the web axis of the member.", GH_ParamAccess.item);
            pManager.AddVectorParameter("Vector", "V", "The vector representing the web normal of the member.", GH_ParamAccess.item);
            pManager.AddTextParameter("Name", "N", "The name of the member.", GH_ParamAccess.item);
            pManager[2].Optional = true;
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("Member", "M", "Steel stud member.", GH_ParamAccess.item);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object can be used to retrieve data from input parameters and 
        /// to store data in output parameters.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            // 1. Declare placeholder variables and assign initial invalid data.
            //    This way, if the input parameters fail to supply valid data, we know when to abort.
            Line line = Line.Unset;
            Vector3d vector = Vector3d.Unset;
            string name = string.Empty;

            // 2. Retrieve input data.
            if (!DA.GetData(0, ref line)) { return; }
            if (!DA.GetData(1, ref vector)) { return; }
            if (!DA.GetData(2, ref name)) { name = "<name>"; }

            // 3. Abort on invalid inputs.
            if (!line.IsValid) { return; }
            if (!vector.IsValid) { return; }

            // 4. Build hMember.
            var mem = new HM.hMember(HMGHUtil.GHLineToHMLine(line), HMGHUtil.VectorToTriple(vector));
            mem.Name = name;

            // 9. Assign output.
            DA.SetData(0, mem);
        }


        /// <summary>
        /// Provides an Icon for every component that will be visible in the User Interface.
        /// Icons need to be 24x24 pixels.
        /// </summary>
        protected override System.Drawing.Bitmap Icon
        {
            get
            {
                // You can add image files to your project resources and access them like this:
                //return Resources.IconForThisComponent;
                return null;
            }
        }

        /// <summary>
        /// Each component must have a unique Guid to identify it. 
        /// It is vital this Guid doesn't change otherwise old ghx files 
        /// that use the old ID will partially fail during loading.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("7023886c-2de6-48e6-ae34-aeae752479f2"); }
        }
    }
}
