# Building Dataset Analyzer V0.1 — Phase 1

Implemented and non-CAD tested contract foundation. No SolidWorks inspector,
studio intake, geometry execution, image/video processing or recipe validation.
This is **Building Analysis JSON**, not the future executable Building JSON.

## Files and boundaries

- `schemas/building-analysis.schema.json`: version 0.1.0, JSON Schema Draft-07.
- `schemas/source-manifest.schema.json`: version 0.1.0 source inventory contract.
- `ReadOnlyCadAdapter.cs`: future adapter interface and immutable request/result.
- `ContractValidator.cs`: in-memory schema-subset and semantic validation.
- `BuildingDatasetAnalyzer.Contracts.csproj`: independent .NET Framework 4.8
  library, explicit Compile entries, framework dependencies only, no COM reference.
- `fixtures/simple-facade.{analysis,manifest}.json`: authored wall/window/door graph.
- `fixtures/balcony-building.{analysis,manifest}.json`: adds a separate balcony body.
- `tests/AnalyzerTests.cs`, `tests/AnalyzerTests.csproj`: isolated non-CAD tests.
- `Run-AnalyzerTests.ps1`: build and run only this test project.

The production solution, geometry implementations, LEGO versions and existing
test sources are unchanged. The library accepts JSON strings/objects only and
contains no filesystem access. Tests read only their repository JSON fixtures and
schemas, hash those bytes before/after, and mutate test records only in memory.
Build output stays in the analyzer project/test bin and obj directories; it is
not project memory. Manifest placeholder paths are **never resolved**. There is
no CAD implementation to open, modify, save or overwrite a source document.
An interface is not an OS security boundary or proof that a future COM adapter
is safe; that guarantee must be established separately in Phase 2.

## Contract structure

The authoritative data structures are the JSON Schema definitions rather than
a second, independently evolving set of C# DTOs. Both envelopes reject unknown
properties and unknown schema versions. References use case-sensitive IDs.

| Record | Contents |
| --- | --- |
| Analysis envelope | Schema/analysis/building/manifest IDs, source revision, synthetic flag, run UTC/tool/version/environment, documents, claims, candidates, limitations |
| Document/configuration | Source asset ID, part/assembly kind, explicit units and frames, named configuration and optional parent, per-category coverage, entity arrays |
| Solid body | Name, bounds/frame/method/approximation/tolerance, volume and unit, reciprocal feature ownership, provenance |
| Feature | Native type/name, tree parent, dependency IDs, body IDs, suppression state, provenance |
| Sketch/dimension | Owner feature/frame, entity count and relation descriptions, dimension links; quantity, value/unit, driving/driven/unknown role, optional equation |
| Units/frame | Length/angle unit, architectural/miniature/unknown scale domain, optional architectural scale; origin and orthonormal right-handed axes with explicit frame length unit |
| Claim | observed_fact / inferred_intention / optimization_suggestion, statement, entity evidence, confidence/rationale, alternatives and required checks |
| LEGO candidate | Version, Draft-only lifecycle, family/type, entity and interpretation links, applicability and missing validation evidence |
| Source manifest | Building/revision/manifest identity, root aliases, assets, optional photo/STL/3MF links, limitations |
| Source asset | Kind, relative path/root alias, availability, SHA-256 and size, units/configuration names, related assets and association rationale |

Unavailable quantities use `{status, value: null, reason}` with a nonempty reason;
available quantities carry a typed value and null reason. Unsupported and not
applicable are separate statuses. No missing measurement is replaced with zero.
Unknown dimension units require unavailable values; measured body bounds and
volume require resolved frame/volume units. No automatic scale conversion occurs.
Bounds/tolerance use their frame's length unit; volume has its own cubic unit.
Architectural scale, when available, means **architectural length / miniature
length**; it is descriptive evidence, not an instruction to rescale CAD geometry.

Entity IDs are local to a source document/configuration snapshot. Claim and
candidate entity links have the form `documentId/configurationId/entityId`.
IDs must be unique within that scope across all entity kinds. Provenance repeats
the owning asset/document/configuration, native reference, method and confidence.
The source manifest resolves asset IDs to exact content hashes for nonsynthetic
observations. A future extractor must assign persistent-reference-derived IDs
within that digest/configuration and issue a new analysis ID when reanalyzing;
names alone are insufficient. Phase 1 provides no ID generation or hash intake.

