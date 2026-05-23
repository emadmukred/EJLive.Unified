# EJLive Strict Project Rules for Codex

## Project priority
The current active source is the source of truth. Documentation and legacy/reference files are secondary until mapped and promoted safely.

## Layer ownership
- `EJLive.Client.Service`: headless ATM agent runtime only.
- `EJLive.Client.WinForms`: local admin/config companion only; no production analytics.
- `EJLive.Server.*`: command orchestration, ingestion, archive, operator UI.
- `EJLive.Monitoring.*`: dashboards and read-only operational views.
- `EJLive.Core`: protocols, parsers, XFS normalization, data contracts, transfer, security, migrations.
- `EJLive.Tests`: unit/integration fixtures and regression tests.
- `EJLive.Verification`: gates that fail unsafe architecture.

## UI/backend separation
Any code in WinForms event handlers that performs network receive/send, file transfer, journal parsing, archive writing, remote command execution, or database-heavy operations must be moved to a service/worker/queue layer.

## Journal parsing rule
Do not use one parser for all vendors. Use `IEjTransactionParser` + `EjParserRegistry` with vendor-specific implementations.

## XFS rule
All XFS/vendor logs must be normalized to `NormalizedXfsEvent` before correlation.

## Remote administration rule
Remote control capabilities are permitted only as audited enterprise administration. No stealth, no disabling security tooling, no arbitrary shell execution, no credential capture, no plaintext password logging.
