# INSTALL_TO_PROJECT_FIXED.ps1

Use this fixed installer instead of running the old script without a target path.

Example:

```powershell
cd C:\Users\user\Downloads\EJLive_Codex_Extended_Skills_Plugins_Pack
Set-ExecutionPolicy -Scope Process -ExecutionPolicy Bypass
.\INSTALL_TO_PROJECT_FIXED.ps1 -ProjectRoot "C:\Users\user\Desktop\EJLive.Unified"
```

Replace the project path with your real EJLive repository root.
Do not set `ProjectRoot` to the extracted pack folder itself.
