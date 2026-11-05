using System;
using System.Collections.Generic;
using System.Linq;

namespace SolidWorksAIBridge.Pilot001
{
    // No COM dependency: the adapter supplies current feature-owned bodies and
    // reference identity. Display names are deliberately absent from this contract.
    internal sealed class BodyIdentity<T> where T : class
    {
        private readonly Func<IEnumerable<T>> liveBodies;
        private readonly Func<T, T, bool> same;
        private readonly Dictionary<string, Func<IEnumerable<T>>> anchors = new Dictionary<string, Func<IEnumerable<T>>>();

        public BodyIdentity(Func<IEnumerable<T>> liveBodies, Func<T, T, bool> same)
        {
            this.liveBodies = liveBodies ?? throw new ArgumentNullException(nameof(liveBodies));
            this.same = same ?? throw new ArgumentNullException(nameof(same));
        }

        public string[] Roles { get { return anchors.Keys.ToArray(); } }

        public void Track(string role, Func<IEnumerable<T>> currentOwners)
        {
            if (string.IsNullOrWhiteSpace(role) || currentOwners == null)
                throw new ArgumentException("A body role and current feature ownership are required.");
            // Resolve before replacing a valid anchor. Never accept an arbitrary first face.
            ResolveOwners(role, currentOwners);
            anchors[role] = currentOwners;
        }

        public T Resolve(string role)
        {
            Func<IEnumerable<T>> anchor;
            if (!anchors.TryGetValue(role, out anchor))
                throw new InvalidOperationException("Untracked body role: " + role);
            return ResolveOwners(role, anchor);
        }

        private T ResolveOwners(string role, Func<IEnumerable<T>> anchor)
        {
            var live = (liveBodies() ?? throw new InvalidOperationException("Missing current body inventory.")).ToArray();
            var owners = (anchor() ?? throw new InvalidOperationException("Missing feature faces for " + role)).ToArray();
            if (owners.Length == 0 || owners.Any(owner => owner == null))
                throw new InvalidOperationException("No complete feature/body ownership for " + role);
            var unique = new List<T>();
            foreach (T owner in owners)
            {
                var current = live.Where(body => body != null && same(owner, body)).ToArray();
                if (current.Length != 1)
                    throw new InvalidOperationException("Feature owner is stale, foreign or ambiguous for " + role);
                if (!unique.Any(body => same(body, current[0]))) unique.Add(current[0]);
            }
            if (unique.Count != 1)
                throw new InvalidOperationException("Feature spans multiple solid bodies for " + role);
            return unique[0];
        }

        public void VerifyInventory(int expectedCount)
        {
            var live = (liveBodies() ?? throw new InvalidOperationException("Missing body inventory.")).ToArray();
            if (live.Length != expectedCount || anchors.Count != expectedCount)
                throw new InvalidOperationException("Current/registered body count differs from the plan.");
            var resolved = new List<T>();
            foreach (string role in Roles)
            {
                T body = Resolve(role);
                if (resolved.Any(previous => same(previous, body)))
                    throw new InvalidOperationException("Distinct roles unexpectedly merged into one body.");
                resolved.Add(body);
            }
        }
    }

    internal sealed class BodyGeometry
    {
        public readonly double VolumeMm3;
        public readonly double[] BoundsMm;
        public readonly int Vertices, Edges, Faces;

        public BodyGeometry(double volume, double[] bounds, int vertices, int edges, int faces)
        {
            VolumeMm3 = volume;
            BoundsMm = bounds == null ? null : (double[])bounds.Clone();
            Vertices = vertices;
            Edges = edges;
            Faces = faces;
        }

        public void Verify(double expectedVolume, double[] expectedBounds = null)
        {
            if (!Finite(VolumeMm3) || VolumeMm3 <= 0 || !Finite(expectedVolume) || expectedVolume <= 0 ||
                Math.Abs(VolumeMm3 - expectedVolume) > 0.05)
                throw new InvalidOperationException("Solid volume differs from expected geometry.");
            if (BoundsMm == null || BoundsMm.Length != 6 || BoundsMm.Any(value => !Finite(value)) ||
                Enumerable.Range(0, 3).Any(i => BoundsMm[i] >= BoundsMm[i + 3]) ||
                Vertices <= 0 || Edges <= 0 || Faces <= 0)
                throw new InvalidOperationException("Invalid bounds or empty solid topology.");
            if (expectedBounds != null)
            {
                if (expectedBounds.Length != 6 || expectedBounds.Any(value => !Finite(value)))
                    throw new InvalidOperationException("Invalid expected bounds.");
                for (int i = 0; i < 6; i++)
                    if (Math.Abs(BoundsMm[i] - expectedBounds[i]) > 0.05)
                        throw new InvalidOperationException("Solid bounds differ from expected geometry.");
            }
        }

        public void VerifyUnchanged(BodyGeometry current)
        {
            if (current == null) throw new InvalidOperationException("Non-target solid is missing.");
            current.Verify(VolumeMm3, BoundsMm);
            if (Vertices != current.Vertices || Edges != current.Edges || Faces != current.Faces)
                throw new InvalidOperationException("Non-target topology changed.");
        }

        public static void VerifyAperture(int sideFaces, double[] bounds, double[] expected)
        {
            if (sideFaces != 4 || bounds == null || expected == null || bounds.Length != 6 || expected.Length != 6 ||
                bounds.Any(value => !Finite(value)) || expected.Any(value => !Finite(value)))
                throw new InvalidOperationException("Rectangular through-cut faces/bounds are invalid.");
            for (int i = 0; i < 6; i++)
                if (Math.Abs(bounds[i] - expected[i]) > 0.05)
                    throw new InvalidOperationException("Actual cut face bounds differ from the plan.");
        }
        public static void VerifyDimension(double actualMm, double expectedMm, bool driving)
        {
            if (!driving || !Finite(actualMm) || !Finite(expectedMm) || Math.Abs(actualMm - expectedMm) > 0.001)
                throw new InvalidOperationException("Cut dimension/placement is invalid or not driving.");
        }

        public static void VerifyRebuild(bool rebuilt)
        {
            if (!rebuilt) throw new InvalidOperationException("SOLIDWORKS rebuild failed.");
        }

        private static bool Finite(double value) { return !double.IsNaN(value) && !double.IsInfinity(value); }
    }
}
