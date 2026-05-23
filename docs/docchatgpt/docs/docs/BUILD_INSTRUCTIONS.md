# EJLive Enterprise v3.2.1 - بناء وتشغيل المشروع

## الإصلاحات المنفذة في هذه النسخة

تم إصلاح جميع أخطاء البناء التالية:

### 1. خطأ CS0136 - المتغير المكرر
**المشكلة:** في ملف `ClientMainForm.cs` السطر 88 و 108، كان هناك متغيران باسم `connected` في نطاقات مختلفة.
**الحل:** تم إعادة تسمية المتغير في السطر 108 من `connected` إلى `isConnected`.

### 2. خطأ في OutputPath
**المشكلة:** ملفات `.csproj` كانت تستخدم OutputPath مختلفة.
**الحل:** تم توحيد جميع OutputPath إلى `bin\Debug\` و `bin\Release\`.

### 3. مشاكل المراجع
**المشكلة:** بعض المراجع كانت مفقودة أو غير صحيحة.
**الحل:** تم إضافة جميع المراجع المطلوبة وتصحيح GUIDs.

---

## خطوات البناء الناجح

### الخطوة 1: فك الضغط
```bash
# استخرج ملف ZIP إلى مجلد المشروع
unzip EJLive_Enterprise_WinForms_v3.2.1_FIXED.zip -d D:\Projects\EJLive\
```

### الخطوة 2: فتح المشروع
1. افتح **Visual Studio 2019** أو **Visual Studio 2022**
2. اختر **File → Open → Project/Solution**
3. انتقل إلى مجلد المشروع واختر **`EJLive.sln`**

### الخطوة 3: تنظيف وبناء
```
Build → Clean Solution
Build → Build Solution
```

أو استخدم الاختصارات:
```
Ctrl+Alt+Delete (لتنظيف)
Ctrl+Shift+B (للبناء)
```

### الخطوة 4: التحقق من النتيجة
يجب أن تظهر رسالة في Output:
```
========== Build: 6 succeeded, 0 failed ==========
```

### الخطوة 5: تشغيل المشروع
1. اختر **EJLive.Installer.WinForms** كـ Startup Project
2. اضغط **F5** أو **Ctrl+F5**
3. ستظهر واجهة التثبيت

---

## هيكل المشروع

```
EJLive/
├── EJLive.sln                          # ملف الحل
├── EJLive.Core/                        # مكتبة الأنواع والثوابت
│   ├── Constants.cs
│   ├── ATMInfo.cs
│   └── EJLive.Core.csproj
├── EJLive.Shared/                      # مكتبة الخدمات المشتركة
│   ├── SecurityHelper.cs               # التشفير والضغط
│   └── EJLive.Shared.csproj
├── EJLive.Installer.WinForms/          # برنامج التثبيت
│   ├── InstallerForm.cs
│   ├── Program.cs
│   └── EJLive.Installer.WinForms.csproj
├── EJLive.Client.WinForms/             # تطبيق العميل (ATM)
│   ├── ClientMainForm.cs
│   ├── Services/
│   │   ├── JournalProcessor.cs
│   │   ├── NetworkManager.cs
│   │   └── RemoteCommandHandler.cs
│   └── EJLive.Client.WinForms.csproj
├── EJLive.Server.WinForms/             # تطبيق الخادم
│   ├── ServerMainForm.cs
│   ├── Services/
│   │   ├── EJServer.cs
│   │   └── ArchiveManager.cs
│   └── EJLive.Server.WinForms.csproj
├── EJLive.Monitoring.WinForms/         # لوحة المراقبة
│   ├── MainDashboardForm.cs
│   ├── Program.cs
│   └── EJLive.Monitoring.WinForms.csproj
└── README.md
```

---

## المتطلبات

- **Visual Studio 2019** أو أحدث
- **.NET Framework 4.8 Developer Pack**
- **Windows 7 SP1** أو أحدث

---

## تشغيل المشاريع

### تشغيل الخادم أولاً
```
1. اختر EJLive.Server.WinForms كـ Startup Project
2. اضغط Ctrl+F5
3. في التبويب "Server": اضغط "Start Server"
```

### تشغيل العميل
```
1. اختر EJLive.Client.WinForms كـ Startup Project
2. اضغط Ctrl+F5
3. في التبويب "Configuration": أدخل بيانات الاتصال
4. في التبويب "Sync": اضغط "Start Sync"
```

### تشغيل لوحة المراقبة
```
1. اختر EJLive.Monitoring.WinForms كـ Startup Project
2. اضغط Ctrl+F5
```

---

## استكشاف الأخطاء

### إذا ظهر خطأ "Project not found"
- تأكد من أن جميع ملفات `.csproj` موجودة في المجلدات الصحيحة
- اختر **Build → Clean Solution** ثم **Build → Build Solution**

### إذا ظهر خطأ "Assembly not found"
- تأكد من أن .NET Framework 4.8 مثبت
- اختر **Tools → Get Tools and Features** وتحقق من تثبيت .NET Framework 4.8

### إذا ظهر خطأ "Missing reference"
- انقر بزر الماوس الأيمن على **Solution** → **Restore NuGet Packages**
- اختر **Build → Rebuild Solution**

---

## ملاحظات مهمة

1. **الملفات المؤقتة:** يمكنك حذف مجلدات `bin` و `obj` بأمان - سيتم إعادة إنشاؤها عند البناء
2. **الإعدادات:** تحفظ الإعدادات في ملف `ejlive.config` في مجلد التثبيت
3. **السجلات:** يتم حفظ السجلات في مجلد `Logs` في مجلد التثبيت

---

## الدعم والمساعدة

إذا واجهت أي مشاكل:
1. تحقق من أن جميع المراجع صحيحة في Solution Explorer
2. اختر **Build → Clean Solution** ثم أعد البناء
3. أعد تشغيل Visual Studio إذا استمرت المشاكل
