using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HM = HowickMaker;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Types;
using GH_IO.Serialization;

namespace HowickMakerGH
{
    internal class ParamMember : GH_Param<GooMember>
    {
        public ParamMember() 
            : base("Member", "M", 
                  "Represents a steel stud", 
                  "HowickMaker", "Member", 
                  GH_ParamAccess.item) { }


        public override Guid ComponentGuid => new Guid("d568d18b-f525-4c11-8545-4fd52cc07bc3");
    }

    class GooMember : GH_Goo<HM.hMember>
    {
        public override bool IsValid => true;

        public override string TypeName => "hMember";

        public override string TypeDescription => "hMember";

        public override IGH_Goo Duplicate()
        {
            return new GooMember();
        }

        public override string ToString()
        {
            return this.Value.Name;
        }
    }
}
