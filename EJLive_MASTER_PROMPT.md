# EJLive Enterprise Master Prompt for Codex

Use this prompt when starting a major EJLive task.

## Start

1. Read AGENTS.md.
2. Inspect .codex/skills.
3. Inspect active solution/project files.
4. Inspect src.
5. Inspect legacy/original.
6. Read docs and merge reports.
7. Classify files before implementation.

## Goals

- Strengthen EJLive without breaking the current structure.
- Convert reference functionality into safe active services.
- Keep legacy code preserved unless safely promoted.
- Stabilize build graph.
- Improve sync reliability.
- Improve server/dashboard analytics and NOC UI.
- Add safe enterprise remote operations only.

## First implementation waves

1. Build graph and project reference audit.
2. Constants/contracts cleanup.
3. Persistent Sync State.
4. Client Outbox Queue.
5. Idempotency/checksum/offset tracking.
6. Chunked transfer protocol.
7. Archive indexing.
8. Server-side dashboard analytics.
9. UI redesign: cards, status colors, timeline, filters.
10. Safe command queue and audit.
11. Installer/service wiring.
12. Tests and regression guard.

## Security boundaries

Do not implement stealth, security disabling, hidden arbitrary terminal, log wiping, or unauthorized no-consent control.

Implement authorization, allowlist, audit, encryption, policy flags, visible admin, rollback, and uninstall.

## Output

Always provide:
- inspected files
- changed files
- risks
- build/test status
- rollback
- next step
