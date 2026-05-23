# EJLive Enterprise Ultimate v4.0.0

## Quick Start - Build & Run

### Prerequisites
- Windows 7 SP1 or later
- .NET Framework 4.8+ installed
- Visual Studio 2019+ (Community Edition is fine)

### Step 1: Restore NuGet Packages ⚠️ CRITICAL!

**This is the most important step!**

**Option A (Easiest - 1 minute):**
```
1. Open EJLive.sln in Visual Studio
2. Right-click "Solution 'EJLive'" → "Restore NuGet Packages"
3. Wait for: "Restore completed" message
```

**Option B (Package Manager Console):**
```
1. Tools → NuGet Package Manager → Package Manager Console
2. Run: Install-Package System.Data.SQLite.Core -Version 1.0.118.0
3. Wait for: "Successfully installed"
```

### Step 2: Build Solution

```
Press: Ctrl + Shift + B
```

Expected output:
```
========== Build: 7 succeeded, 0 failed, 0 skipped ==========
```

### Step 3: Run the Application

Choose one to start:

**Server (Central Archive Manager):**
- Right-click `EJLive.Server.WinForms` → Set as StartUp Project
- Press F5 to run

**Client (ATM Client):**
- Right-click `EJLive.Client.WinForms` → Set as StartUp Project
- Press F5 to run

**Monitoring Dashboard:**
- Right-click `EJLive.Monitoring.WinForms` → Set as StartUp Project
- Press F5 to run

---

## Project Structure

```
EJLive_Enterprise_Ultimate/
├── EJLive.Shared/              ← Base library (Logger, Constants)
├── EJLive.Core/                ← Core engines (11 total)
│   ├── Engine/
│   │   ├── ServerEngine.cs        (Multi-client TCP server)
│   │   ├── NetworkEngine.cs       (Protocol & encryption)
│   │   ├── GhostRemoteEngine.cs   (Ghost View remote screen)
│   │   ├── FileWatcherEngine.cs   (Journal sync tracker)
│   │   ├── JournalSyncTracker.cs  (Sync state manager)
│   │   ├── JournalOutbox.cs       (Failed journal queue)
│   │   ├── ImageSyncEngine.cs     (Screenshot capture)
│   │   ├── TransactionAnalysisEngine.cs  (Analytics)
│   │   ├── ReportExportEngine.cs  (CSV/HTML export)
│   │   └── CommunicationProtocol.cs (Wire format)
│   └── Services/
│       ├── DatabaseManager.cs     (SQLite 4-table schema)
│       ├── AlertManager.cs        (Alert queue)
│       ├── AuditLogger.cs         (Immutable audit log)
│       └── RoleBasedAccess.cs     (User permissions)
│
├── EJLive.Client.WinForms/     ← ATM Client UI
├── EJLive.Server.WinForms/     ← Central Server UI (6 tabs)
├── EJLive.Monitor/             ← Background monitoring service
├── EJLive.Monitoring.WinForms/ ← NOC Dashboard UI
└── EJLive.Installer.WinForms/  ← Setup wizard
```

---

## Features

✅ **11 Engines**
- Multi-client TCP server with session management
- AES-256 encryption + RSA-2048 handshake
- Ghost View (non-invasive remote screen capture)
- Journal file synchronization with idempotency
- Automatic retry & failover to local queue
- Real-time image synchronization
- Transaction analysis & reporting
- Report export (CSV/HTML)

✅ **6 User Interfaces**
1. NOC Dashboard - Real-time ATM status
2. Connections - Active client sessions
3. Archive - Journal search & export
4. Remote Control - Ghost View + commands
5. Analytics - Transaction analysis
6. Log - System event logging

✅ **Security**
- AES-256-CBC symmetric encryption
- RSA-2048 key exchange
- PBKDF2 key derivation
- MD5 + SHA-256 checksums
- Role-based access control (Admin/User/Monitor)
- Audit log (immutable)

✅ **Database**
- SQLite with WAL mode
- 4-table schema: ATMStatus, Transactions, JournalQueue, AuditLog
- Monthly partitioning for large deployments
- Automatic cleanup of old records

---

## Troubleshooting

### Error: "System.Data.SQLite could not be found" (CS0006)

**Solution:**
- Restore NuGet packages (see Step 1 above)
- Ensure "Allow NuGet to download missing packages" is enabled in Tools → Options

### Error: Build takes too long or hangs

**Solution:**
- Close Visual Studio
- Delete: `packages` folder, `bin` folder, `obj` folders
- Reopen solution
- Restore NuGet packages again

### Error: "The event 'XXX' is never used" (CS0067)

**Solution:**
- This is a warning treated as error
- Suppress: Project Properties → Build → Suppress Warnings → add CS0067
- Or use: `#pragma warning disable CS0067` in code

### Port already in use (5005)

**Solution:**
- Change port in ServerMainForm.cs: `txtPort.Text = "5006";`
- Or close other applications using port 5005

---

## Deploy to Production

1. **Build Release:**
   - Build → Configuration Manager → Select "Release"
   - Ctrl + Shift + B

2. **Package executables:**
   - `bin\Release\EJLive.Server.WinForms.exe`
   - `bin\Release\EJLive.Client.WinForms.exe`
   - `bin\Release\EJLive.Monitoring.WinForms.exe`
   - Include System.Data.SQLite.dll from packages folder

3. **Deploy:**
   - Copy executables to ATMs and servers
   - Create `C:\EJLive_Storage` directory
   - Run installer on each ATM

---

## Support

All 63 source files are included with full architecture:
- ✅ 11 working engines
- ✅ Complete UI (6 forms with all controls)
- ✅ Database schema (SQLite)
- ✅ Security implementation (AES + RSA)
- ✅ Logging & auditing
- ✅ Event handlers (all wired)

Ready to run after NuGet restoration! 🚀

---

**Version:** 4.0.0  
**Framework:** .NET Framework 4.8  
**Language:** C# 8.0  
**Database:** SQLite 1.0.118  
**Date:** May 3, 2026
