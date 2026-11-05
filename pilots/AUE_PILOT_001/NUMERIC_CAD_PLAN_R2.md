# Pilot #001 — revised numeric CAD proposal R2

2026-11-05. **FOR FINAL USER APPROVAL; NO CAD EXECUTION AUTHORIZED.**
Supersedes R1 and the recommendation to suppress the upper window. Design and
numeric plan only: no pilot executor implemented, no Builder/geometry validation.
All numerical geometry is **ESTIMATED / PROVISIONAL** except the user-selected
180 mm architectural-height target. Units are actual miniature mm, not survey mm.

## Evidence and correction of the R1 review

All three locally available references were re-opened and inspected:

| Reference under references/ | Kind / SHA-256 |
| --- | --- |
| annotated_building_boundary.png.png | Original annotated oblique photograph; CE1161EF4E262B35B73D6E72C2F94EFAE28592C9DF9952E99A1B39A65C3C017B |
| architectural_facade_elevation_drawing.png | Generated elevation; 12747DDF2BA3800435CAC18CAB8D0DC584E1B05051B30C3229FD528B6D1EFBB4 |
| architectural_facade_depth_analysis.png | Generated relative-depth concept; BB0443A4AC9B4CE403058EB95FD0CCA0399508AC2DD765DFCD533225AEC0CCB2 |

Repository/pilot inventory and targeted filename discovery in Downloads, Desktop,
Pictures and Documents found no additional identified original facade photographs.
This is not proof that none exist elsewhere; their paths were requested. No claim
is made to have inspected originals not actually available in this workspace.

**User clarification is evidence:** the upper gable window is visible in original
front-facing photographs. Retain it as a provisional component with provenance
'user-reported original-facade-photo support; direct source review pending'. Do
not reclassify it as an invented feature or omit it because a different oblique
photo obscures it. Presence support is distinct from dimensional verification.
R1 review's recommendation to suppress it is superseded, not carried into R2.

The annotated photo supports the projecting glazed bay, substantial roof edges and
some depth ordering; vegetation hides base/entry details and perspective prevents
metric proportion checks. The generated references agree on component arrangement
but differ by roughly 6% in facade width/height. R2 provisionally uses elevation XY
and relative-depth ordering, never generated artwork as unquestionable truth.

Purple-X region, geometry behind the user-marked cutoff, rear/side extensions,
neighboring buildings, fences, landscape, satellite dish, antennas and unrelated
additions are excluded. Finite boss side faces are unavoidable section faces, not
side-elevation reconstruction. The marked cutoff is a scope boundary, not a
calibrated thickness. No extra photo is demanded for omitted rear/side geometry.

## Dimensions and positions — ESTIMATED unless explicitly locked

Retain R1 uniform scale: elevation apex pixel (707,64), baseline y=987,
wall-left x=98; s=180/923=0.1950162514 mm/pixel.
X=(u-98)*s, Y=(987-v)*s, rounded to 0.1 mm for CAD inputs.
Approximately +/-3 px (+/-0.6 mm) describes clear-edge manual picking only, not
physical accuracy; obscured regions and AI interpretation have larger/unquantified
uncertainty. These are exact proposed input numbers, not precise measurements.
Frame: X right, Y up, exterior facade Z=0, inward negative Z.

| Component | X extent | Y extent | Z / size |
| --- | --- | --- | --- |
| MainWall | 0..224.5 | 0..99.8 | -12..0; 12 backing thickness |
| LeftBayBase | 0..99.7 | 0..27.7 | 0..8; solid supporting wall below glazing |
| LeftBayUpperSupport | 0..99.7 | 27.7..99.8 | 0..8; piers/header around opening, scoped merge into LeftBayBase only |
| LargeWindowOpening | 8.0..90.9 | 27.7..76.4 | 82.9 x 48.7; open through bay/support |
| EntranceRecess | 104.9..135.9 | 0..81.7 | 31 x 81.7; inward 6, leaving 6 support |
| DoorOpening | 110.2..131.1 | 13.7..61.8 | 20.9 x 48.1; through remaining support |
| RightWindowOpening | 155.4..178.8 | 31.6..62.0 | 23.4 x 30.4; through support |
| UpperGableWindowOpening | 108.6..157.4 | 104.3..139.4 | 48.8 x 35.1; through main gable, RETAINED |
| OpeningSurround inner | 155.4..178.8 | 31.6..62.0 | Aligned with right opening |
| OpeningSurround outer | 151.9..182.3 | 28.1..65.5 | 30.4 x 37.4; separate ring, Z=0..3 |

