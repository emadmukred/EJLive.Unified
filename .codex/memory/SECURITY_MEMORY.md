# Security Memory

## Non-negotiable safety rules

Do not implement:
- stealth installation
- zero-footprint cleanup
- disabling Windows Defender
- disabling Firewall
- log wiping
- hidden arbitrary terminal
- unauthorized RDP shadow/no-consent control
- forced password changes without explicit authorization

## Required safe alternatives

- authorized Windows service
- admin-visible configuration
- role-based access control
- command allowlist
- approval workflow for sensitive commands
- audit logging before and after command execution
- encryption and authentication
- feature flags and policy controls
- safe uninstall and rollback

## Sensitive areas

Treat these as high-risk:
- Windows service installation
- registry writes
- firewall changes
- password workflows
- RDP workflows
- screenshot capture
- command execution
- remote operations
- file sync to ATM application paths
