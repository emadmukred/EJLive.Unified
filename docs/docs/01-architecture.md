# EJLive Unified Architecture

## Layer Map

| Layer | Projects | Role |
|---|---|---|
| Application | `EJLive.Application` | Hosts application workflows, readiness checks, and data-flow descriptions used by UI and verification. |
| Business | `EJLive.Business` | Provides `UnifiedBusinessRuntime`, a single composition point for managers, services, engines, sync state, alerts, vendor capability lookup, and analytics. |
| Data/Core | `EJLive.Core`, `EJLive.Shared` | Owns constants, models, SQLite schema/migrations, protocol frames, network/file/transaction engines, security, compression, hashes, and logging. |
| Presentation | `EJLive.Client.WinForms`, `EJLive.Server.WinForms`, `EJLive.Monitoring.WinForms`, `EJLive.Monitor`, `EJLive.Installer.WinForms` | WinForms UI for client agent, server operations, monitoring dashboard, legacy monitor, and installer/setup wizard. |
| Reference | `EJLive.LegacyReference`, `legacy/original` | Preserves every imported project/file as reference source without compiling conflicting duplicate classes. |

## Main Projects

| Project | Functional Role |
|---|---|
| `EJLive.Client.WinForms` | ATM-side agent UI: connection, journal sync, journal analysis, remote control, services, settings, and agent configuration. |
| `EJLive.Server.WinForms` | Server/NOC UI: fleet map, sync dashboard, journal viewer, remote command dispatch, alerts, archive, reports, and settings. |
| `EJLive.Monitoring.WinForms` | Monitoring dashboard with overview, operational map, realtime sync, XFS/vendor views, and reports. |
| `EJLive.Installer.WinForms` | Installer UI plus linked setup wizard. |
| `EJLive.Monitor` | Legacy monitoring dashboard retained as a buildable presentation artifact. |
| `EJLive.Verification` | End-to-end smoke verification for database migration, application/business layer, network, remote commands, file watching, UI composition, and file linkage. |
| `EJLive.Tests` | MSTest unit tests for unified reusable functions and runtime composition. |

## Imported Sources

The following archives were extracted under `legacy/original` and linked through `EJLive.LegacyReference`:

| Import | Files | C# Files | Projects | Solutions |
|---|---:|---:|---:|---:|
| `Coder01` | 1004 | 85 | 7 | 1 |
| `ChatGPT_LatestWorkspace` | 152 | 94 | 7 | 1 |
| `ChatGPT_ActiveTestPackage` | 166 | 95 | 7 | 1 |
| `Kimi_Agent` | 44 | 36 | 4 | 1 |
| `FixedGpt` | 203 | 134 | 7 | 1 |

## Duplicate Strategy

Duplicate and conflicting source from the archives is not deleted. The working assembly compiles a curated, English-named implementation and keeps legacy duplicates as reference-only source. This prevents duplicate type collisions such as repeated `ServerMainForm`, `ClientMainForm`, `NetworkEngine`, `ImageSyncEngine`, `ReportExportEngine`, `TransactionAnalysisEngine`, and `IXfsVendorAdapter` definitions while preserving all source lines for comparison and future extraction.

## Language Policy

Compiled UI labels, workflow names, and new public code use English. Arabic text remains only in preserved legacy/reference source so original behavior and source history are not destroyed before safe migration.
