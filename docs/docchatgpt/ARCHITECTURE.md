# معمارية نظام EJLive Enterprise v3.2.1

## 🏗️ البنية المعمارية الشاملة

```
┌─────────────────────────────────────────────────────────────────┐
│                    EJLive Enterprise System                      │
└─────────────────────────────────────────────────────────────────┘

┌─────────────────────────────────────────────────────────────────┐
│                    Presentation Layer (UI)                       │
├─────────────────────────────────────────────────────────────────┤
│                                                                   │
│  ┌──────────────────┐  ┌──────────────────┐  ┌──────────────┐   │
│  │  Client WinForms │  │  Server WinForms │  │  Monitoring  │   │
│  │   (ATM Side)     │  │  (Central Hub)   │  │  Dashboard   │   │
│  └──────────────────┘  └──────────────────┘  └──────────────┘   │
│                                                                   │
└─────────────────────────────────────────────────────────────────┘

┌─────────────────────────────────────────────────────────────────┐
│                    Business Logic Layer                          │
├─────────────────────────────────────────────────────────────────┤
│                                                                   │
│  ┌──────────────────────────────────────────────────────────┐   │
│  │              Core Services & Managers                     │   │
│  ├──────────────────────────────────────────────────────────┤   │
│  │ • JournalProcessor    - معالجة ملفات الجورنال           │   │
│  │ • NetworkManager      - إدارة الاتصال بالشبكة           │   │
│  │ • RemoteCommandHandler - معالجة الأوامر البعيدة         │   │
│  │ • ArchiveManager      - إدارة الأرشفة                   │   │
│  │ • JournalAnalyzer     - تحليل الجورنال                  │   │
│  │ • ReportGenerator     - إنشاء التقارير                  │   │
│  └──────────────────────────────────────────────────────────┘   │
│                                                                   │
└─────────────────────────────────────────────────────────────────┘

┌─────────────────────────────────────────────────────────────────┐
│                    Data Access Layer                             │
├─────────────────────────────────────────────────────────────────┤
│                                                                   │
│  ┌──────────────────────────────────────────────────────────┐   │
│  │              Shared Services                             │   │
│  ├──────────────────────────────────────────────────────────┤   │
│  │ • SecurityHelper      - التشفير والضغط                  │   │
│  │ • ConfigManager       - إدارة الإعدادات                 │   │
│  │ • LogManager          - إدارة السجلات                   │   │
│  │ • DatabaseHelper      - مساعد قاعدة البيانات            │   │
│  └──────────────────────────────────────────────────────────┘   │
│                                                                   │
└─────────────────────────────────────────────────────────────────┘

┌─────────────────────────────────────────────────────────────────┐
│                    Data Storage Layer                            │
├─────────────────────────────────────────────────────────────────┤
│                                                                   │
│  ┌──────────────────┐  ┌──────────────────┐  ┌──────────────┐   │
│  │  Journal Files   │  │  Configuration   │  │  Logs &      │   │
│  │  (Encrypted)     │  │  Files           │  │  Analytics   │   │
│  └──────────────────┘  └──────────────────┘  └──────────────┘   │
│                                                                   │
└─────────────────────────────────────────────────────────────────┘
```

---

## 📦 هيكل المشاريع المفصل

### 1. **EJLive.Core** - مكتبة الأنواع والثوابت
```
EJLive.Core/
├── Constants.cs              - الثوابت المشتركة (AES Key, IV, Ports)
├── Models/
│   ├── ATMInfo.cs           - معلومات الصراف الآلي
│   ├── JournalEntry.cs      - إدخال الجورنال
│   ├── SyncStatus.cs        - حالة المزامنة
│   ├── RemoteCommand.cs     - الأوامر البعيدة
│   ├── ArchiveInfo.cs       - معلومات الأرشفة
│   └── AlertInfo.cs         - معلومات التنبيهات
├── Enums/
│   ├── ATMType.cs           - نوع الصراف (NCR, GRG, WN)
│   ├── ConnectionStatus.cs  - حالة الاتصال
│   ├── CommandType.cs       - نوع الأمر البعيد
│   └── AlertLevel.cs        - مستوى التنبيه
└── Interfaces/
    ├── IJournalProcessor.cs
    ├── INetworkManager.cs
    └── ICommandHandler.cs
```

