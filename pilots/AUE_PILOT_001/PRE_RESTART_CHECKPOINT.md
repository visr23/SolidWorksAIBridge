# Pre-restart checkpoint — AUE Pilot #001 (2026-11-05)

## Preserved baseline

User manually saved and visually accepted R2 as an **experimental front-facade
baseline only**, with correct 180 mm architectural height. Not final architectural
geometry or a Validated LEGO recipe.

Verified existing native file:
[AUE_PILOT_001_FRONT_V1.SLDPRT](solidworks%20reasults/AUE_PILOT_001_FRONT_V1.SLDPRT).
Actual directory spelling is **solidworks reasults**, not solidworks results.
Repository-relative path:
`pilots/AUE_PILOT_001/solidworks reasults/AUE_PILOT_001_FRONT_V1.SLDPRT`.

- File exists, **151934 bytes**.
- Last write **2026-11-05 12:41:53 Asia/Jerusalem**, UTC 10:41:53.6786787Z.
- SHA-256 `CC352E435F9D971048795E2AD248C6F2FEFF1B55927C9CA8BFFDF08D8F86A8AE`.
- [Machine-readable manifest](comparison/SAVED_BASELINE_MANIFEST.json).
- No CAD file saved, copied, overwritten, renamed or rebuilt during this checkpoint.

## CAD evidence and unsaved state

[Successful construction report](results/R2_RESTART_ATTEMPT_2026-11-05/RESULT.md)
and [full transcript](results/R2_RESTART_ATTEMPT_2026-11-05/CAD_ACTUAL.txt) remain intact.
Construction exit 0; 5 solids, 11 modeling features, 11 sketches, 243 x 180 x 20 mm;
per-operation and final rebuilds passed in that run. Historical 49 focused / 165
legacy checks were not rerun for this checkpoint.

Subsequent [read-only native inspection](comparison/CAD_EVIDENCE.json) identified
that exact open saved path, 5 bodies and 11 named modeling features; ICE wrappers
resolved to native Cut/Boss types. Vertex extrema in model mm:
X=-122.75..120.25, Y=-49.9..130.1, Z=-6..14; hence 243 x 180 x 20 mm.
R2 frame = model-mm + (112.25,49.9,-6). Dimensions include wall 224.5 x 99.8 x 12,
right window 23.4 x 30.4, offsets 54.85/3.1, cut 12.2; pilot depths 6,6.2,12.2,8,
8,12,8,12.2,3. Inspection queried NeedsRebuild2=0; no fresh rebuild performed.
Dimension enumeration is partial, not a complete native-dimension audit.

**Open generated document dirty=True**, unchanged by read-only inspection; latest
SolidWorks title also shows `AUE_PILOT_001_FRONT_V1 *`. This means unsaved document
state remains. What changed since the manual save is UNKNOWN; do not assume it is
only view state or geometry. Saved-file equivalence to current memory has NOT been
proven. User instructed no saves; none attempted in this checkpoint. User must
resolve the dirty-state prompt before restart if those changes should be retained.

## Running work / interrupted preservation

Desktop process check: SLDWORKS PID 3584 and sldworks_fs PID 3516 running; no
AuePilot/Inspector/MSBuild process. No additional CAD test was launched.
Earlier separate-copy command was interrupted at approval; no PRESERVATION log,
no second evidence snapshot, and preserved/ directory empty. No extra CAD copy
exists. Do not resume that command. Inspector source currently includes an optional
copy branch: **do not invoke its two-argument mode** under current instructions.
PowerShell late-bound COM failed TYPE_E_ELEMENTNOTFOUND; typed read-only helper
built and executed successfully after correcting return type/reference build
errors. These are helper build/access issues, not failed geometry tests.

## Files and next session

[Restart inventory](comparison/RESTART_FILE_INVENTORY.json) verifies 16 nonempty
pilot source/project, test report and native inspection files on disk with hashes.
No claim is made about unsaved editor buffers, which were not inspected. Pilot
source and reports listed in the inventory are saved. Original regression evidence
and production builders were not modified. No Git operations.

Reference CAD comparison is **pending**, not executed. The original studio
Part1.SLDPRT path is still unconfirmed. `cad_tests/Part1.SLDPRT` exists but has not
been identified as the original studio model; do not infer identity from filename.
No studio file opened/modified. User was asked for its exact path.

Exact next action after restart: confirm which saved baseline revision to use and
verify its manifest hash; obtain original studio Part1.SLDPRT path, then plan safe
read-only front-facade extraction. Define facade normal, vertical and horizontal
anchor; align in comparison data only, normalize reference architectural height to
180 mm using one uniform scale and retain raw geometry. Exclude context/base and
side/rear geometry. Compare roof polylines, aperture centers/widths/heights, entrance
recess depth, projecting masses and surround borders/projection with provenance,
units and uncertainty. Record measured deviations only after reference registration;
visual estimates and unknowns remain separate. No CAD transforms, edits, tests or
LEGO promotion automatically authorized.

## Reference CAD identified — 2026-11-05

User supplied the Google Drive search location and explicitly identified this
Part1 as the original studio reference for Trumpeldor 15, Kfar Saba. Exact file
metadata was verified read-only through the shortcut target; no native CAD open,
rebuild, save, configuration change or content inspection was performed.

- Source root alias: `studio-buildings` = Google Drive shortcut target
  `1JLCVKHnaLXjxYGaZJM5xziEyy9a6prT4`, folder `בניינים`.
- Source relative to that root: `טרומפלדור 15 כפר סבא/Part1.SLDPRT`.
- Local resolved path at verification:
  `G:\.shortcut-targets-by-id\1JLCVKHnaLXjxYGaZJM5xziEyy9a6prT4\בניינים\טרומפלדור 15 כפר סבא\Part1.SLDPRT`.
- Exists; **15936633 bytes**; last modification **2026-10-25 15:11:17 local**.
- Source hash/native geometry/configuration/facade orientation still unverified.
- Earlier statements that the original source path is unknown are superseded by
  this identification. `cad_tests/Part1.SLDPRT` is not the selected studio reference.
- Exact next step: inspect the explicitly selected source and saved R2 baseline
  through a safe read-only path; confirm saved-state/configuration and facade datums
  before extracting aligned front-only comparison measurements. No comparison or
  geometric deviation has yet been measured. No new tests or Git operations.


## Later handoff — visual/learning foundation (2026-11-05)

See [current viewer](comparison/learning/index.html), [verification](comparison/learning/TEST_RESULTS.md) and the latest ROADMAP entry. The bounded read preserved original dirty=True and pilot dirty=False. Partial native-edge views and conditional opening overlay exist; classifications remain provisional. 65 learning / 70 analyzer / 165 regression checks passed. No CAD save/rebuild/modeling or Git operation. Next: correspondence-review manifest and independent photo evidence, then bounded reads; no automatic original/R2 changes or promotion.