Left base is architectural support, not a display plinth. It remains continuous
under the large opening. Upper support has left pier width 8.0, right pier 8.8,
and header height 23.4, each named/editable. No glass pane, mullions or side-return
window is added in this first opening-based blockout. No floating unsupported
frame: base, piers and header explicitly form one connected left-bay solid.
Base exact hidden profile remains estimated; do not treat fencing/stone site wall
as building support evidence. Ground/base datum remains provisional due occlusion.

Surround independent top/bottom/left/right border widths = 3.5 each; projection
=3.0 independently. Draft border interpretation ~9 px=1.76 mm makes 3.5 approximately
2x, per user visual guidance, not physical measurement. Outer size derives from
inner size plus borders. No independent sill or extra steps/door leaf in this pass.
Door threshold at Y=13.7 follows provisional elevation geometry, not a measured step.
12 backing, +8 bay, -6 recess and +3 surround depths are deliberate visual-test
assumptions. Minimum printable thickness/profile has not been qualified; no print claim.

## Simplified roof profiles — ESTIMATED; intersections PROVISIONAL

Filled front silhouette envelopes, no tiles, long roof slopes into building depth
or hidden roof extensions. Proposed counterclockwise XY polygon vertices:

- MainGableProfile: (99.7,99.8), (232.5,99.8), (118.8,180.0),
  (48.4,131.1), (99.7,104.0), Z=-12..0.
- LeftGableProfile: (-10.5,99.8), (105.7,99.8), (49.5,131.1), Z=0..8.

Profiles deliberately simplify fascia, secondary eave slope and the intersection
near X=48.4..105.7, Y=99.8..131.1. They are approved-direction approximations for
review, not reconstructed roof topology. Retain independent named vertex/ridge/
eave parameters. No complex union or invented intersection seam. The main and
left roofs remain separate solids and their contact is not automatic merge consent.

Whole envelope X=-10.5..232.5 (243.0 wide), Y=0..180 (**180 LOCKED**),
Z=-12..8 (20 relief depth). Wall width 224.5; wall/eave height 99.8; lower ridge
131.1, main ridge 180. No base/accessory contributes to the height. Parameter
changes must preserve this height or report a conflict, not silently rescale.

## Exact intended feature operations / document scope

One new unsaved Part, description AUE001_Front_R2_PROVISIONAL; record actual title.
Expected five solids: MainWall, LeftBaySupport (base+upper support), MainGable,
LeftGable, OpeningSurround. This is expected topology, not an actual CAD count.
No studio/previous pilot documents edited, saved, closed or used as targets.

1. After explicit numeric approval, implement separate opt-in .NET Framework 4.8
   STA pilot/test path; no validated production implementations, signatures or
   default program flow changed. Build/preflight before any CAD execution.
2. Use unchanged connector to attach to already-running SolidWorks; no auto-launch.
   Unchanged CreateWall(224.5,99.8,12) creates new unsaved Part. Frame adapter:
   model mm = (X-112.25,Y-49.9,Z+6); convert to meters at COM boundary.
3. Unchanged CreateWindow(23.4,30.4,54.85,-3.1) cuts right opening before other bodies
   exist. Keep original feature names; fixed-name API is not called repeatedly.
