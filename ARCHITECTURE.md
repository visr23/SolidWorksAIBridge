## Approved proportional-sizing correction — 2026-11-04

[AUE Dimension-Driven Proportional Scaling](ARCHITECTURAL_UNDERSTANDING_ENGINE.md)
allows exactly one user-selected Height or Width target in actual miniature mm;
neither is preferred. Lock that selected-scope extent and apply s=target/relative
extent uniformly to architectural geometry. Height runs from lowest architectural
point to highest roof point; width spans the selected scope, excluding context and
display base. Manufacturing and optional explicit depth compression follow scaling
and must preserve the lock or report a conflict. Keep relative source geometry,
uncertainty, target, factor and adjustments traceable. A miniature target does not
establish real-world architectural mm. ADI and Evidence Reconciliation approval is
unchanged. This refines planned design only; runtime architecture is unchanged.

## Approved AUE V1 design boundary — 2026-11-04

[Architectural Understanding Engine](ARCHITECTURAL_UNDERSTANDING_ENGINE.md) now
specifies the planned photo/video understanding stage: source registration,
segmentation, cameras, shared reconstruction, derived orthographic elevations,
adaptive scope/view planning, depth/opening mapping, consistency and Building JSON
compilation. All AUE modules remain **unimplemented**. This addition records an
approved design boundary; it does not establish a changed runtime architecture.

Facade Only, Partial Building and Full Building share one geometric authority;
independent generated elevation images are never authoritative. Views carry
Required/Optional/Not Required/Unknown status. Relative scale and uncertain geometry
remain explicit. Manufacturing/compression derives a separate plan from architectural
geometry under existing LEGO rules. No sections are required in V1.

Planned flow: Photos/Video -> AUE -> Shared Architectural Geometry -> derived
Elevations/optional Top View -> Depth/Relief -> validated architectural Building
JSON -> LEGO Planner -> deterministic Builder -> printable model. Analyzer compares
interpretations with registered CAD/photo/recording evidence; Planner selects recipes
and resolves manufacturing; Builder executes supported operations. Current Analyzer
0.1.0 schemas are not AUE/Building JSON schemas. Existing code, scoped Validated
LEGO definitions and baselines remain unchanged. See the specification for MVP gates.

The older generic analysis/elevation flow below is refined by this design; historical
implementation/test statements retain their original scope and dates.

## Photo-based blockout command — 2026-11-04

Program dispatches `--photo-blockout` through the existing STA connector to
`SolidWorksModeler.CreatePhotoBlockout` in a new partial file. It creates only a
new unsaved part, four unmerged solids and analytic body-count/volume checks.
`--help`/invalid input exit before COM; no arguments preserve the legacy demo.
Profiles are hard-coded assumed miniature-mm photo-proportion concepts, not
image analysis, executable Building JSON, a native Inspector or a saved-part
resume feature. [Setup and boundaries](PHOTO_BLOCKOUT.md);
[actual build/regression/CAD evidence](tests/results/PHOTO_BLOCKOUT_2026-11-04.md).
The historical entry-point descriptions below predate this additive dispatch.

# SolidWorksAIBridge architecture

Evidence baseline: repository inspection on 2026-11-03. Implementation verified here means source exists and was inspected, not that SolidWorks execution passed. Documentation-only audit; no build or CAD execution performed.

## Current implementation

### Independent Building Dataset Analyzer foundation — 2026-11-04

`analyzer/BuildingDatasetAnalyzer.Contracts.csproj` is a separate .NET Framework
4.8 library, not referenced by the production executable/solution, with no COM
reference. It contains an immutable read-only inspection interface/request/result
and pure in-memory schema/consistency validation. Draft-07 JSON schemas define
Building Analysis/Source Manifest 0.1.0 data structures; two synthetic building
pairs and an isolated test executable exercise them. See [contracts](analyzer/README.md).

