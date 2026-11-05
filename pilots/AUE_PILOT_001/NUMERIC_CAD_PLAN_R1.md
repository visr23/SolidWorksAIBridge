# Pilot #001 — provisional numeric CAD plan R1

2026-11-05. **PROPOSED; NOT CAD-APPROVED; NO SOLIDWORKS EXECUTION.**
Front facade only, new unsaved visual test part. All dimensions below are actual
miniature mm, inferred/design-provisional except the user-specified 180 mm height.
This supersedes the pending numeric and five-body sketch in the older preflight
for this proposed run only. No executable implementation is added in this session.

## Registered local references and missing evidence

| File under references/ | Pixels | SHA-256 |
| --- | --- | --- |
| architectural_facade_elevation_drawing.png | 1448 x 1086 | 12747DDF2BA3800435CAC18CAB8D0DC584E1B05051B30C3229FD528B6D1EFBB4 |
| architectural_facade_depth_analysis.png | 1122 x 1402 | BB0443A4AC9B4CE403058EB95FD0CCA0399508AC2DD765DFCD533225AEC0CCB2 |

Both files were opened and visually inspected. The annotated original photograph
is **missing from this directory and its pilot subtree inventory**. Its path was
requested. Thus source identity/visible relationships and marked cutoff cannot be
cross-checked against the photograph yet. Do not report either generated image as
observed ground truth or execute based solely on this plan without that review
and explicit numeric approval. No source CAD was opened.

The two images agree qualitatively on lower left gable/projecting bay, large lower
left window, central entrance recess, right window, taller main gable and upper
window. The elevation baseline-to-peak is approximately 923 pixels, facade width
1151 pixels. Depth diagram baseline-to-peak is approximately 761 pixels and facade
width 890 pixels: width/height about 1.17 versus elevation 1.25 (roughly 6% narrower).
They are not interchangeable orthographic authorities. Use the elevation as this
proposal's provisional XY reference, depth image only as qualitative ordering.
Its shading/legend establishes no numeric depths, and roof planes are ambiguous.

## Scale, trace and uncertainty

Manual approximate elevation picks: highest roof (707,64), architectural baseline
y=987, wall left x=98, wall right x=1249. Exclude arrows, text and long ground line.
Use s=180/(987-64)=0.1950162514 mm/pixel:
X=(u-98)*s; Y=(987-v)*s. Round proposed CAD inputs to 0.1 mm while retaining these
picks. Image sampling uncertainty is approximately +/-3 pixels (+/-0.6 mm) for
clear edges, larger for roof joins and ambiguous border linework; this is a manual
sampling allowance, not a statistical confidence interval or physical accuracy.
AI interpretation uncertainty remains unquantified until photograph review.

Large opening inner picks approximately (139,595)-(564,845); right opening
(895,669)-(1015,825); door (663,670)-(770,917); upper opening
(655,272)-(905,452). Right draft border is interpreted as about 9 pixels (1.76 mm),
not independently measured. Proposed border is 3.5 mm, approximately twice that
interpretation. Do not double projection, aperture size or global scale.

## Exact provisional inputs for approval

Global X right, Y up, Z outward. X=0 at main wall left, Y=0 at architectural bottom;
main facade exterior Z=0, support extends inward. Extents rather than centers:

| Element | X range | Y range | Z / operation |
| --- | --- | --- | --- |
| MainWall | 0..224.5 | 0..99.8 | -12..0; support thickness 12 |
| LeftFrontProjection | 0..99.7 | 0..99.8 | 0..8; separate solid |
| LargeWindowOpening | 8.0..90.9 | 27.7..76.4 | Through left projection AND support only |
| EntranceRecess | 104.9..135.9 | 0..81.7 | From Z=0 inward 6; leave 6 support thickness |
| DoorOpening | 110.2..131.1 | 13.7..61.8 | From recess floor Z=-6 through remaining support |
| RightWindowOpening | 155.4..178.8 | 31.6..62.0 | Through support only |
| UpperWindowOpening | 108.6..157.4 | 104.3..139.4 | Through main gable only |
| OpeningSurround inner | 155.4..178.8 | 31.6..62.0 | Aligned with right aperture |
| OpeningSurround outer | 151.9..182.3 | 28.1..65.5 | Z=0..3; separate ring solid |

Surround: inner 23.4 x 30.4, four independent borders each 3.5, derived outer
30.4 x 37.4, projection 3.0. No extra sill, glazing, mullions, door leaf or steps in
this first structural pass. Door bottom above zero preserves the drawing's raised
threshold level without adding stairs. The upper opening is included because it
is prominent in both supplied references; it must not disappear in the gable.

12 mm support, +8 mm left projection, -6 mm entrance recess and +3 mm surround are
explicit visual-test depth assumptions, not values extracted from colors. They
provide a visible recess without cutting through a thin wall. No whole-building
reconstruction depth/cutoff or printable minimum is asserted; rear is open/exposed.

Roof/gable simplification: filled front silhouette profiles of finite depth, no
tiles or pitched roof extending into unseen building depth. Proposed polygons,
vertices in counterclockwise XY order (mm):

- MainGableProfile: (99.7,99.8), (232.5,99.8), (118.8,180.0),
  (48.4,131.1), (99.7,104.0); Z=-12..0, separate solid.
- LeftGableProfile: (-10.5,99.8), (105.7,99.8), (49.5,131.1);
  Z=0..8, separate solid.

