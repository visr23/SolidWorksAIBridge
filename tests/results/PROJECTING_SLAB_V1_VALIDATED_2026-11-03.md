# Projecting Balcony Slab V1 — scoped validation and user approval

Decision date: 2026-11-03. Definition: **lego.balcony.projecting-slab**, version **1.0.0**. Lifecycle: **Testing -> Validated**, limited to the evidence and constraints below. This is the authoritative scope record referenced by ProjectingBalconySlabDefinition.ValidationScope; it is not a generic component-library implementation.

## User visual approval

Reviewer: project user, explicitly confirmed in the development conversation after reviewing the test documents.

- **Part3-Part7 approved:** slab-only width, depth, thickness, horizontal position and elevation edit cases appeared correct. Door openings were not expected; their absence is not a defect.
- **Part8 approved:** elevated door opening, balcony alignment and placement appeared correct.
- User explicitly authorized scoped promotion and requested future top-preserving thickness behavior and ground-floor support. Those requests do not imply implementation in V1.

Approval applies to the exact cases in [edit/integration results](BALCONY_EDITS_UPPER_DOOR_2026-11-03.md) and the [clean baseline rerun](BALCONY_CLASSIFIER_RERUN_2026-11-03.md). Explicit user review supplies visual approval evidence, not the earlier failed UI screenshot capture. No reviewer name beyond the project user is inferred.

## Validated host and execution scope

- Existing front-facing **MainWall** generated through CreateWall(180,220,3), in its original orientation/dimensions. Actual miniature mm; X right, Y up, outward +Z.
- Same active Part/modeler session; default Part template with Front Plane. Do not manually resize, reorient, replace or delete the host before additional generation.
- Environment exercised: SolidWorks 2024 revision **32.5.0**, Windows desktop-user COM attachment, .NET Framework 4.8/Visual Studio 18 toolchain. Other environments require applicable checks.
- Rectangular projecting slab, separate solid, zero mating gap; correct measured geometry/volume and rebuild. No automatic saving or closing.
- Existing parameter/host/boundary checks and tracked 2D footprint exclusions remain in effect. No generalized 3D collision, assembly or physical printability certification.
- Elevation input is slab top above wall bottom; manual CAD thickness edits are **center-based**. Validate accepts more numerical inputs than this CAD evidence covers; passing it does not expand approval.

## Validated instances and transitions

Actual miniature mm; each edit starts from a fresh baseline, not the preceding row's edited state.

| Case | Inputs / transition | Verified outcome |
| --- | --- | --- |
| Baseline | Width 56, depth 24, thickness 3, center X=20, top host elevation 120 | Bounds X [-8,48], Y [7,10], Z [1.5,25.5]; slab volume 4032 mm^3; two solids; four features; rebuild |
| Width / Part3 | 56 -> 60; other baseline values unchanged | Volume 4320; expected bounds/counts; rebuild; visual approval |
| Depth / Part4 | 24 -> 26; other baseline values unchanged | Volume 4368; outward extent 27.5; separate bodies; rebuild; visual approval |
| Thickness / Part5 | 3 -> 4 at fixed center Y=8.5 | Volume 5376; Y [6.5,10.5]; top host elevation becomes 120.5; rebuild; visual approval |
| Position / Part6 | Center X 20 -> 30 | X [2,58]; volume 4032; counts unchanged; rebuild; visual approval |
| Elevation / Part7 | Top host elevation 120 -> 130 via center Y 8.5 -> 18.5 | Y [17,20]; volume 4032; counts unchanged; rebuild; visual approval |
| Upper-floor integration / Part8 | Aperture 30 x 60, center (20,40), threshold model Y=10 / host elevation 120; baseline slab X=20/top Y=10 | 13 mm overhang each side; all 8 aperture corners; wall volume 113400; total 117432; 2 solids; 3 sketches + 2 bosses + 1 cut; rebuild; visual approval |

