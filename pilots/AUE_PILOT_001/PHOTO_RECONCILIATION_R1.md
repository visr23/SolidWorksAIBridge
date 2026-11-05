# Pilot #001 — annotated-photo reconciliation of R1

2026-11-05. Status: visual evidence review completed; **R1 NOT reconciled for CAD
approval**. No revised executable geometry, final approved numeric plan or CAD run.

## Evidence registration

Actual local filename is [annotated_building_boundary.png.png](references/annotated_building_boundary.png.png),
not the single-extension path supplied. Original file retained unchanged.
Dimensions 943 x 843; SHA-256
CE1161EF4E262B35B73D6E72C2F94EFAE28592C9DF9952E99A1B39A65C3C017B.
Photo and both generated references were visually opened in this session. The
photo is oblique, vegetation/fence obscure the architectural bottom and openings;
no calibrated camera, plane rectification or source-model alignment is available.

## Boundary and height interpretation

User's purple X excludes the side/rear region to the left of the marked cutoff.
The near-vertical red stroke around image x=200..215 marks the intended cutoff
beside a purple stroke. A separate long red line around x=640..650 accompanies
handwritten 180 mm and a roof-point box. These are distinct annotations; do not
mistake the height annotation for a front-window split or clip the facade there.
Respect the user-declared construction boundary. Pixel positions are identification
aids, not architectural coordinates or a measured depth. No side wall or roof run
behind the cutoff may enter the front-only test. R1's 12 mm support is a proposed
facade backing thickness, not calibration of that real-building cutoff.

180 miniature mm remains the user-defined architectural minimum-to-roof-maximum
extent, excluding base, fence, vegetation, satellite dish, antennas and neighboring
buildings. The photo bottom is obscured; the 180 label is a design constraint, not
a surveyed reference. Do not use the red line's pixel length to calibrate all planes.

## Conflicts and limits

| Item | Generated references / R1 | Photograph cross-check | Disposition |
| --- | --- | --- | --- |
| Large front glazing | 82.9 x 48.7 rectangular opening in forward left bay | Large tall glazed bay and projecting corner are visible; its side return is also visible | Qualitative agreement only; front-plane extents/aspect ratio not measured from oblique pixels; no return geometry in test |
| Upper window | 48.8 x 35.1 opening in tall gable | No corresponding upper glazed opening can be identified; visible dark upper region is partly obscured and shaded | Unsupported addition in R1, not confirmed absent; recommend suppressing unless user deliberately retains draft assumption |
| Two gable silhouettes | Separate lower-left triangular gable and tall main gable with R1 vertex coordinates | Overlapping roof edges, pronounced overhangs and a dark upper region are visible; exact generated two-gable silhouette and ridge locations cannot be matched confidently | Roof topology/vertex coordinates remain draft assumptions; require explicit acceptance or a corrected front outline before final numeric proposal |
| Entrance and raised door | Recess 31 x 81.7, door 20.9 x 48.1 with bottom 13.7 | Entry area heavily obscured by plants/fence; exact portal, head and threshold not recoverable here | Preserve requested entrance concept but all R1 dimensions/placement unverified; not photo-confirmed |
| Right window and surround | Opening 23.4 x 30.4, four borders 3.5, projection 3 | Right facade region partly obscured; four-edge surround and its proportions cannot be verified reliably | Keep requested feature as user guidance; ~2x border rule not a measurement; 3.5/3 remain provisional |
| Depth | Support 12, left +8, entry -6, surround +3 | Oblique relationships support some forward/back layering but no metric depths | Explicit visual-test assumptions only; depth-map colors do not verify these values |
| Overall facade proportions | Wall 224.5 wide, total roof envelope 243.0 wide at height 180 | Perspective, different facade planes and hidden bottom prevent reliable direct width/height comparison | Numeric width and scale anchor remain provisional; no claim of photogrammetric agreement |
| Generated references disagree | Elevation wall width/height about 1.25; depth diagram about 1.17 | Photo does not independently resolve this roughly 6% discrepancy | R1 preference for elevation is a design choice, not evidence reconciliation |
| Excluded region | Front-only plan has no side/rear walls | Purple X/cutoff exclude photographed side/rear extent | Scope compatible in intent; no measured cutoff/depth established |

The annotated photograph does not prove the requested conceptual facade features
are absent. It also does not justify promoting generated linework to observed
architecture. User-specified entrance, right surround and left mass remain approved
concepts; their precise topology/parameters are separately provisional.

## Numeric proposal disposition

[Numeric R1](NUMERIC_CAD_PLAN_R1.md) is retained as an unchanged candidate for review,
not final approval. Its detailed bounds and operations are reproducible assumptions;
photograph inspection does not validate them. Do not fabricate a more precise R2
from an unrectified image. In particular, do not silently execute the unsupported
upper opening or silently discard user-requested features.

Next decision needed: whether this first visual test should explicitly retain the
generated two-gable outline and R1 opening proportions as design assumptions,
with the unsupported upper window suppressed, or wait for a corrected/front-facing
outline. Once that interpretation is accepted, publish an exact numbered numeric
revision, including resulting feature/body counts, then request explicit CAD
approval. Acceptance of interpretation alone is not CAD-execution approval.

## Actual verification and limits

Read-only image review, photo dimensions/hash and Builder/source documentation
inspection only. No perspective rectification, metric photo measurement, CAD
comparison, build, regression suite, SolidWorks/COM control or Git operation.
Existing models and source implementations unchanged. No AUE/LEGO promotion.

## Superseding user clarification — R2

User confirms the upper gable window is visible in original front photographs. Retain it; the recommendation above to suppress it is superseded. Additional front originals have not yet been located for direct review. See [R2](NUMERIC_CAD_PLAN_R2.md), which also explicitly defines bay base/piers/header and provisional roofs. CAD remains unapproved.
