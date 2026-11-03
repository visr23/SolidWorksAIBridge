# Projecting Balcony Slab V1 edits and elevated-door integration — 2026-11-03

## Outcome

Subsequent approval: the user confirmed Part3-Part7 were correct slab-only edit cases (no openings expected) and Part8's elevated opening/alignment/placement appeared correct. Scoped promotion is recorded in [the validation record](PROJECTING_SLAB_V1_VALIDATED_2026-11-03.md). Original Testing/PENDING statements below preserve the state at test execution; actual numeric results remain unchanged. No additional CAD ran for promotion.

**Five actual CAD parameter-edit cases and one separate elevated-door integration case PASSED**, each exit code 0, using a fresh unsaved document. **105 automated regression checks passed, 0 failed.** Production geometry was not modified. No railings, side walls, new production features, or Git operations were introduced.

Definition status remains **Testing**. Numeric CAD validation is complete for these six cases; human visual review/approval is **PENDING**, not assumed. These results do not constitute full-building, printer-profile, generalized host, signed-cross-origin-edit, or all-parameter-range validation.

## Test harness and commands

Added test-only BalconyEditValidation.cs and parameterized the existing test measurement helper for expected bounds/volumes/counts. The original numeric baseline classifier still handles ICE through its underlying type and deduplicates traversal. No production source changed.

Build: Visual Studio 18 Community MSBuild, tests/ManualValidation.csproj, /t:Rebuild /p:Configuration=Debug; exit 0 with existing MSB3305 COM marshaling warnings.

```powershell
powershell.exe -NoProfile -ExecutionPolicy Bypass -File .\tests\Run-RegressionTests.ps1
```

Automated result: 105 passed, 0 failed; exit 0. Then each command below ran once, sequentially, under the interactive desktop user, attaching to the existing SolidWorks instance (runtime revision 32.5.0). Execution was configured to stop on first failure; no case failed.

```powershell
.\tests\bin\Manual\SolidWorksAIBridge.ManualValidation.exe --confirm-cad BALCONY_EDIT_WIDTH
.\tests\bin\Manual\SolidWorksAIBridge.ManualValidation.exe --confirm-cad BALCONY_EDIT_DEPTH
.\tests\bin\Manual\SolidWorksAIBridge.ManualValidation.exe --confirm-cad BALCONY_EDIT_THICKNESS
.\tests\bin\Manual\SolidWorksAIBridge.ManualValidation.exe --confirm-cad BALCONY_EDIT_POSITION
.\tests\bin\Manual\SolidWorksAIBridge.ManualValidation.exe --confirm-cad BALCONY_EDIT_ELEVATION
.\tests\bin\Manual\SolidWorksAIBridge.ManualValidation.exe --confirm-cad BALCONY_UPPER_DOOR
```

Default Part template exists; Front Plane and fresh baseline geometry were checked. Each edit used the existing SolidWorks driving dimension's SystemValue, checked its resulting value, rebuilt, then independently measured actual body vertices/volumes/features. No modeler geometry operations were called on that instance after manual dimension edits.

## Safety and document inventory

| Case | New unsaved document | Documents already open before case | Result |
| --- | --- | --- | --- |
| Width | Part3 | 1 | PASS |
| Depth | Part4 | 2 | PASS |
| Thickness | Part5 | 3 | PASS |
| Position | Part6 | 4 | PASS |
| Elevation | Part7 | 5 | PASS |
| Elevated-door integration | Part8 | 6 | PASS |

The existing document at the start of the sequence was preserved, as were earlier test documents during subsequent cases. Dirty-flag comparisons passed after every case. No existing document was edited, saved, closed, or overwritten. No CAD file was saved and no second SolidWorks instance was launched. Part3 through Part8 remain open and unsaved for review.

## Parameter-edit expected versus actual results

All lengths are actual miniature mm. Initial slab: width 56, depth 24, thickness 3, X center 20, top elevation 120 above wall bottom. Initial model bounds: X [-8,48], Y [7,10], Z [1.5,25.5]. Wall remains 180 x 220 x 3, volume 118800 mm^3.

Measured values below are rounded for readability. Every actual bound matched its expected value within 0.001 mm and every volume within 0.01 mm^3; full-precision transcripts are identified below.

| Edit / document | Driving dimension requested -> actual | Expected slab bounds X / Y / Z | Actual bounds X / Y / Z | Slab volume expected -> actual mm^3 | Total expected -> actual mm^3 |
| --- | --- | --- | --- | --- | --- |
| Width / Part3 | SlabWidth 56 -> 60; actual 60 | [-10,50] / [7,10] / [1.5,25.5] | [-10,50] / [7,10] / [1.5,25.5] | 4320 -> 4320.0000000000045 | 123120 -> 123119.99999999999 |
| Depth / Part4 | SlabDepth 24 -> 26; actual 26 | [-8,48] / [7,10] / [1.5,27.5] | [-8,48] / [7,10] / [1.5,27.5] | 4368 -> 4368 | 123168 -> 123167.99999999999 |
| Thickness / Part5 | SlabThickness 3 -> 4; actual 4 | [-8,48] / [6.5,10.5] / [1.5,25.5] | [-8,48] / [6.5,10.5] / [1.5,25.5] | 5376 -> 5376.0000000000009 | 124176 -> 124175.99999999999 |
| Position / Part6 | CenterX 20 -> 30; actual 30 | [2,58] / [7,10] / [1.5,25.5] | [2,58] / [7,10] / [1.5,25.5] | 4032 -> 4032.0000000000045 | 122832 -> 122831.99999999999 |
| Elevation / Part7 | CenterY 8.5 -> 18.5; actual 18.5 | [-8,48] / [17,20] / [1.5,25.5] | [-8,48] / [17,20] / [1.5,25.5] | 4032 -> 4032.0000000000045 | 122832 -> 122831.99999999999 |

