# EJLive Monitoring Backlog Derived From Excel Requirements

## P0 - Core monitoring foundation

### P0.1 Terminal live summary model
Create a shared terminal summary model that can represent:
- Terminal Id / ATM name
- Customer / vendor / network classification
- Region / province / city / geography
- Online/offline state
- Last heartbeat
- Current fault state
- Supervisor mode state
- Cassette summary
- Last transaction summary
- Alert count

### P0.2 Real-time monitoring views
Implement two required views:
- Matrix view for many terminals at once
- List/detail view for operational drill-down

### P0.3 Dashboard widgets
Initial dashboard widgets should include:
- Online vs offline terminals
- Active faults
- Active supervisor mode terminals
- Cassettes needing replenishment
- Recent abnormal transactions
- Maintenance alerts
- CIT alerts

### P0.4 Cash status model
Add a canonical cash-status structure with these fields:
- Cass1
- Cass2
- Cass3
- Cass4
- Remaining
- Loaded
- Deposit In
- Dispense Out
- Reject
- Retract
- Total

## P1 - Alerts and transaction intelligence

### P1.1 Alarm center
Alarm categories to support:
- Part lifecycle replacement
n- Hardware errors
- Failed transactions
- Cassette status
- Supervisor mode
- Kiosk jam / timeline history

### P1.2 Transaction monitoring
Add transaction views for:
- Normal transactions
- Non-normal transactions
- Suspect transactions
- Card/cardless transactions
- Cash/cashless transactions
- Transfer / received transfer

### P1.3 Inquiry module
Support lookup by:
- OCR serial number
- Journal number
- Card number
- Account number

## P2 - Administration and configuration

### P2.1 System maintenance modules
- Organizational management
- Menu management
- Authority / privilege management
- User management
- Personal settings
- Password edit
- Login parameter management

### P2.2 Log management modules
- User log
- User login log
- Module view log
- User operation log
- Task run log

### P2.3 Regional configuration
- Province definition
- Region definition
- City definition
- Geography definition
- System configuration
- Hardware parameter configuration

## P3 - Customer and maintenance corrections from issue sheets
- Offline maintenance mode
- Stable hardware reset and refresh flow
- Simpler maintenance messages
- Currency denomination visibility in money operations
- Startup hardware health status
- Global busy-state / please-wait guard
- Auto-show cash unit table in maintenance mode
- Deposit reconciliation fixes
- First-transaction deposit stabilization
- Operation-in-progress error hardening

## P4 - Integration and security items
- JWT expiry-aware token lifecycle
- Token persisted locally and reused
- Retry on token expiry
- Post-withdraw acknowledgement callback flow
- OTP activation flow
- WAITING FOR ACTIVATION status handling
- Password reset / activation with encrypted PIN

## Mapping to current visible source files

### `EJLive.Client.WinForms/ClientMainForm.cs`
Best candidate for near-term work on:
- busy-state protection
- startup health summary
- richer journal filters
- customer-mode validation/status display

### `EJLive.Server.WinForms/ServerMainForm.cs`
Best candidate for near-term work on:
- richer terminal connection summary
- alarm summary area
- command grouping
- storage/report stats expansion

### `EJLive.Monitoring.WinForms/*`
Should become the main surface for the monitoring backlog above.

## Suggested first coding slice
1. Introduce shared terminal and cash summary models
2. Expand server-side terminal state tracking and alarm counters
3. Build monitoring dashboard shell with matrix/list switching
4. Add cassette/replenishment summary table
5. Add abnormal transaction and alert lists
