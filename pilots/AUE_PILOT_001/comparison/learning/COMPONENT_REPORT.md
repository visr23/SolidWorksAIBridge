# Component correspondence and diagnostic report

2026-11-05. **Partial visual correspondence, measured native boundaries, provisional
architectural registration. Neither studio CAD nor AUE is absolute ground truth.**

## Views and evidence

- [Original Studio FRONT](studio-front.svg) / [PNG](studio-front.png).
- [Generated AUE R2 FRONT](generated-front.svg) / [PNG](generated-front.png).
- [Supported difference overlay](opening-differences.svg) / [PNG](opening-differences.png).
- [Combined viewer](index.html).
- [Actual bounded read](FRONT_EDGE_EVIDENCE.json), [checkpoints/log](EDGE_READ.txt),
  [input hashes](INPUT_MANIFEST.json), [correspondence/learning record](PILOT_LEARNING_RECORD.json).

Views draw line segments from actual native planar-face loops. Curves are NOT
replaced by chords; point clouds/hulls are NOT used. All loops in this particular
read are closed straight-edge loops. Selected coplanar surface boundaries include
backing faces and overlapping edges; no hidden-line removal or full architectural
silhouette certification. Source roof fascia faces are measured bands, not invented
filled roof triangles. Missing source entry/surround boundaries are not fabricated.
`?` marks unverified architectural correspondence, not an unmeasured edge.
Native loop/edge semantics follow [SOLIDWORKS ILoop2.GetEdges](https://help.solidworks.com/2024/english/api/sldworksapi/SolidWorks.Interop.sldworks~SolidWorks.Interop.sldworks.ILoop2~GetEdges.html);
edge orientation is assembled by endpoints, never inferred from segment ordering.

Both use X right/Y up, identical pixel scale and uniform height normalization:
pilot ((x+112.25)/180,(y+49.9)/180), studio
((x+126.956816904958)/203.345359097175,(y+60)/203.345359097175).
Source bottom/top/anchor remain inferred choices. Nominal matched front-facing
planar surfaces have Z-parallel normals, verified by native surface parameters;
full semantic exterior-plane/occlusion registration is incomplete. No CAD model
was transformed. Pilot height remains 180 mm. For future pilots, the user supplies
the actual studio-model target height; do not silently reuse 180.

## Measurements

All dimensions/positions below are **dimensionless / architectural height**.
Aspect = width/height; center X measured from selected left anchor and center Y
from ground. Raw mm and exact native face/loop references are retained in
[opening measurements](OPENING_COMPARISON.json) and
[surface measurements](SURFACE_MEASUREMENTS.json).

O1/O2/O3 use measured rectangular **host-panel openings**, not proven clear air
through every body: source glazing/shutters may occupy them. Other rows are
bounding boxes of the selected native boundary edges. These can differ in
architectural decomposition: M1 studio includes upper-gable backing while pilot
M1 is full-width wall behind the bay with a separate gable. Do NOT rank those bbox
differences as errors or compute inappropriate full-component overlap.

| ID | Studio W/H x H/H; center (X/H,Y/H); aspect | AUE W/H x H/H; center; aspect | Conditional IoU |
| --- | --- | --- | --- |
| O1 | 0.5697 x 0.3197; (0.3217, 0.2729); 1.7821 | 0.4606 x 0.2706; (0.2747, 0.2892); 1.7023 | 0.6843 |
| O2 | 0.1386 x 0.1386; (1.0203, 0.2842); 1.0000 | 0.1300 x 0.1689; (0.9283, 0.2600); 0.7697 | 0.1538 |
| O3 | 0.2271 x 0.2035; (0.7730, 0.7140); 1.1159 | 0.2711 x 0.1950; (0.7389, 0.6769); 1.3903 | 0.5432 |
| R1 | 0.9700 x 0.4397; (0.8360, 0.7801); 2.2059 | 1.0228 x 0.4456; (0.7803, 0.7772); 2.2955 | Not comparable / unavailable |
| R2 | 0.7493 x 0.1951; (0.3217, 0.6633); 3.8406 | 0.6456 x 0.1739; (0.2644, 0.6414); 3.7125 | Not comparable / unavailable |
| M1 | 0.8852 x 0.9565; (0.8115, 0.4782); 0.9255 | 1.2472 x 0.5544; (0.6236, 0.2772); 2.2495 | Not comparable / unavailable |
| M2 | 0.6434 x 0.7240; (0.3217, 0.3620); 0.8888 | 0.5539 x 0.5544; (0.2769, 0.2772); 0.9990 | Not comparable / unavailable |
| P1 | 0.6287 x 0.5852; (0.3217, 0.2926); 1.0743 | 0.5539 x 0.5544; (0.2769, 0.2772); 0.9990 | Not comparable / unavailable |
| P2 | 0.1730 x 0.0756; (1.0203, 0.3329); 2.2887 | UNKNOWN | Not comparable / unavailable |
| E1 | UNKNOWN | 0.1722 x 0.4539; (0.6689, 0.2269); 0.3794 | Not comparable / unavailable |
| E2 | UNKNOWN | 0.1161 x 0.2672; (0.6703, 0.2097); 0.4345 | Not comparable / unavailable |
| S1 | UNKNOWN | 0.1689 x 0.2078; (0.9283, 0.2600); 0.8128 | Not comparable / unavailable |


Only O1/O2/O3 have supported equivalent rectangular-area arithmetic under the
provisional match/frame. IoU = intersection area / union area. It is NOT photo
accuracy, whole-facade similarity, volume overlap, manufacturing suitability or
proof of which design is correct. Roof/mass area overlap is not reported because
fascia band versus filled profile and visibility scopes are not equivalent.

## IDs, evidence and confidence

| ID | Architectural candidate | Studio snapshot evidence | AUE snapshot evidence | Correspondence confidence / limit |
| --- | --- | --- | --- | --- |
| R1 | Main roof | Cut-Extrude33[1] selected fascia loops; Split4[1]/[2] contextual prior vertices | Main gable body outer f6/l1 | Moderate 0.6; bands vs filled envelope, roof intersections unresolved |
| R2 | Left roof | Cut-Extrude31 selected fascia loops; Split4[3]/[4] contextual | Left gable f3/l0 | Moderate 0.6; simplified profile, no full hidden roof |
| O1 | Large left opening | Boss-Extrude67[1]/f1/l0 | Bay f4/l0 AND backing-wall f4/l0 | Moderate 0.6; one architectural opening spans two pilot bodies |
| O2 | Right opening | Combine9/f469/l2 | Backing wall f4/l1 | Moderate 0.6; backing square; front loop f10/l0 has six edges and a bottom step |
| O3 | Upper opening | Combine9/f469/l0 | Main gable f6/l0 | Moderate 0.6; source grilles not traversed; photo presence user-supported |
| M1 | Main facade mass | Combine9 / Extrude-Thin1 | MainWall with independent cuts | Moderate role, low bbox comparability: roof/backing decomposition differs |
| M2 | Left bay support | Boss-Extrude67[1], Boss-Extrude1, Extrude-Thin4 | base + merged upper-support features | Moderate 0.6; many-to-many features; source also contains upper profile |
| P1 | Bay projection | Bay selected Z=0 planar faces | Bay Z=14 faces | Low depth-match confidence; semantic datum is provisional |
| P2 | Right-opening related projection | Extrude-Thin3/f8,f9 | No isolated counterpart | Low 0.2; role unknown, do not call it a sill/frame automatically |
| E1 | Entrance recess | Not isolated; projected panel notch alone insufficient | Recess f13/l1 | Low 0.2; source UNKNOWN |
| E2 | Door opening | Not isolated | Recess f13/l0 | Low 0.2; source UNKNOWN |
| S1 | OpeningSurround | Not isolated as equivalent component | Surround f0 outer/inner loops | Low 0.2; source UNKNOWN, may be distributed across features |

Face/loop indices and body labels are **snapshot references**, qualified by JSON
hash and configuration; not persistent identity across editing sessions. Match
graph supports one-to-many and many-to-one explicitly; bodies need not correspond
one-to-one to architectural components. Refer to the JSON for exact names.
Measured edge coordinates have high geometric confidence in this open snapshot;
semantic interpretation and selected datums do not inherit that confidence.
Confidence numbers are qualitative review scores, not calibrated probabilities.

## Depth and projections

Within R2, selected planes give entrance recess 6 mm (Z=6 to 0), bay projection
8 mm (6 to 14), surround projection 3 mm (6 to 9). Native surround outer 30.4 x
37.4 and inner 23.4 x 30.4 give four 3.5 mm borders. These are model observations,
not reconstructed real-building dimensions.

Source named planar-face gaps: main f10 at -47.652068 to bay faces at 0 gives
47.652068 mm; Extrude-Thin3 faces at -47.652068 and -39.652068 are 8 mm apart.
These are reliable distances between these specific CAD planes. Whether they
represent equivalent facade projection/recess datums is **unverified**, so no
cross-model architectural depth error or depth ratio is certified. Original
entrance depth and local surround borders/projection remain UNKNOWN.

## Photographic evidence and provisional diagnoses

Re-viewed accessible original annotated oblique photo:
`../../references/annotated_building_boundary.png.png` (943 x 843; hash in manifest).
Visible front-facing glazed mass roughly image x=440..665,y=310..515 supports
large glazing and depth ordering qualitatively. Roof edges are visible, while
vegetation/perspective obscure entry, base and portions of right facade. Purple-X
side region is excluded. The 180 annotation is a user target, not image calibration.
These pixel regions are approximate visual references, not a metric reconstruction.
The two generated elevation/depth PNGs are interpretations, not independent photos.
Additional original front photographs reported by user are not present/registered.

| Finding | Provisional classification | Rationale / evidence needed |
| --- | --- | --- |
| R1/R2 detailed bands/slopes vs shallow filled gables | Intentional modeling simplification | R2 explicitly approved simplified front profiles. Exact photo outline still unresolved |
| O1 relative width/height differ | Both plausible | CAD measures disagree; oblique photo does not establish calibrated frontal size |
| O2 square vs tall rectangle | Both plausible | Strong aspect-ratio difference, but not proof of AUE error or studio correctness |
| O3 upper opening width differs | Insufficient evidence | User supports presence; direct matched front photo needed to adjudicate proportions |
| M1 thin panels vs thick backing | Intentional modeling simplification | Different blockout/manufacturing approaches, not a facade-accuracy score |
| M2 support/bay proportions | Both plausible | Occluded support/ground and different component partitions |
| E1/E2/S1/P1/P2 | Insufficient evidence | Missing semantic/plane/component matches; no invented deviations |

**No Likely AUE Error or Likely Original Discrepancy is asserted on current photo
evidence. All classifications remain provisional/unverified.** Both categories are
supported by the learning contract for future evidence; neither is a required quota.

## Blockers and next step

Completed partial views and supported opening overlay, not full occlusion-resolved
elevations or complete component/depth match. Need confirmation of facade datums,
E1/E2/S1/P2 roles and a registered frontal photo/calibrated camera to adjudicate
architectural faithfulness. Original was dirty=True; comparisons target its open
state, not proven saved-file geometry. Source was not saved, rebuilt or activated.

Next controlled action is [evidence/correspondence verification](CONTROLLED_EXPERIMENT.md),
not automatically changing R2 to resemble the studio model. All recipes stay Draft.
