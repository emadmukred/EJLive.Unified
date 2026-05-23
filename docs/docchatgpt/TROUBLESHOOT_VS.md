# ✅ حل مشاكل Visual Studio — EJLive Enterprise

## 🎯 الخطوات السريعة الأولى

### 1️⃣ إغلق Visual Studio تماماً
```
- أغلق جميع نوافذ VS
- انتظر 5 ثوانٍ
```

### 2️⃣ احذف ملفات الـ Cache
```
اضغط: Windows + R
اكتب: %LOCALAPPDATA%\Microsoft\VisualStudio
احذف جميع مجلدات: 17.0_* أو 16.0_*
```

### 3️⃣ افتح المشروع من جديد
```
Double-click: EJLive.sln
انتظر التحميل (قد يأخذ وقت)
```

### 4️⃣ استعيد الحزم
```
في Visual Studio:
- Project → Restore NuGet Packages
أو
- Tools → NuGet Package Manager → Package Manager Console
- اكتب: Update-Package -Reinstall
```

### 5️⃣ بناء المشروع
```
Ctrl + Shift + B
أو
Build → Build Solution
```

---

## ❌ المشاكل الشائعة والحلول

### مشكلة 1: "Cannot open solution"
```
❌ Error: Cannot find project file
✅ الحل:
   1. تأكد من وجود جميع مجلدات المشاريع:
      - EJLive.Shared
      - EJLive.Core
      - EJLive.Client.WinForms
      - EJLive.Server.WinForms
      - EJLive.Monitor
      - EJLive.Monitoring.WinForms
      - EJLive.Installer.WinForms
   
   2. تأكد من وجود EJLive.sln
   
   3. جرب: Right-click on solution → Reload Project
```

### مشكلة 2: "System.Data.SQLite not found"
```
❌ Error: The type 'SQLiteConnection' is not found
✅ الحل:
   1. في Visual Studio:
      Tools → NuGet Package Manager → Package Manager Console
   
   2. اكتب:
      Install-Package System.Data.SQLite.Core -Version 1.0.118.0
   
   3. اضغط: Enter
   
   4. انتظر الانتهاء (قد يأخذ 1-2 دقيقة)
   
   5. اضغط: Ctrl+Shift+B للبناء
```

### مشكلة 3: "Project unloaded"
```
❌ Error: The project cannot be loaded
✅ الحل:
   1. Right-click على المشروع
   2. اختر: Edit Project File
   3. تحقق من:
      - <TargetFrameworkVersion>v4.8</TargetFrameworkVersion>
      - جميع ProjectReference موجودة
   4. احفظ الملف
   5. Right-click → Reload Project
```

### مشكلة 4: ".NET Framework 4.8 not installed"
```
❌ Error: Target framework .NET Framework 4.8 is not installed
✅ الحل:
   1. اذهب إلى: https://dotnet.microsoft.com/download/dotnet-framework
   2. حمل: .NET Framework 4.8
   3. ثبت البرنامج
   4. أعد تشغيل Visual Studio
   5. افتح EJLive.sln من جديد
```

### مشكلة 5: "Build failed with errors"
```
❌ Error: Build failed (عدة أخطاء)
✅ الحل الشامل:
   1. Build → Clean Solution
   2. احذف مجلدات:
      - bin
      - obj
      من كل مشروع
   3. أغلق Visual Studio
   4. احذف مجلد: .vs (مخفي)
   5. افتح EJLive.sln من جديد
   6. اضغط: Ctrl+Shift+B
```

### مشكلة 6: "Package download failed"
```
❌ Error: Package could not be found
✅ الحل:
   1. تأكد من اتصالك بالإنترنت
   2. في Visual Studio:
      Tools → Options → NuGet Package Manager → Package Sources
   3. تأكد من وجود: nuget.org
   4. جرب:
      Tools → NuGet Package Manager → Package Manager Console
      Clear-Host
      Update-Package -Reinstall
```

---

## 🔍 فحص سريع

### تحقق من المتطلبات:
```powershell
# 1. فحص .NET Framework
reg query "HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\NET Framework Setup\NDP\v4\Full" /v Version

# 2. فحص Visual Studio
dir "C:\Program Files\Microsoft Visual Studio\2022"
أو
dir "C:\Program Files (x86)\Microsoft Visual Studio\2019"
```

### تحقق من الملفات:
```
الملفات المهمة يجب أن توجد:
✅ EJLive.sln
✅ EJLive.Shared\EJLive.Shared.csproj
✅ EJLive.Core\EJLive.Core.csproj
✅ EJLive.Client.WinForms\EJLive.Client.WinForms.csproj
✅ EJLive.Server.WinForms\EJLive.Server.WinForms.csproj
✅ EJLive.Monitor\EJLive.Monitor.csproj
✅ EJLive.Monitoring.WinForms\EJLive.Monitoring.WinForms.csproj
✅ EJLive.Installer.WinForms\EJLive.Installer.WinForms.csproj
```

---

## 🚀 جرب الحل الكامل (النووي)

```
1. اغلق Visual Studio
2. احذف:
   - جميع مجلدات bin
   - جميع مجلدات obj
   - مجلد .vs
3. احذف مجلد: C:\Users\YourUser\.nuget\packages\system.data.sqlite*
4. افتح PowerShell كـ Admin
5. اكتب:
   cd EJLive_Enterprise_Ultimate
   Remove-Item -Path .\.vs -Recurse -Force
   Get-ChildItem -Include bin,obj -Recurse | Remove-Item -Recurse -Force
6. افتح EJLive.sln
7. اضغط: Ctrl+Shift+B
```

---

## ✅ إذا نجح البناء

ستظهر الرسالة:
```
========== Build: 7 succeeded, 0 failed ==========
```

الملفات التنفيذية ستكون في:
```
EJLive.Client.WinForms\bin\Release\EJLive.Client.exe
EJLive.Server.WinForms\bin\Release\EJLive.Server.exe
EJLive.Monitoring.WinForms\bin\Release\EJLive.Monitoring.exe
EJLive.Installer.WinForms\bin\Release\EJLive.Installer.exe
```

---

## 📞 إذا استمرت المشاكل

جرب الخيار الثاني: استخدام PowerShell script
```powershell
cd EJLive_Enterprise_Ultimate
.\BUILD_NO_MSBUILD.ps1 -Configuration Release
```

أو استخدم: SIMPLE_BUILD.bat

---

## 🔧 تفاصيل تقنية

- **Language Version**: C# 8.0
- **Target Framework**: .NET Framework 4.8
- **Output Type**: Mixed (Libraries + Executables)
- **NuGet Package**: System.Data.SQLite.Core 1.0.118.0
- **Solution Format**: Visual Studio 2022 (v17.8)

---

**الآن جاهز؟ افتح `EJLive.sln` واضغط `Ctrl+Shift+B`!** ✅
