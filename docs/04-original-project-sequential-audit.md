# Original Source Sequential Audit

All source roots under `legacy/original` were audited one root at a time with `tools/analyze-original-project.ps1`. Build output folders are excluded. Each root has a file-level manifest, dependency CSV, and markdown summary in `docs/original-audit`.

## Totals

| Metric | Value |
| --- | ---: |
| Source roots | 30 |
| Files | 9930 |
| C# files | 3175 |
| Forms/UI files | 404 |
| Service/manager/engine files | 1567 |
| C# files different from active compiled source | 1523 |

## Source Roots

| Source root | Files | C# | Projects | Solutions | Forms | Services/Engines | Arabic files | Identical C# | Different C# |
| --- | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: |
| `_coder01_source_study` | 136 | 71 | 7 | 1 | 14 | 28 | 72 | 18 | 53 |
| `_codex_zip_study_20260515_053548` | 874 | 591 | 35 | 5 | 65 | 318 | 189 | 375 | 216 |
| `ChatGPT_ActiveTestPackage` | 166 | 95 | 7 | 1 | 7 | 54 | 18 | 54 | 41 |
| `ChatGPT_LatestWorkspace` | 152 | 94 | 7 | 1 | 7 | 54 | 16 | 55 | 39 |
| `Coder01` | 1398 | 148 | 14 | 2 | 29 | 60 | 154 | 43 | 105 |
| `Coder01 (2)` | 702 | 77 | 7 | 1 | 15 | 32 | 78 | 25 | 52 |
| `Coder01-orginal` | 746 | 77 | 7 | 1 | 15 | 32 | 79 | 25 | 52 |
| `CodexMarege` | 672 | 134 | 7 | 1 | 17 | 70 | 56 | 84 | 50 |
| `CodexMarege_restructured_Replit` | 176 | 134 | 7 | 1 | 17 | 70 | 59 | 95 | 39 |
| `CodexMarege-refactor_Gethub` | 106 | 79 | 4 | 1 | 13 | 34 | 33 | 53 | 26 |
| `EJLive_APPs` | 9 | 0 | 0 | 0 | 0 | 0 | 0 | 0 | 0 |
| `EJLive_Client_v5_Enhanced` | 186 | 149 | 9 | 1 | 17 | 72 | 34 | 141 | 8 |
| `EJLive_Enterprise_20260510_build_candidate` | 21 | 18 | 2 | 1 | 0 | 6 | 5 | 7 | 11 |
| `EJLive_Enterprise_20260510_latest_workspace` | 152 | 94 | 7 | 1 | 7 | 54 | 16 | 55 | 39 |
| `EJLive_Enterprise_active_test_package_2026-05-10` | 166 | 95 | 7 | 1 | 7 | 54 | 18 | 54 | 41 |
| `EJLive_Enterprise_v3.2.1_Enhanced` | 48 | 36 | 6 | 1 | 7 | 10 | 18 | 0 | 36 |
| `EJLive_Enterprise_v3.4.0` | 426 | 71 | 7 | 1 | 14 | 28 | 66 | 16 | 55 |
| `EJLive_Enterprise_v3.4.0_Enhanced` | 56 | 42 | 6 | 1 | 7 | 18 | 24 | 0 | 42 |
| `EJLive_Enterprise_v3.4.0_latest` | 146 | 96 | 6 | 1 | 7 | 55 | 17 | 41 | 55 |
| `EJLive_Menus_ai` | 60 | 45 | 6 | 1 | 7 | 20 | 25 | 0 | 45 |
| `EJLive_replik` | 56 | 42 | 6 | 1 | 7 | 18 | 24 | 0 | 42 |
| `EJLive-CodexMarege-fixedGpt` | 203 | 134 | 7 | 1 | 17 | 70 | 38 | 83 | 51 |
| `EJLive.Unified` | 180 | 141 | 9 | 1 | 17 | 72 | 34 | 134 | 7 |
| `EJLiveWorkCoder` | 60 | 45 | 6 | 1 | 7 | 20 | 25 | 0 | 45 |
| `FixedGpt` | 203 | 134 | 7 | 1 | 17 | 70 | 38 | 83 | 51 |
| `Kimi_Agent` | 44 | 36 | 4 | 1 | 7 | 11 | 0 | 0 | 36 |
| `Kimi_Agent_تحليل نظام EJLive` | 45 | 36 | 4 | 1 | 7 | 11 | 0 | 0 | 36 |
| `src` | 55 | 36 | 8 | 1 | 7 | 8 | 0 | 32 | 4 |
| `System-Analyzer-Unified_Replit` | 1549 | 63 | 7 | 2 | 11 | 24 | 67 | 10 | 53 |
| `Testing_UI` | 1137 | 362 | 27 | 4 | 35 | 194 | 127 | 169 | 193 |

## Current Decision

The active project stays buildable and English-facing. All old source roots remain preserved in `legacy/original` and are linked through `EJLive.LegacyReference`. Files that are identical to active compiled source are treated as covered. Files that differ remain staged for safe promotion with tests, not deleted.

