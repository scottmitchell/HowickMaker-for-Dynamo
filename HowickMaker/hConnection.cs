using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HowickMaker
{

    /// <summary>
    /// List of supported connection types
    /// </summary>
    public enum Connection
    {
        FTF = 0,
        BR,
        PT,
        T,
        Invalid,
    }

    /// <summary>
    /// Represents a connection within an hStructure
    /// </summary>
    public class hConnection
    {
        public Triple location;
        public List<int> members = new List<int>();
        internal Connection type;
        public string connectionType {
            get
            {
                return type.ToString();
            }
        }
        public List<int> connectionMemberIndices
        {
            get
            {
                return members;
            }
        }

        internal hConnection(Connection type)
        {
            this.type = type;
        }

        public hConnection(Connection type, List<int> members)
        {
            this.type = type;
            this.members = members;
        }

        internal void AddMember(int member)
        {
            this.members.Add(member);
        }


        /// <summary>
        /// Returns the index of the other member involved in this connection
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

        /// <summary>
        /// Equality for hConnections.
        /// Order of members does not matter.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
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
