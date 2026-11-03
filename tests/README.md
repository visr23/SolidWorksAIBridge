# Regression baseline

Latest CAD result: **BALCONY_BOTH_WALLS PASS**, exit 0, one new unsaved Part9, measured 5952 mm^3 balcony, two solids/eight features, depth 24->26->24 verified. [Actual result](results/BALCONY_BOTH_WALLS_CAD_2026-11-03.md) preserves the transcript. Assembly remains Testing pending user visual review; no broader side-mode validation is implied.

Latest addition: [BalconyAssembly V1 both-wall implementation](BALCONY_ASSEMBLY_V1.md), lifecycle Testing. **141 non-CAD tests passed** (105 existing + 36 assembly). [Implementation checkpoint](results/BALCONY_ASSEMBLY_IMPLEMENTATION_2026-11-03.md) records successful builds; BALCONY_BOTH_WALLS CAD remains NOT RUN pending explicit approval. It is not promoted by the standalone slab's Validated status.

Current status: **Projecting Balcony Slab V1 is Validated within its exact tested scope**, following explicit user visual approval of Part3-Part8. The [scope record](results/PROJECTING_SLAB_V1_VALIDATED_2026-11-03.md) supersedes earlier pending lifecycle statements, not historical results. Transcripts are preserved with hashes. A metadata-only promotion check passed all 105 non-COM tests; no additional CAD ran. Arbitrary combinations, hosts and future ground-floor behavior are not certified by this label.

This baseline protects existing behavior without changing production geometry. Automated tests never connect to, launch, or control SolidWorks. Manual CAD fixtures are a separate opt-in executable and are NOT run by the regression script.

Projecting Balcony Slab V1 adds 42 non-COM checks (95 total) and two opt-in CAD fixtures. See [slab contract/cases](PROJECTING_BALCONY_SLAB_V1.md) and [slab test result](results/PROJECTING_SLAB_2026-11-03.md). The baseline's 53 existing checks remain intact; the new partial modeler file adds slab execution without changing existing geometry methods.

Threshold integration follow-up: 97 checks now pass (two fixture/preflight checks added). The requested ground-door threshold slab requires elevation 0 and is blocked by V1's minimum elevation equal to slab thickness. [Preflight record](results/THRESHOLD_PREFLIGHT_2026-11-03.md) distinguishes this limitation from actual CAD results. BALCONY_DOOR_THRESHOLD is prepared in the manual harness but must not be substituted for a successful CAD test or executed ahead of BALCONY.

Actual BALCONY follow-up: [CAD result](results/BALCONY_CAD_2026-11-03.md) records matching baseline geometry and rebuild, but runner failure from an Instant3D feature-classification defect. All other CAD cases remain NOT RUN. Test-only --inspect-connection reads runtime/template/document count; --inspect-balcony-result <title> reads an existing unsaved test part's known slab features and dimensions without mutation. Execute COM checks in the interactive desktop user's context: the sandbox identity could not see the running object even though the process was visible. Do not launch a second instance to work around this.

Latest classifier correction: [successful rerun report](results/BALCONY_CLASSIFIER_RERUN_2026-11-03.md) records **105 automated checks passed** and **BALCONY numeric baseline PASS**, exit 0. Test-only classification queries GetTypeName for ICE wrappers and counts each feature name once. The documented parameter-edit/visual approval work remains NOT RUN; passing the baseline is not full LEGO validation. Earlier failure reports are retained.

Latest edit/integration validation: [actual results](results/BALCONY_EDITS_UPPER_DOOR_2026-11-03.md) records **five parameter edits plus elevated-door integration PASS**, each in a fresh document, and a fresh **105/0** automated run. Reproduction commands and semantics are in PROJECTING_BALCONY_SLAB_V1.md. Human visual review/approval is still pending; no validated-library registration occurred. Earlier NOT RUN statements describe historical sessions.

## Run non-COM tests

Prerequisites: Windows, .NET Framework 4.8 developer/targeting tools, compatible Visual Studio MSBuild, and registered SldWorks/SwConst version 32 type libraries. No NuGet/test framework dependencies are installed. SolidWorks need not be running, but its type libraries are required to compile the unchanged modeler. This is not yet a test suite portable to a machine without those references.

From the repository root:

