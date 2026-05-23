# EJLive Unified Architecture

## Purpose

EJLive Unified is a staged consolidation of all imported EJLive C# sources. The active solution keeps a clean buildable surface while every original source archive remains linked through the reference project for traceability and future promotion.

## Solution Structure

| Layer | Projects | Responsibility |
| --- | --- | --- |
| Application | `src/EJLive.Application` | Host workflows, readiness checks, and end-to-end data-flow descriptions. |
| Business | `src/EJLive.Business` | Unified runtime, source catalog, service coordination, alerts, journal sync, operational state, and vendor capabilities. |
| Data/Core | `src/EJLive.Core`, `src/EJLive.Shared` | SQLite schema, constants, models, engines, protocol, security, XFS adapters, stores, and reusable helpers. |
| Presentation | `src/EJLive.Client.WinForms`, `src/EJLive.Server.WinForms`, `src/EJLive.Monitoring.WinForms`, `src/EJLive.Monitor`, `src/EJLive.Installer.WinForms` | Active English WinForms surfaces and legacy monitor/setup shells. |
| Verification | `src/EJLive.Tests`, `src/EJLive.Verification` | Unit tests, integration probes, UI composition checks, database migration checks, and file linkage checks. |
| Reference | `src/EJLive.LegacyReference`, `legacy/original` | Preserved imported projects, historical code, reports, binaries, logs, Excel requirements, and study archives. |

## Application / HAL / Drivers Mapping

The requested three-layer hardware-oriented model is represented inside the unified .NET solution as follows:

| Requested layer | Active project mapping | Responsibility |
| --- | --- | --- |
| Application Layer | `EJLive.Application`, `EJLive.Business`, active WinForms entry points | Workflows, calculations, orchestration, validation, sync state, and user-facing actions. |
| HAL Layer | `EJLive.Core.Engine`, `EJLive.Core.Services`, `EJLive.Core.Xfs` | Device-facing abstractions, protocol envelopes, XFS/vendor adapter boundaries, file watching, network transport, and remote command routing. |
| Drivers Layer | Promoted XFS/vendor adapters plus preserved legacy driver/reference material | Vendor/device-specific behavior. Legacy driver-like code remains reference-only until promoted with tests. |

## Active Runtime Composition

`UnifiedBusinessRuntime` binds the currently promoted service and engine set:

- `DatabaseManager`
- `OperationalStateStore`
- `JournalSyncTrackingService`
- `JournalSyncService`
- `AlertManager`
- `RoleBasedAccess`
- `VendorRootCapabilityService`
- `XfsLogAnalysisService`
- `TransactionAnalysisEngine`
- `FileWatcherEngine`
- `ImageSyncEngine`
- `ReportExportEngine`
- `GhostRemoteEngine`

The runtime also exposes `OriginalSourceCatalog`, which records every audited original source root, its role, and the number of non-identical C# files still staged for safe promotion.

## Preservation Rule

No imported source root is deleted. Code that is not compiled in the active layer remains available as reference material through `EJLive.LegacyReference` and `legacy/original`. Promotion into the active assembly requires:

1. A documented source and functional role.
2. A behavior-preserving extraction or adapter.
3. Unit or verification coverage.
4. A successful solution build and verification run.
