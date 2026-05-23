# Current Base Project Validation

## Scope

The active base project is the buildable solution in `EJLive.Unified.sln` and `EJLive.Unified.slnx`. It contains 12 active projects and links all preserved legacy material through `EJLive.LegacyReference`.

## Active Projects

| Project | Layer | Role |
| --- | --- | --- |
| `EJLive.Application` | Application | Readiness, workflow, and data-flow host. |
| `EJLive.Business` | Application/Business | Unified runtime, source catalog, service orchestration, and audit metadata. |
| `EJLive.Client.WinForms` | Presentation | Active client UI. |
| `EJLive.Server.WinForms` | Presentation | Active server UI. |
| `EJLive.Monitoring.WinForms` | Presentation | Active monitoring UI. |
| `EJLive.Monitor` | Presentation | Legacy monitoring UI shell. |
| `EJLive.Installer.WinForms` | Presentation | Installer UI and setup wizard link. |
| `EJLive.Core` | HAL/Data/Core | Engines, services, XFS/vendor abstractions, protocol, database, and security. |
| `EJLive.Shared` | Data/Core | Shared state and support types. |
| `EJLive.LegacyReference` | Reference | Links original source roots, docs, tools, support folders, and external reference materials. |
| `EJLive.Tests` | Verification | Unit tests for promoted reusable behavior. |
| `EJLive.Verification` | Verification | Integration probes for database, UI, network, command routing, file watcher, original-audit coverage, and file linkage. |

## Validation Coverage

- UI to database flow is exercised through `EJLiveApplicationHost`, `UnifiedBusinessRuntime`, `DatabaseManager`, and WinForms composition probes.
- Buttons and visual surfaces are checked through tab/control composition counts for client, server, monitoring, installer, and legacy monitor forms.
- Remote command and network paths are checked with a live local server/client loop.
- File watching is checked with a temporary EJ log file.
- All files are accounted for through `docs/09-file-function-inventory.csv`.
- All original source roots are required to have `summary`, `file-manifest`, and `project-dependencies` audit files.

## Known Safety Rule

Legacy Arabic strings and duplicate code remain preserved in reference source. Active UI labels and newly promoted identifiers must remain English. Any legacy file promoted into active compiled code must be translated, tested, and verified before replacing an existing active path.

