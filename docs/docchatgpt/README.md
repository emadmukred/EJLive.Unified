# EJLive Enterprise Ultimate v4.0.0

نظام أرشفة وحدة الجورنال الإلكتروني الكامل للصرافات الآلية

## المتطلبات

- **Windows 7 أو أحدث**
- **.NET Framework 4.8**
- **Visual Studio 2019+** أو **Build Tools for Visual Studio**
- **NuGet Package Manager**

## البنية

```
EJLive_Enterprise_Ultimate/
├── EJLive.Shared/                  # مكتبة مشتركة (أمان، تسجيل)
├── EJLive.Core/                    # المحركات الجوهرية
├── EJLive.Client.WinForms/         # تطبيق العميل (5 تبويبات)
├── EJLive.Server.WinForms/         # خادم المراقبة (6 تبويبات)
├── EJLive.Monitor/                 # مكتبة عرض البيانات
├── EJLive.Monitoring.WinForms/     # لوحة NOC
└── EJLive.Installer.WinForms/      # برنامج التثبيت
```

## البناء والتثبيت

### الطريقة السريعة (Windows)
```bash
cd EJLive_Enterprise_Ultimate
Build.bat Release
```

### البناء يدويًا
1. افتح `EJLive.sln` في Visual Studio
2. اختر `Release` من القائمة
3. اضغط `Ctrl+Shift+B` أو اختر **Build → Build Solution**

## المشاريع

| المشروع | النوع | الوصف |
|--------|-------|-------|
| **EJLive.Shared** | Library | أداة الأمان، المسجل، سياسات إعادة المحاولة |
| **EJLive.Core** | Library | محركات البروتوكول والمزامنة والتحليل |
| **EJLive.Client.WinForms** | WinApp | تطبيق العميل يعمل على الصراف |
| **EJLive.Server.WinForms** | WinApp | خادم المراقبة المركزي (TCP/5656) |
| **EJLive.Monitor** | Library | عناصر عرض البيانات المشتركة |
| **EJLive.Monitoring.WinForms** | WinApp | لوحة مراقبة العمليات (NOC Dashboard) |
| **EJLive.Installer.WinForms** | WinApp | برنامج تثبيت النظام |

## الميزات الرئيسية

### الأمان
- ✅ تشفير AES-256-CBC (نقل)
- ✅ تشفير RSA-2048 (مفاتيح الجلسة)
- ✅ مفاتيح PBKDF2 مشتقة
- ✅ توقيع ولا يمكن تعديل سجل التدقيق

### المزامنة
- ✅ دعم NCR/GRG/Wincor Nixdorf
- ✅ تتبع الإزاحة لملفات NCR (إعادة الحياة)
- ✅ مركز إعادة محاولة أسي مع jitter
- ✅ Idempotency لتجنب التكرار

### المراقبة
- ✅ 5 تبويبات في العميل
- ✅ 6 تبويبات في الخادم
- ✅ عرض شبحي (Ghost View) بدون تدخل
- ✅ قائمة التنبيهات الذكية

## المسارات الافتراضية

- **قاعدة البيانات**: `%PROGRAMDATA%\EJLive\Data\ejlive.db`
- **الأرشيف**: `%PROGRAMDATA%\EJLive\Archive\`
- **السجلات**: `%PROGRAMDATA%\EJLive\Logs\`
- **التقارير**: `%PROGRAMDATA%\EJLive\Reports\`

## الاتصال

- **البروتوكول**: TCP/IP (مخصص)
- **المنفذ الافتراضي**: 5656
- **جلسة العميل المشفرة**: نعم
- **حد المهلة**: 30 ثانية

## الملفات المرفقة

- `Build.bat` - سكريبت البناء التلقائي
- `Package.bat` - سكريبت إنشاء الحزمة
- `NuGet.Config` - إعدادات NuGet

## الدعم

للمساعدة والدعم الفني:
- البريد الإلكتروني: support@ejlive.com
- الإصدار: 4.0.0
- التاريخ: مايو 2026
