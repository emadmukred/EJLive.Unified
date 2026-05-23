# File Linkage Map

This pass links every source, reference, document, tool, and preserved artifact into the unified Visual Studio solution without compiling structurally broken duplicate code.

## Solution Membership

Both solution files include the same nine projects:

| Project | Role |
| --- | --- |
| `src/EJLive.Shared` | Shared logging, retry, security, and helper contracts. |
| `src/EJLive.Core` | Canonical models, services, SQLite, network engines, sync, remote command, XFS abstractions. |
| `src/EJLive.Client.WinForms` | Buildable ATM-side client UI and runtime workflows. |
| `src/EJLive.Server.WinForms` | Buildable server/NOC UI, network map, remote commands, archive, reports, sync views. |
| `src/EJLive.Monitoring.WinForms` | Buildable monitoring dashboard and operational visualization. |
| `src/EJLive.Installer.WinForms` | Buildable installer shell and linked setup wizard. |
| `src/EJLive.Monitor` | Legacy monitoring dashboard compiled as `EJLive.Monitor.Legacy`. |
| `src/EJLive.LegacyReference` | Non-compiling reference project for preserved files and unprojected folders. |
| `src/EJLive.Verification` | Runtime and linkage verification probes. |

## Compile vs Reference Strategy

| Area | Compiled files | Reference-only files |
| --- | --- | --- |
| Core | `Constants.cs`, `UnifiedModels.cs`, `CoreServices.cs`, `CommunicationProtocol.cs`, `JournalOutbox.cs`, `NetworkEngine.cs`, `OperationalEngines.cs`, `XfsModels.cs` | Duplicate/incomplete Core models, services, engine splits, XFS adapters under `reference-source`. |
| Client | `Program.cs`, `ClientMainForm.cs`, `AssemblyInfo.cs` | Legacy controls, legacy client services, designer/resx files under `reference-source`. |
| Server | `Program.cs`, `ServerMainForm.cs`, `AssemblyInfo.cs` | Older forms, designer/resx files, and `Services/*` under `reference-source`. Their behavior is represented in the unified `ServerMainForm` tabs and Core engines. |
| Monitoring | `Program.cs`, `MainDashboardForm.cs`, `DashboardModels.cs`, `AssemblyInfo.cs` | Legacy designer/resx and backup assembly metadata under `reference-source`. |
| Installer | `Program.cs`, `InstallerForm.cs`, `AssemblyInfo.cs`, linked `../EJLive.Setup/SetupWizardForm.cs` | Legacy installer designer and backup metadata under `reference-source`. |
| Legacy Monitor | `MonitoringDashboard.cs`, `MonitoringDashboard.Designer.cs`, linked `LightUiTheme.cs`, `AssemblyInfo.cs` | Config/package metadata. |

The explicit `EnableDefaultItems=false` projects prevent broken or duplicate legacy files from being compiled accidentally while still showing them in Visual Studio.

## Unprojected Folder Handling

`EJLive.LegacyReference` now links these otherwise unowned areas:

- Root artifacts: `README.md`, `src.zip`, `EJLive.Unified.sln`, `EJLive.Unified.slnx`.
- Documentation: `docs/**/*`.
- Tools: `tools/**/*`.
- Preserved originals: `legacy/original/**/*`.
- Old server package: `src/EJLive.Server/**/*`.
- Setup source folder: `src/EJLive.Setup/**/*`.

`src/EJLive.Setup/SetupWizardForm.cs` is also compiled into `EJLive.Installer.WinForms` and exposed through the `Setup Wizard` button, so it is both preserved and executable.

## Functional Mapping

| Legacy / reference responsibility | Active runtime location |
| --- | --- |
| `NetworkManager`, `AdvancedNetworkManager`, client `NetworkEngine` variants | `EJLive.Core.Engine.NetworkEngine` plus `EJLive.Client.WinForms.ClientMainForm`. |
| Old server `RemoteControlService` | `EJLive.Core.Engine.ServerEngine`, `RemoteCommandEnvelope`, server `Remote Commands` tab. |
| Old server `JournalAnalyticsService` | `TransactionAnalysisEngine`, server `Reports` and `Journal Viewer` tabs. |
| Legacy server dashboard form | `EJLive.Server.WinForms.ServerMainForm` with Fleet, Network Map, Journal Viewer, Sync Dashboard, Remote Commands, Alerts, Archive, Reports, Settings. |
| Legacy client controls/services | Unified client tabs: Connection, Sync, Journal, Remote Control, Services, Settings, Agent Config. |
| Setup wizard source | `EJLive.Installer.WinForms` action button opens `EJLive.Setup.SetupWizardForm`. |
| Legacy monitoring dashboard | Compiled in `EJLive.Monitor.Legacy` and included in verification construction probe. |

## Verification

`EJLive.Verification` now checks:

- SQLite legacy schema migration.
- Client/server TCP connection.
- Remote command routing and command result return path.
- File watcher event/polling fallback.
- Client, server, monitoring, installer, and legacy monitor form construction.
- Solution/project linkage for all files, including root files, tools, docs, legacy source, unprojected server/setup folders, and reference-source project items.
- The generated file-function inventory at `docs/09-file-function-inventory.csv`.

Latest linkage probe result:

```text
PASS Project file linkage: files=402, inventoryRows=402, projects=9, missingProjects=, failedMarkers=, unlinkedFolders=, unaccounted=
```

The inventory can be regenerated with:

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File .\tools\Audit-ProjectFileLinkage.ps1
```
