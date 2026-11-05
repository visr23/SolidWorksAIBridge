using System;
using System.IO;
using System.Linq;
using SolidWorksAIBridge.Analysis.Learning;

internal static class LearningTests
{
    private static int passed, failed;
    private static string fixture;
    private static LearningRecord Fresh() { return LearningRecord.FromJson(fixture); }
    private static void Test(string name, Action action)
    {
        try { action(); passed++; Console.WriteLine("PASS " + name); }
        catch (Exception e) { failed++; Console.WriteLine("FAIL " + name + ": " + e.Message); }
    }
    private static void Check(bool value) { if (!value) throw new Exception("Assertion failed."); }
    private static void Reject(Action action)
    {
        try { action(); } catch (InvalidOperationException) { return; }
        throw new Exception("Invalid evidence accepted.");
    }
    private static void Bad(Action<LearningRecord> edit) { var r = Fresh(); edit(r); Reject(r.Validate); }
    private static double[][] Edge(double x, double y, double a, double b) { return new[] { new[] {x,y,0.0},new[] {a,b,0.0} }; }
    private static int Main(string[] args)
    {
        fixture = File.ReadAllText(args[0]);
        Test("synthetic one-to-many", () => Check(Fresh().Correspondences[0].GeneratedEntities.Length == 2));
        Test("synthetic many-to-one", () => Check(Fresh().Correspondences[1].StudioEntities.Length == 2));
        Test("unknown retained as null", () => Check(!Fresh().Evidence.Single(e => e.Id == "unknown").Value.HasValue));
        Test("roundtrip", () => Check(LearningRecord.FromJson(Fresh().ToJson()).ToJson() == Fresh().ToJson()));
        Test("canonical ordering", () => { var a = Fresh(); var b = Fresh(); b.Evidence.Reverse(); b.Correspondences.Reverse(); b.Correspondences[0].StudioEntities = b.Correspondences[0].StudioEntities.Reverse().ToArray(); Check(a.ToJson() == b.ToJson()); });
        Test("serialization does not mutate caller", () => { var r = Fresh(); r.Evidence.Reverse(); var first = r.Evidence[0].Id; r.ToJson(); Check(first == r.Evidence[0].Id); });
        Test("scope", () => Bad(r => r.Scope = "FullBuilding"));
        Test("version", () => Bad(r => r.Version = "2"));
        Test("no absolute CAD authority", () => Bad(r => r.ReferenceAuthority = "OriginalIsTruth"));
        foreach (double value in new[] { 0.0, -1, Double.NaN, Double.PositiveInfinity })
        { var v = value; Test("height " + value, () => Bad(r => r.TargetHeightMm = v)); }
        Test("duplicate evidence", () => Bad(r => r.Evidence.Add(r.Evidence[0])));
        Test("missing evidence provenance", () => Bad(r => r.Evidence[0].Asset = null));
        Test("nonfinite measurement", () => Bad(r => r.Evidence[0].Value = Double.NaN));
        Test("measurement units", () => Bad(r => r.Evidence[0].Units = null));
        Test("unknown is not zero", () => Bad(r => r.Evidence.Single(e => e.Id == "unknown").Value = 0));
        Test("unknown requires reason", () => Bad(r => r.Evidence.Single(e => e.Id == "unknown").Limitation = null));
        Test("confidence finite", () => Bad(r => r.Evidence[0].Confidence = Double.NaN));
        Test("confidence range", () => Bad(r => r.Correspondences[0].Confidence = 1.1));
        Test("dangling measurement", () => Bad(r => r.Correspondences[0].EvidenceIds = new[] { "missing" }));
        Test("duplicate body in match", () => Bad(r => r.Correspondences[0].StudioEntities = new[] { "same", "same" }));
        Test("unmatched generated component allowed", () => { var r = Fresh(); r.Correspondences[0].StudioEntities = new string[0]; r.Validate(); });
        Test("empty correspondence", () => Bad(r => { r.Correspondences[0].StudioEntities = new string[0]; r.Correspondences[0].GeneratedEntities = new string[0]; }));
        Test("verified match prohibited", () => Bad(r => r.Correspondences[0].Verified = true));
        Test("verified classification prohibited", () => Bad(r => r.Findings[0].Verified = true));
        Test("unknown classification", () => Bad(r => r.Findings[0].Classification = "OriginalAlwaysCorrect"));
        Test("dangling finding", () => Bad(r => r.Findings[0].CorrespondenceId = "missing"));
        Test("recipe cannot promote", () => Bad(r => r.Recipes[0].Status = "Validated"));
        Test("proposal requires alternatives", () => Bad(r => r.Proposals[0].RecipeIds = new[] { "R1" }));
        Test("proposal requires known alternatives", () => Bad(r => r.Proposals[0].RecipeIds = new[] { "R1", "missing" }));
        Test("recipe scope mismatch rejected", () => Bad(r => r.Recipes[1].Component = "unrelated-roof"));
        Test("fixture mandatory", () => Bad(r => r.Experiments[0].FixtureId = null));
        Test("stage cannot skip", () => { var r = Fresh(); Reject(() => Workflow.Advance(r.Experiments[0], "TestInIsolation", r)); });
        Test("diagnose requires measurement", () => { var r = Fresh(); r.Experiments[0].EvidenceIds = new string[0]; Reject(() => Workflow.Advance(r.Experiments[0], "Diagnose", r)); });
        Test("reject allowed", () => { var r = Fresh(); Workflow.Advance(r.Experiments[0], "Reject", r); Check(r.Experiments[0].Stage == "Reject"); });
        Test("rejection terminal", () => { var r = Fresh(); r.Experiments[0].Stage = "Reject"; Reject(() => Workflow.Advance(r.Experiments[0], "Measure", r)); });
        Test("pending cannot remeasure", () => { var r = Fresh(); r.Experiments[0].Stage = "TestInIsolation"; Reject(() => Workflow.Advance(r.Experiments[0], "Remeasure", r)); });
        Test("unrelated experiment evidence rejected", () => { var r = Fresh(); var e = r.Experiments[0]; e.Stage = "TestInIsolation"; e.Outcome = "Passed"; e.EvidenceIds = new[] { "test" }; r.Evidence.Single(x => x.Id == "test").Entity = "otherFixture"; Reject(() => Workflow.Advance(e, "Remeasure", r)); });
        Test("acceptance requires approval", () => { var r = Ready(); r.Experiments[0].HumanApproved = false; Reject(() => Workflow.Advance(r.Experiments[0], "Accept", r)); });
        Test("acceptance requires regression", () => { var r = Ready(); r.Experiments[0].RegressionEvidenceIds = new string[0]; Reject(() => Workflow.Advance(r.Experiments[0], "Accept", r)); });
        Test("failed regression rejected", () => { var r = Ready(); r.Evidence.Single(x => x.Id == "regression").Status = "Failed"; Reject(() => Workflow.Advance(r.Experiments[0], "Accept", r)); });
        Test("acceptance requires remeasurement", () => { var r = Ready(); r.Experiments[0].EvidenceIds = new[] { "test" }; Reject(() => Workflow.Advance(r.Experiments[0], "Accept", r)); });
        Test("positive controlled acceptance keeps recipe Draft", () => { var r = Ready(); Workflow.Advance(r.Experiments[0], "Accept", r); r.Validate(); Check(r.Recipes.All(x => x.Status == "Draft")); });
        Test("full staged workflow", () => { var r = Fresh(); var e = r.Experiments[0]; foreach (var stage in new[] { "Diagnose", "CompareRecipes", "Propose", "TestInIsolation" }) Workflow.Advance(e, stage, r); e.Outcome = "Passed"; e.EvidenceIds = new[] { "test", "remeasure" }; Workflow.Advance(e, "Remeasure", r); e.RegressionEvidenceIds = new[] { "regression" }; e.HumanApproved = true; Workflow.Advance(e, "Accept", r); Check(e.Stage == "Accept"); });
        Test("uniform scale aspect invariant", () => { var a = new Rect(10,20,30,40); var b = a.Normalize(5,10,200); Check(Math.Abs(a.Aspect - b.Aspect) < 1e-12 && Math.Abs(b.Left - .025) < 1e-12); });
        Test("IoU identical", () => Check(new Rect(0,0,2,3).IntersectionOverUnion(new Rect(0,0,2,3)) == 1));
        Test("IoU disjoint", () => Check(new Rect(0,0,1,1).IntersectionOverUnion(new Rect(2,0,1,1)) == 0));
        Test("IoU partial and symmetric", () => { var a = new Rect(0,0,2,2); var b = new Rect(1,0,2,2); Check(Math.Abs(a.IntersectionOverUnion(b) - 1.0/3) < 1e-12 && a.IntersectionOverUnion(b) == b.IntersectionOverUnion(a)); });
        Test("IoU containment", () => Check(new Rect(0,0,4,4).IntersectionOverUnion(new Rect(1,1,2,2)) == .25));
        Test("invalid rectangle", () => Reject(() => new Rect(0,0,-1,1)));
        Test("invalid normalization", () => Reject(() => new Rect(0,0,1,1).Normalize(0,0,0)));
        Test("line loop unordered reversible", () => Check(EdgeEvidence.IsClosedLineLoop(new[] {Edge(1,1,0,1),Edge(0,0,1,0),Edge(1,1,1,0),Edge(0,1,0,0)},1e-8)));
        Test("open edge chain rejected", () => Check(!EdgeEvidence.IsClosedLineLoop(new[] {Edge(0,0,1,0),Edge(1,0,1,1),Edge(1,1,0,1)},1e-8)));
        Test("disconnected loops rejected", () => Check(!EdgeEvidence.IsClosedLineLoop(new[] {Edge(0,0,1,0),Edge(1,0,0,1),Edge(0,1,0,0),Edge(3,0,4,0),Edge(4,0,3,1),Edge(3,1,3,0)},1e-8)));
        Test("degenerate edge rejected", () => Check(!EdgeEvidence.IsClosedLineLoop(new[] {Edge(0,0,0,0),Edge(0,0,1,0),Edge(1,0,0,0)},1e-8)));
        Test("nonfinite loop rejected", () => Check(!EdgeEvidence.IsClosedLineLoop(new[] {Edge(Double.NaN,0,1,0),Edge(1,0,0,1),Edge(0,1,0,0)},1e-8)));
        Test("budget exact stop", () => { var b = new ReadBudget(1,100,()=>false); b.Checkpoint(); Reject(b.Checkpoint); });
        Test("deterministic time budget", () => { double elapsed = 0; var b = new ReadBudget(10,5,()=>false,()=>elapsed); b.Checkpoint(); elapsed = 5; Reject(b.Checkpoint); });
        Test("nonfinite clock rejected", () => { var b = new ReadBudget(10,5,()=>false,()=>Double.NaN); Reject(b.Checkpoint); });
        Test("cancellation", () => { var b = new ReadBudget(10,100,()=>true); try { b.Checkpoint(); } catch (OperationCanceledException) { return; } throw new Exception("Cancellation ignored."); });
        if (args.Length > 1)
        {
            Test("actual pilot record validates and roundtrips", () => { var r = LearningRecord.FromJson(File.ReadAllText(args[1])); Check(r.TargetHeightMm == 180 && LearningRecord.FromJson(r.ToJson()).ToJson() == r.ToJson()); });
            Test("actual pilot proposals remain Draft/Pending", () => { var r = LearningRecord.FromJson(File.ReadAllText(args[1])); Check(r.Recipes.All(e => e.Status == "Draft") && r.Experiments.All(e => e.Outcome == "Pending" && !e.HumanApproved)); });
            Test("actual pilot does not rank studio as truth", () => { var r = LearningRecord.FromJson(File.ReadAllText(args[1])); Check(r.ReferenceAuthority == "CompetingEvidence" && r.Findings.All(e => !e.Verified) && r.Correspondences.Any(e => e.StudioEntities.Length > 1 && e.GeneratedEntities.Length == 1) && r.Correspondences.Any(e => e.GeneratedEntities.Length > 1)); });
        }
        Console.WriteLine("RESULT " + passed + " passed / " + failed + " failed; synthetic, NO COM");
        return failed == 0 ? 0 : 1;
    }
    private static LearningRecord Ready()
    {
        var r = Fresh(); var e = r.Experiments[0]; e.Stage = "Remeasure"; e.Outcome = "Passed";
        e.EvidenceIds = new[] { "test", "remeasure" }; e.RegressionEvidenceIds = new[] { "regression" }; e.HumanApproved = true; return r;
    }
}
