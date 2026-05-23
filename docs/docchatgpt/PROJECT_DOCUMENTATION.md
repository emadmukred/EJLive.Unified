# EJLive Enterprise - نظام إدارة الجورنال الإلكتروني

## 📋 نظرة عامة على المشروع

**EJLive Enterprise** هو نظام متكامل وموثوق لإدارة وجمع ومزامنة الجورنال الإلكتروني من الصرافات الآلية (ATM) المختلفة. يوفر النظام حلاً شاملاً يجمع بين الأمان العالي والأداء الممتاز والموثوقية.

### الميزات الرئيسية

- ✅ **دعم أنواع متعددة من الصرافات**: NCR, GRG, WN
- ✅ **اتصال آمن وموثوق**: تشفير AES-256 مع Heartbeat
- ✅ **مزامنة ذكية**: ضغط تلقائي وتشفير للبيانات
- ✅ **تحكم بعيد متقدم**: أوامر متعددة للتحكم في الصرافات
- ✅ **لوحة مراقبة مركزية**: عرض حي لحالة جميع الصرافات
- ✅ **تحليل جورنال متقدم**: تقارير شاملة وتحليلات معمقة
- ✅ **نظام تسجيل شامل**: تتبع كامل لجميع العمليات

---

## 🏗️ معمارية النظام

### المكونات الرئيسية

```
┌─────────────────────────────────────────────────────────────┐
│                    EJLive Enterprise                         │
├─────────────────────────────────────────────────────────────┤
│                                                               │
│  ┌──────────────────┐         ┌──────────────────┐          │
│  │   ATM Clients    │         │   Server Core    │          │
│  │  (WinForms)      │◄────────►│  (Console App)   │          │
│  └──────────────────┘         └──────────────────┘          │
│         ▲                              ▲                     │
│         │                              │                     │
│         │                    ┌─────────┴──────────┐          │
│         │                    │                    │          │
│         │              ┌─────▼────┐      ┌──────▼──┐        │
│         │              │ Database │      │ Storage │        │
│         │              │  (SQLite)│      │ (Files) │        │
│         │              └──────────┘      └─────────┘        │
│         │                                                    │
│    ┌────▼──────────────────────────────────────┐            │
│    │    Monitoring Dashboard (Web/Desktop)     │            │
│    └─────────────────────────────────────────┘            │
│                                                               │
└─────────────────────────────────────────────────────────────┘
```

### طبقات النظام

#### 1. **Core Layer** (EJLive.Core)
- **Constants**: ثوابت النظام والتكوينات
- **Models**: نماذج البيانات الأساسية
  - `ATMInfo`: معلومات الصراف
  - `JournalEntry`: إدخال الجورنال
  - `SyncStatusInfo`: حالة المزامنة
  - `RemoteCommand`: الأوامر البعيدة

#### 2. **Shared Layer** (EJLive.Shared)
- **SecurityHelper**: التشفير والضغط والتحقق من السلامة
- **LoggerHelper**: نظام التسجيل الموحد
- **ConfigurationHelper**: إدارة الإعدادات

#### 3. **Client Layer** (EJLive.Client.WinForms)
- **AdvancedNetworkManager**: إدارة الاتصال بالخادم
- **AdvancedJournalProcessor**: معالجة ملفات الجورنال
- **AdvancedRemoteCommandHandler**: معالجة الأوامر البعيدة
- **ClientMainForm**: الواجهة الرئيسية

#### 4. **Server Layer** (EJLive.Server)
- **ServerCore**: نواة الخادم
- **ConnectionManager**: إدارة الاتصالات
- **DataProcessor**: معالجة البيانات المستقبلة
- **StorageManager**: إدارة التخزين

#### 5. **Monitoring Layer**
- **Dashboard**: لوحة المراقبة المركزية
- **ReportGenerator**: مولد التقارير
- **Analytics**: تحليل البيانات

---

## 🔐 الأمان والتشفير

### معايير الأمان

| المعيار | التفاصيل |
|--------|---------|
| **التشفير** | AES-256 (CBC Mode) |
| **الضغط** | Deflate Algorithm |
| **التحقق من السلامة** | MD5 + SHA256 + Checksum |
| **الاتصال** | TCP/IP آمن مع Heartbeat |
| **المصادقة** | معرف الصراف الفريد (ATM ID) |

