# EJLive Debug Log Analysis

## Scope
This note summarizes the newly attached debug logs:
- `CARDREAD_1.log`
- `CARDREAD_15.log`
- `MESSAGEIN_1.log`
- `MESSAGEIN_15.log`
- `MESSAGEOUT_1.log`
- `MESSAGEOUT_15.log`

## Primary insight
These files reveal an additional operational layer that sits **between raw XFS/device status and high-level business journal logs**:
1. **card-reader decision and fitness flow**
2. **host message ingress**
3. **host message egress**

This means the full platform should model at least five telemetry layers:
- root/static configuration layer
- XFS/device diagnostic layer
- business journal layer
- host protocol/message layer
- sync/archive/monitoring layer

## 1. CARDREAD logs

### What the files show
The `CARDREAD` logs contain highly structured card-reader trace events such as:
- `Primary card reader success`
- `Card Entered`
- `Card Found in Primary Reader`
- `Read Condition 2`
- `FIT Search Required in Smart Fit Check State`
- `Good Match in Card Read State`
- aggregated counters such as:
  - `Good Read`
  - `Error Mis`
  - `Read Cond 1/2/3`
  - `Card Return`
  - `Not FIT`
  - `Camera Opt`
- card acceptor fitness values
- card-reader synchronizer state

### What this means for the project
The current project needs a dedicated **Card Reader Diagnostic / Fitness parser** distinct from:
- generic business journal parsing
- NCR status message decoding

This parser should normalize at least:
- reader path selection (primary vs secondary)
- card entered / card found / waiting for card
- read condition codes
- FIT search and FIT match outcomes
- reader fitness values
- summary counters for the card-read cycle

### Immediate design implication
Add a future parser family such as:
- `CardReaderTraceAdapter`

and normalized event kinds such as:
- `CardReaderFlow`
- `CardReaderFitness`
- `CardReaderStatistics`

## 2. MESSAGEIN logs

### What the files show
The `MESSAGEIN` files are periodic inbound host traffic traces with entries like:
- timestamp
- sequence/counter number
- short bracketed payload lengths / codes
- repeated inbound patterns every ~3 minutes

### What this likely means
These are not customer business journal lines. They are closer to:
- host keepalive / poll / supervisory traffic
- inbound network application messages
- protocol heartbeat or NDC/DDC traffic capture

### What this means for the project
The platform should maintain a **message transport analysis layer** that can answer:
- Is the ATM still receiving host traffic?
- What is the cadence of inbound polling?
- Has inbound traffic stopped before the ATM went offline?
- Are sequence numbers monotonic and healthy?

### Immediate design implication
Add a future parser family such as:
- `HostMessageInAdapter`

and normalized event kinds such as:
- `HostMessageInbound`
- `HostPollingCycle`
- `ProtocolKeepalive`

## 3. MESSAGEOUT logs

### What the files show
The `MESSAGEOUT` files are periodic outbound traces with repeated payload blocks such as:
- message length markers
- class/type markers
- payload fragments like `LA050100` and `BG531-0283`
- consistent outbound cadence matching inbound traces

### What this likely means
These appear to be:
- host response messages
- ATM outbound protocol messages
- likely NDC/DDC framing or supervisory exchange artifacts

### What this means for the project
The platform should correlate:
- outbound message cadence
- inbound message cadence
- business journal transaction state
- connectivity state

This gives stronger answers to questions like:
- Did the ATM stop sending before it stopped receiving?
- Was the ATM still alive at the transport layer even though no journal sync happened?
- Is a sync outage caused by file flow, business app flow, or protocol transport degradation?

### Immediate design implication
Add a future parser family such as:
- `HostMessageOutAdapter`

and normalized event kinds such as:
- `HostMessageOutbound`
- `ProtocolResponse`
- `TransportSessionActivity`

## 4. Key project improvements implied by these logs

### A. Add protocol telemetry to the monitoring model
The current project should not only monitor:
- last journal sync
- last XFS fault
- last business event

It should also monitor:
- last inbound protocol activity
- last outbound protocol activity
- host poll interval health
- message sequence continuity

### B. Add card-reader health/fidelity visibility
The current project should track more than card faults. It should also expose:
- reader fitness
- FIT match success/failure rates
- read condition distributions
- card return and not-fit counts
- primary vs secondary reader path behavior

### C. Add a transport-health dimension to ATM status
Every ATM card or dashboard detail view should eventually show:
- host traffic inbound recent?
- host traffic outbound recent?
- business transactions recent?
- journal sync recent?
- XFS/device faults present?

This prevents false conclusions when one layer is alive but another is failing.

## 5. What the current project is still missing after this analysis
- no dedicated `CARDREAD` parser
- no `MESSAGEIN` parser
- no `MESSAGEOUT` parser
- no transport telemetry model
- no card-reader fitness counters/dashboard widgets
- no inbound/outbound protocol timestamps in ATM status

## 6. Recommended next implementation order
1. add normalized event kinds for host transport and card-reader diagnostic flow
2. add adapters for `CARDREAD`, `MESSAGEIN`, and `MESSAGEOUT`
3. add transport freshness fields to ATM status/dashboard models
4. surface these insights in Server dashboards and ATM details
5. correlate protocol activity with journal sync delay alerts

## 7. Important conclusion
These debug files confirm that the real EJLive platform must not be limited to journal-file parsing. A production-grade monitoring platform needs to unify:
- device/XFS state
- journal business flow
- host message transport
- sync/archive state
- static vendor/root configuration

Only that combination will make the platform truly real-time and operationally reliable.
