# Slab-top equal-height candidate implementation — 2026-11-04

Subsequent approved validation: [actual CAD comparison PASS](TOP_FACE_CAD_2026-11-04.md).
This implementation checkpoint retains its original pre-execution status below;
Part2/Part3 now await human visual review. No production changes were made during
that validation.

Implemented additive `CreateTopFaceBalconyAssembly` / `top-face-shared-r1`.
Lifecycle **Testing**. CAD **NOT RUN**; visual approval **PENDING**.
See [comparison contract](../TOP_FACE_BALCONY_COMPARISON.md).

## Actual verification

- All non-CAD regressions: **165 passed, 0 failed**, including 24 focused candidate
  checks and the unchanged prior 141 cases.
- Regression runner, application and manual CAD runner Debug rebuilds: **exit 0**.
- Builds emitted existing **MSB3305** COM marshaling warnings; these were not
  treated as CAD verification. No dependency installation was performed.
- Script entry invocation was blocked by local PowerShell execution policy.
  Its MSBuild and non-CAD executable commands were executed directly instead;
  policy was not changed. The direct commands below all succeeded.
- SHA256 of both slab source files, both baseline assembly source files and the
  existing baseline CAD case matched pre-edit values. They were not modified.
- No SolidWorks launch, connection, control or CAD execution. No Git operations.

Commands (Visual Studio's compatible `MSBuild.exe` resolved locally):

```powershell
& $MSBuildPath tests\SolidWorksAIBridge.RegressionTests.csproj /t:Rebuild /p:Configuration=Debug /nologo /verbosity:minimal
.\tests\bin\Debug\SolidWorksAIBridge.RegressionTests.exe
& $MSBuildPath SolidWorksAIBridge\SolidWorksAIBridge.csproj /t:Rebuild /p:Configuration=Debug /nologo /verbosity:minimal
& $MSBuildPath tests\ManualValidation.csproj /t:Rebuild /p:Configuration=Debug /nologo /verbosity:minimal
```

Preserved [regression output](evidence/TOP_FACE_2026-11-04/regressions.txt),
[regression build](evidence/TOP_FACE_2026-11-04/regression-build.txt),
[application build](evidence/TOP_FACE_2026-11-04/app-build.txt),
[manual runner build](evidence/TOP_FACE_2026-11-04/manual-build.txt), and
[hash manifest](evidence/TOP_FACE_2026-11-04/SHA256SUMS.txt).

## Pending integration gate

Obtain explicit approval for `BALCONY_TOP_FACE_COMPARE`: baseline plus candidate
in two fresh unsaved parts, stop/preserve on failure, no existing document changes.
Measure 5952->6448->5952 mm³ for depth 24->26->24; check width 56->60->56,
two-body/facade isolation, U-profile connectivity, rebuilds and six generated
candidate features versus eight baseline features. All values are expectations
until executed. Record actual evidence and human review before any promotion or
replacement. No parapets, unequal-height cuts, ground-floor work or unrelated scope.
