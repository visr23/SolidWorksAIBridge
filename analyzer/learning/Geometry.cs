using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace SolidWorksAIBridge.Analysis.Learning
{
    public sealed class ReadBudget
    {
        private readonly Stopwatch clock = Stopwatch.StartNew();
        private readonly int maximum;
        private readonly double seconds;
        private readonly Func<bool> cancelled;
        private readonly Func<double> elapsed;
        public int Used { get; private set; }
        public ReadBudget(int maximum, double seconds, Func<bool> cancelled, Func<double> elapsedSeconds = null)
        {
            LearningRecord.Require(maximum > 0 && LearningRecord.Finite(seconds) && seconds > 0 && cancelled != null, "Invalid extraction budget.");
            this.maximum = maximum; this.seconds = seconds; this.cancelled = cancelled;
            elapsed = elapsedSeconds ?? (() => clock.Elapsed.TotalSeconds);
        }
        public void Checkpoint()
        {
            if (cancelled()) throw new OperationCanceledException("Read extraction cancelled.");
            double time = elapsed();
            LearningRecord.Require(LearningRecord.Finite(time) && time >= 0 && time < seconds && Used < maximum, "Read extraction budget exhausted.");
            Used++;
        }
    }

    public sealed class Rect
    {
        public double Left { get; private set; }
        public double Bottom { get; private set; }
        public double Width { get; private set; }
        public double Height { get; private set; }
        public double Aspect { get { return Width / Height; } }
        public Rect(double left, double bottom, double width, double height)
        {
            LearningRecord.Require(new[] { left, bottom, width, height }.All(LearningRecord.Finite) && width > 0 && height > 0 && LearningRecord.Finite(left + width) && LearningRecord.Finite(bottom + height), "Invalid rectangle.");
            Left = left; Bottom = bottom; Width = width; Height = height;
        }
        public Rect Normalize(double anchorX, double groundY, double architecturalHeight)
        {
            LearningRecord.Require(LearningRecord.Finite(architecturalHeight) && architecturalHeight > 0 && LearningRecord.Finite(anchorX) && LearningRecord.Finite(groundY), "Invalid registration.");
            return new Rect((Left - anchorX) / architecturalHeight, (Bottom - groundY) / architecturalHeight, Width / architecturalHeight, Height / architecturalHeight);
        }
        public double IntersectionOverUnion(Rect other)
        {
            if (other == null) throw new ArgumentNullException(nameof(other));
            double width = Math.Max(0, Math.Min(Left + Width, other.Left + other.Width) - Math.Max(Left, other.Left));
            double height = Math.Max(0, Math.Min(Bottom + Height, other.Bottom + other.Height) - Math.Max(Bottom, other.Bottom));
            double intersection = width * height;
            double union = Width * Height + other.Width * other.Height - intersection;
            LearningRecord.Require(LearningRecord.Finite(union) && union > 0 && LearningRecord.Finite(intersection), "Rectangle area cannot be represented.");
            return intersection / union;
        }
    }

    public static class EdgeEvidence
    {
        // No sorting of a point cloud or convex hull is accepted as a CAD boundary.
        public static bool IsClosedLineLoop(IEnumerable<double[][]> edgeInput, double tolerance)
        {
            LearningRecord.Require(LearningRecord.Finite(tolerance) && tolerance > 0, "Invalid loop tolerance.");
            if (edgeInput == null) return false;
            var edges = edgeInput.ToList();
            if (edges.Count < 3 || edges.Any(e => e == null || e.Length != 2 || e.Any(p => p == null || p.Length != 3 || !p.All(LearningRecord.Finite)))) return false;
            Func<double[], double[], bool> same = (a, b) => a.Zip(b, (x, y) => Math.Abs(x - y)).Max() <= tolerance;
            if (edges.Any(e => same(e[0], e[1]))) return false;
            var remaining = edges.Skip(1).ToList();
            var start = edges[0][0]; var end = edges[0][1];
            while (remaining.Count > 0)
            {
                var matches = remaining.Where(e => same(e[0], end) || same(e[1], end)).ToList();
                if (matches.Count != 1) return false;
                var edge = matches[0]; remaining.Remove(edge);
                end = same(edge[0], end) ? edge[1] : edge[0];
                if (same(start, end) && remaining.Count > 0) return false;
            }
            return same(start, end);
        }
    }
}