```text
Repository JSON schemas + synthetic building/manifest fixtures
  -> independent non-CAD test executable
  -> in-memory schema-subset + consistency validator

IReadOnlyCadAdapter (interface only; no implementation/source resolver)
```

The library has no filesystem/SolidWorks access. Tests read repository JSON only;
fixture source paths are never resolved. All candidate components remain Draft.
Saved-state extraction, safe intake, assembly traversal and studio pilot remain
planned. Analyzer build/70 checks and existing regression build/165 checks passed;
these establish contract checks, not extraction accuracy or CAD preservation.
Historical single-project descriptions below describe the earlier architecture.

2026-11-04 additive candidate: `TopFaceBalconyAssembly.cs` supplies equal-height
preflight, X/Z basis checks and footprint dependency equations.
`SolidWorksModeler.TopFaceBalconyAssembly.cs` adds `CreateTopFaceBalconyAssembly`:
unchanged slab creation -> measured slab-top face -> two constrained footprints
in one sketch -> one upward scoped merging boss -> existing body/facade invariants.
No existing geometry API or entry point is replaced. One common height and independent
thicknesses are editable; widths/depths bind to slab dimensions. Only equal-height
both-wall configuration is supported by this recipe. Application, regression and
manual runner builds pass; 165 non-CAD checks pass. Candidate **Testing**, CAD
**NOT RUN**, visual review pending. See [comparison](tests/TOP_FACE_BALCONY_COMPARISON.md).
The historical implementation descriptions below retain their original evidence
dates; the baseline's actual 2026-11-03 CAD pass is recorded in ROADMAP.md.

Latest additive component: BalconyAssembly.cs defines the immutable both-wall instance, side plans and COM-free validation. SolidWorksModeler.BalconyAssembly.cs adds CreateBalconyAssembly, reusing the unchanged slab and creating two scoped merging Boss-Extrudes plus depth/placement equations. It targets only the current balcony body and checks facade isolation, body count, expected volume and prismatic topology after each wall. Preferred print direction is +Y/underside-down metadata, not physical printer certification. Definition is **Testing**; 141 non-CAD tests/builds pass, while actual merge/equation/connectivity behavior awaits the approval-gated BALCONY_BOTH_WALLS case. See [assembly details](tests/BALCONY_ASSEMBLY_V1.md). Existing slab and wall/window/door methods/Program behavior are unchanged; left/right/none modes and a generic library/Building JSON pipeline remain future work.

```text
Program.Main [STA]
  -> SolidWorksConnector.Connect
       -> Marshal.GetActiveObject("SldWorks.Application")
  -> SolidWorksModeler
       -> sketches, dimensions, boss extrusions, opening cuts, rebuilds
       -> UnitConverter (millimeters to meters)
```

There is one console project, not a service or image-processing application. Main ignores command-line arguments and runs a hard-coded example: a 180 x 220 x 3 mm wall with door widths 25/35/45 mm, heights 55/65/60 mm, and X centers -55/0/55 mm. It catches exceptions, writes the message to stderr, and sets exit code 1.

| Repository entry | Responsibility |
| --- | --- |
| SolidWorksAIBridge.slnx | Solution referencing the console project |
| SolidWorksAIBridge/SolidWorksAIBridge.csproj | Non-SDK C# executable, .NET Framework 4.8, Debug/Release AnyCPU, SldWorks/SwConst COM references version 32 |
| SolidWorksAIBridge/Program.cs | STA entry point and hard-coded demonstration |
| SolidWorksAIBridge/SolidWorksConnector.cs | Attach to running COM application; contextual connection errors |
| SolidWorksAIBridge/SolidWorksModeler.cs | Stateful single-wall geometry generation and validation helpers |
| SolidWorksAIBridge/UnitConverter.cs | Finite-length check and mm-to-m conversion |
| SolidWorksAIBridge/App.config; Properties/AssemblyInfo.cs | Application/runtime configuration and assembly metadata |
| ARCHITECTURAL_MODELING_STANDARD_V1.md | Modeling/manufacturing design guidance |
| ARCHITECTURAL_LEGO_V1.md | Approved future component architecture and lifecycle specification |

