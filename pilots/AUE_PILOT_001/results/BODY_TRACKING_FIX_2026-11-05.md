# Pilot-only body tracking correction — 2026-11-05

Status: **implemented, built and non-COM tested; corrected CAD path NOT RUN**.
No SolidWorks connection, UI interaction, rebuild, inspection or retry occurred.
The existing unsaved partial Part and studio CAD files were not accessed/modified.

## Changes

- BodyIdentity.cs supplies a pure generic identity/ownership policy. Logical roles
  bind to current Feature references; the COM adapter re-queries **all** face
  owners, checks native COM identity against current PartDoc solid inventory and
  requires exactly one live body. Duplicate face owners are deduplicated by native
  identity. No current Body2 display name is used for lookup or rewritten.
- Track MainWall from its creation feature, then update that role to WindowCut's
  current face ownership after the unchanged legacy cut. Each subsequent pilot
  operation refreshes its role from its result feature after successful rebuild.
  Do not retain stale pre-cut Body2 pointers or rely on the first returned face.
- Validate exact expected body/role counts and one-to-one role/body mapping.
  Fail closed on missing/stale/foreign/ambiguous owners, splits, unexpected extra
  bodies or merged roles; no name or approximate-geometry identity fallback.
- Non-target checks use logical roles and live feature ownership, comparing
  volume, exact vertex bounds and vertex/edge/face counts. No dictionary is keyed
  by mutable solid labels. Geometry checks supplement identity; equal geometry
  never implies two solids have the same identity.
- Right-window future CAD checks explicitly verify driving width/height, center
  offsets, cut depth, four through-cut side faces, measured cut-face bounds and
  successful rebuild; no dependence on incomplete display-dimension enumeration.
  Pilot limits are rectangular, planar front-facing R2 geometry, not a generic
  native-CAD body inspector or persistent saved-file identity scheme.
- R2 dimensions/roof assumptions, production builders, entry point, attempt guard
  and stop-on-first-failure behavior are unchanged. Existing body names are left
  to SolidWorks/user choice. Names are used only in diagnostic output.

## Actual non-CAD verification

Full Visual Studio MSBuild / .NET Framework 4.8, repository-relative commands:

| Command | Actual result |
| --- | --- |
| `MSBuild.exe pilots/AUE_PILOT_001/runner/tests/BodyIdentityTests.csproj /t:Rebuild /nologo /verbosity:minimal` | Exit 0; standalone test project has no COM/project reference |
| `pilots/AUE_PILOT_001/runner/tests/bin/Debug/AuePilot001.BodyIdentityTests.exe` | Exit 0, **49 passed / 0 failed** |
| `MSBuild.exe pilots/AUE_PILOT_001/runner/AuePilot001.R2.csproj /t:Rebuild /p:Configuration=Debug /nologo /verbosity:minimal` | Exit 0; existing MSB3305 COM warnings |
| `pilots/AUE_PILOT_001/runner/bin/Debug/AuePilot001.R2.exe --preflight` | Exit 0; positive plan plus 3 rejected invalid fixtures; no COM |
| `powershell.exe -NoProfile -ExecutionPolicy Bypass -File tests/Run-RegressionTests.ps1` | Exit 0; **165 passed / 0 failed**, CAD cases NOT RUN |

Logs: [focused build](BODY_TRACKING_TEST_BUILD.txt), [49 tests](BODY_TRACKING_TESTS.txt),
[pilot build](BODY_TRACKING_PILOT_BUILD.txt), [preflight](BODY_TRACKING_PREFLIGHT.txt),
[legacy regressions](BODY_TRACKING_LEGACY_TESTS.txt).
The focused suite initially passed 41 cases; 8 additional aperture cases were
added and the final 49-case suite plus pilot build rerun successfully. No failed
build/test occurred during this correction session.

Tests simulate Cut-Extrude post-cut renaming and replaced native references through
fake bodies/feature ownership; they exercise the exact policy used by the COM
adapter, not real SolidWorks extrusion. Cases cover automatic/user renaming,
multiple wrappers, repeated faces, duplicate names, replacement, stale references,
foreign/missing owners, split/merged roles, counts, unchanged targets, nonfinite
values, cut dimensions/position, shallow-pocket versus through-cut bounds and failed
rebuild results. They establish policy behavior, not new CAD execution evidence.

Source protection: all 13 production source/project SHA-256 values matched the
preexisting snapshot. Existing R2_CAD_ATTEMPT.txt still present after builds; no
clear/reset/retry. No Git operation. No family promotion; OpeningSurround Draft.

## Historical geometry and visual evidence

User now confirms partial facade/right-window geometry appears correct and tree
contains MainWall + WindowCut features and one body labeled WindowCut. This is
human visual evidence for that partial result only, not R2 full-model approval.
Prior [CAD record](R2_CAD_RESULT.md) remains unchanged: one body, two modeling
features, two sketches, 224.5 x 99.8 x 12 mm envelope, 23.4 x 30.4 opening,
12.2 mm cut depth, remaining volume 260324.88 mm3; wall/window builder rebuilds
returned successfully. 180 mm was not reached. No fresh CAD measurements/rebuild
are claimed. New adapter checks have not run against that Part.

## Rerun readiness and limitations

**Ready to request a separately approved full R2 experimental attempt**, with
successful build/policy tests and unchanged numeric plan. Not validated in CAD;
new native feature-face resolution and subsequent unsupported operations may still
fail. Existing guard intentionally blocks the old invocation; do not remove it
now. New explicit authorization must specify one fresh unsaved Part and permit a
new attempt record while preserving old evidence/Part1. Do not resume or repair
Part1 without separate instruction. Stop at the first construction/validation
failure. Full five-body geometry, 180 mm height, parametric edits and visual
assessment remain future evidence; no architecture or LEGO promotion implied.
