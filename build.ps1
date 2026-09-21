param(
    [string]$AmongUsPath = ""
)

$ErrorActionPreference = "Stop"

if (-not (Get-Command dotnet -ErrorAction SilentlyContinue)) {
    throw "[TownOfNDev] .NET SDK not found. Install the .NET 8 SDK, then reopen PowerShell and run this script again."
}

Write-Host "[TownOfNDev] dotnet SDK: $(dotnet --version)"

function Assert-LastExitCode {
    param([string]$Step)
    if ($LASTEXITCODE -ne 0) {
        throw "[TownOfNDev] $Step failed with exit code $LASTEXITCODE."
    }
}

Write-Host "[TownOfNDev] Restoring packages..."
dotnet restore .\TownOfNDev.sln
Assert-LastExitCode "Package restore"

Write-Host "[TownOfNDev] Building Release..."
if ([string]::IsNullOrWhiteSpace($AmongUsPath)) {
    dotnet build .\TownOfNDev.sln -c Release --no-restore
} else {
    dotnet build .\TownOfNDev.sln -c Release --no-restore -p:AmongUs="$AmongUsPath"
}
Assert-LastExitCode "Release build"

$dll = ".\TownOfNDev\bin\Release\net6.0\TownOfNDev.dll"
if (-not (Test-Path $dll)) {
    throw "[TownOfNDev] Build completed but the expected DLL was not found at $dll"
}

Write-Host "[TownOfNDev] Done. DLL: $dll"
