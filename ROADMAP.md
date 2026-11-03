# SolidWorksAIBridge roadmap and handoff

Current lifecycle status: **Projecting Balcony Slab V1 is Validated within its recorded sampled-case and host scope**, after explicit user visual approval of Part3-Part8. See [scope/evidence](tests/results/PROJECTING_SLAB_V1_VALIDATED_2026-11-03.md). Earlier Testing/PENDING status entries below are historical. Top-preserving thickness and ground-floor support are future capabilities.

## Session close — 2026-11-03 / exact next-session starting point

Today's completed work:

- Established portable AGENTS/architecture/roadmap/decision documentation, Architectural LEGO contracts and the universal component lifecycle.
- Implemented Projecting Balcony Slab V1 without replacing legacy walls/windows/doors. Actual baseline CAD, five slab-only parameter edits and upper-floor aperture integration passed; user visual approval was recorded and the slab was promoted to **scoped Validated**. Preserved numeric reports/raw text evidence and SHA256 manifests.
- Implemented the existing **facade-plane, separate-sketch/two-wall-Boss** BalconyAssembly baseline. **141 non-CAD tests passed, 0 failed** (105 prior +36 assembly); application/manual runner builds passed with MSB3305 COM warnings. Actual **BALCONY_BOTH_WALLS CAD PASS**: two solids, eight generated features, balcony volume 5952 mm^3; depth 24->26->24 propagated, volume 5952->6448->5952, with unchanged MainWall and successful rebuilds. Assembly remains **Testing** pending final review/recipe selection, not Validated.
- Recorded the user's top-face/shared-sketch optimization feedback and comparison plan (BA-004/OPT-001). **The optimized method has NOT been implemented, compiled, unit-tested or CAD-tested.** Its six-feature target and other predicted volumes/counts are expectations only.

Outstanding limitations: no full Building JSON executor/image pipeline/library registry; current unedited front-facing host and cached-state/sign-branch constraints; standalone slab thickness edits are center-based; ground-floor thin slab/door-leaf clearance, optional parapet/railings and other side modes remain future work. Physical print qualification is not established. No CAD documents were saved as evidence; durable transcripts/reports and user approvals are the preserved record. Broader legacy manual cases retain their own unverified status.

**Tomorrow's exact next task:** implement and validate an **additive optimized equal-height two-side-wall BalconyAssembly** using **one shared sketch on the slab TOP FACE and one upward Boss-Extrude**, Merge Result enabled and scope restricted to the balcony body. Preserve the validated slab and the existing eight-feature BALCONY_BOTH_WALLS implementation/test as the regression baseline. Start by reading AGENTS.md and BA-004, inspect current source/tests, then add the equal-height candidate/top-face profile helper and focused pure tests. Compare against the 56 x24 x3 slab / two 20-high, 2-thick walls: expected two solids, 5952 mm^3 balcony and six total generated features; test width/depth relationships and 24->26->24 depth propagation. Obtain explicit approval before the new CAD comparison. Do not add parapets, unequal-height cuts, ground-floor behavior or unrelated features in that first increment.

Session-close activity itself changes documentation only: no geometry, SolidWorks execution or additional test runs. The 141/0 result and CAD passes above are actual earlier runs, not newly rerun checks. Git commit/push is explicitly authorized for this session close; the default no-automatic-Git rule remains for future sessions without authorization.

Baseline: 2026-11-03 source/documentation audit. Status must be rechecked against code in future sessions. No Git operations, build, dependency installation, or SolidWorks execution were performed for this documentation setup.

Regression follow-up on 2026-11-03: test projects and a non-COM runner were added without production edits. Final run: **53 passed, 0 failed**; manual runner build passed, but every CAD case is **NOT RUN**. See [tests/README.md](tests/README.md) for commands, expected dimensions/features/volumes, and [actual result record](tests/results/BASELINE_2026-11-03.md). No Git operations, dependency installation, or SolidWorks execution occurred. The original baseline paragraph describes the earlier documentation-only session.

## Status vocabulary

- Implemented: source exists; does not imply runtime validation.
- Partial: a narrower implemented capability exists than the intended architecture.
- Planned: design only; no corresponding implementation found.
- Experimental: repository evidence identifies a prototype/experiment.
- UNKNOWN: available evidence cannot establish the claim.

