# EJLive Enterprise - ملخص التنفيذ التراكمي

## النسخة المعدلة

تم العمل على نسخة:

`C:\Users\user\Desktop\EJLIVE\EJLive_Enterprise_v3.4.0_Enhanced_Work`

## الخدمات الموجودة والمحسنة والجديدة

| الخدمة | الحالة | الوظيفة |
|---|---|---|
| FileWatcherEngine | محسنة ومضافة للبناء | مراقبة ملفات الجورنال حسب نوع الصراف NCR/GRG/WN مع القراءة عبر FileShare.ReadWrite لتقليل مشاكل قفل الملفات |
| NetworkEngine | محسنة | اتصال العميل بالسيرفر، Handshake، Heartbeat، إرسال الجورنال والملفات، استقبال أوامر الريموت |
| EJServer | محسنة جذريا | استقبال اتصالات TCP، إرسال ACK، استقبال الجورنال بصيغة framed/base64، وضع الرسائل في ConcurrentQueue، ومعالجتها عبر Workers |
| JournalSyncTracker | جديدة | تتبع دورة حياة الجورنال: LocalSaving, Pending, Syncing, ReSyncing, StoredOnServer, Completed, Failed |
| Journal Sync Server Tab | جديدة | شاشة تفاعلية في السيرفر تعرض ATM، الملف، الحجم، الحالة، النسبة، عدد المحاولات، الرسالة، ومسار التخزين |
| Journal Sync Client Tab | جديدة | شاشة تفاعلية في العميل تعرض حالة إرسال الجورنال وتأكيد السيرفر |
| ImageSyncEngine | مفعلة في البناء | مراقبة مجلد صور مشترك حسب نوع الصراف وتجهيز قوائم مزامنة الصور |
| GhostRemoteEngine | مفعلة في البناء | دعم الالتقاط view-only بدون قفل شاشة الصراف أو تسجيل خروج العميل |
| TransactionAnalysisEngine | مفعلة في البناء | تحليل الجورنال واستخراج عمليات السحب، الاستعلام، البطاقات المحتجزة، الأخطاء، وحالة النقد |
| ReportExportEngine | مفعلة في البناء | تصدير CSV/HTML للعمليات والتنبيهات وسجل المزامنة |
| Remote Commands | محسنة | أوامر Restart, Screenshot, TimeSync, Shutdown, Image Sync, Force Sync أصبحت مهيأة عبر البروتوكول |

## آلية التراسل Client ↔ Server

1. العميل يفتح TCP إلى السيرفر.
2. العميل يرسل:
   `EJLIVE_HANDSHAKE|ATM_ID|ATM_TYPE|VERSION`
3. السيرفر يرد:
   `EJLIVE_ACK`
4. العميل يرسل Heartbeat دوريا:
   `HEARTBEAT|ATM_ID|TIMESTAMP`
5. العميل يرسل الجورنال كسطر framed:
   `EJDATA|ATM_ID|FILENAME|SIZE|CHECKSUM|UTC_TIMESTAMP|BASE64_PAYLOAD|SYNC_ID`
6. السيرفر يضع الرسالة في `ConcurrentQueue` ثم يحفظها في:
   `StoragePath\ATM_ID\yyyy-MM\FileName`
7. السيرفر يرد:
   `DATA_ACK|SYNC_ID|STORED|FILENAME`
8. العميل يربط ACK بالـ `SYNC_ID` ويحول الحالة إلى Completed.

## ما تم إصلاحه في البناء

- إدخال ملفات `EJLive.Core\Engine` و`EJLive.Core\Models` في `EJLive.Core.csproj`.
- إصلاح مسار `ATMInfo.cs` داخل المشروع.
- إزالة مرجع `ClientMainForm.Designer.cs` غير الموجود.
- إضافة توافق `Constants` و`ATMType` والقيَم الناقصة للحفاظ على الخدمات القديمة.
- رفع نسخة التطبيق إلى `3.4.1 Enhanced`.
- البناء النهائي ينجح بلا أخطاء أو تحذيرات عبر:
  `dotnet build EJLive.sln -v:minimal`

## فجوات كبرى مؤجلة لخارطة الطريق

هذه لا ينبغي تنفيذها كترقيع سريع داخل WinForms فقط:

- نقل تخزين السيرفر المركزي من ملفات/SQLite إلى PostgreSQL أو Elasticsearch عند إدارة آلاف الصرافات.
- Mutual TLS وشهادات رقمية لكل صراف.
- MFA للأوامر الحساسة مثل Shutdown وGhost.
- فصل السيرفر إلى Windows Service حقيقي مع واجهة مراقبة مستقلة.
- Multi-Tenancy لعزل بيانات أكثر من بنك.
- Active-Active clustering وLoad Balancer.
- توقيع زمني خارجي RFC 3161 وسجل تدقيق immutable.
