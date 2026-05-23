# EJLive Client v5 Enhanced Integration

## Source

`EJLive_Client_v5_Enhanced.zip` is available at `C:\Users\user\Desktop\CodexMarege\Web\EJLive_Client_v5_Enhanced.zip`. The extracted source is preserved at `legacy/original/EJLive_Client_v5_Enhanced`.

## Audit Result

| Metric | Value |
| --- | ---: |
| Files | 186 |
| C# files | 149 |
| Project files | 9 |
| Solution files | 1 |
| Forms/UI files | 17 |
| Service/manager/engine files | 72 |
| C# files identical to active compiled source | 141 |
| C# files different from active compiled source | 8 |
| Files containing Arabic text | 34 |

Full details:

- `docs/original-audit/EJLive_Client_v5_Enhanced-summary.md`
- `docs/original-audit/EJLive_Client_v5_Enhanced-file-manifest.csv`
- `docs/original-audit/EJLive_Client_v5_Enhanced-project-dependencies.csv`

## Architectural Mapping

| Requested layer | EJLive unified mapping | v5 role |
| --- | --- | --- |
| Application Layer | `src/EJLive.Application`, `src/EJLive.Business` | Runtime orchestration, workflow readiness, service coordination, sync and command paths. |
| HAL Layer | `src/EJLive.Core/Engine`, `src/EJLive.Core/Services`, `src/EJLive.Core/Xfs` | Device-facing abstractions, protocol, file watcher, XFS/vendor adapter candidates. |
| Drivers Layer | Preserved reference source plus future `Core/Xfs` adapter promotion | Hardware/vendor behavior candidates must be promoted with tests before becoming compiled drivers. |

## Promotion Decision

The v5 package is close to the active unified source: 141 of 149 C# files match active compiled source by hash. The 8 different C# files are preserved and tracked by `OriginalSourceCatalog` for focused promotion. They should be compared function-by-function before any active replacement, with unit or verification coverage added first.

