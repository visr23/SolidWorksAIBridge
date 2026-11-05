using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Web.Script.Serialization;
using SldWorks;
using SwConst;

internal static class Inspect
{
    [STAThread]
    private static int Main(string[] args)
    {
        try
        {
            if (args.Length != 1) throw new ArgumentException("Read-only inspection requires exactly one output path.");
            var app = (SldWorks.SldWorks)Marshal.GetActiveObject("SldWorks.Application");
            var documents = new List<object>();
            foreach (ModelDoc2 doc in app.GetDocuments() as object[] ?? new object[0])
            {
                string candidatePath = doc.GetPathName();
                if (!candidatePath.EndsWith(@"\solidworks reasults\AUE_PILOT_001_FRONT_V1.SLDPRT", StringComparison.OrdinalIgnoreCase) && !candidatePath.Equals(@"G:\.shortcut-targets-by-id\1JLCVKHnaLXjxYGaZJM5xziEyy9a6prT4\בניינים\טרומפלדור 15 כפר סבא\Part1.SLDPRT", StringComparison.OrdinalIgnoreCase)) continue;
                Console.WriteLine("BEGIN_READ_ONLY " + candidatePath);
                bool dirty = doc.GetSaveFlag();
                string path = doc.GetPathName();
                var features = new List<object>();
                var dimensions = new Dictionary<string, object>();
                var seen = new HashSet<string>();
                Action<Feature> visit = null;
                visit = feature =>
                {
                    if (!seen.Add(feature.Name)) return;
                    string type = feature.GetTypeName2();
                    features.Add(new { name = feature.Name, type = type, resolvedType = type == "ICE" ? feature.GetTypeName() : type });
                    for (var display = (DisplayDimension)feature.GetFirstDisplayDimension(); display != null; display = null)
                    {
                        var dimension = (Dimension)display.GetDimension2(0);
                        if (dimension != null) dimensions[dimension.FullName] = new { systemValue = dimension.SystemValue, drivenState = dimension.DrivenState };
                    }
                    for (var child = (Feature)feature.GetFirstSubFeature(); child != null; child = (Feature)child.GetNextSubFeature()) visit(child);
                };
                for (var feature = (Feature)doc.FirstFeature(); feature != null; feature = (Feature)feature.GetNextFeature()) visit(feature);
                foreach (string name in new[] { "Width@WallSketch", "Height@WallSketch", "D1@MainWall", "WindowWidth@WindowSketch", "WindowHeight@WindowSketch", "CenterX@WindowSketch", "CenterY@WindowSketch", "D1@WindowCut" })
                {
                    var dimension = (Dimension)doc.Parameter(name);
                    if (dimension != null) dimensions[dimension.FullName] = new { systemValue = dimension.SystemValue, drivenState = dimension.DrivenState };
                }
                var bodies = new List<object>();
                if (doc.GetType() == (int)swDocumentTypes_e.swDocPART)
                {
                    foreach (Body2 body in ((PartDoc)doc).GetBodies2((int)swBodyType_e.swSolidBody, false) as object[] ?? new object[0])
                    {
                        Console.WriteLine("BODY_READ " + body.Name);
                        var vertices = (path.EndsWith("Part1.SLDPRT", StringComparison.OrdinalIgnoreCase) && !new[] { "Combine9", "Boss-Extrude67[1]", "Cut-Extrude33[1]", "Cut-Extrude31", "Split4[1]", "Split4[2]" }.Contains(body.Name) ? new object[0] : body.GetVertices() as object[] ?? new object[0]).Cast<Vertex>().Take(1200).Select(v => ((double[])v.GetPoint()).Select(x => x * 1000).ToArray()).ToArray();
                        var faces = new List<object>();
                        var extremes = new List<double[]>();
                        foreach (var direction in new[] { new[] {1.0,0.0,0.0}, new[] {-1.0,0.0,0.0}, new[] {0.0,1.0,0.0}, new[] {0.0,-1.0,0.0}, new[] {0.0,0.0,1.0}, new[] {0.0,0.0,-1.0} })
                        {
                            double x, y, z;
                            if (!body.GetExtremePoint(direction[0], direction[1], direction[2], out x, out y, out z)) throw new InvalidOperationException("Extreme point unavailable.");
                            extremes.Add(new[] { x * 1000, y * 1000, z * 1000 });
                        }
                        Console.WriteLine("BODY_EXTREMA " + body.Name + "; " + string.Join(";", extremes.Select(p => string.Join(",", p))));
                        bodies.Add(new { extremePointsMm = extremes, vertexSamplingLimit = 1200, name = body.Name, volumeMm3 = (double?)null, verticesMm = vertices, faces = faces, vertexCount = body.GetVertexCount(), edgeCount = body.GetEdgeCount(), faceCount = body.GetFaceCount() });
                    }
                }
                int needsRebuild = doc.Extension.NeedsRebuild2;
                if (doc.GetSaveFlag() != dirty || doc.GetPathName() != path) throw new InvalidOperationException("Document metadata changed during read-only inspection.");
                documents.Add(new { title = doc.GetTitle(), path = path, dirty = dirty, configuration = doc.ConfigurationManager.ActiveConfiguration.Name, needsRebuild = needsRebuild, features = features, dimensions = dimensions, bodies = bodies });
                Console.WriteLine("INSPECTED " + doc.GetTitle() + "; Path=" + path + "; Dirty=" + dirty + "; NeedsRebuild=" + needsRebuild + "; Bodies=" + bodies.Count);
            }
            File.WriteAllText(args[0], new JavaScriptSerializer { MaxJsonLength = int.MaxValue }.Serialize(new { revision = app.RevisionNumber(), units = "body vertices mm; dimension SystemValue SI; plane normal unitless/origin meters", documents = documents }));
            return 0;
        }
        catch (Exception exception) { Console.Error.WriteLine(exception); return 1; }
    }
}
