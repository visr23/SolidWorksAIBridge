using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Web.Script.Serialization;
using SldWorks;
using SwConst;
using SolidWorksAIBridge.Analysis.Learning;

internal static class FrontEvidence
{
    private static ReadBudget budget;
    private static string output;
    private static readonly JavaScriptSerializer Json = new JavaScriptSerializer { MaxJsonLength = Int32.MaxValue };
    private static readonly List<object> documents = new List<object>();
    private static readonly List<object> checkpoints = new List<object>();
    private static void Check() { budget.Checkpoint(); }
    private static string[] Parents(Feature feature)
    {
        Check();
        var parents = feature.GetParents() as object[] ?? new object[0];
        if (parents.Length > 32) throw new InvalidOperationException("Direct-parent read budget exceeded.");
        return parents.Cast<Feature>().Select(f => f.Name).Distinct().OrderBy(x => x, StringComparer.Ordinal).ToArray();
    }
    private static object Recipe(Feature feature)
    {
        if (feature == null) return new { missing = true };
        Check();
        string type = feature.GetTypeName2();
        return new { name = feature.Name, type = type, resolvedType = type == "ICE" ? feature.GetTypeName() : type, directParents = Parents(feature), definitionRead = false, editReliability = "Unknown: no edit/rebuild experiment" };
    }
    private static void Save(string status, string reason)
    {
        string temp = output + ".checkpoint";
        File.WriteAllText(temp, Json.Serialize(new { version = "0.1.0", scope = "FrontFacade", status = status, limitation = reason, units = "mm; plane normals unitless", checkpointCount = budget.Used, documents = documents, checkpoints = checkpoints }));
        File.Copy(temp, output, true);
    }
    [STAThread]
    private static int Main(string[] args)
    {
        CultureInfo.DefaultThreadCurrentCulture = CultureInfo.InvariantCulture;
        if (args.Length != 1 || File.Exists(args[0])) { Console.Error.WriteLine("Requires ONE new JSON output path; no CAD write options."); return 2; }
        output = Path.GetFullPath(args[0]);
        string outputRoot = Path.GetFullPath("pilots/AUE_PILOT_001/comparison/learning") + Path.DirectorySeparatorChar;
        if (!output.StartsWith(outputRoot, StringComparison.OrdinalIgnoreCase) || !String.Equals(Path.GetExtension(output), ".json", StringComparison.OrdinalIgnoreCase))
        { Console.Error.WriteLine("Output must be a new .json report in the pilot learning directory."); return 2; }
        budget = new ReadBudget(6000, 180, () => File.Exists(output + ".cancel"));
        SldWorks.SldWorks app = null;
        var snapshots = new List<Tuple<ModelDoc2, bool, string, string>>();
        string activePath = null;
        int beforeCount = 0;
        bool success = false;
        try
        {
            app = (SldWorks.SldWorks)Marshal.GetActiveObject("SldWorks.Application");
            activePath = ((ModelDoc2)app.ActiveDoc)?.GetPathName();
            var opened = app.GetDocuments() as object[] ?? new object[0]; beforeCount = opened.Length;
            string pilotPath = Path.GetFullPath("pilots/AUE_PILOT_001/solidworks reasults/AUE_PILOT_001_FRONT_V1.SLDPRT");
            string studioPath = @"G:\.shortcut-targets-by-id\1JLCVKHnaLXjxYGaZJM5xziEyy9a6prT4\בניינים\טרומפלדור 15 כפר סבא\Part1.SLDPRT";
            foreach (ModelDoc2 doc in opened)
                snapshots.Add(Tuple.Create(doc, doc.GetSaveFlag(), doc.GetPathName(), doc.ConfigurationManager.ActiveConfiguration.Name));
            var selected = snapshots.Where(s => String.Equals(s.Item3, pilotPath, StringComparison.OrdinalIgnoreCase) || String.Equals(s.Item3, studioPath, StringComparison.OrdinalIgnoreCase)).ToArray();
            if (selected.Length != 2) throw new InvalidOperationException("Both exact selected documents must already be open; no auto-open.");
            foreach (var snapshot in selected)
            {
                Check();
                var doc = snapshot.Item1;
                bool studio = String.Equals(snapshot.Item3, studioPath, StringComparison.OrdinalIgnoreCase);
                var bodyNames = studio ? new[] { "Combine9", "Boss-Extrude67[1]", "Cut-Extrude33[1]", "Cut-Extrude31", "Extrude-Thin3" }
                    : new[] { "Pilot001_LargeWindowOpening_PROVISIONAL", "Pilot001_LeftBayUpperSupport_PROVISIONAL", "Pilot001_UpperGableWindowOpening_PROVISIONAL", "Pilot001_LeftGableProfile_PROVISIONAL", "Pilot001_OpeningSurround_PROVISIONAL" };
                var bodies = ((PartDoc)doc).GetBodies2((int)swBodyType_e.swSolidBody, false) as object[] ?? new object[0];
                var bodyResults = new List<object>();
                var recipes = new Dictionary<string, object>(StringComparer.Ordinal);
                var docResult = new { role = studio ? "Studio" : "Generated", title = doc.GetTitle(), path = snapshot.Item3, dirtyBefore = snapshot.Item2, configurationBefore = snapshot.Item4, needsRebuild = doc.Extension.NeedsRebuild2, bodyCount = bodies.Length, bodies = bodyResults, recipes = recipes };
                documents.Add(docResult);
                foreach (string bodyName in bodyNames)
                {
                    Check(); Console.WriteLine("READ_BODY " + docResult.role + ":" + bodyName);
                    var matching = bodies.Cast<Body2>().Where(b => b.Name == bodyName).ToArray();
                    if (matching.Length != 1) throw new InvalidOperationException("Missing/ambiguous selected body snapshot: " + bodyName);
                    var body = matching[0];
                    var faces = body.GetFaces() as object[] ?? new object[0];
                    if (faces.Length > 600) throw new InvalidOperationException("Selected-body face budget exceeded: " + bodyName);
                    var faceResults = new List<object>();
                    bodyResults.Add(new { name = bodyName, inventoryFaceCount = faces.Length, faces = faceResults });
                    int faceIndex = 0;
                    foreach (Face2 face in faces)
                    {
                        Check(); int index = faceIndex++;
                        double area = face.GetArea() * 1e6;
                        if (area < (studio ? 30 : 1)) continue;
                        var surface = (Surface)face.GetSurface();
                        if (!surface.IsPlane()) continue;
                        var plane = (double[])surface.PlaneParams;
                        if (Math.Abs(Math.Abs(plane[2]) - 1) > 1e-7 || Math.Abs(plane[0]) > 1e-7 || Math.Abs(plane[1]) > 1e-7) continue;
                        double z = plane[5] * 1000;
                        double[] planes = studio ?
                            (bodyName == "Combine9" ? new[] { -50.6520681865233, -47.6520681865233, -46.1520681865233 } :
                            bodyName == "Boss-Extrude67[1]" ? new[] { 0.0, -3.0, 1.5 } :
                            bodyName == "Cut-Extrude33[1]" ? new[] { -28.1520681865233 } :
                            bodyName == "Cut-Extrude31" ? new[] { 11.2 } : new[] { -39.6520681865233, -47.6520681865233 }) :
                            (bodyName.Contains("LeftBay") || bodyName.Contains("LeftGable") ? new[] { 14.0 } : bodyName.Contains("OpeningSurround") ? new[] { 9.0 } : new[] { 6.0, 0.0 });
                        if (!planes.Any(p => Math.Abs(z - p) < 1e-5)) continue;
                        Check();
                        var feature = (Feature)face.GetFeature();
                        string featureName = feature == null ? "Unknown" : feature.Name;
                        if (feature != null && !recipes.ContainsKey(featureName)) recipes.Add(featureName, Recipe(feature));
                        var loops = face.GetLoops() as object[] ?? new object[0];
                        if (loops.Length > 40) throw new InvalidOperationException("Selected-face loop budget exceeded.");
                        var loopResults = new List<object>();
                        int loopIndex = 0;
                        foreach (Loop2 loop in loops)
                        {
                            Check();
                            var edges = loop.GetEdges() as object[] ?? new object[0];
                            if (edges.Length > 160) throw new InvalidOperationException("Selected-loop edge budget exceeded.");
                            var segments = new List<double[][]>();
                            int nonLine = 0;
                            foreach (Edge edge in edges)
                            {
                                Check();
                                var curve = (Curve)edge.GetCurve();
                                if (curve == null || !curve.IsLine()) { nonLine++; continue; }
                                var a = (Vertex)edge.GetStartVertex(); var b = (Vertex)edge.GetEndVertex();
                                if (a == null || b == null) { nonLine++; continue; }
                                segments.Add(new[] { ((double[])a.GetPoint()).Select(v => v * 1000).ToArray(), ((double[])b.GetPoint()).Select(v => v * 1000).ToArray() });
                            }
                            bool closed = nonLine == 0 && EdgeEvidence.IsClosedLineLoop(segments, 1e-6);
                            loopResults.Add(new { id = "f" + index + "/l" + loopIndex++, outer = loop.IsOuter(), nativeEdgeCount = edges.Length, unsupportedCurvedEdges = nonLine, closedLineLoop = closed, segmentsMm = segments });
                        }
                        faceResults.Add(new { id = "f" + index, planeNormal = plane.Take(3).ToArray(), zMm = z, areaMm2 = area, ownerFeature = featureName, loops = loopResults });
                    }
                    checkpoints.Add(new { role = docResult.role, body = bodyName, facesRead = faces.Length, selectedFaces = faceResults.Count });
                    Save("Partial", "Cooperative checkpoints; no full-model traversal. CAD boundary completeness/visibility not yet assessed.");
                    Console.WriteLine("CHECKPOINT " + bodyName + "; selected faces=" + faceResults.Count);
                }
                string[] targets = studio ? new[] { "Combine9", "Boss-Extrude67", "Cut-Extrude33", "Cut-Extrude31", "Extrude-Thin3", "Scale1", "Mirror1", "Split4" }
                    : new[] { "MainWall", "WindowCut", "Pilot001_EntranceRecess_PROVISIONAL", "Pilot001_DoorOpening_PROVISIONAL", "Pilot001_LargeWindowOpening_PROVISIONAL", "Pilot001_LeftBayBase_PROVISIONAL", "Pilot001_LeftBayUpperSupport_PROVISIONAL", "Pilot001_MainGableProfile_PROVISIONAL", "Pilot001_LeftGableProfile_PROVISIONAL", "Pilot001_UpperGableWindowOpening_PROVISIONAL", "Pilot001_OpeningSurround_PROVISIONAL" };
                foreach (var name in targets) if (!recipes.ContainsKey(name)) recipes.Add(name, Recipe((Feature)((PartDoc)doc).FeatureByName(name)));
            }
            success = true;
        }
        catch (Exception e) { Console.WriteLine("STOP_READ " + e.GetType().Name + ": " + e.Message); Save("Partial", e.Message); }
        finally
        {
            bool preserved = app != null;
            foreach (var s in snapshots)
            {
                try { preserved &= s.Item1.GetSaveFlag() == s.Item2 && s.Item1.GetPathName() == s.Item3 && s.Item1.ConfigurationManager.ActiveConfiguration.Name == s.Item4; }
                catch { preserved = false; }
            }
            if (app != null)
            {
                preserved &= (app.GetDocuments() as object[] ?? new object[0]).Length == beforeCount;
                preserved &= ((ModelDoc2)app.ActiveDoc)?.GetPathName() == activePath;
            }
            checkpoints.Add(new { documentMetadataPreserved = preserved, saved = false, rebuilt = false, activated = false });
            success &= preserved;
            Save(success ? "CompletedBoundedRead" : "Partial", "Selected planar face edges only; no hidden-line removal, curved-edge chord substitution, or whole-building silhouette claim.");
            Console.WriteLine("METADATA_PRESERVED " + preserved + "; SUCCESS " + success);
        }
        return success ? 0 : 1;
    }
}
