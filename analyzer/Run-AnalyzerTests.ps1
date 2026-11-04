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
    throw 'Compatible Windows MSBuild not found. Supply -MSBuildPath.'
}
Write-Output "MSBuild: $MSBuildPath"
& $MSBuildPath (Join-Path $PSScriptRoot 'tests/AnalyzerTests.csproj') /t:Rebuild /p:Configuration=Debug /nologo /verbosity:minimal
if ($LASTEXITCODE -ne 0) { throw "Analyzer build failed ($LASTEXITCODE); tests not run." }
& (Join-Path $PSScriptRoot 'tests/bin/Debug/AnalyzerTests.exe') $PSScriptRoot
if ($LASTEXITCODE -ne 0) { throw "Analyzer tests failed ($LASTEXITCODE)." }
