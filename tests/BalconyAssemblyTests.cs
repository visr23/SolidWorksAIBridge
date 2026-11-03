using System;
using SolidWorksAIBridge;

internal static partial class RegressionTests
{
    private static BalconyAssemblyInstance AssemblyInstance(double leftHeight = 20, double leftThickness = 2,
        double rightHeight = 20, double rightThickness = 2, double minimum = .8, double depth = 24,
        string id = "both-walls", double x = 20)
    {
        return new BalconyAssemblyInstance(id,
            new ProjectingBalconySlabInstance("both-walls_slab", "MainWall", 56, depth, 3, x, 120),
            leftHeight, leftThickness, rightHeight, rightThickness, minimum);
    }
    private static BalconyAssemblyPlan AssemblyPlan(BalconyAssemblyInstance instance) =>
        BalconyAssemblyDefinition.Validate(instance, new BalconySlabHost("MainWall", 180, 220, 3));

    private static void AssemblyTests()
    {
        Test("assembly: both walls exact bounds and total volume", () =>
        {
            var p = AssemblyPlan(AssemblyInstance());
            Equal(-8, p.Left.LeftMm); Equal(-6, p.Left.RightMm);
            Equal(46, p.Right.LeftMm); Equal(48, p.Right.RightMm);
            Equal(10, p.Left.BottomMm); Equal(30, p.Left.TopMm);
            Equal(10, p.Right.BottomMm); Equal(30, p.Right.TopMm);
            Equal(5952, p.VolumeMm3);
        });
        Test("assembly: positive area slab contacts and zero rear/front gaps", () =>
        {
            var p = AssemblyPlan(AssemblyInstance());
            foreach (var wall in new[] { p.Left, p.Right })
            {
                Equal(p.Slab.TopMm, wall.BottomMm); Equal(48, wall.SlabContactAreaMm2);
                Equal(p.Slab.RearMm, wall.RearMm); Equal(p.Slab.FrontMm, wall.FrontMm);
                Equal(24, wall.DepthMm);
            }
        });
        Test("assembly: configurable independent wall heights and thicknesses", () =>
        {
            var p = AssemblyPlan(AssemblyInstance(leftHeight: 15, leftThickness: 3, rightHeight: 25, rightThickness: 2));
            Equal(15, p.Left.HeightMm); Equal(3, p.Left.ThicknessMm);
            Equal(25, p.Right.HeightMm); Equal(2, p.Right.ThicknessMm);
            Equal(6312, p.VolumeMm3);
        });
        Test("assembly: depth derives from child slab", () =>
        {
            var p = AssemblyPlan(AssemblyInstance(depth: 26));
            Equal(26, p.Left.DepthMm); Equal(26, p.Right.DepthMm); Equal(6448, p.VolumeMm3);
        });
        Test("assembly: depth binding targets exact slab feature", () =>
        {
            var p = AssemblyPlan(AssemblyInstance());
            string expected = "\"SideWallDepth@BalconySideWall_both-walls_Left\" = \"SlabDepth@BalconySlab_both-walls_slab\"";
            if (BalconyAssemblyDefinition.DepthEquation(p, p.Left) != expected) throw new Exception("Wrong child depth binding.");
        });
        Test("assembly: print underside maps to zero; walls grow upward", () =>
        {
            var p = AssemblyPlan(AssemblyInstance());
            Equal(0, p.PrintHeightMm(p.Slab.BottomMm)); Equal(3, p.PrintHeightMm(p.Left.BottomMm));
            Equal(23, p.PrintHeightMm(p.Left.TopMm));
            if (p.PreferredBuildDirection != "+Y") throw new Exception("Incorrect print direction.");
        });
        Test("assembly: negative placement translates both walls", () =>
        {
            var p = AssemblyPlan(AssemblyInstance(x: -20));
            Equal(-48, p.Left.LeftMm); Equal(8, p.Right.RightMm);
        });
        Test("assembly: independent child names; no false CAD validation", () =>
        {
            var p = AssemblyPlan(AssemblyInstance());
            if (p.Left.SketchName == p.Right.SketchName || p.Left.FeatureName == p.Right.FeatureName ||
                p.CadStatus != "NotRun" || BalconyAssemblyDefinition.Status != "Testing" ||
                BalconyAssemblyDefinition.SlabDefinitionVersion != ProjectingBalconySlabDefinition.Version)
                throw new Exception("Invalid lifecycle/child metadata.");
        });
        Test("assembly: null instance", () => Throws<ArgumentNullException>(() => AssemblyPlan(null), "instance"));
        Test("assembly: null slab", () => Throws<ArgumentNullException>(() => AssemblyPlan(
            new BalconyAssemblyInstance("a", null, 20, 2, 20, 2, .8)), "Slab"));
        Test("assembly: null host", () => Throws<ArgumentNullException>(
            () => BalconyAssemblyDefinition.Validate(AssemblyInstance(), null), "host"));
        foreach (string id in new[] { null, "", "bad id", "bad\"id", new string('a', 41), "both-walls_slab" })
        {
            string value = id;
            Test("assembly: invalid/duplicate ID " + (value ?? "null"), () => Throws<ArgumentException>(
                () => AssemblyPlan(AssemblyInstance(id: value)), "Id"));
        }
        foreach (double invalid in new[] { 0.0, -1.0, double.NaN, double.PositiveInfinity, double.Epsilon })
        {
            double value = invalid;
            Test("assembly: invalid left height " + value, () => Throws<ArgumentOutOfRangeException>(
                () => AssemblyPlan(AssemblyInstance(leftHeight: value)), "LeftHeightMm"));
            Test("assembly: invalid right thickness " + value, () => Throws<ArgumentOutOfRangeException>(
                () => AssemblyPlan(AssemblyInstance(rightThickness: value)), "RightThicknessMm"));
        }
        Test("assembly: printer minimum required", () => Throws<ArgumentOutOfRangeException>(
            () => AssemblyPlan(AssemblyInstance(minimum: 0)), "MinimumPrintableThicknessMm"));
        Test("assembly: left wall below printer minimum", () => Throws<ArgumentException>(
            () => AssemblyPlan(AssemblyInstance(leftThickness: .7)), "MinimumPrintableThicknessMm"));
        Test("assembly: right wall below printer minimum", () => Throws<ArgumentException>(
            () => AssemblyPlan(AssemblyInstance(rightThickness: .7)), "MinimumPrintableThicknessMm"));
        Test("assembly: child slab below printer minimum", () => Throws<ArgumentException>(
            () => AssemblyPlan(AssemblyInstance(minimum: 3.1)), "MinimumPrintableThicknessMm"));
        Test("assembly: no clear width remains", () => Throws<ArgumentException>(
            () => AssemblyPlan(AssemblyInstance(leftThickness: 28, rightThickness: 28))));
        Test("assembly: right wall exceeds host top", () => Throws<ArgumentException>(
            () => AssemblyPlan(AssemblyInstance(rightHeight: 101))));
        Test("assembly: slab exceeds horizontal host boundary", () => Throws<ArgumentException>(
            () => AssemblyPlan(AssemblyInstance(x: 70)), "CenterXMm"));
        Test("assembly: CAD API rejects null before COM", () => Throws<ArgumentNullException>(
            () => EmptyModeler().CreateBalconyAssembly(null), "instance"));
        Test("assembly: CAD API requires host before COM", () => Throws<InvalidOperationException>(
            () => EmptyModeler().CreateBalconyAssembly(AssemblyInstance())));
    }
}
