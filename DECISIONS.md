## PB-001 — reproducible photo blockout before source-tree diagnosis (2026-11-04)

Keep the successful massing experiment as explicit `--photo-blockout`, separate
from the unchanged default demo and from the read-only Analyzer/Inspector. The
command always creates a new unsaved part; it cannot silently modify a cloud-saved
pilot or the active user document. No original feature tree is consulted for
reconstruction; a future Inspector supports later failure diagnosis instead.
Assumed miniature geometry is labeled ASSUMED and is not promoted to a Validated
LEGO recipe by visual massing approval. Code and portable handoff go in Git; the
user saves the CAD model separately. See [pilot instructions](PHOTO_BLOCKOUT.md).

# Architectural decision log

Baseline date: 2026-11-03. Decisions below distinguish existing behavior from approved future design. Approval of a specification does not establish implementation or test completion. Sources are repository-relative and should be rechecked when implementation changes.

## Established implementation decisions

| ID | Decision and consequence | Evidence |
| --- | --- | --- |
| I-001 | C# console application on .NET Framework 4.8; STA entry point and direct COM automation. No server/API architecture exists today. | csproj; Program.cs |
| I-002 | Attach to an existing SolidWorks application, never auto-launch in the connector. User documents are not automatically saved/closed; failed parts remain inspectable. | SolidWorksConnector.Connect; SolidWorksModeler cleanup |
| I-003 | Existing public geometry units are actual miniature mm; COM geometry uses meters. Centered-wall/model-coordinate semantics remain stable. | UnitConverter; CreateWall/CreateWindow |
| I-004 | Opening cuts are inward Blind cuts at wall thickness plus configurable tolerance, default 0.2 mm; orientation follows face/sketch normals. | OpeningCutToleranceMm; CreateOpeningCut |
| I-005 | Existing window rows/floors use shared sketches and constraints. Preserve these paths rather than replacing them under Pattern/Mirror guidance. | CreateWindowRow/CreateWindowGrid; modeling standard introduction and section 9 |
| I-006 | Door APIs describe ground apertures, with bottom-edge references. Elevated doors require an additive path. | CreateDoor/CreateDoorRow |

## Approved design decisions, not yet implemented as a system

Source: ARCHITECTURAL_MODELING_STANDARD_V1.md and ARCHITECTURAL_LEGO_V1.md. LEGO specification explicitly records approved direction.

| ID | Decision | Implementation boundary |
| --- | --- | --- |
| A-001 | Architectural LEGO uses reusable parametric definitions, separate building instances, and immutable validation evidence. | No library/contracts implementation found. |
| A-002 | Analyze images before reconstruction; use corrected orthographic/simplified facade representations before CAD. | No recognition/elevation pipeline found. |
| A-003 | Separate recognition, geometric planning/manufacturing resolution, and deterministic CAD execution using validated Building JSON. | Current program directly calls geometry; no JSON executor found. |
| A-004 | Prefer adapting versioned templates, then composition, then new variants/definition proposals when necessary. | Matching/composition engine planned. |
| A-005 | Universal lifecycle: recognition, classification, matching/adaptation, composition, proposal, automated checks, required human visual approval, validated registration. Draft -> Testing -> Validated; untested AI geometry cannot self-register as Validated. | Promotion and validation infrastructure absent. |
| A-006 | Support full buildings and facade-only models with selective side-depth compression while preserving front-facing proportions and important detail/member shapes. | No depth resolver or full-building generator found. |
| A-007 | Building JSON uses real-world mm, parent-local frames, X right/Y up/Z outward, explicit anchors and floor association. Resolved miniature dimensions remain separate. | Existing centered API coordinates require adapters; do not reinterpret in place. |
| A-008 | Body separation follows manufacturing unit, print orientation, supports, assembly, painting, and material use. Scope cuts/merges explicitly. | Existing wall/sill behavior is narrower; generic manufacturing planner absent. |
| A-009 | Standard actual-miniature conventions: wall 3 mm, internal wall 2 mm, minor/medium relief 0.8/1.2 mm, clearance 0.2 mm. Roof thickness and minimum printable feature size are profile/part-specific. | Documented guidance, not global implemented configuration. Assembly clearance is distinct from cut tolerance. |
| A-010 | Favor blockout first, shared datums, meaningful variables, stable geometric references, independent depth zones, and repetition of related feature families. Mark inferred geometry and allow evidence-driven depth revision. | Most relationships/metadata are future contracts. |
| A-011 | Stairs are dedicated parametric components: width, count, rise, run, direction, optional landings/railings. | Contract exists; stairs executor absent. |
| A-012 | Balconies next, then facade depth operations, then prioritize the first end-to-end image-to-building MVP before more isolated features. Golden Dataset supports repeatable validation. | Roadmap only; no balcony executor or dataset found. |

## Projecting slab implementation decisions — 2026-11-03

