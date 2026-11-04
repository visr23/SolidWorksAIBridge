using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web.Script.Serialization;

namespace SolidWorksAIBridge.Analysis
{
    /// <summary>
    /// Pure in-memory validator. Supports exactly the Draft-07 keywords used by
    /// the pinned Phase 1 schemas; rejects unknown keywords instead of ignoring
    /// constraints. Not a general-purpose JSON Schema implementation.
    /// </summary>
    public static class ContractValidator
    {
        public static object Parse(string json)
        {
            return new JavaScriptSerializer { MaxJsonLength = 16777216, RecursionLimit = 256 }
                .DeserializeObject(json);
        }

        public static Dictionary<string, object> Map(object value)
        {
            var result = value as Dictionary<string, object>;
            Require(result != null, "Expected object.");
            return result;
        }

        public static object[] Items(object value)
        {
            var result = value as object[];
            Require(result != null, "Expected array.");
            return result;
        }

        public static void Require(bool condition, string message)
        {
            if (!condition) throw new ArgumentException(message);
        }

        public static void ValidateSchema(object instance, object schema)
        {
            var root = Map(schema);
            CheckKeywords(root);
            Validate(instance, root, root, "$");
        }

        private static readonly HashSet<string> Keywords = new HashSet<string>(new[]
        {
            "$schema", "$id", "title", "type", "additionalProperties", "required", "properties",
            "definitions", "$ref", "anyOf", "enum", "items", "minItems", "maxItems", "uniqueItems",
            "minLength", "pattern", "format", "minimum", "exclusiveMinimum"
        });

        private static void CheckKeywords(Dictionary<string, object> schema)
        {
            foreach (var pair in schema)
            {
                Require(Keywords.Contains(pair.Key), "Unsupported schema keyword: " + pair.Key);
                if (pair.Key == "properties" || pair.Key == "definitions")
                    foreach (var child in Map(pair.Value).Values) CheckKeywords(Map(child));
                if (pair.Key == "items") CheckKeywords(Map(pair.Value));
                if (pair.Key == "anyOf")
                    foreach (var child in Items(pair.Value)) CheckKeywords(Map(child));
            }
        }

