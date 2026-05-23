# Vendor Log Memory

## NCR

Important areas:
- EJDATA.LOG
- EJRCPY.LOG
- EJDATA.LOb
- TRACE logs
- NDC messages
- XFS events
- M-codes / R-codes
- dispense markers
- STAN/RRN/account/amount extraction

## GRG

Track daily journal files, trace files, and vendor-specific event naming.

## Wincor / Diebold-Nixdorf

Track WOSA paths, daily EJ files, trace files, and device/service event mapping.

## Analysis rule

Do not infer transaction success only from one marker. Use full event sequence and known vendor patterns.