- S-001: Add a partial modeler file and separate COM-free typed contracts instead of refactoring existing geometry. Rationale: reuse existing rectangle/dimension/cleanup paths while isolating the first LEGO executor and preserving legacy signatures/behavior.
- S-002: Treat slab inputs as resolved actual-miniature mm; top elevation is measured from the host bottom datum and translated to the centered legacy model frame. Rationale: honor the LEGO anchor without conflating real-world Building JSON with the current CAD API.
- S-003: V1 targets only current MainWall in its original orientation/dimensions; validate host ID, document and feature dependencies. Reject known footprint overlap but allow shared boundaries. Rationale: explicit bounded support is safer than claiming a generic wall registry or 3D collision engine.
- S-004: Always create an unmerged slab and verify one new solid after rebuild. Rationale: preserve a separate manufacturing body even at facade contact. Assembly joint/clearance design remains future work.
- S-005: Definition lifecycle is Testing; the immutable validation record certifies only successful non-COM parameter/placement preflight and retains CAD NotRun. No promotion API is provided. Rationale: code compilation and 95 passing pure tests are not evidence of actual CAD or manufacturing validation.
- S-006: Reuse center-based rectangle dimensions; expose width/thickness/placement and name extrusion depth SlabDepth. No top-preserving equation or in-place instance update is added in this increment. Rationale: keep scope minimal; document that manual thickness edits with fixed center Y move the slab top. Validate fresh instance inputs for reliable elevation/thickness combinations.

Evidence: ProjectingBalconySlab.cs; SolidWorksModeler.BalconySlab.cs; tests/PROJECTING_BALCONY_SLAB_V1.md; tests/results/PROJECTING_SLAB_2026-11-03.md. Actual CAD execution remains NOT RUN.

## Proposed or externally supplied intentions requiring confirmation/detail

These entries must not be mistaken for implemented capabilities or revisions to approved contracts.

- P-001: Optional video input is a future intention supplied in the documentation setup request. Confirm scope, frame selection, calibration, and multi-view evidence strategy before implementation.
- P-002: Exact JSON Schema, parameter binding, repetition, shared-datum references, and child-role overrides need concrete machine-readable definitions consistent with the approved LEGO contract.
- P-003: Select library storage, promotion authority, visual-approval triggers, withdrawal/deprecation policy, and environment compatibility rules before library promotion is implemented.
- P-004: Confirm target printer/material profiles, minimum thickness, numeric/visual tolerances, and acceptable automatic thickening policy. Do not adopt the example's 0.8 mm minimum as universal.
- P-005: Define balcony joints, clearances, railing print grouping, and repeated-body/feature strategy before the relevant geometry increment.
- P-006: Specify depth domains and mating-interface handling before multi-elevation side-depth compression.
- P-007: Choose corrected-elevation data format/editor, scale calibration, and correction synchronization for the MVP.

## User-approved scoped validation and future slab design — 2026-11-03

### V-001: Promote the tested V1 scope

Status: **established, user approved**. Project user visually reviewed and approved Part3-Part7 (slab-only edits, no door openings expected) and Part8 (elevated door aperture, alignment, placement). Combined with the recorded automated suite and actual CAD baseline/edit/integration checks, this authorizes **Validated** for lego.balcony.projecting-slab 1.0.0 only within [the explicit scope record](tests/results/PROJECTING_SLAB_V1_VALIDATED_2026-11-03.md).

Rationale: distinguish a tested reusable capability from unbounded acceptance of every parameter tuple. Documented sampled widths/depths/thicknesses/positions/elevations are not a certified Cartesian range. The existing front-facing MainWall and environment constraints remain. Preserve original failure, corrected passes, measurements, user approval and transcript hashes. Runtime lifecycle/scope constants may reflect the promotion; instance preflight CadStatus remains NotRun because preflight is not CAD execution. No geometry change is part of this promotion.

### F-001: Prefer top-preserving thickness growth in a future version

Status: **user-supplied future design direction; not implemented or validated**. Slab thickness changes should preferably preserve slab top elevation and grow downward, maintaining a door threshold when a relationship is declared. Desired relation: centerY = slabTopY - slabThickness/2. The top datum must be explicit; future dependency logic must not infer it from unrelated screen coordinates.

Rationale: current V1 center-based edits move the top by half the thickness change and can break an existing threshold alignment. Preserve the validated V1 behavior; implement the changed edit semantics in a separately versioned candidate, selecting semantic version according to compatibility impact. Recheck host bounds, threshold relationships, volume, body separation, rebuilds and regressions before promotion. User acceptance of current visuals does not claim this future behavior already exists.

### F-002: Ground-floor thin slab and door-leaf clearance

Status: **user-supplied future capability; not implemented or validated**. Support ground-floor/ground-threshold slabs as an explicit placement policy, with configurable actual-miniature **GroundSlabThicknessMm** and **DoorLeafBottomClearanceMm** (proposed configuration names). Choose thickness against the target printer/material minimum; no universal thin-slab default is established.

Rationale: a slab top at host elevation 0 with positive downward thickness violates V1's current host-height containment. Future support must explicitly permit/validate the intended below-host extent rather than silently raising the slab. If a door leaf is represented, its bottom should clear the slab top by the configured clearance; the leaf is distinct from the wall aperture. Decide whether the leaf bottom/height changes and preserve head/threshold datums deliberately. Do not enlarge/shift the opening to disguise a leaf-clearance problem. No modeled leaf or clearance adjustment exists in V1.