        private static void Validate(object value, Dictionary<string, object> schema,
            Dictionary<string, object> root, string path)
        {
            if (schema.ContainsKey("$ref"))
            {
                string reference = (string)schema["$ref"];
                Require(reference.StartsWith("#/definitions/", StringComparison.Ordinal), "Local definitions only.");
                Validate(value, Map(Map(root["definitions"])[reference.Substring(14)]), root, path);
                return;
            }
            if (schema.ContainsKey("anyOf"))
            {
                bool matches = false;
                foreach (var branch in Items(schema["anyOf"]))
                {
                    try { Validate(value, Map(branch), root, path); matches = true; }
                    catch (ArgumentException) { }
                }
                Require(matches, path + ": no allowed value/availability branch.");
            }
            if (schema.ContainsKey("enum"))
                Require(Items(schema["enum"]).Any(v => Equals(v, value)), path + ": invalid enum.");
            string type = schema.ContainsKey("type") ? (string)schema["type"] : null;
            bool numeric = value is int || value is long || value is decimal || value is double || value is float;
            if (type != null)
            {
                bool valid = type == "null" ? value == null : type == "object" ? value is Dictionary<string, object> :
                    type == "array" ? value is object[] : type == "string" ? value is string :
                    type == "boolean" ? value is bool : numeric && (type == "number" ||
                    type == "integer" && Convert.ToDouble(value) % 1 == 0);
                Require(valid, path + ": expected " + type + ".");
            }
            if (numeric)
            {
                double number = Convert.ToDouble(value, CultureInfo.InvariantCulture);
                Require(!Double.IsNaN(number) && !Double.IsInfinity(number), path + ": nonfinite number.");
                if (schema.ContainsKey("minimum")) Require(number >= Convert.ToDouble(schema["minimum"]), path + ": below minimum.");
                if (schema.ContainsKey("exclusiveMinimum")) Require(number > Convert.ToDouble(schema["exclusiveMinimum"]), path + ": below exclusive minimum.");
            }
            var text = value as string;
            if (text != null)
            {
                if (schema.ContainsKey("minLength")) Require(text.Length >= Convert.ToInt32(schema["minLength"]), path + ": empty string.");
                if (schema.ContainsKey("pattern")) Require(Regex.IsMatch(text, (string)schema["pattern"], RegexOptions.ECMAScript), path + ": invalid pattern.");
                if (schema.ContainsKey("format"))
                {
                    DateTimeOffset timestamp;
                    Require((string)schema["format"] == "date-time" &&
                        Regex.IsMatch(text, @"^\d{4}-\d{2}-\d{2}T\d{2}:\d{2}:\d{2}(\.\d+)?(Z|[+-]\d{2}:\d{2})$") &&
                        DateTimeOffset.TryParse(text, CultureInfo.InvariantCulture, DateTimeStyles.None, out timestamp), path + ": invalid timestamp.");
                }
            }
            var list = value as object[];
            if (list != null)
            {
                if (schema.ContainsKey("minItems")) Require(list.Length >= Convert.ToInt32(schema["minItems"]), path + ": too few items.");
                if (schema.ContainsKey("maxItems")) Require(list.Length <= Convert.ToInt32(schema["maxItems"]), path + ": too many items.");
                if (schema.ContainsKey("uniqueItems") && (bool)schema["uniqueItems"])
                    Require(list.Distinct().Count() == list.Length, path + ": duplicate items.");
                if (schema.ContainsKey("items"))
                    for (int i = 0; i < list.Length; i++) Validate(list[i], Map(schema["items"]), root, path + "[" + i + "]");
            }
            var map = value as Dictionary<string, object>;
            if (map != null && schema.ContainsKey("properties"))
            {
                var properties = Map(schema["properties"]);
                foreach (string required in Items(schema["required"])) Require(map.ContainsKey(required), path + ": missing " + required);
                foreach (var entry in map)
                {
                    Require(properties.ContainsKey(entry.Key), path + ": unknown property " + entry.Key);
                    Validate(entry.Value, Map(properties[entry.Key]), root, path + "." + entry.Key);
                }
            }
        }

