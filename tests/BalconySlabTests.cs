using System;
using SolidWorksAIBridge;

internal static partial class RegressionTests
{
    private static ProjectingBalconySlabInstance Slab(double width = 56, double depth = 24,
        double thickness = 3, double x = 20, double elevation = 120, string id = "balcony-01", string host = "MainWall")
    {
        return new ProjectingBalconySlabInstance(id, host, width, depth, thickness, x, elevation);
    }

    private static BalconySlabValidationRecord ValidateSlab(ProjectingBalconySlabInstance instance)
    {
        return ProjectingBalconySlabDefinition.Validate(instance, new BalconySlabHost("MainWall", 180, 220, 3));
    }

    private static void BalconySlabTests()
    {
        Test("threshold fixture: explicit centered placement and equal overhang", () =>
        {
            var instance = BalconyDoorThresholdCase.CreateInstance();
            Equal(56, instance.WidthMm);
            Equal(BalconyDoorThresholdCase.DoorCenterXMm, instance.CenterXMm);
            Equal(0, instance.ElevationMm);
            Equal(13, (instance.WidthMm - BalconyDoorThresholdCase.DoorWidthMm) / 2);
            Equal(-110, instance.ElevationMm - BalconyDoorThresholdCase.WallHeightMm / 2);
        });
        Test("threshold fixture: V1 rejects slab below host before CAD", () =>
            Throws<ArgumentException>(() => BalconyDoorThresholdCase.Preflight(), "ElevationMm"));
        Test("slab: resolved front-facade bounds and elevation", () =>
        {
            var result = ValidateSlab(Slab());
            Equal(-8, result.LeftMm); Equal(48, result.RightMm);
            Equal(7, result.BottomMm); Equal(10, result.TopMm); Equal(8.5, result.CenterYMm);
            Equal(1.5, result.RearMm); Equal(25.5, result.FrontMm);
        });
        Test("slab: negative horizontal position", () =>
        {
            var result = ValidateSlab(Slab(x: -20));
            Equal(-48, result.LeftMm); Equal(8, result.RightMm);
        });
        Test("slab: boundary contact permitted", () =>
        {
            var result = ValidateSlab(Slab(width: 180, x: 0, elevation: 3));
            Equal(-90, result.LeftMm); Equal(90, result.RightMm); Equal(-110, result.BottomMm);
            Equal(110, ValidateSlab(Slab(elevation: 220)).TopMm);
        });
        Test("slab: custom host dimensions determine face and datum", () =>
        {
            var result = ProjectingBalconySlabDefinition.Validate(Slab(host: "side-snapshot", elevation: 100),
                new BalconySlabHost("side-snapshot", 200, 300, 4));
            Equal(-50, result.TopMm); Equal(2, result.RearMm); Equal(26, result.FrontMm);
        });
        Test("slab: definition/instance/evidence remain distinct", () =>
        {
            var instance = Slab();
            var result = ValidateSlab(instance);
            if (!ReferenceEquals(instance, result.Instance) || result.CadStatus != "NotRun" ||
                !result.ParametersAndPlacementPassed || ProjectingBalconySlabDefinition.Status != "Validated" ||
                ProjectingBalconySlabDefinition.ValidationScope != "tests/results/PROJECTING_SLAB_V1_VALIDATED_2026-11-03.md" ||
                result.DefinitionVersion != "1.0.0" || result.FeatureName != "BalconySlab_balcony-01")
                throw new Exception("Incorrect component metadata or validation claim.");
        });
        Test("slab: null instance", () => Throws<ArgumentNullException>(() => ValidateSlab(null), "instance"));
        Test("slab: null host", () => Throws<ArgumentNullException>(
            () => ProjectingBalconySlabDefinition.Validate(Slab(), null), "host"));
        Test("slab: wrong host dependency", () => Throws<ArgumentException>(() => ValidateSlab(Slab(host: "other")), "HostId"));
        foreach (string id in new[] { null, "", "bad id", "bad@name", "1first", "bad\n", new string('a', 65) })
        {
            string invalid = id;
            Test("slab: unsafe/invalid ID " + (id ?? "null"), () => Throws<ArgumentException>(
                () => ValidateSlab(Slab(id: invalid)), "Id"));
        }
        foreach (double value in new[] { 0.0, -1.0, double.NaN, double.PositiveInfinity, double.Epsilon })
        {
            double invalid = value;
            Test("slab: invalid width " + value, () => Throws<ArgumentOutOfRangeException>(() => ValidateSlab(Slab(width: invalid)), "WidthMm"));
            Test("slab: invalid depth " + value, () => Throws<ArgumentOutOfRangeException>(() => ValidateSlab(Slab(depth: invalid)), "DepthMm"));
            Test("slab: invalid thickness " + value, () => Throws<ArgumentOutOfRangeException>(() => ValidateSlab(Slab(thickness: invalid)), "ThicknessMm"));
        }
        Test("slab: nonfinite X", () => Throws<ArgumentOutOfRangeException>(() => ValidateSlab(Slab(x: double.NaN)), "CenterXMm"));
        Test("slab: nonfinite elevation", () => Throws<ArgumentOutOfRangeException>(() => ValidateSlab(Slab(elevation: double.NegativeInfinity)), "ElevationMm"));
        Test("slab: too wide", () => Throws<ArgumentException>(() => ValidateSlab(Slab(width: 181)), "CenterXMm"));
        Test("slab: right boundary exceeded", () => Throws<ArgumentException>(() => ValidateSlab(Slab(x: 62.001)), "CenterXMm"));
        Test("slab: left boundary exceeded", () => Throws<ArgumentException>(() => ValidateSlab(Slab(x: -62.001)), "CenterXMm"));
        Test("slab: below wall bottom", () => Throws<ArgumentException>(() => ValidateSlab(Slab(elevation: 2.999)), "ElevationMm"));
        Test("slab: above wall top", () => Throws<ArgumentException>(() => ValidateSlab(Slab(elevation: 220.001)), "ElevationMm"));
        Test("slab: invalid host dimension", () => Throws<ArgumentOutOfRangeException>(
            () => ProjectingBalconySlabDefinition.Validate(Slab(), new BalconySlabHost("MainWall", 0, 220, 3)), "host.WidthMm"));
        Test("slab: depth lost to coordinate precision", () => Throws<ArgumentException>(() => ValidateSlab(Slab(depth: 1e-20))));
        Test("slab: elevation arithmetic lost to precision", () => Throws<ArgumentException>(
            () => ProjectingBalconySlabDefinition.Validate(Slab(elevation: 3), new BalconySlabHost("MainWall", 180, 1e300, 3))));
        Test("slab: CAD API null instance before COM", () => Throws<ArgumentNullException>(
            () => EmptyModeler().CreateProjectingBalconySlab(null), "instance"));
        Test("slab: CAD API requires successful host before COM", () => Throws<InvalidOperationException>(
            () => EmptyModeler().CreateProjectingBalconySlab(Slab())));
    }
}