## Verified completed work

| Work | Evidence and verification boundary |
| --- | --- |
| Console/COM bridge | Program.cs, SolidWorksConnector.cs; attaches to active instance |
| Editable centered wall | CreateWall in SolidWorksModeler.cs |
| Rectangular window cut and outward sill | CreateWindow / CreateWindowSill |
| Window rows and multi-floor grids with optional sills | CreateWindowRow / CreateWindowGrid; shared sketches and constraints |
| Ground-door and independent-size multi-door row | CreateDoor / CreateDoorRow |
| Parameter/layout checks, cut direction/depth, rebuild checks, preference cleanup | Helpers and operation bodies in SolidWorksModeler.cs |
| Modeling standard and approved LEGO design | ARCHITECTURAL_MODELING_STANDARD_V1.md; ARCHITECTURAL_LEGO_V1.md |

These are completed source/documentation capabilities. Whether each has passed a previous manual CAD test is UNKNOWN: no test records were found. Local Debug binaries/interops exist but are not test evidence.

## Current development stage

Projecting Balcony Slab V1 is now implemented as a narrow Phase 1/2 slice: typed definition/instance/preflight record, pure parameter/placement validation, and a separate-body CAD executor. The definition is Testing, not Validated. Full Building JSON schema, general resolver/library, composite balconies, and other balcony geometry remain absent. The initial pre-balcony assessment is superseded by this implementation.

Partial capabilities: facade modeling currently means a single wall shell with rectangular openings/sills; Window and Door do not include full assemblies. Input checks and rebuild checks exist, but a layered automated regression/manufacturing validation system does not. No separately marked experimental modules were found; experimental work outside this checkout is UNKNOWN.

## Known limitations and unresolved questions

- Hard-coded Program example; no Building JSON input, image ingestion, service API, or general CLI dispatcher.
- One current wall, front-axis assumptions, in-memory 2D overlap tracking; arbitrary orientations and robust multi-body scope are not general capabilities.
- Standalone window/sill feature names restrict repeated unique calls; indexed row/door paths are distinct.
- Ground doors cannot serve as elevated balcony doors without an additional path. Existing ground behavior must remain stable.
- Shared-sketch repetition exists; native feature patterns/mirrors are not implemented.
- Failures can leave partial CAD features. No rollback, automated saving/export, or print preparation pipeline was found.
- No standalone tests, CI suite, Golden Dataset, or recorded manual validation results found. Build success/current SolidWorks runtime compatibility: UNKNOWN.
- Regression follow-up supersedes the earlier absence of tests: focused non-COM checks and manual fixtures now exist. The slab session also verified full application/manual harness builds (COM marshaling warnings). Current CAD runtime compatibility remains UNKNOWN. No CAD fixture has passed yet and no CI or full Golden Dataset is implemented.
- Exact schema/binding/repetition syntax, library storage/promotion policy, printer profile/tolerances, balcony joints, and selective-depth interface rules remain open (LEGO specification section 15).

## Recommended next steps

1. Validate the now-implemented slab contract/executor in CAD before expanding the component. The narrow typed specification, pure validation, stable names/anchors and separate-body policy exist; general Building JSON/library infrastructure remains future work.
2. Execute the now-defined manual baseline cases for existing wall/window/sill/row/grid/door paths, recording measurements and environment. Automated pure checks pass; CAD checks still require an authorized implementation/testing session.
3. After the implemented projecting slab passes CAD validation, consider optional side walls/basic railings in a separately requested increment. Preserve conversion, dimensioning, rebuild/cleanup conventions and explicit body scope.
4. Add recessed enclosure/aperture and repeated balcony groups with propagation and cut-scope tests. A deep recess is not a pocket in a 3 mm facade.

## Phased delivery

