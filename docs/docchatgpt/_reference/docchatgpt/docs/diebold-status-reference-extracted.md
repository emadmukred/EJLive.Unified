# Diebold Agilis 3 91x Status Reference Extracted

## Purpose
This file captures the practical status-decoding rules extracted from the supplied Diebold Agilis 3 91x status reference so they can be used directly in EJLive design and parsing work.

## 1. Core Diebold status model
Diebold Agilis 3 91x uses **MDS statuses** and related **912 statuses**.

The fundamental printed status format for fault statuses is:

```text
001CR01:3E:48:40
```

Meaning:
- `001` = solicited status sequence index, or `000` for unsolicited
- `CR01` = device type and number
- `:3E:48:40` = status handler + status detail bytes

This is structurally different from NCR's `*tttt*m*d*aa...,M-bb,R-c` format.

## 2. Important architectural implication
The platform must not treat Diebold as a small variation of NCR parsing.

Diebold needs its own adapter model based on:
- **device code + handler byte + detail bytes**
- optional **912 network status mapping**
- explicit **network action** guidance
- explicit **on-site action** guidance
- optional **next-state** behavior for some depository scenarios

## 3. Device map extracted from the reference
The document confirms these device type/number families:
- `AH01` = After Hour Depository
- `AL01` = Alarms
- `CI01` = Currency Acceptor
- `CN01` = Coin Dispenser
- `CR01` = Card Reader
- `D901` or `DI01` = Bill Dispenser dispense function
- `DP01` = Depository / IDM / envelope depository emulation
- `DR01` = Presenter (present, dump, retract, restore)
- `EP01` = Encrypting PIN Pad
- `LC01` = LCD Failure Detection Device
- `PR01` = Receipt Printer
- `PR02` = Journal Printer
- `PR03` = Electronic Journal
- `SD01` = Envelope Dispenser
- `SP01` = Statement Printer
- `SY01` = System Stability
- `****` = Power-up / SYS

## 4. Status handler meanings
The first status byte after the device code is the **status handler**.
Important extracted meanings include:
- `:21` operation completed
- `:23` timeout
- `:24` cancel
- `:2D` attention required / supplies low style conditions
- `:30` reject
- `:31` idle
- `:38` communications issue
- `:39` fault before/without successful start
- `:3A` fault after starting operation
- `:3B` fault, operation may be retried / partial state
- `:3C` warning / degraded performance
- `:3D` supplies out / institution action required
- `:3E` institution repair / hardware-severity fault
- `:3F` warning / device-dependent completion nuance

This means the parser should treat handler bytes as a **first-class severity/impact dimension**.

## 5. Device-specific findings

### 5.1 `CR01` Card Reader
Important statuses extracted:
- `CR01:23:00:00` consumer timeout -> retain card
- `CR01:38:00:00` reader unavailable/inoperative
- `CR01:39:00:00` hardware error
- `CR01:3C:00:00` read incomplete due to timeout/cancel or eject/retain unsupported-data rejection
- `CR01:3D:58:60` anti-fishing / possible fraud / jam scenario
- `CR01:3E:44:40` card retain bin full
- `CR01:3E:48:40` card jammed in reader
- `CR01:3E:58:60` jam / possible fraud attempt (converted form)
- `CR01:3E:60:48` repeated partial insertion
- `CR01:3F:40:41` unsolicited card retain
- `CR01:3F:40:42/53/55` track write failures
- `CR01:3F:42:40` read conditions cannot be satisfied
- `CR01:3F:48:60` card tease
- `CR01:3F:80:01/02` foreign device detection sensor OFF/ON

Implication:
- Diebold card-reader logic must explicitly model:
  - timeout retain
  - anti-fishing
  - foreign device detection / skimmer detection
  - partial insertion and tease behavior
  - track-specific write problems

### 5.2 `D901/DI01` Bill Dispenser
Important statuses extracted:
- `D901:22:00:00` network requested too many or zero bills
- `D901:2D:0m:0n` low cash by denomination bitmap
- `D901:38:00:00` internal communications problem
- `D901:38:39:31` garbled status
- `D901:39:00:00` dispenser shutdown due to prior fault
- `D901:39:36:33` memory error
- `D901:39:36:40` denomination map authentication failed
- `D901:3A:30:31..34` unsolicited bill from feed modules 1-4
- `D901:3A:33:37` diverter could not reject during dispense
- `D901:3A:36:30` counting sensor blocked
- `D901:3A:38:31` dispense timeout
- `D901:3B:33:33` jam between double detect and exit
- `D901:3B:37:32` purge failure
- `D901:3D:31:37..39`, `3D:32:30` feed failure by feed module
- `D901:3D:33:35` command reject from dispenser / denomination not present
- `D901:3D:37:26/38/39` divert cassette nearly full / full / missing
- `D901:3E:01:01` door open before dispense
- `D901:3E:02:00` chest door open and security option blocks dispense
- `D901:3E:03:03` No Retract condition blocks next dispense
- `D901:3F:00:01/03` drawer sensor blocked/not blocked after dispense
- `D901:3F:31:37..39`, `3F:32:30` feed failures after retries
- `D901:3F:33:34` jam between double detect and reject
- `D901:3F:33:36` unidentified cassette code
- `D901:3F:34:30` bill size does not match cassette coding
- `D901:3F:35:38` too many bills dispensed
- `D901:3F:36:35` cassette shuffle / cassette changed / reseated
- `D901:61:nn:00` cassette tamper