### 2. **EJLive.Shared** - الخدمات المشتركة
```
EJLive.Shared/
├── Security/
│   ├── SecurityHelper.cs    - التشفير AES-256 والضغط ZLib
│   ├── EncryptionService.cs - خدمة التشفير المتقدمة
│   └── DecryptionService.cs - خدمة فك التشفير
├── Configuration/
│   ├── ConfigManager.cs     - مدير الإعدادات
│   └── AppSettings.cs       - إعدادات التطبيق
├── Logging/
│   ├── LogManager.cs        - مدير السجلات
│   ├── EventLogger.cs       - مسجل الأحداث
│   └── ErrorLogger.cs       - مسجل الأخطاء
├── Database/
│   ├── DatabaseHelper.cs    - مساعد قاعدة البيانات
│   └── SQLiteManager.cs     - مدير SQLite
└── Utilities/
    ├── DateTimeHelper.cs    - مساعد التاريخ والوقت
    └── FileHelper.cs        - مساعد الملفات
```

### 3. **EJLive.Client.WinForms** - تطبيق العميل
```
EJLive.Client.WinForms/
├── ClientMainForm.cs        - الواجهة الرئيسية
├── ClientMainForm.Designer.cs
├── Tabs/
│   ├── DashboardTab.cs      - تبويب لوحة المعلومات
│   ├── JournalSyncTab.cs    - تبويب المزامنة
│   ├── RemoteControlTab.cs  - تبويب التحكم البعيد
│   ├── SettingsTab.cs       - تبويب الإعدادات
│   └── LogsTab.cs           - تبويب السجلات
├── Services/
│   ├── JournalProcessor.cs  - معالج الجورنال
│   ├── NetworkManager.cs    - مدير الشبكة
│   ├── RemoteCommandHandler.cs - معالج الأوامر البعيدة
│   └── SyncManager.cs       - مدير المزامنة
├── Forms/
│   ├── SettingsForm.cs      - نموذج الإعدادات
│   └── AboutForm.cs         - نموذج حول البرنامج
└── Program.cs
```

### 4. **EJLive.Server.WinForms** - تطبيق الخادم
```
EJLive.Server.WinForms/
├── ServerMainForm.cs        - الواجهة الرئيسية
├── ServerMainForm.Designer.cs
├── Tabs/
│   ├── ServerTab.cs         - تبويب الخادم
│   ├── ConnectionsTab.cs    - تبويب الاتصالات
│   ├── JournalRepoTab.cs    - تبويب مستودع الجورنال
│   ├── ArchiveTab.cs        - تبويب الأرشفة
│   ├── RemoteCommandsTab.cs - تبويب الأوامر البعيدة
│   ├── ReportsTab.cs        - تبويب التقارير
│   ├── SettingsTab.cs       - تبويب الإعدادات
│   └── LogsTab.cs           - تبويب السجلات
├── Services/
│   ├── EJServer.cs          - منطق الخادم الأساسي
│   ├── ConnectionManager.cs - مدير الاتصالات
│   ├── ArchiveManager.cs    - مدير الأرشفة
│   ├── JournalRepository.cs - مستودع الجورنال
│   ├── JournalAnalyzer.cs   - محلل الجورنال
│   └── ReportGenerator.cs   - منشئ التقارير
├── Forms/
│   ├── SettingsForm.cs      - نموذج الإعدادات
│   ├── JournalViewerForm.cs - نموذج عارض الجورنال
│   └── ReportViewerForm.cs  - نموذج عارض التقارير
└── Program.cs
```

