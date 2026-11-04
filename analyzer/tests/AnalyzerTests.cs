using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Web.Script.Serialization;
using SolidWorksAIBridge.Analysis;

namespace SolidWorksAIBridge.AnalysisTests
{
    internal static class AnalyzerTests
    {
        private static int passed;
        private static int failed;
        private static object analysisSchema;
        private static object manifestSchema;
        private static string analysisText;
        private static string manifestText;
        private static Dictionary<string, object> M(object o) { return ContractValidator.Map(o); }
        private static object[] A(object o) { return ContractValidator.Items(o); }
        private static Dictionary<string, object> Config(Dictionary<string, object> a) { return M(A(M(A(a["documents"])[0])["configurations"])[0]); }
        private static Dictionary<string, object> First(Dictionary<string, object> a, string category) { return M(A(Config(a)[category])[0]); }
        private static Dictionary<string, object> Asset(Dictionary<string, object> m) { return M(A(m["assets"])[0]); }

        private static void Test(string name, Action action, bool reject = false)
        {
            try
            {
                action();
                if (reject) throw new InvalidOperationException("Invalid fixture was accepted.");
                passed++; Console.WriteLine("PASS " + name);
            }
            catch (ArgumentException ex)
            {
                if (reject) { passed++; Console.WriteLine("PASS " + name + " [" + ex.Message + "]"); }
                else { failed++; Console.WriteLine("FAIL " + name + ": " + ex.Message); }
            }
            catch (Exception ex) { failed++; Console.WriteLine("FAIL " + name + ": " + ex); }
        }

        private static void Check(Dictionary<string, object> analysis, Dictionary<string, object> manifest)
        {
            ContractValidator.ValidateSchema(analysis, analysisSchema);
            ContractValidator.ValidateSchema(manifest, manifestSchema);
            ContractValidator.ValidateConsistency(analysis, manifest);
        }

        private static void Reject(string name, Action<Dictionary<string, object>, Dictionary<string, object>> mutate)
        {
            // A bad baseline must fail the run, never count as an expected rejection.
            Check(M(ContractValidator.Parse(analysisText)), M(ContractValidator.Parse(manifestText)));
            Test(name, () =>
            {
                var a = M(ContractValidator.Parse(analysisText)); var m = M(ContractValidator.Parse(manifestText));
                mutate(a, m); Check(a, m);
            }, true);
        }

        private static string Hash(string path)
        {
            using (var sha = SHA256.Create()) return Convert.ToBase64String(sha.ComputeHash(File.ReadAllBytes(path)));
        }

