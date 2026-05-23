# EJLive XFS / SPXFS Research Requirements

## Purpose
This document captures the practical engineering requirements extracted from the supplied XFS, NCR, and GRG references so the current EJLive project can evolve into a real multi-vendor ATM monitoring and log-analysis platform.

## 1. Required conceptual architecture

### 1.1 XFS core layer
From the APTRA Advance XFS overview, the project should model a standard XFS-style core with these concepts:
- device-independent application layer
- device/service abstraction layer
- device status management layer
- vendor-dependent extension layer
- maintenance/diagnostics access path
- exception handling / restart / fault recovery path

### 1.2 Vendor adapter layer
The project should not parse all logs through one monolithic parser. It should separate:
- `XfsCore` or equivalent normalized layer
- `NcrAdapter`
- `GrgAdapter`
- future `WnAdapter`
- future `HyosungAdapter`

### 1.3 Normalized output layer
All vendor parsers should emit one shared normalized model for:
- terminal lifecycle state
- transaction lifecycle
- device/module fault
- cassette state snapshot
- cash movement
- card capture / retract / timeout / printer events
- severity and operational impact
- engineer action recommendations

## 2. XFS concepts the current project must support explicitly

### 2.1 Standard device/service model
The architecture should stay ready to normalize at least these device families:
- card reader / writer
- cash dispenser
- depository / cash-in module
- receipt printer
- journal printer
- statement/passbook printer
- encryptor / EPP
- sensors / alarms / operator panel mode switch
- contactless card reader
- biometric devices where available
- miscellaneous interface and indicators

### 2.2 Device status management
From APTRA Advance XFS, the project should expose normalized methods or services for:
- listing device/module states
- mapping vendor status into device state
- converting low-level faults into actionable system alerts
- resetting or clearing module state when supported
- preserving device, module, state, and action descriptions for UI/reporting

### 2.3 Exception handling model
The project should distinguish between:
- application/business-flow failure
- device fault
- persistent hardware/module failure
- communication failure
- power/interlock fault
- customer timeout needing retract/capture flow

## 3. NCR-specific extraction requirements

## 3.1 NCR error status message format
The parser must understand NCR journal/status lines in the form:
- `*tttt*m*d*aa.a,M-bb,R-c`

Where the project should normalize:
- `tttt` = transaction serial number
- `m` = unsolicited vs solicited status
- `d` = device code
- `aa...` = transaction/device status
- `M-bb` = diagnostic module status
- `R-c...` = supply/bin/cassette status

### 3.2 NCR device code dictionary
The normalized dictionary should include at least:
- `A` time of day clock
- `B` power failure
- `D` card reader/writer
- `E` currency dispenser
- `F` depository
- `G` receipt printer
- `H` journal printer
- `L` encryptor
- `P` sensors
- `V` statement printer

### 3.3 NCR currency dispenser parsing
The project should implement a dedicated decoder for NCR device code `E` with:
- transaction outcome code from first digit
- bills dispensed per cassette from remaining transaction digits
- module status decoding (`M-bb`)
- supply state decoding (`R-ccccc`)

Key normalized outputs should include:
- requested vs dispensed notes
- partial dispense
- no dispense
- faulty dispense / quantity unknown
- retract/reject implications
- divert bin full state
- cassette low / empty / sufficient state

### 3.4 NCR diagnostic notebook integration
The project should preserve lookup tables for:
- `M_STATUS`
- `M_DATA` byte layouts
- sensor bit decoding
- cassette state bytes
- dispenser auxiliary statuses
- printer sensor/error bytes
- EPP/encryptor status codes
- contactless reader RF state/error code combinations

This should be stored as structured dictionaries, not only prose docs.

## 4. GRG-specific extraction requirements

### 4.1 GRG business journal flow model
The GRG EJ manual shows that the parser must treat GRG logs as business-flow journals, not only device diagnostics.

