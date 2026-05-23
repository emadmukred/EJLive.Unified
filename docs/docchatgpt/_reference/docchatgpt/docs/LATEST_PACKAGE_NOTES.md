# EJLive Latest Package Notes

Date: 2026-05-10

This package is a refreshed test copy of the active EJLive tree with the
latest grounded memory-alignment work reflected into the downloadable build.

## What was aligned in this package

- `EJLive.sln` now reflects the six visible projects:
  - `EJLive.Core`
  - `EJLive.Shared`
  - `EJLive.Client.WinForms`
  - `EJLive.Server.WinForms`
  - `EJLive.Monitoring.WinForms`
  - `EJLive.Installer.WinForms`
- The client project file was normalized to:
  `EJLive.Client.WinForms/EJLive.Client.WinForms.csproj`
- Solution GUIDs and project-reference GUIDs were aligned across the active
  solution and project files.
- The shared project now compiles the shared monitoring/security files already
  present on disk:
  `MonitoringState.cs`, `MonitoringStateStore.cs`, `SecurityHelper.cs`
- The core project now compiles the visible engine/model/service/XFS files that
  form the current grounded backbone of the project tree.
- The server project now includes `JournalSyncDashboardForm.cs`.
- `JournalSyncDashboardService` now supports the two-argument construction path
  already used by `ServerMainForm`.

## Important scope note

This package reflects the latest grounded state that could be safely aligned
from the active project tree and memory notes inside the current environment.
Some older memory notes described further runtime slices that are not fully
visible on disk in this workspace; those were not invented into this package.

## Local test recommendation

1. Open `EJLive.sln` in Visual Studio 2019 or 2022 on Windows.
2. Run `Clean Solution`.
3. Run `Build Solution`.
4. Report the first compile errors, if any, so the next correction wave can be
   applied directly on the same branch of work.

## Expected focus for the next correction wave

- compile-time errors from Windows/MSBuild
- any remaining namespace or constructor mismatches
- missing package/reference issues
- UI/runtime issues discovered during local test execution
