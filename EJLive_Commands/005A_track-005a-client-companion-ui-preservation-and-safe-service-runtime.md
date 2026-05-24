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

## CODEX COMMAND — [Track 005A] Client Companion UI Preservation and Safe Service Runtime

**Suggested branch:** `track-005a-client-companion-ui-preservation-and-safe-service-runtime`  
**GitHub Issue:** `#7`

انسخ النص التالي كاملًا إلى Codex:

```text
You are Codex working inside the EJLive.Unified repository.

Execute exactly this issue only:
[Track 005A] Client Companion UI Preservation and Safe Service Runtime
GitHub Issue: #7

Mandatory pre-checks:
1. Read AGENTS.md if present.
2. Read MASTER_COMMAND_INDEX.md if present.
3. Read .codex/instructions/EJLive_Strict_Project_Rules.md if present.
4. Run: git branch --show-current
5. Run: git status --short
6. If the working tree is not clean, STOP and report the dirty files. Do not continue.
7. Use one isolated branch only. Suggested branch name: track-005a-client-companion-ui-preservation-and-safe-service-runtime

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

