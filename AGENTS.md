# AGENTS.md — EJLive Enterprise Codex Operating Instructions

## 1. Mission

You are responsible for analyzing, unifying, rebuilding, developing, testing, and stabilizing the EJLive project.

EJLive is an ATM electronic journal archiving, monitoring, synchronization, dashboard, and controlled operations system built primarily with C#/.NET and WinForms. The project includes active source code, legacy/original sources, preserved references, documentation, and prior merged variants.

The core mission is to strengthen the current active project without losing legacy capabilities, without breaking the current structure, and without claiming unsupported build/runtime success.

EJLive is a C#/.NET WinForms ATM electronic journal archiving, synchronization, monitoring, reporting, and controlled operations platform for ATM fleets including NCR, GRG, and Wincor/Nixdorf environments.

---

## 2. Mandatory Pre-Work Before Any Development

Before implementation, always:

1. Read the active solution/project files.
2. Inspect the current active source under `src`.
3. Inspect `legacy/original` as source material for missing or historical capabilities.
4. Read reference documents and merge reports.
5. Map the layers:
   - Client / Agent
   - Server
   - Monitoring / Dashboard
   - Installer
   - Core
   - Shared
   - Models
   - Services
   - Engines
   - Storage
   - UI / Forms
6. Classify files as:
   - active compiled code
   - reference-only
   - legacy/original
   - unlinked
   - broken
   - safe to promote
   - unsafe to compile directly
7. Identify duplicate types, invalid XML project files, missing definitions, drifted contracts, and incompatible namespaces.
8. Produce a plan before large changes.

Do not make broad changes before understanding the build graph and current runtime authority.

---

## 3. Project Authority and Conflict Policy

When active code conflicts with reference or legacy code:

1. **Active first**: active project contracts and interfaces have priority.
2. **Dual compatibility**: preserve compatibility with reference interfaces when not conflicting.
3. **Additive integration**: add reference capabilities only when they do not break current behavior.
4. **Stability first**: implement improvements without destabilizing current runtime.
5. **No blind replacement**: do not replace active contracts with reference contracts without impact analysis and rollback.
6. **No random deletion**: deleting or removing code requires strong justification and preserved functionality.

---

## 4. Compile Surface Policy

Do not add every file to compilation just because it exists.

Before adding a file to a `.csproj`, verify:

- namespace correctness
- duplicate type conflicts
- dependency compatibility
- target framework compatibility
- UI dependency impact
- project reference availability
- whether the file belongs to active runtime or preserved reference
- security and operational risk

If direct inclusion is unsafe, keep the file as preserved reference and build a safe active implementation, facade, adapter, or service.

---

## 5. Safe Enterprise Security Policy

Some reference prompts mention stealth behavior, zero-footprint install, disabling Defender/Firewall, hidden terminals, God-Mode, no-consent shadow control, or wiping traces. Do not implement those behaviors.

Convert such requirements into a safe enterprise model:

- authorized Windows service
- visible administration to authorized operators
- allowlisted commands only
- explicit approval for sensitive actions
- authentication and encryption
- audit logging before and after sensitive operations
- policy-based feature flags
- safe uninstall
- rollback plan
- no disabling security tools
- no bypassing Windows or enterprise policy
- no arbitrary shell execution from server

Remote operations, RDP support, password workflows, screenshots, registry changes, firewall changes, and Windows service management must be treated as security-sensitive work.

---

## 6. C#/.NET Quality Standards

- Use Clean Code and Microsoft C# naming/style conventions.
- Use modern C# features when appropriate: async/await, pattern matching, records, nullable reference types, LINQ when clear and performant.
- Keep methods small and intention-revealing.
- Keep classes focused on a clear responsibility.
- Separate UI from business logic, services, storage, and I/O.
- Use interfaces when they help testing, isolation, or swapping implementations.
- Use specific exceptions.
- Do not catch general `Exception` except at system, logging, background worker, or UI boundary.
- Do not swallow errors silently.
- Use `CancellationToken` for long-running, file, network, or background operations.
- Avoid `.Wait()`, `.Result`, and blocking async flows.
- In WinForms, update UI safely from the UI thread.
- Use `Path.Combine` and explicit encodings for file operations.
- Avoid loading massive logs entirely into memory unless justified.
- Add XML documentation for public APIs and non-obvious architectural decisions.

---

## 7. EJLive Architecture Priorities

The target architecture must support:

