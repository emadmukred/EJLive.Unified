# 🔴 إصلاح System.Data.SQLite الآن — Fix NOW

## ⚡ الحل السريع (30 ثانية):

### الطريقة 1: Package Manager Console (الأفضل)

1. **في Visual Studio**, اضغط على:
   ```
   Tools → NuGet Package Manager → Package Manager Console
   ```

2. **اكتب هذا الأمر:**
   ```powershell
   Install-Package System.Data.SQLite.Core -Version 1.0.118.0
   ```

3. **اضغط: Enter**
   - سينزل الحزمة تلقائياً

4. **اكتب الأمر الثاني:**
   ```powershell
   Install-Package System.Data.SQLite -Version 1.0.118.0
   ```

5. **اضغط: Enter** مرة أخرى

6. **ثم اضغط: Ctrl+Shift+B للبناء**

---

## 🔄 الحل الكامل (إذا فشل الأول)

```powershell
# انسخ هذا كله وألصقه في Package Manager Console:

Clear-Host
Remove-Item -Path .\packages -Recurse -Force -ErrorAction SilentlyContinue
Update-Package -Reinstall -RepositoryPath .\packages

# بعد الانتهاء اضغط: Ctrl+Shift+B
```

---

## ⚠️ إذا قال "Online sources not available"

1. **تأكد من الإنترنت** ✅
2. **اذهب إلى**: Tools > Options > NuGet Package Manager > Package Sources
3. **تأكد من وجود**: `https://api.nuget.org/v3/index.json`
4. **إذا لم يكن موجود**:
   - اضغط: Add
   - Name: `NuGet.org`
   - Source: `https://api.nuget.org/v3/index.json`
   - اضغط: OK

---

## ✅ علامات النجاح

بعد تشغيل الأوامر، يجب أن تظهر:
```
Successfully installed 'System.Data.SQLite.Core'
Successfully installed 'System.Data.SQLite'
```

ثم اضغط: **Ctrl+Shift+B**

يجب أن تظهر:
```
========== Build: 7 succeeded, 0 failed ==========
```

---

## 🎯 الملفات المرفقة للمساعدة

- `INSTALL_NUGET_CONSOLE.txt` — أوامر مفصلة
- `RESTORE_NUGET.bat` — سكريبت بديل
- `FIX_AND_BUILD.bat` — إصلاح شامل

---

**الآن جرب هذا بسرعة!** ⚡
