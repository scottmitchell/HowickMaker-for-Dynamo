using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.DesignScript.Geometry;
using Autodesk.DesignScript.Interfaces;
using Autodesk.DesignScript.Runtime;

namespace HowickMaker
{

    /// <summary>
    /// 
    /// </summary>
    [IsVisibleInDynamoLibrary(false)]
    public enum Connection
    {
        FTF = 0,
        BR,
        PT
    }


    public class hConnection
    {
        public List<int> members = new List<int>();
        public Connection type;

        public hConnection(Connection type)
        {
            this.type = type;
        }

        public void addMember(int member)
        {
            this.members.Add(member);
        }
    }
}