All five cases: **2 separate solid bodies; 2 sketches, 2 bosses, 0 cuts (4 generated features); edit rebuild True and verification rebuild True**. Wall bounds and volume remained unchanged. Each pre-edit baseline passed too.

### Established edit semantics

- Thickness-only edit preserves sketch center Y=8.5, so top moves from Y=10 to Y=10.5 (host elevation 120.5). This confirms the documented center-based behavior; it is not top-preserving automatic adjustment. A thickness edit is not claimed to retain a door-threshold relationship.
- Elevation test computes center Y explicitly: requested top elevation 130 - wallHeight/2 110 - thickness/2 1.5 = 18.5. Measured top Y=20 means actual host elevation 130.
- Position test edits a positive X center 20 -> 30. Other signs/cross-origin changes were not tested.

## Elevated-door integration — Part8

The existing CreateDoor API is ground-constrained. Without introducing a new production feature, the test uses the existing **CreateWindow rectangular aperture cut** as an upper-floor door opening. Consequently the tree names remain WindowSketch / WindowCut. This validates physical door-aperture geometry, not a new semantic Door definition or automatic balcony-to-door dependency system.

Explicit test calculations:

```text
wall width/height/thickness = 180 / 220 / 3
door width/height = 30 / 60
door center X = 20
door threshold host elevation = 120
door threshold model Y = 120 - 220/2 = 10
door center model Y = 10 + 60/2 = 40
CreateWindow(30, 60, 20, 40)
slab width = door width + 2*13 = 56
slab center X = door center X = 20
slab top host elevation = door threshold host elevation = 120
slab depth/thickness = 24 / 3
```

| Integration check | Expected | Actual | Result |
| --- | --- | --- | --- |
| Through-wall aperture | X [5,35], Y [10,70], Z [-1.5,1.5] | All 8 expected aperture corner vertices found on perforated wall | PASS |
| Threshold / slab top model Y | 10 / 10 | 10 / 10 within numeric tolerance | PASS |
| Horizontal center | Door 20, slab 20 | Both 20 from bounds | PASS |
| Left/right overhang | 13 / 13 | 13 / 13 from measured opening/slab bounds | PASS |
| Slab bounds | X [-8,48], Y [7,10], Z [1.5,25.5] | Match within 0.001 | PASS |
| Outward projection | +Z, depth 24 | Z [1.5,25.5] | PASS |
| Perforated wall volume mm^3 | 118800 - 30*60*3 = 113400 | 113400 | PASS |
| Slab volume mm^3 | 4032 | 4032.0000000000014 | PASS |
| Total volume mm^3 | 117432 | 117432 | PASS |
| Solids | 2 separate bodies | Perforated wall (16 vertices), slab (8 vertices) | PASS |
| Generated features | 3 sketches + 2 bosses + 1 cut = 6 | 3 / 2 / 1 | PASS |
| Rebuild | True | True | PASS |
| Runner exit | 0 | 0 | PASS |

The intact through-aperture is supported by both its eight corner positions and the correct removed volume; the separate wall and slab volumes establish that they did not unintentionally merge. The slab footprint meets the aperture at a shared boundary only, respecting V1 constraints. The earlier ground-door elevation-0 limitation remains unchanged.

## Evidence and review gate

Local ignored transcripts: tests/BALCONY_EDIT_WIDTH_actual.log, tests/BALCONY_EDIT_DEPTH_actual.log, tests/BALCONY_EDIT_THICKNESS_actual.log, tests/BALCONY_EDIT_POSITION_actual.log, tests/BALCONY_EDIT_ELEVATION_actual.log, tests/BALCONY_UPPER_DOOR_actual.log, tests/balcony-edit-build.log, tests/balcony-edit-regressions.log. Numeric outcomes are durably summarized above. UI capture did not produce a reliable SolidWorks screenshot (one capture showed an occluding browser; retry reported a destroyed stream), so no screenshot is claimed as visual evidence or saved as a CAD review artifact.

**Human visual review requested:** inspect Part3-Part7 for parameter changes and Part8 for the upper-floor opening, aligned threshold/slab top, equal overhang, outward projection, and two distinct solids. Confirm that center-based thickness behavior is acceptable for this V1. Record explicit approval or requested corrections before assessing lifecycle promotion. No approval has yet been recorded; do not mark Validated.
