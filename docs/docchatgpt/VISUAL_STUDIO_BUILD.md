# 🎯 EJLive Enterprise — Build Without MSBuild

## الطريقة 1️⃣: فتح المشروع مباشرة في Visual Studio (الأفضل)

### الخطوات:
1. **افتح Visual Studio** (Community أو Professional)
2. **اذهب إلى**: File → Open → Project/Solution
3. **اختر**: `EJLive.sln` من المجلد الحالي
4. **اضغط**: Ctrl+Shift+B (أو Build → Build Solution)
5. **انتظر**: سيبني جميع المشاريع تلقائياً ✅

### مسارات البناء:
```
EJLive.Client.WinForms\bin\Release\EJLive.Client.exe
EJLive.Server.WinForms\bin\Release\EJLive.Server.exe
EJLive.Monitoring.WinForms\bin\Release\EJLive.Monitoring.exe
EJLive.Installer.WinForms\bin\Release\EJLive.Installer.exe
```

---

## الطريقة 2️⃣: PowerShell Script (بدون MSBuild مباشر)

```powershell
# في PowerShell:
.\BUILD_NO_MSBUILD.ps1 -Configuration Release
```

---

## الطريقة 3️⃣: إذا كنت لا تملك Visual Studio

### تثبيت Visual Studio Community (مجاني):

1. **اذهب إلى**: https://visualstudio.microsoft.com/downloads/
2. **حمل**: Visual Studio Community 2022
3. **أثناء التثبيت**, اختر:
   - ✅ Desktop Development with C++
   - ✅ .NET Framework 4.8 SDK
4. **اكتمل التثبيت**
5. **ثم**: فتح `EJLive.sln` والضغط Ctrl+Shift+B

---

## الطريقة 4️⃣: استخدام IDE بديل

### Visual Studio Code + C# Compiler

```bash
# تثبيت الإضافات:
# 1. C# DevKit (من Microsoft)
# 2. Omnisharp

# ثم افتح المجلد:
code .
```

---

## 🔧 استكشاف الأخطاء

### إذا قال: "Cannot find System.Data.SQLite"
✅ الحل:
1. اضغط على Project في القائمة
2. Manage NuGet Packages
3. ابحث عن: `System.Data.SQLite.Core`
4. اضغط Install

### إذا قال: ".NET Framework 4.8 not installed"
✅ الحل:
1. حمل من: https://dotnet.microsoft.com/download/dotnet-framework
2. اختر: .NET Framework 4.8
3. ثبت وأعد تشغيل Visual Studio

### إذا قالت: "Assembly not found"
✅ الحل:
1. Right-click على Project
2. Clean Project
3. Rebuild Project

---

## 📊 ترتيب البناء الصحيح

Visual Studio يبني المشاريع بالترتيب الصحيح تلقائياً:

```
1. EJLive.Shared (بدون مراجع)
   ↓
2. EJLive.Core (يستخدم Shared)
   ↓
3. EJLive.Client.WinForms (يستخدم Shared + Core)
   ├→ EJLive.Server.WinForms (يستخدم Shared + Core)
   ├→ EJLive.Monitor (يستخدم Shared + Core)
   │  ↓
   └→ EJLive.Monitoring.WinForms (يستخدم Server + Monitor)
      ↓
4. EJLive.Installer.WinForms (يستخدم الجميع)
```

---

## ✅ نصائح مهمة

- **لا تحتاج MSBuild مباشرة** — Visual Studio يتعامل مع كل شيء
- **كل شيء محفوظ في الـ solution** — لا تحتاج إلى سكريبتات
- **NuGet مدمج** — تلقائي التنزيل والتثبيت
- **الأخطاء واضحة** — سهل الإصلاح من UI

---

## 🚀 بعد البناء الناجح

ستجد الملفات التنفيذية في:

```
✅ EJLive.Client.exe      — تطبيق العميل
✅ EJLive.Server.exe      — الخادم المركزي
✅ EJLive.Monitoring.exe  — لوحة المراقبة
✅ EJLive.Installer.exe   — معالج التثبيت
```

جاهزة للتشغيل مباشرة! 🎉

---

## 💡 الملفات المهمة في المشروع

- `EJLive.sln` — ملف الحل الرئيسي (افتحه في Visual Studio)
- `*.csproj` — ملفات المشاريع (لا تحتاج لتعديلها)
- `packages.config` — مكتبات NuGet (تلقائية)
- `app.config` — إعدادات التطبيق (لا تحتاج لتعديل)

---

## 📞 إذا استمرت المشاكل

1. **تأكد**: Visual Studio معطل بشكل صحيح
2. **اختبر**: Create new project في VS (إذا نجح = VS يعمل)
3. **ابدأ من جديد**: Close VS → Delete bin/obj folders → Open again
4. **نظف الـ Cache**: Delete .vs folder (مجلد مخفي)

---

**الآن انسَ MSBuild، استخدم Visual Studio مباشرة! ✅**
