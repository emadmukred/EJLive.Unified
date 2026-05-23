# EJLive Working Memory

## Current State

- The unified solution is located at `EJLive.Unified/EJLive.Unified.sln`.
- The solution also has a valid `EJLive.Unified/EJLive.Unified.slnx`.
- The solution builds successfully in Debug and Release with `dotnet build`.
- The unified build uses `.NET 8` / `net8.0-windows` for WinForms projects.
- Original files are preserved under `EJLive.Unified/legacy/original`.
- The preservation project is `src/EJLive.LegacyReference`.
- `EJLive.LegacyReference` also links root artifacts, `tools`, `docs`, `src/EJLive.Server`, and `src/EJLive.Setup` as non-compiling reference content.
- `src/EJLive.Monitor` is included in both `.sln` and `.slnx` and builds as `EJLive.Monitor.Legacy`.
- Five attached zip packages were extracted and studied in `_codex_zip_study_20260515_053548`.
- `FixedGpt`, `LatestWorkspace`, and `ActiveTestPackage` have valid project XML; Replit packages have richer source volume but invalid project XML.
- `Coder01.zip` was filtered into `_coder01_source_study`; useful Coder01 UI patterns were integrated into the unified server/client forms.
- The Visual Studio SQLite crash `no such column: created_at_utc` was fixed with schema migration before index creation.
- A local network probe confirmed `NetworkEngine` connects to `ServerEngine` and server accepts `ATM-SMOKE`.
- `EJLive.Verification` now validates SQLite migration, client/server network, remote command routing, file watcher fallback, WinForms UI composition, and project file linkage.
- `tools/Audit-ProjectFileLinkage.ps1` regenerates `docs/09-file-function-inventory.csv`; the latest inventory classifies 402 non-build-output files with 0 unaccounted files.

## Architectural Decisions

- Keep original files verbatim as non-compiling content instead of editing them in place.
- Use SDK-style projects for stable Visual Studio loading and modern build behavior.
- Consolidate duplicated model contracts into `EJLive.Core.Models.UnifiedModels`.
- Consolidate operational services into `EJLive.Core.Services.CoreServices`.
- Consolidate engine behavior into focused files under `EJLive.Core.Engine`.
- Keep UI labels and buildable form text in English.
- Prefer incremental improvements to `EJLive.Unified` over replacing it with any attached package because `EJLive.Unified` builds cleanly and preserves legacy code.
- Use Coder01 as a UX and feature reference, not as a direct replacement for the unified project.
- Keep sensitive remote commands non-destructive until a production execution policy is explicitly approved.
- Compile only the canonical unified files in projects that have duplicate/incomplete legacy code; link old code as `reference-source` instead of compiling it.
- Link `EJLive.Setup.SetupWizardForm` into `EJLive.Installer.WinForms` and expose it through the `Setup Wizard` button.

## Next Useful Work

- Add integration tests for `JournalOutbox`, `DatabaseManager`, `CommunicationProtocol`, and `TransactionAnalysisEngine`.
- Replace placeholder UI actions with real service calls where deployment details are available.
- Validate XFS adapter parsing against real NCR/GRG/Diebold logs.
- Add environment-specific Windows service packaging if the deployment target requires services rather than desktop WinForms executables.
- Use `docs/05-attached-package-study.md` as the reference summary for decisions taken from the attached zip packages.
- Use `docs/06-coder01-study.md` for Coder01-specific UI and runtime integration notes.
- Use `docs/07-final-system-audit.md` for the final tab/feature/verification map.
- Use `docs/08-file-linkage-map.md` for the current file ownership and reference-source map.
- Use `docs/09-file-function-inventory.csv` for the per-file ownership/function manifest.
