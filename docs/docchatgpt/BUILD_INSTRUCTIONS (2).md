# دليل بناء وتشغيل EJLive Enterprise Ultimate v4.0.0
# تعليمات شاملة للمطور

---

## متطلبات البيئة

| المتطلب | الإصدار المطلوب |
|---------|----------------|
| Windows | 7 SP1 / 8.1 / 10 / 11 |
| Visual Studio | 2019 أو 2022 (Community مجاني) |
| .NET Framework | 4.8 |
| C# Language Version | 8.0 |
| مساحة القرص | 500 MB على الأقل |
| RAM | 4 GB على الأقل |

---

## 1. فتح الحل في Visual Studio

```
1. افتح Visual Studio 2019/2022
2. اختر: File → Open → Solution
3. اختر الملف: EJLive.sln
4. انتظر حتى يُحمَّل الحل (قد يستغرق دقيقة)
5. تأكد من عدم وجود أخطاء حمراء في Solution Explorer
```

---

## 2. ترتيب البناء (Build Order)

يجب بناء المشاريع بهذا الترتيب بسبب التبعيات:

```
1. EJLive.Shared        (لا يعتمد على أي شيء)
2. EJLive.Core          (يعتمد على EJLive.Shared)
3. EJLive.Client.WinForms  (يعتمد على Core + Shared)
4. EJLive.Server.WinForms  (يعتمد على Core + Shared + Client.Controls)
5. EJLive.Monitor          (يعتمد على Core + Shared)
6. EJLive.Monitoring.WinForms (يعتمد على Core + Shared + Client.Controls)
7. EJLive.Installer.WinForms  (يعتمد على Core + Shared)
```

### الطريقة السريعة:
```
Build → Build Solution  (Ctrl+Shift+B)
```

---

## 3. تشغيل كل تطبيق

### تطبيق السيرفر (يُشغَّل أولًا على الخادم المركزي)
```
1. Right-click على EJLive.Server.WinForms في Solution Explorer
2. Set as Startup Project
3. F5 أو اضغط زر "Start"
4. عند فتح النافذة: اضغط "▶ تشغيل السيرفر"
```

### تطبيق العميل (يُثبَّت على كل صراف)
```
1. Right-click على EJLive.Client.WinForms
2. Set as Startup Project
3. F5
4. أدخل IP السيرفر والبورت (5656)
5. اختر نوع الصراف (NCR/GRG/WN)
6. اضغط "Connect"
```

### لوحة المراقبة (NOC Dashboard)
```
1. Right-click على EJLive.Monitoring.WinForms
2. Set as Startup Project
3. F5
```

---

## 4. إعداد قاعدة البيانات

النظام يستخدم SQLite محليًا (لا يحتاج تثبيتًا).

ملف قاعدة البيانات يُنشأ تلقائيًا في:
```
C:\ProgramData\EJLive\Database\ejlive.db
```

### للترقية إلى SQL Server (للبيئات الكبيرة):
1. افتح `EJLive.Core/Engine/ArchiveManager.cs`
2. غيّر connection string لـ SQL Server
3. اضف NuGet package: `System.Data.SqlClient`

---

## 5. إعدادات الأمان

### تغيير مفتاح التشفير (مهم في الإنتاج!)
```csharp
// في EJLive.Shared/SecurityHelper.cs
// سطر 18: غيّر هذا النص بمفتاح سري خاص بك
string keySource = "YOUR_UNIQUE_SECURE_KEY_HERE_v4.0.0";
```

### إضافة مستخدمين
```csharp
// في EJLive.Core/Services/RoleBasedAccess.cs
// سطر 24: أضف مستخدمين جدد
["manager"] = (HashPassword("secure_password"), UserRole.Admin),
["noc_user"] = (HashPassword("noc_pass"), UserRole.Observer),
```

### الأدوار والصلاحيات
| الدور | الصلاحيات |
|-------|-----------|
| Admin | تحكم كامل |
| Auditor | عرض + تصدير تقارير + سجل التدقيق |
| Support | عرض + تشخيص + لقطة شاشة |
| Observer | عرض فقط |

