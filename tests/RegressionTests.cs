using System;
using System.Collections;
using System.Reflection;
using System.Runtime.Serialization;
using SolidWorksAIBridge;

internal static partial class RegressionTests
{
    private static int passed;
    private static int failed;
    private const BindingFlags PrivateStatic = BindingFlags.NonPublic | BindingFlags.Static;
    private const BindingFlags PrivateInstance = BindingFlags.NonPublic | BindingFlags.Instance;

    // Never construct/attach a COM application. Only methods that exit before COM,
    // or pure helpers, are called. Missing guards fail locally rather than reaching CAD.
    private static SolidWorksModeler EmptyModeler()
    {
        return (SolidWorksModeler)FormatterServices.GetUninitializedObject(typeof(SolidWorksModeler));
    }

    private static object Call(string name, params object[] args)
    {
        return typeof(SolidWorksModeler).GetMethod(name, PrivateStatic).Invoke(null, args);
    }

    private static object CallOn(object instance, string name, params object[] args)
    {
        return typeof(SolidWorksModeler).GetMethod(name, PrivateInstance).Invoke(instance, args);
    }

    private static void Equal(double expected, double actual)
    {
        if (double.IsNaN(actual) || Math.Abs(expected - actual) > 1e-12)
            throw new Exception("Expected " + expected + ", actual " + actual);
    }

    private static void Throws<T>(Action action, string parameter = null) where T : Exception
    {
        Exception caught = null;
        try { action(); }
        catch (Exception ex)
        {
            caught = ex is TargetInvocationException ? ex.InnerException : ex;
        }
        if (caught == null || caught.GetType() != typeof(T))
            throw new Exception("Expected " + typeof(T).Name + ", actual " +
                (caught == null ? "no exception" : caught.GetType().Name), caught);
        if (parameter != null && ((ArgumentException)caught).ParamName != parameter)
            throw new Exception("Unexpected parameter name: " + ((ArgumentException)caught).ParamName);
    }

    private static void Test(string name, Action action)
    {
        try { action(); passed++; Console.WriteLine("PASS " + name); }
        catch (Exception ex) { failed++; Console.WriteLine("FAIL " + name + ": " + ex); }
    }

