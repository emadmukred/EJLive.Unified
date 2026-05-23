# Known Gaps

## Build / project wiring

- Verify active solution/project graph before implementation.
- Check .sln/.slnx and .csproj references.
- Check duplicate compile includes.

## Sync

- Persistent sync state
- Client outbox
- Idempotency
- Offset tracking
- Chunked transfer
- Delivery confirmation

## Dashboard

- Live monitoring data instead of static values
- Journal sync timeline
- Alerts and NOC status colors
- Server-side reporting

## Security

- RBAC
- Audit log
- Safe command queue
- Policy-driven remote operations

## Installer

- Service registration
- Configuration validation
- Connection checks
- Rollback/uninstall
