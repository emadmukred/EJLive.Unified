#Requires -Version 5.1
<#
.SYNOPSIS
    Generates the Track 001 source-truth baseline document.
.DESCRIPTION
    Captures repository identity, solution/project files, SDK version, baseline
    artifacts, and required verification commands without adding product
    features or changing runtime behavior.
#>
[CmdletBinding()]
param(
    [string]$BaselineArtifactsDir = "",
    [string]$VerificationStatus = ""
)

$ErrorActionPreference = "Stop"
$root = (Resolve-Path ".").Path

function Get-GitValue {
    param([string[]]$Arguments)

    try {
        $value = & git @Arguments 2>$null
        if ($LASTEXITCODE -eq 0) {
            return ($value | Select-Object -First 1)
        }
    }
    catch {
    }

    return "unknown"
}

function Convert-ToRepoRelativePath {
    param([string]$Path)

    return $Path.Substring($root.Length + 1).Replace('\', '/')
}

function Test-ExcludedFromSourceHash {
    param([string]$RelativePath)

    $normalized = $RelativePath.Replace('\', '/')
    return $normalized.StartsWith(".git/", [System.StringComparison]::OrdinalIgnoreCase) -or
           $normalized.StartsWith("artifacts/", [System.StringComparison]::OrdinalIgnoreCase) -or
           $normalized.Equals("docs/phase2-source-of-truth.md", [System.StringComparison]::OrdinalIgnoreCase) -or
           $normalized.IndexOf("/bin/", [System.StringComparison]::OrdinalIgnoreCase) -ge 0 -or
           $normalized.IndexOf("/obj/", [System.StringComparison]::OrdinalIgnoreCase) -ge 0 -or
           $normalized.IndexOf("/.vs/", [System.StringComparison]::OrdinalIgnoreCase) -ge 0
}

function Get-FileSha256 {
    param([string]$Path)

    $sha = [System.Security.Cryptography.SHA256]::Create()
    $stream = $null
    try {
        $stream = [System.IO.FileStream]::new($Path, [System.IO.FileMode]::Open, [System.IO.FileAccess]::Read, [System.IO.FileShare]::ReadWrite)
        return [System.BitConverter]::ToString($sha.ComputeHash($stream)).Replace('-', '')
    }
    finally {
        if ($stream) { $stream.Dispose() }
        $sha.Dispose()
    }
}

if ([string]::IsNullOrWhiteSpace($BaselineArtifactsDir) -and (Test-Path ".\artifacts\baseline")) {
    $latestBaseline = Get-ChildItem -Path ".\artifacts\baseline" -Directory |
        Sort-Object Name -Descending |
        Select-Object -First 1

    if ($latestBaseline) {
        $BaselineArtifactsDir = $latestBaseline.FullName
    }
}

$baselineRelativePath = if ([string]::IsNullOrWhiteSpace($BaselineArtifactsDir)) {
    "not generated yet"
} else {
    Convert-ToRepoRelativePath (Resolve-Path $BaselineArtifactsDir).Path
}

$baselineTimestamp = if ([string]::IsNullOrWhiteSpace($BaselineArtifactsDir)) {
    "not generated yet"
} else {
    Split-Path -Leaf (Resolve-Path $BaselineArtifactsDir).Path
}

if ([string]::IsNullOrWhiteSpace($VerificationStatus)) {
    $summaryPath = if ([string]::IsNullOrWhiteSpace($BaselineArtifactsDir)) { "" } else { Join-Path $BaselineArtifactsDir "summary.txt" }
    $VerificationStatus = if ($summaryPath -and (Test-Path $summaryPath)) { "PASS" } else { "Not run in this invocation" }
}

$filesForHash = Get-ChildItem -Path . -Recurse -File -Force |
    ForEach-Object {
        [PSCustomObject]@{
            FullName = $_.FullName
            RelativePath = Convert-ToRepoRelativePath $_.FullName
        }
    } |
    Where-Object { -not (Test-ExcludedFromSourceHash $_.RelativePath) } |
    Sort-Object RelativePath

$hashes = foreach ($file in $filesForHash) {
    [PSCustomObject]@{
        Path = $file.RelativePath
        SHA256 = Get-FileSha256 $file.FullName
    }
}

$combinedInput = $hashes.SHA256 -join ''
$combinedBytes = [System.Text.Encoding]::UTF8.GetBytes($combinedInput)
$combinedSha = [System.Security.Cryptography.SHA256]::Create()
try {
    $sourceRootHash = [System.BitConverter]::ToString($combinedSha.ComputeHash($combinedBytes)).Replace('-', '')
}
finally {
    $combinedSha.Dispose()
}

$branch = Get-GitValue @("rev-parse", "--abbrev-ref", "HEAD")
$commit = Get-GitValue @("rev-parse", "HEAD")
$shortCommit = Get-GitValue @("rev-parse", "--short", "HEAD")
$sdkVersion = (& dotnet --version).Trim()

$solutionFiles = Get-ChildItem -LiteralPath . -File |
    Where-Object { $_.Extension -in ".sln", ".slnx" } |
    Sort-Object Name |
    ForEach-Object { "- " + $_.Name }

$projectFiles = Get-ChildItem -Path ".\src" -Recurse -Filter "*.csproj" -File |
    Sort-Object FullName |
    ForEach-Object { "- " + (Convert-ToRepoRelativePath $_.FullName) }

$generated = Get-Date -Format "yyyy-MM-dd HH:mm:ss zzz"
$commands = @(
    '$env:EJLIVE_DATABASE_PATH = "$env:TEMP\ejlive_codex_verify.db"',
    'dotnet restore .\EJLive.Unified.sln',
    'dotnet build .\EJLive.Unified.sln --no-restore -m:1 /p:BuildInParallel=false -v:m',
    'dotnet test .\src\EJLive.Tests\EJLive.Tests.csproj --no-restore -m:1 /p:BuildInParallel=false --logger "console;verbosity=minimal"',
    'dotnet run --project .\src\EJLive.Verification\EJLive.Verification.csproj --no-restore -m:1 /p:BuildInParallel=false'
)

$content = @"
# EJLive Phase-2 Source of Truth

Generated: $generated

## Repository Identity

- Branch: $branch
- Commit: $commit
- Short commit: $shortCommit
- .NET SDK: $sdkVersion
- Baseline artifacts: $baselineRelativePath
- Baseline timestamp: $baselineTimestamp
- Verification status: $VerificationStatus

## Solution Files

$($solutionFiles -join "`n")

## Project Files

$($projectFiles -join "`n")

## Source Root Integrity

- Files hashed: $($hashes.Count)
- Source root SHA256: $sourceRootHash
- Excluded from hash: .git/, artifacts/, docs/phase2-source-of-truth.md, bin/, obj/, .vs/

## Required Baseline Commands

~~~powershell
$($commands -join "`n")
~~~

## Track 001 Scope

- Source Truth / Baseline only.
- Build, test, and verification gate only.
- No feature work.
- No UI changes.
- No parser changes.
- No remote command behavior changes.

## Generated Baseline Files

- docs/phase2-source-of-truth.md
- artifacts/ActiveCompileMap.csv
- artifacts/baseline/<yyyyMMdd-HHmmss>/dotnet-info.log
- artifacts/baseline/<yyyyMMdd-HHmmss>/restore.log
- artifacts/baseline/<yyyyMMdd-HHmmss>/build.log
- artifacts/baseline/<yyyyMMdd-HHmmss>/test.log
- artifacts/baseline/<yyyyMMdd-HHmmss>/verification.log
"@

$docsPath = ".\docs\phase2-source-of-truth.md"
New-Item -ItemType Directory -Force -Path (Split-Path $docsPath) | Out-Null
$content | Out-File -FilePath $docsPath -Encoding UTF8
Write-Host "Source of truth written to docs/phase2-source-of-truth.md"