- ATM client/agent
- central server/host
- NOC monitoring/dashboard
- persistent journal sync
- durable outbox queue
- delivery confirmation
- checksum/idempotency
- offset tracking for NCR and daily file tracking for GRG/Wincor
- archive indexing
- role-based access
- audit logging
- vendor-aware log parsing
- XFS/NDC event mapping
- dashboard alerts and reports
- safe remote support workflows
- image/file synchronization
- installer wiring and validation

---

## 8. Client Scope

The client/agent should be lightweight and operationally safe.

Preferred client responsibilities:

- run as an authorized Windows service or controlled agent
- handshake with server
- heartbeat/pulse
- reconnect with backoff
- monitor approved journal/log paths
- maintain local outbox queue
- transfer logs/files with delivery confirmation
- track file offsets and checksums
- receive approved image/file packages
- execute allowlisted commands only
- record local audit logs
- send health, reboot, error, and sync status events
- avoid heavy analytics or dashboard rendering

Client should not host heavy analytical dashboards. Analytics, summaries, tables, and visualization should move to server/dashboard.

---

## 9. Server and Dashboard Scope

The server/dashboard should centralize:

- ATM inventory
- connection state
- journal sync timeline
- archive indexing
- file delivery confirmations
- alerts
- fault analytics
- dashboard cards
- NOC views
- reports and exports
- command queue and approvals
- screenshot workflow if policy-enabled
- image distribution
- time sync workflow
- audit log review
- role-based user actions
- vendor/XFS diagnostics

---

## 10. Sync and State Rules

For journal and file sync:

- Use persistent sync state, not memory-only state.
- Use a durable client outbox queue.
- Track `SyncId`, `ATM_ID`, `FileName`, `Offset`, `Checksum`, `State`, and `RetryCount`.
- Apply idempotency using `ATM_ID + FileName + Offset + Checksum + SyncId`.
- Prefer chunked/length-prefixed transfer for large files.
- Support `START_FILE`, `CHUNK`, `CHUNK_ACK`, `COMPLETE`, `VERIFY`.
- Track NCR fixed files by offset.
- Track GRG/Wincor daily files by file identity/date/checksum.
- Avoid locking live ATM journal files; use compatible file sharing.
- Resume after network/power interruption.

---

## 11. UI/UX and Design System Rules

The UI must be clear, stable, readable, and suitable for NOC operation.

Apply:

- dense but readable NOC dashboard layout
- consistent cards
- clear visual hierarchy
- no overlapping buttons
- consistent spacing
- unified typography
- high-contrast dark mode for 24/7 monitoring
- status colors:
  - green: online and sending data
  - yellow: connected but no recent journal data
  - red: disconnected or critical
  - gray: stale/offline beyond threshold
  - orange: supervisor/maintenance
- server-side analytics and reports
- Journal Sync as timeline:
  Source Read → Local Backup → Sent → Server Stored → Verified
- ATM detail view tabs:
  Overview, EJ Log, Faults, Sync, Commands, Audit
- move heavy client journal analysis to server/dashboard
- merge remote control and service management UI when they affect Windows/service operations
- every button must map to a real service action or be marked as not implemented

---

## 12. Skill Routing

Use skills from `.codex/skills`:

- `atm-systems-analyst`: ATM/EJLive architecture, workflows, operational behavior.
- `ejlive-gap-mapper`: gaps between reference docs, active code, and next implementation slice.
- `system-gap-auditor`: broad code/reference/build audit.
- `solution-architect-reviewer`: architecture and dependency risk.
- `integration-contract-analyzer`: DTOs, sync models, commands, message flows.
- `realtime-sync-device-state`: sync, outbox, ack, queue, retry, conflict handling.
- `winops-remote-control`: safe authorized Windows operations and remote support only.
- `ui-reporting-data-visualization`: UI, dashboards, reports, NOC design.
- `vendor-log-parser`: NCR/GRG/Diebold/Wincor logs.
- `xfs-event-mapper`: XFS/NDC/CEN event interpretation.
- `compatibility-testing-codecraft`: safe C# implementation and testing.
- `legacy-dotnet-modernizer`: gradual .NET/WinForms modernization.
- `code-refactor-planner`: refactoring plans.
- `ejlive-module-implementer`: build confirmed modules after analysis.
- `multi-pdf-project-extractor`: compare multiple reference documents.
- `data-storyteller`: numeric analysis and management summaries.
- `skill-creator`: create or update skills only.

---

## 13. Required Output After Work

Always report:

1. files inspected
2. files changed
3. files classified but not changed
4. legacy/reference features integrated
5. legacy/reference features preserved
6. risks
7. security considerations
8. build/test status
9. unverified items
10. rollback plan
11. next recommended step