    private static int Main()
    {
        Test("conversion: 180 mm wall width", () => Equal(0.18, UnitConverter.MillimetersToMeters(180)));
        Test("conversion: 3 mm thickness", () => Equal(0.003, UnitConverter.MillimetersToMeters(3)));
        Test("conversion: signed -55 mm position", () => Equal(-0.055, UnitConverter.MillimetersToMeters(-55)));
        Test("conversion: zero", () => Equal(0, UnitConverter.MillimetersToMeters(0)));
        foreach (double invalid in new[] { double.NaN, double.PositiveInfinity, double.NegativeInfinity })
        {
            double value = invalid;
            Test("conversion rejects " + value, () => Throws<ArgumentOutOfRangeException>(
                () => UnitConverter.MillimetersToMeters(value), "millimeters"));
        }

        Test("modeler rejects null application", () => Throws<ArgumentNullException>(
            () => new SolidWorksModeler(null), "application"));
        Test("connector Application requires Connect", () => Throws<InvalidOperationException>(
            () => { var unused = new SolidWorksConnector().Application; }));

        Test("positive helper converts", () => Equal(0.025, (double)Call("PositiveMeters", 25.0, "size")));
        Test("nonnegative helper permits zero", () => Equal(0, (double)Call("NonnegativeMeters", 0.0, "gap")));
        Test("position helper preserves sign", () => Equal(-0.055, (double)Call("PositionMeters", -55.0, "x")));
        foreach (double invalid in new[] { 0.0, -1.0, double.NaN, double.PositiveInfinity, double.NegativeInfinity, double.Epsilon })
        {
            double value = invalid;
            Test("positive helper rejects " + value, () => Throws<ArgumentOutOfRangeException>(
                () => Call("PositiveMeters", value, "size"), "size"));
        }
        foreach (double invalid in new[] { -1.0, double.NaN, double.PositiveInfinity, double.NegativeInfinity })
        {
            double value = invalid;
            Test("nonnegative helper rejects " + value, () => Throws<ArgumentOutOfRangeException>(
                () => Call("NonnegativeMeters", value, "gap"), "gap"));
        }

        Test("wall rejects zero width before COM", () => Throws<ArgumentOutOfRangeException>(
            () => EmptyModeler().CreateWall(0, 220, 3), "widthMm"));
        Test("wall rejects negative height before COM", () => Throws<ArgumentOutOfRangeException>(
            () => EmptyModeler().CreateWall(180, -1, 3), "heightMm"));
        Test("wall rejects nonfinite thickness before COM", () => Throws<ArgumentOutOfRangeException>(
            () => EmptyModeler().CreateWall(180, 220, double.NaN), "thicknessMm"));
        Test("window rejects width", () => Throws<ArgumentOutOfRangeException>(
            () => EmptyModeler().CreateWindow(-1, 40, 0, 0), "widthMm"));
        Test("window rejects height", () => Throws<ArgumentOutOfRangeException>(
            () => EmptyModeler().CreateWindow(30, 0, 0, 0), "heightMm"));
        Test("window rejects X", () => Throws<ArgumentOutOfRangeException>(
            () => EmptyModeler().CreateWindow(30, 40, double.NaN, 0), "centerXmm"));
        Test("window rejects Y", () => Throws<ArgumentOutOfRangeException>(
            () => EmptyModeler().CreateWindow(30, 40, 0, double.PositiveInfinity), "centerYmm"));
        Test("window requires wall", () => Throws<InvalidOperationException>(
            () => EmptyModeler().CreateWindow(30, 40, 0, 0)));
        Test("door rejects width", () => Throws<ArgumentOutOfRangeException>(
            () => EmptyModeler().CreateDoor(0, 60, 0), "widthMm"));
        Test("door rejects height", () => Throws<ArgumentOutOfRangeException>(
            () => EmptyModeler().CreateDoor(30, -1, 0), "heightMm"));
        Test("door rejects X", () => Throws<ArgumentOutOfRangeException>(
            () => EmptyModeler().CreateDoor(30, 60, double.NaN), "centerXmm"));
        Test("door requires wall", () => Throws<InvalidOperationException>(
            () => EmptyModeler().CreateDoor(30, 60, 0)));
        Test("door row rejects null widths", () => Throws<ArgumentNullException>(
            () => EmptyModeler().CreateDoorRow(null, new double[0], new double[0]), "widthsMm"));
        Test("door row rejects null heights", () => Throws<ArgumentNullException>(
            () => EmptyModeler().CreateDoorRow(new double[0], null, new double[0]), "heightsMm"));
        Test("door row rejects null centers", () => Throws<ArgumentNullException>(
            () => EmptyModeler().CreateDoorRow(new double[0], new double[0], null), "centerXmm"));
        Test("door row rejects empty arrays", () => Throws<ArgumentException>(
            () => EmptyModeler().CreateDoorRow(new double[0], new double[0], new double[0])));
        Test("door row rejects unequal arrays", () => Throws<ArgumentException>(
            () => EmptyModeler().CreateDoorRow(new[] { 25.0 }, new[] { 55.0, 65.0 }, new[] { 0.0 })));
        Test("door row requires wall", () => Throws<InvalidOperationException>(
            () => EmptyModeler().CreateDoorRow(new[] { 25.0 }, new[] { 55.0 }, new[] { 0.0 })));
        Test("row rejects zero count", () => Throws<ArgumentOutOfRangeException>(
            () => EmptyModeler().CreateWindowRow(0, 20, 30, 10, 0, 0, false, 0, 0), "count"));
        Test("row rejects negative gap", () => Throws<ArgumentOutOfRangeException>(
            () => EmptyModeler().CreateWindowRow(3, 20, 30, -1, 0, 0, false, 0, 0), "spacingMm"));
        Test("grid rejects zero floors", () => Throws<ArgumentOutOfRangeException>(
            () => EmptyModeler().CreateWindowGrid(0, 3, 20, 30, 10, 20, 0, 0, false, 0, 0), "floorCount"));
        Test("grid rejects zero columns", () => Throws<ArgumentOutOfRangeException>(
            () => EmptyModeler().CreateWindowGrid(2, 0, 20, 30, 10, 20, 0, 0, false, 0, 0), "windowsPerFloor"));
        Test("grid rejects negative vertical gap", () => Throws<ArgumentOutOfRangeException>(
            () => EmptyModeler().CreateWindowGrid(2, 3, 20, 30, 10, -1, 0, 0, false, 0, 0), "verticalGapMm"));

        Test("cut tolerance accepts zero and 0.2; preserves value on rejection", () =>
        {
            var modeler = EmptyModeler();
            modeler.OpeningCutToleranceMm = 0;
            Equal(0, modeler.OpeningCutToleranceMm);
            modeler.OpeningCutToleranceMm = 0.2;
            Throws<ArgumentOutOfRangeException>(() => modeler.OpeningCutToleranceMm = -1, "OpeningCutToleranceMm");
            Equal(0.2, modeler.OpeningCutToleranceMm);
            Throws<ArgumentOutOfRangeException>(() => modeler.OpeningCutToleranceMm = double.NaN, "OpeningCutToleranceMm");
            Equal(0.2, modeler.OpeningCutToleranceMm);
        });

        // Exercise real production bounds storage/calculation, not a copied formula.
        Test("bounds stores actual half-width/height extents", () =>
        {
            var modeler = BoundsModeler();
            CallOn(modeler, "RememberBounds", 0.030, 0.040, -0.020, 0.010);
            var list = (IList)typeof(SolidWorksModeler).GetField("occupiedBounds", PrivateInstance).GetValue(modeler);
            object bounds = list[list.Count - 1];
            Equal(-0.035, (double)bounds.GetType().GetField("Left").GetValue(bounds));
            Equal(-0.005, (double)bounds.GetType().GetField("Right").GetValue(bounds));
            Equal(-0.010, (double)bounds.GetType().GetField("Bottom").GetValue(bounds));
            Equal(0.030, (double)bounds.GetType().GetField("Top").GetValue(bounds));
        });
        Test("bounds rejects positive-area overlap", () =>
        {
            var modeler = BoundsModeler();
            Throws<ArgumentException>(() => CallOn(modeler, "ValidateNoOverlap", .02, .02, .005, 0.0));
        });
        Test("bounds allows disjoint rectangles", () => CallOn(BoundsModeler(), "ValidateNoOverlap", .02, .02, .03, 0.0));
        Test("bounds allows shared edge", () => CallOn(BoundsModeler(), "ValidateNoOverlap", .02, .02, .02, 0.0));
        Test("bounds allows shared corner", () => CallOn(BoundsModeler(), "ValidateNoOverlap", .02, .02, .02, .02));
        Test("bounds honors 1e-10 meter overlap tolerance", () => CallOn(BoundsModeler(), "ValidateNoOverlap", .02, .02, .02 - 5e-11, 0.0));
        Test("bounds rejects overlap beyond tolerance", () => Throws<ArgumentException>(
            () => CallOn(BoundsModeler(), "ValidateNoOverlap", .02, .02, .02 - 2e-10, 0.0)));

        BalconySlabTests();
        FeatureClassificationTests();
        AssemblyTests();
        Console.WriteLine("RESULT: " + passed + " passed, " + failed + " failed. CAD tests: NOT RUN.");
        return failed == 0 ? 0 : 1;
    }

    private static SolidWorksModeler BoundsModeler()
    {
        var modeler = EmptyModeler();
        FieldInfo field = typeof(SolidWorksModeler).GetField("occupiedBounds", PrivateInstance);
        field.SetValue(modeler, Activator.CreateInstance(field.FieldType));
        CallOn(modeler, "RememberBounds", .02, .02, 0.0, 0.0);
        return modeler;
    }
}
