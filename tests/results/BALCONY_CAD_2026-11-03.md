# BALCONY actual CAD validation — 2026-11-03

## Outcome

**CAD executed once. Runner FAILED (exit 1) due to a test-only feature-type classifier defect.** Independent read-only inspection confirms all requested baseline geometry, dimensions, outward direction, solid-body count, generated feature count, and successful rebuild. Do not rewrite the failed runner invocation as PASS. Parameter-edit and visual approval checks were not performed; the definition remains Testing, not Validated.

One new document, **Part1**, was created. It remains open and unsaved (GetPathName returned an empty string). No additional cases were run. No production code was changed and no Git operations occurred.

## Connection and environment diagnosis

- Running SLDWORKS process detected: PID 34776, Windows session 1.
- Initial shell identity: desktop-oqlu1qe\codexsandboxoffline, session 1. Marshal.GetActiveObject failed with **0x800401E3 (MK_E_UNAVAILABLE), Operation unavailable**.
- Read-only retry outside the sandbox as desktop-oqlu1qe\pc successfully acquired the existing COM instance. No launch/CoCreateInstance was used.
- PowerShell dynamic inspection of that COM object then raised **0x8002802B (TYPE_E_ELEMENTNOTFOUND), Element not found**. A compiled typed runner avoided that dynamic type-information issue.
- Compiled --inspect-connection succeeded, exit 0: runtime revision **32.5.0**; registered application is SolidWorks 2024.
- Default Part template: **C:\ProgramData\SolidWorks\SOLIDWORKS 2024\templates\Part.prtdot**, existence verified.
- Front Plane: selected by existing CreateWall and independently found in the generated test part.
- Existing document count immediately before execution: **0**. No pre-existing documents required modification; dirty-state checks completed. No documents were saved, closed, or overwritten, and no second SolidWorks instance was launched.

Diagnosis: the original attachment failure depended on execution identity/environment, not missing application registration or a missing process. Run this CAD validation harness in the interactive user's context, with matching privilege level. Do not change the production connector or use auto-launch to mask sandbox ROT visibility.

## Exact execution

Built tests/ManualValidation.csproj with installed Visual Studio 18 Community MSBuild, Debug configuration. Build succeeded with legacy MSB3305 COM marshaling warnings. Added test-only connection checks, document snapshot checks, measurements and a guarded read-only result inspector; production source was untouched.

Commands executed under the desktop user's identity:

```powershell
.\tests\bin\Manual\SolidWorksAIBridge.ManualValidation.exe --inspect-connection
.\tests\bin\Manual\SolidWorksAIBridge.ManualValidation.exe --confirm-cad BALCONY
.\tests\bin\Manual\SolidWorksAIBridge.ManualValidation.exe --inspect-balcony-result Part1
```

The last command is read-only diagnosis of the already-created unsaved part. It does not create, edit, rebuild, save, or close documents. It verified the expected test-document title, empty path and slab feature before reading.

Case inputs: CreateWall(180,220,3); slab instance balcony-01, host MainWall, width 56, depth 24, thickness 3, X center 20, top elevation 120 above wall bottom. No threshold integration, mixed case, legacy case, or edit variant ran.

## Expected versus actual measurements

Actual miniature mm unless stated otherwise. Bounds were computed from the bodies' actual vertices, not copied from input parameters. Volume came from Body2.GetMassProperties(1)[3], converted from m^3 to mm^3.

| Check | Expected | Actual | Assessment |
| --- | --- | --- | --- |
| Wall bounds X | [-90,90] | [-90,90] | Matches |
| Wall bounds Y | [-110,110] | [-110,110] | Matches |
| Wall bounds Z | [-1.5,1.5] | [-1.5,1.5] | Matches |
| Wall volume mm^3 | 118800 | 118799.99999999999 | Matches within 0.01 |
| Slab bounds X | [-8,48] | [-8,48] | Matches |
| Slab bounds Y | [7,10] | [6.9999999999999991,10.000000000000002] | Matches within 0.001 |
| Slab bounds Z | [1.5,25.5] | [1.5,25.500000000000004] | Matches; projects outward +Z |
| Slab width | 56 | 56.000000000000007 (dimension) | Matches |
| Slab depth | 24 | 24 (dimension) | Matches |
| Slab thickness | 3 | 3.0000000000000027 (dimension) | Matches |
| Slab center X | 20 | 20 from measured bounds | Matches |
| Top elevation above wall bottom | 120 | 120 from measured bounds | Matches |
| Slab volume mm^3 | 4032 | 4032.0000000000014 | Matches within 0.01 |
| Total volume mm^3 | 122832 | 122831.99999999999 | Matches within 0.01 |
| Solid bodies | 2 | 2: MainWall and BalconySlab_balcony-01 | Matches; separate solids |
| Generated sketches | 2 | WallSketch, BalconySlabSketch_balcony-01 | Matches |
| Generated bosses | 2 | MainWall: Extrusion; slab: ICE with underlying Boss | Confirmed by read-only diagnosis |
| Generated cuts | 0 | 0 | Matches |
| Generated features including absorbed sketches | 4 | 4 named generated features | Confirmed |
| Rebuild | True | EditRebuild3 returned True | Matches |
| Existing dirty flags | Unchanged | Check completed; snapshot contained 0 documents | No existing docs affected |

Each body had 8 measured vertices. The small floating-point differences are far below documented tolerances. The contact plane at Z=1.5 did not merge the solids.

## Exact failure and diagnosis

The initial measurement helper accepted only Boss or Extrusion from GetTypeName2 for boss counting. Actual output:

```text
FEATURE=MainWall; TYPE=Extrusion
FEATURE=BalconySlab_balcony-01; TYPE=ICE
GENERATED_SKETCHES=2; BOSSES=1; CUTS=0
TEST_DOCUMENT_REBUILD=True
System.InvalidOperationException: BALCONY measured geometry/feature/body/rebuild expectations failed.
CAD_CASE_EXIT=1
```

Stopped creation/testing at this failure and preserved Part1. Read-only follow-up returned exit 0:

```text
FEATURE=WallSketch; TYPE2=ProfileFeature; UNDERLYING_TYPE=ProfileFeature
FEATURE=MainWall; TYPE2=Extrusion; UNDERLYING_TYPE=Extrusion
FEATURE=BalconySlabSketch_balcony-01; TYPE2=ProfileFeature; UNDERLYING_TYPE=ProfileFeature
FEATURE=BalconySlab_balcony-01; TYPE2=ICE; UNDERLYING_TYPE=Boss
```

This matches the official [GetTypeName2 documentation](https://help.solidworks.com/2022/English/api/sldworksapi/SolidWorks.Interop.sldworks~SolidWorks.Interop.sldworks.IFeature~GetTypeName2.html): use GetTypeName for an ICE feature's underlying type. The classifier defect is in newly added test instrumentation, not evidence of a missing slab boss. It has not been patched to silently relabel this invocation as successful.

Durable numeric results are above. Local ignored diagnostic transcripts: tests/balcony-cad-actual.log, tests/balcony-feature-inspection.log, tests/balcony-cad-build.log, tests/balcony-inspection-build.log. No CAD file or screenshot was saved. No regression suite was rerun in this session; the previous non-COM result remains 97 passing checks from its recorded run.

## Handoff

Stop after this case. Next requested development action should be to correct only the test classifier for documented Instant3D feature handling and verify its measurement/reporting logic without altering production geometry. Do not run more CAD cases automatically. Subsequent approved validation can establish a clean runner pass and optional edit/visual checks. The ground-door threshold limitation is unchanged and was not exercised in CAD.
