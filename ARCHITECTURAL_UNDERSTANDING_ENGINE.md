# Architectural Understanding Engine (AUE V1)

Status: **approved architectural direction; design only**, 2026-11-04.
All nine AUE modules below are unimplemented. This specification creates no schema,
recognition service, shared-geometry engine, compiler, dataset or executable plan.
No AUE accuracy, CAD or manufacturing results have been verified.

## Purpose and system boundaries

AUE transforms photographs and optional video frames of real buildings into a
structured, geometrically consistent architectural interpretation. Human-assisted
interpretation is acceptable for the first MVP; full automatic reconstruction is
not a prerequisite.

```text
Photos / Video
 -> Architectural Understanding Engine
 -> Shared Architectural Geometry
 -> Orthographic Elevations / Optional Top View
 -> Depth and Relief Representation
 -> Validated Building JSON
 -> Architectural LEGO Planner
 -> Deterministic SolidWorks Builder
 -> Printable 3D Model
```

This is the intended product flow, not an implemented pipeline. Depth regions are
part of shared geometry; the flow's drawing and relief steps are derived outputs,
not raster-to-geometry dependencies. Every view must derive from the same geometry
revision. Independent AI-generated elevation images are never geometric authority.
Corrections change structured geometry/constraints first, then regenerate views.
Architectural sections are outside AUE V1.

AUE owns photographed-building interpretation. [Analyzer](BUILDING_DATASET_ANALYZER.md)
owns evidence comparison against studio CAD, photographs, recordings and reference
evidence, alongside its existing extraction/process-learning remit. The LEGO
Planner selects reusable definitions and construction recipes and resolves
manufacturing. The Builder executes supported deterministic CAD operations.
Neither AUE nor Analyzer issues modeling commands or promotes generated recipes.
Existing scoped Validated definitions and regression baselines remain unchanged.

## Adaptive model scope and view requirements

Record the selected scope, included facade/component IDs, omitted regions, intended
miniature use and decision rationale before requesting more evidence.

| Scope | Geometry required | View policy |
| --- | --- | --- |
| Facade Only | Main facade, openings, projections/recesses and limited supporting depth for printable structure | Main facade Required; rear Not Required; side/top Optional unless a specific structural ambiguity makes them Required |
| Partial Building | Main facade plus selected sides; plain railway/row/party walls allowed by evidence or explicit design approval | Front and selected side views Required where needed; rear may be Not Required; top depends on roof/depth needs |
| Full Building | All required facades, sides and roof geometry | Front/rear/left/right and top/roof plan Required for complete delivery; unavailable geometry remains uncertain, never fabricated |

Every front/rear/left/right elevation and top view has an explicit requirement:
**Required / Optional / Not Required / Unknown**, with reason and revision. This
is distinct from evidence availability and geometry confidence. Unknown means the
requirement is undecided, not permission to omit it. Required but missing produces
a partial/blocked result. Optional omissions are recorded; Not Required never means
the real building has no geometry there. Scope-specific closure/backing is a
manufacturing proposal, not observed rear architecture.

Request an additional view only when it can materially resolve a named ambiguity
or acceptance failure for the selected scope. Record the question, expected gain,
alternatives and cost; allow an unresolved or approved design assumption instead.
Do not demand a rear photo to complete a facade-only interpretation. Plain sides
must not gain invented windows. Compression must preserve important side features.

## Planned modules

| Module | Responsibility and output |
| --- | --- |
| 1. Image Intake and Source Registration | Register permitted photographs and optional video frames, source IDs/digests, revisions, frame timestamps/timebase, crops and image transforms; retain original evidence links |
| 2. Structural Segmentation | Identify envelope, walls, floors, roofs, entrances, bays, openings, balconies, stairs, recesses/projections; retain secondary details and occlusion masks separately |
| 3. Camera and Perspective Estimation | Estimate intrinsics/distortion, pose, vanishing directions and calibration constraints with uncertainty; record unsupported or ambiguous camera solutions |
| 4. Multi-View Geometry Reconstruction | Associate entities across viewpoints/frames; fit shared planes, corners, roof profiles, anchors and constraints; preserve alternatives and underconstrained regions |
| 5. Architectural Elevation Generator | Deterministically project shared geometry into required front/rear/left/right and top/roof views with entity links and projection transforms |
| 6. Adaptive View Planner | Maintain scope and requirement matrix; choose useful views/frames, identify missing evidence and request only material additions |
| 7. Depth / Relief and Opening Mapper | Attach signed depth regions and opening semantics to host planes; distinguish through openings, shallow pockets and unknown recesses |
| 8. Multi-View Consistency Validator | Check transforms, shared corners/datums, projected positions, topology/containment, depth signs, units, provenance and contradictions; produce passed/failed/incomplete checks |
| 9. Building JSON Compiler | Convert accepted interpretation to versioned architectural Building JSON; validate schema/semantics, retain evidence links and reject unresolved execution-critical quantities |

