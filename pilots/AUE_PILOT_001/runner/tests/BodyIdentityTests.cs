using System;
using System.Collections.Generic;
using SolidWorksAIBridge.Pilot001;

internal static class BodyIdentityTests
{
    private sealed class Solid
    {
        public readonly int Id;
        public string DisplayName;
        public Solid(int id, string name) { Id = id; DisplayName = name; }
    }
    private static int passed, failed;
    private static void Test(string name, Action action)
    {
        try { action(); passed++; Console.WriteLine("PASS " + name); }
        catch (Exception ex) { failed++; Console.WriteLine("FAIL " + name + ": " + ex.Message); }
    }
    private static void Reject(Action action)
    {
        try { action(); } catch (InvalidOperationException) { return; }
        throw new Exception("Invalid identity/geometry was accepted.");
    }
    private static void Equal(bool value) { if (!value) throw new Exception("Assertion failed."); }
    private static BodyGeometry Wall(double volume = 260324.88, double right = 224.5, int faces = 10)
    {
        return new BodyGeometry(volume, new[] { 0.0, 0.0, -12.0, right, 99.8, 0.0 }, 16, 24, faces);
    }
    private static int Main()
    {
        var live = new List<Solid>();
        var tracker = new BodyIdentity<Solid>(() => live, (a, b) => a.Id == b.Id);
        var wall = new Solid(1, "MainWall");
        live.Add(wall);
        Solid[] owners = { wall };
        tracker.Track("Facade", () => owners);
        Test("Cut-Extrude rename MainWall -> WindowCut preserves identity", () =>
        {
            wall.DisplayName = "WindowCut";
            Equal(ReferenceEquals(tracker.Resolve("Facade"), wall));
            tracker.VerifyInventory(1);
        });
        Test("User body rename does not affect identity", () =>
        {
            wall.DisplayName = "arbitrary display label";
            Equal(ReferenceEquals(tracker.Resolve("Facade"), wall));
        });
        Test("Repeated feature-face owners resolve one solid", () =>
        {
            owners = new[] { wall, new Solid(1, "different COM wrapper") };
            Equal(ReferenceEquals(tracker.Resolve("Facade"), wall));
        });
        var replacement = new Solid(2, "WindowCut");
        Test("Replaced body after Cut-Extrude resolved through updated feature", () =>
        {
            live.Clear(); live.Add(replacement);
            owners = new[] { replacement, replacement };
            tracker.Track("Facade", () => owners);
            Equal(ReferenceEquals(tracker.Resolve("Facade"), replacement));
            tracker.VerifyInventory(1);
        });
        Test("Stale pre-cut face owner rejected, no name fallback", () => Reject(() => tracker.Track("Facade", () => new[] { wall })));
        Test("Rejected anchor update preserves valid role", () => Equal(ReferenceEquals(tracker.Resolve("Facade"), replacement)));
        Test("Unrelated body with identical display name is not selected", () =>
        {
            var roof = new Solid(3, replacement.DisplayName);
            live.Add(roof);
            tracker.Track("Roof", () => new[] { roof });
            Equal(ReferenceEquals(tracker.Resolve("Facade"), replacement));
            Equal(ReferenceEquals(tracker.Resolve("Roof"), roof));
            tracker.VerifyInventory(2);
        });
        Test("Split-cut feature owning two solids rejected", () => Reject(() => tracker.Track("Split", () => live.ToArray())));
        Test("Unintended merge of logical roles rejected", () =>
        {
            var merged = new BodyIdentity<Solid>(() => new[] { replacement, new Solid(55, "untracked") }, (a, b) => a.Id == b.Id);
            merged.Track("Facade", () => new[] { replacement }); merged.Track("Bay", () => new[] { replacement });
            Reject(() => merged.VerifyInventory(2));
        });
        Test("Unexpected extra solid rejected", () => Reject(() => tracker.VerifyInventory(1)));
        Test("Missing feature faces rejected", () => Reject(() => tracker.Track("Missing", () => null)));
        Test("Empty face ownership rejected", () => Reject(() => tracker.Track("Missing", () => new Solid[0])));
        Test("Null face owner rejected", () => Reject(() => tracker.Track("Missing", () => new Solid[] { null })));
        Test("Foreign document owner rejected", () => Reject(() => tracker.Track("Foreign", () => new[] { new Solid(99, "WindowCut") })));
        Test("Untracked role rejected", () => Reject(() => tracker.Resolve("NotRegistered")));
        Test("Duplicate live identity rejected", () =>
        {
            live.Add(new Solid(2, "duplicate wrapper"));
            Reject(() => tracker.Resolve("Facade")); live.RemoveAt(live.Count - 1);
        });
        Test("Missing live body rejected", () =>
        {
            live.Remove(replacement); Reject(() => tracker.Resolve("Facade")); live.Add(replacement);
        });
        Test("Two geometrically identical solids still require identity", () =>
        {
            var twin = new Solid(44, "WindowCut"); live.Add(twin);
            Equal(ReferenceEquals(tracker.Resolve("Facade"), replacement)); live.Remove(twin);
        });
        Test("Wall-minus-window analytic volume and bounds accepted", () => Wall().Verify((224.5 * 99.8 - 23.4 * 30.4) * 12, new[] { 0.0, 0.0, -12.0, 224.5, 99.8, 0.0 }));
        Test("Incorrect cut volume rejected", () => Reject(() => Wall(268861.2).Verify(260324.88)));
        Test("Incorrect wall bounds rejected", () => Reject(() => Wall(right: 225).Verify(260324.88, Wall().BoundsMm)));
        Test("Non-target volume mutation rejected", () => Reject(() => Wall().VerifyUnchanged(Wall(260325.0))));
        Test("Non-target topology mutation rejected", () => Reject(() => Wall().VerifyUnchanged(Wall(faces: 11))));
        Test("Unchanged non-target geometry accepted", () => Wall().VerifyUnchanged(Wall()));
        Test("Nonfinite measured volume rejected", () => Reject(() => Wall(double.NaN).Verify(260324.88)));
        Test("Nonfinite bounds rejected", () => Reject(() => new BodyGeometry(1, new[] { double.NaN, 0.0, 0.0, 1.0, 1.0, 1.0 }, 8, 12, 6).Verify(1)));
        Test("Empty topology rejected", () => Reject(() => Wall(faces: 0).Verify(260324.88)));
        foreach (var pair in new[] { Tuple.Create("Width", 23.4), Tuple.Create("Height", 30.4), Tuple.Create("CenterX", 54.85), Tuple.Create("CenterY", 3.1), Tuple.Create("CutDepth", 12.2) })
        {
            Test("Cut " + pair.Item1 + " dimension accepted", () => BodyGeometry.VerifyDimension(pair.Item2, pair.Item2, true));
            Test("Incorrect cut " + pair.Item1 + " rejected", () => Reject(() => BodyGeometry.VerifyDimension(pair.Item2 + 1, pair.Item2, true)));
        }
        Test("Driven/reference dimension rejected", () => Reject(() => BodyGeometry.VerifyDimension(23.4, 23.4, false)));
        Test("Nonfinite cut dimension rejected", () => Reject(() => BodyGeometry.VerifyDimension(double.NaN, 23.4, true)));
        var aperture = new[] { 155.4, 31.6, -12.0, 178.8, 62.0, 0.0 };
        Test("Measured rectangular through-cut bounds accepted", () => BodyGeometry.VerifyAperture(4, aperture, aperture));
        Test("Blind pocket does not pass through-cut geometry", () =>
            Reject(() => BodyGeometry.VerifyAperture(4, new[] { 155.4, 31.6, -6.0, 178.8, 62.0, 0.0 }, aperture)));
        Test("Wrong aperture width rejected even with correct outer body", () =>
            Reject(() => BodyGeometry.VerifyAperture(4, new[] { 155.4, 31.6, -12.0, 179.8, 62.0, 0.0 }, aperture)));
        Test("Wrong signed aperture position rejected", () =>
            Reject(() => BodyGeometry.VerifyAperture(4, new[] { 155.4, 37.8, -12.0, 178.8, 68.2, 0.0 }, aperture)));
        Test("Extra cut faces rejected", () => Reject(() => BodyGeometry.VerifyAperture(5, aperture, aperture)));
        Test("Missing cut bounds rejected", () => Reject(() => BodyGeometry.VerifyAperture(4, null, aperture)));
        Test("Nonfinite cut bounds rejected", () =>
            Reject(() => BodyGeometry.VerifyAperture(4, new[] { double.NaN, 31.6, -12.0, 178.8, 62.0, 0.0 }, aperture)));
        Test("Malformed cut bounds rejected", () => Reject(() => BodyGeometry.VerifyAperture(4, new[] { 1.0 }, aperture)));        Test("Successful rebuild accepted", () => BodyGeometry.VerifyRebuild(true));
        Test("Failed rebuild rejected", () => Reject(() => BodyGeometry.VerifyRebuild(false)));
        Console.WriteLine("RESULT: {0} passed, {1} failed. Synthetic non-COM policy tests; no SolidWorks connection.", passed, failed);
        return failed == 0 ? 0 : 1;
    }
}
