using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using SldWorks;
using SwConst;

internal static class BalconyCadChecks
{
    internal static void InspectConnection(SldWorks.SldWorks application)
    {
        Console.WriteLine("COM_ATTACH=SUCCESS; SOLIDWORKS_REVISION=" + application.RevisionNumber());
        string template = application.GetUserPreferenceStringValue((int)swUserPreferenceStringValue_e.swDefaultTemplatePart);
        Console.WriteLine("PART_TEMPLATE=" + template);
        if (string.IsNullOrWhiteSpace(template) || !File.Exists(template))
            throw new InvalidOperationException("Configured default Part template does not exist.");
        Console.WriteLine("PART_TEMPLATE_EXISTS=true");
        var docs = application.GetDocuments() as object[];
        Console.WriteLine("EXISTING_DOCUMENT_COUNT=" + (docs == null ? 0 : docs.Length));
    }

    internal static List<Tuple<ModelDoc2, bool>> Snapshot(SldWorks.SldWorks application)
    {
        var result = new List<Tuple<ModelDoc2, bool>>();
        var docs = application.GetDocuments() as object[];
        if (docs != null)
            foreach (object doc in docs)
                result.Add(Tuple.Create((ModelDoc2)doc, ((ModelDoc2)doc).GetSaveFlag()));
        return result;
    }

    internal static void CheckExisting(List<Tuple<ModelDoc2, bool>> snapshot)
    {
        foreach (var item in snapshot)
            if (item.Item1.GetSaveFlag() != item.Item2)
                throw new InvalidOperationException("An existing document's dirty flag changed; stop.");
        Console.WriteLine("EXISTING_DOCUMENT_DIRTY_FLAGS_UNCHANGED=true");
    }

    internal static void Measure(ModelDoc2 model, bool assertComplete)
    {
        MeasureExpected(model, assertComplete, new[] { -8.0, 7, 1.5, 48, 10, 25.5 }, 4032, 118800, 2, 0);
    }

    internal static void MeasureExpected(ModelDoc2 model, bool assertComplete, double[] expectedSlabBounds,
        double expectedSlabVolume, double expectedWallVolume, int expectedSketches, int expectedCuts)
    {
        Console.WriteLine("NEW_TEST_DOCUMENT=" + model.GetTitle());
        Console.WriteLine("NEW_TEST_PATH=" + model.GetPathName());
        var part = (PartDoc)model;
        bool planeExists = part.FeatureByName("Front Plane") != null;
        Console.WriteLine("FRONT_PLANE_EXISTS=" + planeExists);
        var bodies = part.GetBodies2((int)swBodyType_e.swSolidBody, false) as object[] ?? new object[0];
        Console.WriteLine("SOLID_BODY_COUNT=" + bodies.Length);
        double totalVolume = 0;
        int slabCount = 0, wallCount = 0;
        foreach (object item in bodies)
        {
            var body = (Body2)item;
            var vertices = body.GetVertices() as object[] ?? new object[0];
            double[] bounds = { double.PositiveInfinity, double.PositiveInfinity, double.PositiveInfinity,
                double.NegativeInfinity, double.NegativeInfinity, double.NegativeInfinity };
            foreach (object vertex in vertices)
            {
                var point = (double[])((Vertex)vertex).GetPoint();
                for (int axis = 0; axis < 3; axis++)
                {
                    bounds[axis] = Math.Min(bounds[axis], point[axis] * 1000);
                    bounds[axis + 3] = Math.Max(bounds[axis + 3], point[axis] * 1000);
                }
            }
            var mass = (double[])body.GetMassProperties(1);
            double volume = mass[3] * 1e9;
            totalVolume += volume;
            Console.WriteLine(string.Format(CultureInfo.InvariantCulture,
                "BODY={0}; VERTICES={1}; BOUNDS_MM=[{2:R},{3:R},{4:R},{5:R},{6:R},{7:R}]; VOLUME_MM3={8:R}",
                body.Name, vertices.Length, bounds[0], bounds[1], bounds[2], bounds[3], bounds[4], bounds[5], volume));
            if (Near(volume, expectedSlabVolume, .01) && Match(bounds, expectedSlabBounds)) slabCount++;
            if (Near(volume, expectedWallVolume, .01) && Match(bounds, new[] { -90.0, -110, -1.5, 90, 110, 1.5 })) wallCount++;
        }
        Console.WriteLine("TOTAL_VOLUME_MM3=" + totalVolume.ToString("R", CultureInfo.InvariantCulture));
        var seen = new HashSet<string>(StringComparer.Ordinal);
        int sketches = 0, bosses = 0, cuts = 0;
        for (Feature feature = (Feature)model.FirstFeature(); feature != null; feature = (Feature)feature.GetNextFeature())
            Visit(feature, seen, ref sketches, ref bosses, ref cuts);
        Console.WriteLine("GENERATED_SKETCHES=" + sketches + "; BOSSES=" + bosses + "; CUTS=" + cuts);
        if (assertComplete)
        {
            bool rebuilt = model.EditRebuild3(); // Only the newly created test document.
            Console.WriteLine("TEST_DOCUMENT_REBUILD=" + rebuilt);
            if (!planeExists || bodies.Length != 2 || slabCount != 1 || wallCount != 1 ||
                sketches != expectedSketches || bosses != 2 || cuts != expectedCuts || !rebuilt)
                throw new InvalidOperationException("BALCONY measured geometry/feature/body/rebuild expectations failed.");
            Console.WriteLine("BALCONY_NUMERIC_GEOMETRY=PASS; VISUAL_REVIEW=PENDING");
        }
    }

    private static void Visit(Feature feature, HashSet<string> seen, ref int sketches, ref int bosses, ref int cuts)
    {
        string reportedType = feature.GetTypeName2();
        string type;
        if (!CadFeatureClassifier.TryResolveOnce(feature.Name, reportedType,
            () => feature.GetTypeName(), seen, out type)) return;
        Console.WriteLine("FEATURE=" + feature.Name + "; TYPE=" + reportedType + "; RESOLVED_TYPE=" + type);
        if (feature.Name == "WallSketch" || feature.Name == "BalconySlabSketch_balcony-01" || feature.Name == "WindowSketch")
        {
            if (type == "ProfileFeature") sketches++;
        }
        if (feature.Name == "MainWall" || feature.Name == "BalconySlab_balcony-01")
        {
            if (CadFeatureClassifier.IsBoss(type)) bosses++;
        }
        if (type == "Cut") cuts++;
        for (Feature child = (Feature)feature.GetFirstSubFeature(); child != null; child = (Feature)child.GetNextSubFeature())
            Visit(child, seen, ref sketches, ref bosses, ref cuts);
    }

    private static bool Near(double actual, double expected, double tolerance) =>
        !double.IsNaN(actual) && Math.Abs(actual - expected) <= tolerance;
    private static bool Match(double[] actual, double[] expected)
    {
        for (int i = 0; i < actual.Length; i++) if (!Near(actual[i], expected[i], .001)) return false;
        return true;
    }
}