The system should recognize and normalize:
- `ATM POWER UP`
- `ENTER POWERUP MODE`
- `ENTER OFFLINE MODE`
- `ENTER OUTOFSERVICE MODE`
- `ENTER INSERVICE MODE`
- `ENTER MAINTENANCE MODE`
- `LINE UP`
- `LINE DOWN`
- `TRANSACTION START`
- `TRANSACTION START NOCARD`
- `TRANSACTION REQUEST`
- `TRANSACTION REPLY NEXT`
- `TRANSACTION SERIAL NUMBER`
- `DISPENSE COMMAND FROM HOST`
- `DISPENSE COMMAND TO CASSETTE`
- `DISPENSE SUCCESS`
- `DISPENSE FAIL`
- `PRESENT SUCCESS`
- `MONEY TAKEN`
- `TAKE CASH TIMEOUT, RETRACTING`
- `CARD CAPTURED: TAKE CARD TIMEOUT`
- `CASH DEPOSIT START`
- `START COUNT NOTES`
- `COUNT NOTES`
- `CASH TOTAL`
- `ENCASH SUCCESS`
- `CASH DEPOSIT RETRACT`
- `CIM RETRACT`
- `CDM RETRACT`
- `CASSETTE STATUS`
- `CASH UNIT CHANGED`
- `TRANSACTION RECEIPT DATA`

### 4.2 GRG cassette line format
The parser should treat lines like:
- `CAS(00010):0011/0000/ 10/CNY/EMPTY/RCYC/TYPE(A)`

As a structured cassette snapshot with fields:
- cassette id
- remaining count
- reject count
- denomination
- currency
- cassette state
- cassette class/type
- note type

### 4.3 GRG abnormal flow rules
The parser should classify GRG abnormal flows into at least:
- startup network disconnect
- in-service network disconnect
- cash-in network disconnect
- cash-out network disconnect
- card reader device error
- cash-in device error
- cash-out device error
- receipt printer error
- count limit exceeded
- cash allocation invalid
- physical dispense shortage
- reject limit reached
- take card timeout
- take notes timeout
- retract after timeout

## 5. SPXFS / device-integration implications for current project

The user noted that deep log analysis requires understanding of the SPXFS layer that interacts physically with ATM devices. Based on the supplied references, the project should therefore separate:
- **journal/business events**
- **XFS device status**
- **vendor-specific driver/module status**
- **low-level physical hardware interpretation**

This means each parsed event should ideally track:
- source layer: `business_journal`, `xfs_status`, `driver_error`, or `hardware_diag`
- vendor
- device family
- raw code(s)
- normalized meaning
- severity
- customer impact
- service impact
- suggested corrective action

## 6. Practical implementation targets for the current codebase

The current project should next gain these concrete artifacts:

### 6.1 Shared normalized models
Add models for:
- normalized ATM event
- normalized device fault
- normalized cassette snapshot
- normalized transaction step
- normalized timeout/retract/capture incident
- normalized parser evidence / raw line reference

### 6.2 Dictionary/lookup services
Add structured decoders for:
- NCR device codes
- NCR dispenser status/module/supply values
- NCR printer and card-reader status values
- NCR sensor/encryptor/contactless status values
- GRG business flow tokens and abnormal patterns

### 6.3 Parser services
Implement vendor-aware parsers such as:
- `NcrStatusMessageParser`
- `NcrDiagnosticCodeInterpreter`
- `GrgJournalFlowParser`
- future `WnJournalParser`
- future `HyosungJournalParser`

### 6.4 Unified orchestration layer
Implement a selector/router that:
- detects vendor from source/config/log style
- passes lines to the right parser
- merges outputs into one normalized stream

### 6.5 UI/reporting integration
The dashboard and analysis screens should eventually show:
- current service mode
- last successful transaction step
- active device faults by module
- cassette/supply state
- unresolved timeouts / retract / card-capture incidents
- severity and recommended action

## 7. What is still missing externally
To complete the four-vendor path properly, the project still needs:
- real WN sample logs
- real Hyosung sample logs
- any WN/Hyosung XFS or journal code references

## 8. Guidance for current project decisions
- Keep NCR and GRG as implemented first-class vendors now.
- Keep WN and Hyosung as first-class planned vendors in the architecture now.
- Do not overfit the normalized event model to GRG text phrases only.
- Do not overfit low-level decoding to NCR only.
- Treat APTRA/XFS concepts as architecture guidance and vendor manuals as decoding guidance.
