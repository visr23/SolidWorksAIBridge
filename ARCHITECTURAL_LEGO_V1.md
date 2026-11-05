# Architectural LEGO V1

Status: approved architectural direction; specification only.

This document defines a reusable architectural component system for SolidWorksAIBridge. It complements [Architectural Modeling Standard V1](ARCHITECTURAL_MODELING_STANDARD_V1.md), which remains the source of modeling and miniature manufacturing principles. Contracts described here are planned capabilities, not claims that executors, schemas, libraries, or tests already exist.

The next geometry feature is parametric balconies. Existing working wall, window, sill, door, row, and grid implementations must be preserved. This specification does not authorize a broad refactor or implementation of every component.

## 1. System boundaries

```text
Building photographs
  -> AI architectural analysis
  -> corrected 2D elevation / simplified construction plan
  -> Building JSON
  -> schema, semantic, and capability validation
  -> scale and manufacturing resolution
  -> validated manufacturing plan
  -> deterministic SolidWorks execution
  -> geometry verification and execution report
```

AI interpretation identifies architectural intent, evidence, uncertainty, relationships, and candidate templates. It does not issue arbitrary COM calls. The executor accepts resolved, supported operations and must not invent missing architectural dimensions.

The corrected elevation and Building JSON identify the same building revision. Corrections must update structured geometry and relationships; a raster illustration alone is not an executable construction plan. Initial elevations use black lines on white, minimal internal detail, and emphasize envelope, levels, bays, apertures, recesses, projections, and structural divisions.

Build the main mass first, then primary openings, major recesses/projections, secondary elements, repetition, details, and manufacturing parts. Front, side, and rear elevations share floor, sill, head, roof, and structural-band datums.

## 2. Component families and primitives

Every component uses the contracts and lifecycle below, regardless of family.

| Family | Type | Minimum architectural parameters |
| --- | --- | --- |
| Structural | Wall | Width, height, thickness, wall role |
| Structural | Floor | Width, depth, thickness; rectangular slab initially |
| Structural | Roof | Width, depth, thickness, roof form; flat initially |
| Structural | Column | Height and rectangular section or diameter |
| Structural | Beam | Length and section width/height |
| Openings | Opening | Host, width, height, profile, through/pocket behavior |
| Openings | Window | Aperture plus optional frame, sill, glazing representation |
| Openings | Door | Aperture plus optional leaf/frame and threshold elevation |
| Architectural | Balcony | Variant, width, depth, slab thickness, optional side walls and railing; recess opening height/back wall where applicable |
| Architectural | Railing | Path, height, member thickness, post spacing, infill mode |
| Architectural | Stairs | Width, step count, rise, run, direction, optional landings and railings |
| Architectural | Recess | Host, profile, inward depth |
| Architectural | Projection | Host, profile, outward depth |
| Architectural | Cornice | Path, section width/height, projection |
| Secondary details | ACUnit | Width, height, depth, attachment |
| Secondary details | Pipe | Path, diameter, optional wall thickness |
| Secondary details | Shutter | Width, height, thickness, optional simplified slats |
| Secondary details | Awning | Width, projection, thickness, slope |

Wall roles include external_facade, rear_facade, side_facade, party_wall, and connection_wall. Context can justify a plain party wall; the AI must not invent openings.

Geometric primitives are executor-neutral recipes: rectangle/circle/polygon profiles, paths, slab/box/cylinder solids, boss extrusion, scoped cut, reference plane, geometric constraints, offset, linear pattern, and mirror. Sweeps and richer profiles are later capabilities. Architectural types add meaning, placement rules, and manufacturing behavior to these primitives.

### 2.1 Dedicated Stairs contract

Stairs V1 represents a straight flight, not unrelated manually placed boxes:

- dimensions.width: flight width.
- parameters.stepCount: positive integer N, the number of rises.
- dimensions.rise: vertical rise per module.
- dimensions.run: horizontal advance per module; corresponds to tread advance in the modeling standard.
- parameters.directionXZ: normalized two-number horizontal direction in the component frame.
- Total rise = N * rise; total flight run = N * run.
- Optional start/end landings specify length and thickness; landing lengths are additional to flight run.
- Optional left/right railings reference the reusable Railing recipe.
- A master step and linear repetition express uniform steps. Solid construction may require a supporting base or a different solid recipe; disconnected floating steps must fail connectivity checks.

The origin is the lower walking level at the flight start. An end landing starts at the flight endpoint and top elevation. Direction is transformed by component rotation. Turning stairs and intermediate landings are future composites of flights and landings. This explicit module convention avoids ambiguity about whether the top landing counts as a tread.

## 3. Component Definition contract

A definition is reusable architectural knowledge, independent of a particular building.

| Field | Contract |
| --- | --- |
| definitionId | Stable namespaced identifier |
| version | Immutable semantic version |
| family / type | Architectural classification |
| status | Draft, Testing, or Validated |
| parameterSchema | Typed dimensions/options, units, constraints, defaults, supported ranges |
| anchors | Named coordinate frames and attachment surfaces |
| recipe | Primitive operations or version-pinned child definitions and parameter bindings |
| dependencies | Required roles, hosts, datums, definitions, and executor capabilities |
| manufacturingRules | Thickness, grouping, clearance, compression, and representation rules |
| validationProfile | Required checks, tolerances, fixtures, supported environments |
| approvalPolicy | Conditions requiring human visual approval |
| validationRecordIds | Evidence supporting the exact version |
| provenance | Author/source and creation history |

