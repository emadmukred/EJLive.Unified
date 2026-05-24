# EJLive Codex Execution Prompts — Commands Matching Each GitHub Issue

هذا الملف يحوّل كل GitHub Issue إلى أمر جاهز للنسخ داخل Codex Desktop/CLI.

## طريقة الاستخدام الصارمة

1. نفّذ أمرًا واحدًا فقط في كل جلسة Codex.
2. افتح Branch منفصل لكل Track.
3. لا تنفذ Track جديد قبل إنهاء السابق أو حفظه في Branch منفصل.
4. لا تسمح لـ Codex بخلط أكثر من Issue في نفس التعديل.
5. بعد كل Build يجب أن يكون `git status` خاليًا من `bin/`, `obj/`, `.vs/`.
6. لا تعمل Merge إلى `main` إلا بعد مراجعة Pull Request ونجاح restore/build/test/verification.

## أوامر التحقق القياسية

```powershell
$env:EJLIVE_DATABASE_PATH = "$env:TEMP\ejlive_codex_verify.db"
dotnet restore .\EJLive.Unified.sln
dotnet build .\EJLive.Unified.sln --no-restore -m:1 /p:BuildInParallel=false -v:m
dotnet test .\src\EJLive.Tests\EJLive.Tests.csproj --no-restore -m:1 /p:BuildInParallel=false --logger "console;verbosity=minimal"
dotnet run --project .\src\EJLive.Verification\EJLive.Verification.csproj --no-restore -m:1 /p:BuildInParallel=false
dotnet build .\EJLive.Unified.slnx --no-restore -m:1 /p:BuildInParallel=false -v:m
```

## CODEX COMMAND — [Track 005B] Unified Client UI, Service Binding, Vendor Paths, and Image Sync

**Suggested branch:** `track-005b-unified-client-ui-service-binding-vendor-paths-and-image-syn`  
**GitHub Issue:** `#8`

انسخ النص التالي كاملًا إلى Codex:

