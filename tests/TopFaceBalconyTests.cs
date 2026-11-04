using System;
using SolidWorksAIBridge;

internal static partial class RegressionTests
{
    private static BalconyAssemblyPlan TopPlan(double width = 56, double depth = 24,
        double x = 20, double left = 2, double right = 2)
    {
        return TopFaceBalconyAssembly.Validate(new BalconyAssemblyInstance("top-both",
            new ProjectingBalconySlabInstance("top-slab", "MainWall", width, depth, 3, x, 120),
            20, left, 20, right, .8), new BalconySlabHost("MainWall", 180, 220, 3));
    }

    private static void TopFaceTests()
    {
        Test("top face: expected U-section, distinct loops, full slab contact", () =>
        {
            var p = TopPlan();
            Equal(-8, p.Left.LeftMm); Equal(-6, p.Left.RightMm);
            Equal(46, p.Right.LeftMm); Equal(48, p.Right.RightMm);
            Equal(10, p.Left.BottomMm); Equal(30, p.Left.TopMm);
            foreach (var wall in new[] { p.Left, p.Right })
            {
                Equal(1.5, wall.RearMm); Equal(25.5, wall.FrontMm);
                Equal(48, wall.SlabContactAreaMm2);
            }
            if (p.Left.RightMm >= p.Right.LeftMm) throw new Exception("Profiles overlap.");
            Equal(5952, p.VolumeMm3);
        });
        foreach (double depth in new[] { 24.0, 26, 24 })
        {
            double d = depth;
            Test("top face: analytic depth " + d + " (not CAD propagation)", () =>
            {
                var p = TopPlan(depth: d);
                Equal(d, p.Left.DepthMm); Equal(d, p.Right.DepthMm);
                Equal(d == 26 ? 6448 : 5952, p.VolumeMm3);
            });
        }
        Test("top face: width edit moves outer edges and keeps independent thickness", () =>
        {
            var p = TopPlan(width: 60, left: 3, right: 2);
            Equal(-10, p.Left.LeftMm); Equal(-7, p.Left.RightMm);
            Equal(48, p.Right.LeftMm); Equal(50, p.Right.RightMm);
            Equal(6720, p.VolumeMm3);
        });
        Test("top face: six exact equation targets, unitful rear offset and child references", () =>
        {
            string[] e = TopFaceBalconyAssembly.Equations(TopPlan());
            string[] expected = {
                "\"Depth_Left@BalconyTopWallSketch_top-both\" = \"SlabDepth@BalconySlab_top-slab\"",
                "\"CenterX_Left@BalconyTopWallSketch_top-both\" = abs(\"CenterX_top-slab@BalconySlabSketch_top-slab\" - (\"SlabWidth@BalconySlabSketch_top-slab\" - \"Thickness_Left@BalconyTopWallSketch_top-both\") / 2)",
                "\"CenterZ_Left@BalconyTopWallSketch_top-both\" = 1.5mm + \"SlabDepth@BalconySlab_top-slab\" / 2",
                "\"Depth_Right@BalconyTopWallSketch_top-both\" = \"SlabDepth@BalconySlab_top-slab\"",
                "\"CenterX_Right@BalconyTopWallSketch_top-both\" = abs(\"CenterX_top-slab@BalconySlabSketch_top-slab\" + (\"SlabWidth@BalconySlabSketch_top-slab\" - \"Thickness_Right@BalconyTopWallSketch_top-both\") / 2)",
                "\"CenterZ_Right@BalconyTopWallSketch_top-both\" = 1.5mm + \"SlabDepth@BalconySlab_top-slab\" / 2"
            };
            if (e.Length != expected.Length) throw new Exception("Wrong binding count.");
            for (int i = 0; i < e.Length; i++) if (e[i] != expected[i]) throw new Exception("Wrong binding: " + e[i]);
        });
        Test("top face: negative slab placement retains signed reference", () =>
        {
            string[] e = TopFaceBalconyAssembly.Equations(TopPlan(x: -20));
            if (!e[1].Contains("abs(-\"CenterX_top-slab@") || !e[4].Contains("abs(-\"CenterX_top-slab@"))
                throw new Exception("Lost negative center branch.");
        });
        foreach (double signX in new[] { -1.0, 1 })
            foreach (double signZ in new[] { -1.0, 1 })
            {
                double sx = signX, sz = signZ;
                Test("top face: signed X/Z axes " + sx + "/" + sz, () =>
                {
                    if (!TopFaceBalconyAssembly.XIsHorizontal(sx, 0, 0, sz) ||
                        TopFaceBalconyAssembly.XIsHorizontal(0, sx, sz, 0)) throw new Exception("Wrong swapped/signed mapping.");
                });
            }
        foreach (double bad in new[] { 0.0, .5, double.NaN, double.PositiveInfinity })
        {
            double value = bad;
            Test("top face: rejects non-unit/nonfinite basis " + value, () => Throws<InvalidOperationException>(
                () => TopFaceBalconyAssembly.XIsHorizontal(value, 0, 0, 1)));
        }
        Test("top face: rejects rotated/skew basis", () => Throws<InvalidOperationException>(
            () => TopFaceBalconyAssembly.XIsHorizontal(.707, .707, -.707, .707)));
        Test("top face: unequal heights rejected before CAD", () => Throws<ArgumentException>(
            () => TopFaceBalconyAssembly.Validate(AssemblyInstance(rightHeight: 19), new BalconySlabHost("MainWall", 180, 220, 3))));
        Test("top face: thickness cannot close interior", () => Throws<ArgumentException>(() => TopPlan(left: 54)));
        Test("top face: host bounds retained", () => Throws<ArgumentException>(() => TopPlan(width: 200)));
        Test("top face: minimum printable section retained", () => Throws<ArgumentException>(() => TopPlan(left: .7)));
        Test("top face: null input rejected", () => Throws<ArgumentNullException>(
            () => TopFaceBalconyAssembly.Validate(null, new BalconySlabHost("MainWall", 180, 220, 3)), "instance"));
        Test("top face: executor rejects null without COM", () => Throws<ArgumentNullException>(
            () => EmptyModeler().CreateTopFaceBalconyAssembly(null), "instance"));
        Test("top face: executor requires host before COM", () => Throws<InvalidOperationException>(
            () => EmptyModeler().CreateTopFaceBalconyAssembly(AssemblyInstance())));
        Test("top face: lifecycle and names cannot replace baseline", () =>
        {
            var p = TopPlan();
            if (TopFaceBalconyAssembly.Status != "Validated" ||
                TopFaceBalconyAssembly.Id != "lego.balcony.projecting-assembly.equal-height-top-face" ||
                TopFaceBalconyAssembly.Version != "1.0.0" ||
                TopFaceBalconyAssembly.Recipe != "top-face-shared-r1" ||
                TopFaceBalconyAssembly.ValidationRecord != "tests/results/TOP_FACE_VALIDATED_2026-11-04.md" ||
                BalconyAssemblyDefinition.Status != "Testing" || p.CadStatus != "NotRun" ||
                TopFaceBalconyAssembly.FeatureName(p) == p.Left.FeatureName ||
                TopFaceBalconyAssembly.SketchName(p) == p.Right.SketchName || ProjectingBalconySlabDefinition.Status != "Validated")
                throw new Exception("Lifecycle or baseline separation lost.");
        });
    }
}
