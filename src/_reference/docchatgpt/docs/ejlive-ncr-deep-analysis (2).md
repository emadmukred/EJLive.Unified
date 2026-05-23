# EJLive NCR Deep Analysis

## Scope
This document unifies the NCR findings from:
- NCR business and journal logs (`EJDATA`, `EJRCPY`)
- NCR debug traces (`CARDREAD`, `MESSAGEIN`, `MESSAGEOUT`)
- NCR merged traces (`OOXFS`, `DEBUG`, `MergedTrace`)
- NCR root files (`filter.ini`, `exp.dat`, `dckeymap.dat`, `kbape.cnf`)
- NCR configuration bundle (`*.accfg`, `*.reg`, `SupervisorKeyboard.xml`, translation and validation files)
- NCR status semantics references captured in `docs/ncr-status-message-reference-extracted.md`

## 1. NCR is a multi-layered runtime, not just a journal source
The project should continue to treat NCR support as the combination of:
1. business journal flow
2. status/device code messages
3. card-reader diagnostic traces
4. host protocol traffic
5. OOXFS runtime traces
6. DEBUG / middleware traces
7. static root and composed configuration

## 2. What the current code now supports
The current project now contains real implementation foundations for NCR, not only notes:
- `NcrXfsAdapter`
  - parses NCR status/device messages
  - parses command reject patterns
  - parses journal flow hints such as transaction start/end, PIN entered, card taken, notes presented/taken, diagnostic dispense reports, ATR/GENAC markers
- `NcrConfigCapabilityParser`
  - extracts high-level capability hints from `.accfg`, `.reg`, `SupervisorKeyboard.xml`, translation files, and terminal configuration payloads
- `VendorRootCapabilityService`
  - exposes vendor root profile behavior for NCR and other vendors

## 3. Practical NCR parsing/monitoring domains

### 3.1 Business journal flow
Observed and supported patterns include:
- `TRANSACTION START`
- `TRANSACTION END`
- `PIN ENTERED`
- `CARD TAKEN`
- `NOTES PRESENTED`
- `NOTES TAKEN`
- `ATR RECEIVED`
- `GENAC 1 / GENAC 2`
- `DIAGNOSTIC DISPENSE REPORT`

### 3.2 Low-level device/status decoding
Key supported NCR format:
- `*tttt*m*d*aa...,M-bb,R-c`

This should be interpreted through:
- device family
- handler / status category
- module status
- supply state
- derived operational/customer impact

### 3.3 Command reject decoding
Key supported pattern:
- `*tttt*2*CNN`

Meaning now modeled as a separate event type:
- `CommandReject`
- with category/detail decoding and recommended action

### 3.4 Static configuration awareness
NCR config artifacts reveal:
- receipt service composition
- cash-in/cash-retract behavior toggles
- reset behavior
- voice guidance and enhanced audio
- supervisor keyboard layout
- terminal metadata
- receipt and exception form names
- registry-driven operational flags

## 4. Why NCR still needs more work
Even after the recent implementation changes, NCR support is still not complete because:
- not all card/device subtypes are fully decoded
- not all sensor vector permutations are interpreted yet
- printer/journal/statement families are only partially expanded in code
- no separate UI views exist yet for NCR-specific config capabilities or command-reject intelligence
- no dedicated correlation view yet ties NCR business flow and OOXFS/runtime layers together in the server UI

## 5. Immediate next NCR improvements still recommended
1. add a dedicated correlation UI path for merged NCR traces
2. deepen sensor vector interpretation for device `P`
3. add richer PR01/PR02/PR03 event specialization
4. add explicit command-response grouping for NCR command rejects vs business flow
5. surface NCR config capability data in ATM Details and dashboards

## 6. Final conclusion
NCR support in EJLive is no longer only an idea. It now has:
- code-level parser foundations
- configuration capability parsing foundations
- runtime/middleware evidence from logs
- root/config awareness

The next phase is to strengthen the UI exposure and correlation logic so operators can use these layers directly in monitoring and diagnosis.
