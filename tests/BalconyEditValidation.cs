using System;
using System.Globalization;
using SldWorks;
using SwConst;
using SolidWorksAIBridge;

internal static class BalconyEditValidation
{
    internal static bool IsCase(string name) => name == "BALCONY_EDIT_WIDTH" || name == "BALCONY_EDIT_DEPTH" ||
        name == "BALCONY_EDIT_THICKNESS" || name == "BALCONY_EDIT_POSITION" ||
        name == "BALCONY_EDIT_ELEVATION" || name == "BALCONY_UPPER_DOOR";

    internal static int Run(string name)
    {
        var connector = new SolidWorksConnector();
        connector.Connect();
        BalconyCadChecks.InspectConnection(connector.Application);
        var existing = BalconyCadChecks.Snapshot(connector.Application);
        ModelDoc2 model = null;
        try
        {
            var modeler = new SolidWorksModeler(connector.Application);
            model = modeler.CreateWall(180, 220, 3);
            Console.WriteLine("CASE=" + name + "; NEW_TEST_DOCUMENT=" + model.GetTitle());
            if (name == "BALCONY_UPPER_DOOR")
            {
                const double doorWidth = 30, doorHeight = 60, doorCenterX = 20, thresholdHostElevation = 120;
                double thresholdModelY = thresholdHostElevation - 220.0 / 2;
                double doorCenterY = thresholdModelY + doorHeight / 2;
                // Existing rectangular-aperture API: semantic upper-floor door, not ground CreateDoor.
                modeler.CreateWindow(doorWidth, doorHeight, doorCenterX, doorCenterY);
                modeler.CreateProjectingBalconySlab(new ProjectingBalconySlabInstance("balcony-01", "MainWall",
                    doorWidth + 2 * 13, 24, 3, doorCenterX, thresholdHostElevation));
                CheckApertureCorners(model);
                BalconyCadChecks.MeasureExpected(model, true, new[] { -8.0, 7, 1.5, 48, 10, 25.5 },
                    4032, 113400, 3, 1);
                Console.WriteLine("UPPER_DOOR_THRESHOLD_MODEL_Y=10; SLAB_TOP_MODEL_Y=10; SIDE_OVERHANG_MM=13");
            }
            else
            {
                modeler.CreateProjectingBalconySlab(new ProjectingBalconySlabInstance("balcony-01", "MainWall", 56, 24, 3, 20, 120));
                BalconyCadChecks.Measure(model, true); // Verify fresh baseline before touching dimensions.
                string dimension;
                double value;
                double[] bounds;
                double volume;
                switch (name)
                {
                    case "BALCONY_EDIT_WIDTH":
                        dimension = "SlabWidth@BalconySlabSketch_balcony-01"; value = 60;
                        bounds = new[] { -10.0, 7, 1.5, 50, 10, 25.5 }; volume = 4320; break;
                    case "BALCONY_EDIT_DEPTH":
                        dimension = "SlabDepth@BalconySlab_balcony-01"; value = 26;
                        bounds = new[] { -8.0, 7, 1.5, 48, 10, 27.5 }; volume = 4368; break;
                    case "BALCONY_EDIT_THICKNESS":
                        dimension = "SlabThickness@BalconySlabSketch_balcony-01"; value = 4;
                        bounds = new[] { -8.0, 6.5, 1.5, 48, 10.5, 25.5 }; volume = 5376; break;
                    case "BALCONY_EDIT_POSITION":
                        dimension = "CenterX_balcony-01@BalconySlabSketch_balcony-01"; value = 30;
                        bounds = new[] { 2.0, 7, 1.5, 58, 10, 25.5 }; volume = 4032; break;
                    case "BALCONY_EDIT_ELEVATION":
                        dimension = "CenterY_balcony-01@BalconySlabSketch_balcony-01";
                        value = 130 - 220.0 / 2 - 3.0 / 2; // Top elevation -> centered sketch Y.
                        bounds = new[] { -8.0, 17, 1.5, 48, 20, 25.5 }; volume = 4032; break;
                    default: throw new ArgumentException("Unknown edit case.");
                }
                var edit = (Dimension)model.Parameter(dimension);
                if (edit == null || edit.DrivenState != (int)swDimensionDrivenState_e.swDimensionDriving)
                    throw new InvalidOperationException("Required driving dimension not found: " + dimension);
                double beforeMm = edit.SystemValue * 1000;
                edit.SystemValue = value / 1000;
                if (!model.EditRebuild3()) throw new InvalidOperationException("Rebuild after parameter edit failed.");
                Console.WriteLine(string.Format(CultureInfo.InvariantCulture,
                    "EDIT={0}; BEFORE_MM={1:R}; REQUESTED_MM={2:R}; ACTUAL_MM={3:R}; EDIT_REBUILD=True",
                    dimension, beforeMm, value, edit.SystemValue * 1000));
                if (Math.Abs(edit.SystemValue * 1000 - value) > .001)
                    throw new InvalidOperationException("Dimension did not accept requested edit.");
                BalconyCadChecks.MeasureExpected(model, true, bounds, volume, 118800, 2, 0);
            }
            Console.WriteLine("CASE_RESULT=" + name + ":PASS; VISUAL_APPROVAL=PENDING");
            return 0;
        }
        catch
        {
            Console.Error.WriteLine("CASE_FAILED=" + name + "; PRESERVED_DOCUMENT=" + (model == null ? "none" : model.GetTitle()));
            throw;
        }
        finally { BalconyCadChecks.CheckExisting(existing); }
    }

    private static void CheckApertureCorners(ModelDoc2 model)
    {
        var bodies = ((PartDoc)model).GetBodies2((int)swBodyType_e.swSolidBody, false) as object[] ?? new object[0];
        bool matchedWall = false;
        foreach (object item in bodies)
        {
            var body = (Body2)item;
            double volume = ((double[])body.GetMassProperties(1))[3] * 1e9;
            if (Math.Abs(volume - 113400) > .01) continue;
            var vertices = body.GetVertices() as object[] ?? new object[0];
            int matched = 0;
            foreach (double x in new[] { 5.0, 35.0 })
                foreach (double y in new[] { 10.0, 70.0 })
                    foreach (double z in new[] { -1.5, 1.5 })
                    {
                        bool found = false;
                        foreach (object vertex in vertices)
                        {
                            var point = (double[])((Vertex)vertex).GetPoint();
                            if (Math.Abs(point[0] * 1000 - x) < .001 && Math.Abs(point[1] * 1000 - y) < .001 &&
                                Math.Abs(point[2] * 1000 - z) < .001) found = true;
                        }
                        if (found) matched++;
                    }
            Console.WriteLine("UPPER_DOOR_APERTURE_MATCHED_CORNERS=" + matched + "/8; BOUNDS_MM=X[5,35],Y[10,70],Z[-1.5,1.5]");
            matchedWall = matched == 8;
        }
        if (!matchedWall) throw new InvalidOperationException("Expected through-wall upper-floor aperture corners not found.");
    }
}
