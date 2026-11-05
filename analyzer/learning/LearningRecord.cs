using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Script.Serialization;

namespace SolidWorksAIBridge.Analysis.Learning
{
    public sealed class Evidence
    {
        public string Id { get; set; }
        public string Asset { get; set; }
        public string Entity { get; set; }
        public string Method { get; set; }
        public string Status { get; set; }
        public double? Value { get; set; }
        public string Units { get; set; }
        public double Confidence { get; set; }
        public string Limitation { get; set; }
    }

    public sealed class Correspondence
    {
        public string Id { get; set; }
        public string Component { get; set; }
        public string[] StudioEntities { get; set; } = new string[0];
        public string[] GeneratedEntities { get; set; } = new string[0];
        public string[] EvidenceIds { get; set; } = new string[0];
        public double Confidence { get; set; }
        public bool Verified { get; set; }
        public string Limitation { get; set; }
    }

    public sealed class Finding
    {
        public string Id { get; set; }
        public string CorrespondenceId { get; set; }
        public string Classification { get; set; }
        public bool Verified { get; set; }
        public string[] EvidenceIds { get; set; } = new string[0];
        public string Rationale { get; set; }
    }

    public sealed class Recipe
    {
        public string Id { get; set; }
        public string Component { get; set; }
        public string[] Entities { get; set; } = new string[0];
        public string[] FeatureTypes { get; set; } = new string[0];
        public string[] Dependencies { get; set; } = new string[0];
        public string[] EvidenceIds { get; set; } = new string[0];
        public string Status { get; set; } = "Draft";
        public string Risks { get; set; }
    }

    public sealed class Proposal
    {
        public string Id { get; set; }
        public string FindingId { get; set; }
        public string[] RecipeIds { get; set; } = new string[0];
        public string Action { get; set; }
        public string Status { get; set; } = "Draft";
    }

    public sealed class Experiment
    {
        public string Id { get; set; }
        public string ProposalId { get; set; }
        public string FixtureId { get; set; }
        public string Stage { get; set; } = "Measure";
        public string Outcome { get; set; } = "Pending";
        public string[] EvidenceIds { get; set; } = new string[0];
        public string[] RegressionEvidenceIds { get; set; } = new string[0];
        public bool HumanApproved { get; set; }
    }

    public sealed class LearningRecord
    {
        public string Version { get; set; } = "0.1.0";
        public string Scope { get; set; } = "FrontFacade";
        public string ReferenceAuthority { get; set; } = "CompetingEvidence";
        public double TargetHeightMm { get; set; }
        public List<Evidence> Evidence { get; set; } = new List<Evidence>();
        public List<Correspondence> Correspondences { get; set; } = new List<Correspondence>();
        public List<Finding> Findings { get; set; } = new List<Finding>();
        public List<Recipe> Recipes { get; set; } = new List<Recipe>();
        public List<Proposal> Proposals { get; set; } = new List<Proposal>();
        public List<Experiment> Experiments { get; set; } = new List<Experiment>();

        internal static void Require(bool condition, string message)
        {
            if (!condition) throw new InvalidOperationException(message);
        }

        internal static bool Finite(double value)
        {
            return !Double.IsNaN(value) && !Double.IsInfinity(value);
        }

        private static HashSet<string> Ids(IEnumerable<string> values)
        {
            var result = new HashSet<string>(StringComparer.Ordinal);
            foreach (var value in values)
                Require(!String.IsNullOrWhiteSpace(value) && result.Add(value), "Missing or duplicate ID.");
            return result;
        }

        private static void References(string[] values, HashSet<string> ids)
        {
            Require(values != null, "Missing reference array.");
            Ids(values);
            Require(values.All(ids.Contains), "Dangling reference.");
        }