These deliberately simplify roof-edge/fascia joins and flatten the small secondary
eave slope. Overall proposed envelope X=-10.5..232.5 (243.0 wide), Y=0..180,
Z=-12..8 (20 total relief depth). Highest point remains exactly 180; roof overhangs
are included in width. Main wall + upper profile is not a rectangular wall extended
behind the roof slopes. Profile thickness here means Z depth; roof border/fascia
linework is omitted. No rear walls, side-elevation geometry or accessories.

## Exact intended operations and test document scope

One new unsaved Part only, suggested description AUE001_Front_R1_PROVISIONAL.
Actual automatic document title must be reported, not assumed. Five expected
unmerged solids: support, left projection, main gable, left gable, surround.
Cuts add no bodies. Surface contact between these solids is intentional; no merge
by contact or claim of a single printable unit. No old pilot/studio document edits.

1. Independent opt-in pilot/test path, .NET Framework 4.8/STA, no production builder
   changes. Non-COM preflight validates dimensions, containment, polygons and scopes.
2. Attach through existing connector only after approval; create MainWall via
   unchanged CreateWall(224.5,99.8,12). Existing centered COM coordinates map from
   proposed frame as Xcom=X-112.25, Ycom=Y-49.9, Zcom=Z+6 (all in mm before conversion).
   This creates the new Part; do not call CreateNewPart separately beforehand.
3. Unchanged CreateWindow(23.4,30.4,54.85,-3.1) creates the right opening in that
   sole support body, before other solids. Retain existing WallSketch/MainWall and
   WindowSketch/WindowCut names; do not bypass fixed naming to call it twice.
4. Pilot EntranceRecessSketch -> EntranceRecess: rectangular 31 x 81.7 pocket,
   inward depth 6, support selected explicitly. Pilot DoorSketch -> DoorOpening:
   20.9 x 48.1 aperture from recessed face through remaining 6 plus 0.2 tolerance,
   support only. Do not misuse unchanged ground-door semantics at raised threshold.
5. Pilot LeftMassSketch -> LeftFrontProjection: 99.7 x 99.8, outward 8, merge=false.
6. Pilot LargeWindowSketch -> LargeWindowOpening: 82.9 x 48.7, inward cut from
   Z=8 through 20 plus 0.2 tolerance, explicit two-body scope support+left projection.
   No other body exists in this region; validate both are actually perforated.
7. Pilot MainGableSketch -> MainGableProfile and LeftGableSketch -> LeftGableProfile:
   above polygons, dimension-driven vertex positions/constraints, separate bosses,
   merge=false. Pilot UpperWindowSketch -> UpperWindowOpening: 48.8 x 35.1,
   through main gable 12 plus 0.2 tolerance, main gable body only.
8. Pilot SurroundSketch -> OpeningSurround: two nested constrained rectangles,
   independent top/bottom/left/right border dimensions and named 3 mm outward boss,
   merge=false. Drive outer extents from inner extents and borders; no competing
   redundant dimensions. Keep OpeningSurround Draft, no library registration.
9. Rebuild after each feature; report actual dimensions, feature names/types,
   body count, bounds and volumes and any failure. Do not save/close documents.

Reusing wall/window code preserves its known behavior but does not extend historic
LEGO validation to this new host/dimension combination. Validated balcony builders
are not applicable substitutes for facade/roof/surround geometry. All unsupported
operations stay pilot-only; production signatures and implementations unchanged.

## Planned verification after explicit numeric approval

Before CAD: implement/build isolated path, run focused non-COM geometry/preflight
checks and applicable legacy regression build. No manual CAD harness auto-run.
After creation: successful rebuilds, five solids, positive individual volumes,
height 180 +/-0.05 mm, element bounds and depths +/-0.05 mm, three window apertures,
raised door aperture, 6 mm recess and unchanged non-target bodies after each cut.
Check parameter edits independently and restore baseline: surround border 3.5->4.0
(outer 31.4 x 38.4), projection 3->4, left projection 8->10 with dependent cut depth,
recess 6->5 with dependent door depth, right width 23.4->24.4 with dependent surround
outer width 31.4. Height lock stays 180; check rebuild/body scope after every edit.
No physical-print qualification or automated AUE accuracy claim.

## Actual verification this session

Both PNGs viewed, dimensions and SHA-256 obtained with PowerShell/System.Drawing;
scale and picked pixel spans calculated. Numeric consistency checks recorded in
ROADMAP. Original-photo comparison NOT RUN (missing file). No source-code changes,
builds, runtime regressions, SolidWorks control, CAD or Git operations performed.
Next: inspect missing annotated photo and reconcile discrepancies, then receive
explicit approval for this exact numeric CAD plan (or a revised numbered version).

Verification correction: initial PowerShell array-expression parsing failed before evaluating the spot checks; corrected independent expressions were rerun successfully (8/8). No CAD or runtime tests were run.


## Photo review update — 2026-11-05

Photo found under double extension and inspected. See [reconciliation report](PHOTO_RECONCILIATION_R1.md). R1 is NOT reconciled for final CAD approval: upper window unsupported, roof topology and obscured opening geometry unverified. Numeric inputs above remain an unchanged provisional candidate. No CAD authorized or run.


## Superseded by R2

See [R2 numeric proposal](NUMERIC_CAD_PLAN_R2.md): upper window retained under user clarification, left base/support made explicit, estimates and roof uncertainty preserved. R2 awaits final numeric CAD approval.
