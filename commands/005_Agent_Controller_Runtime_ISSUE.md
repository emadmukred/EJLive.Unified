ISSUE TITLE:
## ISSUE TITLE

[Track 005] Agent Controller Runtime

## ISSUE BODY

# [Track 005] Agent Controller Runtime

## Objective
جعل AgentController قلبًا منظّمًا لمكونات الكلاينت.

## Scope
Allowed:
- Work only inside the target layer and files listed below.
- Add or update tests directly related to this Track.
- Add adapters/facades instead of deleting existing working code.

Forbidden:
- Do not execute dangerous remote commands in AgentHeadlessController.
- Do not parse financial analytics in client UI.

## Target Files / Layers
**Target layer:** Client.Service/Core

**Files / context:**
```text
IAgentController.cs, AgentHeadlessController.cs, Core/Agent services
```

## Required Actions
Run this as one isolated Codex task. Do not continue to any other Track.

```text
Task: Build Agent Controller Runtime.

Required work:
1. Define or confirm `IAgentController` with StartAll, StopAll, GetStatus, ForceJournalSync, ForceLogBackup.
2. Implement state machine: Stopped, Starting, Running, Paused, Failed.
3. Coordinate NetworkSessionManager, HeartbeatService, AdvancedFileWatcher, JournalOutboxAdapter, HealthReporter.
4. Ensure exceptions in observers never crash the agent.
5. Add tests for state transitions and idempotent StopAll.
```

## Required Verification Commands
```powershell
$env:EJLIVE_DATABASE_PATH = "$env:TEMP\ejlive_codex_verify.db"
dotnet restore .\EJLive.Unified.sln
dotnet build .\EJLive.Unified.sln --no-restore -m:1 /p:BuildInParallel=false -v:m
dotnet test .\src\EJLive.Tests\EJLive.Tests.csproj --no-restore -m:1 /p:BuildInParallel=false --logger "console;verbosity=minimal"
dotnet run --project .\src\EJLive.Verification\EJLive.Verification.csproj --no-restore -m:1 /p:BuildInParallel=false
dotnet build .\EJLive.Unified.slnx --no-restore -m:1 /p:BuildInParallel=false -v:m
```

## Acceptance Criteria
- Agent lifecycle is deterministic.
- Status contains state, connection, session, outbox, last heartbeat, last sync, last error.

## Required Tests
- Add or update unit tests when code is changed.
- Add or update verification probes when architecture, compile map, service boundary, parser, XFS, security, database, or transport behavior is changed.
- For parser/XFS work, add fixtures and expected output snapshots.
- For UI/backend separation work, prove backend execution continues without blocking UI.

## Required Codex Output
- Modified files
- Added files
- Deprecated/removed files, if any
- Tests added or updated
- Commands executed
- Verification result
- Known limitations
- Rollback plan

## Pull Request Requirements
- Link this Issue in the PR body.
- Use `.github/pull_request_template.md`.
- Include exact files modified/added.
- Include exact commands executed and their results.
- Include rollback plan.
- Do not merge until restore/build/test/verification pass.

## Clarification: Client UI is required, but only as a configuration/admin companion

The EJLive Client must have a UI, but it must be a lightweight Configuration / Administration / Activation Companion only.

The Client UI is not an analytics dashboard.

Allowed Client UI responsibilities:
- Configure ATM ID.
- Configure ATM vendor/type.
- Configure server host/port.
- Configure journal paths.
- Configure backup paths.
- Configure sync policy.
- Configure security/certificate settings.
- Show local service status only.
- Show connected/disconnected state.
- Show last heartbeat.
- Show pending outbox count.
- Show last sync time.
- Show last local error.
- Start/stop/restart the local Windows Service through approved service-control actions.
- Test connection.
- Validate paths.
- Request force sync through the service API/queue.
- Display local setup logs and diagnostics.

Forbidden Client UI responsibilities:
- No local analytics dashboard.
- No charts or reports.
- No transaction analysis views.
- No XFS analysis views.
- No journal parsing inside WinForms event handlers.
- No direct socket loops inside UI.
- No direct file transfer loops inside UI.
- No direct archive pipeline inside UI.
- No direct remote command execution inside UI.
- No RDP/firewall/registry/password execution from UI buttons.
- No blocking operations on the UI thread.

Required separation:
- `EJLive.Client.Service` is the production runtime.
- `EJLive.Client.WinForms` is only the admin/config companion.
- UI buttons must call async service APIs or enqueue commands only.
- If the UI freezes, crashes, or closes, the Windows Service must continue running.
- Server/dashboard owns analytics, visualization, reports, operator commands, and monitoring.

[Track 005A] Client Companion UI Preservation and Safe Service Runtime
Objective
Preserve and expand all current/reference EJLive Client UI screens as a Client Companion UI for configuration, activation, diagnostics, local service control, sync status, journal preview, and safe command requests, while keeping production execution inside EJLive.Client.Service.

Mandatory Architecture Decision
The client must have a full UI, but the UI is not the production runtime and not a local analytics dashboard.

