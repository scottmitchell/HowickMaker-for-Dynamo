using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HM = HowickMaker;
using Geo = Autodesk.DesignScript.Geometry;
using Autodesk.DesignScript.Interfaces;
using Autodesk.DesignScript.Runtime;

namespace HowickMakerDynamo
{
    public class Structure
    {

        //  ██████╗ ██╗   ██╗██████╗      ██████╗███╗   ██╗███████╗████████╗██████╗ 
        //  ██╔══██╗██║   ██║██╔══██╗    ██╔════╝████╗  ██║██╔════╝╚══██╔══╝██╔══██╗
        //  ██████╔╝██║   ██║██████╔╝    ██║     ██╔██╗ ██║███████╗   ██║   ██████╔╝
        //  ██╔═══╝ ██║   ██║██╔══██╗    ██║     ██║╚██╗██║╚════██║   ██║   ██╔══██╗
        //  ██║     ╚██████╔╝██████╔╝    ╚██████╗██║ ╚████║███████║   ██║   ██║  ██║
        //  ╚═╝      ╚═════╝ ╚═════╝      ╚═════╝╚═╝  ╚═══╝╚══════╝   ╚═╝   ╚═╝  ╚═╝
        //               

        [MultiReturn(new[] { "members", "braces", "connections" })]
        public static Dictionary<string, object> FromLines(
            List<Geo.Line> lines,
            [DefaultArgument("[]")] List<string> names,
            [DefaultArgument("{}")] Dictionary<string, Geo.Vector> webNormalsDict,
            [DefaultArgument("{}")] Dictionary<string, int> priorityDict,
            [DefaultArgument("{}")] Dictionary<string, int> extensionDict,
            [DefaultArgument("null")] string options)
        {
            // Parse Options
            string[] option = options.ToString().Split(',');
            double intersectionTolerance = (options == "null") ? 0.001 : Double.Parse(option[0]);
            double planarityTolerance = (options == "null") ? 0.001 : Double.Parse(option[1]);
            bool generateBraces = (options == "null") ? false : bool.Parse(option[2]);
            bool threePieceBrace = (options == "null") ? false : bool.Parse(option[3]);
            double braceLength1 = (options == "null") ? 6 : Double.Parse(option[4]);
            double braceLength2 = (options == "null") ? 3 : Double.Parse(option[5]);
            bool firstConnectionIsFTF = (options == "null") ? false : bool.Parse(option[6]);

            // Create empty dictionaries if unprovided
            if (webNormalsDict == null) { webNormalsDict = new Dictionary<string, Geo.Vector>(); }
            if (priorityDict == null) { priorityDict = new Dictionary<string, int>(); }
            if (extensionDict == null) { extensionDict = new Dictionary<string, int>(); }
            names = CompleteListOfNames(names, lines.Count);

            // Convert Lines
            var hLines = new List<HM.Line>();
            foreach (Geo.Line l in lines)
            {
                hLines.Add(new HM.Line(new HM.Triple(l.StartPoint.X, l.StartPoint.Y, l.StartPoint.Z), new HM.Triple(l.EndPoint.X, l.EndPoint.Y, l.EndPoint.Z)));
            }

            // Convert webNormals
            var hWebNormalsDict = new Dictionary<string, HM.Triple>();
            foreach (string vectorName in webNormalsDict.Keys)
            {
                var vector = webNormalsDict[vectorName];
                hWebNormalsDict[vectorName] = new HM.Triple(vector.X, vector.Y, vector.Z);
            }

            HM.hStructure structure = HM.hStructure.StructureFromLines(
                hLines,
                names,
                hWebNormalsDict,
                priorityDict,
                extensionDict,
                intersectionTolerance,
                planarityTolerance,
                generateBraces,
                threePieceBrace,
                braceLength1,
                braceLength2,
                firstConnectionIsFTF);

            var mems = structure.Members.ToList();
            var components = new List<Member>();
            foreach (HM.hMember m in mems)
            {
                components.Add(new Member(m));
            }
            return new Dictionary<string, object>
            {
                { "members", components },
                { "braces", structure.BraceMembers.ToList() },
                { "connections", structure.Connections }
            };
        }


        /// <summary>
        /// Adds member names if any are missing
        /// </summary>
        /// <param name="names"></param>
        /// <param name="numMembers"></param>
        /// <returns></returns>
        [IsVisibleInDynamoLibrary(false)]
        internal static List<string> CompleteListOfNames(List<string> names, int numMembers)
        {
            if (names.Count > numMembers)
            {
                throw new System.ArgumentException("List of names cannot be longer than list of member lines.", "names");
            }
            else if (names.Count == numMembers)
            {
                return names;
            }
            else
            {
                var fillNum = numMembers - names.Count;
                for (int i = 0; i < fillNum; i++)
                {
                    names.Add(i.ToString());
                }
                return names;
            }
        }
    }
}