        private static int Main(string[] args)
        {
            if (args.Length != 1) { Console.Error.WriteLine("Supply analyzer directory only; no CAD inputs supported."); return 2; }
            string root = Path.GetFullPath(args[0]);
            // Only repository JSON fixtures and schemas are read; placeholder paths
            // in manifests are never resolved. Mutations below are all in memory.
            string[] files = Directory.GetFiles(Path.Combine(root, "fixtures"), "*.json")
                .Concat(Directory.GetFiles(Path.Combine(root, "schemas"), "*.json")).ToArray();
            var before = files.ToDictionary(f => f, Hash);
            analysisSchema = ContractValidator.Parse(File.ReadAllText(Path.Combine(root, "schemas/building-analysis.schema.json")));
            manifestSchema = ContractValidator.Parse(File.ReadAllText(Path.Combine(root, "schemas/source-manifest.schema.json")));
            foreach (string name in new[] { "simple-facade", "balcony-building" })
            {
                analysisText = File.ReadAllText(Path.Combine(root, "fixtures/" + name + ".analysis.json"));
                manifestText = File.ReadAllText(Path.Combine(root, "fixtures/" + name + ".manifest.json"));
                Test(name + " schema and consistency", () => Check(M(ContractValidator.Parse(analysisText)), M(ContractValidator.Parse(manifestText))));
            }
            Reject("unsupported version", (a, m) => a["schemaVersion"] = "9.0.0");
            Reject("missing run", (a, m) => a.Remove("run"));
            Reject("unexpected property", (a, m) => a["savePath"] = "source.SLDPRT");
            Reject("invalid timestamp", (a, m) => M(a["run"])["utc"] = "yesterday");
            Reject("building mismatch", (a, m) => m["buildingId"] = "other");
            Reject("source revision mismatch", (a, m) => m["sourceRevision"] = "other");
            Reject("manifest mismatch", (a, m) => m["manifestId"] = "other");
            Reject("synthetic mismatch", (a, m) => a["synthetic"] = false);
            Reject("real observation without digest", (a, m) => { a["synthetic"] = false; m["synthetic"] = false; });
            Reject("duplicate assets", (a, m) => m["assets"] = new object[] { Asset(m), Asset(m) });
            Reject("unknown root", (a, m) => Asset(m)["rootAlias"] = "other");
            foreach (string path in new[] { "../building.SLDPRT", "a/../../b", "C:/building.SLDPRT", "/root/file", "a\\file", "//server/share/file", "a/%2e%2e/file", "a/./b", "a//b", "a./file" })
            {
                string unsafePath = path;
                Reject("unsafe path " + path, (a, m) => Asset(m)["relativePath"] = unsafePath);
            }
            Reject("dangling asset association", (a, m) => Asset(m)["relatedAssetIds"] = new[] { "missing" });
            Reject("available source without hash", (a, m) => Asset(m)["availability"] = "available");
            Reject("fabricated hash", (a, m) => Asset(m)["sha256"] = new Dictionary<string, object> { { "status", "available" }, { "value", "not-a-digest" }, { "reason", null } });
            Reject("optional type mismatch", (a, m) => M(M(m["optionalInputs"])["photographs"])["value"] = new[] { "cad-1" });
            Reject("unknown document asset", (a, m) => M(A(a["documents"])[0])["assetId"] = "missing");
            Reject("document type mismatch", (a, m) => M(A(a["documents"])[0])["kind"] = "assembly");
            Reject("configuration not declared", (a, m) => Config(a)["name"] = "Other");
            Reject("missing coverage", (a, m) => Config(a)["coverage"] = new object[0]);
            Reject("configuration cycle", (a, m) => Config(a)["parentId"] = "cfg-default");
            Reject("duplicate feature", (a, m) => Config(a)["features"] = new[] { First(a, "features"), First(a, "features") });
            Reject("provenance mismatch", (a, m) => M(First(a, "features")["provenance"])["configurationId"] = "other");
            Reject("dangling dependency", (a, m) => First(a, "features")["dependsOn"] = new[] { "missing" });
            Reject("dependency cycle", (a, m) => First(a, "features")["dependsOn"] = new[] { "feature-window" });
            Reject("feature hierarchy cycle", (a, m) => First(a, "features")["treeParentId"] = "feature-wall");
            Reject("body scope missing", (a, m) => First(a, "features")["bodyIds"] = new[] { "missing" });
            Reject("body link not reciprocal", (a, m) => First(a, "bodies")["featureIds"] = new string[0]);
            Reject("inverted bounds", (a, m) => M(M(First(a, "bodies")["bounds"])["value"])["min"] = new object[] { 999, 0, 0 });
            Reject("invalid vector", (a, m) => M(M(First(a, "bodies")["bounds"])["value"])["min"] = new object[] { 0, 0 });
            Reject("negative tolerance", (a, m) => M(M(First(a, "bodies")["bounds"])["value"])["tolerance"] = -1);
            Reject("unknown bounding frame", (a, m) => M(M(First(a, "bodies")["bounds"])["value"])["frameId"] = "missing");
            Reject("bounds with unresolved units", (a, m) => M(A(M(A(a["documents"])[0])["frames"])[0])["lengthUnit"] = "unknown");
            Reject("volume with unresolved units", (a, m) => M(A(Config(a)["bodies"])[1])["volumeUnit"] = "unknown");
            Reject("sketch frame missing", (a, m) => First(a, "sketches")["frameId"] = "missing");
            Reject("dimension ownership mismatch", (a, m) => First(a, "dimensions")["sketchId"] = "sketch-window");
            Reject("dimension quantity mismatch", (a, m) => First(a, "dimensions")["unit"] = "deg");
            Reject("unresolved measured units", (a, m) => First(a, "dimensions")["unit"] = "unknown");
            Reject("unknown missing value without reason", (a, m) => M(First(a, "dimensions")["equation"])["reason"] = "");
            Reject("unavailable value fabricated", (a, m) => M(First(a, "dimensions")["equation"])["value"] = "invented");
            Reject("nonfinite dimension", (a, m) => M(First(a, "dimensions")["value"])["value"] = Double.NaN);
            Reject("invalid confidence", (a, m) => M(M(A(a["claims"])[0])["confidence"])["level"] = "certain");
            Reject("unclassified claim", (a, m) => M(A(a["claims"])[0])["classification"] = "ai_fact");
            Reject("dangling claim evidence", (a, m) => M(A(a["claims"])[0])["evidenceIds"] = new[] { "doc-1/cfg-default/missing" });
            Reject("suggestion without checks", (a, m) => M(A(a["claims"]).Last())["requiredChecks"] = new string[0]);
            Reject("automatic candidate promotion", (a, m) => M(A(a["candidates"])[0])["lifecycle"] = "Validated");
            Reject("candidate without interpretation", (a, m) => M(A(a["candidates"])[0])["claimIds"] = new[] { "fact-width" });
            Reject("candidate missing evidence", (a, m) => M(A(a["candidates"])[0])["entityIds"] = new[] { "doc-1/cfg-default/missing" });
            Reject("candidate missing validation gaps", (a, m) => M(A(a["candidates"])[0])["missingEvidence"] = new string[0]);
            Reject("frame not orthonormal", (a, m) => M(A(M(A(a["documents"])[0])["frames"])[0])["xAxis"] = new object[] { 2, 0, 0 });
            Reject("left handed frame", (a, m) => M(A(M(A(a["documents"])[0])["frames"])[0])["zAxis"] = new object[] { 0, 0, -1 });
            Test("unknown schema keyword fails closed", () => ContractValidator.ValidateSchema(1, ContractValidator.Parse("{\"multipleOf\":2}")), true);
            Test("unknown dimension explicitly unavailable", () =>
            {
                var a = M(ContractValidator.Parse(analysisText));
                var dimension = First(a, "dimensions"); dimension["unit"] = "unknown";
                dimension["value"] = new Dictionary<string, object> { { "status", "unavailable" }, { "value", null }, { "reason", "Units unresolved in synthetic example." } };
                Check(a, M(ContractValidator.Parse(manifestText)));
            });
            Test("adapter request copies configuration list", () =>
            {
                var names = new List<string> { "Default" }; var request = new InspectionRequest("b", "a", new string('a', 64), names);
                names.Clear(); ContractValidator.Require(request.ConfigurationNames.Count == 1, "Request mutated.");
            });
            Test("adapter rejects hashless request", () => new InspectionRequest("b", "a", null, new[] { "Default" }), true);
            Test("adapter rejects implicit configuration selection", () => new InspectionRequest("b", "a", new string('a', 64), new string[0]), true);
            Test("blocked result requires reason", () => new InspectionResult("blocked", null, null), true);
            Test("contract assembly has no COM/geometry reference", () => ContractValidator.Require(
                typeof(IReadOnlyCadAdapter).Assembly.GetReferencedAssemblies().All(r => !r.Name.Contains("Interop") && r.Name != "SolidWorksAIBridge"), "Unsafe reference."));
            Test("schema and fixture input bytes preserved", () => ContractValidator.Require(before.All(p => Hash(p.Key) == p.Value), "Input modified."));
            Console.WriteLine("RESULT: " + passed + " passed, " + failed + " failed.");
            return failed == 0 ? 0 : 1;
        }
    }
}