| Phase | Scope and exit gate |
| --- | --- |
| 1: LEGO/Building JSON contract | Schema, typed definitions/instances/validation records, dependency planning, manufacturing resolution, dry-run reporting, narrow adapters, baseline fixtures. Only implement the subset needed next. |
| 2: Balconies | Projecting/recessed slabs, side walls, basic railings, repeated arrangements; verify dimensions, editable dependencies, print grouping, scoped cuts, and legacy regressions. |
| 3: Facade recesses/projections | Reusable depth operations with independent zones, printable-thickness checks and regression cases. |
| 4: First end-to-end MVP | Photos -> AI analysis -> corrected orthographic elevation -> validated Building JSON -> supported SolidWorks model. Prioritize this after balcony/facade validation instead of indefinitely adding isolated details. |
| 5: Additional components/refinement | Stairs executor, roofs/structural components, pipes/equipment/shutters/awnings, multi-elevation buildings, angled geometry, richer variants, optional video ingestion. |

Long-term: template adaptation and composition, controlled new-definition proposals, versioned validated library, full buildings and reduced-depth facades, traceable image corrections, and 3D-print manufacturing preparation. All are planned, not implemented.

## Session handoff requirements

Record changed files, evidence/status changes, exact checks/results, remaining UNKNOWN items, and next action. Update this roadmap when implementation changes. Keep older decisions in DECISIONS.md; do not infer completion from an approved plan. Current documentation setup ran no application tests; only documentation/content checks were appropriate.

## Latest handoff — Projecting Balcony Slab V1 (2026-11-03)

- Completed: immutable resolved-miniature instance and host snapshot; definition/version/Testing metadata; preflight evidence and computed bounds; CreateProjectingBalconySlab on MainWall with outward transform-derived extrusion, Merge=false, rebuild/body-count checks, stable names and conservative footprint registration. Existing wall/window/door methods and Program behavior are unchanged; the original modeler declaration is now partial to isolate the additive executor.
- Verified: **95 automated tests passed, 0 failed** (53 legacy + 42 slab checks). Full application and manual harness Debug builds passed with MSB3305 COM marshaling warnings. No executables touching CAD were run. See [result record](tests/results/PROJECTING_SLAB_2026-11-03.md).
- Prepared: BALCONY and BALCONY_MIXED manual fixtures with expected dimensions, volumes, features, and two bodies; see [case specification](tests/PROJECTING_BALCONY_SLAB_V1.md).
- Unresolved: every CAD case remains NOT RUN; actual extrusion direction, separate-body behavior, editable CAD dimensions and rebuilds are unverified. Host must be the current unedited front-facing wall. Cached bounds are not live geometry. No general printability, joint, collision, schema, or library-validation system exists. Manual thickness edits hold center Y rather than preserving slab top automatically.
- Exact next step: in an explicitly authorized SolidWorks session, run the legacy fixtures plus BALCONY/BALCONY_MIXED, measure dimensions/body counts/volumes, capture standard views and width/depth edit rebuilds, and append actual results. Resolve any discrepancy before adding side walls, railings, recesses or repetition. Do not mark the definition Validated or expand scope on the strength of non-COM checks alone.

## Latest validation handoff — ground-door threshold request (2026-11-03)

- Requested scope now supersedes the previous multi-case execution step: **BALCONY first, then only the ground-door threshold integration case; stop on failure/limitation**.
- Prerequisite inspection found no running SolidWorks process or visible app/window despite the user's report that it was open. Requested the user open it on the connected computer; no auto-launch or CAD execution occurred. BALCONY remains BLOCKED / NOT RUN.
- Integration placement is calculated explicitly in the test harness: door center X=0, threshold Y=-110; slab width 56 for a 30 mm door gives 13 mm each-side overhang; slab top host elevation 0 implies Y [-113,-110] for 3 mm thickness. **Existing V1 rejects this below-host placement**. Production code was not modified and a raised/misaligned slab was not substituted.
- Implemented test-only arithmetic fixture and a preflight-gated manual case. Verified **97 automated checks passed, 0 failed**; manual harness build passed with existing COM marshaling warnings. Integration CAD and all measurements remain NOT RUN, not PASS.
- Details and expected-versus-actual values: [threshold report](tests/results/THRESHOLD_PREFLIGHT_2026-11-03.md).
- Exact next step: once SolidWorks is accessible, execute BALCONY alone and preserve any failure. Before threshold integration execution, obtain the user's direction on the reported host-height limitation; do not alter production boundaries or run other cases without authorization.

