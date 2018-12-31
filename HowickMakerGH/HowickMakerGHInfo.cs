using System;
using System.Drawing;
using Grasshopper.Kernel;

namespace HowickMakerGH
{
    public class HowickMakerGHInfo : GH_AssemblyInfo
    {
        public override string Name
        {
            get
            {
                return "HowickMaker";
            }
        }
        public override Bitmap Icon
        {
            get
            {
                //Return a 24x24 pixel bitmap to represent this GHA library.
                return null;
            }
        }
        public override string Description
        {
            get
            {
                //Return a short string describing the purpose of this GHA library.
                return "HowickMaker is a library for parametrically designing for Howick steel-stud roll-forming machines";
            }
        }
        public override Guid Id
        {
            get
            {
                return new Guid("d127195c-a4bb-4812-9253-855d33105a8c");
            }
        }

        public override string AuthorName
        {
            get
            {
                //Return a string identifying you or your company.
                return "Scott Mitchell";
            }
        }
        public override string AuthorContact
        {
            get
            {
                //Return a string representing your preferred contact details.
                return "";
            }
        }
    }
}