These are logical boundaries, not a requirement for nine services. The View Planner
runs before and during reconstruction. Segmentation, cameras, reconstruction and
depth mapping may iterate; validation never silently repairs conflicting evidence.
The compiler emits architectural intent and relationships; exact recipe selection,
definition locks, manufacturing resolution and executor capability checks belong
to the downstream Planner. Unsupported capabilities block execution there.

## Shared Architectural Geometry contract (planned)

Use one versioned structured representation, independent of drawings and CAD.
Minimum logical records (machine-readable schema and encoding remain future work):

| Record | Required meaning |
| --- | --- |
| Interpretation | Building/pilot ID, schema/revision, source IDs/digests, scope, tool versions, review decisions and unresolved issues |
| Frames | Global origin and axes; local component/facade frames, parent links and explicit invertible transforms |
| Entities | Stable IDs per revision, type, primary/secondary/context classification, host/parent/floor links, profiles/planes, positions and dependencies |
| Anchors and constraints | Shared facade corners, floor/roof reference elevations, sill/head lines, wall intersections/angles, alignment and cross-view correspondences |
| Quantities | Value or null, unit/domain, measured/inferred/estimated/approved-design classification, evidence, uncertainty interval or reason unavailable |
| Depth regions | Host-plane ID, local polygon/boundary, signed depth or unresolved range, units, reference normal, semantic operation and affected entity IDs |
| Views | Requirement and rationale, evidence availability, geometry revision, projection frame/transform, entity-to-line/region mapping and uncertainty annotations |
| Validation | Check IDs, input revision/digests, tolerances, actual residuals, outcome, missing evidence and reviewer decisions |

Adopt LEGO's right-handed **X_RIGHT_Y_UP_Z_OUT**: X right in the front elevation,
Y up, Z outward toward its viewer; building depth extends toward negative Z.
Declare a ground/main-facade origin for each interpretation; its exact surveyed
location is evidence-dependent. Local walls have outward normals and attachment
anchors consistent with [LEGO coordinates](ARCHITECTURAL_LEGO_V1.md). Do not treat
left/right images as mirrored coordinates; each projection declares its basis.
Share corner/anchor IDs rather than copying independent coordinates into views.
Store wall adjacency, angle/parallelism and intersection constraints explicitly;
do not assume every building is rectangular. Dependencies must resolve without cycles.

Unknown geometry uses null/alternatives with reasons. Human-approved design
assumptions remain assumptions, not observed facts. Confidence is separate from
review status, consistency, compiler eligibility and LEGO lifecycle. An apparently
consistent drawing is not proof of source truth.

## Drawings, depth and semantic layers

Generate only selected views as simplified black linework on white backgrounds,
emphasizing envelope, major openings, roof profiles, levels, projections and recesses.
Reduce internal window divisions during structural blockout. Every drawing identifies
the geometry revision, frame, projection, scale state and entity IDs. Unknown or
inferred edges require a legend/annotation; they must not look confirmed without
qualification. Raster exports are previews; editable geometry remains authoritative.

Signed depth is relative to a named base facade plane and its outward unit normal:
negative is recessed, zero the reference plane, positive projecting. A through
opening is a semantic aperture with host/extent, not merely a negative color or
arbitrarily deep pocket. Unknown wall thickness or recess depth stays unresolved.
Roof surfaces, floor levels and wall relationships belong to structured geometry,
not a single flattened depth image. Overlapping regions need explicit ownership,
ordering/relationship and validation; ambiguous overlaps block compilation as needed.

