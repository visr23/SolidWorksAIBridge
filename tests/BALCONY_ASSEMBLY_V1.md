# BalconyAssembly V1 — first both-wall implementation

Definition lego.balcony.projecting-assembly 1.0.0: **Testing**, not Validated. Both-wall source and non-CAD checks exist; the approved **BALCONY_BOTH_WALLS CAD case now PASSED**, including depth 24->26->24. See [actual CAD results](results/BALCONY_BOTH_WALLS_CAD_2026-11-03.md). Fresh unsaved Part9 is ready for visual review; human approval is pending. Left-only/right-only/none modes remain planned. The standalone scoped Validated slab implementation and evidence are unchanged. Earlier NOT RUN wording below describes the pre-execution checkpoint.

## Public API

```csharp
modeler.CreateWall(180, 220, 3);
var slab = new ProjectingBalconySlabInstance(
    "both-walls_slab", "MainWall", 56, 24, 3, 20, 120);
var assembly = new BalconyAssemblyInstance(
    "both-walls", slab,
    20, 2, // left height and thickness
    20, 2, // right height and thickness
    0.8);  // explicit printer-profile minimum, fixture choice only
modeler.CreateBalconyAssembly(assembly);
```

All inputs are resolved actual miniature mm. Side heights are above slab top. Slab top elevation is above the host bottom, using the existing slab contract. Left/right height and thickness are independent; this first API requires both walls. Assembly and child slab IDs must differ. Assembly IDs use 1-40 safe characters starting with a letter.

BalconyAssemblyDefinition.Validate produces an immutable analytic plan using the unchanged slab preflight. It checks dimensions, IDs/host, minimum printable sections, positive clear width, wall containment within current host height, finite/distinct coordinates, contact area and volume. Its CadStatus remains NotRun. Analytic face contact is not proof that SolidWorks has actually merged geometry.

## Geometry execution and safety

- New partial file SolidWorksModeler.BalconyAssembly.cs calls CreateProjectingBalconySlab unchanged, then creates independently named left/right sketches and Boss-Extrudes. No Boolean Combine.
- Standard FeatureExtrusion3 uses **Merge=true, UseFeatScope=true, UseAutoSelect=false**. The sketch is selected with mark 0, and only the current non-facade balcony body with **mark 8**. Exactly one mark-8 target is required. Re-resolve after each merge using COM identity relative to the facade body.
- Rear profile plane is the existing facade exterior face; initial walls run from that plane to slab front, with full-area bottom contact to slab top. The current host must be unedited/front-facing and have one solid before assembly creation. Active-document/sketch/name/known-footprint conflicts reject before geometry where possible.
- Depth equations reference the slab's named SlabDepth. Wall-center equations reference slab width/placement/thickness and their own height/thickness, maintaining side placement/bottom attachment for edits in the supported coordinate branch. Sizes remain independent named dimensions; depth/placement are dependent.
- After each wall, rebuild and require exactly two solids, unchanged facade vertex bounds/volume, expected balcony volume and genus-zero Euler topology. These guards fail closed; no auto-merge-all or Combine fallback. Failures remain open for inspection, without rollback/save/close.
- Side-wall footprints are registered after complete success. Existing cached-bounds limitations remain: do not call more modeler operations after manually resizing/reorienting host or generated dimensions.

