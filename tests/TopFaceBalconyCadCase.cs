using System;
using System.Collections.Generic;
using System.Globalization;
using SldWorks;
using SwConst;
using SolidWorksAIBridge;

internal static class TopFaceBalconyCadCase
{
    internal static int Run()
    {
        // Run the unchanged eight-feature baseline first; stop on any failure.
        if (BalconyAssemblyCadCase.Run() != 0) throw new InvalidOperationException("Baseline comparison failed.");
        var connector = new SolidWorksConnector();
        connector.Connect();
        BalconyCadChecks.InspectConnection(connector.Application);
        var existing = BalconyCadChecks.Snapshot(connector.Application);
        ModelDoc2 model = null;
        try
        {
            var modeler = new SolidWorksModeler(connector.Application);
            model = modeler.CreateWall(180, 220, 3);
            Console.WriteLine("CASE=BALCONY_TOP_FACE_COMPARE; RECIPE=top-face-shared-r1; NEW_TEST_DOCUMENT=" + model.GetTitle() + "; PATH=" + model.GetPathName());
            var slab = new ProjectingBalconySlabInstance("both-walls_slab", "MainWall", 56, 24, 3, 20, 120);
            var instance = new BalconyAssemblyInstance("both-walls", slab, 20, 2, 20, 2, .8);
            var plan = TopFaceBalconyAssembly.Validate(instance, new BalconySlabHost("MainWall", 180, 220, 3));
            modeler.CreateTopFaceBalconyAssembly(instance);
            Verify(model, plan, 24, 5952);
            var depth = (Dimension)model.Parameter("SlabDepth@" + plan.Slab.FeatureName);
            if (depth == null) throw new InvalidOperationException("Slab depth edit dimension missing.");
            depth.SystemValue = .026;
            if (!model.EditRebuild3()) throw new InvalidOperationException("Depth edit rebuild failed.");
            Verify(model, plan, 26, 6448);
            depth.SystemValue = .024;
            if (!model.EditRebuild3()) throw new InvalidOperationException("Depth restore rebuild failed.");
            Verify(model, plan, 24, 5952);
            var slabWidth = (Dimension)model.Parameter("SlabWidth@" + plan.Slab.SketchName);
            if (slabWidth == null) throw new InvalidOperationException("Slab width edit dimension missing.");
            slabWidth.SystemValue = .060;
            if (!model.EditRebuild3()) throw new InvalidOperationException("Width edit rebuild failed.");
            Verify(model, plan, 24, 6240, 60);
            slabWidth.SystemValue = .056;
            if (!model.EditRebuild3()) throw new InvalidOperationException("Width restore rebuild failed.");
            Verify(model, plan, 24, 5952);
            Console.WriteLine("BALCONY_TOP_FACE_COMPARE_NUMERIC=PASS; WIDTH_EDIT_AND_RESTORE=PASS; DEPTH_EDIT_AND_RESTORE=PASS; VISUAL_APPROVAL=PENDING");
            return 0;
        }
        catch
        {
            Console.Error.WriteLine("FAILED_TEST_DOCUMENT_PRESERVED=" + (model == null ? "none" : model.GetTitle()));
            throw;
        }
        finally { BalconyCadChecks.CheckExisting(existing); }
    }