Optional colored relief maps use blue for recesses, neutral gray for the reference
plane and yellow/orange for projections. Colors/gradients visualize structured
values only. Include legend, units, range and an explicit unknown treatment; never
recover authoritative depth from palette values or invent metric depth from shading.

Primary geometry includes walls, floors, roofs, entrances, balconies, stairs,
recesses, projections and structural openings. Record AC units, pipes, cables,
antennas, decorative fixtures and other secondary elements with source regions,
attachment candidates and uncertainty for later LEGO placement. Simplification
may hide them in a drawing but must not irreversibly discard them. Vegetation,
cars and unrelated obstructions are context/occlusion evidence; obscured walls are
not confirmed blank walls or confirmed openings.

## Architectural Drawing Intelligence (ADI) and Evidence Reconciliation

Status: **approved specification; unimplemented**. These names identify the existing
AUE drawing and evidence-review responsibilities, not additional implemented services.
ADI uses the shared geometry, selected-view requirements and uncertainty annotations
to produce consistent simplified architectural drawings. Drawing corrections update
the shared representation and regenerate affected views; independently generated
images never become geometric authority.

Evidence Reconciliation retains source-linked observations, competing interpretations,
missing information and discrepancies across photographs, optional frames and
Analyzer reference findings. Register frames, scale and revisions before comparison;
keep initial photo inference separate from CAD-assisted corrections. Resolve conflicts
through evidence or explicit reviewed assumptions, issue a new interpretation revision
and retain unresolved issues. Neither human agreement nor CAD correspondence turns
an estimated dimension into a measured fact or validates a LEGO recipe.

These approved responsibilities are preserved by Height/Width proportional scaling.
Detailed interfaces, algorithms and scoring remain future implementation decisions.

## Scale and manufacturing separation

### Dimension-Driven Proportional Scaling

Status: approved design correction, 2026-11-04; unimplemented. Height and Width
are equally supported choices. Neither is permanently preferred or mandatory.

1. The user selects **exactly one** primary dimension: **Height** or **Width**, and
   supplies a finite, positive target value in **actual miniature millimeters**.
   Reject missing/both selections and invalid target values; do not silently choose.
2. Lock that selected dimension. Determine its positive finite extent D from the
   AUE relative geometry for the selected architectural scope. With target T mm,
   use one uniform factor **s = T / D** (miniature mm per relative unit).
3. Scale every other architectural length, position offset and depth with the same
   factor, about a recorded anchor: **pScaled = anchorScaled + s * (pRelative - anchorRelative)**.
   Rotations and dimensionless ratios remain unchanged. Unknown lengths remain
   unknown; scaling does not supply missing geometry or remove uncertainty.
4. **Height** is the vertical extent from the lowest architectural building point
   to the highest roof point in the selected scope. **Width** spans the selected
   architectural building scope along its declared building-width axis. Exclude
   unrelated context and the display base from both measurements. Facade Only and
   Partial Building use their selected scope bounds, not unseen whole-house bounds.
   Record included entities, extrema and axis; unresolved relevant bounds block a
   confirmed lock until evidence or an explicit reviewed design assumption resolves them.
5. Apply manufacturing constraints **after** proportional scaling. Thickness,
   clearances, backing and other adjustments must not silently change the locked
   target dimension. If a constraint would change that architectural extent,
   report a conflict and require an explicit revised user target/scope or approved
   compatible adjustment; never auto-rescale or unlock. Recheck the lock after
   every derived transformation using a declared numeric tolerance.
6. Optional depth compression is a separate, explicitly enabled and parameterized
   transformation after uniform scaling, with affected domains and protected
   features recorded. It must preserve the locked Height or Width. Uncompressed
   geometry remains available; compressed output must not be called proportional
   observed geometry. No implicit per-axis scaling is allowed in the uniform stage.
7. Preserve photo-estimate provenance, uncertainty bounds and correlations through
   scaling. The user target is a design constraint, not evidence of real-building
   metric size or certainty in all other dimensions.
