# Photo blockout integration — actual evidence, 2026-11-04

The initial standalone prototype created four bodies and matched its analytic
volume on SolidWorks revision 32.5.0. The user subsequently approved the massing
and roofs visually and requested integration as a separate command and a Git push
for continuation at home. This record covers the integrated command, not a native
Inspector or automatic photo-analysis system.

## Actual checks

From the repository root using compatible full Visual Studio MSBuild:

| Command | Result |
|---|---|
| `MSBuild.exe SolidWorksAIBridge\SolidWorksAIBridge.csproj /t:Rebuild /p:Configuration=Debug /nologo /verbosity:minimal` | Exit 0; existing MSB3305 COM marshaling warnings |
| `SolidWorksAIBridge\bin\Debug\SolidWorksAIBridge.exe --help` | Exit 0; usage printed before COM connection |
| `SolidWorksAIBridge\bin\Debug\SolidWorksAIBridge.exe --invalid` | Exit 1; rejected before COM connection |
| `powershell.exe -NoProfile -ExecutionPolicy Bypass -File tests\Run-RegressionTests.ps1` | Build exit 0; 165 passed, 0 failed; existing MSB3305 warnings; regression CAD cases not run |
| `SolidWorksAIBridge\bin\Debug\SolidWorksAIBridge.exe --photo-blockout` | Exit 0; new unsaved Part2, four successful rebuilds, four solid bodies; measured volume sum 2,624,250 mm³ matched expected |

Integrated CAD output:

```text
NEW_UNSAVED_PART Part2
CREATED MainMass_ASSUMED
CREATED FrontLeftMass_ASSUMED
CREATED MainGableEnvelope_ASSUMED
CREATED SecondaryGableEnvelope_ASSUMED
SUCCESS; NewPart=Part2; Bodies=4; VolumeMm3=2624250; ExpectedMm3=2624250; Saved=false; Dimensions=ASSUMED_CONCEPT_ONLY; Roofs=SOLID_ENVELOPES_NOT_TILE_RECIPES
```

The CAD invocation ran in the interactive user's context with explicit tool
approval. No existing user document or source CAD was edited, saved or closed by
this command. No source feature tree was inspected. No cloud save/sync is claimed;
the user is saving the model separately. Analyzer tests were not rerun because
Analyzer code was not changed. Legacy no-argument CAD demo was not rerun.

The earlier prototype received visual approval; this integrated run has numeric
verification, not a new human visual approval. Model is an experimental concept,
not a Validated general LEGO recipe. Parameter propagation, constrained sketches,
printability, source-model comparison, geometry overlap and automated saved-model
resume remain untested/unimplemented. See [portable continuation](../../PHOTO_BLOCKOUT.md).
