# BALCONY classifier correction and actual rerun — 2026-11-03

## Outcome

**Automated suite: 105 passed, 0 failed. BALCONY numeric CAD baseline: PASS, runner exit 0.** Definition remains **Testing**, not Validated. Documented parameter-edit checks, visual evidence/review and required final approvals have not been completed; this is not a declaration that all lifecycle validation requirements passed.

Only test-side classification and focused regression tests were changed. Existing production geometry is unchanged. No Git operations occurred. The prior failed run is preserved in [its original report](BALCONY_CAD_2026-11-03.md), not relabeled as passing.

## Classifier correction

CadFeatureClassifier.TryResolveOnce retains the existing name-based traversal deduplication. It reads GetTypeName only for an ICE wrapper, then classifies the resolved type. An ordinary Boss/Extrusion does not query the fallback. Wrapped and unwrapped appearances of the same named feature cannot count twice; absorbed sketches encountered again are also skipped.

BalconyCadChecks uses this shared test-only classifier. No changes to SolidWorks settings or production extrusion are needed. Eight new non-COM tests exercise ordinary Boss and Extrusion, ICE wrapping either type, duplicate wrapped visits, distinct wall/slab bosses, a wrapped Cut, and repeated absorbed sketches.

## Commands and environment

```powershell
powershell.exe -NoProfile -ExecutionPolicy Bypass -File .\tests\Run-RegressionTests.ps1
& 'C:\Program Files\Microsoft Visual Studio\18\Community\MSBuild\Current\Bin\MSBuild.exe' .\tests\ManualValidation.csproj /t:Rebuild /p:Configuration=Debug /nologo /verbosity:minimal
.\tests\bin\Manual\SolidWorksAIBridge.ManualValidation.exe --confirm-cad BALCONY
```

Automated script and manual harness build: exit 0. Builds retain MSB3305 legacy COM marshaling warnings. Automated result: **105 passed, 0 failed** (97 prior checks + 8 classifier checks). Automated tests made no CAD calls.

The single CAD command ran as the interactive desktop user outside the isolated sandbox, attaching to the existing SolidWorks instance. Runtime revision: **32.5.0**. Default Part template exists at C:\ProgramData\SolidWorks\SOLIDWORKS 2024\templates\Part.prtdot. No second instance launched. Front Plane exists in the generated part.

## Actual CAD case

Created one new unsaved **Part2**. GetPathName returned empty. Existing document count before creation was **0**, and the existing-document dirty-flag check completed. No existing user document was edited, saved, closed or overwritten. The test did not close the earlier Part1; it was not present in this run's initial document inventory. Part2 remains open and unsaved after execution.

Inputs unchanged: wall 180 x 220 x 3 mm; slab width 56, depth 24, thickness 3, center X=20, top elevation 120 above wall bottom. All values are actual miniature mm.

| Check | Expected | Actual | Result |
| --- | --- | --- | --- |
| Wall bounds X/Y/Z | [-90,90] / [-110,110] / [-1.5,1.5] | Exact match | PASS |
| Slab bounds X | [-8,48] | [-8,48] | PASS |
| Slab bounds Y | [7,10] | [6.9999999999999991,10.000000000000002] | PASS |
| Slab bounds Z | [1.5,25.5] | [1.5,25.500000000000004] | PASS |
| Slab width/depth/thickness | 56 / 24 / 3 | 56 / 24.000000000000004 / 3.0000000000000027 from vertices | PASS |
| Slab horizontal center | 20 | 20 from bounds | PASS |
| Slab top above wall bottom | 120 | 120 within floating-point tolerance | PASS |
| Extrusion direction | Outward +Z | From facade Z=1.5 to Z=25.5 | PASS |
| Wall volume mm^3 | 118800 | 118799.99999999999 | PASS |
| Slab volume mm^3 | 4032 | 4032.0000000000023 | PASS |
| Total volume mm^3 | 122832 | 122831.99999999999 | PASS |
| Solid bodies | 2 separate solids | 2: wall and slab | PASS |
| Generated sketches | 2 | 2 | PASS |
| Generated bosses | 2 | 2; slab ICE resolved to Boss | PASS |
| Generated cuts | 0 | 0 | PASS |
| Total generated features incl. absorbed sketches | 4 | 4 | PASS |
| Rebuild | True | True | PASS |
| Runner exit code | 0 | 0 | PASS |

Bounds derive from body vertices and volume from mass properties, not input echo. Numeric differences meet 0.001 mm / 0.01 mm^3 tolerances. The two measured solids confirm the touching facade/slab were not merged. The generated features are WallSketch, MainWall, BalconySlabSketch_balcony-01, and BalconySlab_balcony-01.

Key actual output:

```text
NEW_TEST_DOCUMENT=Part2
NEW_TEST_PATH=
SOLID_BODY_COUNT=2
FEATURE=MainWall; TYPE=Extrusion; RESOLVED_TYPE=Extrusion
FEATURE=BalconySlab_balcony-01; TYPE=ICE; RESOLVED_TYPE=Boss
GENERATED_SKETCHES=2; BOSSES=2; CUTS=0
TEST_DOCUMENT_REBUILD=True
BALCONY_NUMERIC_BASELINE=PASS; PARAMETER_EDIT_AND_VISUAL_REVIEW=NOT_RUN
EXISTING_DOCUMENT_DIRTY_FLAGS_UNCHANGED=true
CAD_CASE_EXIT=0
```

Local ignored transcripts: tests/ice-classifier-tests.log, tests/ice-manual-build.log, tests/balcony-cad-rerun.log. No CAD files or screenshots were saved. No other CAD case was run and no manual parameter changes were made.

## Remaining acceptance/approval work

The BALCONY runner now passes all baseline numeric acceptance assertions. Full documented validation is still incomplete: width/depth edit-and-rebuild checks, driving-dimension inspection, visual evidence/review, and final approval remain pending. Broader legacy/mixed regression cases and the ground-door threshold limitation remain separate. Stop after this requested rerun; perform further CAD work only within a subsequent authorized scope.
