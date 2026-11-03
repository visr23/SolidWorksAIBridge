# BALCONY_BOTH_WALLS actual CAD result — 2026-11-03

## Outcome

**PASS — runner exit code 0**, including baseline depth 24 mm, depth propagation to 26 mm, and restoration to 24 mm. This was one explicitly approved case creating one new unsaved **Part9**, left open for visual review. BalconyAssembly V1 remains **Testing**; user visual approval is **PENDING**, not inferred from numeric success.

No production geometry changes, Git operations, additional CAD cases, document saves or document closes occurred. No second SolidWorks instance was launched.

## Execution and prerequisites

Attached to the existing process, PID 34776, Windows session 1, as the interactive desktop user outside the isolated sandbox. Compiled COM attachment succeeded. Runtime revision **32.5.0**; configured Part template **C:\ProgramData\SolidWorks\SOLIDWORKS 2024\templates\Part.prtdot** existed. Existing document count before creation was **0**. The runner's existing-document dirty-flag check completed unchanged; no existing document was edited or overwritten.

Executed exactly once:

```powershell
.\tests\bin\Manual\SolidWorksAIBridge.ManualValidation.exe --confirm-cad BALCONY_BOTH_WALLS
```

GetPathName for Part9 was empty: no CAD file was saved. The runner stopped after the authorized case and restored the baseline depth for visual review. No screenshot is claimed; the user should review the actual open test document.

Production source identities matched the compiled implementation checkpoint before execution:

- BalconyAssembly.cs: 83A8B96EB31ACE4AC788024603896ECFA89B8E64A56EF5710CD5E45C150CA27D.
- SolidWorksModeler.BalconyAssembly.cs: A06B564A9B0CFDA45CCD954771A3F181AE83ACD005253797A6B58E39E736FAD2.

The earlier **141 passing non-CAD tests** and successful builds remain recorded in [the implementation checkpoint](BALCONY_ASSEMBLY_IMPLEMENTATION_2026-11-03.md). They were not rerun during this CAD-only validation session.

## Expected versus actual geometry

Actual miniature mm unless stated otherwise; tolerances 0.001 mm and 0.01 mm^3. Bounds/shape were measured from actual vertices and volume from body mass properties, not echoed input.

| Check | Expected | Actual | Result |
| --- | --- | --- | --- |
| MainWall bounds | X [-90,90], Y [-110,110], Z [-1.5,1.5] | Exact match at all stages | PASS |
| MainWall volume mm^3 | 118800 | 118799.99999999999 at all stages | PASS |
| Baseline balcony bounds | X [-8,48], Y [7,30], Z [1.5,25.5] | X [-8.0000000000000053,48], Y [6.9999999999999991,30.000000000000004], Z [1.5,25.500000000000004] | PASS |
| Baseline balcony volume mm^3 | 5952 | 5952 | PASS |
| Total baseline volume mm^3 | 124752 | 124752 within numeric tolerance, sum of measured bodies | PASS |
| Slab | 56 x 24 x 3, top model Y=10 | U-profile dimensions/corners and volume match | PASS |
| Left wall | X [-8,-6], Y [10,30], Z [1.5,25.5] | Expected U-profile corners found at both depth ends | PASS |
| Right wall | X [46,48], Y [10,30], Z [1.5,25.5] | Expected U-profile corners found at both depth ends | PASS |
| Facade rear gap | 0 | Rear profile at Z=1.5, same as facade exterior plane | PASS |
| Wall fronts / slab front | Same, Z=25.5 | Match at baseline and after restore | PASS |
| Final solids | 2 | MainWall and BalconyAssembly_both-walls | PASS |
| Generated features | 4 sketches + 4 Boss-Extrudes = 8 | 4 sketches, 4 bosses; no cuts or Combine | PASS |
| Balcony connectivity | One connected U-shaped solid | Single measured balcony solid; 16/16 U-profile corner vertices; V=16, E=24, F=10, Euler=2 | PASS for this prismatic fixture |
| Facade isolation | No balcony-to-MainWall merge | Independent unchanged facade and separately measured balcony body | PASS |
| Rebuild | True | True at every verification; depth edit/restore rebuilds accepted | PASS |

The extrusion path used Merge Result enabled with explicit selected-body scope to the current balcony body. Successful per-wall body/volume guards plus measured final membership/shape establish the expected result for this fixture. No Boolean Combine operation was used. Independently named sketches/Boss-Extrudes remain in the feature history; human editing/visual inspection remains part of review.

## Actual depth propagation and restore

| Stage | Requested slab depth | Both wall depth dimensions | Measured balcony front Z | Measured balcony volume mm^3 | Bodies / features / rebuild |
| --- | --- | --- | --- | --- | --- |
| Baseline | 24 | 24 within 0.001 | 25.500000000000004 | 5952 | 2 / 8 / True |
| Edit | 26 | 26 within 0.001, equation-driven | 27.5 | 6447.9999999999982 | 2 / 8 / True |
| Restore | 24 | 24 within 0.001, equation-driven | 25.500000000000004 | 5952.0000000000018 | 2 / 8 / True |

All stages matched 16/16 expected U-profile vertices; rear gap stayed zero. Heights 20 mm and thicknesses 2 mm remain established by the same measured profile. MainWall volume/bounds/topology stayed unchanged (V=8, E=12, F=6). At depth 26, expected total volume 125248 mm^3 also matches the measured body sum. Restored total returns to 124752 mm^3.

## Printing geometry and evidence

Underside model Y=7 maps to build-plate Z=0 under the documented +Y-to-printer-+Z orientation. Side walls rise from model Y=10 to 30, above a flat slab footprint. Slab thickness 3 and side thickness 2 exceed the fixture's explicit 0.8 mm minimum. This verifies the intended geometry, not slicer/material qualification or a universally support-free print claim.

Original local log retained: tests/assembly-cad-first.log. Durable exact [CAD transcript](evidence/BALCONY_BOTH_WALLS_2026-11-03/actual-cad-transcript.txt) with [SHA256 manifest](evidence/BALCONY_BOTH_WALLS_2026-11-03/SHA256SUMS.txt).

Actual terminal result:

```text
BALCONY_BOTH_WALLS_NUMERIC=PASS; DEPTH_EDIT_AND_RESTORE=PASS; VISUAL_APPROVAL=PENDING
EXISTING_DOCUMENT_DIRTY_FLAGS_UNCHANGED=true
CAD_CASE_EXIT=0
```

## Required visual review / handoff

Stop after this case. Ask the user to review **Part9**, restored to depth 24: both upright side walls, flush rear faces, full slab depth, separate MainWall, one connected balcony body, independent parametric features, and underside-down printing orientation. Record explicit approval/corrections before evaluating promotion. No additional CAD mutations or production fixes are authorized by this successful test.