## Latest actual CAD handoff — BALCONY only (2026-11-03)

- Connection diagnosed: process 34776 visible in session 1; sandbox identity got MK_E_UNAVAILABLE (0x800401E3), desktop-user retry attached successfully. PowerShell dynamic type inspection hit 0x8002802B; compiled typed COM worked. Runtime revision 32.5.0; configured Part.prtdot exists; Front Plane verified in new part. No second instance launched.
- Executed **BALCONY once**, creating only unsaved Part1. Existing document count was 0. No existing/user document was edited/saved/closed/overwritten. Production code and Git untouched.
- Actual geometry: wall 180 x 220 x 3; slab 56 x 24 x 3, bounds X [-8,48], Y [7,10], Z [1.5,25.5]; slab volume 4032 mm^3, total 122832 mm^3 within tolerance. Two separate solids and successful rebuild confirmed.
- Runner result: **FAILED, exit 1**. New test helper counted only one boss because GetTypeName2 reported slab ICE. Read-only inspection confirmed underlying Boss via GetTypeName: actual generated count is two sketches plus two bosses, zero cuts. Test instrumentation needs correction; do not relabel failed execution as PASS. Definition remains Testing.
- Stopped as requested. No mixed/threshold/legacy/parameter-edit cases ran. Part1 remains open and unsaved. Detailed expected/actual evidence: [BALCONY actual report](tests/results/BALCONY_CAD_2026-11-03.md). Prior 97 non-COM tests were not rerun this session; manual harness builds passed with COM warnings.
- Exact next step: fix the **test-only** Instant3D classifier using documented underlying type handling; verify measurement logic and then run only subsequently authorized checks. No production geometry fix is justified by this count mismatch. Do not expand geometry scope or run other cases automatically. Threshold boundary-policy decision remains unresolved.

## Latest handoff — classifier fix and clean BALCONY rerun (2026-11-03)

- Completed **test-only** ICE classification fix: resolve underlying type only for ICE, preserve per-name deduplication, count normal and wrapped Boss/Extrusion once. Production geometry unchanged.
- Verified **105 automated tests passed, 0 failed**, including eight new classifier regressions. Manual harness rebuilt successfully with existing MSB3305 COM warnings.
- Ran **only BALCONY**, attaching to existing SolidWorks revision 32.5.0. Fresh unsaved **Part2**: two bodies, slab 56 x 24 x 3 mm, +Z projection, expected position/volumes, two sketches plus two bosses and zero cuts; rebuild True. **Numeric CAD baseline PASS; exit 0.** See [actual rerun report](tests/results/BALCONY_CLASSIFIER_RERUN_2026-11-03.md).
- No existing docs were open at initial inventory; no existing/user document was edited/saved/closed/overwritten. No other case or parameter-edit test ran. Part2 remains open/unsaved. No second instance or Git operations.
- Remaining: full documented acceptance is not complete. Parameter-edit/rebuild behavior, visual evidence/review, and final approval remain pending. Definition stays **Testing**, not Validated. Threshold placement restriction is unchanged.
- Exact next step: obtain the next authorized scope for documented BALCONY edit/visual checks on a test-only document, record actual outcomes and approval evidence, and only then assess lifecycle promotion. Do not run broader CAD cases or expand production geometry automatically.

## Latest handoff — parameter edits and elevated-door integration (2026-11-03)

