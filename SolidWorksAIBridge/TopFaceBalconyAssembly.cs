using System;
using System.Globalization;

namespace SolidWorksAIBridge
{
    /// <summary>Equal-height, both-side recipe only; analytic planning is not CAD evidence.</summary>
    public static class TopFaceBalconyAssembly
    {
        public const string Id = "lego.balcony.projecting-assembly.equal-height-top-face";
        public const string Version = "1.0.0";
        public const string Recipe = "top-face-shared-r1";
        // Validation covers only the recorded host/fixtures and sampled edits.
        // It does not certify every input accepted by Validate or physical printing.
        public const string Status = "Validated";
        public const string ValidationRecord = "tests/results/TOP_FACE_VALIDATED_2026-11-04.md";
        public static string SketchName(BalconyAssemblyPlan plan) => "BalconyTopWallSketch_" + plan.Instance.Id;
        public static string FeatureName(BalconyAssemblyPlan plan) => "BalconyTopWalls_" + plan.Instance.Id;

        public static BalconyAssemblyPlan Validate(BalconyAssemblyInstance instance, BalconySlabHost host)
        {
            var plan = BalconyAssemblyDefinition.Validate(instance, host);
            if (instance.LeftHeightMm != instance.RightHeightMm)
                throw new ArgumentException("The shared upward boss requires exactly equal wall heights.");
            return plan;
        }

        public static string[] Equations(BalconyAssemblyPlan plan)
        {
            string sketch = SketchName(plan);
            string slab = plan.Slab.SketchName;
            string x = (plan.Instance.Slab.CenterXMm < 0 ? "-" : "") + Q("CenterX_" + plan.Instance.Slab.Id + "@" + slab);
            string depth = Q("SlabDepth@" + plan.Slab.FeatureName);
            // Host must remain unedited, as in the validated slab adapter. Explicit units
            // prevent document display units from changing the facade-plane offset.
            string rear = plan.Slab.RearMm.ToString("R", CultureInfo.InvariantCulture) + "mm";
            var result = new string[6];
            int i = 0;
            foreach (var wall in new[] { plan.Left, plan.Right })
            {
                string half = "(" + Q("SlabWidth@" + slab) + " - " + Q("Thickness_" + wall.Role + "@" + sketch) + ") / 2";
                result[i++] = Q("Depth_" + wall.Role + "@" + sketch) + " = " + depth;
                result[i++] = Q("CenterX_" + wall.Role + "@" + sketch) + " = abs(" + x + (wall.Role == "Left" ? " - " : " + ") + half + ")";
                result[i++] = Q("CenterZ_" + wall.Role + "@" + sketch) + " = " + rear + " + " + depth + " / 2";
            }
            return result;
        }

        private static string Q(string value) => "\"" + value + "\"";

        // Basis vectors are architectural X/Z expressed in sketch coordinates.
        // Reject arbitrary rotations/scale instead of silently creating a skew profile.
        public static bool XIsHorizontal(double xu, double xv, double zu, double zv)
        {
            const double tolerance = 1e-8;
            Func<double, bool> zero = v => !double.IsNaN(v) && Math.Abs(v) < tolerance;
            Func<double, bool> unit = v => !double.IsNaN(v) && Math.Abs(Math.Abs(v) - 1) < tolerance;
            if (unit(xu) && zero(xv) && zero(zu) && unit(zv)) return true;
            if (zero(xu) && unit(xv) && unit(zu) && zero(zv)) return false;
            throw new InvalidOperationException("Top-face sketch must have orthogonal unit axes aligned with model X/Z.");
        }
    }
}