Recipes are declarative and restricted to supported operations; an AI-generated recipe is not unrestricted executable code. Defaults are normalized explicitly. Parameter relations must remain meaningful when dimensions are edited.

## 4. Component Instance contract

An instance is one element in one building and references an exact definition version.

| Field | Contract |
| --- | --- |
| id | Unique stable ID within the building, including generated IDs |
| definitionId / definitionVersion | Exact reusable definition reference |
| type | Must agree with the referenced definition |
| parentId | Owner and local coordinate frame; building ID for root components |
| floorId | Associated datum ID, or null for spanning/unassigned components |
| placement | position[3] and rotationDegXYZ[3] in the parent frame |
| dimensions | Real-world architectural dimensions required by the type |
| parameters | Type-specific options, hostId, relationship references, and recipe choices |
| dependsOn | Additional construction prerequisite IDs |
| manufacturing | Classification, print group, merge policy, optional actual-mm overrides |
| evidence | Optional source, confidence, image references, and dimension provenance |

Evidence source is directly_observed, inferred_from_pattern, or estimated; confidence is in [0, 1]. Depth provenance additionally distinguishes measured, inferred, and stylized/printable depth. Omitted evidence means unspecified, never directly observed by default.

Ownership and hosting are distinct: parentId establishes the frame and composition; parameters.hostId identifies geometry affected by a cut or attachment. A cut inherits its target's manufacturing unit and creates no printable body of its own.

Instance validity is separate from definition status. A Validated definition does not certify every parameter combination, placement, host, printer configuration, or building arrangement.

## 5. Validation Record contract

A validation record is an immutable, traceable result, not a status assertion by the AI.

| Field | Contract |
| --- | --- |
| id / timestamp | Record identity and execution time |
| subject | Definition/version and digest; optionally building revision and instance ID |
| environment | Resolver/executor versions, SolidWorks version, manufacturing profile |
| inputs | Fixture IDs, exact parameters, normalized-input digest, dependency versions |
| checks | Parameter, dependency, geometry, regression, and manufacturing results |
| expectations | Expected values and explicit numeric/visual tolerances |
| artifacts | Reports, screenshots, geometry measurements, optional CAD outputs and digests |
| approval | Required/not required, reviewer identity, outcome, reviewed artifact digest |
| outcome | Passed, failed, or incomplete, with diagnostics |

Changing the recipe, relevant dependency, environment, parameters, or reviewed artifact cannot reuse evidence as though nothing changed. Compatibility policy determines which checks must be rerun; the record remains tied to its original inputs.

## 6. Universal Architectural Component Lifecycle

Every architectural LEGO component supports the same extensible workflow:

1. Recognize elements in architectural images, retaining evidence and uncertainty.
2. Classify them into an architectural family and component type.
3. Match and parametrically adapt existing validated templates within supported ranges.
4. Compose existing geometric primitives when no suitable template exists.
5. Propose new component definitions when available primitives are insufficient.
6. Run automated geometry, parameter, dependency, regression, and manufacturing validation.
7. Obtain human visual approval when required by the approval policy.
8. Register the exact validated version in the reusable validated component library.

```text
Draft -> Testing -> Validated
```

- Draft permits incomplete proposals, schemas, and recipes.
- Testing means required checks and reviews are in progress.
- Validated requires passing applicable checks and recorded required approvals.
- Failed or incomplete checks prevent promotion. Reworking a Testing candidate returns it to Draft with a new candidate revision/digest.
- Editing a released Validated definition creates a new Draft version; the old version remains immutable.
- AI must never directly register untested generated geometry as Validated.
- Human approval supplements automated checks; it does not replace them.

The same lifecycle applies to all present and future component families. A new definition can express a new composition without requiring a new SolidWorks operation. If a new operation is necessary, its executor implementation and tests are prerequisites for validation.

## 7. Versioning and reusable library rules

- Use stable namespaced IDs and exact semantic versions. Breaking contract/anchor/meaning changes increment major; compatible added capabilities increment minor; compatible corrections increment patch. Every changed release still requires applicable validation.
- Building schema version, definition version, building revision, and executor version are distinct identifiers.
- Pin child definition versions and supported primitive/executor capabilities. Do not silently select latest versions during reproduction.
- Candidate storage may contain Draft and Testing entries. The validated library exposes only eligible Validated releases with evidence.
- Promotion is a controlled operation checking records and approval policy; generation alone cannot grant eligibility.
- Preserve released definitions and records. Withdrawn/deprecated versions retain history and explicit eligibility metadata; status progression is not a substitute for withdrawal policy.
- Persist a definition lock manifest/digests with an execution report so the same building can be reconstructed.
- Existing working geometry is not automatically labeled a Validated library definition. Capture adapters, supported ranges, and baseline evidence first.

## 8. Building JSON and coordinate conventions

### 8.1 Coordinates and units