Facts live in CAD entity records and explicitly classified observed claims;
architectural purpose lives in inferred claims. Suggestions must list required
checks and cannot establish improvements. Confidence is qualitative evidence
confidence, not a probability or lifecycle status. Candidates must link an
interpretation and remain Draft regardless of confidence. Synthetic observations
are visibly labeled and **never evidence of real measured CAD facts**.

Coverage must name bodies, features, sketches, dimensions, dependencies,
assemblies and equations exactly once per configuration. Partial/unavailable/
unsupported/not-applicable categories carry reasons. Fixture coverage is partial
or unavailable; fixtures do not establish extraction completeness. Feature trees
and dependency graphs do not establish historical operation chronology.

Manifest paths are portable forward-slash relative references. Root aliases
contain no absolute studio paths. Validation rejects absolute paths, traversal,
drive letters, backslashes, percent encoding, empty/dot segments and trailing
spaces/dots. It is metadata validation, **not** a filesystem path resolver:
symlinks/reparse points, Windows device names, source/output containment and
access permissions must be checked by a future authorized intake boundary.
Optional asset links may describe known but not-supplied sources; check each
asset's availability. Filenames do not prove photograph/CAD revision association.

## Validation and execution

From the repository root, with compatible Windows Visual Studio MSBuild and
.NET Framework 4.8 reference assemblies:

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File .\analyzer\Run-AnalyzerTests.ps1
powershell -NoProfile -ExecutionPolicy Bypass -File .\tests\Run-RegressionTests.ps1
```

`-ExecutionPolicy Bypass` applies only to the child process, without changing
machine policy. Either runner accepts `-MSBuildPath` if auto-discovery fails.
The legacy runner compiles installed COM type libraries but executes only
non-COM tests; it does not run the application or manual CAD harness.

Call `ValidateSchema` on both parsed records, then `ValidateConsistency`.
Schema validation covers the pinned schemas' type, local reference, required,
additional-properties, enum, anyOf, numeric/string/array and timestamp constraints.
The dependency-free validator implements **only the subset used here**, not all
of Draft-07; unknown schema keywords fail closed. It is intended for trusted
repository contracts, not adversarial/unbounded JSON. It uses the framework
JavaScriptSerializer; no external JSON Schema conformance suite was run.
Schema evolution needs validator support and focused tests before new keywords.

Semantic checks cover source identity, hash availability, configuration identity,
explicit coverage, unique IDs, typed/scoped links, provenance, reciprocal ownership,
acyclic dependencies/hierarchies, finite values, ordered bounds, frames, dimensional
unit compatibility, fact/interpretation separation and Draft-only candidates.
They do not prove source truth, geometric correctness or actual extraction accuracy.

Latest execution: [Phase 1 results](../tests/results/ANALYZER_PHASE1_2026-11-04.md).

## Proposed Phase 2 — SolidWorks CAD Inspector

1. Decide supported native versions and saved-state access API, capability matrix,
   explicit configuration scope and fail-safe behavior **before CAD access**.
   Assess Document Manager capabilities/licensing versus isolated-copy inspection;
   do not assume either recovers every record without rebuilding.
2. Implement a separate adapter and authorized intake boundary: explicit asset
   allowlist, resolved path containment and reparse-point checks, complete assembly
   reference inventory, source hashes before/after, separate outputs, no writable
   fallback. Never auto-launch or control pre-existing user documents.
3. Prove safe saved-state access on disposable synthetic CAD fixtures first.
   Record unsupported fields and inaccessible configurations; do not change source
   configurations, rebuild, repair, migrate, relink or save originals. If safety
   cannot be established, return blocked with a reason.
4. Extract deterministic per-document/configuration entities and provenance,
   expanding the versioned contract for assembly instances/transforms/mates,
   richer sketch relations/equations and dependency descriptors as supported.
   Add adapter/failure/configuration/reference tests and source-preservation evidence.
5. Only under a later authorized task, explicitly select three studio projects,
   then run audited intake/extraction and review coverage/accuracy. No current
   studio selection or execution authorization is implied by this handoff.

Still outside Phase 2: computer vision, video, mesh processing, optimization CAD
execution, recipe promotion, physical print qualification and fine-tuning.
