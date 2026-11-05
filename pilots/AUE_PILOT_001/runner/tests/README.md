# Pilot body identity regressions (non-COM)

These standalone .NET Framework 4.8 console tests compile the same BodyIdentity.cs
used by the pilot, without a SolidWorks reference or connection. No packages needed.
Use full Visual Studio MSBuild, then the test executable:

```powershell
MSBuild.exe pilots/AUE_PILOT_001/runner/tests/BodyIdentityTests.csproj /t:Rebuild /nologo /verbosity:minimal
if ($LASTEXITCODE -ne 0) { throw 'Build failed; do not run stale tests.' }
.\pilots\AUE_PILOT_001\runner\tests\bin\Debug\AuePilot001.BodyIdentityTests.exe
if ($LASTEXITCODE -ne 0) { throw 'Body identity regressions failed.' }
```

Tests simulate post-Cut-Extrude naming/reference changes, ambiguous/missing body
ownership, expected geometry/dimensions and rebuild-result checks. They do not
perform Cut-Extrude in SolidWorks. CAD integration requires separate approval.
See [actual correction results](../../results/BODY_TRACKING_FIX_2026-11-05.md).