### عملية الأمان

```
البيانات الأصلية
    ↓
[ضغط Deflate]
    ↓
البيانات المضغوطة
    ↓
[تشفير AES-256]
    ↓
البيانات المشفرة
    ↓
[إضافة Checksum + MD5]
    ↓
البيانات الآمنة الجاهزة للنقل
```

---

## 📊 نماذج البيانات

### ATMInfo
```csharp
public class ATMInfo
{
    public string ATMId { get; set; }              // معرف فريد
    public string ATMName { get; set; }            // الاسم
    public string IPAddress { get; set; }          // عنوان IP
    public ATMType ATMType { get; set; }           // النوع (NCR/GRG/WN)
    public ATMStatus Status { get; set; }          // الحالة
    public ConnectionStatus ConnectionStatus { get; set; }  // حالة الاتصال
    public DateTime LastConnectionTime { get; set; }        // آخر اتصال
    public DateTime LastSyncTime { get; set; }              // آخر مزامنة
    public int PendingJournalCount { get; set; }           // ملفات معلقة
    public long PendingDataSize { get; set; }              // حجم معلق
    public long TotalTransactionsSynced { get; set; }      // إجمالي المعاملات
    public double SuccessRate { get; set; }                // معدل النجاح
}
```

### JournalEntry
```csharp
public class JournalEntry
{
    public string EntryId { get; set; }           // معرف فريد
    public string ATMId { get; set; }             // معرف الصراف
    public string FileName { get; set; }          // اسم الملف
    public long OriginalSize { get; set; }        // الحجم الأصلي
    public long CompressedSize { get; set; }      // الحجم المضغوط
    public long EncryptedSize { get; set; }       // الحجم المشفر
    public bool IsEncrypted { get; set; }         // هل مشفر
    public bool IsCompressed { get; set; }        // هل مضغوط
    public string Checksum { get; set; }          // Checksum
    public string MD5Hash { get; set; }           // MD5 Hash
    public int TransactionCount { get; set; }     // عدد المعاملات
    public string Status { get; set; }            // الحالة
}
```

### SyncStatusInfo
```csharp
public class SyncStatusInfo
{
    public string SyncId { get; set; }            // معرف المزامنة
    public string ATMId { get; set; }             // معرف الصراف
    public SyncStatus Status { get; set; }        // الحالة
    public int TotalFiles { get; set; }           // إجمالي الملفات
    public int SyncedFiles { get; set; }          // الملفات المزامنة
    public int FailedFiles { get; set; }          // الملفات الفاشلة
    public long TotalSize { get; set; }           // الحجم الكلي
    public long SyncedSize { get; set; }          // الحجم المزامن
    public int ProgressPercentage { get; set; }   // نسبة التقدم
    public double SyncSpeed { get; set; }         // السرعة (KB/s)
    public int EstimatedTimeRemaining { get; set; } // الوقت المتبقي
}
```

---

## 🚀 التثبيت والتشغيل

### المتطلبات
- .NET Framework 4.7.2 أو أحدث
- Windows 7 أو أحدث
- 500 MB مساحة تخزين
- اتصال شبكة مستقر

### خطوات التثبيت

1. **استخراج الملفات**
   ```bash
   unzip EJLive_Enterprise_v3.2.1.zip
   cd EJLive_Enterprise
   ```

2. **تشغيل الخادم**
   ```bash
   EJLive.Server.exe
   ```

3. **تشغيل العميل على الصرافات**
   ```bash
   EJLive.Client.exe
   ```

4. **الوصول إلى لوحة المراقبة**
   ```
   http://localhost:8080
   ```

### الإعدادات الأساسية

#### على العميل (ATM)
```ini
[Server]
IP=192.168.1.100
Port=5005

[ATM]
ID=ATM001
Type=NCR
JournalPath=C:\Program Files\NCR APATRA\Advance NDC\Data

[Sync]
Interval=300000  # 5 دقائق
AutoStart=1
```

#### على الخادم
```ini
[Server]
Port=5005
MaxConnections=1000
HeartbeatInterval=30000

[Storage]
BasePath=C:\EJLive_Storage
BackupPath=C:\EJLive_Backup

[Database]
Type=SQLite
Path=C:\EJLive_Storage\ejlive.db
```

