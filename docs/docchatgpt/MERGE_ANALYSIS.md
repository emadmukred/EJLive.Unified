# EJLive merged system analysis

## Compared sources

1. `Coder01`
   - Most stable build baseline.
   - Stronger legacy engines for journal watching, networking, transaction analysis, and report export.
   - Simpler UI and fewer integration services.

2. `EJLive_Coder_New\EJLive_Enterprise_Ultimate`
   - Same general architecture as the Ultimate line.
   - Useful as a second validation source, but one build was previously affected by locked Visual Studio/debug output files.

3. `EJLive_Enterprise_Ultimate`
   - Best base for the merged system.
   - Richest WinForms surfaces, database services, alerting, audit logging, role/security helpers, archive/journal viewer, command handling, and SQLite deployment fixes.

## Merge decision

The fourth system is based on `EJLive_Enterprise_Ultimate` because it already contains the most complete client/server/monitoring structure and the SQLite native deployment fix. The better reliability ideas from `Coder01` were kept as design criteria: offset-based journal mirroring, safe file reads, local backup, polling fallback, command audit trail, and no destructive edits to original projects.

## Fixes applied in this fourth system

- Kept SQLite runtime deployment through `Directory.Build.targets`, including `x86` and `x64` `SQLite.Interop.dll`.
- Fixed journal outbox reliability by assigning missing item ids before enqueue and avoiding invalid completed-state updates when no sync id exists.
- Added `CMD_SYNC_TIME` and wired the client remote command handler to acknowledge it.
- Replaced placeholder designer button handlers in `ServerMainForm` with real server start/stop, command sending, archive scanning, and log clearing behavior.
- Replaced placeholder buttons in `MainDashboardForm` with refresh, remote command, archive index, report export, and settings navigation behavior.
- Adjusted client theme colors to a clearer operational palette with better contrast and less visually heavy dark styling.

## Remaining recommended work

- Normalize Arabic source encoding in legacy files to remove mojibake comments/text shown by some tools.
- Add an explicit ATM selector to the standalone monitoring dashboard command strip instead of sending commands to the first connected ATM.
- Add automated integration tests around `FileWatcherEngine -> JournalOutbox -> JournalProcessor -> ServerEngine`.
- Review production policy for actual OS time setting because changing system time remotely requires elevated Windows privileges and should remain audited.
