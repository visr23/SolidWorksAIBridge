# BalconyAssembly V1 implementation checkpoint — 2026-11-03

Status **Testing**. Both-wall code is implemented/compiled and non-CAD tests pass. **BALCONY_BOTH_WALLS CAD: NOT RUN — awaiting explicit approval.** No SolidWorks attachment, launch, geometry operation or Git operation occurred in this session.

## Checks actually run

```powershell
powershell.exe -NoProfile -ExecutionPolicy Bypass -File .\tests\Run-RegressionTests.ps1
```

Final run: **141 passed, 0 failed**, exit 0. Existing 105 tests retained; 36 new assembly tests. Analytic placement/contact/printability checks are not measured solid connectivity or CAD validation.

Visual Studio 18 Community MSBuild Debug Rebuild of tests/ManualValidation.csproj and SolidWorksAIBridge/SolidWorksAIBridge.csproj: both exit 0, with existing MSB3305 COM marshaling warnings. The non-CAD project recompiles current source too. Initial failures in two new underflow guard tests were resolved in the new assembly guard by using the existing UnitConverter path; final results above are after correction.

Local ignored logs: tests/assembly-noncad.log, tests/assembly-manual-build.log, tests/assembly-app-build.log. Geometry executor/source identities:

Final transcript copies are retained under evidence/ASSEMBLY_IMPLEMENTATION_2026-11-03/ as .txt files with a SHA256SUMS.txt manifest; original local logs retained. They contain no actual CAD execution evidence.

| Source | SHA256 |
| --- | --- |
| BalconyAssembly.cs | 83A8B96EB31ACE4AC788024603896ECFA89B8E64A56EF5710CD5E45C150CA27D |
| SolidWorksModeler.BalconyAssembly.cs | A06B564A9B0CFDA45CCD954771A3F181AE83ACD005253797A6B58E39E736FAD2 |
| ProjectingBalconySlab.cs (unchanged) | 8AD7115669EC7EF78336F3BE4E53948CFD00D8F17B4073CBBE54D12A171256AC |
| SolidWorksModeler.BalconySlab.cs (unchanged) | 661C975C1D6A5BC63D5CCCFE6A6447F0DFB5FB54B031CF1B1FCD369EA6975E59 |
| SolidWorksModeler.cs (unchanged) | 6E6EDF6346F4D598B864CE3530A44258BED0D0B6A188CEF4999A0F12BB818511 |

## Next approved action required

Review [single-case procedure and expectations](../BALCONY_ASSEMBLY_V1.md), then explicitly approve:

```powershell
.\tests\bin\Manual\SolidWorksAIBridge.ManualValidation.exe --confirm-cad BALCONY_BOTH_WALLS
```

One new unsaved document; baseline plus depth edit/restore; two solids and eight generated features expected. Stop on failure, preserve the failed part, and do not replace scoped merging with an all-body merge or Combine fallback. Existing user documents must remain unchanged. Append actual CAD outcomes rather than reclassifying this implementation checkpoint as a CAD PASS.
