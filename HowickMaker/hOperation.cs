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
    /// List of valid operations on a Howick FRAMA 3200
    /// </summary>
    [IsVisibleInDynamoLibrary(false)]
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
        internal double _loc;
        internal Operation _type;

        internal hOperation(double loc, Operation type)
        {
            _loc = loc;
            _type = type;
        }
    }
}