- Right-handed X_RIGHT_Y_UP_Z_OUT: X right in the front elevation, Y up, Z outward toward the viewer. Building depth extends toward negative Z.
- Architectural dimensions and positions use real-world millimeters. Manufacturing settings ending in Mm use actual miniature millimeters. Angles use degrees.
- placement.position is relative to the parent's anchor/frame. Floor association does not add a hidden translation.
- rotationDegXYZ uses active right-hand rotations about successive local X, Y, Z axes. For column vectors, R = Rx * Ry * Rz; local point maps to parent as p = translation + R * point. Parent transforms compose outward to the building frame.
- The executor converts resolved miniature millimeters to SolidWorks meters exactly once.

| Type | Local anchor |
| --- | --- |
| Wall | Bottom center of exterior face; thickness extends inward (-local Z) |
| Opening, Window, Door | Bottom center of aperture on host exterior face |
| Floor, flat Roof | Center of top surface; thickness extends downward |
| Balcony | Center of slab rear edge at walking surface; projection extends +local Z |
| Stairs | Center of flight start at lower walking level |
| Column | Base center |
| Beam | Start centerline; length along +local X |
| Railing, Pipe, Cornice | Start of local path |
| Recess, Projection, other attached details | Center of host attachment footprint; inward/outward depth follows host normal |

Side/rear walls use transformed local frames. Mirroring is an explicit operation, not a negative scale or undocumented rotation. Symmetry uses the true architectural axis, with a dedicated plane when required.

### 8.2 Structural schema contract

The following is the schema design; a machine-readable JSON Schema is a Phase 1 deliverable, not an existing file.

```text
Building:
  schemaVersion = "ARCHITECTURAL_LEGO_V1"
  id, revision: nonempty strings
  units = "mm"
  coordinateSystem = "X_RIGHT_Y_UP_Z_OUT"
  manufacturing: ManufacturingSettings
  floors: FloorDatum[]
  components: ComponentInstance[]
  optional sources, elevationReferences, sharedDatums, extensions

FloorDatum:
  id: nonempty string
  elevation: finite number in building coordinates
  height: positive number

ComponentInstance:
  required: id, definitionId, definitionVersion, type, parentId,
            floorId, placement, dimensions, parameters,
            dependsOn, manufacturing
  placement.position: exactly 3 finite numbers
  placement.rotationDegXYZ: exactly 3 finite numbers
  dimensions/parameters: discriminated by type and definition schema
```

Use JSON Schema Draft 2020-12 with closed objects, except reserved extensions. Counts are integers; dimensions are positive unless explicitly allowed to be zero; JSON cannot contain NaN or infinity. Normalize defaults explicitly. Semantic validation handles references, cycles, containment, geometry, and executor support beyond JSON Schema.

A structurally valid document may describe unimplemented types. It becomes executable only after definition resolution, semantic validation, manufacturing resolution, and capability checks all succeed.

## 9. Composite components and construction dependencies

Store instances in a flat array with parentId; do not duplicate children in nested arrays. Definitions expand recipes into deterministic generated IDs such as balcony-01/slab. Named child roles can be explicitly overridden, but duplicate role ownership must be rejected. A Window or Door generates one aperture; an explicit aperture child replaces that role rather than causing a second cut.

```text
Balcony
  slab: Floor
  leftReturn / rightReturn: Wall (optional)
  railing: Railing (optional)
  facadeAperture: Opening (recessed)
  backWall: Wall (recessed)
```

Parent-frame resolution, host references, child recipes, datums, and dependsOn produce a directed dependency graph. Reject cycles and missing references before geometry execution. A composite's completion depends on its children; the children's parent-frame dependency must not be mistaken for a cycle in feature construction.

Typical operation order: datums/frames -> host bodies -> apertures/recesses -> slabs/returns/projections -> details/railings -> repetition -> verification. Use topological ordering with stable ID ordering for independent operations. Dependency requirements take precedence over broad feature categories.

Repeated elements are classified as Unique, RepeatedVertical, RepeatedHorizontal, or RepeatedGrid. Prefer a master and Linear Pattern/Mirror for new repeated geometry. Propagate related components together, including balcony apertures and railings. Do not impose repetition without evidence. Repetition contract details are finalized before repeated-balcony execution.

### 9.1 BalconyAssembly V1 — approved side-wall rules, Draft composite

Scoped release update (2026-11-04): the separate optimized definition
**lego.balcony.projecting-assembly.equal-height-top-face 1.0.0**, recipe
**top-face-shared-r1**, is **Validated only within its recorded sampled host/cases**
after numeric CAD comparison and explicit user approval of Part3. One slab-top
shared sketch plus one upward merging boss is the preferred equal-height
two-side-wall recipe within that scope. See [exact approval and limits](tests/results/TOP_FACE_VALIDATED_2026-11-04.md).
The broader assembly definition and eight-feature implementation retain their
own Testing status/baseline; parapets, unequal heights, other side modes, railings
and all other untested configurations are not promoted. Earlier implementation
and NOT RUN statements below retain their historical evidence dates.

Definition ID: **lego.balcony.projecting-assembly**, version **1.0.0**, lifecycle **Testing** for the first both-wall implementation. Code and 36 new non-CAD tests now exist; actual assembly CAD remains **NOT RUN**. Other side modes remain planned. See [implementation and approval-gated CAD case](tests/BALCONY_ASSEMBLY_V1.md). This new composite remains separate from scoped Validated lego.balcony.projecting-slab 1.0.0, whose geometry/behavior/evidence are unchanged. Design/validation steps below retain the broader plan; they are not claims that every mode/test has been implemented or run.