    private static void Verify(ModelDoc2 model, BalconyAssemblyPlan plan, double depth, double volume, double width = 56)
    {
        if (!model.EditRebuild3()) throw new InvalidOperationException("Assembly verification rebuild failed.");
        var bodies = ((PartDoc)model).GetBodies2((int)swBodyType_e.swSolidBody, false) as object[] ?? new object[0];
        Console.WriteLine("VERIFY_DEPTH_MM=" + depth + "; SOLIDS=" + bodies.Length + "; REBUILD=True");
        if (bodies.Length != 2) throw new InvalidOperationException("Expected MainWall and one BalconyAssembly body.");
        int facadeCount = 0, assemblyCount = 0;
        foreach (object item in bodies)
        {
            var body = (Body2)item;
            double actualVolume = ((double[])body.GetMassProperties(1))[3] * 1e9;
            var vertices = body.GetVertices() as object[] ?? new object[0];
            var points = new List<double[]>();
            foreach (object vertex in vertices)
            {
                var p = (double[])((Vertex)vertex).GetPoint();
                points.Add(new[] { p[0] * 1000, p[1] * 1000, p[2] * 1000 });
            }
            Console.WriteLine(string.Format(CultureInfo.InvariantCulture,
                "BODY={0}; VOLUME_MM3={1:R}; V={2}; E={3}; F={4}", body.Name, actualVolume,
                body.GetVertexCount(), body.GetEdgeCount(), body.GetFaceCount()));
            if (Math.Abs(actualVolume - 118800) < .01)
            {
                AssertBounds(points, new[] { -90.0, -110, -1.5, 90, 110, 1.5 });
                facadeCount++;
            }
            else if (Math.Abs(actualVolume - volume) < .01)
            {
                AssertBounds(points, new[] { 20 - width / 2, 7, 1.5, 20 + width / 2, 30, 1.5 + depth });
                // U-section corners at rear/front prove the expected wall/slab profile,
                // supplemented by a single solid, volume, and genus-zero Euler topology.
                double l = 20 - width / 2, r = 20 + width / 2;
                double[,] section = { { l, 7 }, { r, 7 }, { r, 30 }, { r - 2, 30 },
                    { r - 2, 10 }, { l + 2, 10 }, { l + 2, 30 }, { l, 30 } };
                int matched = 0;
                foreach (double z in new[] { 1.5, 1.5 + depth })
                    for (int i = 0; i < 8; i++)
                        if (points.Exists(p => Math.Abs(p[0] - section[i, 0]) < .001 &&
                            Math.Abs(p[1] - section[i, 1]) < .001 && Math.Abs(p[2] - z) < .001)) matched++;
                Console.WriteLine("CONNECTED_U_PROFILE_CORNERS=" + matched + "/16; REAR_GAP_MM=0; FRONT_DEPTH_MM=" + depth);
                if (matched != 16 || body.GetVertexCount() - body.GetEdgeCount() + body.GetFaceCount() != 2)
                    throw new InvalidOperationException("Unexpected assembly connectivity/profile topology.");
                Console.WriteLine("PRINT_ORIENTATION=underside Y=7 maps to plate Z=0; walls extend along +Y");
                assemblyCount++;
            }
            else throw new InvalidOperationException("Unexpected body volume: " + actualVolume);
        }
        if (facadeCount != 1 || assemblyCount != 1) throw new InvalidOperationException("Body roles are not isolated.");
        foreach (var wall in new[] { plan.Left, plan.Right })
        {
            var dimension = (Dimension)model.Parameter("Depth_" + wall.Role + "@" + TopFaceBalconyAssembly.SketchName(plan));
            if (dimension == null || Math.Abs(dimension.SystemValue * 1000 - depth) > .001)
                throw new InvalidOperationException("Wall depth did not follow slab depth.");
            CheckDimension(model, "Thickness_" + wall.Role + "@" + TopFaceBalconyAssembly.SketchName(plan), 2);
            CheckDimension(model, "CenterX_" + wall.Role + "@" + TopFaceBalconyAssembly.SketchName(plan),
                Math.Abs(20 + (wall.Role == "Left" ? -1 : 1) * (width - 2) / 2));
            CheckDimension(model, "CenterZ_" + wall.Role + "@" + TopFaceBalconyAssembly.SketchName(plan), 1.5 + depth / 2);
            var feature = ((PartDoc)model).FeatureByName(TopFaceBalconyAssembly.FeatureName(plan));
            if (feature == null || !CadFeatureClassifier.IsBoss(feature.GetTypeName2() == "ICE" ? feature.GetTypeName() : feature.GetTypeName2()))
                throw new InvalidOperationException("Shared wall Boss-Extrude missing.");
        }
        CheckDimension(model, "WallHeight@" + TopFaceBalconyAssembly.FeatureName(plan), 20);
        int sketches = 0, bosses = 0, forbidden = 0;
        var seen = new HashSet<string>();
        for (Feature feature = (Feature)model.FirstFeature(); feature != null; feature = (Feature)feature.GetNextFeature())
            Count(feature, seen, ref sketches, ref bosses, ref forbidden);
        Console.WriteLine("SKETCHES=" + sketches + "; BOSSES=" + bosses + "; CUT_OR_COMBINE=" + forbidden);
        if (sketches != 3 || bosses != 3 || forbidden != 0) throw new InvalidOperationException("Expected six generated features, no Combine/cut.");
    }

    private static void CheckDimension(ModelDoc2 model, string name, double expectedMm)
    {
        var dimension = (Dimension)model.Parameter(name);
        if (dimension == null || Math.Abs(dimension.SystemValue * 1000 - expectedMm) > .001)
            throw new InvalidOperationException("Incorrect/missing editable dimension: " + name);
    }
    private static void Count(Feature feature, HashSet<string> seen, ref int sketches, ref int bosses, ref int forbidden)
    {
        string type;
        if (!CadFeatureClassifier.TryResolveOnce(feature.Name, feature.GetTypeName2(), () => feature.GetTypeName(), seen, out type)) return;
        if (type == "ProfileFeature") sketches++;
        if (CadFeatureClassifier.IsBoss(type)) bosses++;
        if (type == "Cut" || type == "Combine") forbidden++;
        for (Feature child = (Feature)feature.GetFirstSubFeature(); child != null; child = (Feature)child.GetNextSubFeature())
            Count(child, seen, ref sketches, ref bosses, ref forbidden);
    }

    private static void AssertBounds(List<double[]> points, double[] expected)
    {
        var actual = new[] { double.PositiveInfinity, double.PositiveInfinity, double.PositiveInfinity,
            double.NegativeInfinity, double.NegativeInfinity, double.NegativeInfinity };
        foreach (var point in points)
            for (int i = 0; i < 3; i++) { actual[i] = Math.Min(actual[i], point[i]); actual[i + 3] = Math.Max(actual[i + 3], point[i]); }
        Console.WriteLine("BOUNDS_MM=" + string.Join(",", Array.ConvertAll(actual, n => n.ToString("R", CultureInfo.InvariantCulture))));
        for (int i = 0; i < 6; i++) if (Math.Abs(actual[i] - expected[i]) > .001) throw new InvalidOperationException("Measured body bounds differ.");
    }
}
