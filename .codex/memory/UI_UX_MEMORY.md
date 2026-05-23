# UI / UX Memory

## UI direction

EJLive UI must support NOC-style operation:
- clear visual hierarchy
- no overlapping buttons
- readable dense data
- consistent spacing
- consistent status cards
- high-contrast dark mode for monitoring rooms
- server-side dashboards and reports

## Status colors

- Green: online and sending data
- Yellow: connected but no recent journal data
- Red: disconnected or critical fault
- Gray: stale/offline beyond threshold
- Orange: supervisor/maintenance state

## Journal Sync view

Preferred visualization:
Source Read → Local Backup → Sent → Server Stored → Verified

## Client UI rule

Keep client lightweight. Move heavy analytics, charts, reports, summaries, and tables to server/dashboard.

## Server/dashboard UI rule

Every button must map to a real service action or be clearly marked as not implemented.