#### Component contract and dependencies

The assembly owns a slab child and optional left/right Wall children with stable roles/IDs: assembly-id/slab, assembly-id/leftWall, assembly-id/rightWall. Resolve child role names into safe executor names without requiring a slash in the current slab instance ID. Pin the slab definition version; preserve its host and unit constraints. Reusing a validated child does not validate the composite or extend the child's tested parameter domain.

Use the existing common definition/instance/validation contracts. Assembly instance parameters include slab dimensions/placement/host, side mode **none | left | right | both**, and each enabled side's independently configurable height and thickness. Slab and walls retain independently editable named parametric features even when their resulting solids are united. Disabled sides create no geometry. No railings, recesses or repetition are included in this increment.

Assembly local origin is the slab rear-edge center at its top surface, as in the balcony convention. X is horizontal, Y up, Z outward. For slab width W, depth D and thickness T:

```text
slab:      X [-W/2, W/2], Y [-T, 0], Z [0, D]
leftWall:  X [-W/2, -W/2+tL], Y [0, hL], Z [0, D]
rightWall: X [W/2-tR, W/2],   Y [0, hR], Z [0, D]
```

This is the proposed minimal rectangular recipe: wall thickness lies inside the slab's outer width, preserving the slab footprint. Heights are measured above the slab top. Heights/thicknesses may differ by side and are independent of slab thickness/depth. Resolve architectural dimensions to actual miniature mm before execution; printer minimums are actual-mm profile values.

Parent/frame and host dependencies resolve first, then slab, then each enabled wall's scoped merging Boss-Extrude, then checks. **Wall depth is derived from and persistently bound to slab depth**, never an independently writable duplicate. Side-wall rear boundaries coincide with the exterior facade plane (local Z=0); their front boundaries coincide with the slab front edge (Z=D). Maintain both conditions after depth edits through CAD relationships/equations or stable references, not merely equal initial numbers. Document any unsupported direction of dependency editing rather than allowing a wall-depth edit to break the assembly.

#### Approved default geometry and manufacturing behavior

1. Side walls begin flush at the exterior facade plane with **zero gap**; do not apply assembly clearance to this interface by default.
2. Enabled walls extend all the way to the slab front edge. Slab-depth edits update wall depth parametrically.
3. Side-wall height and thickness remain independently configurable; support none, left-only, right-only and both.
4. **Default: slab plus enabled side walls form one connected printable solid body.** Share one assembly printGroupId with explicit within-group union. Keep that entire balcony body **separate from the main facade body**, even where their faces touch.
5. Maintain separate editable slab/left/right features and dimensions; solid-body union does not mean flattening the feature history into an uneditable imported solid.
6. Preferred print orientation places the **slab underside flat on the build plate**, with side walls extending upward. In this architectural frame the underside is Y=-T and build direction is +Y; manufacturing orientation maps +Y to printer +Z and places the underside at printer Z=0. Do not change architectural placement to encode print orientation. This is a preference, not a universal support-free printability claim.
7. Validate connectivity, printable thickness, intended merge scope, independent dimensions, dependent depth edits and rebuilds before lifecycle promotion.

These assembly-specific rules refine generic body separation by manufacturing unit. The facade is a different manufacturing unit and must never be included in the balcony union. They do not change the standalone slab's separate-body contract.

#### Proposed minimal implementation

In a separately authorized implementation increment:

1. Add a small COM-free assembly specification/preflight and explicit child bindings/print group. Validate IDs/host, side mode, finite positive dimensions, profile minimum thickness, placement/clear opening, and enabled-side width usage (tL+tR < W when both sides exist).
2. Invoke the existing slab path unchanged on a fresh supported host. Add each side wall as a separately named **standard Boss-Extrude with Merge Result enabled**, resting on the slab with shared rear/front depth references. Where supported, set merge scope / Selected Bodies explicitly to **only the balcony slab body**, or the current merged balcony body when adding the next wall. Exclude MainWall. Resolve the current assembly body after each feature/rebuild instead of assuming an old body reference remains valid. Do not use indiscriminate automatic selection of touching bodies. If target isolation cannot be guaranteed, stop and report the API limitation before changing strategy.
3. **No separate Boolean Combine is needed when the Boss-Extrude merges successfully.** This user clarification supersedes the earlier isolated-wall-bodies-plus-Combine proposal. With no walls, retain the slab alone. Preserve the independent slab and side-wall sketches/Boss-Extrudes and their editable dimensions. After each enabled wall and final rebuild, require exactly **two solid bodies**: MainWall and one connected BalconyAssembly. Confirm actual wall-to-slab connectivity and unchanged facade geometry/volume; count alone is insufficient to prove correct membership or connectivity.
4. Use separate assembly validation records and regression fixtures; no automatic promotion based on the slab's existing status.

#### Proposed validation matrix and reference fixture

Use the validated host/slab baseline as the initial CAD fixture: host 180 x 220 x 3, slab W=56, D=24, T=3, X center 20, top host elevation 120. Proposed test-only side settings: hL=hR=20, tL=tR=2 actual mm; these are fixture values, not printer defaults. Test all four side modes in fresh documents.

