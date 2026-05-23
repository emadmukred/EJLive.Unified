# Duplicate and Promotion Map

## Duplicate Strategy

The active solution does not delete duplicate legacy code. Identical files are treated as evidence that the active assembly already covers that source. Different files remain preserved and are tracked by `OriginalSourceCatalog.TotalDifferentCSharpFromActive`.

## Current Totals

| Metric | Value |
| --- | ---: |
| Audited source roots | 30 |
| Audited files | 9930 |
| Audited C# files | 3175 |
| Non-identical C# files staged for review | 1523 |
| English-only source roots | 4 |

## Priority Promotion Candidates

| Candidate | Source roots | Reason |
| --- | --- | --- |
| Journal sync and XFS vendor adapters | `ChatGPT_LatestWorkspace`, `ChatGPT_ActiveTestPackage`, `FixedGpt`, `CodexMarege` | Strong service coverage and direct operational value. |
| Enhanced v5 client deltas | `EJLive_Client_v5_Enhanced` | Only 8 compiled C# files differ from the active source, making it a focused high-confidence promotion candidate. |
| Fleet prediction and operational state | `FixedGpt`, `CodexMarege`, `Coder01-orginal` | Adds higher-level monitoring and health behavior. |
| Settings and log viewer UI patterns | `Kimi_Agent` | English-only independent implementation with clean UI intent. |
| Menu and interaction consolidation | `EJLive_Menus_ai`, `EJLiveWorkCoder`, `EJLive_Enterprise_v3.4.0_Enhanced` | UI/menu variants can guide English tab and button consolidation without replacing active behavior first. |
| Remote control and Windows startup/access helpers | `FixedGpt`, `CodexMarege`, `Coder01` | Useful client-side operations; needs privilege and environment testing. |
| Parser behavior from logs and Excel files | `ChatGPT_ActiveTestPackage`, `Testing_UI`, `_codex_zip_study_20260515_053548` | Provides real input evidence for unit and integration tests. |

## Safe Refactor Policy

Shared code can be extracted only after its scenarios are represented by tests. Old duplicate code remains in `legacy/original` until the replacement is verified. If an old active duplicate is replaced, the removed block must be preserved in comments or reference source until behavior parity is proven.