        // Call only after schema validation. References are scoped by document and
        // configuration; IDs across all entity kinds within a configuration are unique.
        public static void ValidateConsistency(object analysisValue, object manifestValue)
        {
            var analysis = Map(analysisValue);
            var manifest = Map(manifestValue);
            foreach (string key in new[] { "buildingId", "manifestId", "sourceRevision", "synthetic" })
                Require(Equals(analysis[key], manifest[key]), "Analysis/manifest mismatch: " + key);
            var assets = Index(manifest["assets"]);
            var roots = new HashSet<string>(Items(manifest["rootAliases"]).Cast<string>());
            foreach (var asset in assets.Values)
            {
                Require(roots.Contains((string)asset["rootAlias"]), "Unknown root alias.");
                string relative = (string)asset["relativePath"];
                Require(relative.Split('/').All(p => p.Length > 0 && p != "." && p != ".." &&
                    p == p.Trim() && !p.EndsWith(".", StringComparison.Ordinal)), "Unsafe relative path.");
                foreach (string link in Items(asset["relatedAssetIds"])) Require(assets.ContainsKey(link), "Dangling asset association.");
                foreach (string field in new[] { "sha256", "sizeBytes" })
                    Require(((string)asset["availability"] == "available") == Available(asset[field]), "Source availability/digest mismatch.");
            }
            var optional = Map(manifest["optionalInputs"]);
            foreach (string field in new[] { "photographs", "stl", "threeMf" })
                if (Available(optional[field]))
                    foreach (string assetId in Items(Map(optional[field])["value"]))
                        Require(assets.ContainsKey(assetId) && (string)assets[assetId]["kind"] ==
                            (field == "photographs" ? "photo" : field == "threeMf" ? "3mf" : "stl"), "Invalid optional asset link.");
            var documents = Index(analysis["documents"]);
            var evidence = new HashSet<string>();
            foreach (var document in documents.Values)
            {
                string assetId = (string)document["assetId"];
                Require(assets.ContainsKey(assetId), "Unknown document asset.");
                Require((string)assets[assetId]["kind"] == ((string)document["kind"] == "part" ? "sldprt" : "sldasm"), "Document kind mismatch.");
                if (!(bool)analysis["synthetic"]) Require(Available(assets[assetId]["sha256"]), "Real observations require source hash.");
                var frames = Index(document["frames"]);
                foreach (var frame in frames.Values) ValidateFrame(frame);
                var configurations = Index(document["configurations"]);
                foreach (var config in configurations.Values)
                {
                    Require(Items(assets[assetId]["configurationNames"]).Contains(config["name"]), "Configuration not in manifest.");
                    if (config["parentId"] != null) Require(configurations.ContainsKey((string)config["parentId"]), "Unknown configuration parent.");
                    var coverage = Items(config["coverage"]).Select(Map).ToArray();
                    Require(coverage.Length == 7 && coverage.Select(c => c["category"]).Distinct().Count() == 7, "Coverage categories must be explicit and unique.");
                    var entities = new Dictionary<string, Dictionary<string, object>>();
                    foreach (string category in new[] { "bodies", "features", "sketches", "dimensions" })
                        foreach (var pair in Index(config[category]))
                        {
                            Require(!entities.ContainsKey(pair.Key), "Duplicate entity ID.");
                            entities.Add(pair.Key, pair.Value);
                            // Portable claim evidence key includes document/configuration.
                            evidence.Add((string)document["id"] + "/" + (string)config["id"] + "/" + pair.Key);
                            var provenance = Map(pair.Value["provenance"]);
                            Require(Equals(provenance["assetId"], assetId) && Equals(provenance["documentId"], document["id"]) &&
                                Equals(provenance["configurationId"], config["id"]), "Provenance scope mismatch.");
                        }
                    var features = Index(config["features"]); var bodies = Index(config["bodies"]);
                    var sketches = Index(config["sketches"]); var dimensions = Index(config["dimensions"]);
                    foreach (var feature in features.Values)
                    {
                        Links(feature["dependsOn"], features); Links(feature["bodyIds"], bodies);
                        if (feature["treeParentId"] != null) Require(features.ContainsKey((string)feature["treeParentId"]), "Unknown tree parent.");
                        foreach (string bodyId in Items(feature["bodyIds"])) Require(Items(bodies[bodyId]["featureIds"]).Contains(feature["id"]), "Nonreciprocal feature/body link.");
                    }
                    CheckCycles(features, "dependsOn"); CheckCycles(features, "treeParentId");
                    foreach (var body in bodies.Values)
                    {
                        Links(body["featureIds"], features);
                        foreach (string featureId in Items(body["featureIds"])) Require(Items(features[featureId]["bodyIds"]).Contains(body["id"]), "Nonreciprocal body/feature link.");
                        if (Available(body["bounds"]))
                        {
                            var bounds = Map(Map(body["bounds"])["value"]);
                            Require(frames.ContainsKey((string)bounds["frameId"]), "Unknown bounding frame.");
                            Require((string)frames[(string)bounds["frameId"]]["lengthUnit"] != "unknown", "Measured bounds require resolved frame units.");
                            var min = Items(bounds["min"]); var max = Items(bounds["max"]);
                            for (int i = 0; i < 3; i++) Require(Convert.ToDouble(min[i]) <= Convert.ToDouble(max[i]), "Inverted bounds.");
                        }
                        Require((string)body["volumeUnit"] != "unknown" || !Available(body["volume"]), "Measured volume requires resolved units.");
                    }
                    foreach (var sketch in sketches.Values)
                    {
                        Require(features.ContainsKey((string)sketch["featureId"]) && frames.ContainsKey((string)sketch["frameId"]), "Unknown sketch owner/frame.");
                        Links(sketch["dimensionIds"], dimensions);
                        foreach (string dimensionId in Items(sketch["dimensionIds"])) Require(Equals(dimensions[dimensionId]["sketchId"], sketch["id"]), "Nonreciprocal sketch dimension.");
                    }
                    foreach (var dimension in dimensions.Values)
                    {
                        Require(features.ContainsKey((string)dimension["featureId"]), "Unknown dimension feature.");
                        if (dimension["sketchId"] != null)
                        {
                            string sketchId = (string)dimension["sketchId"];
                            Require(sketches.ContainsKey(sketchId) && Items(sketches[sketchId]["dimensionIds"]).Contains(dimension["id"]) &&
                                Equals(sketches[sketchId]["featureId"], dimension["featureId"]), "Invalid dimension/sketch ownership.");
                        }
                        string unit = (string)dimension["unit"], quantity = (string)dimension["quantity"];
                        Require(quantity == "length" ? new[] { "m", "mm", "cm", "in", "unknown" }.Contains(unit) :
                            quantity == "angle" ? new[] { "rad", "deg", "unknown" }.Contains(unit) : unit == "unitless" || unit == "unknown", "Dimension unit mismatch.");
                        Require(unit != "unknown" || !Available(dimension["value"]), "Unresolved units cannot carry measured values.");
                    }
                }
                CheckCycles(configurations, "parentId");
            }
            var claims = Index(analysis["claims"]);
            foreach (var claim in claims.Values)
            {
                foreach (string entity in Items(claim["evidenceIds"])) Require(evidence.Contains(entity), "Dangling claim evidence.");
                if ((string)claim["classification"] == "optimization_suggestion") Require(Items(claim["requiredChecks"]).Length > 0, "Suggestion needs validation checks.");
            }
            foreach (var candidate in Index(analysis["candidates"]).Values)
            {
                foreach (string entity in Items(candidate["entityIds"])) Require(evidence.Contains(entity), "Dangling candidate evidence.");
                Links(candidate["claimIds"], claims);
                Require(Items(candidate["claimIds"]).Cast<string>().Any(c => (string)claims[c]["classification"] == "inferred_intention"), "Candidate requires explicit interpretation.");
            }
        }