8. Retain original relative geometry/revision, scope bounds and anchor, selected
   primary dimension, target/unit, D, factor s, scaled revision and each manufacturing
   adjustment/compression setting with before/after values and validation outcomes.
   Changing scope or geometry requires recomputing s under the same user lock and
   issuing a new traceable revision; never overwrite the source interpretation.

Example: relative width 120 and height 80. Width=180 mm gives s=1.5 and height=120 mm;
Height=100 mm gives s=1.25 and width=150 mm. These are alternative user choices,
not simultaneous constraints. Both preserve the same architectural proportions.

This explicit miniature target can resolve output sizing without establishing
real-world architectural millimeters. Keep a relative interpretation plus a derived
miniature sizing plan; do not relabel miniature mm as architectural mm or fabricate
a real-world scale denominator for Building JSON. The compiler/Planner adapter must
support that distinction explicitly or report it unsupported before execution.
All other execution-critical unknowns still block execution eligibility.

Architectural Drawing Intelligence (ADI) and Evidence Reconciliation remain approved
and unchanged by this scaling correction; their responsibilities are named above.
Detailed implementation contracts remain future work.

Known references/calibrated measurements establish architectural millimeters with
provenance and error bounds. Without absolute scale, retain explicit relative units
and ratios in the interpretation. Never label relative values as mm or assume a
standard door size is a measurement. Relative-only interpretations can pass scoped
consistency checks but cannot compile to execution-eligible metric Building JSON
until calibration or an explicit reviewed scale/design decision resolves the gap.

Keep observed/inferred building geometry immutable by revision. A separate derived
manufacturing plan applies scale, selected scope, backing/closure, wall thickness,
minimum sections, clearances, print groups and controlled depth compression under
[LEGO manufacturing rules](ARCHITECTURAL_LEGO_V1.md) and the
[modeling standard](ARCHITECTURAL_MODELING_STANDARD_V1.md). Store transformation
parameters, affected depth domains and source-to-manufactured entity mappings.
Preserve important facade dimensions and recognizable side openings/details;
compression must not silently shrink window widths or projections. Validate joints,
collisions, thickness and connectivity after transformation. No universal printer
minimum, compression factor or physical-print qualification is established here.
Existing miniature-mm APIs still require resolved lengths; COM meters are converted
only at the executor boundary. Architectural mm, miniature mm and relative units
must never be conflated.

“Validated Building JSON” means passing the declared schema and architectural
semantic gates for its revision and recording unresolved/non-executable fields.
Execution eligibility additionally requires all necessary quantities resolved and
Planner capability/manufacturing checks passed. It does not mean CAD validation,
accurate reconstruction, Validated LEGO definitions or a successful physical print.

## Analyzer and LEGO integration contracts

AUE emits an interpretation revision, source references, shared geometry, selected
views, uncertainty register and validation report. Analyzer consumes those artifacts
alongside separately registered reference evidence. Compare only matched building
and source/CAD revisions, with explicit coordinate registration, scale and entity
correspondences. Record residuals, tolerances, coverage, discrepancies, missing
reference evidence and reviewer decisions. Reference CAD may contain simplification
or manufacturing compression; compare architectural and manufactured domains
separately rather than treating every CAD dimension as real-building truth.

Freeze the initial photo interpretation before CAD comparison to distinguish image
inference from reference-assisted corrections. Analyzer returns findings linked to
both revisions; AUE creates a new reviewed revision rather than overwriting evidence.
AUE interpretation JSON, Analyzer Building Analysis/Source Manifest 0.1.0, executable
architectural Building JSON and resolved manufacturing plans are distinct contracts.
Do not inject new fields into the existing closed Analyzer schemas; introduce a
separate versioned comparison contract or explicitly versioned extension later.
The existing Analyzer validator does not validate AUE or executable Building JSON.

The Planner maps accepted architectural entities to supported, version-pinned LEGO
definitions/recipes and records unsupported cases. No AUE output can replace existing
validated geometry builders or automatically register a recipe as Validated. Preserve
Draft -> Testing -> scoped Validated and all prior regression/approval evidence.

## Smallest practical MVP: Pilot Building #001 facade interpretation

