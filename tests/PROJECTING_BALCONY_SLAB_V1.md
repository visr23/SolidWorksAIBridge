# Projecting Balcony Slab V1 — implementation and manual validation

Definition: lego.balcony.projecting-slab, version 1.0.0, lifecycle **Validated within the recorded scope**. User visual approval covers Part3-Part7 slab-only edits and Part8 elevated-door integration. See the [authoritative scope/approval record](results/PROJECTING_SLAB_V1_VALIDATED_2026-11-03.md) for tested cases, host constraints and preserved evidence. This is not an implemented generic component-library registry. Scope remains a rectangular projecting slab only.

## API and units

```csharp
modeler.CreateWall(180, 220, 3);
var instance = new ProjectingBalconySlabInstance(
    "balcony-01", "MainWall", 56, 24, 3, 20, 120);
modeler.CreateProjectingBalconySlab(instance);
```

Constructor arguments: instance ID, host ID, width, outward depth, slab thickness, horizontal center, top elevation. All lengths are **resolved actual miniature mm**, not real-world Building JSON lengths. The host anchor is bottom center of the wall exterior face: elevation 120 on a wall of height 220 means model top Y=10. Thickness extends downward. Z starts at wallThickness/2 and projects toward +Z. There is no implicit scaling, compression, rotation, or clearance.

ProjectingBalconySlabDefinition.Validate(instance, hostSnapshot) performs COM-free preflight. Its returned BalconySlabValidationRecord captures immutable inputs, definition/version, UTC timestamp, calculated bounds, and parameters/placement success. CadStatus remains NotRun: it is a preflight record, not an execution or complete manufacturing-validation record. Invalid input throws an exception rather than producing passing evidence. This is a small typed contract slice, not a general Building JSON/schema/library implementation.

The executor constructs the host snapshot from its current wall, validates the instance, checks the active document/host dependencies/name collisions, and rejects positive-area intersections with tracked facade footprints. It reuses existing rectangle/dimension helpers, sketches on the wall face, derives outward sign from the sketch transform, and requests an unmerged Blind boss. It checks rebuild success and exactly one additional solid body. Size/position dimensions and extrusion depth are editable; no global variable engine is introduced.

## Limits

Actual validation update: all five parameter edits and the elevated-door integration passed in fresh Part3-Part8; see [edit/integration results](results/BALCONY_EDITS_UPPER_DOOR_2026-11-03.md). User visual approval is now recorded and scoped promotion completed. Thickness-only edits demonstrably shift the top by half the thickness change; elevation edits use the explicit center-Y calculation. Earlier Testing/PENDING statements below and in historical reports describe their original sessions, not the current scoped approval.

- Current front-facing, unedited MainWall only. A pure host snapshot with a custom ID is useful for preflight testing, but the CAD adapter does not resolve arbitrary wall IDs or planes.
- Host dimensions and occupied bounds are cached by the existing modeler. Do not manually resize/reorient/delete/replace a host and then reuse that modeler. Use a fresh invocation/part.
- Stable instance IDs must start with a letter and contain at most 64 letters/digits/underscores/hyphens. Deterministic sketch/feature names reject reuse, including partial failed features.
- Width must fit the wall; slab bottom/top must fit its height. Edge contact is permitted. Slab footprint cannot overlap tracked openings, sills, or slabs; shared boundaries such as a threshold are allowed. This conservative 2D policy is not general 3D collision detection.
- The footprint is registered only after successful rebuild/body count checks. Failures may leave partial geometry; there is no rollback or automatic saving/closing.
- Slab stays separate from the facade, with zero mating gap. Assembly joints, printer minimums, clearances and printability are not certified by preflight.
- The reused rectangle exposes a center-Y dimension. Changing thickness manually with center Y fixed changes the slab top. To configure a new top elevation/thickness pair reliably, create a fresh instance/part through the API; no in-place instance update or top-datum equation is implemented. Do not claim automatic top preservation after arbitrary manual edits.
- No railings, side walls, recesses, patterns, full composites, or automatic component promotion.

## Manual CAD cases and execution status

BALCONY executed once on 2026-11-03. Measured baseline dimensions, +Z direction, two solid bodies, four generated features and rebuild matched expectations. The runner returned failure because its new feature counter did not resolve the slab's Instant3D ICE wrapper to underlying Boss; a read-only inspection confirmed that type. Part1 remains unsaved/open. See [actual CAD report](results/BALCONY_CAD_2026-11-03.md). No further case or edit test ran; lifecycle remains Testing. Do not count the failed runner invocation as a complete PASS. BALCONY_MIXED remains NOT RUN.

Subsequent classifier fix/rerun: [new actual result](results/BALCONY_CLASSIFIER_RERUN_2026-11-03.md) records **105 automated checks passed** and **BALCONY numeric CAD baseline PASS (exit 0)** in fresh unsaved Part2. ICE resolves to underlying Boss and deduplication prevents repeated counts. The first failed result remains historical evidence; Part1 was absent from the rerun's initial inventory (not closed by the test). Production geometry unchanged. Edit tests, visual review and approvals remain incomplete; definition stays Testing. No other CAD case ran.

