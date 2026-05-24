# EJLive Phase-2 Source of Truth

Generated: 2026-05-24 05:00:01 +03:00

## Repository Identity

- Branch: 3-track-001-source-truth-and-baseline-gate
- Commit: 9ccaec5d7ddc0dd92f50a6d88917ea5be4e1f4f1
- Short commit: 9ccaec5
- .NET SDK: 10.0.203
- Baseline artifacts: artifacts/baseline/20260524-045745
- Baseline timestamp: 20260524-045745
- Verification status: PASS

## Solution Files

- EJLive.Unified.sln
- EJLive.Unified.slnx

## Project Files

- src/EJLive.Application/EJLive.Application.csproj
- src/EJLive.Business/EJLive.Business.csproj
- src/EJLive.Client.Service/EJLive.Client.Service.csproj
- src/EJLive.Client.WinForms/EJLive.Client.WinForms.csproj
- src/EJLive.Core/EJLive.Core.csproj
- src/EJLive.Installer.WinForms/EJLive.Installer.WinForms.csproj
- src/EJLive.LegacyReference/EJLive.LegacyReference.csproj
- src/EJLive.Monitor/EJLive.Monitor.csproj
- src/EJLive.Monitoring.WinForms/EJLive.Monitoring.WinForms.csproj
- src/EJLive.Server.WinForms/EJLive.Server.WinForms.csproj
- src/EJLive.Shared/EJLive.Shared.csproj
- src/EJLive.Tests/EJLive.Tests.csproj
- src/EJLive.Verification/EJLive.Verification.csproj

## Source Root Integrity

- Files hashed: 806
- Source root SHA256: 7AB5DFDBE6793913A17D4CF1865C333C7A4526F2C273D02C3D48DCF01E5B0C90
- Excluded from hash: .git/, artifacts/, docs/phase2-source-of-truth.md, bin/, obj/, .vs/

## Required Baseline Commands

~~~powershell
$env:EJLIVE_DATABASE_PATH = "$env:TEMP\ejlive_codex_verify.db"
dotnet restore .\EJLive.Unified.sln
dotnet build .\EJLive.Unified.sln --no-restore -m:1 /p:BuildInParallel=false -v:m
dotnet test .\src\EJLive.Tests\EJLive.Tests.csproj --no-restore -m:1 /p:BuildInParallel=false --logger "console;verbosity=minimal"
dotnet run --project .\src\EJLive.Verification\EJLive.Verification.csproj --no-restore -m:1 /p:BuildInParallel=false
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
