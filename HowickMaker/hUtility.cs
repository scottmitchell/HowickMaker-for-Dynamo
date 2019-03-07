using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HowickMaker
{
    public class hUtility
    {
        #region PreCSV Check

        private static double rollerSpacing = 8.85827;
        private static double endTrussWidth = 0.75;
        private static double notchWidth = 2.0;


        /// <summary>
        /// Get the maximum number of rollers that would be disengaged during 
        /// the run of these members on the Howick machine.
        /// </summary>
        /// <param name="members"></param>
        /// <param name="clearance"></param>
        /// <returns></returns>
        public static int CheckMaxNumRollersDisengaged(List<hMember> members, double clearance = 0.25)
        {
            var operations = BuildCompositeListOfOperations(members);
            var scaryOps = operations.Where(op => op._type == Operation.END_TRUSS || op._type == Operation.NOTCH);

            if (scaryOps.Count() == 0) return 0;

            var numDisengagedRollers = 1;

            foreach (hOperation op in scaryOps)
            {
                var disengagedRollers = new bool[6];
                disengagedRollers[0] = true;

                var opWidth = op._type == Operation.END_TRUSS ? endTrussWidth : notchWidth;

                var opsInRange = scaryOps.Where(o => o._loc - op._loc > 0
                                                  && o._loc - op._loc < rollerSpacing * 5 + 5);

                foreach (hOperation otherOp in opsInRange)
                {
                    var otherOpWidth = otherOp._type == Operation.END_TRUSS ? endTrussWidth : notchWidth;
                    var locClearance = opWidth / 2 + otherOpWidth / 2 + clearance;
                    var locDiff = otherOp._loc - op._loc;

                    var offset = Math.Abs(rollerSpacing - locDiff) % rollerSpacing;

                    var disengaged = offset < locClearance;
                    if (disengaged)
                    {
                        int rollerNumber = (int)Math.Round(Math.Abs(rollerSpacing - locDiff) / rollerSpacing) + 1;
                        disengagedRollers[rollerNumber] = true;
                    }
                }

                var locNumDisengaged = disengagedRollers.Where(r => r).Count();

                if (locNumDisengaged > numDisengagedRollers) numDisengagedRollers = locNumDisengaged;
            }

            return numDisengagedRollers;
        }

        /// <summary>
        /// Get a combined list of all operations from a set of IMembers. 
        /// Operation location for these operations will be set relative 
        /// to the set of members as they would go through the Howick machine.
        /// </summary>
        /// <param name="members"></param>
        /// <returns></returns>
        public static List<hOperation> BuildCompositeListOfOperations(List<hMember> members)
        {
            var operations = new List<hOperation>();

            var currentLocation = 0.0;
            var overhangingOps = false;
            foreach (hMember member in members)
            {
                overhangingOps = overhangingOps || OperationsOverhangFront(member);
                if (overhangingOps) currentLocation += 2.5;

                foreach (hOperation op in member.Operations)
                {
                    var newOp = new hOperation(currentLocation + op._loc, op._type);
                    operations.Add(newOp);
                }
                // Add this member's length to current location
                currentLocation += member.Length;
                // Add SHEAR waste to current location
                currentLocation += 0.125;

                overhangingOps = OperationsOverhangEnd(member);
            }

            return operations;
        }

        private static bool OperationsOverhangFront(hMember member)
        {
            var potentiallyOverhangingOps = member.Operations.Where(op => op._type == Operation.NOTCH || op._type == Operation.LIP_CUT);
            foreach (hOperation op in potentiallyOverhangingOps)
            {
                if (op._loc <= 1)
                {
                    return true;
                }
            }
            return false;
        }

        private static bool OperationsOverhangEnd(hMember member)
        {
            var potentiallyOverhangingOps = member.Operations.Where(op => op._type == Operation.NOTCH || op._type == Operation.LIP_CUT);
            foreach (hOperation op in potentiallyOverhangingOps)
            {
                if (op._loc >= member.Length - 1)
                {
                    return true;
                }
            }
            return false;
        }




        #endregion

        #region Export to file
        /// <summary>
        /// Export a list of hMembers to csv file for Howick production
        /// </summary>
        /// <param name="filePath"></param>
        /// <param name="hMembers"></param>
        /// <param name="normalLabel"></param>
        public static void ExportToFile(string filePath, List<hMember> hMembers, string title = "H", bool normalLabel = true)
        {
            string csv = "UNIT,INCH" + "\n" +
                         "PROFILE,S8908,Standard Profile" + "\n" +
                         "FRAMESET," + title + "\n";

            foreach (hMember member in hMembers)
            {
                csv += member.AsCSVLine(normalLabel);
                csv += "\n";
            }

            File.WriteAllText(filePath, csv);
        }

        #endregion
    }
}