### 5. **EJLive.Monitoring.WinForms** - لوحة المراقبة
```
EJLive.Monitoring.WinForms/
├── MainDashboardForm.cs     - الواجهة الرئيسية
├── MainDashboardForm.Designer.cs
├── Tabs/
│   ├── DashboardTab.cs      - لوحة المعلومات
│   ├── ATMNetworkTab.cs     - تبويب شبكة الصرافات
│   ├── AlertsTab.cs         - تبويب التنبيهات
│   ├── ArchiveTab.cs        - تبويب الأرشفة
│   └── SettingsTab.cs       - تبويب الإعدادات
├── Controls/
│   ├── ATMStatusCard.cs     - بطاقة حالة الصراف
│   ├── NetworkMapControl.cs - عنصر خريطة الشبكة
│   └── AlertPanel.cs        - لوحة التنبيهات
├── Services/
│   ├── DashboardService.cs  - خدمة لوحة المعلومات
│   └── AlertService.cs      - خدمة التنبيهات
└── Program.cs
```

### 6. **EJLive.Installer.WinForms** - برنامج التثبيت
```
EJLive.Installer.WinForms/
├── InstallerForm.cs         - واجهة التثبيت
├── InstallerForm.Designer.cs
├── Services/
│   ├── InstallationService.cs - خدمة التثبيت
│   └── RegistryHelper.cs    - مساعد السجل
└── Program.cs
```

---

## 🔄 تدفق البيانات

### سيناريو المزامنة الكاملة

```
1. ATM Client (الصراف)
   ↓
2. JournalProcessor - قراءة ملفات الجورنال
   ↓
3. SecurityHelper - تشفير البيانات (AES-256)
   ↓
4. NetworkManager - إرسال البيانات عبر TCP/IP
   ↓
5. EJLive Server (الخادم)
   ↓
6. ConnectionManager - استقبال البيانات
   ↓
7. SecurityHelper - فك تشفير البيانات
   ↓
8. JournalRepository - تخزين الملفات
   ↓
9. JournalAnalyzer - تحليل البيانات
   ↓
10. ReportGenerator - إنشاء التقارير
```

---

## 🔐 نموذج الأمان

### التشفير
- **الخوارزمية:** AES-256 (Advanced Encryption Standard)
- **مفتاح التشفير:** 32 بايت (256 بت)
- **متجه التهيئة (IV):** 16 بايت (128 بت)
- **وضع التشفير:** CBC (Cipher Block Chaining)
- **الحشو:** PKCS7

### التحقق من السلامة
- **MD5 Hash:** للتحقق من سلامة البيانات
- **Checksum:** للتحقق من الملفات المستقبلة

### المصادقة
- **اسم المستخدم وكلمة المرور:** للوصول إلى الإعدادات
- **مفتاح الخادم:** للتحقق من هوية الخادم

---

## 🌐 نموذج الاتصال

### البروتوكول
- **نوع الاتصال:** TCP/IP
- **المنفذ الافتراضي:** 5005
- **Timeout:** 30 ثانية
- **Heartbeat:** كل 30 ثانية

### رسائل الاتصال

#### 1. رسالة الاتصال الأولية
```
┌─────────────────────────────────────────┐
│ Message Type: CONNECT                   │
│ ATM ID: 001                             │
│ ATM Type: NCR                           │
│ Version: 3.2.1                          │
│ Timestamp: 2026-05-01 12:00:00          │
└─────────────────────────────────────────┘
```

#### 2. رسالة إرسال الجورنال
```
┌─────────────────────────────────────────┐
│ Message Type: JOURNAL_DATA              │
│ ATM ID: 001                             │
│ File Name: EJ_20260501.txt              │
│ File Size: 1024 bytes                   │
│ Checksum: A1B2C3D4E5F6                  │
│ Data: [Encrypted & Compressed]          │
└─────────────────────────────────────────┘
```

#### 3. رسالة الأمر البعيد
```
┌─────────────────────────────────────────┐
│ Message Type: REMOTE_COMMAND            │
│ Command: RESTART                        │
│ ATM ID: 001                             │
│ Timestamp: 2026-05-01 12:00:00          │
│ Signature: [Digital Signature]          │
└─────────────────────────────────────────┘
```