For enabled sides, expected balcony volume is W*D*T + D*(tL*hL + tR*hR), omitting disabled terms. Walls meet the slab by full-area face contact, with no duplicate solid volume. Expected balcony volumes: none **4032**, one side **4992**, both **5952 mm^3**. Plain facade volume stays **118800 mm^3**; final body count is **2** in every mode. Left/right walls have local Z [0,24], with rear on the facade and front on the slab edge.

The merging Boss-Extrude recipe produces proposed generated feature totals including sketches: none **4**, one side **6**, both sides **8** (host 2 + slab 2 + two features per enabled wall; **no Combine feature**). These replace the earlier 4/7/9 extrusion-plus-Combine estimates. They remain prospective expectations to confirm when implemented, not measured outcomes.

Automated preflight tests should cover all modes, invalid IDs/hosts/dimensions, enabled/disabled-side rules, printer minimums, excessive wall thickness, deterministic child bindings and expected bounds/volume. CAD tests must measure rear gap=0, front alignment, height/thickness, connectivity, volume, body/feature counts and facade isolation. Verify Merge Result and selected-body scope on each side-wall Boss-Extrude, one connected balcony solid and an independent unchanged MainWall after rebuild, and no added Combine feature. Edit slab depth 24->26 and verify wall fronts move together while heights/thicknesses remain unchanged; expected balcony volumes become 4368/5408/6448 mm^3 for none/one/both. Also edit each wall's height/thickness independently and rebuild, checking collisions, minimum thickness and final connectivity. Never claim physical printer qualification from CAD-only checks.

Run the unchanged slab baseline/regressions as compatibility checks. Capture standard views and underside/build-direction evidence, then obtain human visual approval before Draft -> Testing -> Validated promotion. All assembly tests remain **NOT RUN**.

## 10. Miniature manufacturing resolution

Never overwrite real-world dimensions with printable dimensions. Produce a separate resolved plan recording, per dimension: architectural value, nominal scaled value, actual manufacturing value, adjustment reason, and provenance.

Resolution order:

1. Uniform architectural scale: miniature length = architectural length / scaleDenominator.
2. Selective building-depth compression.
3. Explicit printable thickness/representation overrides.
4. Declared mating-interface clearances.
5. Final thickness, collision, connectivity, and body-scope checks.

| Actual miniature setting | Standard default |
| --- | --- |
| wallThicknessMm | 3 mm |
| internalWallThicknessMm | 2 mm |
| minorProjectionMm | 0.8 mm |
| mediumProjectionMm | 1.2 mm |
| assemblyClearanceMm | 0.2 mm |
| minimumFeatureThicknessMm | Required manufacturing-profile choice; no universal default |
| Roof thickness | Part-specific; no standard default |

Use role-based global variables in new geometry. Settings are miniature conventions, not universal architectural dimensions. Explicit instance actual-mm overrides take precedence over profile settings; otherwise dimensions scale normally. Every override is reported.

- Never scale actual-mm thicknesses or clearances again.
- Below-minimum dimensions cause errors by default; optional thickening requires a declared policy and recorded adjustment.
- Apply clearances only at declared joints, with explicit direction. An inserted roof profile shrinks to provide clearance. Assembly clearance is independent of opening-cut tolerance even if both are 0.2 mm.
- Use controlled printable thickness instead of unnecessary solid mass. Prefer surface/body-driven termination and stable coincident/converted references when they express real dependencies.
- Thin-feature line directions and thickness offsets must remain consistent.
- manufacturing.class is main_print, separate_print, post_process_part, or ignore.
- printGroupId identifies a manufacturing unit. mergePolicy is never or within_group; within_group permits only explicitly targeted merges, never indiscriminate merging of touching geometry.
- A group may contain multiple bodies; check connectivity where a single printable unit is required. Cuts explicitly identify target bodies and must not affect unrelated bodies.
- Separate canopies, roofs, railings, or other parts when orientation/support/painting/assembly requires it. Favor unobtrusive seam locations and compensate for adjacent wall thickness in opening placement.

### 10.1 Facade-only mode and depth compression

mode is full_building or facade_only. facadeIds selects retained facade components. Retain their hosted elements and declared required dependencies; omitted interiors/rear elements cannot remain dangling references. Returns/backing must be explicitly specified, not invented.

sideDepthFactor is in (0, 1]. Compress annotated building-depth spans and side-wall runs, preserving front-facing X/Y scale and proportions. This is semantic resolution, not a blanket nonuniform transform. Balcony projection, facade relief, member sections, circular pipes, and printable thicknesses retain their intended dimensions by default. compressFacadeProjections is an explicit opt-in affecting eligible depth dimensions only. Report the final affected dimensions and revalidate interfaces. Detailed depth-domain rules must be locked before multi-elevation execution.

Separate genuinely different recess depths and feature chains. New evidence may revise inferred depth and regenerate affected descendants; preserve the earlier evidence/revision history. Do not compensate for an incorrect shared depth with unnecessary filler solids.

## 11. Existing functionality and operation mapping

Current public APIs include CreateWall, CreateWindow, CreateWindowSill, CreateWindowRow, CreateWindowGrid, CreateDoor, and CreateDoorRow. Public lengths are millimeters; the existing connector attaches to a running SolidWorks instance. The existing program demonstrates a wall and multi-door row, not Building JSON execution.