- Added only test harness cases/measurement expectations; production geometry unchanged. No railings, walls, other production features, or Git operations introduced. Manual harness build passed (existing MSB3305 warnings); **105 automated checks passed, 0 failed**.
- Actual CAD **PASS** in six fresh unsaved parts: Part3 width, Part4 depth, Part5 thickness, Part6 horizontal position, Part7 elevation, Part8 elevated-door integration. Each exit 0. All edits retain two solids, four generated features, expected measured volumes/bounds and successful rebuilds. Thickness-only change preserves sketch center and shifts top; elevation is explicitly calculated.
- Part8 reuses CreateWindow as a 30 x 60 upper-floor door aperture at center (20,40), threshold Y=10 (host elevation 120). Slab 56 x 24 x 3 at center X=20/top Y=10 gives equal 13 mm overhang. All eight aperture corner vertices, unchanged hole volume, two separate solids, six generated features, total 117432 mm^3 and successful rebuild verified.
- Existing docs preserved; dirty-flag checks passed after each case. Part3-Part8 remain open/unsaved. No document saves/closes or new SolidWorks instance. UI capture did not provide reliable visual evidence; numeric tests do not substitute for human review.
- Evidence: [actual edit/integration report](tests/results/BALCONY_EDITS_UPPER_DOOR_2026-11-03.md). Definition remains **Testing**, visual approval PENDING. Ground-threshold restriction, generic Door semantics, automatic door-to-balcony dependency updates, broader CAD regressions and printer-profile validation remain outside these results.
- Exact next step: request the user's visual review of Part3-Part8, especially Part8's threshold alignment/opening/overhang/body separation and acceptance of center-based thickness edits. Record approval or corrections before assessing Validated status; do not expand production features or perform additional CAD mutations automatically.

## Latest handoff — user approval and scoped validation (2026-11-03)

- **User visual approvals recorded:** Part3-Part7 appeared correct as slab-only parameter-edit tests; door openings were not expected. Part8's elevated door opening, balcony alignment and placement appeared correct.
- **Promoted lego.balcony.projecting-slab 1.0.0 to Validated within explicit scope**, not all numerical inputs or continuous parameter combinations. Host: current unedited front-facing MainWall 180 x 220 x 3, SolidWorks 2024 revision 32.5.0. Cases: baseline 56 x 24 x 3 at X=20/top elevation 120, individual width/depth/thickness/X/elevation edits, and the exact elevated-aperture integration. See [authoritative approval record](tests/results/PROJECTING_SLAB_V1_VALIDATED_2026-11-03.md).
- Preserved 12 raw CAD/automated/build transcripts as repository-friendly .txt evidence plus SHA256 manifest, including the original failed classifier run. Existing reports and original logs retained. No saved CAD/screenshot evidence is invented.
- Changed only lifecycle status/scope metadata in the component source and corresponding test expectation; geometry calculations and both executor files remain unchanged. Fresh **105 automated tests passed, 0 failed** after promotion metadata update. **No CAD tests or document operations ran** in this promotion session; no Git operations.
- Known limits: thickness editing is still center-based (top can move); ground elevation 0 is rejected for positive thickness; upper-floor door fixture uses CreateWindow rectangular cut geometry; no automatic Door-to-Balcony relationship, generalized hosts, railings, side walls, patterns or printer certification. Broad legacy CAD cases retain their own unverified status.
- Future design direction: prefer fixed slab top and downward thickness growth to retain threshold alignment. Ground-floor support should include configurable printable thin slab and separate door-leaf bottom clearance. Both need separate design/versioning, implementation and validation; they do not expand V1's approval.
- **Exact next implementation step:** in a separately authorized increment, introduce a versioned top-datum-preserving thickness constraint and focused edit/door-threshold integration tests, retaining this validated V1 behavior/version. Then scope ground-floor thin-slab and leaf-clearance support. No new geometry work begins as part of this approval record.

## Latest handoff — BalconyAssembly V1 side-wall rules (2026-11-03)

- Recorded user-approved geometry/manufacturing rules in DECISIONS.md (BA-001) and ARCHITECTURAL_LEGO_V1.md. New **lego.balcony.projecting-assembly 1.0.0 is Draft**, composite design only. The validated slab child remains unchanged and does not confer validation on the assembly.
- Required rear gap=0, fronts at slab edge, persistent depth propagation from slab, independent per-side height/thickness, none/left/right/both modes, one connected printable balcony solid and a separate facade body. Keep all child features editable. Preferred print orientation is slab underside down/walls up.
- Proposed minimal safe recipe: reuse the unchanged slab; add individually named, unmerged wall bosses; explicitly union only the balcony child bodies. No-wall variant skips union. Final host+assembly count must be two solids, with connectivity and minimum printable thickness checked.
- Planned initial matrix uses the validated 180 x 220 x 3 host / 56 x 24 x 3 slab and test-only 20 mm high / 2 mm thick sides. All four side modes, depth 24->26 propagation, independent height/thickness edits, scope/connectivity, volume/counts, print orientation and legacy regressions need checks. Expected values in the specification are prospective, not actual results.
- **This session changed documentation only. No tests, SolidWorks execution, source changes or Git operations.** Existing validated slab evidence/status preserved. Actual system architecture unchanged; ARCHITECTURE.md was not edited.
- Exact next implementation step: when separately authorized, add the Draft assembly specification, preflight and child depth bindings, then the bounded wall-boss/explicit-union executor and focused fixtures. Confirm selected-body union cannot absorb the facade before running new CAD cases. Do not implement railings/recesses/patterns. Top-preserving slab edits and ground-floor support remain future versioned work, not prerequisites that permit altering V1 silently.