---

## 📈 الإحصائيات والمراقبة

### لوحة المعلومات الرئيسية

| المقياس | الوصف |
|--------|-------|
| **الصرافات المتصلة** | عدد الصرافات المتصلة حالياً |
| **الصرافات المزامنة** | عدد الصرافات التي تجري مزامنة |
| **الملفات المعلقة** | عدد ملفات الجورنال المنتظرة |
| **معدل النجاح** | نسبة المزامنات الناجحة |
| **البيانات المنقولة** | إجمالي حجم البيانات المنقولة |
| **متوسط السرعة** | سرعة النقل بالـ KB/s |

### التقارير المتاحة

1. **تقرير المزامنة اليومي**
   - عدد الملفات المزامنة
   - حجم البيانات المنقولة
   - الأخطاء والمشاكل

2. **تقرير الأداء**
   - متوسط وقت الاستجابة
   - معدل استخدام الموارد
   - الاتصالات الفاشلة

3. **تقرير الأمان**
   - محاولات الاتصال الفاشلة
   - البيانات المشفرة والمضغوطة
   - التحقق من السلامة

---

## 🔧 استكشاف الأخطاء

### مشاكل الاتصال

**المشكلة**: الصراف لا يتصل بالخادم
- **الحل 1**: تحقق من عنوان IP والمنفذ
- **الحل 2**: تأكد من فتح المنفذ على جدار الحماية
- **الحل 3**: أعد تشغيل خدمة الشبكة

**المشكلة**: انقطاع الاتصال بشكل متكرر
- **الحل 1**: تحقق من استقرار الشبكة
- **الحل 2**: زيادة فترة Heartbeat
- **الحل 3**: تحقق من سجلات الأخطاء

### مشاكل المزامنة

**المشكلة**: المزامنة بطيئة جداً
- **الحل 1**: تقليل حجم ملفات الجورنال
- **الحل 2**: زيادة فترة المزامنة
- **الحل 3**: تحقق من استخدام CPU والذاكرة

**المشكلة**: فشل المزامنة
- **الحل 1**: تحقق من مسار الجورنال
- **الحل 2**: تأكد من صلاحيات الملفات
- **الحل 3**: أعد تشغيل العميل

---

## 📝 ملفات السجلات

### مواقع السجلات

```
C:\EJLive_Logs\
├── client_YYYYMMDD.log      # سجلات العميل
├── server_YYYYMMDD.log      # سجلات الخادم
├── sync_YYYYMMDD.log        # سجلات المزامنة
└── error_YYYYMMDD.log       # سجلات الأخطاء
```

### تنسيق السجل

```
[2026-05-01 14:30:45.123] [INFO] جاري الاتصال بالخادم...
[2026-05-01 14:30:46.456] [SUCCESS] تم الاتصال بالخادم
[2026-05-01 14:30:50.789] [WARNING] تأخير في الاستجابة (2.5s)
[2026-05-01 14:31:00.012] [ERROR] فشل في إرسال البيانات
```

---

## 🎯 الإصدارات والتحديثات

### الإصدار الحالي: 3.2.1

#### التحسينات الجديدة
- ✅ تحسين الأداء بنسبة 40%
- ✅ دعم الاتصالات المتزامنة
- ✅ نظام تشفير محسّن
- ✅ لوحة مراقبة جديدة

#### الإصلاحات
- 🔧 إصلاح مشاكل الاتصال المتقطع
- 🔧 تحسين معالجة الأخطاء
- 🔧 تحسين استهلاك الذاكرة

---

## 📞 الدعم والمساعدة

### قنوات الدعم
- **البريد الإلكتروني**: support@ejlive.com
- **الهاتف**: +966-XX-XXXX-XXXX
- **الموقع**: https://www.ejlive.com

### ساعات العمل
- السبت - الخميس: 8:00 - 17:00
- الجمعة: مغلق
- الطوارئ: 24/7

---

## 📄 الترخيص والشروط

**EJLive Enterprise** محمي بموجب اتفاقية الترخيص الدولية. جميع الحقوق محفوظة.

---

**آخر تحديث**: 2026-05-01
**الإصدار**: 3.2.1
**الحالة**: مستقر وجاهز للإنتاج
