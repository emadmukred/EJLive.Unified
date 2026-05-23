# EJLive Codex Strict Command Pack v2

هذه الحزمة تجعل كل أمر مستقلًا للتنفيذ الفردي في GitHub Issue أو Codex Desktop.

## أين توضع الأوامر؟
# أين نضع أوامر Codex؟

## القاعدة المختصرة
- ضع القواعد الدائمة في المستودع نفسه: `AGENTS.md` و`.codex/instructions/`.
- ضع كل مهمة تنفيذية منفصلة كـ GitHub Issue مستقل عندما تريد Pull Request قابل للمراجعة.
- استخدم Codex Desktop/CLI للمهام المحلية السريعة، التجريب، مقارنة الملفات، إصلاح Build، أو العمل على Branch محلي.

## توزيع الاستخدام

### 1) داخل GitHub
استخدم GitHub Issues لكل أمر منفصل عندما يكون المطلوب:
- تعديل فعلي في Repository.
- إنشاء Pull Request.
- تشغيل CI أو مراجعة Build/Test.
- تتبع كل مهمة كـ Track منفصل.

ضع الأمر كاملًا في Issue، ثم اجعل Codex يعمل على Issue واحد فقط.

### 2) داخل Codex Desktop
استخدم Codex Desktop عندما يكون المطلوب:
- العمل على نسخة محلية من المشروع.
- تنفيذ Build/Test محليًا.
- فحص ملفات أو مقارنة Branches.
- تجربة refactor قبل رفع Pull Request.

الصق أمرًا واحدًا فقط من مجلد `commands/` في الجلسة.

### 3) داخل المستودع كتعليمات دائمة
ضع هذا الملف في الجذر:
- `AGENTS.md`

وضع هذا الملف كمرجع صارم:
- `.codex/instructions/EJLive_Strict_Project_Rules.md`

ولا تجعل Codex يبدأ من الذاكرة فقط. يجب أن يقرأ قواعد المشروع قبل أي تعديل.

## External references
- OpenAI Codex: https://openai.com/codex/ — Codex as an AI coding agent for features, refactors, migrations, pull requests, multi-agent workflows, app/editor/terminal usage.
- Microsoft .NET Windows Service with BackgroundService: https://learn.microsoft.com/en-us/dotnet/core/extensions/windows-service — Use BackgroundService/Worker Service for long-running Windows Service work.
- Microsoft .NET Worker Services: https://learn.microsoft.com/en-us/dotnet/core/extensions/workers — General Worker Service guidance.
- Microsoft FileSystemWatcher: https://learn.microsoft.com/en-us/dotnet/api/system.io.filesystemwatcher — Watch filesystem changes, combined with polling/offset in ATM live journals.
- CEN/CENELEC CEN/XFS 3.51: https://www.cencenelec.eu/areas-of-work/xfs_cwa16926_351_release/ — Multi-vendor financial peripheral interface; basis for XFS normalization.
- Microsoft Windows LAPS: https://learn.microsoft.com/en-us/windows-server/identity/laps/laps-overview — Use enterprise local admin password management instead of plaintext remote passwords.
- Microsoft Remote Desktop access: https://learn.microsoft.com/en-us/windows-server/remote/remote-desktop-services/remotepc/remote-desktop-allow-access — Remote Desktop is sensitive administrative access and must be governed.

