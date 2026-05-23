# Coder01 Study And Integration Notes

## Source Package

- Package studied: `C:\Users\user\Desktop\EJLIVE\EJLiveWorkCoder\Coder01.zip`
- Filtered source extraction: `_coder01_source_study`
- Extracted source files: 136
- Extracted C# files: 71
- Extracted C# lines: 17,600
- Extracted project files: 7, all XML-valid.

Large generated folders, binaries, `dist`, `bin`, `obj`, packages, and `.vs` were excluded from the study extraction.

## Useful Coder01 Patterns

- Client UI should stay ATM-first and light: connection, queue sending, journal inspection, services/remote operations, settings.
- Remote screen and screenshot operations should be a single operational workflow.
- Server dashboard needs visual ATM state cards, not only tabular grids.
- ATM state colors should carry operational meaning:
  - green: connected and active
  - yellow: connected but idle
  - blue: syncing or waiting
  - orange: supervisor mode
  - red: recently offline or warning
  - gray: critical offline
- Journal workflows benefit from quick filters for approved, declined, capture, cash, and errors.

## Integrated Changes In Unified Project

- Added `Network Map` tab to `EJLive.Server.WinForms`.
- Added Coder01-inspired ATM cards using the existing unified `ATMInfo.GetCardState`, `GetCardColor`, and `GetStatusLabel` contract.
- Server starts listening automatically on form show, while retaining manual Start/Stop buttons.
- Client auto-connects on form show when `AutoConnect` is enabled.
- Reworked client `Remote Control` tab into a split screen:
  - left: live screenshot preview and capture actions
  - right: pending remote command grid and remote log
- Added screenshot capture, local screenshot persistence, and `GhostFrame` network send when connected.
- Added protocol helpers for `GhostStart`, `GhostFrame`, and `GhostStop`.
- Added server logging for received remote screen frames.
- Added targeted and broadcast remote command routing from server to client.
- Added client-side command handling and command result acknowledgements.
- Added file watcher polling fallback and duplicate unchanged event suppression.
- Added monitoring `Operational Map` cards.
- Added journal quick filter buttons and multi-match highlight behavior.
- Updated layout flow panels to wrap controls so action buttons remain usable at smaller window widths.

## Runtime Fixes Driven By Visual Studio Screenshots

- Fixed `DatabaseManager.Initialize` for existing SQLite files whose `audit_log` or `sync_records` tables were created before the `created_at_utc` and `updated_at_utc` columns existed.
- Schema initialization now creates base tables first, adds missing columns through `ALTER TABLE`, then creates indexes.
- This prevents the `SQL logic error no such column: created_at_utc` crash seen in both client and server screenshots.

## Verification

- `dotnet build EJLive.Unified\EJLive.Unified.slnx --configuration Debug`: success, 0 warnings, 0 errors.
- `dotnet build EJLive.Unified\EJLive.Unified.slnx --configuration Release`: success, 0 warnings, 0 errors.
- SQLite legacy-schema migration probe: `MigrationOK auditColumn=created_at_utc auditIndex=ix_audit_log_created_at syncColumn=updated_at_utc`.
- Network probe: `NetworkOK connectReturned=True accepted=True atm=ATM-SMOKE errors=0`.
- Verification project probe: SQLite, network, remote commands, file watcher, and WinForms UI composition all pass.