4. Pilot EntranceRecessSketch -> inward pocket 6, selected MainWall only.
   Pilot DoorOpeningSketch -> cut from Z=-6 inward 6.2, MainWall only.
   Pilot LargeWindowSketch -> cut from Z=0 inward 12.2, MainWall only.
   The additional 0.2 is end-depth tolerance, not an opening size increase.
5. Pilot LeftBayBaseSketch -> boss from Z=0 outward 8, merge=false.
   Pilot LeftBayUpperSupportSketch -> U-shaped upper support from Z=0 outward 8,
   explicitly merge into selected LeftBayBase ONLY, not touching MainWall.
   U polygon: (0,27.7),(8,27.7),(8,76.4),(90.9,76.4),(90.9,27.7),
   (99.7,27.7),(99.7,99.8),(0,99.8). This builds the front opening directly,
   matching MainWall's already cut aperture; no further multi-body window cut.
   Explicit within-bay merge is necessary for support connectivity; no broad merge.
6. Pilot MainGableSketch and LeftGableSketch -> separate bosses with above profiles,
   merge=false. UpperGableWindowSketch -> inward cut 12.2 in MainGable ONLY.
7. Pilot SurroundSketch -> constrained nested-rectangle boss outward 3, merge=false;
   inner dimensions reference right opening and outer dimensions derive from four
   named border parameters. OpeningSurround remains Draft, no library promotion.
8. Rebuild after each step, preserve failure part for inspection, restore preferences
   and sketch settings in finally blocks. Do not save/close or mutate other documents.

R1-to-R2 numerical silhouette/opening/depth inputs are unchanged because no new
calibrated measurements justify altering them. R2 changes evidence classification,
explicitly retains upper window, and splits left support into base and editable
piers/header with a narrow internal merge. This is a substantial feature-plan
clarification, not a claim that R1 lacked all support after its rectangular cut.

## Planned verification and unresolved conflicts

Before CAD: isolated path build, pure parameter/host/containment/profile checks and
applicable non-COM regressions; no automatic manual CAD harness run. Expected support
volumes and polygons can be checked analytically before COM. Historical builder
validation does not validate these new dimensions or combinations.
After approval/run: actual features/dimensions, five positive-volume solids,
rebuild success, entire architectural height 180 +/-0.05 mm, individual bounds/depths
+/-0.05 mm, three complete window apertures, door/recess and non-target-body isolation.
Verify connected left base/piers/header and no accidental merge into main facade;
large opening clear through both aligned layers, base remains intact. Independently
edit base height 27.7->28.7 and move opening bottom with it (fixed head 76.4),
bay projection 8->10, upper width 48.8->49.8 (fixed center), right width 23.4->24.4
with linked surround, border 3.5->4 and projection 3->4; restore baseline and rebuild.
Unsupported edits must fail explicitly, never leave a claimed successful baseline.

Unresolved: directly inspecting the additional original front photos (user reports
upper-window support); generated-view proportional discrepancy; exact roof joins,
obscured base/entry geometry, dimensional calibration, depth and printer profile.
These are transparent visual-test assumptions, not reasons to omit the upper
window or expand scope. Additional sources may require a numbered R3 before CAD.
No revised geometry may silently replace numeric inputs after approval.

## Actual checks this session

Three local references re-viewed; local/source discovery and documentation/Builder
inspection only. Numeric R2 consistency checks recorded in ROADMAP, not CAD tests.
No production/pilot implementation change, build, regression suite, SolidWorks/COM,
existing studio-file access/edit or Git operation. Runtime architecture unchanged.
**Next: user review and explicit approval of R2 numeric plan and stated assumptions
before any SolidWorks control; register additional originals if supplied.**

## One-test approval and execution outcome — 2026-11-05

User approved ONE experimental construction test, not architectural accuracy. It stopped at a pilot body-identity check after wall/right window creation. See [actual result](results/R2_CAD_RESULT.md). Part1 remains unsaved/open; 180 mm height and remaining features were NOT achieved. No retry is authorized by this record. Upper window remains in the plan, not yet built. User stopped Computer Use during capture recovery.
