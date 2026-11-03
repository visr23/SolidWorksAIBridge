using System;
using System.Text.RegularExpressions;

namespace SolidWorksAIBridge
{
    /// <summary>Both-walls V1 composite. All values are resolved actual miniature mm.</summary>
    public sealed class BalconyAssemblyInstance
    {
        public string Id { get; }
        public ProjectingBalconySlabInstance Slab { get; }
        public double LeftHeightMm { get; }
        public double LeftThicknessMm { get; }
        public double RightHeightMm { get; }
        public double RightThicknessMm { get; }
        public double MinimumPrintableThicknessMm { get; }

        public BalconyAssemblyInstance(string id, ProjectingBalconySlabInstance slab,
            double leftHeightMm, double leftThicknessMm, double rightHeightMm, double rightThicknessMm,
            double minimumPrintableThicknessMm)
        {
            Id = id; Slab = slab;
            LeftHeightMm = leftHeightMm; LeftThicknessMm = leftThicknessMm;
            RightHeightMm = rightHeightMm; RightThicknessMm = rightThicknessMm;
            MinimumPrintableThicknessMm = minimumPrintableThicknessMm;
        }
    }

    public sealed class BalconySideWallPlan
    {
        public string Role { get; }
        public string SketchName { get; }
        public string FeatureName { get; }
        public string DimensionSuffix { get; }
        public double LeftMm { get; }
        public double RightMm { get; }
        public double BottomMm { get; }
        public double TopMm { get; }
        public double RearMm { get; }
        public double FrontMm { get; }
        public double ThicknessMm => RightMm - LeftMm;
        public double HeightMm => TopMm - BottomMm;
        public double DepthMm => FrontMm - RearMm;
        public double CenterXMm => LeftMm + ThicknessMm / 2;
        public double CenterYMm => BottomMm + HeightMm / 2;
        public double SlabContactAreaMm2 => ThicknessMm * DepthMm;
        public double VolumeMm3 => SlabContactAreaMm2 * HeightMm;

        internal BalconySideWallPlan(string role, string id, double left, double right,
            double bottom, double top, double rear, double front)
        {
            Role = role; DimensionSuffix = "_" + id + "_" + role;
            SketchName = "BalconySideWallSketch_" + id + "_" + role;
            FeatureName = "BalconySideWall_" + id + "_" + role;
            LeftMm = left; RightMm = right; BottomMm = bottom; TopMm = top; RearMm = rear; FrontMm = front;
        }
    }

    /// <summary>Preflight result only: analytic contact is not measured CAD connectivity.</summary>
    public sealed class BalconyAssemblyPlan
    {
        public BalconyAssemblyInstance Instance { get; }
        public BalconySlabValidationRecord Slab { get; }
        public BalconySideWallPlan Left { get; }
        public BalconySideWallPlan Right { get; }
        public string CadStatus => "NotRun";
        public string PreferredBuildDirection => "+Y";
        public double VolumeMm3 => Instance.Slab.WidthMm * Instance.Slab.DepthMm * Instance.Slab.ThicknessMm + Left.VolumeMm3 + Right.VolumeMm3;
        public double PrintHeightMm(double modelYMm) => modelYMm - Slab.BottomMm;

        internal BalconyAssemblyPlan(BalconyAssemblyInstance instance, BalconySlabValidationRecord slab,
            BalconySideWallPlan left, BalconySideWallPlan right)
        { Instance = instance; Slab = slab; Left = left; Right = right; }
    }

    public static class BalconyAssemblyDefinition
    {
        public const string Id = "lego.balcony.projecting-assembly";
        public const string Version = "1.0.0";
        public const string Status = "Testing";
        public const string SlabDefinitionVersion = "1.0.0";