## Command Index
- [001 — Source Truth and Baseline Gate](commands/001_Source_Truth_and_Baseline_Gate.md) — حرجة — Solution/Verification
- [002 — Active Compile Map Synchronizer](commands/002_Active_Compile_Map_Synchronizer.md) — حرجة — Verification/Core
- [003 — Client Service Headless Foundation](commands/003_Client_Service_Headless_Foundation.md) — حرجة — Client.Service
- [004 — UI Backend Separation Gate](commands/004_UI_Backend_Separation_Gate.md) — حرجة — Client.WinForms/Server.WinForms/Core
- [005 — Agent Controller Runtime](commands/005_Agent_Controller_Runtime.md) — عالية — Client.Service/Core
- [006 — Secure Handshake Service](commands/006_Secure_Handshake_Service.md) — حرجة — Core/Network
- [007 — Heartbeat and Pulse Reliability](commands/007_Heartbeat_and_Pulse_Reliability.md) — عالية — Client.Service/Server/Core
- [008 — Advanced File Watcher and Safe Live File Reader](commands/008_Advanced_File_Watcher_and_Safe_Live_File_Reader.md) — حرجة — Client.Service/Core/File
- [009 — Journal Outbox Compatibility Adapter](commands/009_Journal_Outbox_Compatibility_Adapter.md) — عالية — Client.Service/Core/Sync
- [010 — Reliable Chunked Transfer Session](commands/010_Reliable_Chunked_Transfer_Session.md) — حرجة — Core/Network/Server
- [011 — Server Ingestion Pipeline](commands/011_Server_Ingestion_Pipeline.md) — حرجة — Server/Core/Data
- [012 — EJ Parser Registry and Contracts](commands/012_EJ_Parser_Registry_and_Contracts.md) — حرجة — Core/Parsing
- [013 — NCR EJ Transaction Parser](commands/013_NCR_EJ_Transaction_Parser.md) — حرجة — Core/Parsing/NCR
- [014 — GRG EJ/TRACE Parser](commands/014_GRG_EJ_TRACE_Parser.md) — عالية — Core/Parsing/GRG
- [015 — Wincor/ProView EJ Parser Skeleton](commands/015_Wincor_ProView_EJ_Parser_Skeleton.md) — عالية — Core/Parsing/Wincor
- [016 — Diebold/Agilis EJ Parser Skeleton](commands/016_Diebold_Agilis_EJ_Parser_Skeleton.md) — عالية — Core/Parsing/Diebold
- [017 — Hyosung EJ Parser Skeleton](commands/017_Hyosung_EJ_Parser_Skeleton.md) — متوسطة — Core/Parsing/Hyosung
- [018 — Cash Distribution and Counters Parser](commands/018_Cash_Distribution_and_Counters_Parser.md) — عالية — Core/Parsing/Cash
- [019 — XFS Normalization Contracts](commands/019_XFS_Normalization_Contracts.md) — حرجة — Core/XFS
- [020 — NCR XFS Adapter](commands/020_NCR_XFS_Adapter.md) — عالية — Core/XFS
- [021 — GRG XFS Adapter](commands/021_GRG_XFS_Adapter.md) — عالية — Core/XFS
- [022 — Wincor XFS Adapter](commands/022_Wincor_XFS_Adapter.md) — عالية — Core/XFS
- [023 — Diebold XFS Adapter](commands/023_Diebold_XFS_Adapter.md) — عالية — Core/XFS
- [024 — Hyosung XFS Adapter](commands/024_Hyosung_XFS_Adapter.md) — عالية — Core/XFS
- [025 — EJ XFS TRACE Correlation Engine](commands/025_EJ_XFS_TRACE_Correlation_Engine.md) — حرجة — Core/Correlation
- [026 — Safe Remote Command Queue](commands/026_Safe_Remote_Command_Queue.md) — حرجة — Server/Core/Security
- [027 — Windows Policy Enforcer Audit Mode](commands/027_Windows_Policy_Enforcer_Audit_Mode.md) — حرجة — Client/Core/Security
- [028 — RDP Shadow Remote Assistance Governance](commands/028_RDP_Shadow_Remote_Assistance_Governance.md) — عالية — Client/Server/Security
- [029 — File Image Distribution Service](commands/029_File_Image_Distribution_Service.md) — عالية — Server/Client/Core
- [030 — Screenshot Capture Delivery](commands/030_Screenshot_Capture_Delivery.md) — متوسطة — Client/Server/Core
- [031 — Server Dashboard Snapshot Model](commands/031_Server_Dashboard_Snapshot_Model.md) — عالية — Server/Monitoring/Core
- [032 — ATM Vendor Strategy Registry](commands/032_ATM_Vendor_Strategy_Registry.md) — عالية — Core/Vendors
- [033 — Database Migrations Runner](commands/033_Database_Migrations_Runner.md) — حرجة — Core/Data
- [034 — Structured Logging and Correlation IDs](commands/034_Structured_Logging_and_Correlation_IDs.md) — عالية — Shared/Core/Server/Client
- [035 — Security Hardening and Secrets](commands/035_Security_Hardening_and_Secrets.md) — حرجة — Core/Security
- [036 — Local ATM Health Snapshot](commands/036_Local_ATM_Health_Snapshot.md) — عالية — Client.Service/Core
- [037 — Outbox Maintenance and Quota](commands/037_Outbox_Maintenance_and_Quota.md) — متوسطة — Client.Service/Core/Sync
- [038 — Server Command Retry and Delivery Tracking](commands/038_Server_Command_Retry_and_Delivery_Tracking.md) — عالية — Server/Core
- [039 — ATM Journal Request Service](commands/039_ATM_Journal_Request_Service.md) — عالية — Server/Client/Core
- [040 — Time Sync Service](commands/040_Time_Sync_Service.md) — متوسطة — Client/Server/Core
- [041 — Installer Deployment Rollback](commands/041_Installer_Deployment_Rollback.md) — عالية — Installer/Client.Service/Server
- [042 — Reference Promotion Protocol](commands/042_Reference_Promotion_Protocol.md) — حرجة — Docs/Verification
- [043 — NOC Dashboard UI Upgrade After Snapshot](commands/043_NOC_Dashboard_UI_Upgrade_After_Snapshot.md) — متوسطة — Monitoring/Server UI
- [044 — Regression Verification Gate](commands/044_Regression_Verification_Gate.md) — حرجة — Tests/Verification
- [045 — Final Packaging and Release Readiness](commands/045_Final_Packaging_and_Release_Readiness.md) — عالية — Build/Release