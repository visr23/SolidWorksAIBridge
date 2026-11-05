# Isolated learning records V0.1

Separate .NET Framework 4.8 library; no COM, CAD access, production reference or new
dependency. Existing Analyzer 0.1.0 contracts/schema/fixtures are unchanged. This
learning JSON is a separate format, not Building Analysis or executable Building JSON.

`LearningRecord` stores component correspondences (including one-to-many,
many-to-one and unmatched entities), provenance-qualified measurements and unknowns,
provisional discrepancy classifications, competing Draft recipes with direct
dependency references, proposed corrections and experiment/approval state.
Neither CAD is ground truth. Photographs are independent evidence, with uncertainty.
Numeric confidence is a review score, not a calibrated probability.

Validation rejects duplicate/dangling references, missing provenance/units,
nonfinite measurements, unknown-as-zero, empty matches, invalid confidence,
automatic verification/promotion and recipe comparisons with unequal declared
component scope. Recipe concepts remain Draft even if a controlled experiment is
accepted. No library-registration or CAD-modeling API is supplied.

`Workflow.Advance` enforces Measure -> Diagnose -> CompareRecipes -> Propose ->
TestInIsolation -> Remeasure -> Accept, or terminal Reject. Acceptance requires
positive isolated evidence and remeasurement for the SAME fixture, successful
regression evidence and human approval. Records validate evidence claims; they do
not authenticate real test execution, signed approvals or native source identity.
Calling code must preserve actual logs/provenance. Current pilot experiment Pending.

`Geometry.cs` provides uniform registration, conditional rectangular IoU, closed
native-line-loop checks and a cooperative count/time/cancellation budget. It does
not turn point clouds into silhouettes, compute general polygon booleans,
perform hidden-line removal or infer architectural roles. A single native COM
call cannot be preempted by this cooperative budget. Boundary edge references
must be qualified by snapshot hash/configuration; names/indices are not durable IDs.

Deterministic JSON serialization sorts record/reference collections without
mutating caller objects. This is stable for the supported DTO format, not a
general canonical-JSON implementation. Synthetic tests cover transitions/gates,
matching, serialization, units, unknowns, overlap and boundary/cancellation cases.

From repository root:

```powershell
powershell.exe -NoProfile -ExecutionPolicy Bypass -File analyzer/learning/Run-LearningTests.ps1
```

Optionally validate the pilot record in the same run:

```powershell
powershell.exe -NoProfile -ExecutionPolicy Bypass -File analyzer/learning/Run-LearningTests.ps1 -PilotRecord pilots/AUE_PILOT_001/comparison/learning/PILOT_LEARNING_RECORD.json
```

Tests never attach to SolidWorks or run production geometry. Optional pilot-record
checks inspect JSON only. No CI or full Golden Dataset established by this addition.
Actual results: [pilot validation report](../../pilots/AUE_PILOT_001/comparison/learning/TEST_RESULTS.md).

The pilot-only `FrontEvidence.csproj` is the bounded native read adapter for selected
already-open documents. It does not implement/generalize `IReadOnlyCadAdapter` or
the saved-state Inspector. The actual comparison, photographs and blockers live in
the [pilot reports](../../pilots/AUE_PILOT_001/comparison/learning/index.html).