Use Pilot Building #001 as the future validation case, beginning with its main
facade and available side views. The pilot identity is user-designated; this session
has not located, opened or verified its images, saved CAD or scale. Do not assume
an automatically assigned Part title proves identity. The existing
[photo-blockout experiment](PHOTO_BLOCKOUT.md) supplies context, not an AUE result
or metric ground truth. No automatic whole-house reconstruction is required.

Start with a human-assisted Facade Only interpretation: one facade plane, main
contour, major opening profiles, floor/roof datums, entrance recess and projecting
volumes, plus only the side/roof geometry needed to constrain them. Permit relative
units and unresolved depth. Produce one shared geometry fixture, its front drawing,
a five-view requirement matrix and uncertainty/discrepancy register. Generate a
selected side/top view only when needed. A plain side may be an explicit miniature
design decision. Defer full rear geometry, detailed windows, autonomous video/CV,
LEGO placement and CAD execution.

### Acceptance criteria (future checks, none run in this session)

| Gate | Required evidence |
| --- | --- |
| Pilot/scope | Explicit source inventory and Pilot #001 identity; selected model scope, included regions and reason for every view requirement |
| Shared authority | All generated views reference one geometry revision and entity IDs; changing a corner/opening regenerates affected views without independent drawing edits |
| Structural coverage | Main contour, major openings, roof profiles, entrance recess and projections represented or explicitly unavailable; secondary/context observations retained |
| Geometry consistency | Non-COM fixtures check finite values, frames, shared anchors, host containment, datum agreement, depth signs and cross-view projection residuals under declared tolerances |
| Uncertainty | Missing scale/depth and occlusion survive serialization and rendering; unsupported/contradictory cases return incomplete/blocked, never fabricated metric success |
| Scope adaptation | Fixtures cover all three scopes and four requirement statuses; facade-only works without rear evidence; required missing evidence blocks complete status |
| Compilation boundary | Relative-only/critical unresolved input cannot yield execution-eligible metric JSON; negative fixtures reject invalid units, broken links and opening/pocket confusion |
| Proportional scaling | Future fixtures exercise Height and Width independently, reject zero/two selections and invalid bounds/targets, verify s=T/D and unchanged ratios, selected-scope/base exclusions, uncertainty/traceability and lock preservation through manufacturing; conflicts fail explicitly |
| Manufacturing separation | A synthetic compression example preserves source geometry and protected facade/side features; unsupported transforms fail with diagnostics |
| Pilot comparison | Later authorized comparison against matched CAD/reference evidence audits the named structural categories, logs residuals and missing dimensions, and distinguishes design compression from interpretation error |
| Review/reproducibility | Human review tied to exact geometry/drawing digests; fixed reviewed inputs regenerate equivalent normalized geometry/views; known discrepancies remain visible |

Choose numeric tolerances, annotated expected entities and scoring rules before
pilot evaluation. First contract/projection tests may use synthetic data without
studio access. Pilot validation remains incomplete until permitted source evidence
and comparison are available; synthetic success alone is not Pilot #001 acceptance.
No Golden Dataset or AUE accuracy result exists yet.

## Recommended implementation order and unresolved decisions

1. After implementation approval, define versioned interpretation/view/quantity
   contracts and synthetic fixtures; agree relative units, scope/status semantics,
   stable IDs and evidence/review rules. Keep this independent of COM.
2. Implement shared geometry, transforms, deterministic orthographic projection and
   non-COM consistency/uncertainty tests; then scope planning and structured depth.
3. Add human-assisted source registration, segmentation and camera constraints;
   select a reconstruction method only after a small fixture demonstrates its limits.
4. Under separate asset-access authorization, register Pilot #001 photos/side views,
   freeze a first interpretation, render necessary views and review uncertainty.
5. Define/version Analyzer comparison correspondence and coordinate/scale alignment;
   obtain authorized reference evidence, compare and issue revised interpretations.
   Inspector implementation/safety remains a separate workstream, not assumed ready.
6. Implement the Building JSON compiler and Planner integration for a supported
   subset; only later authorize deterministic CAD execution and print validation.
   Broaden multi-view/video automation and full-building scope after the MVP gates.

