# Coder01 vs Current EJLive Draft

## What the attached reference project adds
The attached `Coder01.zip` contains a much more complete EJLive solution than the current draft. The main missing areas identified from the comparison are:

### 1. Missing source trees in the current draft
The current draft is missing large parts of the source tree that are present in `Coder01`, especially:
- `EJLive.Core`
- `EJLive.Shared`
- `EJLive.Installer.WinForms`
- `EJLive.Server.WinForms` supporting forms and services
- `EJLive.Client.WinForms` supporting controls, designer files, and advanced services

### 2. Client-side gaps
The attached project includes additional client artifacts that the current draft does not yet have:
- `ClientMainForm.Designer.cs`
- reusable controls under `Controls/`
- advanced client services such as:
  - `AdvancedJournalProcessor.cs`
  - `AdvancedNetworkManager.cs`
  - `AdvancedRemoteCommandHandler.cs`
  - `WindowsRemoteAccessService.cs`
  - `WindowsStartupService.cs`
- richer client constants and packaging files

### 3. Core domain and engine gaps
The attached project includes a more complete core layer with:
- richer `Constants.cs`
- `TransactionModels.cs`
- `JournalSyncModels.cs`
- `ATMInfo.cs`
- shared protocol/engine files:
  - `CommunicationProtocol.cs`
  - `FileWatcherEngine.cs`
  - `GhostRemoteEngine.cs`
  - `ImageSyncEngine.cs`
  - `JournalOutbox.cs`
  - `JournalSyncTracker.cs`
  - `NetworkEngine.cs`
  - `ReportExportEngine.cs`
  - `ServerEngine.cs`
  - `TransactionAnalysisEngine.cs`
- service layer files such as alerting, auditing, DB, and RBAC

### 4. Server-side gaps
The attached project has richer server UI and support forms including:
- `ServerMainForm.Designer.cs`
- `ATMCardPanel.cs`
- `ATMDetailDrawerForm.cs`
- `JournalViewerForm.cs`
- `EJServerService.cs`
- analytics and remote control services

### 5. Monitoring/UI gaps
The attached project also includes both:
- `EJLive.Monitor`
- `EJLive.Monitoring.WinForms`
which confirms that the intended product surface is broader than the minimal dashboard currently staged in the draft.

## Recommended import priority
1. Restore `EJLive.Core` and `EJLive.Shared` first
2. Restore client designer/supporting files next
3. Restore server designer/supporting forms and services
4. Compare monitoring modules and fold in missing dashboard/UI behavior
5. Reconcile package references and fix compile mismatches

## First transfer batch completed in this turn
This turn starts the restoration of critical missing foundation files:
- `EJLive.Core/EJLive.Core.csproj`
- `EJLive.Core/Constants.cs`
- `EJLive.Core/Models/TransactionModels.cs`
- `EJLive.Shared/EJLive.Shared.csproj`
- `EJLive.Client.WinForms/ClientMainForm.Designer.cs`

## Important caution
The attached project uses additional package references such as SQLite and more complete engine/service dependencies. Importing UI files without restoring those foundations would leave the solution inconsistent, so the restoration should continue in dependency order rather than copying designer files alone.
