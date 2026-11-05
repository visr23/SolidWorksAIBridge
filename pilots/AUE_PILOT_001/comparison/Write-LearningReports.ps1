$ErrorActionPreference='Stop'
$out=Join-Path $PSScriptRoot 'learning'
$openings=@(Get-Content (Join-Path $out 'OPENING_COMPARISON.json') -Raw|ConvertFrom-Json)
$surfaces=@(Get-Content (Join-Path $out 'SURFACE_MEASUREMENTS.json') -Raw|ConvertFrom-Json)
function Number($value){return ([double]$value).ToString('0.0000',[cultureinfo]::InvariantCulture)}
function Measurement($value){if(!$value){return 'UNKNOWN'};return "$(Number $value.widthH) x $(Number $value.heightH); ($(Number $value.centerXH), $(Number $value.centerYH)); $(Number $value.aspect)"}
$table=[Text.StringBuilder]::new()
[void]$table.AppendLine('| ID | Studio W/H x H/H; center (X/H,Y/H); aspect | AUE W/H x H/H; center; aspect | Conditional IoU |')
[void]$table.AppendLine('| --- | --- | --- | --- |')
foreach($r in $openings){[void]$table.AppendLine("| $($r.id) | $(Measurement $r.studio) | $(Measurement $r.generated) | $(Number $r.intersectionOverUnion) |")}
foreach($r in $surfaces){[void]$table.AppendLine("| $($r.id) | $(Measurement $r.studio) | $(Measurement $r.generated) | Not comparable / unavailable |")}
$report=@'
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

TABLE

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
'@
$report=$report.Replace('TABLE',$table.ToString())
$report|Set-Content (Join-Path $out 'COMPONENT_REPORT.md') -Encoding UTF8
$recipes=@'
# First feature-recipe comparison — Draft

2026-11-05. Source CAD is reference evidence, not an authoritative target. This
report distinguishes measured result, observed feature dependency, source-code
intent and untested edit/manufacturing predictions. No recipe is promoted.

## Observed construction

