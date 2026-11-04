# Building Dataset Analyzer V0.1 Phase 1 — 2026-11-04

Implemented contract foundation only; SolidWorks inspection remains planned.

## Executed verification

Environment: Windows, Visual Studio 18 Community MSBuild Current (toolchain path
reported by runner), .NET Framework 4.8. No new package dependency.

| Exact command from repository root | Actual result |
| --- | --- |
| `.\analyzer\Run-AnalyzerTests.ps1` | Blocked by PowerShell script execution policy; no build/tests in this invocation |
| `.\tests\Run-RegressionTests.ps1` | Blocked by PowerShell script execution policy; no build/tests in this invocation |
| `powershell -NoProfile -ExecutionPolicy Bypass -File .\analyzer\Run-AnalyzerTests.ps1` | Final rebuild exit 0; **70 passed, 0 failed**, no compiler warnings reported |
| `powershell -NoProfile -ExecutionPolicy Bypass -File .\tests\Run-RegressionTests.ps1` | Rebuild exit 0; **165 passed, 0 failed**, existing MSB3305 COM marshaling warnings |

The first analyzer build succeeded but execution failed its two positive fixtures:
qualified entity references used `/`, while their initial schema allowed only local
IDs. Its negative checks were therefore not independent evidence. Corrected the
scoped-reference schemas, verified both positive fixtures, and added an explicit
valid-baseline prerequisite before every rejection mutation. Intermediate corrected
run: 67/0. Added unresolved body-unit checks and a valid unknown-dimension case;
final run: **70/0**. No failed run is being claimed as a passing suite.

Final analyzer coverage: two synthetic building pairs; unsupported/missing schema
fields, versions/timestamps, unknown schema keywords, source identity/digests,
unsafe manifest paths, optional asset kinds, missing/duplicate IDs, configuration
coverage/hierarchy, scoped provenance/references, dependency/tree cycles, reciprocal
feature/body/sketch/dimension links, bounds/tolerance/frame rules, unresolved and
incompatible units, unavailable-value reasons, nonfinite values, confidence and
claim classification, suggestion checks, Draft-only candidate policy, immutable
adapter requests, explicit configuration requirements and blocked-result reasons.
Schema/fixture bytes were hashed before/after by the passing runner and unchanged.
The independent contract assembly reference check passed (no COM/geometry reference).

## Not run and limitations

- No SolidWorks launch, attach, document access/control or CAD execution. No real
  studio dataset access, native source hash intake, save/export or Git operations.
- No application/manual harness build or CAD suite: production geometry is untouched;
  these are outside scope and non-CAD tests do not validate extraction or geometry.
- No inspector implementation, actual extraction measurements, studio accuracy,
  third-party JSON Schema conformance suite or filesystem resolver security test.
- Fixtures are authored graphs/bounds/dimensions, not generated CAD or a Golden
  Dataset. Full BREP, assembly instance/mate transforms, rich sketch entities,
  physical printing, CV/video/mesh processing and recipe validation remain absent.
- Library validates JSON in memory and has no filesystem or COM access. Future
  adapters still need independent safety enforcement and source-preservation proof.

Next action: define the Phase 2 saved-state API/version/configuration access strategy
and capability matrix, then implement the isolated inspector against disposable
synthetic CAD fixtures under separate CAD authorization before studio intake.


## Fresh verification before authorized Git publication

On 2026-11-04, reran the same process-scoped PowerShell command documented above
for analyzer/Run-AnalyzerTests.ps1. Rebuild exit 0, **70 passed / 0 failed**,
including input-byte preservation and no COM/geometry assembly reference.
No CAD execution or source changes in this publication checkpoint. Regression
165/0 was verified during the preceding photo-blockout integration and not rerun
here. User explicitly authorized commit/push of this formerly local foundation.
