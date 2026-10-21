# Builds the compatibility variant against the exact DLLs from a fresh official
# Town of Us: Mira 1.7.9 Beta 1 installation. Does not install or upload files.
[CmdletBinding()]
param(
    [Parameter(Mandatory = $true)]
    [string]$InstallDir
)

$ErrorActionPreference = 'Stop'
$repoRoot = (Resolve-Path (Join-Path $PSScriptRoot '..')).Path
$gameDir = (Resolve-Path -LiteralPath $InstallDir -ErrorAction Stop).Path
$plugins = Join-Path $gameDir 'BepInEx\plugins'
if (-not (Test-Path -LiteralPath $plugins -PathType Container)) {
    throw "No BepInEx\plugins folder at '$plugins'. Use the clean official 1.7.9 Beta 1 game installation."
}

function Find-RequiredDll([string]$name) {
    $matches = @(Get-ChildItem -LiteralPath $plugins -Filter $name -File -Recurse -ErrorAction Stop)
    if ($matches.Count -ne 1) {
        throw "Expected exactly one '$name' under '$plugins'; found $($matches.Count). Remove conflicting/old installations and verify your ToU-Mira 1.7.9 Beta 1 installation."
    }
    return $matches[0].FullName
}

$tou = Find-RequiredDll 'TownOfUsMira.dll'
$mira = Find-RequiredDll 'MiraAPI.dll'
Write-Host "Using ToU DLL: $tou"
Write-Host "Using MiraAPI DLL: $mira"
Write-Warning 'Verify these DLLs come from the OFFICIAL ToU-Mira 1.7.9 Beta 1 installer; filenames alone cannot prove their versions.'

$oldTou = [Environment]::GetEnvironmentVariable('TownOfUs179Dll', 'Process')
$oldMira = [Environment]::GetEnvironmentVariable('MiraApi179Dll', 'Process')
$oldAmongUs = [Environment]::GetEnvironmentVariable('AmongUs', 'Process')
Push-Location $repoRoot
try {
    $env:TownOfUs179Dll = $tou
    $env:MiraApi179Dll = $mira
    # Prevent a pre-existing AmongUs environment variable from auto-copying an
    # untested compatibility DLL into the live game through CopyToAmongUs.
    $env:AmongUs = ''

    dotnet restore '.\TownOfNDev.sln' --force-evaluate
    if ($LASTEXITCODE -ne 0) { throw "Restore failed (exit $LASTEXITCODE). Do not publish this variant." }
    dotnet build '.\TownOfNDev.sln' -c Release --no-restore
    if ($LASTEXITCODE -ne 0) { throw "Build failed (exit $LASTEXITCODE). Do not publish this variant." }
    $output = Join-Path $repoRoot 'TownOfNDev\bin\Release\net6.0\TownOfNDev-0.1.0-beta.2.0.0-tou179b1.dll'
    if (-not (Test-Path -LiteralPath $output -PathType Leaf)) {
        throw "Build returned success but expected DLL was not found: $output"
    }
    Write-Host "Compatibility DLL built: $output"
    Write-Warning 'Build success does not establish runtime or multiplayer compatibility. Test against a fresh 1.7.9 Beta 1 installation before publishing.'
}
finally {
    [Environment]::SetEnvironmentVariable('TownOfUs179Dll', $oldTou, 'Process')
    [Environment]::SetEnvironmentVariable('MiraApi179Dll', $oldMira, 'Process')
    [Environment]::SetEnvironmentVariable('AmongUs', $oldAmongUs, 'Process')
    Pop-Location
}