Evidence is [selected native face owners and direct parents](FRONT_EDGE_EVIDENCE.json).
GetParents is direct only; no recursive dependency closure. Native feature
definitions/merge scope flags and all sketch relations were NOT accessed. Named
sketch parents prove dependency links, not full constraint/driving status.
See [official GetParents contract](https://help.solidworks.com/2023/English/api/sldworksapi/SolidWorks.Interop.sldworks~SolidWorks.Interop.sldworks.IFeature~GetParents.html).

| Component | Studio observed construction | Generated R2 construction | What can be learned now |
| --- | --- | --- | --- |
| Main facade / O2 / O3 | Extrude-Thin1 (BossThin) owns front/backing faces; parents Boss-Extrude1, Sketch3. Combine9 (CombineBodies) parents Extrude-Thin12, Split1. Multiple architectural roles share Combine9 | MainWall extrusion; separate WindowCut and three pilot facade cuts. Each has named sketch/host parents | Thin panel vs solid backing + scoped cuts are competing approaches; operation-count comparison is not equivalent without matching scope |
| Left bay / O1 | Boss-Extrude1 (Extrusion) owns support faces, parents Front Plane, Sketch1, Sketch2. Boss-Extrude67 depends on Boss-Extrude1/6/23/24 and Sketch129 | Base boss then U-shaped upper-support boss; native upper-support parents include base and upper sketch. One current bay body; backing opening is separate | One opening spans several features/bodies. Evaluate connected support, clear overlap and base edit semantics before choosing a reusable recipe |
| Main roof | Boss-Extrude17 owns 3 fascia faces; parents Boss-Extrude16, Sketch37. Cut-Extrude33 has direct parents including Boss-Extrude17/52, Cut-Extrude24/Thin5, Sketch116. Split4 is observed but its roles need matching | One named polygon sketch/boss, separate from wall; upper opening is a targeted cut | Source band detail and pilot filled envelope solve different geometric scopes. Simpler shallow profile is promising for blockout, not automatically superior for faithful roof reconstruction |
| Left roof | Boss-Extrude9 owns fascia faces; parents Boss-Extrude8, Sketch27. Cut-Extrude31 depends on roof bosses/cuts and Sketch113 | One triangle sketch/boss | Prefer parameterized silhouette controls for future experiments; do not remove source detail based only on complexity |
| Cornice / right-related projection | Extrude-Thin4 (BossThin) parents source bosses/cuts/Extrude-Thin1/Sketch25; Extrude-Thin3 parents Cut-Extrude7, Extrude-Thin1, Sketch17 | Separate ring boss for OpeningSurround; no independent first-pass sill | Source strips/projection are not yet the same semantic object as S1. Do not call these interchangeable recipes |
| Repetition / scale | Mirror1 is MirrorPattern with LPattern1 and Plane3 parents; Scale1 depends on Boss-Extrude66 | No mirror/pattern needed for three unique first-pass apertures | These are contextual source observations; grille bodies were NOT traversed. Scale1 means pre-scale sketch numbers must not be treated as final dimensions |

Pilot geometry/merge policy also has source-code and successful baseline evidence:
R2 bay upper support merges only into bay base; roofs/surround remain separate,
cuts have explicit targets; relevant non-target invariants passed historically.
Fresh read confirms current body/face ownership and direct parent links, **not** a
new merge/edit experiment. Studio exact historical merge/cut/pattern scopes remain
unknown because selected feature definitions were not read.

## Evaluation, without premature ranking

- **Geometric outcome:** O1/O2/O3 host-panel boundaries are measurable; depth,
  visible region, glass/shutters and full roof matching incomplete. Similarity is
  not photo fidelity; do not optimize towards one CAD merely because it exists.
- **Efficiency:** R2's 11 known modeling features describe this blockout. The
  source's 303 historical tree/subfeature entries include 133 sketches and folders;
  they are NOT 303 modeling operations. No valid total construction-efficiency
  score exists for these unequal component/detail scopes. Local native dependency
  lists support identifying candidate simplifications, not asserting time savings.
- **Parametric editability:** R2 has named dimensions/equations in source and a
  passed baseline. Independent edits/restoration NOT RUN. Studio sketches and
  direct dependencies observed, full constraints/equations and edits NOT RUN.
  Neither path is proven more editable.
- **Rebuild reliability:** NeedsRebuild2=0 in both open snapshots is a status read,
  not an actual successful fresh rebuild. R2 historical construction rebuild passed;
  studio original never rebuilt in this workflow. No robustness ranking.
- **Manufacturing:** source thin walls, split bodies, details and R2 thick backing
  indicate different intent. No printer profile, minimum-section/contact/fit audit
  or physical print qualification. 48 vs 5 bodies is not a quality metric.
- **Reuse:** candidate planar profile, host-scoped aperture, connected bay support
  and nested-profile surround are Draft recipe concepts. Existing validated slab,
  balcony paths and LEGO definitions stay unchanged. Surround remains Draft.

## Draft recommendations

1. Store architectural role independently from body/feature identity; permit a
   body to support several roles and a role to span several bodies.
2. Keep aperture, pane, shutter, surround and sill evidence separate. Do not tune
   an aperture to match a pane/shutter outline by accident.
3. Learn shared-geometry-first only within a matched role/detail/manufacturing scope.
   Prototype shared profile or host-cut alternatives, retaining separate named
   parameters and exact body targeting; do not refactor validated builders.
4. Extract local evidence progressively with budgets/checkpoints; detailed grille
   traversal/mass properties are unnecessary for the current facade task.
5. Preserve competing hypotheses and photographic uncertainty. Do not encode the
   source's square O2, or the AUE tall O2, as a learned rule without new evidence.

Machine-readable feature observations/parents are preserved as Draft recipe
records in [learning JSON](PILOT_LEARNING_RECORD.json). No new recipe execution,
library registration, family promotion or production modification occurred.
'@
$recipes|Set-Content (Join-Path $out 'RECIPE_REPORT.md') -Encoding UTF8
$experiment=@'
# Proposed controlled experiment — FRONT-CORRESPONDENCE-001

**Draft / Pending. No CAD modeling experiment authorized or executed here.**

Question: can we reliably match clear host-panel openings, their glazing/shutters,
surrounds and roof datums without preferring either CAD interpretation?

## MEASURE

Freeze hashes of current extracted JSON/photographs and record both configurations,
open-state dirty flags and code revision/digests. Keep baseline R2 at 180 mm.
Record native loop references, coordinate units, selected height/anchor, confidence
and missing values. Current Studio is dirty=True; do not save it or claim its
open snapshot equals disk. Views and observations remain in separate report files.

## DIAGNOSE

Review R1/R2/O1/O2/O3/E1/E2/S1/P1/P2 role annotations against accessible photos.
Confirm which original face is architectural exterior and which loops are host
openings versus pane/shutter/frame boundaries. Register a front photo or calibrated
view if available; if unavailable, keep both dimension hypotheses plausible.
Do not declare an AUE/original error merely from inter-CAD disagreement.

## COMPARE RECIPES

Freeze two explicit candidates for ONE component/detail scope: existing R2
nested-profile ring versus a planned four-border-profile ring alternative.
Do not substitute the source's whole thin-panel/roof/grille pipeline as an
equivalent one-window fixture. Require target bodies, merge policy, editable inner
width/height, independent border/projection and dimensional dependencies.

## PROPOSE

Prepare a non-CAD rectangle/loop fixture first: same opening dimensions and host
frame, both small and large aspect ratios, border/projection cases and both height
scales. Pure report/registration improvements are permitted now. Any actual CAD
recipe construction requires a separately authorized new disposable Part; neither
original nor R2 is a test target. No existing production builder is replaced.

## TEST IN ISOLATION

Run deterministic geometry/registration/record tests, fail on ambiguous/missing
roles, invalid/curved/open boundary evidence, stale frames or wrong cut/merge scope.
For a future explicitly approved CAD fixture, measure dimensions, connectivity,
isolation, rebuilds, independent edits and restoration; stop at first failure.
Log fixture ID and actual results. Pending or unrelated evidence cannot count as a
successful experiment. This task did NOT execute that CAD fixture.

## RE-MEASURE

Compare exact same component metrics, same units/transform/photo camera and
tolerances; include outside-target invariants and source preservation evidence.
Use measured loop/area overlap where equivalent, not a vertex cloud or raw entry
count. Track interpretation accuracy separately from construction efficiency.

## ACCEPT / REJECT

Accept only with positive isolated-test evidence, remeasurement tied to that
fixture, passing applicable regression evidence and explicit human approval.
Reject or remain Pending when evidence fails/is missing. Learning acceptance does
not promote a LEGO family; a separate exact-version validation/review process is
still required. Current recipes/proposals remain Draft and experiment Pending.

## Concrete next implementation step

Add a small correspondence-review manifest that confirms/rejects facade datums,
host-panel vs pane/frame role and the E1/E2/S1/P2 source entities; validate it with
synthetic many-to-many and ambiguity fixtures. Then read only those confirmed
faces/definitions under the existing bounded extractor. No expensive broad source
traversal or automatic geometry correction. Missing frontal photo/camera evidence
is a blocker to assigning architectural blame, not to improving evidence storage.
'@
$experiment|Set-Content (Join-Path $out 'CONTROLLED_EXPERIMENT.md') -Encoding UTF8
Write-Output 'COMPONENT_RECIPE_AND_EXPERIMENT_REPORTS_SAVED'
