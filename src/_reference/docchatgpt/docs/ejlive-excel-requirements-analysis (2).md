# EJLive Excel Requirements Analysis

## Sources analyzed
- `Central ATM Monitoring Sys.xlsx`
- `Central ATM Monitoring System (1).xlsx`
- `Central ATM Monitoring System.xlsx`
- `Central Monitoring - Copy (2).xlsx`
- `Central Monitoring Ysys Requred.xlsx`

## Consolidated requirement themes

### 1. Central monitoring and dashboard
The Excel files consistently describe a central monitoring product with these core capabilities:
- Real-time device status monitoring
- Device fault timeline monitoring
- Real-time cassette status monitoring
- Real-time transaction monitoring
- Maintenance and CIT notifications
- Dashboard views for operations and transaction follow-up
- Terminal classification by customer, vendor, and network
- Remote control and reporting

### 2. Transaction and inquiry features
Repeated requirements across the sheets:
- Daily EJ download and transaction file ingestion
- Normal and abnormal transaction details
- Account and customer detail lookups
- Suspect transaction tracking
- Inquiry by serial number / OCR
- Inquiry by journal number
- Inquiry by card number
- Inquiry by account number
- Transaction inquiry by journal

### 3. Alarming and operational alerts
Common alarm requirements:
- Lifecycle replacement alarms for parts
- Kiosk hardware error alarms
- Failed transaction alarms
- Cassette status alarms
- Supervisor mode alarms
- Kiosk error and jam timeline history

### 4. Regional and configuration management
The monitoring workbook also requests:
- Province definition
- Region definition
- City definition
- Geography definition
- System configuration
- Hardware parameter configuration

### 5. Terminal maintenance module set
From the bilingual sheets in `Central Monitoring - Copy (2).xlsx`, the terminal maintenance area should include:
- Terminal management
- Command management
- Stop-plan maintenance
- Terminal manager maintenance
- Real-time monitoring matrix view
- Real-time monitoring list view

### 6. System maintenance module set
Required administration features:
- Organizational management
- Menu management
- Authority management
- User management
- User log
- User login log
- Module viewing log
- User operation log
- Task run log
- Personal settings and password editing
- Login parameter management

### 7. Cash and replenishment visibility
The monitoring tables indicate expected inventory/cash visibility fields:
- Cass1
- Cass2
- Cass3
- Cass4
- Rem
- Loaded
- IN (Deposit)
- OUT (Dispense)
- Reject
- Retract
- Total

This should drive both monitoring UI and receipt/report reconciliation logic.

### 8. Customer and maintenance issues still relevant
The issue sheets still highlight implementation gaps that should be treated as backlog or validation targets:
- Offline maintenance mode support
- Safer hardware reset and status refresh flow
- Simpler user-facing maintenance messages
- Currency denomination visibility during deposit/dispense flows
- Startup device health summary
- Prevent duplicate actions while "please wait" is active
- Auto-show cash unit table in maintenance mode
- Deposit reconciliation mismatches between accepted / rejected / receipt totals
- Better handling of first deposit attempts and in-progress operation errors

### 9. Integration and security flow requirements
The 01-04-2022 sheet adds integration work items:
- JWT token with expiry handling
- Reuse token from kiosk file storage
- Regenerate token on expiry and retry failed API call
- Post-withdraw acknowledgement flow after customer success screen and receipt
- OTP activation flow
- WAITING FOR ACTIVATION login state
- New password / confirm password flow with encrypted PIN

## Immediate implementation guidance for current project structure

### Client application (`EJLive.Client.WinForms`)
Most directly aligned additions:
- Add startup device health summary block
- Improve transaction/journal filters to support abnormal/suspect/error-focused views
- Add visible guard state during long-running operations to prevent repeated clicks
- Prepare deposit reconciliation model fields for accepted/rejected/retract totals
- Add token state and activation-state handling hooks in customer flow

### Server application (`EJLive.Server.WinForms`)
Most directly aligned additions:
- Expand connection/terminal list refresh to represent real terminals and their live status
- Add alarm aggregation and notification summary support
- Add region/customer/vendor/network grouping metadata support
- Add storage/reporting hooks for transaction and terminal monitoring datasets
- Add remote command categories that map to terminal maintenance requirements

### Monitoring application (`EJLive.Monitoring.WinForms`)
This appears to be the biggest gap relative to the Excel requirements and should become the main delivery surface for:
- Monitoring dashboard
- Matrix/list terminal monitoring
- Alarm center
- Transaction tables
- Replenishment/cash dashboard
- Regional configuration views
- Reporting and inquiry screens

## Recommended delivery order
1. Build monitoring data contracts and terminal summary model
2. Add dashboard + terminal list/matrix views
3. Add alarm center and cassette/replenishment metrics
4. Add transaction inquiry and abnormal/suspect transaction views
5. Add regional/configuration management screens
6. Add administrative modules (users, roles, logs, login settings)
7. Complete customer-flow fixes and JWT/OTP integration items

## Notes on source quality
- The five Excel files are largely overlapping; `Central Monitoring - Copy (2).xlsx` contains the clearest functional decomposition.
- The issue sheets mix completed and incomplete work, so they should be used as validation targets, not as proof that functionality already exists.
- Several features refer to modules not visible in the currently staged source files, especially the monitoring project. Those should be implemented from the agent file tree before claiming completion.