```powershell
powershell.exe -NoProfile -ExecutionPolicy Bypass -File .\tests\Run-RegressionTests.ps1
```

The execution-policy flag affects only that child process; it does not change machine policy. If organizational policy prohibits it, run the script through an approved policy instead. An explicit build-tool path can be supplied:

```powershell
.\tests\Run-RegressionTests.ps1 -MSBuildPath 'C:\path\to\MSBuild.exe'
```

The script rebuilds the test project, then runs its executable. It fails on build errors or failed assertions. It never runs production Program.Main. Do not substitute dotnet build: this project requires the full-framework COM resolution task. bin/obj/log output is ignored by the existing repository ignore rules.

### Coverage and boundaries

The project links current UnitConverter.cs, SolidWorksModeler.cs, and SolidWorksConnector.cs rather than loading stale application binaries. Assertions execute production methods, not copied conversion/layout formulas.

- Conversion of dimensions, zero and signed positions; NaN/infinity rejection.
- Positive/nonnegative/position helpers, including underflow to zero.
- Public wall/window/ground-door early parameter guards; missing-wall rejection.
- Door-row null/empty/mismatched arrays; missing-wall rejection.
- Row/grid count and gap guards.
- Opening-cut tolerance setter acceptance/rejection and preservation after failure.
- Actual stored rectangle extents, overlap rejection, shared-edge/corner acceptance, and overlap tolerance boundaries.
- Null modeler application and connector access before Connect.

Private numeric/bounds helpers are invoked through reflection. Tests allocate an uninitialized modeler with a null application and inject only an empty bounds list for bounds tests. They deliberately bypass the COM constructor, never fabricate a successful wall, and never call a geometry path past the missing-wall guard. Renaming private helpers may require updating the harness. This is a minimal characterization baseline, not a recommended architecture for future pure logic.

Not covered automatically: successful wall/window/door creation, containment checks reached after a wall exists, full row/grid layout and spacing generation, geometry-based direction, default field initialization on a valid COM-backed modeler, active-document handling, sketch constraints, body scope, volume, rebuilds, or parameter-edit propagation. These require the manual cases below. Do not infer tested default cut depth merely from setter tests.

## Manual SolidWorks procedure — explicit human execution only

Do not execute these commands during an automated test run. The operator must explicitly authorize and start a manual CAD validation session. The runner attaches to a manually started application; it does not launch SolidWorks. It creates one new unsaved part per invocation and leaves it open. It never saves or closes other documents.

1. Build the manual runner only (building does not connect to CAD):

```powershell
& 'C:\path\to\MSBuild.exe' .\tests\ManualValidation.csproj /t:Rebuild /p:Configuration=Debug /nologo
```

2. Manually start SolidWorks 2024 with matching privilege level. Configure a default Part template containing Front Plane. Record exact version/service pack/template. Keep unrelated documents out of the test workflow.
3. Explicitly invoke one case, for example:

```powershell
.\tests\bin\Manual\SolidWorksAIBridge.ManualValidation.exe --confirm-cad WINDOW
```

4. Inspect the new part in front, side/section, and isometric views. Measure using model coordinates, expand absorbed sketches, inspect cut depth and dimensions, and rebuild. Case generation exit code 0 is NOT a visual/geometry PASS.
5. Record actuals and evidence, then use a fresh invocation/new part for the next case. No edits to production Program.cs are needed. The manual runner is separate test-only code.

All dimensions below are actual miniature mm. Every case begins with CreateWall(180,220,3), centered bounds X [-90,90], Y [-110,110], Z [-1.5,1.5]. Default opening cut depth should be 3.2 mm inward from Z=1.5, nominally ending at -1.7; the physical wall remains bounded at -1.5. Expect one solid body in each valid case.

Feature counts below include only generated sketches, bosses and cuts, including absorbed sketches; exclude default planes/origin/folders/history. Counting visible top-level tree rows alone is incorrect.

