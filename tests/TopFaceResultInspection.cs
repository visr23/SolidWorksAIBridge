using System;
using System.Globalization;
using SldWorks;
using SolidWorksAIBridge;

// Standalone read-only diagnostic. Never creates, activates, edits, selects,
// rebuilds, saves or closes a document; never calls AccessSelections/ModifyDefinition.
internal static class TopFaceResultInspection
{
    [STAThread]
    private static int Main(string[] args)
    {
        if (args.Length != 1 || args[0] != "--inspect-part3") return 2;
        try
        {
            var connector = new SolidWorksConnector();
            connector.Connect();
            var app = connector.Application;
            var docs = app.GetDocuments() as object[] ?? new object[0];
            Console.WriteLine("READ_ONLY_DOCUMENT_COUNT=" + docs.Length);
            ModelDoc2 candidate = null;
            foreach (object item in docs)
            {
                var doc = (ModelDoc2)item;
                Console.WriteLine("DOCUMENT=" + doc.GetTitle() + "; PATH=" + doc.GetPathName() + "; DIRTY=" + doc.GetSaveFlag());
                if (doc.GetTitle() == "Part3" && doc.GetPathName() == "" &&
                    ((PartDoc)doc).FeatureByName("BalconyTopWalls_both-walls") != null) candidate = doc;
            }
            if (candidate == null) throw new InvalidOperationException("Exact candidate Part3 not found.");
            bool dirtyBefore = candidate.GetSaveFlag();
            var part = (PartDoc)candidate;
            var profile = part.FeatureByName("BalconyTopWallSketch_both-walls");
            var boss = part.FeatureByName("BalconyTopWalls_both-walls");
            var sketch = (Sketch)profile.GetSpecificFeature2();
            int kind = 0;
            var face = sketch.GetReferenceEntity(ref kind) as Face2;
            if (face == null) throw new InvalidOperationException("Sketch support is not a face.");
            var normal = (double[])face.Normal;
            Console.WriteLine("SUPPORT_ENTITY_TYPE=" + kind + "; SUPPORT_NORMAL=" + Numbers(normal));
            if (Math.Abs(normal[0]) > 1e-8 || Math.Abs(normal[1] - 1) > 1e-8 || Math.Abs(normal[2]) > 1e-8)
                throw new InvalidOperationException("Support is not an upward face.");
            int edges = 0, diagonals = 0;
            var math = (MathUtility)app.GetMathUtility();
            var toModel = (MathTransform)sketch.ModelToSketchTransform.Inverse();
            foreach (object item in sketch.GetSketchSegments() as object[] ?? new object[0])
            {
                var segment = (SketchSegment)item;
                if (segment.ConstructionGeometry) diagonals++; else edges++;
                var line = (SketchLine)segment;
                foreach (SketchPoint point in new[] { (SketchPoint)line.GetStartPoint2(), (SketchPoint)line.GetEndPoint2() })
                {
                    var modelPoint = (double[])((MathPoint)((MathPoint)math.CreatePoint(new[] { point.X, point.Y, point.Z })).MultiplyTransform(toModel)).ArrayData;
                    if (Math.Abs(modelPoint[1] - .010) > 1e-8) throw new InvalidOperationException("Footprint does not lie on slab top Y=10mm.");
                }
            }
            Console.WriteLine("SHARED_SKETCH_PROFILE_EDGES=" + edges + "; CONSTRUCTION_DIAGONALS=" + diagonals + "; ALL_ENDPOINTS_MODEL_Y_MM=10");
            if (edges != 8 || diagonals != 4) throw new InvalidOperationException("Expected two rectangular profiles in one sketch.");
            var data = (ExtrudeFeatureData2)boss.GetDefinition();
            var o = (double[])((MathPoint)((MathPoint)math.CreatePoint(new[] { 0.0, 0, 0 })).MultiplyTransform(toModel)).ArrayData;
            var n = (double[])((MathPoint)((MathPoint)math.CreatePoint(new[] { 0.0, 0, 1 })).MultiplyTransform(toModel)).ArrayData;
            double upward = (n[1] - o[1]) * (data.ReverseDirection ? -1 : 1);
            Console.WriteLine("BOSS_MERGE=" + data.Merge + "; FEATURE_SCOPE=" + data.FeatureScope + "; AUTO_SELECT=" + data.AutoSelect);
            Console.WriteLine("BOSS_DEPTH_MM=" + (data.GetDepth(true) * 1000).ToString("R", CultureInfo.InvariantCulture) + "; REVERSED=" + data.ReverseDirection + "; MODEL_Y_DIRECTION=" + upward);
            Console.WriteLine("SCOPE_BODY_COUNT_GETTER=" + data.GetFeatureScopeBodiesCount() + "; ACCESS_SELECTIONS_CALLED=False");
            if (!data.Merge || !data.FeatureScope || data.AutoSelect || Math.Abs(upward - 1) > 1e-8 || Math.Abs(data.GetDepth(true) - .020) > 1e-8)
                throw new InvalidOperationException("Unexpected boss settings/direction.");
            if (candidate.GetSaveFlag() != dirtyBefore) throw new InvalidOperationException("Candidate dirty flag changed during inspection.");
            Console.WriteLine("READ_ONLY_SKETCH_AND_BOSS_INSPECTION=PASS; CANDIDATE_DIRTY_FLAG_UNCHANGED=True");
            return 0;
        }
        catch (Exception ex) { Console.Error.WriteLine(ex); return 1; }
    }

    private static string Numbers(double[] numbers)
    {
        return string.Join(",", Array.ConvertAll(numbers, n => n.ToString("R", CultureInfo.InvariantCulture)));
    }
}
