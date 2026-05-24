ISSUE TITLE:
## ISSUE TITLE

[Track 007] Heartbeat and Pulse Reliability

## ISSUE BODY

# [Track 007] Heartbeat and Pulse Reliability

## Objective
إرسال نبضات دورية موثوقة مع Backoff وإظهار الحالة في السيرفر.

## Scope
Allowed:
- Work only inside the target layer and files listed below.
- Add or update tests directly related to this Track.
- Add adapters/facades instead of deleting existing working code.

Forbidden:
- Do not send per-second heartbeat by default unless configured.
- Do not block file transfer on heartbeat failure.

## Target Files / Layers
**Target layer:** Client.Service/Server/Core

**Files / context:**
```text
NetworkEngine, ServerEngine, AgentHeadlessController
```

## Required Actions
Run this as one isolated Codex task. Do not continue to any other Track.

```text
Task: Implement reliable heartbeat/pulse.

Required work:
1. Heartbeat payload must be JSON with ATM_ID, SessionId, AgentState, OutboxCount, LastJournalOffset, CPU, Memory, Disk, WatcherState.
2. Server replies with HeartbeatAck containing ServerTimeUtc and PendingCommandCount.
3. Add heartbeat timeout thresholds: Online, Warning, Offline, CriticalOffline.
4. Add exponential backoff + jitter for reconnect.
5. Prevent overlapping heartbeat sends.
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
- Online/offline state is accurate.
- Reconnect storms are prevented.
- Heartbeat test covers server down/up.

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
# Client UI Full Preservation + Vendor Path Registry + Image Sync

## Objective
Preserve all submitted/reference EJLive Client UI screens and expand them into one unified Client Companion UI, while keeping production execution inside EJLive.Client.Service.

## Mandatory Decision
The client must have a full UI.  
The UI is not an analytics dashboard and not the production runtime.

Production runtime:
- EJLive.Client.Service
- Windows Service / BackgroundService
- Runs background operations: heartbeat, reconnect, file watcher, outbox, sync, command receiver, health reporting, local audit.

Client UI:
- EJLive.Client.WinForms
- Configuration / Administration / Activation / Diagnostics / Local Status Companion.

## Required UI Tabs
Do not remove any current/reference UI screens. Preserve and unify:

1. Connection
2. Sync
3. Journal Viewer
4. Remote Control / Safe Requests
5. Services / Control
6. Settings
7. Agent Config
8. Diagnostics
9. Logs
10. Installer / Activation
11. Image Sync / Content Delivery
12. Vendor Paths / Mapping
13. Local Health / Service Snapshot

## Required Client Status Display
The UI must display the active state of:

- Agent Controller
- File Watcher
- Socket Data
- Socket Files
- Screenshot
- Ghost / Remote Assistance readiness
- Windows Startup
- Handshake
- Pulse / Heartbeat
- Auto reconnect
- Auto ping
- Journal watcher
- Image inbox
- Outbox queue
- Failed queue
- Last sync
- Last heartbeat
- Current session id
- ATM status
- Local logs

## Vendor Path Registry
Create:

```text
VendorPathRegistry
IAtmVendorPathProvider
NcrVendorPathProvider
GrgVendorPathProvider
WincorVendorPathProvider
DieboldVendorPathProvider
HyosungVendorPathProvider
GenericVendorPathProvider

# Client UI Full Preservation + Vendor Path Registry + Image Sync

## Objective
Preserve all submitted/reference EJLive Client UI screens and expand them into one unified Client Companion UI, while keeping production execution inside EJLive.Client.Service.

## Mandatory Decision
The client must have a full UI.  
The UI is not an analytics dashboard and not the production runtime.

Production runtime:
- EJLive.Client.Service
- Windows Service / BackgroundService
- Runs background operations: heartbeat, reconnect, file watcher, outbox, sync, command receiver, health reporting, local audit.

Client UI:
- EJLive.Client.WinForms
- Configuration / Administration / Activation / Diagnostics / Local Status Companion.

## Required UI Tabs
Do not remove any current/reference UI screens. Preserve and unify:

1. Connection
2. Sync
3. Journal Viewer
4. Remote Control / Safe Requests
5. Services / Control
6. Settings
7. Agent Config
8. Diagnostics
9. Logs
10. Installer / Activation
11. Image Sync / Content Delivery
12. Vendor Paths / Mapping
13. Local Health / Service Snapshot