        public void Validate()
        {
            Require(Version == "0.1.0" && Scope == "FrontFacade", "Unsupported version/scope.");
            Require(ReferenceAuthority == "CompetingEvidence", "Neither CAD model is absolute ground truth.");
            Require(Finite(TargetHeightMm) && TargetHeightMm > 0, "Invalid target height.");
            Require(Evidence != null && Correspondences != null && Findings != null && Recipes != null && Proposals != null && Experiments != null, "Missing record collection.");
            Require(!Evidence.Any(e => e == null) && !Correspondences.Any(e => e == null) && !Findings.Any(e => e == null) && !Recipes.Any(e => e == null) && !Proposals.Any(e => e == null) && !Experiments.Any(e => e == null), "Null record.");
            var evidence = Ids(Evidence.Select(e => e.Id));
            var correspondence = Ids(Correspondences.Select(e => e.Id));
            var findings = Ids(Findings.Select(e => e.Id));
            var recipes = Ids(Recipes.Select(e => e.Id));
            var proposals = Ids(Proposals.Select(e => e.Id));
            Ids(Experiments.Select(e => e.Id));
            foreach (var item in Evidence)
            {
                Require(!String.IsNullOrWhiteSpace(item.Asset) && !String.IsNullOrWhiteSpace(item.Entity) && !String.IsNullOrWhiteSpace(item.Method), "Evidence provenance required.");
                Require(new[] { "Measured", "Inferred", "Unknown", "Passed", "Failed" }.Contains(item.Status), "Unknown evidence status.");
                Require(Finite(item.Confidence) && item.Confidence >= 0 && item.Confidence <= 1, "Invalid confidence.");
                Require(!item.Value.HasValue || Finite(item.Value.Value), "Nonfinite evidence.");
                Require(!item.Value.HasValue || !String.IsNullOrWhiteSpace(item.Units), "Measurement units required.");
                Require(item.Status != "Unknown" || (!item.Value.HasValue && !String.IsNullOrWhiteSpace(item.Limitation)), "Unknown is not zero.");
            }
            foreach (var item in Correspondences)
            {
                Require(!String.IsNullOrWhiteSpace(item.Component), "Component required.");
                Ids(item.StudioEntities ?? new string[0]); Ids(item.GeneratedEntities ?? new string[0]);
                Require(item.StudioEntities != null && item.GeneratedEntities != null && item.StudioEntities.Length + item.GeneratedEntities.Length > 0, "At least one matched or unmatched entity required.");
                Require(Finite(item.Confidence) && item.Confidence >= 0 && item.Confidence <= 1, "Invalid match confidence.");
                References(item.EvidenceIds, evidence);
                Require(!item.Verified, "V0.1 records provisional correspondences only; no automatic verification.");
                Require(!String.IsNullOrWhiteSpace(item.Limitation), "Provisional match limitation required.");
            }
            foreach (var item in Findings)
            {
                Require(correspondence.Contains(item.CorrespondenceId), "Unknown matched component.");
                Require(new[] { "LikelyAueError", "LikelyOriginalDiscrepancy", "BothPlausible", "IntentionalSimplification", "InsufficientEvidence" }.Contains(item.Classification), "Invalid discrepancy class.");
                Require(!item.Verified, "Classifications remain provisional in V0.1.");
                References(item.EvidenceIds, evidence);
                Require(!String.IsNullOrWhiteSpace(item.Rationale), "Classification rationale required.");
            }
            foreach (var item in Recipes)
            {
                Require(item.Status == "Draft", "Learning cannot promote LEGO recipes.");
                Require(!String.IsNullOrWhiteSpace(item.Component) && !String.IsNullOrWhiteSpace(item.Risks), "Recipe context/risks required.");
                Require(item.Entities != null && item.FeatureTypes != null && item.Dependencies != null, "Recipe arrays required.");
                Ids(item.Entities); Ids(item.FeatureTypes); Ids(item.Dependencies);
                References(item.EvidenceIds, evidence);
            }
            foreach (var item in Proposals)
            {
                Require(item.Status == "Draft" && findings.Contains(item.FindingId) && !String.IsNullOrWhiteSpace(item.Action), "Invalid Draft proposal.");
                References(item.RecipeIds, recipes);
                Require(item.RecipeIds.Length >= 2, "Recipe comparison requires competing recipes.");
                Require(item.RecipeIds.Select(id => Recipes.Single(r => r.Id == id).Component).Distinct(StringComparer.Ordinal).Count() == 1, "Competing recipes must declare the same component scope.");
            }
            foreach (var item in Experiments)
            {
                Require(proposals.Contains(item.ProposalId) && !String.IsNullOrWhiteSpace(item.FixtureId), "Experiment proposal/fixture required.");
                Require(Workflow.Stages.Contains(item.Stage), "Unknown workflow stage.");
                Require(new[] { "Pending", "Passed", "Failed" }.Contains(item.Outcome), "Unknown experiment outcome.");
                References(item.EvidenceIds, evidence); References(item.RegressionEvidenceIds, evidence);
                if (item.Stage == "Accept") Workflow.CheckAcceptance(item, this);
            }
        }

