# EJLive Client UI System (ATM-First)

## 1) Product Boundaries

Client app responsibilities:
- Stable connectivity with server.
- Lightweight file sending pipeline from ATM.
- Windows/remote-access readiness for ATM support.
- Basic local visibility (status + logs + live screenshot preview).

Server/Dashboard responsibilities:
- Heavy analytics and portal-level business analysis.
- Global reporting and cross-ATM comparisons.
- Deep journal analysis workflows.

This keeps ATM client runtime light and operational.

## 2) Tab Information Architecture

### Final tab model (client)
1. `الاتصال`
2. `المزامنة` (sending queue only)
3. `الجورنال` (local operational inspection only)
4. `الخدمات والتحكم` (merged: Windows services + remote view)
5. `الإعدادات`

### Removed overlap
- Old split between `التحكم البعيد` and `الخدمات` is merged.
- `طلب الشاشة` and `Screenshot` are one workflow now:
  - Capture current ATM screen
  - Show inside client
  - Save to screenshots folder
  - Send frame to server when connected

## 3) Visual Tokens

| Token | Value | Usage |
| --- | --- | --- |
| Window | `#F6F8FA` | Main background |
| Surface | `#FFFFFF` | Work surfaces |
| SurfaceAlt | `#F1F5F9` | Toolbars / secondary bands |
| Header | `#E8EEF5` | Headers and grid bands |
| Border | `#CBD5E1` | Outlines |
| Text | `#1F2937` | Primary text |
| Muted | `#64748B` | Secondary text |
| Primary | `#0066CC` | Primary actions |
| Success | `#198754` | Running/connected |
| Warning | `#B46400` | Caution/manual attention |
| Danger | `#B42318` | Stopped/errors |

## 4) Typography & Spacing

- Font family: `Segoe UI`
- Base body: `9.5pt`
- Dense controls: `8.5-9pt`
- Min button height: `34px`
- Grid row height: `28px+`
- Group padding: `10-14px`
- Page edge padding: `12-20px`

## 5) State Language (Unified)

Use consistent service state text and color:
- `● Running/Connected/Enabled` -> `Success`
- `○ Stopped/Disabled` -> `Muted`

Start/Stop controls:
- Start action visible and active when service is off.
- Stop action visible and active when service is on.

## 6) Layout Rules for All Screens

- Use `TableLayoutPanel` for fixed forms and settings matrices.
- Use `FlowLayoutPanel` only where wrapping is expected.
- Avoid fixed-position UI that can overlap on resize.
- Avoid dark blocks except true media canvas (remote screen view).
- Keep logs and queues in bordered, full-width containers.

## 7) Services + Remote Operations Pattern

Merged operations tab must include:
- Left: remote screen canvas + capture toolbar + remote log.
- Right: Windows service controls + incoming command queue.

Required actions:
- Capture screen now.
- Open screenshots folder.
- Start/stop core operational services.

## 8) Rollout Checklist for Other WinForms

For each form:
1. Apply `LightUiTheme.Apply(this)`.
2. Replace free-position controls with table/flow layouts.
3. Normalize status text to `●/○` pattern.
4. Replace duplicate actions with one canonical action.
5. Validate at minimum and normal window sizes (no overlap).

## 9) Performance Guardrails

- No heavy analysis jobs in ATM client UI thread.
- Keep client operations I/O-focused and non-blocking.
- Move portal/analytics behavior to server/dashboard modules.