Production runtime:

EJLive.Client.Service
Windows Service / BackgroundService
Responsible for heartbeat, reconnect, file watching, outbox, sync, command receiving, health reporting, and local audit.
Client UI:

EJLive.Client.WinForms
Configuration / Administration / Activation / Diagnostics Companion only.
It must remain optional for runtime continuity.
Required UI Tabs — Do Not Remove
Preserve and consolidate these screens/tabs without removing their current functions:

Connection
Sync
Journal / Journal Viewer
Remote Control
Services / Control
Settings
Agent Config
Diagnostics
Logs
Installer / Activation
Required UI Capabilities
Connection
Server IP / Host
Server Port
ATM ID / AgentId / TerminalId
ATM Name / TerminalName
ATM Type / Vendor / Model
Network Type / Link
Source Journal Path
Backup Path
Connect / Disconnect / Ping
Open Journal Folder
Open Log Folder
Connection status
Session ID
Bytes sent/received
Health percentage
Last data timestamp
Local health matrix
Connection log panel
Sync
Force Send
Clear Failed
Pause Sync
Resume Sync
Read Source
Local Backup
Queue
Send
Verify Server
Progress percentage
Queue count
Total size
Success rate
File grid with file name, size, status, progress, retries, checksum, added time
Manual send
Delete failed
Temporary pause
Resume
Journal / Journal Viewer
Local journal preview
Date filter
Status filter
Load
Export
Raw viewer
Transaction count
Search
Open source file
Allowed only as lightweight local preview. Final forensic analysis, reporting, XFS correlation, and dashboard analytics remain server-side.

Remote Control / Safe Command Requests
Capture Screen request
Stop Screen request
Open local screenshots
Restart ATM request
Sync Time request
Change Password request
Windows Remote Assistance request
Command queue grid
Command ID
Status
Queued At
Result
Screen preview area
Important: UI buttons must enqueue requests or call approved service APIs only. Sensitive actions require policy, authorization, audit, and rollback in the service/server layer.

Services / Control
Agent Controller
File Watcher
Socket Data
Socket Files
Screenshot
Remote Assistance / Ghost Access
Windows Startup
Start / Stop / Refresh
Run Diagnostics
Ensure Startup
Activate Service
Start Service
Service Status
Settings
Server/network settings
ATM identity
Vendor type
Journal path
Backup path
Auto connect
Auto backup
Encryption options
Compression options
Password change UI with strict policy controls
Agent Config
Key / Value / Operational Usage grid
Load config
Apply config
Save config
Open config folder
Mask sensitive values
Validate configuration
Required config keys include, where applicable:

AgentId
TerminalId
TerminalName
Vendor
Model
NetworkType
ServerHost
ServerPort
SourcePath
BackupPath
DurableSyncEnabled
AckRequired
DedupEnabled
AutoConnect
HeartbeatIntervalSec
ReconnectIntervalSec
AllowedPasswordAccounts
ScopedFirewallRemoteAddresses
Strict Implementation Rules
Forbidden inside WinForms event handlers:

Long-running socket loops
File watcher loops
Chunked transfer loops
Full journal parsing
Archive pipeline
XFS correlation
Direct sensitive system changes
Blocking calls such as Task.Wait, .Result, or unnecessary Thread.Sleep
Required flow:

UI Button
→ ClientCompanionFacade
→ Local Service API / Named Pipe / Queue / IPC
→ EJLive.Client.Service
→ AgentController
→ Result Snapshot
→ UI Refresh
Security and Governance Rules
Do not implement stealth behavior.
Do not hide from the OS or operator.
Do not disable security tools, firewall, Defender, EDR, logging, or audit.
Do not execute arbitrary shell commands from UI.
Do not log secrets, passwords, card numbers, account numbers, or service keys.
RDP/Remote Assistance/Firewall/Registry/Password operations must be explicit, allowlisted, audited, reversible, and role-controlled.
Prefer enterprise policy integration and Windows LAPS where available for local administrator password governance.
Acceptance Criteria
All current/reference Client UI tabs are preserved or mapped to the unified Client Companion UI.
No tab is removed without documented replacement.
Client UI can be closed without stopping EJLive.Client.Service.
Client UI freeze does not stop heartbeat, journal sync, file watching, or command receiving.
Client UI contains configuration, diagnostics, service control, logs, sync status, and journal preview.
Client UI does not contain local analytics dashboard or final forensic reporting.
Sensitive controls create policy-controlled requests and never bypass service/server governance.
Required Tests / Verification
Add verification probe to detect blocking operational code inside WinForms event handlers.
Add test or verification rule that EJLive.Client.Service does not reference System.Windows.Forms.
Add UI smoke test or manual checklist for all required tabs.
Add service continuity test: UI closed while service continues heartbeat/outbox/watch state.
Required Output
Modified files list
Added files list
UI tab mapping table
Removed/deprecated UI elements, if any, with replacement path
Commands executed
Build/test/verification result
Known limitations
Rollback plan

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
