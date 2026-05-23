# EJLive Multi-Vendor Log Scope

## Active vendor scope
The project should remain open and extensible for these ATM/vendor families:
- NCR
- GRG
- WN (Wincor / Wincor Nixdorf)
- Hyosung

## Current reference evidence available in the project context
### NCR
Available references now include:
- NCR status/error message guides
- NCR diagnostic status code notebook
- APTRA Advance XFS overview
- sample NCR logs already present in agent files

### GRG
Available references now include:
- GRG EJ log manual
- sample GRG logs already present in agent files

### WN
Not yet provided as sample logs in the current project files, but the architecture must remain ready to add WN-specific patterns as soon as examples arrive.

### Hyosung
Not yet provided as sample logs in the current project files, but the architecture must remain ready to add Hyosung-specific patterns as soon as examples arrive.

## Implementation rule for the current project
Do not hard-code the parser, state model, or monitoring flow around a single vendor only.

Instead, keep the design split into:
1. shared normalized event model
2. vendor-specific parsing rules
3. vendor-specific error/status dictionaries
4. shared dashboard and analytics outputs

## Practical parsing targets
The system should be able to normalize at least these categories across vendors:
- startup / powerup / offline / out-of-service / in-service / maintenance transitions
- transaction start / end / serial number
- host request / host reply next state
- dispense command / cassette target / dispense result
- cash deposit start / count notes / rollback / encash success / retract
- cassette status snapshots and cassette state changes
- note OCR / serial capture lines
- card capture / card reader failures
- receipt and journal printer failures
- customer timeout flows such as take card timeout and take cash timeout
- network disconnect / reconnect transitions
- hardware fault codes and XFS/SPXFS related device/module statuses

## Current interpretation policy
- NCR parsing should use the NCR status-message and diagnostic-code references already supplied.
- GRG parsing should use the GRG EJ flow manual and sample journal patterns already supplied.
- WN and Hyosung should remain first-class planned vendors, not afterthoughts, even before their samples arrive.

## Next data expected from the user
When available, add:
- WN sample logs
- Hyosung sample logs
- any additional vendor-specific XFS/SP manuals

These should be mapped into the same normalized pipeline rather than creating a separate monitoring system per vendor.
