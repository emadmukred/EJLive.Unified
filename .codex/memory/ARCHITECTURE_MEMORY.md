# Architecture Memory

## Architectural rules

- Core contains contracts, models, engines, protocol logic, and reusable services.
- Shared contains cross-project helpers such as security, logging, common models, and shared state.
- Client/Agent should stay lightweight and operational.
- Server should centralize intake, archive, command queue, analytics, and operational control.
- Monitoring/Dashboard should display NOC status, alerts, reports, and fleet views.
- Installer should wire deployment, prerequisites, service registration, configuration, and validation without hiding behavior.

## Compile surface rule

A file on disk is not automatically active runtime code.

Classify as:
- active compiled source
- preserved reference
- legacy candidate
- content/resource/config
- broken and blocked
- safe to promote
- unsafe direct compile

## Integration pattern

If legacy code is useful but unsafe:
- preserve it
- extract the intent
- implement a new safe service/facade/adapter
- document the mapping
