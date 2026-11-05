# Verification — visual comparison and controlled learning foundation

2026-11-05. Windows / .NET Framework 4.8 / full Visual Studio MSBuild. No new
package dependency. Production builders and established evidence unchanged.

| Exact repository-root command | Actual result |
| --- | --- |
| `MSBuild.exe analyzer/learning/tests/LearningTests.csproj /t:Rebuild /nologo /verbosity:minimal` | Exit 0, separate COM-free library/test executable |
| `analyzer/learning/tests/bin/Debug/LearningTests.exe analyzer/learning/tests/synthetic.json pilots/AUE_PILOT_001/comparison/learning/PILOT_LEARNING_RECORD.json` | Exit 0, **65 passed / 0 failed** (62 synthetic/policy/geometry + 3 actual pilot JSON checks); no COM |
| `powershell.exe -NoProfile -ExecutionPolicy Bypass -File analyzer/Run-AnalyzerTests.ps1` | Exit 0, **70 passed / 0 failed**, existing analyzer foundation |
| `powershell.exe -NoProfile -ExecutionPolicy Bypass -File tests/Run-RegressionTests.ps1` | Exit 0, **165 passed / 0 failed**, CAD tests NOT RUN; existing COM build warnings |
| `MSBuild.exe pilots/AUE_PILOT_001/comparison/FrontEvidence.csproj /t:Rebuild /nologo /verbosity:minimal` | Exit 0; existing MSB3305 interop warnings |
| `pilots/AUE_PILOT_001/comparison/bin/FrontEvidence/AuePilot001.FrontEvidence.exe <absolute-new-path-to-learning/FRONT_EDGE_EVIDENCE.json>` | One completed bounded read, exit 0; **22 selected planar faces / 32 closed native line loops**, no curved edges in those selected loops |
| `powershell.exe -NoProfile -ExecutionPolicy Bypass -File pilots/AUE_PILOT_001/comparison/Build-VisualLearning.ps1` | Exit 0; three SVG/PNG views, learning/measurement JSON and viewer generated |
| `./pilots/AUE_PILOT_001/comparison/Write-LearningReports.ps1` | Exit 0; component/recipe/controlled-experiment reports generated |

Full logs: [65 checks](TESTS.txt), [learning build](TEST_BUILD.txt),
[70 existing analyzer checks](EXISTING_ANALYZER_TESTS.txt),
[165 legacy checks](LEGACY_REGRESSIONS.txt), [initial native helper build](EDGE_BUILD.txt),
[final helper build](EDGE_BUILD_FINAL.txt), [native read](EDGE_READ.txt),
[report build](REPORT_BUILD.txt), [production hashes](PRODUCTION_HASH_RESULT.txt).
Historical 49 body-identity tests were NOT rerun because their code did not change.

## Native-read scope and preservation

Both exact selected documents already open; no open/close/activate, save, rebuild,
configuration changes, dimension edits, sketch actions or geometry operations.
Original dirty=True and pilot dirty=False; all preexisting document metadata and
active-document path/count preserved by finally checks. Source manufacturing/
merge definitions not accessed. No full-tree, grille-body, mass-property or all-model
face extraction. Read 5 named bodies per model, selected faces/loops and direct
parents only; maximum observed direct parents 6. Checkpoint budget used 1183.

Adapter has cooperative count/time/cancellation checks (6000 checkpoints, 180s),
per-body 600-face/per-face 40-loop/per-loop 160-edge caps and a 32-direct-parent cap.
Create `<output>.cancel` to request cancellation; checks cannot interrupt a single
in-flight COM call. Per-body JSON checkpoints preserve partial evidence. Output
must be a NEW .json under the pilot learning directory, not a CAD path. Strict
output path and parent-cap hardening compiled after the completed read; no CAD
reread was needed (all observed parent lists <=6). Read-time parent sampling did
not omit any actual observed parent. No claims about saved original equivalence.

## Failures / not-run checks

First report generation in PowerShell 7 failed loading .NET Framework
System.Web.Util.Utf16StringValidator. Corrected runtime to Windows PowerShell 5
(`powershell.exe`); report generation and JSON validation passed. No CAD access
occurred in that report failure. Native extraction did not fail in this task.
A diagnostic one-line PowerShell inventory command had a parser error and was
corrected; no CAD actions or source evidence were changed.

No new CAD modeling/parameter-edit/rebuild experiment, no photo calibration,
full hidden-line silhouette, curved-edge reconstruction, complete dependency/
constraint graph, manufacturing qualification or LEGO promotion. These are not
passed checks. Three image outputs were visually opened and inspected for
geometry/annotation legibility. They are labeled partial native-face elevations,
with conditional rectangle overlap only; unknown source roles remain explicit.

All **13** production source/project hashes in the current session snapshot match
at comparison completion. No approved family, production entry point, existing
fixture or successful historical report was edited. No Git operation.

Final checks: report viewer local links checked (13 references, 0 missing); final native helper build exit 0. Invalid .SLDPRT output argument rejected with expected exit 2 before COM attachment, no file created ([log](INVALID_OUTPUT_PREFLIGHT.txt)). No extractor/build process remained running. Final production hash check 13/13 unchanged.
