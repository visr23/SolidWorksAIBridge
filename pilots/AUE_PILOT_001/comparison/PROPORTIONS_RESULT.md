# First proportional FRONT comparison — 2026-11-05

Status: **read-only CAD extraction PASS; provisional XY comparison completed**.
Not a validated geometric match or LEGO recipe. No CAD modeling, save, scale,
rebuild, activation/configuration change or production-builder edits. No Git.

## Available evidence and preservation

Selected open native documents, both Default configuration:
- Generated `solidworks reasults/AUE_PILOT_001_FRONT_V1.SLDPRT`: 5 solids,
  11 modeling features / 11 sketches, dirty=False, NeedsRebuild2=0.
- User-identified original `studio-buildings/טרומפלדור 15 כפר סבא/Part1.SLDPRT`:
  48 solids, 303 distinct feature/subfeature entries (includes folders/sketches,
  NOT 303 modeling operations), dirty=True, NeedsRebuild2=0.

Path/dirty flags unchanged across each completed inspection. Original was already
modified in memory; this compares its currently open state, not proven saved-file
state. No original file hash-before/after guarantee is claimed. No source or
reference files were opened, closed or saved by this helper. Both remain open.
Rebuild-needed status was read; no fresh rebuild run. Historical successful R2
rebuild and 49/165 non-COM tests remain separate evidence, not rerun results.

Final read-only [native evidence](PROPORTIONS_FRONT.json), [log](PROPORTIONS_FRONT.txt),
[metrics](PROPORTIONS_METRICS.csv), [registration/corners](PROPORTIONS_REGISTRATION.json),
[projected vertex overlay](PROPORTIONS_OVERLAY.svg). Run derived arithmetic only:
`./pilots/AUE_PILOT_001/comparison/Compare-Proportions.ps1`.

## Exact method and registration limits

Use meters-to-mm conversion only at readout. Read actual body directional extrema
through GetExtremePoint; no approximate GetBox envelope. Inspect all 48 source
solid-body extrema. Original includes outlying bodies/details; whole-document
bounding box is NOT the architectural normalization extent.

Candidate front bodies: Combine9 (main), Boss-Extrude67[1] (left bay),
Cut-Extrude33[1] (main roof), Cut-Extrude31 (left roof). Six source candidates were
sampled; the four used here have complete vertex arrays (956,557,52,60), below
1200-vertex cap. Two additional Split4 bodies are retained as context only.
Candidate architectural bottom Y=-60; top main roof Y=143.345359097175;
Hsource=203.345359097175 mm; horizontal anchor X=-126.956816904958.
These are measured CAD coordinates; their selection as corresponding facade
datums is **inferred/provisional**, not user-confirmed architectural truth.

Pilot coordinates normalized as ((x+112.25)/180,(y+49.9)/180).
Source coordinates normalized as ((x-anchor)/Hsource,(y+60)/Hsource).
No independent width stretch, no CAD transform or scale feature, no side/rear
comparison. Height=1 for both. Source-at-180 values below are arithmetic equivalents,
not requested/actual source-file modifications or a new target.

Source rectangular candidates require four measured corners with two distinct X
and Y values. Main candidates on measured z=-50.6520681865233; left candidate on
z=0. Right ROI x=60..100,y<30; upper ROI x>0,y=60..110; left ROI x=-120..0,
y=-37..28. Geometry matched rectangle corner conditions. Their interpretation as
matching clear apertures (rather than pane/frame boundaries) remains provisional;
no complete loop/face ownership/through-depth verification for source apertures.
Full plane-normal/reference-plane and depth registration remains **NOT completed**.
Thus this is an XY facade-candidate comparison, not a completed 3D registration.

## Preliminary measured proportional differences

| Quantity | Pilot | Source equivalent at height 180 | Pilot relative difference |
| --- | ---: | ---: | ---: |
| Roof envelope width | 243.00 | 247.30 | -1.74% |
| Lower ridge height | 131.10 | 136.95 | -4.27% |
| Right aperture width | 23.40 | 24.94 | -6.18% |
| Right aperture height | 30.40 | 24.94 | +21.89% |
| Large left aperture width | 82.90 | 102.54 | -19.15% |
| Large left aperture height | 48.70 | 57.54 | -15.36% |
| Upper aperture width | 48.80 | 40.88 | +19.39% |
| Upper aperture height | 35.10 | 36.63 | -4.17% |

All table values are measured coordinates under provisional semantic registration.
Dimensions are shown in common-height-equivalent mm for readability; ratios/CSV
are authoritative for this proportional task. Rounded reporting is not tolerance
certification. Aperture aspect ratios do not depend on selected height:
right pilot 0.7697 vs source candidate 1.0000; large 1.7023 vs 1.7821;
upper 1.3903 vs 1.1159. These are stronger proportional observations than position
until anchors are confirmed. In height-percentage-point terms pilot centers differ:
right (-9.19,-2.42), large (-4.70,+1.62), upper (-3.41,-3.70); X right/Y up.
These positional deltas are conditional on provisional datums, not final errors.

## Unknowns / not compared

- Exact front scope, architectural ground/top and horizontal anchor approval.
- Full roof outline correspondence, fascia/overlap/occlusion and frontmost edges;
  overlay contains projected vertices, NOT reconstructed silhouette edges.
- Source aperture versus pane/frame inner boundary classification.
- Entrance recess geometry/depth, exact projecting mass and OpeningSurround
  sections/borders/projection: **no defensible deviations yet**, depth-plane mapping
  and source body/face correspondence incomplete. No visual estimates substituted.
- Saved original equivalence: original dirty=True. No parameter edits, rebuild
  robustness or physical-print validation performed. No family promoted.

## Extraction failures/limits and Draft lessons

Initial all-face inventory was too slow and explicitly stopped; second reduced
vertex inventory and third capped inventory also stopped at the detailed Mirror1
body (10156 faces / 20128 vertices). Those attempts exited -1 and produced no
complete source JSON; they are NOT passed tests. Successful extrema-only inventory
omitted source vertices/mass properties. Subsequent selected-body inventory passed
without the expensive source mass queries. Do not diagnose an actual CAD geometry
failure from these read-performance limits. Inspector save/copy mode removed;
current helper is single-output, selected-path read-only. Surface arrays currently
empty, volumes null, vertex lists empty for unselected source bodies: unavailable
fields, not evidence of absent geometry. Display dimensions are partial.
Arithmetic guard initially rejected a five-point right ROI containing an exterior
corner. Narrowed the documented candidate ROI, then required the same strict
four-corner check; no failed check bypassed or CAD repaired.

Draft AUE recommendations: retain independent opening width/height/aspect-ratio
constraints; replace provisional proportions only after source-role confirmation;
revisit large-window size and upper-window width, and preserve two-ridge hierarchy.
Do not auto-apply these recommendations to R2.
Draft Analyzer recommendations: staged extrema -> role classification -> selected
faces/loops; no eager full-model mass/face traversal, especially detailed grilles;
record unavailable values, dirty/saved state, exact configuration, semantic matches,
uniform-scale transform, scope and confidence. Need explicit extraction budgets,
progress/checkpoints and cancellation instead of repeated broad traversal.

Exact next implementation step: confirm highlighted facade/body/rectangle mapping
and datums with user, then add narrow face/loop evidence for these candidates to
measure clear aperture boundaries, roof outlines, entrance recess and surround.
Use pure fixtures for proportional registration/role matching before extending CAD
reads. Continue front-only, no CAD reconstruction/edits or validated-builder changes.
Next building's target height will be provided from user's existing CAD per AUE-011.
