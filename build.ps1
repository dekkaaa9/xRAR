# xRAR Build Script
# Compiles Release build and generates the Inno Setup installer.
# Usage: .\build.ps1

param(
    [switch]$SkipInstaller
)

$ErrorActionPreference = "Stop"

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "  xRAR Build Script" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

# --- Step 1: Build Release ---
Write-Host "[1/3] Building Release..." -ForegroundColor Yellow
Push-Location "$PSScriptRoot\src\xRAR"
dotnet publish -c Release -r win-x64 -p:PublishSingleFile=false -p:SelfContained=true -o "bin\Release\net7.0-windows\publish" 2>&1 | Select-Object -Last 5
if ($LASTEXITCODE -ne 0) {
    Write-Host "BUILD FAILED!" -ForegroundColor Red
    Pop-Location
    exit 1
}
Pop-Location
Write-Host "  Build OK" -ForegroundColor Green
Write-Host ""

# --- Step 2: Copy publish output to expected path ---
Write-Host "[2/3] Preparing files..." -ForegroundColor Yellow
$publishDir = "$PSScriptRoot\src\xRAR\bin\Release\net7.0-windows\publish"
$targetDir = "$PSScriptRoot\src\xRAR\bin\Release\net7.0-windows"
if (Test-Path $publishDir) {
    Copy-Item "$publishDir\*" $targetDir -Recurse -Force
}
Write-Host "  Files ready" -ForegroundColor Green
Write-Host ""

# --- Step 3: Build Installer (Inno Setup) ---
if (-not $SkipInstaller) {
    Write-Host "[3/3] Building installer..." -ForegroundColor Yellow

    $iscc = $null
    $paths = @(
        "${env:ProgramFiles}\Inno Setup 7\ISCC.exe",
        "${env:ProgramFiles}\Inno Setup 6\ISCC.exe",
        "${env:ProgramFiles(x86)}\Inno Setup 6\ISCC.exe",
        "${env:ProgramFiles(x86)}\Inno Setup 5\ISCC.exe"
    )
    foreach ($p in $paths) {
        if (Test-Path $p) { $iscc = $p; break }
    }

    if ($iscc) {
        $distDir = "$PSScriptRoot\dist"
        if (-not (Test-Path $distDir)) { New-Item -ItemType Directory -Path $distDir | Out-Null }

        & $iscc "$PSScriptRoot\installer.iss" 2>&1
        if ($LASTEXITCODE -ne 0) {
            Write-Host "INSTALLER BUILD FAILED!" -ForegroundColor Red
            exit 1
        }
        Write-Host "  Installer: dist\xRAR-Setup-1.0.0.exe" -ForegroundColor Green
    } else {
        Write-Host "  Inno Setup not found - skipping installer." -ForegroundColor DarkYellow
        Write-Host "  Download from: https://jrsoftware.org/isdl.php" -ForegroundColor DarkYellow
    }
} else {
    Write-Host "[3/3] Skipping installer (-SkipInstaller)" -ForegroundColor DarkYellow
}

Write-Host ""
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "  DONE!" -ForegroundColor Green
Write-Host "========================================" -ForegroundColor Cyan
