# AUE Pilot #001 — front facade CAD preflight

Status: PREPARATION ONLY; reference verification incomplete; CAD approval not yet
requested or granted. No executable pilot builder created or CAD accessed.
Current request is FRONT FACADE ONLY, superseding broader view deliverables for
this test; rear/side elevations, site and accessories are excluded.

## Source gate

Repository inventory contains no source images or approved elevation draft.
Filename searches in local Downloads, Desktop, Pictures and Documents did not
identify the references. Selected generically named generated images inspected
were unrelated. This is not proof that the files are absent elsewhere. User local
paths are required. Prior conversation images are not available in this turn.
Register exact source/draft revisions and hashes and visually verify identity
before numerical proposal or implementation. Do not use unrelated search images.

PHOTO_BLOCKOUT.md records an older 125 mm-high assumed concept, deep centered
prisms and unconstrained sketches. Its earlier visual massing approval does not
establish the current approved elevation's proportions. Do not run that command
as this requested 180 mm dimension-driven facade test.

## Dimension proposal pending reference

| Quantity | Proposed rule / status |
| --- | --- |
| Architectural height | 180 miniature mm; user design constraint, lowest architecture to highest roof; no base |
| Front width, wall/eave levels, ridge positions, bay widths | Derive from approved elevation using s=180/(highest roof ordinate-lowest architectural ordinate); numeric values pending |
| Large/right window and entrance extents/positions | Same uniform scale and registered origin; numeric values pending |
| Right surround border widths | Approximately 2 times initial draft border width after uniform scaling; latest user request resolves prior thickness ambiguity as border width |
| Surround projection/section | Independent provisional values to propose after photo review; do not double projection implicitly |
| Wall thickness, facade support depth, left projection, entrance recess | Unverified; explicit provisional actual-mm values required before CAD approval; no whole-building depth |
| Gable section thickness and relief depth | Pilot manufacturing assumptions, not observed dimensions; numeric values pending |

No numeric proportion is invented without the approved draft. The 2x border
correction remains user visual guidance pending CAD validation, not measurement.
Independent edge overrides and inner/outer consistency must be retained. No sill
is added unless the reference and approved proposal call for it.

## Proposed feature tree and construction order

Names below are proposed logical names, not reports of actual features.

1. Pilot001_Parameters: named dimensions/equations, front origin and 180 mm lock.
2. WallSketch -> MainWall: dimension-driven facade support through unchanged
   CreateWall where the verified outline is compatible. Respect centered-wall
   origin via an explicit coordinate adapter; never include unsupported upper
   rectangles behind sloping roof outlines.
3. Pilot001_RightWindowSketch -> right WindowCut: unchanged CreateWindow if host
   and containment permit. Perform compatible legacy cuts before additional bodies.
4. Pilot001_EntranceSketch -> EntranceRecess: pilot-only controlled-depth feature
   scoped to facade body, with retained recessed portal region for the door.
5. Pilot001_DoorSketch -> DoorOpening: scoped aperture in recessed portal. Existing
   ground-door API applies only if its original face/depth/bottom semantics hold;
   otherwise isolated pilot path, with no modification of validated methods.
6. Pilot001_LeftMassSketch -> LeftFrontProjection: independently dimensioned
   facade-only projecting body, explicit no-merge policy for this test.
7. Pilot001_LargeWindowSketch -> LargeWindowOpening: pilot scoped cut of the
   appropriate left/facade host only; exact ownership to resolve from reference.
8. Pilot001_MainGableSketch -> MainGableProfile and
   Pilot001_LeftGableSketch -> LeftGableProfile: constrained pilot-only simplified
   front roof silhouettes with finite section/relief, separate unmerged bodies,
   no tiles or rear roof extension. Final count depends on verified draft.
9. Pilot001_SurroundSketch -> OpeningSurround: independent pilot rectangular-ring
   boss with named inner dimensions, four border widths and outward projection;
   separate unmerged body, linked to right opening. Remains Draft.

If the reference requires different body ownership/order, revise this proposal
before CAD approval. Every cut selects explicit target bodies; no automatic
all-body cuts or contact-based merges. Planned body count with two gable bodies:
5 (facade, left projection, two roof profiles, surround), contingent on reference
and successful topology. Recess/door cuts do not add bodies. This is not an actual
count or a physical-print connectivity claim.

## Builder compatibility and isolation

Source inspected: Program, csproj, SolidWorksModeler wall/opening entry points,
photo blockout, ProjectingBalconySlab contract and recorded scoped evidence.
Existing standalone CreateWindow uses fixed feature names and cannot be invoked
twice unchanged in one wall. Do not rename features to bypass bookkeeping or assume
multi-body cut safety. Reuse one compatible unchanged opening; other targets use
isolated pilot operations. Balcony slab/assembly definitions are not applicable
facade/roof substitutes and their Validated status does not validate this model.
Use a separate opt-in pilot executable/project or isolated pilot-only path with
.NET Framework 4.8, explicit Compile entries and STA. Do not change existing
validated production implementations or use the default demo command.

## Approval and execution gate

After source verification: finish numeric parameter/body/operation proposal,
prepare isolated implementation and non-COM checks/build, then present the exact
plan for explicit user approval. The user explicitly requires a stop before any
SolidWorks control, including COM preflight. No automatic start/launch.

After approval: attach to the already-running application and create a new unsaved
part, operate only on that returned document, leave failures open and preserve
existing studio documents without save/close/edit. Restore changed preferences
and sketch settings in finally blocks. Record exact command, environment and
actual feature names/types, bodies, volume/bounds, dimensions and failures.
Validate rebuilds after each operation, height=180 under declared tolerance,
roof maxima, aperture sizes/positions, recess/projection signs and depths, target
body isolation and named driving dimensions. Exercise independent parameter edits
and restore/rebuild baseline; do not claim validation from initial creation alone.
No production promotion, physical-print claim or Git operations are authorized.

## Verification this preparation session

Documentation/source and local asset discovery only. No build, regression tests,
CAD execution, new part, geometry measurements or parameter-edit checks run.
Blocking input: local approved draft and source-photo paths. Next: inspect/register
those files and replace pending quantities with a reviewable numeric proposal.

## Numeric proposal update — 2026-11-05

Elevation/depth PNGs are now available and inspected. See [numeric plan R1](AUE_PILOT_001/NUMERIC_CAD_PLAN_R1.md) for provisional dimensions, hashes, comparisons and exact proposed operations. Annotated photo remains missing; original-photo cross-check and explicit numeric CAD approval are pending. Earlier missing-all-assets statements above describe the prior session. No SolidWorks execution.