## Latest handoff — scoped Boss-Extrude clarification (2026-11-03)

- User-approved BA-002 supersedes the earlier isolated-side-wall-bodies / separate Combine proposal. **BalconyAssembly V1 remains Draft; no geometry exists yet.** Standalone slab V1 stays unchanged and scoped Validated.
- Next recipe: reuse slab, add each side as a standard editable Boss-Extrude with **Merge Result enabled**, explicitly scoped to the current slab/merged balcony body where supported. Exclude MainWall, resolve updated body references after rebuild, and do not add a redundant Combine. Stop/report if safe merge isolation cannot be established.
- Verify **exactly two solids**, actual connected slab/side-wall membership, independent unchanged MainWall, expected bounds/volumes, feature editability, dependent depth changes and rebuilds. Revised prospective generated-feature counts: none 4, one side 6, both sides 8, including sketches and no Combine.
- This clarification changed only DECISIONS.md, ARCHITECTURAL_LEGO_V1.md and ROADMAP.md. No source changes, tests, SolidWorks execution or Git operations.
- Exact next step when implementation is authorized: add assembly preflight/child bindings, then prove scoped Merge Result on one wall in a fresh test part before expanding to both sides; validate scope/connectivity after each rebuild. Do not implement the superseded Combine recipe or modify the validated slab.

## Latest handoff — both-wall implementation, CAD approval pending (2026-11-03)

- Implemented first **both-wall** BalconyAssembly 1.0.0; lifecycle **Testing**, not Validated. Reuses the unchanged validated slab. Separate editable wall Boss-Extrudes request Merge Result with selected balcony-body mark 8; MainWall excluded; no Combine. Re-resolves body identity after each wall and guards facade bounds/volume, two-body count, assembly volume and prismatic Euler topology.
- New pure plan validates dimensions, host/IDs, minimum printable thickness, clear width, side bounds/contact and underside-down printing geometry. Named equations bind wall depth and centers to slab dimensions; sign-branch/cached-state limits remain. Left-only/right-only/none, rails/recesses/patterns and generic host support are not implemented.
- **141 non-CAD tests passed, 0 failed**: all prior 105 plus 36 assembly cases. Full application and manual runner Debug builds passed with existing MSB3305 COM warnings. Standalone slab/legacy source hashes match prior validated evidence.
- Prepared one **BALCONY_BOTH_WALLS** case: 180 x 220 x 3 host, 56 x 24 x 3 slab, both walls height 20/thickness 2; expected assembly volume 5952, total 124752 mm^3, two solids, eight generated features. Same case edits depth 24->26 (assembly 6448) and restores 24 to verify persistent wall-depth binding. Expected values are NOT actual results.
- **No CAD execution or Git operations.** Actual merging, connectivity, parameter dependencies, rebuild and printing geometry remain unverified in SolidWorks. See [checkpoint](tests/results/BALCONY_ASSEMBLY_IMPLEMENTATION_2026-11-03.md) and [case instructions](tests/BALCONY_ASSEMBLY_V1.md).
- Exact next step: obtain user approval for `.\tests\bin\Manual\SolidWorksAIBridge.ManualValidation.exe --confirm-cad BALCONY_BOTH_WALLS`, then attach to existing SolidWorks as desktop user, create one new unsaved part, stop/preserve on failure, and record actual results. Do not auto-run, launch another instance or modify existing user documents. Promotion requires actual CAD evidence and visual review.

## Latest handoff — BALCONY_BOTH_WALLS actual CAD PASS (2026-11-03)