        private static bool Available(object value) { return (string)Map(value)["status"] == "available"; }
        private static Dictionary<string, Dictionary<string, object>> Index(object value)
        {
            var result = new Dictionary<string, Dictionary<string, object>>(StringComparer.Ordinal);
            foreach (var item in Items(value).Select(Map))
            {
                string id = (string)item["id"];
                Require(!result.ContainsKey(id), "Duplicate ID: " + id); result.Add(id, item);
            }
            return result;
        }
        private static void Links(object value, Dictionary<string, Dictionary<string, object>> targets)
        {
            var ids = Items(value).Cast<string>().ToArray();
            Require(ids.Distinct().Count() == ids.Length, "Duplicate link.");
            foreach (string id in ids) Require(targets.ContainsKey(id), "Dangling reference: " + id);
        }
        private static void CheckCycles(Dictionary<string, Dictionary<string, object>> nodes, string field)
        {
            var active = new HashSet<string>(); var done = new HashSet<string>();
            foreach (string id in nodes.Keys) Visit(id, nodes, field, active, done);
        }
        private static void Visit(string id, Dictionary<string, Dictionary<string, object>> nodes, string field,
            HashSet<string> active, HashSet<string> done)
        {
            if (done.Contains(id)) return;
            Require(active.Add(id), "Dependency/hierarchy cycle.");
            object links = nodes[id][field];
            foreach (string next in links == null ? new string[0] : links is string ? new[] { (string)links } : Items(links).Cast<string>())
                Visit(next, nodes, field, active, done);
            active.Remove(id); done.Add(id);
        }
        private static void ValidateFrame(Dictionary<string, object> frame)
        {
            double[][] axes = new[] { "xAxis", "yAxis", "zAxis" }.Select(k => Items(frame[k]).Select(Convert.ToDouble).ToArray()).ToArray();
            for (int i = 0; i < 3; i++)
                for (int j = 0; j < 3; j++) Require(Math.Abs(axes[i].Zip(axes[j], (a, b) => a * b).Sum() - (i == j ? 1 : 0)) < 1e-9, "Frame must be orthonormal.");
            double determinant = axes[0][0] * (axes[1][1] * axes[2][2] - axes[1][2] * axes[2][1]) -
                axes[0][1] * (axes[1][0] * axes[2][2] - axes[1][2] * axes[2][0]) +
                axes[0][2] * (axes[1][0] * axes[2][1] - axes[1][1] * axes[2][0]);
            Require(Math.Abs(determinant - 1) < 1e-9, "Frame must be right handed.");
        }
    }
}
