# BALCONY_TOP_FACE_COMPARE — actual CAD PASS (2026-11-04)

User explicitly approved the two documented comparison parts against the existing
SolidWorks instance. Comparison runner **exit 0**. Subsequent read-only sketch/boss
inspection **exit 0**. Lifecycle remains **Testing**, human visual review **PENDING**.

## Execution and document preservation

- Attached to existing SolidWorks revision **32.5.0**; no instance was launched.
- Initial document count **1**. Created only **Part2** (unchanged eight-feature
  baseline) and **Part3** (optimized six-feature candidate), both unsaved.
- Existing document dirty-flag checks passed after each case. Final read-only
  inventory contained exactly three documents: original **Part1**, saved and
  **DIRTY=False**, plus unsaved **Part2/Part3**. No saves/closes/exports or edits
  to the original document. Both comparison parts remain open at restored values.
- No production code changes, project changes, Git operations, or additional CAD
  geometry cases. Preserved slab/baseline source and baseline CAD-case hashes still
  match the pre-implementation values.
- Sandbox read-only attachment failed with **0x800401E3** before creating any
  document. The single desktop-user comparison execution then succeeded.

Executed command:

```powershell
.\tests\bin\Manual\SolidWorksAIBridge.ManualValidation.exe --confirm-cad BALCONY_TOP_FACE_COMPARE
```

## Measured results

Host: 180 x 220 x 3 mm. Slab: 56 x 24 x 3 mm, X center 20, top Y=10
(120 above host bottom). Both walls height 20 mm, thickness 2 mm.

| Part / state | Balcony volume mm³ (raw measured) | Solids | Sketches + bosses | Rebuild |
| --- | ---: | ---: | --- | --- |
| Part2, depth 24 | 5952 | 2 | 4 + 4 | PASS |
| Part2, depth 26 | 6447.9999999999982 | 2 | 4 + 4 | PASS |
| Part2, depth restored 24 | 5952 | 2 | 4 + 4 | PASS |
| Part3, depth 24 | 5952.0000000000055 | 2 | 3 + 3 | PASS |
| Part3, depth 26 | 6447.9999999999964 | 2 | 3 + 3 | PASS |
| Part3, depth restored 24 | 5952.0000000000027 | 2 | 3 + 3 | PASS |
| Part3, width 60 / depth 24 | 6240.0000000000036 | 2 | 3 + 3 | PASS |
| Part3, width restored 56 | 5952.0000000000018 | 2 | 3 + 3 | PASS |

All states passed the existing numeric tolerances (volume .01 mm³, bounds/corners
.001 mm). MainWall volume stayed **118799.99999999999 mm³** and bounds stayed
X [-90,90], Y [-110,110], Z [-1.5,1.5]. Each balcony remained one connected solid
with V/E/F=16/24/10 and Euler characteristic 2. No cuts or Combine features.

All 16 rear/front U-profile corners matched in every state. Both walls remained
flush at facade **Z=1.5** and reached slab front **Z=25.5**, moving to **27.5** at
depth 26 and returning on restoration. Baseline width bounds were X [-8,48];
candidate width 60 moved them to [-10,50] and back. Slab underside/top remained
Y=7/10; wall tops Y=30. Candidate checks passed for common WallHeight=20,
independent Thickness_Left/Right=2, both equation-driven depths and X/Z centers.
Depth propagation passed in both parts; width propagation was tested in Part3
only, as documented. No manual or other baseline width test is implied.

## Read-only feature inspection

Added standalone test diagnostic [TopFaceResultInspection.cs](../TopFaceResultInspection.cs),
compiled separately with the .NET Framework compiler against the existing manual
runner and embedded registered interop. It is not included in either runner's
project and has an explicit `--inspect-part3` guard. It only reads Part3 and the
document inventory; it never activates, selects, rebuilds, rolls back, modifies,
saves or closes documents. Initial compilation required correcting the interop
`GetReferenceEntity` argument from `out` to `ref`; final compilation succeeded.

Actual readback:

- Sketch support entity type **2 (face)**; normal **(0,1,0)**.
- One `BalconyTopWallSketch_both-walls` with **8 profile edges and 4 construction
  diagonals**. All endpoints transformed to model coordinates lie on **Y=10 mm**.
- `BalconyTopWalls_both-walls`: **Merge=True, FeatureScope=True, AutoSelect=False**;
  blind depth **20 mm**, ReverseDirection=False, model extrusion direction **+Y**.
- Candidate dirty flag unchanged during inspection.

The creation path selected exactly the exclusive balcony body with mark 8 and
checked selection count=1 before the boss. Stored flags plus unchanged MainWall,
two-body count and measured balcony geometry support isolation in these sampled
edits. **The stored scope-body list was not independently enumerated**: its getter
returned **-1** without AccessSelections. No rollback/AccessSelections was performed
to obtain that list. This is a readback limitation, not an observed merge failure;
do not claim that a body-list enumeration succeeded. The SolidWorks documentation
[describes limitations of scope-body readback for merged features](https://help.solidworks.com/2024/english/api/sldworksapiprogguide/Overview/Programming_Multibody_Parts.htm?id=1.4.39.1).

## Evidence and next action

Preserved [comparison transcript](evidence/TOP_FACE_CAD_2026-11-04/comparison-actual.txt),
[read-only inspection](evidence/TOP_FACE_CAD_2026-11-04/inspection-actual.txt), and
[SHA256 manifest](evidence/TOP_FACE_CAD_2026-11-04/SHA256SUMS.txt).
The previous **165 passing non-CAD checks** were not rerun in this CAD-only session;
no application or manual-runner rebuild was necessary or performed.

Stop for user visual review of **Part2 versus Part3**, especially the shared top-face
sketch, upward common boss, correct body separation, rear/front alignment, editable
parameters and underside-down print orientation. No promotion/replacement or further
CAD mutation is authorized by this result. Physical printing is unverified; no
saved CAD or screenshots were created as evidence.
