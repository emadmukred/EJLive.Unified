# 🚀 تشغيل التطبيقات مباشرة (بدون بناء)

## الطريقة الأسرع: استخدام النسخ المسبقة

إذا كان لديك أي نسخة قديمة من EJLive تعمل، يمكنك استخدام الملفات الجاهزة مباشرة.

---

## الخطوة 1: البناء (مرة واحدة فقط)

### أسهل طريقة:

```
1. افتح: EJLive.sln بـ Visual Studio
2. اضغط: Ctrl+Shift+B
3. انتظر ≈ 2 دقيقة
```

---

## الخطوة 2: التشغيل

بعد البناء الناجح:

### تشغيل العميل (على جهاز ATM):
```powershell
cd EJLive_Enterprise_Ultimate\EJLive.Client.WinForms\bin\Release
.\EJLive.Client.exe
```

### تشغيل الخادم (على جهاز المركز):
```powershell
cd EJLive_Enterprise_Ultimate\EJLive.Server.WinForms\bin\Release
.\EJLive.Server.exe
```

### تشغيل لوحة المراقبة (NOC):
```powershell
cd EJLive_Enterprise_Ultimate\EJLive.Monitoring.WinForms\bin\Release
.\EJLive.Monitoring.exe
```

### تشغيل المثبِّت:
```powershell
cd EJLive_Enterprise_Ultimate\EJLive.Installer.WinForms\bin\Release
.\EJLive.Installer.exe
```

---

## إعدادات الاتصال الأساسية

### على العميل:
```
Server IP:     127.0.0.1 (أو IP الخادم الفعلي)
Server Port:   5656
ATM ID:        ATM001
ATM Type:      NCR (أو GRG / WN)
```

### على الخادم:
```
Listen Port:   5656
Archive Path:  C:\Program Files\EJLive\Archive\
Database Path: C:\ProgramData\EJLive\Data\
```

---

## المسارات المطلوبة

تأكد من وجود هذه المجلدات (سيتم إنشاؤها تلقائياً):

```
C:\ProgramData\EJLive\Data\          ← قاعدة البيانات SQLite
C:\ProgramData\EJLive\Archive\       ← الملفات المؤرشفة
C:\ProgramData\EJLive\Logs\          ← سجلات التطبيق
C:\ProgramData\EJLive\Reports\       ← التقارير المُصدَّرة
```

---

## معالجة الأخطاء الشائعة

### ❌ "Cannot find System.Data.SQLite"
**الحل**: 
```
1. في Visual Studio: Tools → NuGet Package Manager → Package Manager Console
2. اكتب: Install-Package System.Data.SQLite.Core
3. اضغط Enter
4. أعد البناء (Ctrl+Shift+B)
```

### ❌ "Port 5656 already in use"
**الحل**:
```powershell
# ابحث عن البرنامج الذي يستخدم المنفذ:
netstat -ano | findstr :5656

# أوقفه أو غيّر المنفذ في app.config
```

### ❌ "Access denied for %PROGRAMDATA%"
**الحل**:
```
1. افتح PowerShell كـ Admin
2. شغّل البرنامج من هناك
```

---

## نصائح مهمة

✅ **لا تحتاج لـ MSBuild مباشرة** — Visual Studio يتعامل معه تلقائياً

✅ **البناء يحدث تلقائياً** — عند Ctrl+Shift+B

✅ **NuGet يتنزل تلقائياً** — عند البناء الأول

✅ **جميع الملفات موجودة** — لا حاجة لإضافة شيء

---

## الخطوة التالية: الاختبار

بعد التشغيل:

1. **الخادم**: يجب أن يبدأ في الاستماع على المنفذ 5656
2. **العميل**: يجب أن يتصل بالخادم
3. **قاعدة البيانات**: سيتم إنشاؤها تلقائياً
4. **السجلات**: ستظهر في `%PROGRAMDATA%\EJLive\Logs\`

---

**الآن استمتع بـ EJLive Enterprise! 🎉**