Measured envelope: widths 56/60, depths 24/26, thicknesses 3/4, X centers 20/30, top elevations 120/120.5/130. **This is sampled-case/transition coverage, not exhaustive validation of continuous intervals or their Cartesian combinations.** Width 60 with depth 26 and thickness 4 together was not tested. Negative/cross-origin CAD edits and arbitrary boundary cases are not promoted because some pure preflight tests exercise them.

The elevated aperture reuses CreateWindow rectangular cut geometry with WindowSketch/WindowCut names. It is not a new semantic Door component or automatic Door-to-Balcony dependency. Fixture placement was calculated explicitly.

## Preserved evidence and identity

- [Original CAD diagnosis and failed classifier invocation](BALCONY_CAD_2026-11-03.md): retained, not relabeled PASS.
- [Classifier correction and clean baseline rerun](BALCONY_CLASSIFIER_RERUN_2026-11-03.md): 105 automated checks and passing BALCONY runner.
- [Five edits and upper-door integration](BALCONY_EDITS_UPPER_DOOR_2026-11-03.md): six actual CAD passes, visually approved above.
- [Ground-threshold limitation](THRESHOLD_PREFLIGHT_2026-11-03.md): unchanged by elevated-door success.
- [Raw transcript archive](evidence/SLAB_V1_2026-11-03/README.md) and [SHA256 manifest](evidence/SLAB_V1_2026-11-03/SHA256SUMS.txt): original failure/diagnosis, clean baseline, six CAD cases, automated/build output and promotion metadata check. Original local logs retained.

No CAD files were saved and no trustworthy screenshots were produced by the agent. Do not invent saved artifacts; numeric transcripts and explicit user approval are the preserved evidence.

Geometry executor SHA256 values remain those actually tested:

| Source | SHA256 |
| --- | --- |
| SolidWorksModeler.BalconySlab.cs | 661C975C1D6A5BC63D5CCCFE6A6447F0DFB5FB54B031CF1B1FCD369EA6975E59 |
| SolidWorksModeler.cs | 6E6EDF6346F4D598B864CE3530A44258BED0D0B6A188CEF4999A0F12BB818511 |

ProjectingBalconySlab.cs changed only Status=Validated and ValidationScope pointing here. Before SHA256: 329E778F23753F154F4EF08E0DF01DFAC3BEDD60FE4DCE9BC4CF10B268D9F0AB. After: 8AD7115669EC7EF78336F3BE4E53948CFD00D8F17B4073CBBE54D12A171256AC. Content comparison verified no other source change. Preflight CadStatus remains NotRun: instance preflight alone executes no CAD, independently of definition approval.

Promotion session ran only the non-COM regression script: **105 passed, 0 failed**, exit 0, after updating the lifecycle metadata assertion. No new CAD tests/document operations or Git commands. Prior CAD execution supplies the geometry evidence.

## Limitations and future work

- Manual thickness edits move top/bottom about the fixed center; no automatic threshold preservation. This is the tested V1 behavior, with a future improvement preference recorded.
- Ground elevation 0 is still rejected for positive slab thickness. Configurable ground-floor thin slabs and door-leaf clearance are future capabilities.
- No railings, side walls, recessed balconies, patterns, generic hosts, full Building JSON pipeline, modeled door leaf, automatic inter-component associativity or manufacturing-profile certification.
- Broader legacy/manual cases retain their own NOT RUN/UNKNOWN status; this promotion does not certify them.
- New geometry/anchor/behavior, dependencies, environment or scope need a new candidate/version and applicable checks. Do not expand scope through the status label alone.

Next implementation step: design a separately versioned top-elevation-preserving thickness behavior (top fixed, growth downward), with explicit threshold relations and edit/integration regressions. Apply semantic versioning according to compatibility impact. Ground-floor support follows as a bounded increment: configurable printable thin slab and actual-mm door-leaf bottom clearance, without conflating clearance with aperture height. Neither capability is implemented by this promotion.