---

## 💾 نموذج التخزين

### هيكل المجلدات على الخادم
```
C:\EJLive_Storage\
├── ATM_001\
│   ├── 2026\
│   │   ├── 05\
│   │   │   ├── EJ_20260501.ejf (Encrypted)
│   │   │   ├── EJ_20260502.ejf (Encrypted)
│   │   │   └── metadata.json
│   │   └── 04\
│   │       └── ...
│   └── metadata.json
├── ATM_002\
│   └── ...
├── Archive\
│   ├── 2026_Q1.zip
│   ├── 2026_Q2.zip
│   └── ...
└── Logs\
    ├── 2026-05-01.log
    ├── 2026-05-02.log
    └── ...
```

### ملف metadata.json
```json
{
  "atm_id": "001",
  "atm_type": "NCR",
  "file_name": "EJ_20260501.ejf",
  "original_size": 1024,
  "compressed_size": 512,
  "encrypted": true,
  "checksum": "A1B2C3D4E5F6",
  "timestamp": "2026-05-01T12:00:00Z",
  "transaction_count": 150,
  "has_errors": false
}
```

---

## 📊 نموذج البيانات

### جدول ATM Information
```
ATM_ID (PK)    | IP Address    | Type  | Last Sync | Status
001            | 192.168.1.10  | NCR   | 12:00:00  | Online
002            | 192.168.1.11  | GRG   | 11:55:00  | Online
003            | 192.168.1.12  | WN    | 10:30:00  | Offline
```

### جدول Journal Files
```
File_ID (PK) | ATM_ID (FK) | File_Name | Size | Checksum | Timestamp
1            | 001         | EJ_001.ejf| 1024 | A1B2C3D4 | 2026-05-01
2            | 001         | EJ_002.ejf| 2048 | E5F6A1B2 | 2026-05-02
```

### جدول Sync History
```
Sync_ID (PK) | ATM_ID (FK) | Start_Time | End_Time | Status | Files_Count
1            | 001         | 12:00:00   | 12:05:00 | Success| 5
2            | 001         | 12:30:00   | 12:35:00 | Success| 3
```

---

## 🎯 معايير الأداء

| المقياس | الهدف |
|---------|-------|
| وقت الاتصال الأولي | < 2 ثانية |
| سرعة المزامنة | > 1 MB/s |
| معدل الأخطاء | < 0.1% |
| توفر النظام | > 99.9% |
| وقت الاستجابة | < 500 ms |

---

## 🔧 التقنيات والمكتبات

| التقنية | الإصدار | الاستخدام |
|---------|---------|----------|
| .NET Framework | 4.8 | الإطار الأساسي |
| Windows Forms | - | واجهات المستخدم |
| System.Net.Sockets | - | الاتصال بالشبكة |
| System.Security.Cryptography | - | التشفير |
| System.IO.Compression | - | ضغط البيانات |
| SQLite | 3.x | قاعدة البيانات |

---

## 📋 خطة التطوير المرحلي

### المرحلة 1: البنية التحتية (الأسبوع 1)
- تطوير مكتبات الأساس والخدمات المشتركة
- نظام التشفير والضغط
- نظام الاتصال TCP/IP الأساسي

### المرحلة 2: تطبيق العميل (الأسبوع 2)
- واجهة رئيسية مع تبويبات
- معالج الجورنال لـ NCR/GRG/WN
- مدير الشبكة والمزامنة
- معالج الأوامر البعيدة

### المرحلة 3: تطبيق الخادم (الأسبوع 3)
- واجهة رئيسية مع تبويبات
- خادم TCP/IP متقدم
- مدير الأرشفة
- محلل الجورنال والتقارير

### المرحلة 4: لوحة المراقبة (الأسبوع 4)
- لوحة المعلومات الحية
- خريطة الشبكة البصرية
- نظام التنبيهات

### المرحلة 5: الإنهاء والاختبار (الأسبوع 5)
- برنامج التثبيت الموحد
- الاختبار الشامل
- التوثيق والتسليم