Ground-slab thickness, leaf bottom clearance, assembly clearance and opening-cut tolerance are separate quantities; do not scale actual-mm values twice or equate them. Future fixtures must cover aperture integrity, leaf clearance, minimum printable thickness, host contact, separate-body behavior and rebuilds. No production feature was added by this decision.

## BA-001 — Projecting BalconyAssembly side-wall manufacturing unit (2026-11-03)

Status: **user-approved design rules; new composite lifecycle Draft; not implemented/tested**. Proposed ID lego.balcony.projecting-assembly 1.0.0. Pin and preserve the already scoped Validated Projecting Balcony Slab V1; assembly validation is independent.

- Start walls flush at the exterior facade plane with no gap; extend them to the slab front edge. Bind wall depth persistently to slab depth so editing the slab propagates to both enabled walls.
- Keep wall height/thickness independently configurable and support none/left/right/both. Proposed rectangular recipe places thickness inside slab side boundaries and measures height above slab top.
- Default to one connected printable balcony solid containing slab and enabled walls; keep it separate from the facade solid. Preserve independently editable child features/dimensions. The printable unit is the whole balcony, not each wall and not the touching facade.
- Prefer printing with slab underside flat on the plate, walls upward. Treat this as manufacturing orientation metadata separate from architectural coordinates.
- Require solid connectivity, minimum printable thickness, safe union target scope and successful rebuilds, including depth propagation, before promotion. Printer-specific thickness limits remain configurable, with no universal default assigned.

Rationale: a slab with upright side walls is one intended printable unit in the approved orientation, while union with the facade would defeat that manufacturing separation. Zero facade gap is intentional and must not be changed by generic assembly-clearance defaults.

Original proposed implementation (superseded by BA-002 below): create unmerged wall bosses, then unite balcony child bodies separately. Preserve this history, but do not implement that Combine-based recipe as the default.

See ARCHITECTURAL_LEGO_V1.md's BalconyAssembly V1 section for local bounds, proposed fixture volumes/counts and validation matrix. This decision records rules/plan only; no side-wall geometry or CAD tests ran. Future top-preserving slab thickness and ground-floor designs remain separately versioned work and must not silently alter the validated child.

## BA-002 — Side-wall Boss-Extrude merge scope clarification (2026-11-03)

Status: **user-approved implementation direction; BalconyAssembly V1 remains Draft, unimplemented**. Supersedes BA-001's proposed separate-body-plus-Boolean-Combine strategy; geometric/printing requirements otherwise remain.

- Use standard independently named side-wall Boss-Extrudes with **Merge Result enabled**. Where the API supports merge scope / Selected Bodies, target only the slab/current balcony body, never MainWall. After each wall/rebuild, resolve the current assembly body before targeting the next wall; do not assume stale body handles remain valid.
- A successfully merging Boss-Extrude needs **no separate Boolean Combine**. Do not introduce a redundant union feature. If facade isolation cannot be guaranteed through the supported API, stop and report the limitation rather than auto-select all touching solids or silently change the recipe.
- Require exactly two solids after rebuild: independent MainWall and connected BalconyAssembly (slab plus enabled walls). Verify actual slab/wall connectivity, intended body membership, expected volumes and unchanged facade, not count alone.
- Preserve separate editable slab/left-wall/right-wall sketch and Boss-Extrude histories. Merging the solid result must not flatten parametric features.

Rationale: scoped Merge Result expresses the desired manufacturing unit directly, avoids a redundant Boolean feature and preserves normal SolidWorks parametric editing. Fixture feature-count expectations become 4/6/8 for none/one/both sides, including sketches, replacing the earlier 4/7/9 proposal. No geometry or tests were implemented/executed for this clarification; the validated standalone slab remains unchanged.

## BA-003 — First both-wall implementation boundary (2026-11-03)

Status: **implemented/non-CAD tested; Testing, CAD NOT RUN**. Add a narrow immutable both-wall contract and a new partial modeler file; leave standalone slab and all legacy geometry unchanged. Other side modes stay future work rather than silently presenting incomplete modes as supported.

Use FeatureExtrusion3 for documented selected-body mark 8: Merge=true, selected scope=true, AutoSelect=false. Resolve target by COM identity relative to the facade each time; require exactly one selected target and verify two bodies, unchanged facade and expected balcony mass/topology after each wall. No all-body/Combine fallback. Rationale: explicit API scope plus actual-state guards are necessary at the touching facade interface. Actual safety remains pending CAD execution, not established by compilation.

Bind side depth to the named slab depth, and center dimensions to slab position/size plus independent wall dimensions through equations. This retains persistent depth dependency and editable features without altering the slab. Equations follow the existing center-dimension sign branches; cross-origin edits are outside current support. Restrict the first adapter to an unedited single-solid host and walls within host height. Require a supplied minimum printable section, not a new universal printer default.