API reference: [FeatureExtrusion3](https://help.solidworks.com/2024/english/api/sldworksapi/solidworks.interop.sldworks~solidworks.interop.sldworks.ifeaturemanager~featureextrusion3.html) documents selected-body mark 8. With Merge enabled, a scope-body getter may not expose a normal selected-body array; actual separation/shape/volume checks are required rather than assuming a body-list query proves safety. The merge flags, equation API behavior and body references compile but await actual CAD execution.

## Connectivity and printing

The pure plan puts slab Y [-T,0], side walls Y [0,H], all Z [0,D], with left/right thickness inside the slab edges. Each wall contacts the slab over thickness*depth, positive area. Actual CAD fixture checks a single U-shaped solid through measured corners, volume, two-body isolation and Euler topology. This is a bounded prismatic test, not a universal topology/connectivity validator.

Preferred print direction is architectural +Y mapped to printer +Z; subtract slab-bottom Y to place the underside on the build plate. All planned wall material is above that plate, supported by the slab footprint. Minimum sections are checked against the supplied profile; **physical printing/support/material qualification is not claimed**. The fixture's 0.8 mm minimum is not a universal default.

## Automated coverage and actual status

Run from repository root:

```powershell
powershell.exe -NoProfile -ExecutionPolicy Bypass -File .\tests\Run-RegressionTests.ps1
```

Final result: **141 passed, 0 failed** (existing 105 plus 36 assembly tests). Coverage: both-wall bounds/volume, independent dimensions, positive contact, zero rear/front gaps, derived depth and exact depth-equation reference, underside/build direction, signed initial placement, IDs, null/missing dependencies, invalid/nonfinite/underflow input, minimum sections, clear width and host-height/width limits. No COM geometry operations execute in this suite. Actual CAD merge/connectivity and depth propagation remain unverified.

Manual runner and full application Debug builds passed with MSB3305 legacy COM marshaling warnings. An initial new-guard underflow test exposed a direct-division precision issue; the new assembly guard now uses the existing UnitConverter path and final suite passes. No validated slab or legacy method was changed to fix it.

## Single CAD case — approval required, NOT RUN

Build only:

```powershell
& 'C:\path\to\MSBuild.exe' .\tests\ManualValidation.csproj /t:Rebuild /p:Configuration=Debug /nologo
```

After explicit user approval, in the interactive desktop user's context with SolidWorks already running:

```powershell
.\tests\bin\Manual\SolidWorksAIBridge.ManualValidation.exe --confirm-cad BALCONY_BOTH_WALLS
```

This creates exactly one new unsaved Part, snapshots existing document dirty flags, and never launches SolidWorks or saves/closes documents. It stops on failure and preserves the new part. It checks the baseline, edits slab depth 24->26 to verify both wall depths follow, then restores 24 and checks again. Final part is the baseline for visual review. No other CAD case runs automatically.

### Expected baseline (all values actual miniature mm)

| Item | Expected |
| --- | --- |
| MainWall | X [-90,90], Y [-110,110], Z [-1.5,1.5]; volume 118800 mm^3 |
| Slab | 56 x 24 x 3; X [-8,48], Y [7,10], Z [1.5,25.5] |
| Left side | thickness 2, height 20; X [-8,-6], Y [10,30], Z [1.5,25.5] |
| Right side | thickness 2, height 20; X [46,48], Y [10,30], Z [1.5,25.5] |
| Rear gap / front alignment | zero / both at Z=25.5 |
| Wall-slab contact | 48 mm^2 each |
| Merged balcony volume | 4032 + 960 + 960 = **5952 mm^3** |
| Total volume | **124752 mm^3** |
| Final solids | **2**: independent MainWall and BalconyAssembly_both-walls |
| Generated features, including absorbed sketches | **4 sketches + 4 bosses = 8**; zero cuts/Combine |
| Connectivity | one U-shaped balcony solid; all 16 expected rear/front U-section vertices; V-E+F=2 |
| Print plate mapping | model underside Y=7 -> printer Z=0; wall top printer Z=23 |
| Rebuild | True after each wall and final verification |

At depth 26: side fronts Z=27.5; balcony volume **6448**, total **125248 mm^3**; same heights/thicknesses/features/two bodies. Both SideWallDepth dimensions must read 26 through equations, then return to 24 on restore. Tolerances: 0.001 mm and 0.01 mm^3.

Required feature names:

```text
WallSketch / MainWall
BalconySlabSketch_both-walls_slab / BalconySlab_both-walls_slab
BalconySideWallSketch_both-walls_Left / BalconySideWall_both-walls_Left
BalconySideWallSketch_both-walls_Right / BalconySideWall_both-walls_Right
```

Inspect Merge Result/selected-body settings and independent feature editing, facade separation, upright side walls, flush rear/full-depth front and underside-down print orientation. Record actual results/screenshots and seek visual approval separately before promotion. No side-wall CAD measurements or approval exist yet.

## Known implementation limits

Both sides only; no rails/recesses/patterns/ground-floor support. Assembly body identity, selected merge behavior and equation solving are untested in actual CAD. Coordinate equations retain initial sign branches, matching the legacy center-dimension convention; cross-origin parameter edits are not supported/validated. Pure acceptance outside the slab's sampled validated scope does not confer CAD approval. Printer minimums are explicit but no manufacturing pipeline is implemented.