---

## 6. إعداد التشغيل التلقائي كـ Windows Service

### للعميل (على الصراف):
```batch
REM شغّل هذا الأمر كـ Administrator
sc create "EJLiveClient" binPath="C:\EJLive\EJLive.Client.exe" start=auto
sc description "EJLiveClient" "EJLive ATM Journal Sync Client"
sc start "EJLiveClient"
```

### للسيرفر:
```batch
sc create "EJLiveServer" binPath="C:\EJLive\EJLive.Server.exe" start=auto
sc description "EJLiveServer" "EJLive ATM Journal Archive Server"
sc start "EJLiveServer"
```

---

## 7. هيكل مجلدات التثبيت

### على الصراف (Client):
```
C:\EJLive\
├── EJLive.Client.exe
├── EJLive.Core.dll
├── EJLive.Shared.dll
├── config\
│   └── client.json
├── backup\          ← النسخ الاحتياطية المحلية للجورنال
├── outbox\          ← JournalOutbox (لضمان عدم فقدان البيانات)
└── logs\
    └── ejlive_client_YYYYMMDD.log
```

### على السيرفر:
```
D:\EJLive\
├── EJLive.Server.exe
├── EJLive.Core.dll
├── EJLive.Shared.dll
├── Archive\
│   ├── ATM-001\
│   │   ├── 2024-01\
│   │   │   ├── EJDATA_20240101.log.enc
│   │   │   └── EJDATA_20240101.log.enc.idx
│   │   └── 2024-02\
│   └── ATM-002\
├── Reports\
│   ├── Daily\
│   ├── Monthly\
│   └── Audit\
├── AuditLogs\
│   └── audit_YYYYMMDD.log
└── config\
    └── server.json
```

---

## 8. المنفذ الشبكي المطلوب

| المنفذ | البروتوكول | الاستخدام |
|--------|-----------|----------|
| 5656 | TCP | الاتصال بين العميل والسيرفر |

### إعداد Firewall:
```batch
netsh advfirewall firewall add rule name="EJLive Server" ^
  protocol=TCP dir=in localport=5656 action=allow
```

---

## 9. اختبار الاتصال

```batch
REM على جهاز العميل — اختبر الوصول للسيرفر
telnet SERVER_IP 5656
REM أو
Test-NetConnection -ComputerName SERVER_IP -Port 5656
```

---

## 10. الخطوات التالية للتطوير

### الحزمة الأولى (المقترح خلال أسبوعين):
- [ ] ربط ServerEngine بـ Channel<T> للمعالجة غير المتزامنة
- [ ] إضافة Length-Prefixed Framing للنقل
- [ ] اختبار JournalOutbox مع انقطاع شبكة حقيقي
- [ ] تفعيل تشفير قاعدة بيانات الأرشيف

### الحزمة الثانية (الأسبوعان التاليان):
- [ ] إضافة LiveCharts/OxyPlot للرسوم البيانية
- [ ] تصدير PDF للتقارير
- [ ] جدولة التقارير التلقائية

### الحزمة الثالثة (الشهر الثاني):
- [ ] RSA Handshake لتبادل مفاتيح AES
- [ ] Mutual Authentication بالشهادات
- [ ] نظام OTA Updates

---

## 11. حل المشكلات الشائعة

| المشكلة | الحل |
|---------|------|
| `IOException: The process cannot access the file` | ملف الجورنال مقفل — النظام يعيد المحاولة تلقائيًا بـ FileShare.ReadWrite |
| `SocketException: Connection refused` | تأكد أن السيرفر يعمل وأن البورت 5656 مفتوح |
| `AES Decryption failed` | تأكد من تطابق مفتاح التشفير بين العميل والسيرفر |
| واجهة المراقبة بطيئة | تأكد من إيقاف OwnerDraw الزائد في ListView |
| بطاقات الصرافات لا تظهر | تأكد من اتصال العملاء بالسيرفر (راجع تبويب Connections) |