- Executed the single user-approved case once on existing SolidWorks revision 32.5.0/process 34776, in one new unsaved **Part9**. No production changes, other CAD cases, saves/closes, second instance or Git operations. Initial open-document count was 0; dirty-flag check completed unchanged.
- **PASS, exit 0:** final two solids (MainWall and connected BalconyAssembly), eight generated features (four sketches/four bosses), balcony baseline volume **5952 mm^3**, unchanged MainWall volume/bounds and successful rebuild. Rear faces at facade plane Z=1.5; walls reach slab front Z=25.5.
- Depth propagation **24->26->24 passed**: both equation-driven wall depths followed, measured front Z 25.5->27.5->25.5, balcony volume 5952->6448->5952 within tolerance; counts/rebuild/connectivity checks passed at each stage. All 16 U-profile corners matched, with V/E/F=16/24/10. No Combine or facade merge.
- Underside-down/+Y-up printing geometry confirmed for the fixture; no physical printing/material certification. Prior **141 passing non-CAD checks** were not rerun during this CAD-only session.
- Evidence: [actual result and preserved transcript](tests/results/BALCONY_BOTH_WALLS_CAD_2026-11-03.md). Assembly remains **Testing**, visual approval **PENDING**. Part9 is left open/unsaved at restored depth 24.
- Exact next step: obtain the user's visual review of Part9 (both walls, flush rear/full-depth fronts, two bodies, editable features, intended print orientation), record approval/corrections, then assess scoped validation. Do not promote, run more cases or modify production geometry automatically.

## Latest handoff — top-face optimization proposal awaiting approval (2026-11-03)

- User hands-on feedback favors one sketch on the slab TOP FACE and one upward merging wall Boss-Extrude, optionally including a solid front parapet. Detailed candidate **top-face-shared-r1 (Draft)** and learning rule OPT-001 are recorded in DECISIONS.md / BA-004. Existing two-sketch/two-wall-boss implementation and its numeric CAD PASS remain a preserved Testing regression baseline; validated standalone slab unchanged.
- Equal-height candidate: resolve top face geometrically, use a new X/Z-aware profile helper, normalize union footprints, bind slab width/depth and independent thicknesses, and extrude upward with Merge Result scoped only to balcony body. Both-wall whole-Part target is **3 sketches + 3 bosses = 6 features**, compared with the existing 8. Two solids still mandatory; no Combine.
- Eventual configurations: none/left/right/both sides; optional **solid parapet** distinct from future railings; independent heights/thicknesses; underside-down print orientation. Both-side/parapet corners require an explicit policy: proposed union keeps the taller wall through overlaps. Do not cut taller adjacent wall corners merely because cuts target the correct body.
- Unequal heights: first evaluate shared Hmax boss plus scoped lowering cuts, protecting taller footprints and staying above slab top. Compare direct separate/grouped bosses. Expected totals are 8 with one lower-height group or 10 with two, plus an optional reference plane; no automatic feature-count advantage. Height-order changes/zero-depth cuts require explicit suppression/replanning or a more robust separate-boss recipe.
- Baseline candidate fixture expects same 5952 mm^3 balcony (6448 at depth 26). Additional planned volumes with 2 mm walls: equal 20 mm sides/front 8032; sides 20/front 10 =>6992; left20/right15 no front =>5712; left20/right15/front10 =>6752. These are calculated expectations, **not tested results**.
- Evaluate geometry, volume, connectivity/two-body isolation, facade invariance, stable rebuilds/references, edits, manufacturing and then feature complexity. Preserve existing tests/evidence. Candidate best-practice rule: **Build shared geometry first, then modify only the differences.** No Modeling Optimization Engine exists yet.
- **This session changed only DECISIONS.md and ROADMAP.md. No source changes, tests, SolidWorks execution or Git operations.** No scope promotion or final assembly validation occurred.
- Exact next step: present/review the equal-height top-face construction and corner policy; **wait for user implementation approval**. Then add an additive equal-height both-wall candidate and planner tests first, without replacing the existing API/case. Further CAD comparison requires explicit approval. Subsequent increments cover other modes/parapet and evidence-based unequal-height recipe selection, without unrelated production features/dependencies.
