using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Geo = Autodesk.DesignScript.Geometry;
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
        PT,
        T
    }

    [IsVisibleInDynamoLibrary(false)]
    public class hConnection
    {
        public Geo.Point location;
        public List<int> members = new List<int>();
        public Connection type;

        public hConnection(Connection type)
        {
            this.type = type;
        }

        public hConnection(Connection type, List<int> members)
        {
            this.type = type;
            this.members = members;
        }

        public void AddMember(int member)
        {
            this.members.Add(member);
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        internal int GetOtherIndex(int index)
        {
            foreach (int i in members)
            {
                if (i != index)
                {
                    index = i;
                }
            }

            return index;
        }

        public override bool Equals(object value)
        {
            hConnection con = value as hConnection;

            return (con != null)
                && (members.Contains(con.members[0]))
                && (members.Contains(con.members[1]))
                && (type == con.type);
        }
    }
}
