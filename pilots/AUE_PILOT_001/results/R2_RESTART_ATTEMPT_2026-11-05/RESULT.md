# R2 restart — single authorized CAD rerun, 2026-11-05

**Construction/baseline numeric checks PASS, exit 0. Human visual review pending.**

## Execution and preservation

User explicitly authorized one fresh isolated experimental R2 rerun after restart.
Desktop process inventory found SLDWORKS PID 3584, no active pilot/MSBuild; prior
attempt inactive. Runner attached to revision **32.5.0 (SolidWorks 2024)**, with
**0 existing documents**. Created **Part1**, empty path/unsaved, left open after
Front view/Zoom to Fit. No save/close calls. Final document count 1; existing-document
metadata check true (no preexisting documents in this instance).

Command from repository root, executed once in desktop context:
`pilots/AUE_PILOT_001/results/R2_RESTART_ATTEMPT_2026-11-05/run/AuePilot001.R2.exe --confirm-cad-r2`

[Full transcript](CAD_ACTUAL.txt), [exit code](CAD_EXIT.txt).
Separate copied build/run directory has its own CreateNew attempt marker, now
consumed; original failed-attempt marker and logs remain untouched. No retry.
Fresh preceding build and preflight passed; [build](BUILD.txt), [preflight](PREFLIGHT.txt).
Historical 49 focused / 165 legacy tests were not rerun during this CAD session.

## Actual geometry

| Item | Measured result |
| --- | --- |
| Overall extent | X=-10.5..232.5, Y=0..180, Z=-12..8 mm in R2 frame |
| Overall size | **243 wide x 180 high x 20 relief depth mm** |
| Solid bodies | **5** |
| Modeling features / sketches | **11 / 11** |
| Facade after recess/door/right and large windows | 190650.18 mm3; 224.5 x 99.8 x 12 mm bounds |
| Connected left bay base + upper support | 47302.64 mm3; X=0..99.7, Y=0..99.8, Z=0..8 |
| Main gable after upper window | 70328.58 mm3; X=48.4..232.5, Y=99.8..180, Z=-12..0 |
| Left gable | 14548.24 mm3; X=-10.5..105.7, Y=99.8..131.1, Z=0..8 |
| Separate right surround | 1276.8 mm3; X=151.9..182.3, Y=28.1..65.5, Z=0..3 |

Existing wall/right-window builders completed; refreshed native body identity
passed after the cut. Right aperture verified 23.4 x 30.4 mm, offsets 54.85/3.1 mm,
12.2 mm cut, four through-cut side faces and expected aperture bounds.
All nine subsequent pilot operations passed their implemented body-count,
target-volume, native identity and non-target bounds/volume/topology checks.
Bay upper support merged only into bay base; facade remained independent.
Each operation rebuild and parameter-binding rebuild returned success;
**final EditRebuild3 passed**, with locked **actual height 180 mm**.

## Failures, deviations and limitations

No actual CAD/validation failure or construction deviation was reported; no check
was bypassed. Runner completed baseline R2 construction. It does **not** execute
the numeric plan's proposed independent parameter edits/restoration, so those
remain **NOT RUN**. Final report enumerates 22 display dimensions, not a complete
dimension audit. Explicit final aperture-side-face metrology exists for the right
window; remaining operations use their implemented profile/volume/scope checks.
No screenshot/visual assessment or physical-print validation was performed.
Provisional roof intersections, hidden geometry, depth assumptions and photo accuracy
remain unresolved. OpeningSurround stays Draft; no LEGO promotion or full AUE validation.

No source/project edits, production-builder changes, existing CAD file opening,
or Git operations. All **12 root production .cs/.csproj files** in the prepared
snapshot match SHA-256 after execution; [hash result](PRODUCTION_HASH_CHECK.txt).
This snapshot does not include Properties/AssemblyInfo.cs; do not call it a
13-file comparison. Architecture and implementation decisions did not change.

Next: human visual inspection of open Part1 against approved provisional R2.
If accepted, separately scope parameter-edit/restore evidence before any lifecycle
promotion. Do not rerun the consumed experiment or alter Part1 automatically.
