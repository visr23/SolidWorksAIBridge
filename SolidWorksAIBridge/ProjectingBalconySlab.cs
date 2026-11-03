using System;
using System.Text.RegularExpressions;

namespace SolidWorksAIBridge
{
    /// <summary>A building instance resolved to actual miniature mm; not real-world Building JSON.</summary>
    public sealed class ProjectingBalconySlabInstance
    {
        public string Id { get; }
        public string HostId { get; }
        public double WidthMm { get; }
        public double DepthMm { get; }
        public double ThicknessMm { get; }
        public double CenterXMm { get; }
        public double ElevationMm { get; }

        public ProjectingBalconySlabInstance(string id, string hostId, double widthMm,
            double depthMm, double thicknessMm, double centerXMm, double elevationMm)
        {
            Id = id;
            HostId = hostId;
            WidthMm = widthMm;
            DepthMm = depthMm;
            ThicknessMm = thicknessMm;
            CenterXMm = centerXMm;
            ElevationMm = elevationMm;
        }
    }

    /// <summary>Snapshot of an unedited, front-facing wall created by this modeler.</summary>
    public sealed class BalconySlabHost
    {
        public string Id { get; }
        public double WidthMm { get; }
        public double HeightMm { get; }
        public double ThicknessMm { get; }

        public BalconySlabHost(string id, double widthMm, double heightMm, double thicknessMm)
        {
            Id = id;
            WidthMm = widthMm;
            HeightMm = heightMm;
            ThicknessMm = thicknessMm;
        }
    }

    /// <summary>Successful non-COM preflight evidence and immutable resolved geometry.
    /// Does not certify CAD, regression, assembly, or printer suitability.</summary>
    public sealed class BalconySlabValidationRecord
    {
        public ProjectingBalconySlabInstance Instance { get; }
        public BalconySlabHost Host { get; }
        public DateTime CheckedAtUtc { get; } = DateTime.UtcNow;
        public string DefinitionId => ProjectingBalconySlabDefinition.Id;
        public string DefinitionVersion => ProjectingBalconySlabDefinition.Version;
        public string CadStatus => "NotRun";
        public bool ParametersAndPlacementPassed => true;
        public double LeftMm { get; }
        public double RightMm { get; }
        public double BottomMm { get; }
        public double TopMm { get; }
        public double RearMm { get; }
        public double FrontMm { get; }
        public double CenterYMm => BottomMm + Instance.ThicknessMm / 2;
        public string SketchName => "BalconySlabSketch_" + Instance.Id;
        public string FeatureName => "BalconySlab_" + Instance.Id;

        internal BalconySlabValidationRecord(ProjectingBalconySlabInstance instance, BalconySlabHost host,
            double left, double right, double bottom, double top, double rear, double front)
        {
            Instance = instance;
            Host = host;
            LeftMm = left; RightMm = right; BottomMm = bottom;
            TopMm = top; RearMm = rear; FrontMm = front;
        }
    }

    public static class ProjectingBalconySlabDefinition
    {
        public const string Id = "lego.balcony.projecting-slab";
        public const string Version = "1.0.0";
        public const string Status = "Validated"; // Scoped user approval; not every accepted instance/range.
        public const string ValidationScope = "tests/results/PROJECTING_SLAB_V1_VALIDATED_2026-11-03.md";
        public const string ManufacturingClass = "separate_print";

        public static BalconySlabValidationRecord Validate(ProjectingBalconySlabInstance instance,
            BalconySlabHost host)
        {
            if (instance == null) throw new ArgumentNullException(nameof(instance));
            if (host == null) throw new ArgumentNullException(nameof(host));
            if (string.IsNullOrEmpty(instance.Id) || !Regex.IsMatch(instance.Id, @"\A[A-Za-z][A-Za-z0-9_-]{0,63}\z"))
                throw new ArgumentException("Use a stable ID: letter followed by up to 63 letters, digits, underscores or hyphens.", "Id");
            if (string.IsNullOrWhiteSpace(host.Id) || !string.Equals(instance.HostId, host.Id, StringComparison.Ordinal))
                throw new ArgumentException("The slab must reference the supplied host wall.", "HostId");
            Positive(host.WidthMm, "host.WidthMm");
            Positive(host.HeightMm, "host.HeightMm");
            Positive(host.ThicknessMm, "host.ThicknessMm");
            Positive(instance.WidthMm, "WidthMm");
            Positive(instance.DepthMm, "DepthMm");
            Positive(instance.ThicknessMm, "ThicknessMm");
            Finite(instance.CenterXMm, "CenterXMm");
            Finite(instance.ElevationMm, "ElevationMm");

            // Host-local Y=0 is the wall bottom; convert to the legacy centered model frame.
            double left = instance.CenterXMm - instance.WidthMm / 2;
            double right = instance.CenterXMm + instance.WidthMm / 2;
            double top = instance.ElevationMm - host.HeightMm / 2;
            double bottom = top - instance.ThicknessMm;
            double rear = host.ThicknessMm / 2;
            double front = rear + instance.DepthMm;
            if (instance.WidthMm > host.WidthMm ||
                Math.Abs(instance.CenterXMm) > (host.WidthMm - instance.WidthMm) / 2)
                throw new ArgumentException("The complete slab must fit within the host width.", "CenterXMm");
            if (instance.ElevationMm > host.HeightMm || instance.ElevationMm < instance.ThicknessMm)
                throw new ArgumentException("Slab top and bottom must fit within the host height.", "ElevationMm");
            if (!FiniteValue(left) || !FiniteValue(right) || !FiniteValue(bottom) || !FiniteValue(top) ||
                !FiniteValue(front) || left >= right || bottom >= top || rear >= front ||
                UnitConverter.MillimetersToMeters(left) >= UnitConverter.MillimetersToMeters(right) ||
                UnitConverter.MillimetersToMeters(bottom) >= UnitConverter.MillimetersToMeters(top) ||
                UnitConverter.MillimetersToMeters(rear) >= UnitConverter.MillimetersToMeters(front))
                throw new ArgumentException("Slab coordinates must remain finite and geometrically distinct in meters.");
            return new BalconySlabValidationRecord(instance, host, left, right, bottom, top, rear, front);
        }

        private static bool FiniteValue(double value) => !double.IsNaN(value) && !double.IsInfinity(value);
        private static void Finite(double value, string name)
        {
            if (!FiniteValue(value)) throw new ArgumentOutOfRangeException(name, "Value must be finite.");
        }
        private static void Positive(double value, string name)
        {
            Finite(value, name);
            if (value <= 0 || UnitConverter.MillimetersToMeters(value) <= 0)
                throw new ArgumentOutOfRangeException(name, "Length must be positive and representable in meters.");
        }
    }
}