| LEGO operation | Reuse or planned executor |
| --- | --- |
| Wall | Existing CreateWall for its supported front-facing single-wall case; adapter converts bottom-face anchor to centered wall coordinates |
| Rectangular Window / Opening | Existing CreateWindow for compatible facade apertures; adapter converts bottom to center coordinates |
| Ground Door | Existing CreateDoor / CreateDoorRow when bottom matches wall bottom |
| Elevated Door | New generic aperture path; preserve ground-door semantics |
| Window rows/grids and sills | Existing APIs where compatible; preserve shared-sketch implementation |
| Floor / flat Roof / Beam / Column | New profile extrusion recipes |
| Balcony | New composite slab, returns, railing, and optional aperture/back wall |
| Recess / Projection | New body-scoped cut/boss depth operations |
| Stairs | New master-step/repetition recipe, supporting geometry, landings, railings |
| Railing | New member extrusions and repeated posts |
| Cornice / secondary details | Simplified extrusions initially; richer paths/sweeps later |

Current helpers are facade-specific, the modeler tracks one wall, and overlap checks use 2D bounds. Do not silently extend those assumptions to arbitrary planes or multi-body geometry. Add narrow adapters and new scoped paths instead of refactoring working methods. Multiple independent unique windows may also require a new path because existing standalone names are fixed; preserve existing naming behavior.

A deep recessed balcony cannot be a deep pocket in a thin wall: cut a facade aperture and construct an inward slab, returns, and back wall with independent depth. Do not reuse wall-through-cut depth rules for a controlled shallow recess.

## 12. Automated validation and Golden Dataset

Run non-COM validation before connecting to SolidWorks. Reject unsupported operations before mutation. Preserve failed CAD results for diagnosis according to existing behavior; do not imply transactional rollback exists.

| Validation layer | Required checks |
| --- | --- |
| Schema/parameters | Required fields, units, types, finite ranges, counts, definition agreement |
| Dependencies | Unique IDs, valid hosts/parents/floors, recipe roles, no cycles, pinned versions |
| Architectural | Aperture containment, shared datums, stair endpoints, relationship/repetition consistency |
| Manufacturing | Actual thickness, clearances, compression effects, print groups, targeted merges/cuts |
| Geometry | Measured bounds, extrusion direction/depth, nonzero solids, intended connectivity/body count, no unintended intersections |
| Parametric | Edit width/depth/elevation and rebuild; dependent parts follow correctly |
| Regression | Existing wall, window, sill, rows/grids, and doors retain expected behavior |
| Visual | Standard views and required human approval tied to exact artifacts |

Golden Dataset cases contain versioned Building JSON, definition locks, manufacturing profile, expected dimensions/body counts, tolerances, standard-view images, and environment metadata. CAD files may be stored as supporting artifacts; unstable COM IDs must not be the comparison oracle. Screenshot differences supplement numeric checks and use declared visual tolerances.

Include positive, boundary, invalid, and mixed-feature cases. Initial balcony cases: projecting slab, complete projecting balcony, recessed depth exceeding facade thickness, row/stack, parameter edits, collisions, and cut-scope isolation. Record expected preflight failures and ensure they occur before mutation. Geometric fixtures seed reusable definition validation; image/elevation fixtures later cover interpretation quality separately from executor correctness.

An existing implementation is not proof of a completed test suite. Establish and record baselines before claiming library validation. Promotion requires all applicable checks plus any required approval; failures or missing evidence block registration.

## 13. Minimal two-floor Building JSON example

This illustrative facade-only building includes one spanning wall, windows, doors, entrance stairs, and a projecting balcony. Referenced definition versions are proposed identifiers, not claims of existing Validated library entries. The example becomes executable only after those definitions and capabilities exist. The 0.8 mm minimum is a fixture choice, not a universal standard. Repeated windows use a common definition but remain explicit instances here; no repetition syntax is implied.

