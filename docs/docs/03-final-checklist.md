# Final Merge Checklist

| Check | Status | Evidence |
|---|---|---|
| Current workspace inspected | Done | Existing `src` projects inventoried. |
| Attached archives inspected | Done | Five archives extracted to `legacy/original` and counted. |
| All imported code preserved | Done | `EJLive.LegacyReference` links `legacy/original/**/*`. |
| Layered structure added | Done | `EJLive.Application` and `EJLive.Business` added above existing Core/Shared and WinForms projects. |
| UI labels in compiled forms use English | Done | Compiled form files scanned for Arabic text; no Arabic found. |
| Duplicate code handled safely | Done | Duplicates retained as reference-only source; compiled assembly uses curated unified files. |
| Unit tests added | Done | `UnifiedRuntimeTests` covers security round trip, business runtime sync tracking, and application flow/readiness. |
| End-to-end verification added | Done | `EJLive.Verification` checks DB migration, layers, network, remote command routing, file watcher, UI composition, and file linkage. |
| Classic `.sln` required by Visual Studio | Done | `EJLive.Unified.sln` generated from the project list. |
| File role inventory | Done | `docs/09-file-function-inventory.csv` generated for all non-build-output files. |

## Feature Coverage

| Original Function Area | Unified Location |
|---|---|
| Client agent connection | `EJLive.Client.WinForms`, `EJLive.Core.Engine.NetworkEngine` |
| Journal file watching and outbox | `EJLive.Core.Engine.FileWatcherEngine`, `EJLive.Core.Engine.JournalOutbox` |
| Journal sync tracking | `EJLive.Business.UnifiedBusinessRuntime`, `EJLive.Core.Services.JournalSyncTrackingService` |
| Server listener and client sessions | `EJLive.Server.WinForms`, `EJLive.Core.Engine.ServerEngine` |
| Remote commands and ghost screen | `EJLive.Client.WinForms`, `EJLive.Server.WinForms`, `CommunicationProtocol`, `GhostRemoteEngine` |
| Monitoring dashboard | `EJLive.Monitoring.WinForms`, `EJLive.Monitor` |
| SQLite database and migrations | `EJLive.Core.Services.DatabaseManager` |
| Security/hash/compression | `EJLive.Shared.SecurityHelper` |
| Vendor paths and XFS references | `EJLive.Core.Constants`, `EJLive.Core.Xfs`, reference source |
| Installer/setup | `EJLive.Installer.WinForms`, linked `EJLive.Setup.SetupWizardForm` |
| Kimi Agent ideas | Preserved in `legacy/original/Kimi_Agent`, mapped for future promotion |

## Future Promotion Rule

Do not delete legacy/reference code during promotion. First extract the behavior into Core/Business/Application, then add unit or verification coverage, then update the UI caller, then leave the old source linked until behavior is confirmed in the verification suite.