| Case | Operations after wall | Expected geometry | Sketch / boss / cut counts | Expected volume mm^3 |
| --- | --- | --- | --- | --- |
| WALL | None | 180 x 220 x 3 wall; WallSketch/MainWall | 1 / 1 / 0 (2 total) | 118800 |
| WINDOW | CreateWindow(30,40,-20,30) | Aperture X [-35,-5], Y [10,50], through wall; WindowSketch/WindowCut | 2 / 1 / 1 (4 total) | 115200 |
| SILL | Same window; CreateWindowSill(36,3,2) | Sill X [-38,-2], Y [7,10], Z [1.5,3.5]; WindowSillSketch/WindowSill | 3 / 2 / 1 (6 total) | 115416 |
| ROW | CreateWindowRow(3,20,30,10,0,20,true,3,2) | Centers X -30,0,30; Y 20; opening Y [5,35]; sill Y [2,5], outward 2; clear gap 10 | 3 / 2 / 1 (6 total) | 113760 |
| GRID | CreateWindowGrid(2,3,20,30,10,20,0,10,true,3,2) | Centers X -30,0,30; floor Y -15,35; pitch Y 50, clear gap 20; sill centers Y -31.5,18.5 | 5 / 3 / 2 (10 total) | 108720 |
| DOOR | CreateDoor(30,60,0) | Aperture X [-15,15], Y [-110,-50]; bottom open; DoorSketch_01/DoorCut_01 | 2 / 1 / 1 (4 total) | 113400 |
| DOOR_ROW | CreateDoorRow([25,35,45],[55,65,60],[-55,0,55]) | X [-67.5,-42.5],[-17.5,17.5],[32.5,77.5]; tops -55,-45,-50; bottoms -110; DoorRowSketch_01/DoorRowCut_01 | 2 / 1 / 1 (4 total) | 99750 |
| INVALID | Window touching right edge; door taller than wall; two door-row openings touching | Three ArgumentException-family rejections; no features added after wall | 1 / 1 / 0 (2 total) | 118800 |

ROW names: WindowRowSketch_01, WindowRowCut_01, WindowRowSillSketch_01, WindowRowSills_01. GRID names use Floor_01 and Floor_02 with suffixes _WindowSketch, _WindowCut, _WindowSillSketch, _WindowSills. Each grid floor owns one cut, not one cut per opening.

INVALID inputs are CreateWindow(30,40,75,0), CreateDoor(30,221,0), and CreateDoorRow([30,30],[60,60],[-15,15]). The existing manual runner checks rejection type; the operator checks the unchanged tree/body. An unexpected success makes the case FAIL, even if generation exits before later checks.

### Success criteria and edits

- Fresh baseline measurements within 0.001 mm; volume within 0.01 mm^3. These are initial CAD numeric-check tolerances, not printer tolerances.
- Exact generated feature counts/names, one solid, fully cut apertures, expected extrusion direction, no rebuild errors, and driving size/position dimensions where specified.
- Window-row openings share sizes/alignment and clear gaps; floors remain separately editable. Inspect constraints rather than assuming the entire sketch is fully defined.
- After capturing fresh measurements, edit wall Width 180 -> 200 and rebuild (WALL); window width 30 -> 32 and rebuild (WINDOW); door height 60 -> 65 and rebuild (DOOR). Expected edited window X [-36,-4], Y [10,50]; door bottom -110/top -45. Feature counts remain unchanged. Restore or discard test parts afterward manually.
- Do not call additional modeler operations after manual dimension edits: its stored wall sizes/bounds are not a live geometry database. Do not claim standalone sill associativity beyond what an actual edit test demonstrates.
- Verify temporary dimension prompts/preferences are restored and no unrelated documents changed. Capture front/side/isometric and feature-tree evidence.

## Record results

Create a dated Markdown report under tests/results/ (or update the baseline report with a new clearly dated entry). Preserve previous results. Record:

```text
Run ID / operator / date:
OS, .NET, MSBuild, SolidWorks version/service pack, template, privilege level:
Source file SHA256 values (and revision if known; no Git required):
Commands, exit codes, build warnings:
Automated: passed/failed count, transcript path:
Manual case ID: NOT RUN | PASS | FAIL | BLOCKED
Expected vs actual dimensions / volume / feature counts / body count:
Rebuild and edit outcomes:
Screenshots / reports / CAD artifact paths:
Deviations, explanation, next action:
```

Use Get-FileHash on the tested sources to identify exact content. Logs are ignored by existing .gitignore; keep a durable Markdown summary and deliberately retained evidence links. Never replace NOT RUN/UNKNOWN with PASS without actual measurements. This baseline seeds later Golden Dataset work; it is not a validated reusable LEGO library.