```json
{
  "schemaVersion": "ARCHITECTURAL_LEGO_V1",
  "id": "building-01",
  "revision": "1",
  "units": "mm",
  "coordinateSystem": "X_RIGHT_Y_UP_Z_OUT",
  "manufacturing": {
    "scaleDenominator": 50,
    "mode": "facade_only",
    "facadeIds": ["front-wall"],
    "sideDepthFactor": 0.4,
    "compressFacadeProjections": false,
    "wallThicknessMm": 3,
    "internalWallThicknessMm": 2,
    "minimumFeatureThicknessMm": 0.8,
    "assemblyClearanceMm": 0.2,
    "belowMinimumPolicy": "error"
  },
  "floors": [
    { "id": "ground", "elevation": 0, "height": 3000 },
    { "id": "upper", "elevation": 3000, "height": 3000 }
  ],
  "components": [
    {
      "id": "front-wall",
      "definitionId": "lego.wall.rectangular",
      "definitionVersion": "1.0.0",
      "type": "Wall",
      "parentId": "building-01",
      "floorId": null,
      "placement": { "position": [0, 0, 0], "rotationDegXYZ": [0, 0, 0] },
      "dimensions": { "width": 9000, "height": 6000, "thickness": 250 },
      "parameters": { "role": "external_facade" },
      "dependsOn": [],
      "manufacturing": { "class": "main_print", "printGroupId": "facade", "mergePolicy": "within_group" }
    },
    {
      "id": "ground-window",
      "definitionId": "lego.window.rectangular",
      "definitionVersion": "1.0.0",
      "type": "Window",
      "parentId": "front-wall",
      "floorId": "ground",
      "placement": { "position": [-2500, 900, 0], "rotationDegXYZ": [0, 0, 0] },
      "dimensions": { "width": 1400, "height": 1500 },
      "parameters": { "hostId": "front-wall", "representation": "opening_only" },
      "dependsOn": [],
      "manufacturing": { "class": "main_print" }
    },
    {
      "id": "upper-window",
      "definitionId": "lego.window.rectangular",
      "definitionVersion": "1.0.0",
      "type": "Window",
      "parentId": "front-wall",
      "floorId": "upper",
      "placement": { "position": [-2500, 3900, 0], "rotationDegXYZ": [0, 0, 0] },
      "dimensions": { "width": 1400, "height": 1500 },
      "parameters": { "hostId": "front-wall", "representation": "opening_only" },
      "dependsOn": [],
      "manufacturing": { "class": "main_print" }
    },
    {
      "id": "entrance",
      "definitionId": "lego.door.rectangular",
      "definitionVersion": "1.0.0",
      "type": "Door",
      "parentId": "front-wall",
      "floorId": "ground",
      "placement": { "position": [0, 0, 0], "rotationDegXYZ": [0, 0, 0] },
      "dimensions": { "width": 1200, "height": 2200 },
      "parameters": { "hostId": "front-wall", "representation": "opening_only" },
      "dependsOn": [],
      "manufacturing": { "class": "main_print" }
    },
    {
      "id": "entrance-stairs",
      "definitionId": "lego.stairs.straight",
      "definitionVersion": "1.0.0",
      "type": "Stairs",
      "parentId": "building-01",
      "floorId": "ground",
      "placement": { "position": [0, -450, 1500], "rotationDegXYZ": [0, 0, 0] },
      "dimensions": { "width": 1500, "rise": 150, "run": 300 },
      "parameters": {
        "stepCount": 3,
        "directionXZ": [0, -1],
        "construction": "solid",
        "landings": [{ "location": "end", "length": 600, "thickness": 150 }],
        "railings": { "left": false, "right": false },
        "connectsToId": "entrance"
      },
      "dependsOn": ["entrance"],
      "manufacturing": { "class": "separate_print", "printGroupId": "entrance-steps", "mergePolicy": "within_group" }
    },
    {
      "id": "balcony-door",
      "definitionId": "lego.door.rectangular",
      "definitionVersion": "1.0.0",
      "type": "Door",
      "parentId": "front-wall",
      "floorId": "upper",
      "placement": { "position": [2000, 3000, 0], "rotationDegXYZ": [0, 0, 0] },
      "dimensions": { "width": 1200, "height": 2200 },
      "parameters": { "hostId": "front-wall", "representation": "opening_only" },
      "dependsOn": [],
      "manufacturing": { "class": "main_print" }
    },
    {
      "id": "balcony-01",
      "definitionId": "lego.balcony.rectangular",
      "definitionVersion": "1.0.0",
      "type": "Balcony",
      "parentId": "front-wall",
      "floorId": "upper",
      "placement": { "position": [2000, 3000, 0], "rotationDegXYZ": [0, 0, 0] },
      "dimensions": { "width": 2800, "depth": 1200, "slabThickness": 150 },
      "parameters": {
        "variant": "projecting",
        "accessOpeningId": "balcony-door",
        "sideWalls": { "left": false, "right": false },
        "railing": { "edges": ["left", "front", "right"], "height": 1000, "memberThickness": 50, "postSpacing": 700 }
      },
      "dependsOn": ["balcony-door"],
      "manufacturing": { "class": "separate_print", "printGroupId": "balcony-01", "mergePolicy": "within_group" }
    }
  ]
}
```

Nominal front facade size is 180 x 120 mm at 1:50; manufacturing wall thickness is 3 mm rather than the nominal 5 mm. Balcony width/depth remain 56/24 mm, slab thickness 3 mm, and railing members 1 mm. The steps rise from -450 to 0 architectural mm; their 900 mm flight and 600 mm end landing reach the entrance at Z = 0. These front-associated projection dimensions are preserved by the example's compression policy. Declared joints must be resolved separately before manufacturing execution.

## 14. Phased implementation roadmap

| Phase | Deliverables and exit criteria |
| --- | --- |
| 1: LEGO architecture and Building JSON | Implement schema, typed contracts, lifecycle records, semantic validator, manufacturing resolver, dependency planner, dry-run output, and narrow legacy adapters. Establish Golden Dataset baselines. No need to execute every declared type. |
| 2: Parametric balconies | Next geometry feature: projecting slab -> side walls/basic railing -> recessed enclosure -> repeated arrangements. Validate editable parameters, propagation, target-body scope, printable separation, and legacy regressions. |
| 3: Facade recesses and projections | Reusable scoped depth operations, independent depth zones, thickness and collision checks; validate representative fixtures. |
| 4: First end-to-end image-to-building MVP | Photos -> AI analysis -> corrected elevation -> validated Building JSON -> supported SolidWorks model. Restrict initial geometry to the validated subset; record evidence, uncertainty, corrections, and verification. Prioritize this over expanding individual features. |
| 5: Additional components and geometry refinement | Stairs executor, remaining structural/details, additional elevations, roof forms, angled geometry, richer paths, and advanced patterns as dataset needs justify. |

