# 🚀 Getting Started with EJLive Enterprise v4.0.0

**Not Using MSBuild? No Problem!** This guide shows you exactly how to build and run the system using Visual Studio directly.

---

## ⚡ Quick Start (3 Steps)

### Step 1: Open the Project
```
Double-click: EJLive.sln
↓
Visual Studio opens automatically
```

### Step 2: Build
```
Press: Ctrl + Shift + B
↓
Wait 2-3 minutes (first build only)
↓
✅ Build succeeded
```

### Step 3: Run
```
Use: START_APPLICATIONS.bat
↓
Choose which app to run
↓
Done!
```

---

## 📋 All Helper Files

| File | Purpose |
|------|---------|
| **SIMPLE_BUILD.bat** | Open VS with one click |
| **START_APPLICATIONS.bat** | Menu to run any app |
| **CHECK_ENVIRONMENT.bat** | Verify setup is correct |
| **INSTRUCTIONS_AR.txt** | Arabic instructions |
| **VISUAL_STUDIO_BUILD.md** | Detailed steps |
| **RUN_DIRECTLY.md** | How to run apps |
| **FINAL_BUILD_REPORT.txt** | Complete technical report |

---

## ✅ Prerequisites

**Required:**
- [ ] Visual Studio (Community is free)
  - Download: https://visualstudio.microsoft.com/downloads/
  - Install: Desktop development with C# + .NET Framework 4.8
- [ ] .NET Framework 4.8
  - Download: https://dotnet.microsoft.com/download/dotnet-framework

---

## 🎯 Detailed Steps

### Method 1: GUI (Easiest)
1. Click `SIMPLE_BUILD.bat` → Opens Visual Studio
2. Press `Ctrl+Shift+B` → Builds everything
3. Click `START_APPLICATIONS.bat` → Run any app

### Method 2: Manual
1. Double-click `EJLive.sln`
2. In Visual Studio: `Build → Build Solution`
3. After success, run executables from `bin\Release\`

### Method 3: PowerShell
```powershell
.\BUILD_NO_MSBUILD.ps1 -Configuration Release
```

---

## 🔍 Troubleshooting

### "System.Data.SQLite not found"
```
In Visual Studio:
Tools → NuGet Package Manager → Package Manager Console
→ Install-Package System.Data.SQLite.Core
→ Ctrl+Shift+B (rebuild)
```

### ".NET Framework 4.8 not found"
```
Download and install from:
https://dotnet.microsoft.com/download/dotnet-framework
Then restart Visual Studio
```

### Build fails after changes
```
1. Right-click solution
2. Clean Solution
3. Build Solution
```

---

## 📁 Project Structure

```
EJLive.sln                          ← Open this in Visual Studio
EJLive.Shared/                      ← Base library (Security, Logging)
EJLive.Core/                        ← 11 Engines (Protocol, Sync, etc.)
EJLive.Client.WinForms/             ← Client App (5 tabs)
EJLive.Server.WinForms/             ← Server App (6 tabs)
EJLive.Monitor/                     ← Monitoring Library
EJLive.Monitoring.WinForms/         ← NOC Dashboard
EJLive.Installer.WinForms/          ← Setup Wizard
```

---

## 🎬 Running Applications

After successful build:

### Option 1: Using the Menu
```
Click: START_APPLICATIONS.bat
→ Choose [1-4] from menu
→ App starts
```

### Option 2: Manual
```
EJLive.Client.exe
↳ EJLive.Client.WinForms\bin\Release\

EJLive.Server.exe
↳ EJLive.Server.WinForms\bin\Release\

EJLive.Monitoring.exe
↳ EJLive.Monitoring.WinForms\bin\Release\

EJLive.Installer.exe
↳ EJLive.Installer.WinForms\bin\Release\
```

---

## ✨ Features

✅ **Client App** (5 Tabs)
- Synchronization
- Live Events
- Local Archive
- Settings
- Support

✅ **Server App** (6 Tabs)
- Overview
- ATMs
- Archive
- Reports
- Alerts
- Admin

✅ **Security**
- AES-256 encryption
- RSA-2048 key exchange
- PBKDF2 key derivation
- Immutable audit log

✅ **Database**
- SQLite with WAL mode
- Monthly partitions
- Idempotency checks
- Pre-aggregated stats

---

## 🔗 Links

- **Visual Studio Download**: https://visualstudio.microsoft.com/downloads/
- **.NET Framework 4.8**: https://dotnet.microsoft.com/download/dotnet-framework
- **Project Documentation**: See README.md

---

## ❓ FAQs

**Q: Do I need MSBuild?**
A: No! Visual Studio handles everything automatically.

**Q: Can I use VS Code?**
A: Yes! Install C# DevKit extension and open the folder.

**Q: How long does the first build take?**
A: 2-3 minutes. Subsequent builds are faster.

**Q: Where are the executable files?**
A: In `bin\Release\` folder inside each project.

**Q: Can I run multiple apps at once?**
A: Yes! Run Server on one PC and Client on another.

**Q: What if I still get errors?**
A: Read `VISUAL_STUDIO_BUILD.md` or run `CHECK_ENVIRONMENT.bat`

---

## 🎓 Next Steps

1. ✅ Ensure Visual Studio and .NET 4.8 are installed
2. ✅ Open `EJLive.sln`
3. ✅ Press `Ctrl+Shift+B`
4. ✅ Wait for build to complete
5. ✅ Run applications from `bin\Release\`

**That's it! You're ready to use EJLive Enterprise.** 🚀

---

*Last Updated: May 2026 | Version 4.0.0 | Platform: Windows + .NET Framework 4.8*
