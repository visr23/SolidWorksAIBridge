# R2 isolated CAD attempt — 2026-11-05

**FAILED / STOPPED at first pilot verification failure. One CAD attempt only.**
This tests construction, not architectural accuracy. OpeningSurround stays Draft.

## Actual commands and outcomes

Compatible toolchain: Visual Studio 18 Community full MSBuild, .NET Framework 4.8,
SolidWorks 2024 revision 32.5.0, Windows desktop-user COM attachment.

- `MSBuild.exe pilots/AUE_PILOT_001/runner/AuePilot001.R2.csproj /t:Rebuild /p:Configuration=Debug /nologo /verbosity:minimal`: initial build failed CS0656
  (pilot project omitted Microsoft.CSharp reference); corrected reference, final
  rebuild exit 0, existing MSB3305 COM warnings. [Final build log](R2_BUILD.txt).
- `pilots/AUE_PILOT_001/runner/bin/Debug/AuePilot001.R2.exe --preflight`: exit 0,
  nine operation plans and expected volumes; three negative fixtures reject changed
  height, through-support recess and missing left pier. [Preflight](R2_PREFLIGHT.txt).
  This is focused parameter/preflight coverage, not complete geometry validation.
- `powershell.exe -NoProfile -ExecutionPolicy Bypass -File tests/Run-RegressionTests.ps1`:
  exit 0, **165 passed / 0 failed**, CAD tests NOT RUN by this script.
  [Regression log](R2_LEGACY_REGRESSIONS.txt).
- `pilots/AUE_PILOT_001/runner/bin/Debug/AuePilot001.R2.exe --confirm-cad-r2`:
  **exit 1**, single new unsaved Part1, stopped at CreateWindow_Right verification.
  [Actual transcript](R2_CAD_ACTUAL.txt). No retry, continuation or second Part.

The runner's persistent `R2_CAD_ATTEMPT.txt` guard in its output directory blocks
re-execution. Do not clear it or retry without new explicit authorization. Build
output and guard are local artifacts, not portable validated evidence.

## What actually exists

| Item | Observed result |
| --- | --- |
| New document | Part1; empty path (unsaved); left open |
| Main wall | 224.5 wide x 99.8 high x 12 thick; vertex bounds verified |
| Right window | Existing CreateWindow(23.4,30.4,54.85,-3.1) completed successfully, including its dimension/placement and rebuild checks; width 23.4 also enumerated in transcript |
| Right opening cut depth | 12.2 mm, enumerated |
| Modeling features | **2**: MainWall and WindowCut |
| Sketch features | **2**: WallSketch and WindowSketch |
| Solid bodies | **1**, reported body name WindowCut |
| Remaining solid volume | 260324.88000000003 mm3; matches wall minus rectangular aperture analytically |
| Actual envelope | X=0..224.5, Y=0..99.8, Z=-12..0 in R2 frame |
| Height | **99.8 mm actual; 180 mm NOT reached** |
| Rebuilds | Wall and right-window builder calls both completed their required successful rebuilds; full R2 final rebuild NOT reached |

The report enumerated four display dimensions, not all dimensions in the model.
Opening height/placement above are supported by the successfully returned existing
builder's internal parameter/rectangle checks, not a separate final metrology run.
No extra rebuild or parameter edit was attempted after failure.

## Failure and deviations from R2

Pilot checker looked up a body by the label MainWall after the window cut.
SolidWorks had renamed the resulting body WindowCut, so lookup threw:
`Cannot resolve exclusive body MainWall (MainWall)`.
This is a **pilot identity-tracking defect**, not an observed failure of the existing
wall/window builders. Stop-on-first-failure was honored; no geometry repair/retry.
Only two planned modeling features exist instead of the complete eleven.

Not executed: parameter bindings, entrance recess, door opening, large opening,
left-bay base/piers/header, both gable profiles, upper gable opening and surround.
Expected five bodies were not reached. Parameter edits/restore, final height,
remaining operation volumes, merge/cut isolation and full-model review NOT RUN.
No architectural accuracy, successful complete blockout or LEGO promotion claimed.

## Preservation and visual review

Existing document snapshot: **0 before, 1 after** (the new Part1). Thus no studio
CAD document was open in this instance during the run. No save/close/open-existing
calls are in the runner. Thirteen production .cs/.csproj hashes matched their
pre-CAD snapshot after execution; production implementations were not edited.
The isolated project references the existing production project without adding
pilot sources to its Compile entries or changing its entry point.

Runner requested Front view/Zoom to Fit for Part1. Computer-use capture was
occluded by other windows; a clean model screenshot was not obtained. During
capture recovery the user stopped Computer Use with physical Escape. No further
computer-use calls or CAD control followed. Part1 remains open for direct user
inspection. Screenshot-based visual validation is incomplete.
A read-only InspectPartialResult.ps1 helper was written but **NOT RUN**, and grants
no further CAD access authorization. Do not substitute its existence for evidence.

## Handoff

Next development step: correct pilot body tracking using an explicit current
feature/face-to-body resolution, not mutable body labels; add a focused regression
for a renamed post-cut body. Also improve full dimension enumeration. Preserve
this transcript/executed sources. Obtain new authorization before another CAD
attempt or modifying this partial Part. Unsupported pilot operations remain
compiled but CAD-untested. Roof intersections/dimensions remain provisional.