```text
You are Codex working inside the EJLive.Unified repository.

Execute exactly this issue only:
[Track 005B] Unified Client UI, Service Binding, Vendor Paths, and Image Sync
GitHub Issue: #8

Mandatory pre-checks:
1. Read AGENTS.md if present.
2. Read MASTER_COMMAND_INDEX.md if present.
3. Read .codex/instructions/EJLive_Strict_Project_Rules.md if present.
4. Run: git branch --show-current
5. Run: git status --short
6. If the working tree is not clean, STOP and report the dirty files. Do not continue.
7. Use one isolated branch only. Suggested branch name: track-005b-unified-client-ui-service-binding-vendor-paths-and-image-syn

Strict execution rules:
- Do not move to any other Track.
- Do not implement unrelated features.
- Do not rewrite the whole project.
- Do not delete working legacy/reference code unless the issue explicitly requires it and a replacement/rollback is documented.
- Do not commit build outputs: bin/, obj/, .vs/, *.dll, *.exe, *.pdb, *.cache, *.deps.json, *.runtimeconfig.json.
- Do not add stealth behavior.
- Do not disable Defender, firewall, EDR, logging, audit, or security tools.
- Do not add arbitrary shell execution.
- Do not log secrets, passwords, service keys, card numbers, or account numbers.
- If a requested change is unsafe, implement the safe governed equivalent and document the reason.
- If build/test fails, fix only inside the issue scope. If failure is unrelated, document it and stop.

Issue specification:
---
[Track 005B] Unified Client UI, Service Binding, Vendor Paths, and Image Sync
Objective
Build one complete EJLive Client Companion UI that preserves all current/reference client screens and binds them to the client runtime through safe facades, snapshots, and service requests.

This task is for the client only. It must not move central analytics, final transaction reports, or fleet dashboards into the client.

Architecture Decision
EJLive.Client.Service
= background runtime and operational execution

EJLive.Client.WinForms
= full companion UI for setup, configuration, diagnostics, local status, sync visibility, journal preview, image/content delivery status, and controlled requests
The UI must exist and must be complete. The runtime must not depend on the UI. If the UI closes or freezes, the service must continue heartbeat, reconnect, file watching, outbox, sync, socket sessions, status publishing, and local logging.

Scope
Allowed:

src/EJLive.Client.WinForms/**
src/EJLive.Client.Service/**
src/EJLive.Core/** for client-facing DTOs, snapshots, facades, vendor path registry, and local service APIs
src/EJLive.Shared/** for config/logging helpers used by the client
src/EJLive.Tests/**
src/EJLive.Verification/**
docs/**
Forbidden:

Do not remove any current/reference client screen without mapping it to the new UI.
Do not place long-running operational loops inside WinForms event handlers.
Do not add central analytics or fleet dashboards to the client.
Do not implement final forensic journal reports in the client UI.
Do not expose secrets or sensitive values in UI/log exports.
Required Tabs
Preserve and unify all current/reference screens into:

Connection
Sync
Journal Viewer
Controlled Requests
Services / Control
Settings
Agent Config
Diagnostics
Logs
Installer / Activation
Image Sync / Content Delivery
Vendor Paths / Mapping
Local Health / Service Snapshot
Required Connection UI
Show and control through a facade/service request:

Server host and port
ATM/Agent/Terminal identifiers
ATM name, branch, region
Vendor/type/model
Network type
Source journal path
Backup path
Connect/disconnect/ping request
Open journal folder
Open log folder
Connection state
Session ID
Bytes sent/received
Health percentage
Last data time
Last heartbeat time
Reconnect state
Connection log panel
Local health matrix
Required Sync UI
Show and control:

Force send request
Clear failed
Pause/resume sync
Read source
Local backup
Queue/send/verify server
Progress percentage
Pending/syncing/completed/failed counts
Total size and success rate
File grid: item id, file, size, status, progress, retries, bytes, checksum, added time, last attempt, failure reason
Required Journal Viewer UI
Provide lightweight local preview only:

Date range filter
Status/type filter
Load/export
Raw journal preview
Search
Basic transaction/status count
Last lines preview
Open source file/folder
File size and last write time
Server-side remains responsible for final parsing, reports, correlation, and analytics.

Required Controlled Requests UI
Show request controls and request state:

Screen capture request
Stop screen request
Open local screenshots
Restart request
Time sync request
Support access readiness/request
Command/request grid: name, request id, status, queued, sent, completed, result, failure reason
Screen preview area
All sensitive or system-affecting requests must be routed through approved service/server policy and audit. The UI must not perform unmanaged direct execution.

Required Services / Control UI
Show:

Agent Controller
File Watcher
Journal Watcher
Image Inbox Watcher
Socket Data
Socket Files
Screenshot
Support Access readiness
Startup registration
Service installation state
Start/stop/refresh requests
Run diagnostics
Ensure startup
Activate service
Service status
Each row must show status, last transition, last error, details, and recommended action.

Required Settings UI
Show:

Server/network settings
ATM identity
Vendor/model
Journal path
Backup path
Image inbox path
Image destination path
Auto connect
Auto backup
Durable sync
ACK required
Dedup enabled
Encryption option
Compression option
Heartbeat interval
Reconnect interval
Sensitive values must be masked.

Required Agent Config UI
Show a grid:

Key | Value | Operational Usage | Source | IsSensitive | Validation
Include load/apply/save/open folder/validate/export/import actions.

Required keys include identifiers, vendor/model, network settings, server settings, source path, backup path, image inbox path, image destination path, durable sync, ACK required, dedup enabled, auto connect, auto backup, heartbeat interval, reconnect interval, and scoped configuration values where applicable.

Required Diagnostics UI
Include checks for:

Local service existence
Startup registration
Path permissions
Journal source
Backup path
Image inbox
Image destination
Server reachability
Port connectivity
Handshake readiness
Outbox readiness
Disk space
Local health score
Recommended actions
Required Logs UI
Include:

Live local log view
Auto-scroll
Clear
Save
Export
Filter by component
Filter by severity
Sensitive value masking
Correlation ID display where available
Required Installer / Activation UI
Include:

Install/activate/remove client service actions through approved mechanisms
Ensure startup
Prerequisites check
Service name
Install path
Config path
Health file path
Current version
Rollback option where implemented
Required Image Sync / Content Delivery UI
Show:

Vendor
ATM type/model
Package name
Server source
Client inbox path
ATM destination path
File size
Checksum
Received time
Promoted time
Status
Failure reason
Receipt status
Open inbox
Open destination
Validate destination
Manual promote request
Required flow:

server package -> client staging -> checksum verification -> destination allowlist validation -> vendor-specific destination promotion -> receipt to server -> status in UI
Required Vendor Paths / Mapping
Create or bind:

VendorPathRegistry
IAtmVendorPathProvider
NcrVendorPathProvider
GrgVendorPathProvider
WincorVendorPathProvider
DieboldVendorPathProvider
HyosungVendorPathProvider
GenericVendorPathProvider
Each provider must define journal source paths, backup paths, trace/log paths, image/content inbox path, image/content destination paths, screenshot cache path, supported extensions, rollover behavior, validation rules, write permission needs, and restart requirement notes if applicable.

Do not hard-code one universal path for all vendors. Paths must be vendor-aware, configurable, locally validated, stored in AgentConfig, visible in the UI, and included in the client capability snapshot sent to the server.

Required Snapshot Models
Create or bind:

ClientRuntimeSnapshot
ClientServiceSnapshot
ClientComponentSnapshot
ClientSyncSnapshot
ClientPathSnapshot
ClientCommandSnapshot
ClientImageSyncSnapshot
Snapshots must include connected state, session id, handshake state, heartbeat state, last heartbeat, last sync, queue counts, watcher states, socket state, service state, health score, last error, and recommendations.

Required Internal Flow
ClientMainForm/UI Controls -> ClientCompanionFacade -> IClientServiceGateway -> local service API/queue/in-process fallback -> EJLive.Client.Service -> AgentController -> runtime services -> snapshots -> UI refresh
If IPC is not implemented in this task, add interfaces and an in-process fallback adapter so future IPC can be added without rewriting the UI.

Required File Binding Document
Create:

docs/client-ui-service-binding-map.md
Columns:

File path | Current role | Target role | Bound UI tab | Bound service/component | Keep/Refactor/Deprecate/Reference-only | Reason
Include client-facing files from Client.WinForms, Client.Service, Core client-facing contracts/services, Shared client helpers, Installer client functions, and legacy/reference client UI files if present.

Do not blindly compile legacy/reference files. Promote through adapter and tests only.

Required Verification
Add verification probes for:

EJLive.Client.Service must not reference System.Windows.Forms.
WinForms event handlers must not contain long-running operational loops.
Required UI tabs must be present or documented in UI mapping.
Vendor path registry must include NCR, GRG, Wincor, Diebold, Hyosung, Generic.
Client UI must read snapshots/facade instead of directly running file transfer/parser loops.
Sensitive values must be masked in logs/UI exports.
Required Tests
Add or update tests for:

Client snapshot generation
ClientCompanionFacade request routing
Service gateway fallback
Vendor path provider validation
Image/content path validation
UI tab mapping document existence
Sensitive value masking
Service continuity where testable
Required Build Commands
$env:EJLIVE_DATABASE_PATH = "$env:TEMP\ejlive_codex_verify.db"
dotnet restore .\EJLive.Unified.sln
dotnet build .\EJLive.Unified.sln --no-restore -m:1 /p:BuildInParallel=false -v:m
dotnet test .\src\EJLive.Tests\EJLive.Tests.csproj --no-restore -m:1 /p:BuildInParallel=false --logger "console;verbosity=minimal"
dotnet run --project .\src\EJLive.Verification\EJLive.Verification.csproj --no-restore -m:1 /p:BuildInParallel=false
dotnet build .\EJLive.Unified.slnx --no-restore -m:1 /p:BuildInParallel=false -v:m
Acceptance Criteria
All submitted/reference client UI capabilities are preserved, mapped, and represented.
Unified Client Companion UI contains all required tabs.
UI displays all local client operations and active statuses.
UI controls are bound to facade/service requests, not direct background loops.
Client service continues without UI.
Vendor-specific journal and image paths are implemented through provider registry.
Image/content delivery status is visible in UI.
Client-facing files are inventoried and mapped.
Build/test/verification pass.
Documentation includes docs/client-ui-service-binding-map.md.
Required Output
Modified files list
Added files list
Removed/deprecated files list if any
UI tab mapping table
Client service binding map summary
Vendor path providers added
Tests added/updated
Commands executed
Verification result
Known limitations
Rollback plan
Execution Order
Do not execute before:

#3 Source Truth and Baseline Gate
#4 Active Compile Map Synchronizer
#5 Client Service Headless Foundation
#6 UI Backend Separation Gate
#7 Client Companion UI Preservation and Safe Service Runtime
This issue is for full client UI/service binding after foundation gates are stable.
---

After implementation:
1. Run the verification commands required by the issue.
2. Run: git status --short
3. Confirm that git status does not include bin/, obj/, .vs/, dll, exe, pdb, or generated cache files.
4. Produce a final report with:
   - Modified files
   - Added files
   - Removed/deprecated files, if any
   - Tests added/updated
   - Commands executed
   - Verification result
   - Known limitations
   - Rollback plan

Do not create or merge a pull request unless explicitly asked in this session.
```

---