---

## 14. Mandatory pre-work

Before any implementation:

1. Read active solution/project files.
2. Inspect `src` as active implementation authority.
3. Inspect `legacy/original` as source material for preserved capabilities.
4. Read project documentation and merge reports.
5. Map layers:
   - Core
   - Shared
   - Client/Agent
   - Server
   - Monitoring/Dashboard
   - Installer
   - Setup
   - Models
   - Services
   - Engines
   - UI Forms
   - Storage/Database
6. Classify files:
   - active compiled code
   - reference-only
   - legacy/original
   - unlinked
   - broken
   - safe to promote
   - unsafe to compile directly
7. Identify duplicate types, drifted contracts, missing definitions, invalid project XML, package/reference problems, and UI actions not wired to services.
8. Produce a short plan before broad changes.

## 15. Conflict policy

When active code conflicts with reference/legacy code:

- Active first.
- Preserve dual compatibility when safe.
- Add reference interfaces only when they do not break active runtime.
- Do not replace active contracts blindly.
- No random deletion.
- Keep project structure stable unless justified.

## 16. Compile surface policy

Do not add every file to compilation simply because it exists.

Before adding a file to a `.csproj`, verify namespace, duplicates, dependencies, target framework, UI impact, project references, runtime role, and security risk.

If direct inclusion is unsafe, keep it as preserved reference and build a safe active implementation/facade/adapter.

## 17. Safety and security policy

Do not implement:

- stealth installation
- zero-footprint cleanup
- disabling Windows Defender
- disabling firewall/security tools
- log wiping
- unauthorized shadow access
- arbitrary hidden terminal from server
- forced password/RDP changes without authorization

Convert unsafe requests to enterprise-safe implementation:

- authorized Windows service
- visible administration to approved operators
- allowlisted commands
- explicit approvals
- encryption and authentication
- audit logging before/after sensitive actions
- role-based access
- feature flags/policies
- rollback and uninstall

## 18. C#/.NET standards

- Clean Code
- Microsoft C# conventions
- async/await for I/O and network flows
- CancellationToken for long-running tasks
- specific exceptions
- no silent error swallowing
- no `.Wait()` or `.Result()` in async paths
- safe WinForms UI-thread updates
- Path.Combine and explicit encodings
- XML documentation for public or non-obvious APIs
- small methods and focused classes

## 20. EJLive implementation priorities

1. Stabilize build graph and project references.
2. Resolve constants/contracts drift.
3. Persistent sync state.
4. Durable client outbox queue.
5. Idempotency and checksum verification.
6. NCR offset tracking and GRG/Wincor daily file tracking.
7. Chunked transfer protocol with ACKs.
8. Server archive indexing.
9. Role-based access and audit logging.
10. Safe command queue.
11. Server-side analytics/dashboard/reporting.
12. NOC UI redesign and status colors.
13. Vendor/XFS diagnostics.
14. Installer wiring and deployment validation.
15. Tests/build/regression plan.

## 21. Skill routing

Use `.codex/skills` actively. Select the most specific skill for the task. For complex tasks, chain skills in this order:

1. `atm-systems-analyst`
2. `system-gap-auditor`
3. `ejlive-gap-mapper`
4. `solution-architect-reviewer`
5. task-specific skill
6. `compatibility-testing-codecraft`
7. `test-strategy-regression-guard`
8. `risk-rollback-planner`

## 22. Output requirement

After every task, report:

- inspected files
- changed files
- unchanged classified files
- integrated legacy/reference features
- preserved features
- security considerations
- build/test status
- unverified assumptions
- rollback plan
- next recommended step

# AGENTS.md Memory Appendix

## Codex Memory Layer

Use `.codex/memory` as the project-local memory layer.

Before major work, read:

1. `.codex/memory/PROJECT_MEMORY.md`
2. `.codex/memory/ACTIVE_CONTEXT.md`
3. `.codex/memory/ARCHITECTURE_MEMORY.md`
4. `.codex/memory/KNOWN_GAPS.md`
5. `.codex/memory/SECURITY_MEMORY.md`
6. `.codex/memory/SYNC_MEMORY.md` when task touches sync/journal transfer
7. `.codex/memory/UI_UX_MEMORY.md` when task touches UI/dashboard
8. `.codex/memory/DECISIONS.md` when task affects architecture

After major work, update the relevant memory file using `.codex/memory/MEMORY_UPDATE_PROTOCOL.md`.

Do not store secrets or sensitive ATM/customer data in memory files.
