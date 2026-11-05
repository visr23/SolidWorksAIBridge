param([string]$MSBuildPath, [string]$PilotRecord)
$ErrorActionPreference='Stop'
if (!$MSBuildPath) {
    $found=Get-Command MSBuild.exe -ErrorAction SilentlyContinue
    if($found){$MSBuildPath=$found.Source}
    else{$MSBuildPath=Get-ChildItem "$env:ProgramFiles/Microsoft Visual Studio" -Recurse -Filter MSBuild.exe -ErrorAction SilentlyContinue|Where-Object {$_.FullName -match 'MSBuild\\Current\\Bin\\MSBuild.exe$'}|Select-Object -First 1 -ExpandProperty FullName}
}
if(!$MSBuildPath){throw 'Full Windows MSBuild/.NET Framework 4.8 required.'}
& $MSBuildPath (Join-Path $PSScriptRoot 'tests/LearningTests.csproj') /t:Rebuild /nologo /verbosity:minimal
if($LASTEXITCODE -ne 0){throw "Learning build failed: $LASTEXITCODE"}
$arguments=@((Join-Path $PSScriptRoot 'tests/synthetic.json'))
if($PilotRecord){$arguments+= (Resolve-Path -LiteralPath $PilotRecord).Path}
& (Join-Path $PSScriptRoot 'tests/bin/Debug/LearningTests.exe') @arguments
if($LASTEXITCODE -ne 0){throw "Learning tests failed: $LASTEXITCODE"}
