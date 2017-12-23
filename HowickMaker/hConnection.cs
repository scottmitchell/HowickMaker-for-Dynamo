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
        BR
    }


    public class hConnection
    {
        public List<int> members;
        public Connection type;

        public hConnection(Connection type)
        {
            this.type = type;
        }
    }
}