Local .vs, bin, and obj artifacts were observed and are not portable source or validation evidence. A Microsoft directory was present with no files returned by the recursive file inspection. No separate scripts, SolidWorks macros, web/API server, test projects, CI workflows, image assets, schema files, or component-library implementation were found outside generated/IDE content.

Subsequent regression-baseline addition (2026-11-03): tests/ now contains a dependency-free console test project linking unchanged production sources, a PowerShell build/run script, a separately invoked manual CAD runner, expected-geometry cases, and a dated result record. No SolidWorks macros or production behavior were added. See [test procedure](tests/README.md) and [baseline result](tests/results/BASELINE_2026-11-03.md). The earlier inventory paragraph records the initial audit, not the current absence of tests.

### Implemented automation APIs

| API | Behavior found in source |
| --- | --- |
| CreateNewPart | Default Part template creates an unsaved document |
| CreateWall | Centered rectangle on Front Plane, driving width/height dimensions, mid-plane thickness; WallSketch/MainWall names |
| CreateWindow | Rectangular face-supported opening, driving dimensions and signed model-center placement |
| CreateWindowSill | Outward boss under the latest window, merged with wall |
| CreateWindowRow | Shared sketch/cut for horizontal openings, equal-size/alignment relations and clear gaps, optional sill sketch/boss |
| CreateWindowGrid | Bottom-to-top floor features, shared sketch per row and optional sills; validates complete grid layout first |
| CreateDoor | Bottom-edge-constrained ground opening with independent width/height/X position |
| CreateDoorRow | Independently sized ground doors in one shared sketch and one cut |
| OpeningCutToleranceMm | Finite nonnegative extra inward depth, default 0.2 mm, affecting subsequently created openings |
| CreateProjectingBalconySlab | Additive V1 separate-body slab path with typed preflight; implemented and compiled, actual CAD NOT RUN |

Opening cuts use owning wall thickness plus tolerance and determine reversal from face/sketch normal alignment. Helpers verify rectangle geometry, driving dimensions, bounds/overlap, feature-name conflicts, active document/sketch state, and rebuild outcomes. Cleanup restores preferences and AddToDB. Failed partial features can remain; there is no transactional rollback.

### Current coordinates and limits

Public APIs consume actual miniature mm; COM uses meters. The wall is centered on the model origin, with width along X, height along Y, and exterior face at +thickness/2 in Z. Window centers are signed model X/Y; door bottoms follow the wall bottom.

The modeler stores one current wall and in-memory occupied rectangles. These bounds do not describe all arbitrary user-created or edited geometry. Standalone window/sill names are fixed; rows and doors have indexed names. Current facade helpers assume the original axes/front face. Window grids are not native Linear Patterns or Mirrors. Doors are apertures, not modeled leaves/frames; windows are apertures with optional sills, not complete window assemblies.

## Planned architecture: Architectural LEGO

### Implemented first slice: Projecting Balcony Slab V1

ProjectingBalconySlab.cs now defines a reusable definition (lego.balcony.projecting-slab 1.0.0, Testing), immutable instance/host snapshot, and a COM-free preflight validation record. Inputs are already resolved miniature mm. Horizontal position is host-local X; elevation is the slab top above the wall bottom. The validator converts to the centered legacy model frame and checks IDs, host reference, dimensions, bounds, and representable coordinates. This record explicitly says CAD NotRun and does not implement complete library validation.

SolidWorksModeler.BalconySlab.cs adds CreateProjectingBalconySlab through a partial class; existing geometry methods remain unchanged. It checks current host/document/sketch/name dependencies and known footprint collisions, reuses AddFaceRectangle, resolves outward sketch-normal sign, creates a nonmerged extrusion and checks rebuild plus one new solid. Successful footprint registration prevents later tracked aperture/sill/slab overlap. It operates only on the current unedited front-facing MainWall. The API does not resolve arbitrary hosts, Building JSON, architectural scaling, or floor metadata.