The single prepared CAD case checks the U-profile corners/topology/volumes/body separation and depth 24->26->24 in one fresh unsaved document. Analytic contact tests are distinguished from measured connectivity and physical printing. 141 non-CAD checks and both builds pass; user approval is required before any CAD execution. Evidence: tests/results/BALCONY_ASSEMBLY_IMPLEMENTATION_2026-11-03.md.

## BA-004 — Top-face shared-wall optimization candidate (2026-11-03)

Status: **user feedback recorded; concrete construction proposal awaiting implementation approval**. Candidate recipe **top-face-shared-r1: Draft**. Existing BalconyAssembly both-wall implementation remains a preserved Testing regression baseline with actual CAD PASS; it is not replaced or finally promoted. Standalone Projecting Balcony Slab V1 stays scoped Validated and unchanged.

### Motivation and compatibility

The user proposes sketching wall footprints on the slab's top face and growing upward, reflecting hands-on SolidWorks modeling experience. Current code creates two facade-plane sketches and two depth extrusions with individual bindings; its full baseline Part has four sketches/four bosses. A top-face common boss can reduce this to three sketches/three bosses for equal-height walls. The actual eight-feature baseline and depth propagation evidence remain authoritative comparison fixtures: tests/results/BALCONY_BOTH_WALLS_CAD_2026-11-03.md.

Implement a separate candidate path only after approval; do not rewrite CreateBalconyAssembly or its BALCONY_BOTH_WALLS case. Reuse the validated slab creation, units, preflight conventions, selected-body targeting and ICE-aware test classification. Add only a small top-face profile builder and recipe-specific planning/measurements; no new package/dependency or actual Modeling Optimization Engine is required.

### Proposed data and geometry

Eventual configuration: sideMode none/left/right/both; optional **solid front parapet**; independent enabled-wall heights and thicknesses; slab width/depth/thickness/placement/host and explicit manufacturing minimums. The solid parapet is its own wall role, not a replacement for future glass, metal or decorative Railing components. Omitted walls generate no footprint. No walls and no parapet delegates to the unchanged slab, with no wall sketch/boss.

Use the slab top rear-edge center as local origin, X horizontal, Y up and Z outward. Slab occupies X [-W/2,W/2], Y [-T,0], Z [0,D]. Footprints at Y=0:

```text
Left:  X [-W/2, -W/2+tL], Z [0,D]
Right: X [W/2-tR, W/2],   Z [0,D]
Front: X [-W/2, W/2],    Z [D-tF,D]
```

Keep positive clear interior width, and tF < D when the front wall is present. Validate enabled dimensions/sections against the supplied print profile. Side rears stay on the facade plane without a gap; sides reach the slab front, and parapet outer face is on that front edge.

Build the **planar union** of enabled rectangular footprints before creating sketch loops. Both sides alone give two disjoint loops; a front wall joins appropriate footprints into an L/U outline. Do not extrude overlapping rectangles/shared duplicate edges or unintentionally fill the balcony interior. A small deterministic orthogonal union/subtraction routine is sufficient for these rectangles; do not add a geometry library merely for this case.

**Proposed corner rule for approval:** where footprints overlap, the taller wall continues through the corner; the solid is the union of the walls, with local height equal to the maximum included height. A lower-wall trim must exclude a taller neighbor's footprint. Do not silently choose front-wall ownership or cut a taller side's corner. Different corner ownership would need explicit revised expected geometry.

### Equal-height construction / SolidWorks operations

1. Create host/slab through existing APIs. Resolve the slab body separately from MainWall, as before.
2. Find the slab's top planar face by body ownership, upward normal and measured slab-top elevation/extent, not face-list index or screen coordinates. Select that face as sketch support. Use ModelToSketchTransform to map architectural X/Z into sketch coordinates; the existing AddFaceRectangle assumes the facade X/Y plane and cannot be reused unchanged.
3. In one WallFootprintSketch, create normalized closed loop(s) with CreateLine/rectangle geometry as appropriate. Use horizontal/vertical/coincident/collinear relations, independent wall thickness dimensions and persistent references/equations to slab width/depth. Convert only genuinely needed slab edges with SketchUseEdge3, keeping reference outlines construction-only if retained. Do not accidentally extrude a complete converted slab perimeter.
4. Select the profile and only the current balcony body (mark 8); use FeatureExtrusion3 with Merge=true, selected scope=true, AutoSelect=false. Extrude upward by the common height, verifying sketch normal against architectural +Y before choosing direction. Never include MainWall or add Combine.
5. Rebuild; verify actual geometry/connectivity/volumes, unchanged facade, two solids and intended feature history. Retain editable wall parameters even though they share a sketch/boss.