Implication:
- Diebold dispenser parsing must support:
  - denomination-aware faults
  - module-aware faults
  - retry outcome differences (`3A` vs `3F`)
  - cassette tamper bitmap decoding
  - operator/on-site action mapping

### 5.3 `DR01` Presenter
Important statuses extracted:
- `DR01:23:00:01` consumer had access to cash but did not take all of it
- `DR01:23:00:30` presented money forgotten (No Retract enabled)
- `DR01:38:00:00` internal communications error
- many `3A` hardware faults during present/restore/retract/reject/purge
- `DR01:3F:00:40` forgotten money removed
- `DR01:3F:02:01` delivery area sensors blocked
- `DR01:3F:30:41` no bills to present
- `DR01:3F:46:00` no bills retracted or dumped
- `DR01:3F:46:nn` bills retracted or dumped, with quantity semantics

Implication:
- Presenter must be treated as a separate device family, not lumped into dispenser only.
- Withdraw lifecycle should distinguish:
  - dispense
  - present
  - retract/dump
  - customer no-take

### 5.4 `CI01` Currency Acceptor / cash-in path
Important statuses extracted include:
- deposit timeout/cancel
- communications problems
- module inoperative
- shutter open/close faults
- deposit cassette nearly full/full
- counterfeit bill bin full
- Cashguard activated
- cassettes missing/full/manipulated state
- network/state data errors
- module undocked
- cash retracted during close/deposit operations
- network sequence errors / No Retract situations
- cassette shuffle
- cash in input/reject area after clear
- cassette tamper bitmap `CI01:61:xx:yy`
- BCR expanded status support

Implication:
- Cash-in logic should be deeper than simple deposit success/fail.
- The platform should support:
  - cassette tamper
  - counterfeit bin state
  - suspect note / suspicious magnetics
  - scanner/gate/bin problems
  - special IDM and BCR behavior

### 5.5 `DP01` Depository / IDM / envelope depository emulation
The reference clearly distinguishes:
- standard envelope depository
- IDM
- envelope depository emulation

Important statuses include:
- timeout/cancel
- transport jams
- ink cartridge missing
- deposit cassette full/absent
- gate did not open
- suspicious magnetics
- abnormal document retain
- scanner initialization/top scanner errors
- ImageWay manager problems
- foreign items in entry slot
- tamper/fishing status

Implication:
- Diebold deposit handling requires subtype awareness, not one generic depository parser.

### 5.6 `PR01`, `PR02`, `PR03`, `SP01`
Receipt, journal, electronic journal, and statement printer status sets are all distinct and include:
- supply low/out
- communications and timeout classes
- hardware faults
- top-of-form mark detection faults
- jam/cut/deliver/dump/retain faults
- invalid print data / XFS forms/media mismatch
- electronic journal file missing / file locked / disk full (`PR03`)

Implication:
- The project should split printer diagnostics by printer role, not simply by generic `PrinterEvent`.
- `PR03` is especially important for **electronic journal health** and should feed journal monitoring logic.

### 5.7 `EP01`, `LC01`, `SY01`, `****`
Important extracted meanings:
- `EP01` covers EPP internal comms, key storage, invalid key usage, stuck key, cert failures
- `LC01` covers LCD/video failure detection
- `SY01` covers system stability such as low disk space
- `****:3F:00:00` means power returned after power failure

Implication:
- The platform needs system-health categories beyond cash/card only.

## 6. 912 status cross-reference
The manual provides 912-to-MDS cross-reference tables.
This is strategically important because it means the project should support:
- decoding raw MDS status when it appears in journals
- mapping MDS ↔ 912 when the source is network-side or host-side status data

This should become a dedicated lookup layer later.

## 7. What this adds to the project beyond NCR-only references
Compared with the NCR status reference, this Diebold manual adds a different diagnostic philosophy:
- device family names are different
- handler-byte semantics are central
- network action and on-site action are explicitly described per status
- next-state behavior matters for deposit devices
- 912 cross-reference is first-class

## 8. Immediate development implications for EJLive
The project should eventually add:
- `DieboldMdsAdapter`
- `Diebold912CrossReferenceService`
- device-subtype-aware parsers for `DP01`, `DR01`, `PR01/02/03`, `SP01`
- a stronger normalized action model including:
  - network action suggestion
  - on-site action suggestion
  - retry/removal-from-service recommendation
  - next-state recommendation

## 9. Priority Diebold parser targets
Highest-value first:
1. `CR01` card reader
2. `D901/DI01` bill dispenser
3. `DR01` presenter
4. `CI01` currency acceptor / cash-in
5. `PR01`, `PR02`, `PR03`
6. `DP01` standard + IDM + emulation
7. `EP01`, `SY01`, `****`

## 10. Final conclusion
Diebold support in the project should be modeled as a dedicated MDS/912 ecosystem, not as a small variant of NCR or GRG.
It has enough structure, semantics, corrective-action guidance, and subtype variation to justify its own parser family and mapping services.