        public static BalconyAssemblyPlan Validate(BalconyAssemblyInstance instance, BalconySlabHost host)
        {
            if (instance == null) throw new ArgumentNullException(nameof(instance));
            if (instance.Slab == null) throw new ArgumentNullException("Slab");
            if (string.IsNullOrEmpty(instance.Id) || !Regex.IsMatch(instance.Id, @"\A[A-Za-z][A-Za-z0-9_-]{0,39}\z"))
                throw new ArgumentException("Assembly ID must be 1-40 safe characters, starting with a letter.", "Id");
            var slab = ProjectingBalconySlabDefinition.Validate(instance.Slab, host);
            if (instance.Id == instance.Slab.Id) throw new ArgumentException("Assembly and slab child IDs must differ.", "Id");
            Positive(instance.LeftHeightMm, "LeftHeightMm"); Positive(instance.LeftThicknessMm, "LeftThicknessMm");
            Positive(instance.RightHeightMm, "RightHeightMm"); Positive(instance.RightThicknessMm, "RightThicknessMm");
            Positive(instance.MinimumPrintableThicknessMm, "MinimumPrintableThicknessMm");
            double minimum = instance.MinimumPrintableThicknessMm;
            if (Math.Min(instance.Slab.ThicknessMm, Math.Min(instance.Slab.WidthMm, instance.Slab.DepthMm)) < minimum ||
                instance.LeftThicknessMm < minimum || instance.RightThicknessMm < minimum)
                throw new ArgumentException("Slab and wall sections must meet the supplied printer-profile minimum.", "MinimumPrintableThicknessMm");
            double clearWidth = instance.Slab.WidthMm - instance.LeftThicknessMm - instance.RightThicknessMm;
            if (!Finite(clearWidth) || clearWidth <= 0 || clearWidth / 1000 <= 0)
                throw new ArgumentException("Side thicknesses must leave positive clear balcony width.");
            if (instance.Slab.ElevationMm + Math.Max(instance.LeftHeightMm, instance.RightHeightMm) > host.HeightMm)
                throw new ArgumentException("Both side walls must fit within the current host height.");
            var left = new BalconySideWallPlan("Left", instance.Id, slab.LeftMm, slab.LeftMm + instance.LeftThicknessMm,
                slab.TopMm, slab.TopMm + instance.LeftHeightMm, slab.RearMm, slab.FrontMm);
            var right = new BalconySideWallPlan("Right", instance.Id, slab.RightMm - instance.RightThicknessMm, slab.RightMm,
                slab.TopMm, slab.TopMm + instance.RightHeightMm, slab.RearMm, slab.FrontMm);
            CheckWall(left); CheckWall(right);
            var plan = new BalconyAssemblyPlan(instance, slab, left, right);
            if (!Finite(plan.VolumeMm3) || plan.VolumeMm3 <= 0) throw new ArgumentException("Nonfinite/zero assembly volume.");
            return plan;
        }

        // Links the actual CAD side depth to the validated slab's editable depth.
        public static string DepthEquation(BalconyAssemblyPlan plan, BalconySideWallPlan wall) =>
            "\"SideWallDepth@" + wall.FeatureName + "\" = \"SlabDepth@" + plan.Slab.FeatureName + "\"";

        private static bool Finite(double value) => !double.IsNaN(value) && !double.IsInfinity(value);
        private static void Positive(double value, string name)
        {
            if (!Finite(value) || value <= 0)
                throw new ArgumentOutOfRangeException(name, "Length must be finite, positive and representable in meters.");
            double meters = UnitConverter.MillimetersToMeters(value);
            if (meters <= 0)
                throw new ArgumentOutOfRangeException(name, "Length is too small to represent in meters.");
        }
        private static void CheckWall(BalconySideWallPlan wall)
        {
            if (!Finite(wall.TopMm) || wall.LeftMm / 1000 >= wall.RightMm / 1000 ||
                wall.BottomMm / 1000 >= wall.TopMm / 1000 || wall.RearMm / 1000 >= wall.FrontMm / 1000 ||
                !Finite(wall.SlabContactAreaMm2) || wall.SlabContactAreaMm2 <= 0 || !Finite(wall.VolumeMm3) || wall.VolumeMm3 <= 0)
                throw new ArgumentException("Wall bounds/contact/volume must be finite and geometrically distinct.");
        }
    }
}
