param([string]$MSBuildPath)
$ErrorActionPreference = 'Stop'
if (-not $MSBuildPath) {
    $command = Get-Command MSBuild.exe -ErrorAction SilentlyContinue
    if ($command) { $MSBuildPath = $command.Source }
    else {
        $candidates = @(Get-ChildItem -Path "$env:ProgramFiles/Microsoft Visual Studio" -Filter MSBuild.exe -Recurse -ErrorAction SilentlyContinue |
            Where-Object { $_.FullName -match 'MSBuild\\Current\\Bin\\MSBuild.exe$' } |
            Sort-Object FullName -Descending)
        if ($candidates.Count) { $MSBuildPath = $candidates[0].FullName }
    }
}
if (-not $MSBuildPath -or -not (Test-Path -LiteralPath $MSBuildPath)) {
    throw 'Compatible Visual Studio MSBuild not found. Supply -MSBuildPath. Do not use dotnet build for COM references.'
}
Write-Output "MSBuild: $MSBuildPath"
& $MSBuildPath (Join-Path $PSScriptRoot 'SolidWorksAIBridge.RegressionTests.csproj') /t:Rebuild /p:Configuration=Debug /nologo /verbosity:minimal
if ($LASTEXITCODE -ne 0) { throw "Test build failed ($LASTEXITCODE); tests were not run." }
# This executable contains only the non-COM test entry point, never Program.Main.
& (Join-Path $PSScriptRoot 'bin/Debug/SolidWorksAIBridge.RegressionTests.exe')
if ($LASTEXITCODE -ne 0) { throw "Regression tests failed ($LASTEXITCODE)." }
