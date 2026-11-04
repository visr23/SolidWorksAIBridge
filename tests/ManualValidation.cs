using System;
using SolidWorksAIBridge;

internal static class ManualValidation
{
    [STAThread]
    private static int Main(string[] args)
    {
        if (args.Length == 2 && args[0] == "--confirm-cad" && args[1] == "BALCONY_TOP_FACE_COMPARE")
        {
            try { return TopFaceBalconyCadCase.Run(); }
            catch (Exception ex) { Console.Error.WriteLine(ex); return 1; }
        }
        if (args.Length == 2 && args[0] == "--confirm-cad" && args[1] == "BALCONY_BOTH_WALLS")
        {
            try { return BalconyAssemblyCadCase.Run(); }
            catch (Exception ex) { Console.Error.WriteLine(ex); return 1; }
        }
        if (args.Length == 2 && args[0] == "--confirm-cad" && BalconyEditValidation.IsCase(args[1]))
        {
            try { return BalconyEditValidation.Run(args[1]); }
            catch (Exception ex) { Console.Error.WriteLine(ex); return 1; }
        }
        if (args.Length == 2 && args[0] == "--inspect-balcony-result")
        {
            try
            {
                var readOnly = new SolidWorksConnector();
                readOnly.Connect();
                var docs = readOnly.Application.GetDocuments() as object[] ?? new object[0];
                foreach (object document in docs)
                {
                    var model = (SldWorks.ModelDoc2)document;
                    if (model.GetTitle() != args[1] || model.GetPathName() != "") continue;
                    if (((SldWorks.PartDoc)model).FeatureByName("BalconySlab_balcony-01") == null) continue;
                    Console.WriteLine("READ_ONLY_RESULT_INSPECTION=" + model.GetTitle());
                    foreach (string name in new[] { "WallSketch", "MainWall", "BalconySlabSketch_balcony-01", "BalconySlab_balcony-01" })
                    {
                        var feature = ((SldWorks.PartDoc)model).FeatureByName(name);
                        Console.WriteLine("FEATURE=" + name + "; TYPE2=" + feature.GetTypeName2() + "; UNDERLYING_TYPE=" + feature.GetTypeName());
                    }
                    foreach (string name in new[] { "SlabWidth@BalconySlabSketch_balcony-01", "SlabThickness@BalconySlabSketch_balcony-01", "SlabDepth@BalconySlab_balcony-01" })
                    {
                        var dimension = (SldWorks.Dimension)model.Parameter(name);
                        Console.WriteLine("DIMENSION=" + name + "; MM=" + (dimension.SystemValue * 1000).ToString("R", System.Globalization.CultureInfo.InvariantCulture));
                    }
                    return 0;
                }
                throw new InvalidOperationException("Expected unsaved balcony test result was not found; nothing inspected or changed.");
            }
            catch (Exception ex) { Console.Error.WriteLine(ex); return 1; }
        }
        if (args.Length == 1 && args[0] == "--inspect-connection")
        {
            try
            {
                var readOnly = new SolidWorksConnector();
                readOnly.Connect();
                BalconyCadChecks.InspectConnection(readOnly.Application);
                return 0;
            }
            catch (Exception ex) { Console.Error.WriteLine(ex); return 1; }
        }
        // No connection or document mutation without an explicit case and opt-in.
        string[] cases = { "WALL", "WINDOW", "SILL", "ROW", "GRID", "DOOR", "DOOR_ROW", "INVALID", "BALCONY", "BALCONY_MIXED", "BALCONY_DOOR_THRESHOLD" };
        if (args.Length != 2 || args[0] != "--confirm-cad" || Array.IndexOf(cases, args[1]) < 0)
        {
            Console.Error.WriteLine("MANUAL CAD ONLY: --confirm-cad WALL|WINDOW|SILL|ROW|GRID|DOOR|DOOR_ROW|INVALID|BALCONY|BALCONY_MIXED|BALCONY_DOOR_THRESHOLD");
            return 2;
        }
        try
        {
            if (args[1] == "BALCONY_DOOR_THRESHOLD")
            {
                // Preflight before COM/document creation; do not raise the threshold
                // or substitute BALCONY_MIXED to hide the current API limitation.
                try { BalconyDoorThresholdCase.Preflight(); }
                catch (ArgumentException ex)
                {
                    Console.Error.WriteLine("BLOCKED before CAD: " + ex.Message);
                    return 3;
                }
            }
            var connector = new SolidWorksConnector();
            connector.Connect(); // Attach only; never launch.
            var modeler = new SolidWorksModeler(connector.Application);
            if (args[1] == "BALCONY") BalconyCadChecks.InspectConnection(connector.Application);
            var existingDocuments = args[1] == "BALCONY" ? BalconyCadChecks.Snapshot(connector.Application) : null;
            var testDocument = modeler.CreateWall(180, 220, 3);
            Console.WriteLine("NEW_TEST_DOCUMENT=" + testDocument.GetTitle());
            switch (args[1])
            {
                case "WINDOW": modeler.CreateWindow(30, 40, -20, 30); break;
                case "SILL":
                    modeler.CreateWindow(30, 40, -20, 30);
                    modeler.CreateWindowSill(36, 3, 2);
                    break;
                case "ROW": modeler.CreateWindowRow(3, 20, 30, 10, 0, 20, true, 3, 2); break;
                case "GRID": modeler.CreateWindowGrid(2, 3, 20, 30, 10, 20, 0, 10, true, 3, 2); break;
                case "DOOR": modeler.CreateDoor(30, 60, 0); break;
                case "BALCONY":
                    try
                    {
                        modeler.CreateProjectingBalconySlab(new ProjectingBalconySlabInstance("balcony-01", "MainWall", 56, 24, 3, 20, 120));
                        BalconyCadChecks.Measure(testDocument, true);
                    }
                    catch
                    {
                        try { BalconyCadChecks.Measure(testDocument, false); }
                        catch (Exception diagnostic) { Console.Error.WriteLine("Partial measurement failed: " + diagnostic.Message); }
                        throw;
                    }
                    finally { BalconyCadChecks.CheckExisting(existingDocuments); }
                    break;
                case "BALCONY_DOOR_THRESHOLD":
                    modeler.CreateDoor(BalconyDoorThresholdCase.DoorWidthMm,
                        BalconyDoorThresholdCase.DoorHeightMm, BalconyDoorThresholdCase.DoorCenterXMm);
                    modeler.CreateProjectingBalconySlab(BalconyDoorThresholdCase.CreateInstance());
                    break;
                case "BALCONY_MIXED":
                    modeler.CreateWindow(30, 40, -50, 40);
                    modeler.CreateDoor(30, 60, 0);
                    modeler.CreateProjectingBalconySlab(new ProjectingBalconySlabInstance("balcony-01", "MainWall", 56, 24, 3, 20, 120));
                    break;
                case "DOOR_ROW":
                    modeler.CreateDoorRow(new[] { 25.0, 35.0, 45.0 }, new[] { 55.0, 65.0, 60.0 }, new[] { -55.0, 0.0, 55.0 });
                    break;
                case "INVALID":
                    ExpectArgument(() => modeler.CreateWindow(30, 40, 75, 0)); // touches right edge
                    ExpectArgument(() => modeler.CreateDoor(30, 221, 0));
                    ExpectArgument(() => modeler.CreateDoorRow(new[] { 30.0, 30.0 }, new[] { 60.0, 60.0 }, new[] { -15.0, 15.0 }));
                    break;
            }
            Console.WriteLine("Case generated. NOT a visual PASS: measure and record results using tests/README.md.");
            return 0;
        }
        catch (Exception ex) { Console.Error.WriteLine(ex); return 1; }
    }

    private static void ExpectArgument(Action action)
    {
        try { action(); }
        catch (ArgumentException ex) { Console.WriteLine("Expected rejection: " + ex.Message); return; }
        throw new Exception("Expected parameter rejection did not occur.");
    }
}
