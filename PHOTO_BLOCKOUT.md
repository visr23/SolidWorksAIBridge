# Photo blockout pilot — continue on another computer

Status: experimental photo-based massing, not a Validated LEGO recipe. The user
visually approved the initial masses and roofs on 2026-11-04. Dimensions remain
assumed miniature millimeters; no native source feature tree was read or used.

## Pull and build at home

Use Windows, SolidWorks 2024 (version 32 registered COM libraries), .NET Framework
4.8 targeting tools and full Visual Studio MSBuild. No new packages are required.
Run from the cloned repository root. Pull only after preserving local work:

```powershell
git status
git pull --ff-only
$vswhere = "${env:ProgramFiles(x86)}\Microsoft Visual Studio\Installer\vswhere.exe"
$msbuild = & $vswhere -latest -products * -requires Microsoft.Component.MSBuild -find 'MSBuild\**\Bin\MSBuild.exe' | Select-Object -First 1
if (-not $msbuild) { throw 'Install compatible Visual Studio MSBuild and .NET Framework 4.8 targeting tools.' }
& $msbuild .\SolidWorksAIBridge\SolidWorksAIBridge.csproj /t:Rebuild /p:Configuration=Debug /nologo /verbosity:minimal
if ($LASTEXITCODE -ne 0) { throw 'Build failed; do not run an old binary.' }
.\SolidWorksAIBridge\bin\Debug\SolidWorksAIBridge.exe --help
```

Start SolidWorks yourself, configure the default Part template, and use a template
with `Front Plane`. Run PowerShell as the same Windows user and privilege level
as SolidWorks. A sandbox-account shell may see the process yet fail COM attachment.
Do not launch a second instance or alter Windows security settings to work around it.

## Recreate the concept in a NEW document

```powershell
.\SolidWorksAIBridge\bin\Debug\SolidWorksAIBridge.exe --photo-blockout
```

This command creates a new unsaved Part every time. It never opens a source file,
edits the previously active document, resumes a cloud file, saves, closes or launches
SolidWorks. Failed parts remain open for inspection. It creates four separate
unmerged bodies, verifies four successful rebuilds, body count and summed volume
(2,624,250 mm³), then displays isometric view. The roofs are solid triangular
envelopes; sketches are coordinate-created and unconstrained, not a fully
parametric building system. No printability, overlap or round-trip claim is made.

`--help` and invalid arguments do not connect to CAD. No arguments retain the
legacy wall/door demo; **use the explicit --photo-blockout flag** for this pilot.

## Continue from the model saved in your cloud

Save the current pilot Part under a distinct name of your choosing and wait for
your cloud service to finish syncing it. Git carries the code and this handoff,
not the CAD document or the original photographs. At home open the synced Part
manually. Do not run --photo-blockout to resume it: that creates another Part.
Before further automation, tell Codex which saved document is the pilot and have
it verify the intended target and four named features/bodies. Automated resume,
entrance cuts and openings are not implemented by this command yet.

Feature names: MainMass_ASSUMED, FrontLeftMass_ASSUMED,
MainGableEnvelope_ASSUMED, SecondaryGableEnvelope_ASSUMED. These distinguish the
pilot from the uploaded original Part1.SLDPRT; automatic Part1/Part2 titles alone
do not establish identity.

## Reproducible geometry assumptions

XY is front elevation, +Y up, Z is depth. All extrusions are centered at Z=0.

| Body | XY profile, mm | Depth, mm |
|---|---|---:|
| Main mass | rectangle (-20,0) to (90,80) | 140 |
| Front-left mass | rectangle (-90,0) to (-20,65) | 170 |
| Main gable envelope | triangle (-28,80), (98,80), (35,125) | 150 |
| Secondary gable envelope | triangle (-98,65), (-12,65), (-55,90) | 180 |

These are concept assumptions based on photo proportions, not real-world survey
measurements or dimensions extracted from the supplied mesh. Photo-derived plan
is manually encoded; this command does not run computer vision or accept Building JSON.

## Next experiment

Establish a known reference dimension and check front/side proportions. Add the
entrance recess and primary facade openings, with explicit body scope, in the
selected pilot only. Continue reconstruction without consulting the source tree.
Develop the read-only Inspector separately for later diagnosis; it remains
unimplemented. Record failures and compare to the original tree only when needed;
candidate recipes must be tested before promotion.