The main csproj and both test builds include the new files. Program remains the door-row demonstration. Tests include 42 new pure/precondition cases, bringing the passing non-COM baseline to 95. Application/manual builds pass; CAD fixtures remain NOT RUN. [Slab details and limitations](tests/PROJECTING_BALCONY_SLAB_V1.md) include center-Y/thickness edit semantics and separate-print behavior.

```text
Photos (optional future video/keyframes)
 -> architectural analysis and element classification
 -> orthographic/corrected facade construction representation
 -> versioned Building JSON
 -> schema + semantic + capability validation
 -> template/variant resolution, dependency and manufacturing planning
 -> deterministic CAD executor
 -> geometry/regression report and reusable-library validation evidence
```

The end-to-end pipeline layers are not implemented as general subsystems; the slab slice above supplies only a typed resolved component and executor. Optional video is a future intention with ingestion/calibration/keyframe strategy unresolved. The planned corrected facade includes major protrusions/recesses and shared datums; architectural relationships are retained rather than flattened into unrelated shapes.

The planned reusable library has Structural, Openings, Architectural, and Secondary-detail families, including walls, floors, roofs, windows, doors, balconies, stairs, railings, recesses, projections, pipes, and facade equipment. Definitions hold versioned schemas/recipes; instances hold building-specific IDs, parameters, placement, parent, host, and floor; validation records preserve exact test/approval evidence. Matching adapts supported templates; composition reuses primitives; insufficient primitives lead to Draft proposals, not automatic validated registration.

Draft -> Testing -> Validated applies universally. Definition validation and individual building-instance validation remain separate. New variants retain exact versions and regression evidence.

Building JSON uses real-world mm, parent-local transforms, X right/Y up/Z outward, and explicit anchors. A resolver converts to actual miniature dimensions, applies selected side-depth compression, thickness rules and joint clearances, then converts to COM meters at execution. Existing centered-wall API coordinates must be adapted, not reinterpreted in place.

Full-building and facade-only modes, selectively reduced side depth, printable bodies, minimum feature sizes, and assembly preparation are planned. Front-facing proportions and important details must survive simplification. Wall/internal-wall defaults are 3/2 actual mm, relief conventions 0.8/1.2 mm, assembly clearance 0.2 mm; minimum feature and roof thickness are profile/part-specific. These documented rules are not an implemented manufacturing engine.

See ARCHITECTURAL_LEGO_V1.md for contracts, the illustrative two-floor JSON, dependencies, validation strategy, and unresolved details. Its definition references and JSON example are specifications, not an executable library.

## Cross-computer prerequisites and verification

Source declares .NET Framework 4.8 and SolidWorks 2024-era COM references. A compatible Windows build toolchain, installed/registered type libraries, configured default Part template with Front Plane, and a running SolidWorks instance at matching privilege level are required for the intended workflow. Availability and current compatibility on each computer are UNKNOWN until verified. No portable setup script or approved canonical build command was found.

Local binaries do not establish correspondence to current source. No recorded runtime test outcomes were found; current CAD correctness and build readiness remain UNKNOWN. Inspect code and run authorized relevant checks when resuming implementation.

Historical regression baseline: the linked-source non-COM harness and manual runner compiled with the installed Visual Studio MSBuild/registered v32 COM libraries (MSB3305 marshaling warnings), and 53 checks passed. The subsequent slab session raised this to 95 passing tests and also built the full application and manual harness successfully. This verifies compilation and numeric/placement checks, not CAD operation success. All manual CAD fixtures remain NOT RUN; CAD correctness remains UNKNOWN. Reflection-based legacy helper tests intentionally bypass COM-backed construction and do not establish general successful layout behavior.