        public string ToJson()
        {
            Validate();
            var json = new JavaScriptSerializer { MaxJsonLength = Int32.MaxValue };
            var copy = json.Deserialize<LearningRecord>(json.Serialize(this));
            copy.Evidence = copy.Evidence.OrderBy(e => e.Id, StringComparer.Ordinal).ToList();
            copy.Correspondences = copy.Correspondences.OrderBy(e => e.Id, StringComparer.Ordinal).ToList();
            copy.Findings = copy.Findings.OrderBy(e => e.Id, StringComparer.Ordinal).ToList();
            copy.Recipes = copy.Recipes.OrderBy(e => e.Id, StringComparer.Ordinal).ToList();
            copy.Proposals = copy.Proposals.OrderBy(e => e.Id, StringComparer.Ordinal).ToList();
            copy.Experiments = copy.Experiments.OrderBy(e => e.Id, StringComparer.Ordinal).ToList();
            foreach (var e in copy.Correspondences) { Array.Sort(e.StudioEntities, StringComparer.Ordinal); Array.Sort(e.GeneratedEntities, StringComparer.Ordinal); Array.Sort(e.EvidenceIds, StringComparer.Ordinal); }
            foreach (var e in copy.Findings) Array.Sort(e.EvidenceIds, StringComparer.Ordinal);
            foreach (var e in copy.Recipes) { Array.Sort(e.Entities, StringComparer.Ordinal); Array.Sort(e.FeatureTypes, StringComparer.Ordinal); Array.Sort(e.Dependencies, StringComparer.Ordinal); Array.Sort(e.EvidenceIds, StringComparer.Ordinal); }
            foreach (var e in copy.Proposals) Array.Sort(e.RecipeIds, StringComparer.Ordinal);
            foreach (var e in copy.Experiments) { Array.Sort(e.EvidenceIds, StringComparer.Ordinal); Array.Sort(e.RegressionEvidenceIds, StringComparer.Ordinal); }
            return json.Serialize(copy);
        }

        public static LearningRecord FromJson(string text)
        {
            var value = new JavaScriptSerializer().Deserialize<LearningRecord>(text);
            Require(value != null, "Null learning record."); value.Validate(); return value;
        }
    }

    public static class Workflow
    {
        public static readonly string[] Stages = { "Measure", "Diagnose", "CompareRecipes", "Propose", "TestInIsolation", "Remeasure", "Accept", "Reject" };
        public static void Advance(Experiment experiment, string next, LearningRecord record)
        {
            record.Validate();
            int current = Array.IndexOf(Stages, experiment.Stage);
            LearningRecord.Require(current >= 0 && current < 6 && (next == "Reject" || next == Stages[current + 1]), "Skipped or terminal workflow stage.");
            if (next == "Diagnose") LearningRecord.Require(experiment.EvidenceIds.Any(id => record.Evidence.Any(e => e.Id == id && e.Status == "Measured")), "Measurement evidence required before diagnosis.");
            if (next == "Remeasure") LearningRecord.Require(experiment.Outcome != "Pending" && experiment.EvidenceIds.Any(id => record.Evidence.Any(e => e.Id == id && e.Method == "IsolatedTest" && e.Entity == experiment.FixtureId && (e.Status == "Passed" || e.Status == "Failed"))), "Actual isolated test evidence required.");
            if (next == "Accept") CheckAcceptance(experiment, record);
            experiment.Stage = next;
        }

        internal static void CheckAcceptance(Experiment experiment, LearningRecord record)
        {
            LearningRecord.Require(experiment.Outcome == "Passed" && experiment.HumanApproved, "Positive experiment and human approval required.");
            LearningRecord.Require(experiment.EvidenceIds.Any(id => record.Evidence.Any(e => e.Id == id && e.Entity == experiment.FixtureId && e.Status == "Passed" && e.Method == "IsolatedTest")), "Missing successful isolated-test evidence for this fixture.");
            LearningRecord.Require(experiment.EvidenceIds.Any(id => record.Evidence.Any(e => e.Id == id && e.Entity == experiment.FixtureId && e.Status == "Measured" && e.Method == "Remeasure")), "Missing remeasurement for this fixture.");
            LearningRecord.Require(experiment.RegressionEvidenceIds.Length > 0 && experiment.RegressionEvidenceIds.All(id => record.Evidence.Any(e => e.Id == id && e.Status == "Passed" && e.Method == "Regression")), "Successful regression evidence required.");
        }
    }
}
