# Equal-height slab-top wall comparison

Definition `lego.balcony.projecting-assembly.equal-height-top-face` **1.0.0**,
recipe `top-face-shared-r1`: **Validated within recorded sampled scope**, following
explicit user Part3 visual approval. Preferred equal-height two-side-wall recipe;
the eight-feature baseline remains unchanged/Testing. See the
[authoritative release scope](results/TOP_FACE_VALIDATED_2026-11-04.md).

**Actual CAD comparison PASS**, exit 0
on 2026-11-04: Part2 baseline versus Part3 candidate. Depth propagation passed in
both; candidate width propagation passed. Read-only top-face/shared-sketch/upward
boss inspection also passed. **Human visual review APPROVED** for Part3 in the
recorded scope. At the end of the CAD run both parts remained open/unsaved at
restored baseline dimensions; no SolidWorks operations were made during promotion.
See [actual results and limitations](results/TOP_FACE_CAD_2026-11-04.md).

## Explicit execution scope

After user approval, with SolidWorks already running and an appropriate default
Part template, run:

```powershell
.\tests\bin\Manual\SolidWorksAIBridge.ManualValidation.exe --confirm-cad BALCONY_TOP_FACE_COMPARE
```

This attaches to the existing instance and creates **two new unsaved parts**:
the unchanged `BALCONY_BOTH_WALLS` eight-feature baseline first, then the new
six-feature candidate. Stop on the first failure and preserve the failed part.
No launch, saves, closes, exports, or changes to existing user documents are
authorized by this case. Existing document dirty flags are checked. Keep both
comparison parts for visual review; approval to execute is not lifecycle approval.

## Implemented candidate

`CreateTopFaceBalconyAssembly` reuses the unchanged validated slab creator.
It locates the upward planar face on the exclusive slab body using actual
boundary vertices and area, then maps architectural X/Z through
`ModelToSketchTransform`. Unsupported axes/origin are rejected.
One sketch contains two disjoint, constrained center rectangles; their diagonals
remain construction-only. Both have editable independent thicknesses, equation-
driven depth and X/Z centers. One common `WallHeight` drives the upward boss.
Merge Result is enabled with exactly the balcony body selected using mark 8;
selected-body scope is enabled and automatic body selection is disabled.
The postcondition checks two solids, unchanged MainWall bounds/volume, expected
balcony volume and genus-zero topology. No Combine/cut is added.

The slab-width equation moves the outside wall edges with the slab; depth and
depth-center equations retain facade rear contact and slab front alignment.
The rear offset is an explicit `mm` literal from the supported, unedited host.
The top face provides vertical support; no slab edit semantics are changed.

## Reference comparison measurements

The table remains the test contract. All listed values/counts were measured within
tolerance in the approved 2026-11-04 run; raw values are in the actual result report.

Host: 180 x 220 x 3 mm; slab: 56 x 24 x 3 mm; X center 20;
top elevation 120 above host bottom. Walls: common height 20, thickness 2 each.

| State | Balcony volume mm³ | Solids | Baseline features | Candidate features |
| --- | ---: | ---: | ---: | ---: |
| Depth 24 | 5952 | 2 | 8 | 6 |
| Depth 26 | 6448 | 2 | 8 | 6 |
| Depth restored 24 | 5952 | 2 | 8 | 6 |
| Candidate width 60, depth 24 | 6240 | 2 | — | 6 |
| Candidate width restored 56 | 5952 | 2 | — | 6 |

Both cases check 16 rear/front U-section corners, actual body bounds/volume,
facade volume 118800 mm³ and bounds, Euler topology, rebuild and feature counts.
Candidate checks also inspect common height, independent thicknesses, depth and
X/Z-center dimensions at every state. Expected rear Z=1.5; front Z=25.5 or 27.5;
slab underside Y=7, top Y=10, wall tops Y=30. Six features means three sketches
and three bosses across the whole part, including absorbed sketches; no cuts or
Combine. Visual review should confirm both profiles share the slab-top sketch,
upward boss direction, correct body scope, feature editability, and intended
underside-down print orientation.

## Limits

Only equal-height both-wall configuration. No parapet, unequal-height trimming,
ground-floor behavior, other side modes, new dependencies, or baseline replacement.
Host must remain the current unedited front-facing MainWall. Equation X placement
uses initial sign branches; crossing the model origin is unsupported, as in the
baseline. Arbitrary rotated/translated sketch frames fail explicitly. Face-reference
stability, actual COM constraint solving, merge isolation and edit propagation
passed the documented sampled CAD comparison; arbitrary edits/hosts remain
unverified. Stored flags and measured isolation passed, but the stored scope-body
list was not independently enumerated (getter -1 without rollback). Physical print
qualification is not established. No further run or lifecycle promotion follows
automatically from this numeric PASS.
