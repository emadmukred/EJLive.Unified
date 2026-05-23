# EJLive MergedTrace Analysis

## Scope
This analysis covers the attached NCR merged trace bundle:
- `MergedTrace_14.log`
- `MergedTrace_16.log`
- `MergedTrace_20.log`

## Main conclusion
`MergedTrace` confirms that NCR monitoring in the project must include not only:
- business journal flow
- device status messages
- card reader traces
- message in/out traffic

but also a distinct **OOXFS runtime and DEBUG orchestration layer**.

This layer exposes the live internal behavior of the ATM application stack:
- XFS session initialization
- service open / register flows
- async GetInfo calls
- terminal state messaging
- virtual controller chains
- customisation layer state
- UPS / heartbeat / supervisory health

## 1. OOXFS findings

The OOXFS traces show repeated patterns such as:
- `WFSOpen()=0`
- `WFSVERSION (SPI)`
- `WFRegister(...)`
- `GetInfo(...)`
- `GetInfoAsync(...)`
- `WFS_GETINFO_COMPLETE`
- `WFSCancelAsyncRequest(...)`
- XFS commands such as:
  - `WFS_INF_SIU_STATUS`
  - `WFS_INF_VDM_CAPABILITIES`
  - `WFS_INF_VDM_STATUS`

### Implications
The platform needs a parser and model for:
- XFS service/session lifecycle
- async request health
- capabilities/status polling behavior
- cancel/failure anomalies
- service handle mapping
- command-level freshness and timing

### New concepts to model
- XFS session opened
- service registered
- GetInfo pending/completed
- repeated polling cadence
- cancel request success/failure
- device capability query versus device status query

## 2. DEBUG trace findings

The DEBUG traces show the ATM middleware/orchestration flow around host commands:
- `Start:Virtual controllers processing`
- `Inbound virtual controller dll: ...`
- `Outbound virtual controller dll: ...`
- `Processing Message - Terminal Command`
- `Validating message...`
- `Deformatting message...`
- `Customisation Layer Status: [IDLE]`
- `Software ID and Release No. Request received`
- `Message Handler - Terminal State response sent`
- `Message Handler - awaiting incoming message...`
- `UPS - NOT AVAILABLE`
- periodic heartbeat activity:
  - `CL about to send heartbeat`
  - `CL just sent heartbeat`

### Implications
The platform needs a dedicated interpretation layer for:
- host command handling lifecycle
- message validation / deformatting / forwarding path
- virtual controller chain execution
- customisation layer state machine
- terminal state reporting path
- UPS and heartbeat telemetry

## 3. Operational value for the project

With these traces, the project can eventually answer:
- Did the host message reach the ATM application layer?
- Did the virtual controllers process it correctly?
- Did the ATM validate and deformat it?
- Did XFS status polling succeed at the same time?
- Was a failure in the journal caused by transport, middleware, XFS, or hardware?
- Was the ATM still alive from a heartbeat point of view even when transaction flow stalled?

## 4. Missing platform capabilities now clearly identified
The current project still lacks explicit support for:
- `OoxfsRuntimeAdapter`
- `DebugTraceAdapter`
- XFS polling cadence metrics
- virtual controller execution telemetry
- terminal command lifecycle telemetry
- UPS / heartbeat operational metrics in the dashboard

## 5. Recommended normalized event categories to add
The normalized XFS model should grow to include:
- `XfsSessionLifecycle`
- `XfsGetInfoCycle`
- `XfsCapabilitiesQuery`
- `XfsStatusPolling`
- `VirtualControllerInbound`
- `VirtualControllerOutbound`
- `TerminalCommandLifecycle`
- `MiddlewareValidationFlow`
- `HeartbeatTelemetry`
- `UpsState`

## 6. Monitoring/dashboard implications
The server dashboards should eventually expose:
- last inbound protocol message
- last outbound protocol message
- last XFS poll success
- last virtual controller processing cycle
- heartbeat cadence health
- UPS availability flag
- customisation layer state
- terminal state query freshness

## 7. Most important takeaway
The real system is not just:
- logs
- sync
- XFS statuses

It is an application platform with a middleware chain. `MergedTrace` proves that observability must include:
- network transport
- host message lifecycle
- virtual controller routing
- XFS runtime interactions
- business journal outputs
- hardware/device status

Any production-grade EJLive monitoring platform should unify these layers into one operator view.
