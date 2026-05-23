#Requires -Version 5.1
<#
.SYNOPSIS
    EJLive Phase-2 Baseline Verification Script
.DESCRIPTION
    Runs restore/build/test/verification on the exact source version under development.
    Any failure stops the pipeline and returns non-zero exit code.
    Logs are saved under artifacts/baseline/<timestamp>/
#>
[CmdletBinding()]
param(
    [string]$SolutionPath = ".\EJLive.Unified.sln",
    [string]$SolutionXPath = ".\EJLive.Unified.slnx",
    [string]$TestProject = ".\src\EJLive.Tests\EJLive.Tests.csproj",
    [string]$VerificationProject = ".\src\EJLive.Verification\EJLive.Verification.csproj",
    [string]$ArtifactsRoot = ".\artifacts\baseline"
)

$ErrorActionPreference = "Stop"
$stopwatch = [System.Diagnostics.Stopwatch]::StartNew()

function Write-StepHeader {
    param([string]$Message)
    Write-Host "`n========================================" -ForegroundColor Cyan
    Write-Host $Message -ForegroundColor Cyan
    Write-Host "========================================`n" -ForegroundColor Cyan
}

function Invoke-Step {
    param(
        [string]$Name,
        [scriptblock]$Action,
        [string]$LogFile
    )
    Write-StepHeader $Name
    $sw = [System.Diagnostics.Stopwatch]::StartNew()
    try {
        & $Action *>&1 | Tee-Object -FilePath $LogFile
        $exitCode = $LASTEXITCODE
        if ($exitCode -ne 0) {
            throw "$Name failed with exit code $exitCode. See $LogFile"
        }
        $sw.Stop()
        Add-Content -Path $LogFile -Value "`n--- Completed in $($sw.Elapsed) ---`n"
        Write-Host "$Name completed successfully in $($sw.Elapsed)." -ForegroundColor Green
        return $true
    }
    catch {
        $sw.Stop()
        $errMsg = $_.Exception.Message
        Add-Content -Path $LogFile -Value "`n--- FAILED after $($sw.Elapsed) ---`nERROR: $errMsg`n"
        Write-Host "$Name FAILED: $errMsg" -ForegroundColor Red
        throw
    }
}

# Ensure artifacts directory exists
$timestamp = Get-Date -Format "yyyyMMdd-HHmmss"
$artifactsDir = Join-Path $ArtifactsRoot $timestamp
New-Item -ItemType Directory -Force -Path $artifactsDir | Out-Null

Write-Host "EJLive Phase-2 Baseline Verification" -ForegroundColor Yellow
Write-Host "Timestamp: $timestamp" -ForegroundColor Yellow
Write-Host "Artifacts: $(Resolve-Path $artifactsDir)" -ForegroundColor Yellow

# Step 1: dotnet --info
Invoke-Step -Name "Step 1: dotnet --info" -LogFile (Join-Path $artifactsDir "dotnet-info.log") -Action {
    dotnet --info
}

# Step 2: dotnet restore
Invoke-Step -Name "Step 2: dotnet restore" -LogFile (Join-Path $artifactsDir "restore.log") -Action {
    dotnet restore $SolutionPath --verbosity normal
}

# Step 3: dotnet build (solution)
Invoke-Step -Name "Step 3: dotnet build (solution)" -LogFile (Join-Path $artifactsDir "build.log") -Action {
    dotnet build $SolutionPath --no-restore -m:1 /p:BuildInParallel=false -v:m
}

# Step 4: dotnet test
Invoke-Step -Name "Step 4: dotnet test" -LogFile (Join-Path $artifactsDir "test.log") -Action {
    dotnet test $TestProject --no-restore -m:1 /p:BuildInParallel=false --logger "console;verbosity=minimal"
}

# Step 5: dotnet run verification
Invoke-Step -Name "Step 5: dotnet run verification" -LogFile (Join-Path $artifactsDir "verification.log") -Action {
    dotnet run --project $VerificationProject --no-restore -m:1 /p:BuildInParallel=false
}

# Step 6: dotnet build slnx
Invoke-Step -Name "Step 6: dotnet build (slnx)" -LogFile (Join-Path $artifactsDir "build-slnx.log") -Action {
    dotnet build $SolutionXPath --no-restore -m:1 /p:BuildInParallel=false -v:m
}

$stopwatch.Stop()

# Summary
$summary = @"
========================================
EJLive Phase-2 Baseline Verification Summary
========================================
Timestamp: $timestamp
Duration: $($stopwatch.Elapsed)
All steps completed successfully.
Artifacts saved to: $(Resolve-Path $artifactsDir)

Files generated:
- dotnet-info.log
- restore.log
- build.log
- test.log
- verification.log
- build-slnx.log
"@

$summaryPath = Join-Path $artifactsDir "summary.txt"
$summary | Out-File -FilePath $summaryPath -Encoding UTF8
Write-Host $summary -ForegroundColor Green

exit 0
