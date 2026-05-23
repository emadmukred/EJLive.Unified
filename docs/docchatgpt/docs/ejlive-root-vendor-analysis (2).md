# EJLive Root Vendor Analysis

## Scope
This analysis is derived from the attached root packages for:
- NCR
- GRG
- WN
- Nautilus
- Diebold
- DelaRue

## Immediate findings

### 1. `PROAGENT/DATA/filter.ini` is the common denominator
All six vendor root packages include a `PROAGENT/DATA/filter.ini` file.

This strongly suggests that the current system architecture is expected to support a **shared ProAgent / ProView / NDC-DDC filtering layer** across vendors, with vendor-specific variations in the actual filter content and mappings.

Implication for the project:
- The system should treat `filter.ini` as a first-class configuration artifact.
- A future parser/configuration layer should be able to ingest vendor root filter mappings and convert them into normalized monitoring/event rules.
- `filter.ini` is not just static baggage; it likely drives event classification and mapping into operational event numbers.

### 2. WN-family roots dominate several packages
The following packages clearly show Wincor/Wincor Nixdorf style heritage in filter metadata:
- RootDiebold
- RootGrg
- RootNautilus
- RootNcr
- RootWn

These share nearly identical header structure referencing:
- NDC-DDC applications
- ProTopas / PrvPro
- ProView event assignment
- corresponding SQL / WOSA scripts

Implication:
- The project should not model vendor support only as ATM hardware type.
- It should also model **platform lineage**:
  - NCR / APTRA / XFS
  - ProTopas / ProView / WOSA-style filtered event architecture
  - GRG business-journal layer on top of vendor middleware
- This means one ATM vendor may still inherit tooling/config from another middleware lineage.

### 3. NCR root has XFS data files that expose real operational configuration surfaces
`RootNcr.zip` includes:
- `XFS/data/cdmdata/exp.dat`
- `XFS/data/ttu/dckeymap.dat`
- `XFS/kbape.cnf`

Implication:
- The current project needs a configuration model not only for logs and events, but also for:
  - cash dispenser currency exponent configuration
  - operator panel / keyboard key mapping
  - encryptor / keypad / BAPE related configuration
- These should become configurable domain objects rather than opaque files.

### 4. GRG root includes media/form descriptors
`RootGrg.zip` includes:
- `XFS/Media/RPTR/ReceiptPtrMediaDC.wfm`
- `XFS/Media/RPTR/ReceiptPtrMediaDCL.wfm`
- `XFS/Media/SPTR/DocumentPtrMediaDC.wfm`
- `XFS/Media/SPTR/DocumentPtrMediaDCL.wfm`

Implication:
- The project should model printer/media form factors and document layout descriptors.
- Dashboard and diagnostics should eventually be aware of vendor media templates and printer families.
- Receipt/document printer behavior differs structurally and should not be represented only as generic printer status.

### 5. DelaRue root has a distinct filter header
`RootDelaRue.zip` still exposes `filter.ini`, but its header is clearly different:
- `Filter for NDC-DDC on DeLaRue ATMs`
- older Windows NT era variant
- direct mapping lines for hard disk, virtual memory, reboot attempts, ProView commands

Implication:
- DelaRue support should likely be modeled as a separate adapter profile, even if the file name is the same.
- The project needs vendor capability profiles that distinguish:
  - common filter syntax
  - vendor-specific event taxonomy
  - platform generation/version differences

## Architectural conclusions for the full system

### A. Add a vendor root capability layer
The project should maintain a structured model for each vendor root package, covering:
- filter engine presence
- XFS media descriptors
- keyboard/keymap config presence
- dispenser config data presence
- platform lineage
- likely middleware family

### B. Separate these layers clearly
1. **Root configuration layer**
   - static vendor files such as `filter.ini`, `.wfm`, `.cnf`, `.dat`
2. **Live journal/log layer**
   - EJ logs, journal files, OCR lines, transaction logs
3. **XFS/device diagnostic layer**
   - status/error decoding, `M_STATUS`, `M_DATA`, module/device status
4. **Dashboard/application layer**
   - operator insights, sync tracking, alerts, ATM cards

### C. Treat `filter.ini` as an event-mapping source
The project should eventually parse filter lines like:
- `<ProviderId>:<EventId>=<Regex>`

and normalize them into:
- provider/module id
- event id
- regex rule
- semantic category
- mapped alert/telemetry meaning

### D. Treat media/template files as printer/document capabilities
Files like `.wfm` should be mapped into:
- printer family
- document class
- row/column size
- logical media template

### E. Treat NCR data/config files as capability descriptors
Files like:
- `exp.dat`
- `dckeymap.dat`
- `kbape.cnf`

should be modeled as:
- dispenser currency exponent configuration
- keyboard map profile
- secure keypad/operator panel profile

## What this means for the current project baseline
The current project should expand from:
- log parsing only

to:
- **vendor root awareness**
- **middleware capability awareness**
- **device/media/config profile awareness**
- **dashboard awareness of vendor-specific static configuration**

## Suggested next implementation steps
1. Add vendor root profile models in `EJLive.Core`
2. Add a root capability service that can infer vendor/platform capabilities from extracted root files
3. Add future parsing support for `filter.ini` rule catalogs
4. Add support for `.wfm`, `.cnf`, and `.dat` configuration descriptors
5. Reflect root-derived capabilities into dashboard cards and ATM detail views

## Current project risks revealed by the root analysis
- The project currently understands logs and XFS events better than it understands vendor static root configuration.
- Without a root capability layer, dashboards may miss important static differences between vendors.
- Treating all vendors as only journal variants would under-model printer/media/keymap/dispenser config differences.