API references: [FeatureExtrusion3](https://help.solidworks.com/2024/english/api/sldworksapi/solidworks.interop.sldworks~solidworks.interop.sldworks.ifeaturemanager~featureextrusion3.html), [Convert Entities](https://help.solidworks.com/2025/english/SolidWorks/sldworks/c_Convert_Entities.htm?id=bf50f2c9700f4c08a78ab0b620a30f8e). Face/contour/reference stability remains a hypothesis to test, not validated behavior.

### Unequal heights — compare recipes before selecting

First evaluate common footprint extrusion to Hmax, then one scoped lowering cut per distinct lower target height (group equal-height reductions). For a target height h, cut only the footprint needing that height **minus all taller neighbors' footprints**, downward from Hmax to h. Normalize cut regions too. The original slab remains below Y=0 and must not be reached by these cuts. Use FeatureCut4 (or the existing supported scoped cut API) with explicit current BalconyAssembly target, AutoSelect disabled, and controlled depth Hmax-h. A wall-thickness opening cut helper is not a lowering-cut helper and must not be reused with incorrect depth semantics.

Use a retained maximum-height cap face as support only if its role-based selection is stable through the cuts; otherwise use a named maximum-height reference plane and include its cost. A cut's body scope alone does not protect adjacent walls within the same body: region subtraction, vertical limits and measured geometry checks are mandatory. Re-resolve the assembly body after every operation. [FeatureCut4 API](https://help.solidworks.com/2024/english/api/sldworksapi/solidworks.interop.sldworks~solidworks.interop.sldworks.ifeaturemanager~featurecut4.html).

Compare with separate top-face bosses per wall or per equal-height group, each scoped to the same balcony body and starting from slab top. Prefer that recipe if lowering cuts, top-face references or height-order changes make the shared-max recipe harder to maintain. Retain separate sketches where contour reuse is unreliable rather than forcing a complex shared-sketch selection system.

Named height parameters, Hmax, extrusion depth and cut depths must remain linked. Height-order changes can turn a cut depth into zero or change its protected corner region; suppress/replan those features explicitly, never leave invalid zero-depth cuts or fixed stale masks. If no simple reliable update path is demonstrated, choose separate bosses for that configuration. Enabled-wall changes also require deterministic footprint regeneration and validation, not just modifying extrusion numbers.

Expected whole-Part complexity, **including host/slab and absorbed sketches**, excluding an optional cut datum:

| Recipe/configuration | Sketches | Bosses | Cuts | Total generated features |
| --- | --- | --- | --- | --- |
| Existing both-wall equal-height baseline | 4 | 4 | 0 | 8 (actual) |
| Candidate, no walls/parapet | 2 | 2 | 0 | 4 |
| Candidate, any enabled walls all equal height | 3 | 3 | 0 | 6 |
| Shared max + one lower-height group | 4 | 3 | 1 | 8 |
| Shared max + two lower-height groups | 5 | 3 | 2 | 10 |
| Separate wall bosses, two included walls | 4 | 4 | 0 | 8 |
| Separate wall bosses, three included walls | 5 | 5 | 0 | 10 |

A dedicated cut plane adds one reference feature, not a sketch/boss/cut. Separate equal-height groups can reduce counts too. Thus unequal-height shared-max construction is not automatically simpler or smaller. These candidate counts are planned, not measured results.

### Concrete comparison fixtures and validation gates

Start with the same host/slab as baseline: 180 x 220 x 3 host; W=56, D=24, T=3; center X=20, slab top host elevation 120. Both sides h=20,t=2 must reproduce balcony volume **5952**, total **124752 mm^3**, two solids and identical U-section, with **six instead of eight** generated features. Depth 24->26->24 must reproduce 6448->5952 balcony volume while wall height/thickness stay fixed.

For union corner semantics, expected wall volume equals D*tL*hL + D*tR*hR + W*tF*hF - tL*tF*min(hL,hF) - tR*tF*min(hR,hF), omitting disabled terms; add W*D*T for total balcony volume. Proposed cases (all enabled thicknesses 2):

- Both sides 20 plus front parapet 20: **8032 mm^3** balcony, common boss only.
- Both sides 20 plus front 10: **6992 mm^3** balcony. Shared max begins at 8032 and removes central front strip volume 1040, preserving taller side corners.
- Left 20, right 15, no front: **5712 mm^3** balcony; compare one scoped trim versus two direct bosses.
- Left 20, right 15, front 10: **6752 mm^3** balcony; two lower-height groups, with corner protection.

Test none/left/right/both crossed with front off/on; equal/unequal heights; independent thicknesses; width/depth/placement/elevation edits; height-order changes; disabled/reenabled roles; exact boundaries and minimum/invalid sections. Build pure planner tests first. Preserve all existing regression tests, source and raw CAD baseline evidence. Compare equivalent fresh Parts, measuring dimensions, corner profiles, volume, two-body separation/connectivity, facade invariance, rebuild errors/repeatability and edit propagation, then tree complexity/reference cost. Rebuild timing may be measured consistently but must not substitute for correctness. Old U-profile topology checks are not universal across parapet/mode configurations; use recipe-specific expected profiles and appropriate topology checks.

Printing remains underside-down with walls growing upward. Manufacturing eligibility/minimums and human visual approval remain required; do not infer physical printer qualification. Any candidate CAD execution requires separate explicit approval.

### OPT-001 — Candidate Modeling Optimization Engine learning

Record the user-supplied candidate best practice: **"Build shared geometry first, then modify only the differences."** It is a strategy to evaluate, not an unconditional instruction to use extra cuts. Score correctness, parametric robustness, stable references/rebuilds, manufacturing suitability, edit behavior, reproducibility and feature-tree complexity together. Feature count alone must not choose a recipe. The future engine is not implemented by recording this rule.

### Staged proposal / approval boundary

1. After approval, add only the equal-height both-wall top-face candidate and pure planning tests, retaining the existing path/case.
2. Compare baseline-equivalent CAD and width/depth edits under separate execution approval; verify the six-feature target without reducing robustness.
3. Add remaining side modes and optional solid parapet footprints; validate unions/corners.
4. Prototype shared-max/scoped lowering and direct-boss alternatives for unequal heights; choose per robustness evidence and validate height-order changes.
5. Record visual approval and scoped evidence before replacement/promotion. Do not alter validated slab edit semantics or ground-floor support as part of this optimization.

This session modified DECISIONS.md and ROADMAP.md only. No production geometry, test source, CAD execution, test run or Git operation. Implementation awaits user approval, including the proposed corner rule.

## BA-005 — Next-session optimization scope and baseline preservation (2026-11-03)

Status: **user-directed next development task; not implemented or tested**. Implement and validate the optimized **equal-height, two-side-wall** candidate with one slab-top-face shared sketch and one scoped upward Boss-Extrude. Keep MainWall separate. Preserve the existing separate facade-plane wall sketches/Boss-Extrudes, its actual BALCONY_BOTH_WALLS passes and the scoped Validated standalone slab as compatibility baselines.

Rationale: evaluate the user's simpler six-feature construction against the measured eight-feature baseline without destroying working geometry or conflating a design proposal with tested implementation. Initial scope excludes optional parapet, unequal-height trimming and ground-floor work; their eventual designs remain in BA-004/F-001/F-002. The unequal-height corner policy remains a proposal and does not block the first equal-height two-wall comparison. Correctness, rebuild/edit robustness, manufacturing and body isolation outrank feature-count savings. Obtain approval before candidate CAD execution, and keep the assembly Testing until required evidence/review are complete.

This session-close instruction authorizes a reviewed commit/push to origin/main without force. It does not authorize geometry changes, SolidWorks execution or implementation during closing, and does not override the future-session default against automatic Git operations.

## BA-006 — Additive equal-height slab-top executor (2026-11-04)

Status: **implemented, compiled and non-CAD tested; Testing; CAD NOT RUN**.
The current user request authorizes implementation and non-CAD verification only.
BA-004/BA-005's first increment is `CreateTopFaceBalconyAssembly`, a separate API;
the validated slab and existing `CreateBalconyAssembly` / `BALCONY_BOTH_WALLS`
remain unchanged. No new dependency or optimization engine was introduced.

Use one geometrically resolved upward planar slab face, one X/Z-transformed shared
sketch with two disjoint center rectangles and construction-only diagonals, and one
upward common-height boss. Require exactly equal heights before CAD; retain independent
thickness dimensions. Shared geometry does not require a union algorithm for these
two disjoint footprints; parapet/overlap/unequal-height cases are excluded.

Bind each footprint's depth to SlabDepth, its X center to slab center/width minus
its own thickness, and its Z center to the unitful fixed facade rear offset plus
half SlabDepth. Face support locates vertical placement. Initial sign branches and
an unedited front-facing host remain limits; unsupported sketch axes/origin fail
explicitly. This avoids reusing the facade-only AddFaceRectangle or silently assuming
top-sketch axis direction. Face extents use edge endpoints/area because GetBox is
approximate, per the [SolidWorks API](https://help.solidworks.com/2024/english/api/sldworksapi/SOLIDWORKS.Interop.sldworks~SOLIDWORKS.Interop.sldworks.IFace2~GetBox.html).

Keep the baseline's explicit mark-8 selected balcony body, Merge=true,
UseFeatScope=true, UseAutoSelect=false, and re-resolve the body for final invariants.
No MainWall selection or Combine fallback. Body count plus facade bounds/volume,
balcony volume and topology are required; actual CAD isolation is not proven by
these source guards or successful compilation.

Validation: 165 non-CAD checks passed (141 existing +24 candidate), all three Debug
builds passed with existing MSB3305 warnings. The dedicated comparison runs the
unchanged baseline then candidate in two fresh parts, checks depth 24->26->24 and
candidate width 56->60->56, expected six-versus-eight features and identical U-shape.
No CAD, SolidWorks control or Git operations occurred. Actual constraint solving,
merge scope, stable references/edits and visual review remain approval-gated.
See [results](tests/results/TOP_FACE_IMPLEMENTATION_2026-11-04.md) and
[execution scope](tests/TOP_FACE_BALCONY_COMPARISON.md). No promotion or baseline replacement.

## BA-007 — Scoped optimized release and preferred equal-height recipe (2026-11-04)

Status: **user visually approved; scoped Validated**. The user confirms Part3's
equal-height two-side-wall geometry is correct and matches the intended method.
All applicable checks for that fixture and its measured edits passed. Promote
only `lego.balcony.projecting-assembly.equal-height-top-face` **1.0.0**, recipe
`top-face-shared-r1`, under [this scope/evidence record](tests/results/TOP_FACE_VALIDATED_2026-11-04.md).
The distinct definition identity prevents broad promotion of the existing assembly
definition. Retain its eight-feature path/case unchanged as a Testing regression
baseline, and retain the validated slab V1 unchanged.

Prefer **one shared slab-top sketch and one upward merging boss scoped only to
the balcony body** for equal-height two-side-wall construction within recorded
scope. Rationale: actual six-versus-eight feature comparison preserved geometry,
two bodies, facade isolation, rebuilds and parameter propagation, and the user
approved the intended method. This is an evidence-based recipe preference, not an
API replacement, universal optimization rule or authorization to alter geometry.

The scope-body list getter limitation (-1 without rollback), host/sign-branch
constraints, absence of saved CAD/screenshots and physical print qualification
remain explicit. Approval covers no parapet, unequal-height, other side modes,
railings, ground-floor behavior, other hosts or arbitrary parameter combinations.
Future released-recipe geometry changes require a new candidate/version and checks.

Record the user's next priorities in order: remaining balcony configurations;
front parapets and railings; Building Dataset Analyzer; Building Process Learning.
Analyzer and learning are planned capabilities, not subsystems established by
this decision. No implementation of those priorities is authorized in this session.

Promotion changed only metadata, test expectation and docs. Fresh **165/0** non-CAD
checks and regression rebuild passed (existing MSB3305 warnings). No SolidWorks or
Git operations; executor geometry unchanged. Earlier BA-004/BA-006 Testing status
is historical for this candidate; broad assembly status is not superseded.

## BDA-001 — Silent building evidence and process-learning module (2026-11-04)

Status: **user-requested future design; not implemented or executed**.
Adopt [BUILDING_DATASET_ANALYZER.md](BUILDING_DATASET_ANALYZER.md) as the module's
proposed architecture and staged pilot. Building Dataset Analyzer extracts and
correlates evidence; Building Process Learning derives reusable workflows and
evaluates alternatives. Keep both separate from the active balcony implementation.
Priority order: remaining balcony configurations -> front parapets and railings ->
Building Dataset Analyzer -> Building Process Learning. The learning track later
supports the image-to-building MVP and Golden Dataset; it does not displace working
geometry or establish those future systems by documentation alone.

Silent recordings are sufficient input: **voice narration is never required**.
Combine approximate visible operation intervals with photos, native feature trees,
sketches, dimensions, dependencies, body/component relationships and 3MF/STL geometry.
Keep recording time, CAD dependency order and inferred historical sequence distinct;
final files cannot establish every intermediate operation. A future optional,
non-mutating API event logger writes sidecar evidence and improves clock/operation
alignment, subject to supported API coverage. No logger implementation is selected.

Every component/recipe carries source-building, photo-region, native-entity and
recording-interval provenance with digests/tool versions; unavailable sources stay
explicitly missing. Separate observed facts, inferred intentions and unverified
optimization suggestions. Analyze efficiency, parameterization, edit/rebuild
robustness and profile-specific manufacturing. Propose alternatives rather than
copying historical workarounds; correctness and repeatability outrank feature count.
New recipes are **Draft**. Exact-version promotion requires subsequent applicable
CAD evidence and human approval, never automatic analyzer registration as Validated.

Preserve studio originals read-only and write only to a separate analysis workspace.
Future comparisons use disposable working copies/reconstructed test parts with
frozen inputs/environment/tolerances, not original studio documents. Ask short
human clarification/approval only for necessary unresolved decisions, after the
modeling session; no narration requests or routine modeling interruptions.

Pilot: 3–5 existing building/address folders, photos, native CAD and available 3MF,
plus one silent OBS recording of a new session. Start with synthetic manifest/schema
fixtures, then authorized pilot intake and structured extraction. Fine-tuning is
outside the initial plan. Rationale: preserve native/visual provenance and test
whether useful workflow knowledge can be recovered before investing in training.

This decision documents future boundaries only. No production/project changes,
SolidWorks execution, studio selection/access/analysis, builds/tests or Git operations.
No existing component lifecycle or validation evidence changed. Open extraction API,
version/license, scale/units, timing, review and tolerance choices remain in the plan.

## BDA-002 — Prioritize native CAD Inspector V0.1 over sequential family completion (2026-11-04)

Status: **explicitly user-approved strategic decision; implementation not started**.
Supersedes BDA-001 and BA-007's balcony/family-first scheduling and BDA-001's
3–5-building multimodal initial pilot. Their source-preservation, evidence and
Draft-only proposal rules remain in force. Prioritize **Building Dataset Analyzer
V0.1**, beginning with an independent read-only SolidWorks CAD Inspector, structured
JSON export and a pilot on **three existing studio building projects**.

Rationale: real studio feature/sketch/dimension/body/dependency evidence can reveal
reusable building patterns and modeling alternatives across balcony, window,
railing, stair, roof and other families. Use extracted knowledge to support multiple
family workstreams rather than manually finishing every variation in sequence.
This is a strategic expectation to evaluate, not a claim that the analyzer already
works or that parallel family implementations were produced.

V0.1 extracts native saved-state evidence with explicit source references, units,
coverage, confidence and provenance; missing/unsupported data remains unknown with
reasons. Inspect sketches, dimensions/equations, geometry and solid bodies, feature
dependencies and accessible assembly relationships. Validate structured JSON and
audit extraction/reproducibility across three explicitly chosen projects. Define
the read-only access contract and synthetic fixtures before studio intake. Actual
API/version/configuration support and pilot sources remain to be determined.

Photographs, 3MF/STL, silent OBS recordings, optional API events and more detailed
Building Process Learning are later integrations; they must not block V0.1 or
require narration. Fine-tuning remains out of scope. Assess parameterization,
rebuild/reference robustness and manufacturing alternatives, distinguishing static
observations from predictions; do not mutate source geometry to test hypotheses.
Later reproducible comparisons use separately authorized disposable fixtures.

Keep every discovered recipe **Draft** until the exact version has applicable
geometry/parameter/rebuild/manufacturing validation and required human approval.
No automatic promotion or alteration of current scoped Validated slab/top-face
assembly releases. Preserve the eight-feature baseline, all CAD/non-CAD regression
fixtures and durable evidence. Remaining LEGO families remain planned/incomplete;
this changes priority, not their completion or approval status.

The [V0.1 scope/acceptance criteria](BUILDING_DATASET_ANALYZER.md) and current
ROADMAP closeout are authoritative for next work. This closeout changes only the
three planning documents: no code, new tests/builds, SolidWorks operations, studio
analysis or Git operations. Previously recorded 165/0 regressions and CAD passes
are historical evidence, not rerun checks. No implemented architecture changed.

## Historical change notes

2026-11-03 parameter/integration validation: test each driving dimension in a fresh part. Thickness-only edit preserves the current center-Y constraint; elevation changes calculate center Y explicitly. For the upper-floor door integration, reuse the existing rectangular CreateWindow cut with test-side door semantics rather than modify ground-constrained CreateDoor. Center slab and align top using explicitly calculated threshold coordinates; do not claim a persistent parametric Door-to-Balcony relationship. Rationale: validate the approved slab and actual spatial integration without introducing new production features or hiding current semantics. Six CAD cases passed and 105 automated checks passed; lifecycle promotion awaits human visual review. Evidence: tests/results/BALCONY_EDITS_UPPER_DOOR_2026-11-03.md.

2026-11-03 classifier correction: extract a test-only COM-free type resolver, calling GetTypeName only for ICE and using the existing per-name visited set before resolution/counting. This preserves ordinary feature behavior and avoids double-counting absorbed/wrapped traversal results. Eight focused regressions and the fresh BALCONY rerun confirm the fix; no production change was needed. Baseline numeric CAD PASS does not imply parameter-edit/visual approval or Validated status. Evidence: tests/results/BALCONY_CLASSIFIER_RERUN_2026-11-03.md.

2026-11-03 actual BALCONY validation diagnosis: execute COM checks as the interactive desktop user when sandbox identity cannot see the running object; preserve attach-only behavior. Prefer compiled typed interop over PowerShell dynamic introspection after TYPE_E_ELEMENTNOTFOUND. Feature counting must resolve Instant3D ICE wrappers through GetTypeName before classifying underlying geometry. Rationale: actual slab geometry matched expected measurements, while the test helper falsely undercounted bosses. Preserve the failed result and unsaved Part1; no production change or additional test execution is authorized by this diagnosis. Evidence: tests/results/BALCONY_CAD_2026-11-03.md.

2026-11-03 threshold integration preflight: retain V1's current host-height rule during validation. A ground-door threshold is host elevation 0, so a downward-thickened slab would extend below the wall and is rejected. Do not hide that limitation by raising the slab or substituting BALCONY_MIXED. Test-only placement arithmetic and preflight evidence were added; production changes require a separate decision. See tests/results/THRESHOLD_PREFLIGHT_2026-11-03.md. Rationale: test the requested spatial relationship honestly and preserve working behavior while validation is pending.

2026-11-03 regression-baseline addition: retain unchanged production source and link it into a dependency-free .NET Framework console harness. Use reflection for existing private numeric/bounds helpers rather than refactoring geometry for test access. Keep the manual CAD harness separate and explicitly opted in; never infer geometry PASS from a successful build or case generation. Evidence: tests/README.md and tests/results/BASELINE_2026-11-03.md (53 non-COM checks pass; CAD cases NOT RUN). This is an initial regression seed, not library validation or a full Golden Dataset.

Append decisions or mark them superseded with a replacement reference and reason; preserve history. Cite source or approval evidence. Keep implementation status separate from design acceptance. Update ARCHITECTURE.md and ROADMAP.md when a decision becomes executable capability. Unknown prior testing remains UNKNOWN until evidence is recorded.