Open technical decisions: geometry/profile kernel and serialization schema/version;
annotation/editor and correction UX; camera calibration/reconstruction methods and
video-frame selection; uncertainty representation/propagation; evidence storage and
privacy; reference-scale choice; numeric/visual tolerances and pilot ground-truth
annotations; overlap/topology handling; comparison schema and registration method;
compiler/Planner boundary encoding and supported subset; compression domains and
printer/material profile. No dependency, framework migration or tool provider is
selected by this design. These require evidence and scoped implementation choices.

## Session evidence and limits

Documentation review only. Existing source dispatch and Analyzer project boundaries
were inspected. Historical Analyzer 70/0 and regression 165/0 results are recorded
in [Phase 1 evidence](tests/results/ANALYZER_PHASE1_2026-11-04.md); they are not AUE
tests and were not rerun here. No build, runtime test, AUE extraction, image generation,
CAD access, studio intake, dataset comparison or physical print was performed.
Implementation must await approval. Preserve all existing evidence and definitions.

## Pilot #001 — Trumpeldor 15, Kfar Saba (2026-11-05)

Status: **approved requirements; design only, unimplemented and unvalidated**.
This pilot-specific policy refines the generic MVP above. The user identifies the
building; photo, marked cutoff, current elevation draft and source CAD revisions
are not registered repository artifacts here. No revised drawing/model is claimed.

| Deliverable | Requirement | Boundary/purpose |
| --- | --- | --- |
| Main front orthographic elevation | Required | Envelope, openings, roof, surround |
| Relevant side elevation(s) | Required | Only portions within selected reconstruction boundary; exact side IDs unresolved |
| Simplified top footprint / roof plan | Required | Included footprint, roof and marked depth cutoff |
| Relative depth and relief mapping | Required | Host-linked projections/recesses; unknown values explicit |
| Rear elevation | Not Required | Open or hollow rear permitted; no rear evidence prerequisite |

Do not invent unseen rear windows, doors, facade details or roof extensions.
An open/hollow rear is a permitted reconstruction choice, not an observation of
the real rear. Any backing/closure is a separate manufacturing proposal.
Reconstruction depth must follow the user-marked cutoff in the reference photo.
Register its image revision and marked boundary before assigning numeric depth;
later calibrate against the matched existing source model where available. Keep
photo-only and reference-assisted revisions distinct. Missing/ambiguous cutoff
leaves depth unresolved; do not substitute guessed whole-building depth.

Lock **Height = 180 actual miniature mm**, from the lowest architectural building
point to the highest roof point, excluding display base and unrelated details.
Use the uniform proportional sizing contract, recording extrema, scope and factor.
Manufacturing must preserve the lock or report a conflict. This design target does
not establish real-world measurements; other pilots retain Height-or-Width choice.

### Right-front WindowSurround and planned workflow

The user reports a visibly projecting architectural surround at the right-front
window, underrepresented in the current AUE elevation draft, and estimates its
thickness should be approximately **2× the depicted thickness**. Record this as
**user-supplied visual guidance pending CAD validation**, not a verified physical
measurement. Preserve the draft revision/source region when supplied. Whether
“thickness” affects border width, outward projection or section remains unresolved;
do not automatically double all dimensions, the aperture, sill or building scale.
No numeric surround dimensions or successful correction are established here.

Use the independent Draft OpeningSurround family / WindowSurround application in
[Architectural LEGO](ARCHITECTURAL_LEGO_V1.md#opening-surround-draft-proposal).
Planned sequence: register photo/cutoff/draft -> freeze photo-only interpretation ->
resolve visual correction with uncertainty -> update shared surround geometry ->
regenerate required front, bounded sides, top and relief -> later compare matched
source CAD -> resolve manufacturing/capability -> independently validate geometry,
Builder and regressions, with human visual review. None ran in this session.

Future pilot checks must verify cutoff agreement across side/top views, no invented
rear/roof entities, the 180 mm lock after transformations, independent aperture,
surround and sill edits, and correction provenance. Missing execution-critical
quantities block execution; absent rear evidence does not. OpeningSurround remains
Draft until independent geometry, SolidWorks Builder and regression validation
supports promotion under the existing lifecycle and visual approval requirements.
