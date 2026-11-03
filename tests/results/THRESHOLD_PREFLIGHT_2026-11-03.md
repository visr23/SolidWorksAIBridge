# Approved CAD validation attempt and threshold preflight — 2026-11-03

Requested order: BALCONY first, then WALL + ground DOOR + threshold-aligned projecting slab; stop after these cases or on failure. No other CAD cases were executed.

## Execution prerequisites and actual status

- Process inspection found no running SLDWORKS process.
- Visible application inventory independently showed no SolidWorks application/window.
- User was asked to open SolidWorks on the connected computer; the agent did not launch it.
- BALCONY: **BLOCKED / NOT RUN** pending an accessible running instance. No test document was created and no geometry measurements were collected.
- BALCONY_DOOR_THRESHOLD: **BLOCKED by existing API contract / CAD NOT RUN**. Pure preflight confirms that V1 rejects the required placement. This is not a CAD failure or CAD PASS.
- No existing user document was modified, saved, closed, or overwritten. No production code changes or Git operations occurred.

## Explicit integration fixture

The test-only BalconyDoorThresholdCase.cs calculates placement; no automatic door-to-slab placement is assumed.

| Quantity | Required/expected, actual miniature mm | Actual CAD |
| --- | --- | --- |
| Wall | Width 180, height 220, thickness 3 | NOT RUN |
| Door | Width 30, height 60, center X=0 | NOT RUN |
| Door aperture bounds | X [-15,15], Y [-110,-50], through wall | NOT RUN |
| Door threshold model Y | -wallHeight/2 = -110 | Not measured |
| Slab top elevation above wall bottom | thresholdY + wallHeight/2 = 0 | Rejected by pure V1 validator |
| Slab width/depth/thickness | 30 + 2*13 = 56; depth 24; thickness 3 | NOT RUN |
| Slab horizontal placement | center X=door center X=0; 13 overhang per side | NOT RUN |
| Required slab bounds | X [-28,28], Y [-113,-110], Z [1.5,25.5] | NOT RUN |
| Slab volume | 56*24*3 = 4032 mm^3 | NOT RUN |
| Wall after door volume | 118800 - 30*60*3 = 113400 mm^3 | NOT RUN |
| Total volume | 117432 mm^3 | NOT RUN |
| Bodies | 2, opening intact and slab separate | NOT RUN |
| Generated sketches / bosses / cuts | 3 / 2 / 1 (6 total), including absorbed sketches | NOT RUN |
| Rebuild | Success, with no feature errors | NOT RUN |

Current definition requires ElevationMm >= ThicknessMm and both slab top/bottom inside host height. Required elevation 0 with thickness 3 violates this. A raised slab would fail threshold alignment; BALCONY_MIXED is not a substitute. Supporting this fixture requires a separately authorized placement-policy/geometry change or a different requested opening/host arrangement. No such change was made.

The new manual case includes this pure preflight before COM connection/document creation and returns exit code 3 when blocked. Its CAD branch is prepared but has NOT executed. Do not invoke it until the first BALCONY case passes and the API limitation is resolved explicitly.

## Checks actually run

```powershell
powershell.exe -NoProfile -ExecutionPolicy Bypass -File .\tests\Run-RegressionTests.ps1
```

**97 passed, 0 failed**, exit 0: prior 95 plus explicit threshold placement arithmetic and expected ElevationMm rejection. This regression PASS confirms the current limitation, not the requested integration geometry.

ManualValidation.csproj rebuilt with Visual Studio 18 Community MSBuild, Debug configuration: exit 0; MSB3305 COM marshaling warnings. No CAD executable was invoked. Local ignored logs: tests/threshold-preflight.log, tests/threshold-manual-build.log.

## Next step

When SolidWorks is accessible, run only BALCONY first, capture actual measurements and preserve any failed document. Stop before threshold CAD execution until the user chooses how to address the reported API limitation. Keep earlier test records intact and append actual results rather than relabeling this preflight as CAD validation.
