# EJLive Unified

EJLive Unified is the merged C#/.NET workspace for the EJLive Enterprise client, server, monitoring, installer, core engines, shared utilities, and preserved legacy/source references.

## Solution

- `EJLive.Unified.slnx` is the lightweight Visual Studio solution file.
- `EJLive.Unified.sln` is generated for classic Visual Studio/MSBuild compatibility.
- `Directory.Build.props` disables project-reference parallel discovery because the installed .NET SDK workload resolver can fail silently under parallel WinForms solution builds.

## Layers

- `src/EJLive.Application`: application workflow surface and readiness/data-flow descriptions.
- `src/EJLive.Business`: unified business runtime that binds managers, services, engines, sync state, alerts, and vendor capability services.
- `src/EJLive.Core` and `src/EJLive.Shared`: data/core layer, SQLite access, constants, models, security, protocol, and operational engines.
- `src/EJLive.Client.WinForms`, `src/EJLive.Server.WinForms`, `src/EJLive.Monitoring.WinForms`, `src/EJLive.Monitor`, `src/EJLive.Installer.WinForms`: presentation layer.
- `src/EJLive.LegacyReference`: reference-only project that links preserved legacy/imported code and documentation.
- `legacy/original`: 30 extracted source archives and packages, including Coder01, ChatGPT latest workspace, active test packages, Kimi Agent, FixedGpt, `EJLive_Client_v5_Enhanced`, historical enterprise releases, testing bundles, and source-study archives.
- `docs`: architecture, data-flow, sequential audit, duplicate/promotion map, final checklist, and generated file-function inventory.
- `docs/08-final-readiness-review.md`: final base-project readiness decision and phase-two entry report.

## Verification

Use:

```powershell
dotnet restore .\EJLive.Unified.sln
dotnet build .\EJLive.Unified.sln --no-restore -m:1 /p:BuildInParallel=false
dotnet test .\src\EJLive.Tests\EJLive.Tests.csproj --no-restore -m:1 /p:BuildInParallel=false
dotnet run --project .\src\EJLive.Verification\EJLive.Verification.csproj --no-restore
```

Set `EJLIVE_DATABASE_PATH` when you need to force SQLite into a writable location during sandboxed or CI runs.
