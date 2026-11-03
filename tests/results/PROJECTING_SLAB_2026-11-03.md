# Projecting Balcony Slab V1 result — 2026-11-03

Status: code implemented, non-COM checks passed, CAD **NOT RUN**, definition lifecycle **Testing**, not Validated.

## Automated execution

```powershell
powershell.exe -NoProfile -ExecutionPolicy Bypass -File .\tests\Run-RegressionTests.ps1
```

Result: **95 passed, 0 failed**, exit code 0. Includes the original 53 checks and 42 new slab checks. New coverage: deterministic bounds/elevation conversion, signed placement, exact boundaries, host snapshot dimensions and reference matching, definition/instance/preflight metadata, invalid IDs, positive/finite/representable dimensions, boundary violations, coordinate precision loss, and CAD API missing-host/null preconditions. No successful COM path ran.

Additional builds (no executables launched):

```powershell
& 'C:\Program Files\Microsoft Visual Studio\18\Community\MSBuild\Current\Bin\MSBuild.exe' .\SolidWorksAIBridge\SolidWorksAIBridge.csproj /t:Rebuild /p:Configuration=Debug /nologo /verbosity:minimal
& 'C:\Program Files\Microsoft Visual Studio\18\Community\MSBuild\Current\Bin\MSBuild.exe' .\tests\ManualValidation.csproj /t:Rebuild /p:Configuration=Debug /nologo /verbosity:minimal
```

Both builds passed, exit code 0. Environment: Windows, Visual Studio 18 Community MSBuild, .NET Framework 4.8 targeting, registered SldWorks/SwConst v32 libraries. Builds emitted MSB3305 legacy COM marshaling warnings. Runtime SolidWorks version/service pack/template were not queried. Local ignored logs: tests/slab-run.log, tests/slab-app-build.log, tests/slab-manual-build.log.

| Source under SolidWorksAIBridge/ | SHA256 |
| --- | --- |
| ProjectingBalconySlab.cs | 329E778F23753F154F4EF08E0DF01DFAC3BEDD60FE4DCE9BC4CF10B268D9F0AB |
| SolidWorksModeler.BalconySlab.cs | 661C975C1D6A5BC63D5CCCFE6A6447F0DFB5FB54B031CF1B1FCD369EA6975E59 |
| SolidWorksModeler.cs | 6E6EDF6346F4D598B864CE3530A44258BED0D0B6A188CEF4999A0F12BB818511 |

## Actual CAD results

- BALCONY: NOT RUN.
- BALCONY_MIXED: NOT RUN.
- Existing manual baseline cases: NOT RUN in this session.
- No measurements, CAD artifacts, screenshots, parameter-edit rebuild outcomes or human visual approvals collected.

Outward extrusion, dimension naming/edit behavior, separate-body creation and legacy CAD compatibility are implemented intentions pending actual SolidWorks validation. Do not promote the definition on the basis of this record.

Next action: in an explicitly authorized CAD session, run the legacy fixtures and BALCONY/BALCONY_MIXED, measure/report results against ../PROJECTING_BALCONY_SLAB_V1.md, and resolve any CAD discrepancies before expanding the component.
