using System;
using System.Collections.Generic;

namespace SolidWorksAIBridge.Analysis
{
    /// <summary>
    /// Future inspection boundary. Inputs are immutable descriptors, never paths,
    /// streams, COM objects or existing user-document handles. No implementation
    /// is supplied in Phase 1. This contract alone is not a filesystem sandbox.
    /// </summary>
    public interface IReadOnlyCadAdapter
    {
        string AdapterVersion { get; }
        IReadOnlyCollection<string> SupportedCapabilities { get; }
        InspectionResult Inspect(InspectionRequest request);
    }

    public sealed class InspectionRequest
    {
        public string BuildingId { get; private set; }
        public string AssetId { get; private set; }
        public string ExpectedSha256 { get; private set; }
        public IReadOnlyCollection<string> ConfigurationNames { get; private set; }

        public InspectionRequest(string buildingId, string assetId, string expectedSha256,
            IEnumerable<string> configurationNames)
        {
            if (String.IsNullOrWhiteSpace(buildingId) || String.IsNullOrWhiteSpace(assetId))
                throw new ArgumentException("Building and asset IDs are required.");
            if (expectedSha256 == null || !System.Text.RegularExpressions.Regex.IsMatch(
                expectedSha256, "\\A[a-f0-9]{64}\\z"))
                throw new ArgumentException("Expected SHA-256 must be lowercase hexadecimal.");
            if (configurationNames == null)
                throw new ArgumentNullException(nameof(configurationNames));
            var names = new List<string>(configurationNames);
            if (names.Count == 0 || names.Exists(String.IsNullOrWhiteSpace) ||
                new HashSet<string>(names, StringComparer.Ordinal).Count != names.Count)
                throw new ArgumentException("Explicit unique configuration names are required.");
            BuildingId = buildingId;
            AssetId = assetId;
            ExpectedSha256 = expectedSha256;
            ConfigurationNames = names.AsReadOnly();
        }
    }

    public sealed class InspectionResult
    {
        public string Status { get; private set; }
        public string AnalysisJson { get; private set; }
        public string Limitation { get; private set; }

        public InspectionResult(string status, string analysisJson, string limitation)
        {
            if (status != "complete" && status != "partial" && status != "blocked")
                throw new ArgumentException("Unknown inspection status.");
            if (status != "blocked" && String.IsNullOrWhiteSpace(analysisJson))
                throw new ArgumentException("Inspection output is required.");
            if (status != "complete" && String.IsNullOrWhiteSpace(limitation))
                throw new ArgumentException("Incomplete inspection needs a limitation.");
            Status = status;
            AnalysisJson = analysisJson;
            Limitation = limitation;
        }
    }
}
