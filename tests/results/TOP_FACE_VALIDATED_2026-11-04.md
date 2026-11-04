# Equal-height top-face balcony — scoped Validated release

Approval recorded **2026-11-04**. Reviewer: **the user in this session**.
Subject: **Part3** from the approved `BALCONY_TOP_FACE_COMPARE` execution.
User confirmation: "the optimized Part3 balcony with two equal-height side walls
appears geometrically correct and matches the intended modeling method."
The user also confirmed the documented CAD comparison passed and explicitly
requested promotion if applicable criteria were met.

## Exact definition and scope

- Definition ID: `lego.balcony.projecting-assembly.equal-height-top-face`
- Definition version: **1.0.0**
- Recipe revision: **top-face-shared-r1**
- Status: **Validated within this recorded sampled-case and host scope**
- Preferred recipe for the tested equal-height two-side-wall configuration:
  **one shared sketch on the slab TOP FACE and one upward merging Boss-Extrude,
  explicitly scoped to the balcony body**; six whole-Part generated features.
- Slab dependency: unchanged `lego.balcony.projecting-slab` **1.0.0**.
- Environment: SolidWorks 2024 revision **32.5.0**, existing front-facing,
  unedited MainWall **180 x 220 x 3 mm**, documented Part template/Front Plane.
- Approved reference inputs (actual miniature mm): slab **56 x 24 x 3**, X center
  **20**, top elevation **120** above host bottom; **both** side walls height
  **20**, thickness **2** each; supplied minimum printable section **0.8**.
- Sampled edits: depth **24->26->24** at width 56; width **56->60->56** at depth
  24. This does not validate a continuous range, simultaneous combinations, other
  heights/thicknesses/placements, or all inputs accepted by the preflight API.

This ID isolates the optimized recipe from the broader
`lego.balcony.projecting-assembly` **1.0.0**, which stays **Testing**. The existing
eight-feature implementation/API/case remains unchanged as the regression baseline;
its lifecycle is not promoted by this approval. No registry/library executor is
implemented by adding release metadata. Future geometry changes to this released
recipe require a new candidate/version and applicable evidence.

## Acceptance criteria review

All **applicable criteria for the exact scope above are satisfied**:

| Criterion | Actual evidence / outcome |
| --- | --- |
| Non-CAD regression checks and builds | Implementation: 165/0, application and both runner builds passed; metadata promotion: fresh 165/0 and regression runner rebuild passed |
| Six generated candidate features | Part3: three sketches + three bosses at all sampled states; no cut/Combine |
| Two solids and connectivity | MainWall plus one balcony solid; U-profile 16/16 corners and V/E/F=16/24/10 passed |
| Dimensions and expected mass | Slab/wall bounds, wall height 20/thicknesses 2, centers and depths passed; balcony 5952 mm³ at D=24 and 6448 mm³ at D=26 |
| Depth propagation/restoration | Part2 and Part3: 24->26->24, 5952->6448->5952 mm³; successful rebuilds |
| Width propagation/restoration | Part3: 56->60->56, 5952->6240->5952 mm³; successful rebuilds |
| Flush rear and full-depth fronts | Rear Z=1.5 throughout; fronts at slab edge Z=25.5->27.5->25.5; U-section corners passed |
| Shared slab-top sketch | Read-only support face normal (0,1,0), eight profile edges/four construction diagonals, all endpoints at Y=10 mm |
| Upward common boss | Read-only depth 20 mm, model direction +Y; one common wall boss |
| Merge/body isolation | Merge=True, FeatureScope=True, AutoSelect=False; exclusive balcony-body selection/count guard; unchanged measured MainWall volume/bounds at all sampled edits |
| Manufacturing geometry | Sections exceed the supplied 0.8 mm minimum; connected unit, separate facade, underside-down/+Y-up geometry; physical printer qualification excluded |
| Compatibility baseline | Unchanged slab and eight-feature implementation/case; same-run baseline CAD PASS |
| Required human visual review | User explicitly approved Part3 geometry and intended modeling method in this session |

The scope-body getter returned **-1** without rollback. Its stored body list was
**not independently enumerated**; no failed geometry/isolation check occurred.
The recorded selection guard, stored flags, measured two-body geometry and facade
invariance satisfy the scoped isolation criterion without claiming list readback.

No CAD/screenshot files were saved. Human approval is tied to the explicitly named
Part3 from the execution, not to an invented CAD file digest. Durable evidence is
the measured transcripts, inspection, source digests and user approval record.

## Preserved evidence and promotion activity

- [Actual CAD comparison and inspection](TOP_FACE_CAD_2026-11-04.md)
- [Implementation/build record](TOP_FACE_IMPLEMENTATION_2026-11-04.md)
- [CAD evidence hashes](evidence/TOP_FACE_CAD_2026-11-04/SHA256SUMS.txt)
- [Promotion non-CAD output](evidence/TOP_FACE_PROMOTION_2026-11-04/regressions.txt)
- [Promotion build output](evidence/TOP_FACE_PROMOTION_2026-11-04/regression-build.txt)
- [Release source digests](evidence/TOP_FACE_PROMOTION_2026-11-04/release-sources.sha256)
- [Promotion evidence hashes](evidence/TOP_FACE_PROMOTION_2026-11-04/SHA256SUMS.txt)

This promotion changed lifecycle/identity/evidence metadata, its existing test
expectation and documentation only. No geometry calculations, executor files,
baseline APIs, CAD cases or Program behavior changed. A fresh non-CAD regression
runner build and **165 passed / 0 failed** verified the metadata update; existing
MSB3305 COM warnings remain. Application/manual builds were not rerun; their prior
successful implementation builds remain historical evidence. **No SolidWorks
operations or Git operations occurred during promotion.**

## Limits and next priorities

No approval for none/left/right-only modes, front parapets, railings, unequal
heights, ground-floor behavior, generalized hosts/orientations, new placements,
independent wall-height/thickness edits, physical printing, or arbitrary parameter
combinations. Initial X sign-branch/cached-host constraints and unsupported sketch
frame checks remain. The slab's center-based thickness semantics are unchanged.

Next priorities, in order: remaining balcony configurations; front parapets and
railings; **Building Dataset Analyzer**; **Building Process Learning**. These are
planned, not implemented or validated by this session. The exact next development
step is to scope an additive remaining-configuration increment (none/left/right
modes and unequal-height recipe selection), define pure planner fixtures and CAD
acceptance criteria, and preserve this release and the eight-feature baseline.
No further implementation or CAD execution is authorized by this approval record.