Additional requested integration fixture: BALCONY_DOOR_THRESHOLD explicitly computes slab center from the ground-door center and slab top from the door threshold. It is **blocked by V1's host-height boundary rule**, not an alternative name for BALCONY_MIXED. See [threshold preflight and expected-versus-actual record](results/THRESHOLD_PREFLIGHT_2026-11-03.md). Current runner preflights that case before connecting to CAD and exits 3 on the known limitation. No production policy was changed. Required execution order is BALCONY first; stop on failure and do not run other cases automatically.

Use the manual setup/build procedure in [README.md](README.md). These commands are for an explicitly authorized human CAD session only; automated regression does not execute them:

```powershell
.\tests\bin\Manual\SolidWorksAIBridge.ManualValidation.exe --confirm-cad BALCONY
.\tests\bin\Manual\SolidWorksAIBridge.ManualValidation.exe --confirm-cad BALCONY_MIXED
```

Each invocation creates a fresh unsaved 180 x 220 x 3 mm wall in an already running SolidWorks instance. Count generated sketches, bosses and cuts including absorbed sketches, excluding default tree entries.

| Check | BALCONY expected | BALCONY_MIXED expected |
| --- | --- | --- |
| Slab inputs | Width 56, depth 24, thickness 3, X center 20, elevation 120 | Same |
| Slab model bounds, mm | X [-8,48], Y [7,10], Z [1.5,25.5] | Same |
| Slab volume | 4032 mm^3 | Same |
| Wall bounds | X [-90,90], Y [-110,110], Z [-1.5,1.5] | Same |
| Solid bodies | 2: wall and slab | 2: perforated wall and slab |
| Generated sketches / bosses / cuts | 2 / 2 / 0 (4 total) | 4 / 2 / 2 (8 total) |
| Total solid volume | 122832 mm^3 | 113832 mm^3 |
| Window | None | 30 x 40, center (-50,40), bounds X [-65,-35], Y [20,60] |
| Ground door | None | 30 x 60, center X=0, bottom -110/top -50 |

Slab feature names: BalconySlabSketch_balcony-01 and BalconySlab_balcony-01. Sketch size dimensions: SlabWidth and SlabThickness; placement dimensions: CenterX_balcony-01 and CenterY_balcony-01. Extrusion dimension: SlabDepth. Expected face contact at Z=1.5 must not merge the bodies. Capture front, side, isometric, expanded feature-tree, and two-body evidence.

Pass only after measuring bounds within 0.001 mm, volumes within 0.01 mm^3, expected counts, outward direction, distinct bodies, driving dimensions and error-free rebuild. Check the wall body independently for unchanged thickness/volume (118800 mm^3 plain; 109800 mm^3 mixed). No unintended wall cut or merge is acceptable.

After baseline capture, edit SlabWidth 56 -> 60 and SlabDepth 24 -> 26, then rebuild. Expected slab bounds X [-10,50], Y [7,10], Z [1.5,27.5], volume 4680 mm^3; body count remains 2. Record actual placement-dimension sign behavior as part of CAD validation. Recreate a fresh case rather than calling more modeler operations after manual edits.

Record actuals using README.md's report template. Run the existing WALL/WINDOW/SILL/ROW/GRID/DOOR/DOOR_ROW/INVALID fixtures in the same environment before declaring integrated CAD regression success. A successful generation command or two-body runtime assertion alone is not visual approval or a complete CAD PASS.

## Reproduce parameter edits and elevated-door integration

Build ManualValidation.csproj as above. Each case below creates its own fresh unsaved wall/slab part; it edits only that part. Attach to the existing SolidWorks instance in the desktop user's context. Stop on any nonzero exit; preserve failed parts.

```powershell
.\tests\bin\Manual\SolidWorksAIBridge.ManualValidation.exe --confirm-cad BALCONY_EDIT_WIDTH
.\tests\bin\Manual\SolidWorksAIBridge.ManualValidation.exe --confirm-cad BALCONY_EDIT_DEPTH
.\tests\bin\Manual\SolidWorksAIBridge.ManualValidation.exe --confirm-cad BALCONY_EDIT_THICKNESS
.\tests\bin\Manual\SolidWorksAIBridge.ManualValidation.exe --confirm-cad BALCONY_EDIT_POSITION
.\tests\bin\Manual\SolidWorksAIBridge.ManualValidation.exe --confirm-cad BALCONY_EDIT_ELEVATION
.\tests\bin\Manual\SolidWorksAIBridge.ManualValidation.exe --confirm-cad BALCONY_UPPER_DOOR
```

Expected edits: width 56->60 (volume 4320), depth 24->26 (4368), thickness 3->4 at fixed center Y (5376), X center 20->30 (4032), top elevation 120->130 through center Y 8.5->18.5 (4032). Units: mm and mm^3. Every edit retains 2 bodies / 4 generated features and must rebuild. The independent upper-door fixture uses CreateWindow(30,60,20,40), threshold host elevation 120, and slab 56 x 24 x 3 centered at X=20 with top elevation 120; expect total volume 117432, 2 bodies, 6 features. This reuses rectangular cut geometry, not the ground-door API or a new production Door feature. See the result report for exact bounds and actual outcomes.