## Required Client Status Display
The UI must display the active state of:

- Agent Controller
- File Watcher
- Socket Data
- Socket Files
- Screenshot
- Ghost / Remote Assistance readiness
- Windows Startup
- Handshake
- Pulse / Heartbeat
- Auto reconnect
- Auto ping
- Journal watcher
- Image inbox
- Outbox queue
- Failed queue
- Last sync
- Last heartbeat
- Current session id
- ATM status
- Local logs

## Vendor Path Registry
Create:

```text
VendorPathRegistry
IAtmVendorPathProvider
NcrVendorPathProvider
GrgVendorPathProvider
WincorVendorPathProvider
DieboldVendorPathProvider
HyosungVendorPathProvider
GenericVendorPathProvider

Each provider must define:

Journal source paths
Journal backup paths
Trace/log paths
Image/content inbox path
Image/content active destination paths
Screenshot cache path
Supported file extensions
Rollover behavior
Validation rules
Write permission requirements
Whether destination replacement requires ATM application restart
Image Sync / Content Delivery Flow
Server sends image/content package
→ Client receives package into staging folder
→ Verify checksum/signature
→ Validate vendor destination allowlist
→ Promote to vendor-specific destination folder
→ Return receipt to server
→ Show local status in Client UI
Required Image Sync UI Fields
Vendor
ATM type
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
Path Rules

Do not hard-code one universal path for all ATMs.

All paths must be:

Vendor-aware
Configurable
Validated locally
Stored in AgentConfig
Visible in Agent Config UI
Sent to server as part of client capability snapshot
Client UI Allowed Functions

Allowed:

Configure ATM identity
Configure server IP/port
Configure vendor/type
Configure journal paths
Configure backup paths
Configure image paths
Configure sync policy
Show local service state
Show outbox/sync status
Show journal preview
Show logs
Show diagnostics
Request force sync
Request screenshot
Request remote assistance
Request password change through governed command flow
Start/stop/restart local service through approved service-control mechanism
Client UI Forbidden Functions

Forbidden inside WinForms event handlers:

Long-running socket loops
File watcher loops
Chunked file transfer loops
Full journal forensic parsing
XFS correlation
Archive pipeline
Direct RDP enablement
Direct firewall changes
Direct registry changes
Direct password changes
Arbitrary shell execution
Blocking calls such as Task.Wait, .Result, or unnecessary Thread.Sleep
Security Rules

The client service must be legitimate and auditable:

No stealth behavior.
No hiding from Windows Services.
No disabling Defender, firewall, EDR, logging, or audit.
No arbitrary shell execution.
No credential logging.
No passwords or service keys in logs.
RDP/Remote Assistance/Firewall/Registry/Password operations must be allowlisted, audited, reversible, and role-controlled.
Windows LAPS awareness should be supported where available.
Remote Credential Guard / restricted admin should be treated as policy-governed helpdesk access.
Required Execution Pattern
UI Button
→ ClientCompanionFacade
→ Local Service API / Named Pipe / Queue / IPC
→ EJLive.Client.Service
→ AgentController
→ Result Snapshot
→ UI Refresh
Acceptance Criteria
All submitted/reference Client UI screens are preserved or mapped.
No Client UI tab is removed without documented replacement.
Client UI includes Connection, Sync, Journal, Remote Control, Services, Settings, Agent Config, Diagnostics, Logs, Installer, Image Sync, Vendor Paths.
Client UI shows active client operations and service state.
Client UI can close without stopping EJLive.Client.Service.
UI freeze does not stop heartbeat, journal sync, file watching, or command receiving.
Image/content paths are vendor-specific and configurable.
Sensitive operations are requests governed by policy, not direct unmanaged button actions.
Final analytics, XFS correlation, fleet reports, and forensic transaction analysis remain server-side.
Required Tests / Verification
Verification fails if EJLive.Client.Service references System.Windows.Forms.
Verification warns if WinForms event handlers contain blocking operational code.
UI smoke checklist verifies all required tabs exist.
Service continuity test verifies background service continues without UI.
Vendor path validation test covers NCR, GRG, Wincor, Diebold, Hyosung, Generic.
Image sync test verifies staging → checksum → allowlist → promote → receipt.