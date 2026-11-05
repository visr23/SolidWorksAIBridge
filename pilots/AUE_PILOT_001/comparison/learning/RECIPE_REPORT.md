# First feature-recipe comparison — Draft

2026-11-05. Source CAD is reference evidence, not an authoritative target. This
report distinguishes measured result, observed feature dependency, source-code
intent and untested edit/manufacturing predictions. No recipe is promoted.

## Observed construction

Evidence is [selected native face owners and direct parents](FRONT_EDGE_EVIDENCE.json).
GetParents is direct only; no recursive dependency closure. Native feature
definitions/merge scope flags and all sketch relations were NOT accessed. Named
sketch parents prove dependency links, not full constraint/driving status.
See [official GetParents contract](https://help.solidworks.com/2023/English/api/sldworksapi/SolidWorks.Interop.sldworks~SolidWorks.Interop.sldworks.IFeature~GetParents.html).

| Component | Studio observed construction | Generated R2 construction | What can be learned now |
| --- | --- | --- | --- |
| Main facade / O2 / O3 | Extrude-Thin1 (BossThin) owns front/backing faces; parents Boss-Extrude1, Sketch3. Combine9 (CombineBodies) parents Extrude-Thin12, Split1. Multiple architectural roles share Combine9 | MainWall extrusion; separate WindowCut and three pilot facade cuts. Each has named sketch/host parents | Thin panel vs solid backing + scoped cuts are competing approaches; operation-count comparison is not equivalent without matching scope |
| Left bay / O1 | Boss-Extrude1 (Extrusion) owns support faces, parents Front Plane, Sketch1, Sketch2. Boss-Extrude67 depends on Boss-Extrude1/6/23/24 and Sketch129 | Base boss then U-shaped upper-support boss; native upper-support parents include base and upper sketch. One current bay body; backing opening is separate | One opening spans several features/bodies. Evaluate connected support, clear overlap and base edit semantics before choosing a reusable recipe |
| Main roof | Boss-Extrude17 owns 3 fascia faces; parents Boss-Extrude16, Sketch37. Cut-Extrude33 has direct parents including Boss-Extrude17/52, Cut-Extrude24/Thin5, Sketch116. Split4 is observed but its roles need matching | One named polygon sketch/boss, separate from wall; upper opening is a targeted cut | Source band detail and pilot filled envelope solve different geometric scopes. Simpler shallow profile is promising for blockout, not automatically superior for faithful roof reconstruction |
| Left roof | Boss-Extrude9 owns fascia faces; parents Boss-Extrude8, Sketch27. Cut-Extrude31 depends on roof bosses/cuts and Sketch113 | One triangle sketch/boss | Prefer parameterized silhouette controls for future experiments; do not remove source detail based only on complexity |
| Cornice / right-related projection | Extrude-Thin4 (BossThin) parents source bosses/cuts/Extrude-Thin1/Sketch25; Extrude-Thin3 parents Cut-Extrude7, Extrude-Thin1, Sketch17 | Separate ring boss for OpeningSurround; no independent first-pass sill | Source strips/projection are not yet the same semantic object as S1. Do not call these interchangeable recipes |
| Repetition / scale | Mirror1 is MirrorPattern with LPattern1 and Plane3 parents; Scale1 depends on Boss-Extrude66 | No mirror/pattern needed for three unique first-pass apertures | These are contextual source observations; grille bodies were NOT traversed. Scale1 means pre-scale sketch numbers must not be treated as final dimensions |

Pilot geometry/merge policy also has source-code and successful baseline evidence:
R2 bay upper support merges only into bay base; roofs/surround remain separate,
cuts have explicit targets; relevant non-target invariants passed historically.
Fresh read confirms current body/face ownership and direct parent links, **not** a
new merge/edit experiment. Studio exact historical merge/cut/pattern scopes remain
unknown because selected feature definitions were not read.

## Evaluation, without premature ranking

- **Geometric outcome:** O1/O2/O3 host-panel boundaries are measurable; depth,
  visible region, glass/shutters and full roof matching incomplete. Similarity is
  not photo fidelity; do not optimize towards one CAD merely because it exists.
- **Efficiency:** R2's 11 known modeling features describe this blockout. The
  source's 303 historical tree/subfeature entries include 133 sketches and folders;
  they are NOT 303 modeling operations. No valid total construction-efficiency
  score exists for these unequal component/detail scopes. Local native dependency
  lists support identifying candidate simplifications, not asserting time savings.
- **Parametric editability:** R2 has named dimensions/equations in source and a
  passed baseline. Independent edits/restoration NOT RUN. Studio sketches and
  direct dependencies observed, full constraints/equations and edits NOT RUN.
  Neither path is proven more editable.
- **Rebuild reliability:** NeedsRebuild2=0 in both open snapshots is a status read,
  not an actual successful fresh rebuild. R2 historical construction rebuild passed;
  studio original never rebuilt in this workflow. No robustness ranking.
- **Manufacturing:** source thin walls, split bodies, details and R2 thick backing
  indicate different intent. No printer profile, minimum-section/contact/fit audit
  or physical print qualification. 48 vs 5 bodies is not a quality metric.
- **Reuse:** candidate planar profile, host-scoped aperture, connected bay support
  and nested-profile surround are Draft recipe concepts. Existing validated slab,
  balcony paths and LEGO definitions stay unchanged. Surround remains Draft.

## Draft recommendations

1. Store architectural role independently from body/feature identity; permit a
   body to support several roles and a role to span several bodies.
2. Keep aperture, pane, shutter, surround and sill evidence separate. Do not tune
   an aperture to match a pane/shutter outline by accident.
3. Learn shared-geometry-first only within a matched role/detail/manufacturing scope.
   Prototype shared profile or host-cut alternatives, retaining separate named
   parameters and exact body targeting; do not refactor validated builders.
4. Extract local evidence progressively with budgets/checkpoints; detailed grille
   traversal/mass properties are unnecessary for the current facade task.
5. Preserve competing hypotheses and photographic uncertainty. Do not encode the
   source's square O2, or the AUE tall O2, as a learned rule without new evidence.

Machine-readable feature observations/parents are preserved as Draft recipe
records in [learning JSON](PILOT_LEARNING_RECORD.json). No new recipe execution,
library registration, family promotion or production modification occurred.
