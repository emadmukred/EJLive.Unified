# EJLive Unified Checklist

## Buildable Solution

- `EJLive.Unified.sln` is present for Visual Studio and MSBuild.
- `EJLive.Unified.slnx` is present for lightweight Visual Studio solution loading.
- `Directory.Build.props` keeps project reference discovery deterministic for WinForms solution builds.
- Active projects are layered into Application, Business, Data/Core, Presentation, Verification, and Reference.

## Preservation

- All imported source roots remain under `legacy/original`.
- Historical and future-development files are linked through `EJLive.LegacyReference`.
- Inactive legacy code is preserved as reference source instead of being deleted.
- Source-role metadata for 30 original roots, including `EJLive_Client_v5_Enhanced`, is promoted into `OriginalSourceCatalog`.

## English Active Surface

- Active WinForms tabs checked by verification use English labels.
- Active Business/Application/Core identifiers added during consolidation are English.
- Legacy Arabic text remains only in preserved reference archives unless a file is promoted into the active assembly, at which point labels and public identifiers must be normalized to English.

## Function Coverage

- Client presentation: connection, sync, journal, remote control, services, settings, and agent configuration.
- Server presentation: fleet, network map, journal viewer, sync dashboard, remote commands, alerts, archive, reports, and settings.
- Monitoring presentation: overview, operational map, device state, realtime sync, XFS events, vendor logs, and reports.
- Business services: operational state, journal sync, alerts, role access, vendor capability catalog, and XFS analysis.
- Core engines: network, server, file watcher, image sync, report export, transaction analysis, ghost remote, protocol, and security helpers.
- Data/Core: SQLite migration, audit log, sync records, constants, models, and shared state.

## Verification Gates

- Unit tests cover encryption/compression round-trip, business runtime state, application data flow, and original source audit catalog.
- Verification app probes database migration, application/business readiness, network transport, remote command routing, file watcher fallback, UI composition, and file linkage.
- `docs/09-file-function-inventory.csv` must be regenerated whenever files are added or removed.
- `docs/original-audit` must contain one current summary, file manifest, and dependency CSV for every source root under `legacy/original`.