## 15. Remaining architectural decisions

Current evidence/approval update: Projecting Balcony Slab V1 is **Validated only for documented tested cases and host constraints**, after actual CAD baseline/edit/integration passes and explicit user visual approval. The [scope record](tests/results/PROJECTING_SLAB_V1_VALIDATED_2026-11-03.md) is authoritative; the implementation note below describes an earlier stage. Top-preserving thickness growth and ground-floor thin-slab/door-leaf clearance remain future, separately versioned capabilities.

Implementation note (2026-11-03): the first narrow executor is now Projecting Balcony Slab V1 (lego.balcony.projecting-slab 1.0.0, Testing). It implements resolved-miniature instance/host/preflight contracts and unmerged front-wall extrusion, not the full schema/library/composite pipeline. See [implementation and CAD cases](tests/PROJECTING_BALCONY_SLAB_V1.md). Pure tests/builds pass; actual CAD remains NOT RUN. Other component mappings above remain future work.

The approved contracts establish direction; the following details require explicit choices during their relevant phase:

1. Phase 1: exact machine-readable schema, parameter-binding syntax, child-role override syntax, repetition records, and shared-datum references.
2. Phase 1: library storage, promotion authority, approval-policy conditions, withdrawal process, and supported environment compatibility matrix.
3. Phase 1: manufacturing profile for the target printer/material, numeric/visual tolerances, and policy for approved minimum-thickness adjustments.
4. Phase 2: balcony assembly interfaces, whether railings share a print unit, explicit clearances, and the initial repeated-body/feature execution strategy.
5. Before multi-elevation execution: depth-domain annotations and interface resolution under side-depth compression.
6. Phase 4: corrected-elevation representation/editor, photo scale calibration, correction synchronization, and the initial Golden Dataset images/building scope.

These decisions do not authorize changing existing behavior by default. Preserve working implementations and add validated capabilities incrementally.

## Opening surround — Draft proposal

Approved proposal, 2026-11-05: **OpeningSurround** is a proposed reusable LEGO
family; **WindowSurround** is its window-specific application. Status **Draft**.
No typed contract, registered definition, geometry recipe, SolidWorks Builder or
surround regression suite exists. Existing window, sill and shared-sketch APIs
remain unchanged. The surround is independent of the aperture cut and optional sill.

### Planned contract

- Stable surround ID, host wall/opening references, attachment frame, outward
  normal and source evidence; explicit dependency and placement bindings.
- Separate inner/outer dimensions or profiles, border width and wall projection.
  For aligned rectangles, outer width = inner width + left/right borders; outer
  height = inner height + top/bottom borders. Declare the driving set, derive
  redundant values and reject inconsistent constraints without resizing the aperture.
- Independently editable top, bottom, left and right profiles, including per-edge
  border widths, sections and projection. Uniform border width may be an explicit
  default binding. Specify corner joins; detect gaps, overlaps and self-intersections.
- Optional sill as a distinct child/reference with independent dimensions and an
  explicit bottom-profile interface. No sill is valid; avoid duplicating existing sills.
- Rectangular initial support; profile-based extension for future nonrectangular
  openings with explicit boundary segments/joins. Unsupported shapes fail pre-CAD.
- Parametric updates propagate intended host/opening dependencies while preserving
  independent controls, unrelated features and sill identity. Exact schema/binding
  syntax and supported ranges remain future implementation decisions.

Retain architectural/relative evidence separately from resolved miniature mm.
Apply the AUE sizing adapter before current APIs; convert to COM meters only at
execution. After scaling, check each profile's narrowest section and corner joins
against the selected manufacturing profile's minimum printable thickness. No
universal numeric minimum is assigned. Below-minimum dimensions fail by default;
explicitly approved thickening records before/after values and preserves the height
lock or reports a conflict. Bounding width/projection alone does not prove thickness.

### Planned Builder and independent validation

Use named constrained profiles and deterministic additive operations, explicit
target bodies and merge policy. Choose separate print body versus selected-host
merge explicitly; contact alone never authorizes merging. Do not enlarge the
aperture or cut unrelated bodies. Corner recipe and supported parameter ranges
must be selected during implementation; this proposal establishes no working recipe.

Independently validate geometry (inner/outer bounds, border widths, projections,
sections, joins, connectivity/body scope), SolidWorks Builder (direction/depth,
merge isolation, rebuilds and independent parameter edits), and regression tests
(new surround cases plus existing wall/window/sill/row/grid/door behavior).
Non-COM fixtures should reject nonfinite/invalid dimensions, inconsistent bounds,
missing hosts, dependency cycles, unsupported profiles, collisions and minimum
thickness failures before mutation. CAD cases must cover asymmetric profiles,
optional/no sill, parameter edits and unrelated-body isolation. Human visual
approval must reference exact artifacts. Keep Draft until these independent gates
support Draft -> Testing -> Validated; visual agreement or focused tests alone
cannot establish full validation. No tests or CAD have run for this proposal.

For [Pilot #001](ARCHITECTURAL_UNDERSTANDING_ENGINE.md), the right-front approximately
2× correction is user visual guidance pending CAD validation, not measurement.
Resolve border width, projection and section separately before assigning dimensions.
