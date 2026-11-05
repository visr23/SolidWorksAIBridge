# GitHub backup checkpoint — 2026-11-05

User explicitly authorized repository inspection, checkpoint, commit and normal
push for verified project progress before restart. Earlier no-Git instructions
apply to prior CAD/development tasks, not this backup request.

## Repository verification before commit

- Repository: SolidWorksAIBridge; branch `main`.
- Remote: `origin`, https://github.com/visr23/SolidWorksAIBridge.
- Starting HEAD: `2ad61d7` (AUE V1 documentation).
- `git fetch --no-tags origin` succeeded; main vs origin/main: 0 ahead / 0 behind.
- No older unpushed local commits; no existing staged changes on entry.
- Five modified documentation files plus new pilot and analyzer/learning files
  reviewed. Production builders/approved family metadata and prior tracked test
  source/evidence unchanged. No discard/reset/clean/stash/history rewrite.

## Backup content

Include the isolated R2 runner/body-identity policy/tests, pilot design history,
original failed CAD attempt and successful 180 mm experimental baseline evidence,
read-only comparison tools/evidence, two annotated front views/overlay/viewer,
learning-record library/fixtures/tests, Draft recipes/proposals, documentation and
existing provided pilot photo/generated-view inputs. Designs, experimental source,
failed checks and unknowns retain their recorded statuses; backup is not promotion.

Include the manually saved generated native baseline:
`pilots/AUE_PILOT_001/solidworks reasults/AUE_PILOT_001_FRONT_V1.SLDPRT`.
151934 bytes; SHA-256 matches existing saved-baseline manifest:
`CC352E435F9D971048795E2AD248C6F2FEFF1B55927C9CA8BFFDF08D8F86A8AE`.
This necessary pilot reference artifact is not an executable/debug build product.
The actual studio source on Google Drive is outside this repository/backup.

Exclude (preserve locally, do not delete):
- bin/obj/IDE caches/lock/temp files under existing rules.
- copied R2 run executables, PDBs and runtime config; retain text attempt marker.
- duplicate `.json.checkpoint` scratch snapshot.
- empty `comparison/SOURCE_HASHES_BEFORE.csv` from an unsuccessful hash attempt.
- unverified `cad_tests/Part1.SLDPRT` (54628 bytes), which is NOT the selected studio
  original and NOT the successful saved R2 baseline.

Durable text test/build/inspection transcripts referenced by reports are retained
as evidence; compiler outputs/binaries are excluded. `.gitattributes` preserves
JSON/CSV/TXT evidence bytes and native/image inputs across newline settings, so
recorded SHA-256 inputs remain reproducible. Archived invalid-ID test names retain
intentional trailing spaces; their log whitespace rule recognizes these fixtures.
Only nonsemantic source/doc/SVG trailing blank lines were cleaned. No CAD file
changed during backup.

The request's exclusion list ended at "U". Clarification was requested while the
review proceeded; absent a reply, use the explicit inclusion list and conservative
exclusion of unrelated/unverified scratch/build files. No speculative extra work.

## Verification evidence

Latest development checks actually executed: 65/0 learning (including actual pilot
record), 70/0 existing analyzer, 165/0 legacy regressions. Historical body policy
49/0. These are preserved logs, not newly rerun checks in this backup-only task.
[Commands and limitations](pilots/AUE_PILOT_001/comparison/learning/TEST_RESULTS.md).
13/13 production source/project hashes still match the development snapshot.
Final staged review: 112 files, approximately 5.9 MB; no unstaged changes or
non-ignored untracked files. Staged whitespace checks passed. Four recorded input
manifest hashes matched; sampled staged raw evidence/log/R2 bytes matched local
files. Build/scratch files remain ignored locally.
Strong credential-pattern filename scan found no hits in candidate source/evidence;
this is a limited scan, not a guarantee that every possible secret format is covered.
No SolidWorks connection, save, rebuild, tests, configuration change or geometry
operation was run for backup.

## Restart handoff

The original studio CAD was dirty=True at the last read. Git backup does NOT save
unsaved SolidWorks memory or backup that Google Drive source. User must resolve
any unsaved CAD state manually before restarting; no save is authorized here.
R2 remains 180 mm, 5 bodies, 11 modeling features and experimental only.

Exact next development action: correspondence-review manifest and deterministic
ambiguity fixtures; confirm facade datums/host-panel vs pane/frame and E1/E2/S1/P2
roles, register independent front-photo/camera evidence, then expand bounded reads.
No automatic geometry correction, new CAD experiment or LEGO promotion.

This document is prepared immediately before the containing backup commit. Commit
and push outcome/commit ID are reported after execution, rather than predicted
here. Push normally to the verified origin/main; stop on divergence/rejection,
never force. Verify local HEAD equals remote branch after push.
