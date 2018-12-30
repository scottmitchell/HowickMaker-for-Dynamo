using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HowickMaker
{
    /// <summary>
    /// List of valid operations on a Howick FRAMA 3200
    /// </summary>
    public enum Operation
    {
        END_TRUSS = 0,
        NOTCH,
        LIP_CUT,
        SWAGE,
        WEB,
        BOLT,
        SERVICE_HOLE,
        DIMPLE
    }

    /// <summary>
    /// Represents a Howick operation, 
    /// including the type of operation and the location along a member where it occurs
    /// </summary>
    public class hOperation
    {
        public double _loc;
        public Operation _type;

        public hOperation(double loc, Operation type)
        {
            _loc = loc;
            _type = type;
        }
    }
}
